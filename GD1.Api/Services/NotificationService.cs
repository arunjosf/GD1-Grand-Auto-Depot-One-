using GD1.Api.Hubs;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GD1.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IGenericRepository<Notification> _notificationRepo;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            IGenericRepository<Notification> notificationRepo)
        {
            _hubContext = hubContext;
            _notificationRepo = notificationRepo;
        }

        public async Task SendAsync(long userId, string title, string body, string? actionType = null, long? referenceId = null, string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                ActionType = actionType,
                ReferenceId = referenceId,
                ActionUrl = actionUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepo.AddAsync(notification);

            // Push to the user's specific SignalR group
            await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Body,
                notification.ActionType,
                notification.ReferenceId,
                notification.ActionUrl,
                notification.CreatedAt,
                notification.IsRead
            });
        }

        public async Task SendToManyAsync(IEnumerable<long> userIds, string title, string body, string? actionType = null, long? referenceId = null, string? actionUrl = null)
        {
            foreach (var userId in userIds)
            {
                await SendAsync(userId, title, body, actionType, referenceId, actionUrl);
            }
        }
    }
}
