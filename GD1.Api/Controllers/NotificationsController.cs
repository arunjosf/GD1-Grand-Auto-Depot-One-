using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly IGenericRepository<FranchiseApplication> _appRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Agreement> _agreementRepo;

        public NotificationsController(
            IGenericRepository<Notification> notificationRepo,
            IGenericRepository<FranchiseApplication> appRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Agreement> agreementRepo)
        {
            _notificationRepo = notificationRepo;
            _appRepo = appRepo;
            _bookingRepo = bookingRepo;
            _agreementRepo = agreementRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetUserId();
            var notifications = await _notificationRepo.FindAsync(n => n.UserId == userId);
            
            var validNotifications = new List<object>();

            foreach (var n in notifications.OrderByDescending(x => x.CreatedAt))
            {
                try 
                {
                    // Clean up orphaned franchise applications
                    if ((n.ActionType == "ReviewFranchise" || n.ActionType == "TrackApplication") && n.ReferenceId.HasValue)
                    {
                        var app = await _appRepo.GetByIdAsync(n.ReferenceId.Value);
                        if (app == null || app.IsDeleted)
                        {
                            await _notificationRepo.DeleteAsync(n);
                            continue; // Skip returning this notification
                        }
                    }

                    // Clean up orphaned booking confirmations
                    if (n.ActionType == "ConfirmBooking")
                    {
                        bool shouldDelete = false;

                        if (n.ReferenceId.HasValue)
                        {
                            var agreement = await _agreementRepo.GetByIdAsync(n.ReferenceId.Value);
                            if (agreement == null || agreement.Status != GD1.Domain.Entities.Enums.AgreementStatus.Pending)
                            {
                                shouldDelete = true;
                            }
                        }
                        else if (!string.IsNullOrEmpty(n.ActionUrl) && n.ActionUrl.StartsWith("/agreement/"))
                        {
                            var bookingIdStr = n.ActionUrl.Split('/').LastOrDefault();
                            if (long.TryParse(bookingIdStr, out long bId))
                            {
                                var booking = await _bookingRepo.GetByIdAsync(bId);
                                if (booking == null || booking.Status != GD1.Domain.Entities.Enums.BookingStatus.VerifiedPendingPayment)
                                {
                                    shouldDelete = true;
                                }
                            }
                        }

                        if (shouldDelete)
                        {
                            await _notificationRepo.DeleteAsync(n);
                            continue; // Skip returning this notification
                        }
                    }
                    // Clean up responded booking verifications for Lot Owners
                    if (n.ActionType == "ViewBookings" && n.ReferenceId.HasValue)
                    {
                        var booking = await _bookingRepo.GetByIdAsync(n.ReferenceId.Value);
                        if (booking == null || booking.Status != GD1.Domain.Entities.Enums.BookingStatus.PendingVerification)
                        {
                            await _notificationRepo.DeleteAsync(n);
                            continue; // Skip returning this notification
                        }
                    }

                    // Clean up any other notifications if needed...
                    
                    validNotifications.Add(new 
                    {
                        n.Id,
                        n.Title,
                        n.Body,
                        n.ActionType,
                        n.ReferenceId,
                        n.ActionUrl,
                        n.CreatedAt,
                        n.IsRead
                    });

                    if (validNotifications.Count >= 50) break;
                }
                catch (Exception ex)
                {
                    // Log error and continue so we don't crash the whole endpoint
                    // Optionally add it to validNotifications so it isn't lost if cleanup fails
                    validNotifications.Add(new 
                    {
                        n.Id, n.Title, n.Body, n.ActionType, n.ReferenceId, n.ActionUrl, n.CreatedAt, n.IsRead
                    });
                }
            }

            return Ok(BaseResponse<object>.Ok(validNotifications));
        }

        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var userId = GetUserId();
            var notification = await _notificationRepo.GetByIdAsync(id);

            if (notification == null || notification.UserId != userId)
                return NotFound(BaseResponse<string>.Fail("Notification not found."));

            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);

            return Ok(BaseResponse<string>.Ok("Marked as read."));
        }

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}
