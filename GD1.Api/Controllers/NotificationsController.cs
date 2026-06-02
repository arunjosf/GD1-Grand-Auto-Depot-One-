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

        public NotificationsController(
            IGenericRepository<Notification> notificationRepo,
            IGenericRepository<FranchiseApplication> appRepo)
        {
            _notificationRepo = notificationRepo;
            _appRepo = appRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetUserId();
            var notifications = await _notificationRepo.FindAsync(n => n.UserId == userId);
            
            var validNotifications = new List<object>();

            foreach (var n in notifications.OrderByDescending(x => x.CreatedAt))
            {
                // Clean up orphaned notifications if the application was removed from the DB or cancelled
                if ((n.ActionType == "ReviewFranchise" || n.ActionType == "TrackApplication") && n.ReferenceId.HasValue)
                {
                    var app = await _appRepo.GetByIdAsync(n.ReferenceId.Value);
                    if (app == null || app.IsDeleted)
                    {
                        await _notificationRepo.DeleteAsync(n);
                        continue; // Skip returning this notification
                    }
                }

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
