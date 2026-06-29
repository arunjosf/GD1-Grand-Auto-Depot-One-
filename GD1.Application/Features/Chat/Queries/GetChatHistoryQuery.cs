using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Chat.Queries
{
    public class GetChatHistoryQuery : IRequest<List<ChatMessageDto>>
    {
        public long? BookingId { get; set; }
        public long? ServiceRequestId { get; set; }
        public long? DirectUserId { get; set; }
        public long UserId { get; set; }
    }

    public class ChatMessageDto
    {
        public long Id { get; set; }
        public long? BookingId { get; set; }
        public long? ServiceRequestId { get; set; }
        public long SenderId { get; set; }
        public long? ReceiverId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, List<ChatMessageDto>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Domain.Entities.ServiceRequest> _serviceRepo;
        private readonly IGenericRepository<ChatMessage> _chatRepo;

        public GetChatHistoryQueryHandler(
            IGenericRepository<Booking> bookingRepo, 
            IGenericRepository<Domain.Entities.ServiceRequest> serviceRepo,
            IGenericRepository<ChatMessage> chatRepo)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _chatRepo = chatRepo;
        }

        public async Task<List<ChatMessageDto>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
        {
            if (request.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(request.BookingId.Value);
                if (booking == null) throw new Exception("Booking not found.");
            }
            else if (request.ServiceRequestId.HasValue)
            {
                var service = await _serviceRepo.GetByIdAsync(request.ServiceRequestId.Value);
                if (service == null) throw new Exception("Service Request not found.");
            }
            else if (request.DirectUserId.HasValue)
            {
                // Direct chat validation, no specific entity check needed.
            }
            else
            {
                throw new Exception("Either BookingId, ServiceRequestId, or DirectUserId must be provided.");
            }

            var allChats = await _chatRepo.GetAllAsync();
            
            var relevantChats = allChats
                .Where(c => 
                    (request.BookingId.HasValue && c.BookingId == request.BookingId.Value) ||
                    (request.ServiceRequestId.HasValue && c.ServiceRequestId == request.ServiceRequestId.Value) ||
                    (request.DirectUserId.HasValue && c.BookingId == null && c.ServiceRequestId == null &&
                     ((c.SenderId == request.UserId && c.ReceiverId == request.DirectUserId.Value) ||
                      (c.SenderId == request.DirectUserId.Value && c.ReceiverId == request.UserId))))
                .ToList();

            bool needsSave = false;
            foreach(var msg in relevantChats)
            {
                if (!msg.IsRead && msg.SenderId != request.UserId)
                {
                    msg.IsRead = true;
                    await _chatRepo.UpdateAsync(msg);
                    needsSave = true;
                }
            }

            var messages = relevantChats
                .OrderBy(c => c.CreatedAt)
                .Select(c => new ChatMessageDto
                {
                    Id = c.Id,
                    BookingId = c.BookingId,
                    ServiceRequestId = c.ServiceRequestId,
                    SenderId = c.SenderId,
                    ReceiverId = c.ReceiverId,
                    MessageContent = c.MessageContent,
                    CreatedAt = c.CreatedAt
                })
                .ToList();

            return messages;
        }
    }
}
