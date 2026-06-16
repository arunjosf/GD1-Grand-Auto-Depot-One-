using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Chat.Queries
{
    public class GetConversationsQuery : IRequest<List<ConversationDto>>
    {
        public long UserId { get; set; }
    }

    public class ConversationDto
    {
        public string Category { get; set; } = string.Empty; // "garage" or "serviceCenter"
        public long ReferenceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string LatestMessage { get; set; } = string.Empty;
        public System.DateTime? LatestMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool IsChatActive { get; set; }
        public long OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
    }

    public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRepo;
        private readonly IGenericRepository<ChatMessage> _chatRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.PickupRequest> _pickupRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;

        public GetConversationsQueryHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRepo,
            IGenericRepository<ChatMessage> chatRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<GD1.Domain.Entities.PickupRequest> pickupRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _chatRepo = chatRepo;
            _propertyRepo = propertyRepo;
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
            _lotManagerRepo = lotManagerRepo;
            _pickupRepo = pickupRepo;
            _centerRepo = centerRepo;
        }

        public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = new List<ConversationDto>();

            var allBookings = await _bookingRepo.GetAllAsync();
            var allServices = await _serviceRepo.GetAllAsync();
            var allProperties = await _propertyRepo.GetAllAsync();
            var allChats = await _chatRepo.GetAllAsync();
            var allVehicles = await _vehicleRepo.GetAllAsync();
            var allUsers = await _userRepo.GetAllAsync();
            var allLotManagers = await _lotManagerRepo.GetAllAsync();
            var allPickups = await _pickupRepo.GetAllAsync();
            var allCenters = await _centerRepo.GetAllAsync();

            var currentUser = allUsers.FirstOrDefault(u => u.Id == request.UserId);
            bool isManager = currentUser != null && currentUser.Role == GD1.Domain.Entities.Enums.UserRole.Manager;

            var garageAllowedStatuses = new[] 
            {
                GD1.Domain.Entities.Enums.BookingStatus.Confirmed,
                GD1.Domain.Entities.Enums.BookingStatus.AwaitingPickupAssignment,
                GD1.Domain.Entities.Enums.BookingStatus.PickupAssigned,
                GD1.Domain.Entities.Enums.BookingStatus.ManagerArrived,
                GD1.Domain.Entities.Enums.BookingStatus.PickupVerified,
                GD1.Domain.Entities.Enums.BookingStatus.InTransit,
                GD1.Domain.Entities.Enums.BookingStatus.InLot
            };

            // 1. Garage Conversations (Involving Bookings)
            List<Booking> myGarageBookings = new List<Booking>();
            
            if (isManager)
            {
                // Find properties managed by this manager
                var managedProperties = allLotManagers
                    .Where(lm => lm.ManagerId == request.UserId && lm.IsActive)
                    .Select(lm => lm.PropertyId)
                    .ToList();

                var managerRecordIds = allLotManagers
                    .Where(lm => lm.ManagerId == request.UserId && lm.IsActive)
                    .Select(lm => lm.Id)
                    .ToList();

                myGarageBookings = allBookings.Where(b => 
                    managedProperties.Contains(b.PropertyId)
                ).ToList();

                // Strictly filter bookings for manager:
                // - Either assigned to the vehicle pickup OR vehicle currently storing in their garage (InLot)
                myGarageBookings = myGarageBookings.Where(b => {
                    bool isAssignedPickup = allPickups.Any(pr => 
                        pr.BookingId == b.Id && 
                        pr.ManagerId.HasValue && 
                        managerRecordIds.Contains(pr.ManagerId.Value) &&
                        pr.Status != GD1.Domain.Entities.Enums.PickupStatus.Stored &&
                        pr.Status != GD1.Domain.Entities.Enums.PickupStatus.Declined
                    );
                    
                    bool isCurrentlyStored = b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot;

                    return isAssignedPickup || isCurrentlyStored;
                }).ToList();
            }
            else
            {
                myGarageBookings = allBookings.Where(b => 
                    b.OwnerId == request.UserId || 
                    allProperties.Any(p => p.Id == b.PropertyId && p.LotOwnerId == request.UserId)).ToList();
            }

            foreach(var booking in myGarageBookings)
            {
                bool isActive = System.Array.IndexOf(garageAllowedStatuses, booking.Status) >= 0;
                if (!isActive) continue; // Only show active bookings

                var bookingChats = allChats.Where(c => c.BookingId == booking.Id).ToList();
                var lastMsg = bookingChats.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                
                var isLotOwner = allProperties.Any(p => p.Id == booking.PropertyId && p.LotOwnerId == request.UserId);
                var vehicle = allVehicles.FirstOrDefault(v => v.Id == booking.VehicleId);
                
                User otherUser = null;
                if (isManager)
                {
                    otherUser = allUsers.FirstOrDefault(u => u.Id == booking.OwnerId);
                }
                else if (isLotOwner)
                {
                    otherUser = allUsers.FirstOrDefault(u => u.Id == booking.OwnerId);
                }
                else
                {
                    otherUser = allProperties.Where(p => p.Id == booking.PropertyId).Select(p => allUsers.FirstOrDefault(u => u.Id == p.LotOwnerId)).FirstOrDefault();
                }

                string title = (isLotOwner || isManager) ? $"{otherUser?.FullName ?? "Customer"} - {vehicle?.Brand} {vehicle?.Model} ({vehicle?.RegistrationNo})" : $"{otherUser?.FullName ?? "Lot Owner"}";
                int unreadCount = bookingChats.Count(c => !c.IsRead && c.SenderId != request.UserId);

                conversations.Add(new ConversationDto
                {
                    Category = "garage",
                    ReferenceId = booking.Id,
                    Title = title,
                    LatestMessage = lastMsg?.MessageContent ?? "No messages yet",
                    LatestMessageAt = lastMsg?.CreatedAt,
                    UnreadCount = unreadCount,
                    IsChatActive = isActive,
                    OtherUserId = otherUser?.Id ?? 0,
                    OtherUserName = otherUser?.FullName ?? ((isLotOwner || isManager) ? "Customer" : "Lot Owner")
                });
            }

            // 2. Service Center Conversations
            List<GD1.Domain.Entities.ServiceRequest> myServices = new List<GD1.Domain.Entities.ServiceRequest>();
            if (isManager)
            {
                var managedProperties = allLotManagers
                    .Where(lm => lm.ManagerId == request.UserId && lm.IsActive)
                    .Select(lm => lm.PropertyId)
                    .ToList();

                myServices = allServices.Where(s => 
                    allBookings.Any(b => b.Id == s.BookingId && managedProperties.Contains(b.PropertyId))
                ).ToList();
            }
            else
            {
                myServices = allServices.Where(s => 
                    s.RequestedBy == request.UserId || 
                    allProperties.Any(p => allBookings.Any(b => b.Id == s.BookingId && b.PropertyId == p.Id && p.LotOwnerId == request.UserId))
                ).ToList();
            }

            foreach(var service in myServices)
            {
                var booking = allBookings.FirstOrDefault(b => b.Id == service.BookingId);
                bool isActive = true;
                if (!isManager)
                {
                    if (allProperties.Any(p => allBookings.Any(b => b.Id == service.BookingId && b.PropertyId == p.Id && p.LotOwnerId == request.UserId)))
                    {
                        if (service.IsCompleted == true || service.Status == "Completed" || service.Status == "Cancelled")
                            isActive = false;
                    }
                }
                else
                {
                    if (service.IsCompleted == true || service.Status == "Completed" || service.Status == "Cancelled")
                        isActive = false;
                }
                
                if (!isActive) continue; // Only show active

                var serviceChats = allChats.Where(c => c.ServiceRequestId == service.Id).ToList();
                var lastMsg = serviceChats.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                int unreadCount = serviceChats.Count(c => !c.IsRead && c.SenderId != request.UserId);

                var isLotOwner = allProperties.Any(p => p.Id == booking?.PropertyId && p.LotOwnerId == request.UserId);
                var vehicle = allVehicles.FirstOrDefault(v => v.Id == booking?.VehicleId);
                
                var sc = allCenters.FirstOrDefault(c => c.Id == service.ServiceCenterId);
                string title = (isLotOwner || isManager) ? $"Service for {vehicle?.Brand} {vehicle?.RegistrationNo}" : sc?.Name ?? "Service Center";

                conversations.Add(new ConversationDto
                {
                    Category = "serviceCenter",
                    ReferenceId = service.Id,
                    Title = title,
                    LatestMessage = lastMsg?.MessageContent ?? "No messages yet",
                    LatestMessageAt = lastMsg?.CreatedAt,
                    UnreadCount = unreadCount,
                    IsChatActive = isActive,
                    OtherUserId = sc?.AdminId ?? 0,
                    OtherUserName = sc?.Name ?? "Service Center"
                });
            }

            // 3. Manager/Direct Conversations
            var myDirectChats = allChats.Where(c => 
                c.BookingId == null && 
                c.ServiceRequestId == null && 
                c.ReceiverId.HasValue && 
                (c.SenderId == request.UserId || c.ReceiverId.Value == request.UserId)
            ).ToList();

            var directGroups = myDirectChats
                .GroupBy(c => c.SenderId == request.UserId ? c.ReceiverId!.Value : c.SenderId)
                .ToList();

            foreach (var group in directGroups)
            {
                var otherUserId = group.Key;
                var otherUser = allUsers.FirstOrDefault(u => u.Id == otherUserId);
                if (otherUser == null) continue;

                if (isManager)
                {
                    var managedProperties = allLotManagers
                        .Where(lm => lm.ManagerId == request.UserId && lm.IsActive)
                        .Select(lm => lm.PropertyId)
                        .ToList();

                    bool isBoss = allProperties.Any(p => managedProperties.Contains(p.Id) && p.LotOwnerId == otherUserId);
                    if (!isBoss) continue; // Only direct chat with manager's Lot Owner (Boss)
                }

                long managerUserId = isManager ? request.UserId : otherUser.Id;

                var lastMsg = group.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                int unreadCount = group.Count(c => !c.IsRead && c.SenderId != request.UserId);

                conversations.Add(new ConversationDto
                {
                    Category = "manager",
                    ReferenceId = managerUserId,
                    Title = otherUser.FullName,
                    LatestMessage = lastMsg?.MessageContent ?? "No messages yet",
                    LatestMessageAt = lastMsg?.CreatedAt,
                    UnreadCount = unreadCount,
                    IsChatActive = true,
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.FullName
                });
            }

            return conversations.OrderByDescending(c => c.LatestMessageAt).ToList();
        }
    }
}
