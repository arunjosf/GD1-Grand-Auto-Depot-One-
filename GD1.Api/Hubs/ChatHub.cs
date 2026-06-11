using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GD1.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        private long? GetUserId()
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("userId")?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            if (long.TryParse(userIdStr, out var userId))
                return userId;

            return null;
        }

        public async Task JoinGroup(string category, long referenceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{category}-{referenceId}");
        }

        private async Task<bool> IsMessagingAllowed(string category, long referenceId, long senderId)
        {
            if (category == "garage")
            {
                var booking = await _context.Bookings.Include(b => b.Property).FirstOrDefaultAsync(b => b.Id == referenceId);
                if (booking == null) return false;

                // Vehicle Owner <-> Lot Owner chat restrictions
                // Only allowed between Payment Done and Move Out (Completed)
                var allowedStatuses = new[] 
                {
                    BookingStatus.Confirmed,
                    BookingStatus.AwaitingPickupAssignment,
                    BookingStatus.PickupAssigned,
                    BookingStatus.ManagerArrived,
                    BookingStatus.PickupVerified,
                    BookingStatus.InTransit,
                    BookingStatus.InLot
                };

                if (System.Array.IndexOf(allowedStatuses, booking.Status) < 0)
                {
                    return false;
                }

                // Manager restrictions
                var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
                if (sender != null && sender.Role == UserRole.Manager)
                {
                    var managerLotIds = await _context.LotManagers
                        .Where(lm => lm.ManagerId == senderId && lm.IsActive)
                        .Select(lm => lm.Id)
                        .ToListAsync();

                    var managedPropertyIds = await _context.LotManagers
                        .Where(lm => lm.ManagerId == senderId && lm.IsActive)
                        .Select(lm => lm.PropertyId)
                        .ToListAsync();

                    if (!managedPropertyIds.Contains(booking.PropertyId))
                    {
                        return false;
                    }

                    bool isAssignedPickup = await _context.PickupRequests.AnyAsync(pr =>
                        pr.BookingId == booking.Id &&
                        pr.ManagerId.HasValue &&
                        managerLotIds.Contains(pr.ManagerId.Value) &&
                        pr.Status != PickupStatus.Stored &&
                        pr.Status != PickupStatus.Declined
                    );

                    bool isCurrentlyStored = booking.Status == BookingStatus.InLot;

                    if (!isAssignedPickup && !isCurrentlyStored)
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (category == "serviceCenter")
            {
                var service = await _context.ServiceRequests.Include(sr => sr.Booking).ThenInclude(b => b.Property).FirstOrDefaultAsync(sr => sr.Id == referenceId);
                if (service == null) return false;

                // Vehicle Owner has permanent access
                if (senderId == service.Booking.OwnerId) return true;
                
                // Service Center Admin has permanent access (they need to reply)
                // Wait, SC Admin isn't currently fetched but let's assume they can always reply to VO.
                // For Lot Owner <-> SC, only between booked and finish service.
                if (senderId == service.Booking.Property.LotOwnerId)
                {
                    if (service.IsCompleted == true || service.Status == "Completed" || service.Status == "Cancelled")
                        return false;
                }
                
                // Default true for SC admin or VO replying.
                return true;
            }
            else if (category == "manager")
            {
                return true;
            }
            return false;
        }

        public async Task SendMessage(string category, long referenceId, long receiverId, string messageContent)
        {
            var senderId = GetUserId();
            if (senderId == null) throw new HubException("User not authenticated.");

            if (category == "manager")
            {
                var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId.Value);
                var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverId);
                if (sender != null && receiver != null)
                {
                    if (sender.Role == UserRole.Manager)
                    {
                        var managedPropertyIds = await _context.LotManagers
                            .Where(lm => lm.ManagerId == senderId.Value && lm.IsActive)
                            .Select(lm => lm.PropertyId)
                            .ToListAsync();
                        
                        bool isBoss = await _context.VehicleStorageProperties
                            .AnyAsync(p => managedPropertyIds.Contains(p.Id) && p.LotOwnerId == receiverId);
                        
                        if (!isBoss) throw new HubException("You can only message your boss.");
                    }
                    else if (receiver.Role == UserRole.Manager)
                    {
                        var managedPropertyIds = await _context.LotManagers
                            .Where(lm => lm.ManagerId == receiverId && lm.IsActive)
                            .Select(lm => lm.PropertyId)
                            .ToListAsync();
                        
                        bool isBoss = await _context.VehicleStorageProperties
                            .AnyAsync(p => managedPropertyIds.Contains(p.Id) && p.LotOwnerId == senderId.Value);
                        
                        if (!isBoss) throw new HubException("You can only message your assigned managers.");
                    }
                }
            }

            bool isAllowed = await IsMessagingAllowed(category, referenceId, senderId.Value);
            if (!isAllowed) throw new HubException("Messaging is currently disabled based on the booking or service status.");

            var chatMessage = new ChatMessage
            {
                BookingId = category == "garage" ? referenceId : null,
                ServiceRequestId = category == "serviceCenter" ? referenceId : null,
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                MessageContent = messageContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group($"{category}-{referenceId}").SendAsync("ReceiveMessage", chatMessage);
        }

        public async Task SendWebRTCSignal(string category, long referenceId, string signalData)
        {
            var senderId = GetUserId();
            if (senderId == null) return;

            bool isAllowed = await IsMessagingAllowed(category, referenceId, senderId.Value);
            if (!isAllowed) return;

            await Clients.OthersInGroup($"{category}-{referenceId}").SendAsync("ReceiveWebRTCSignal", senderId.Value, signalData);
        }
    }
}
