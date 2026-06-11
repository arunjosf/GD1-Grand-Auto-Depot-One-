using GD1.Domain.Entities.Base;
using System;

namespace GD1.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public long? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public long? ServiceRequestId { get; set; }
        public ServiceRequest? ServiceRequest { get; set; }

        public long SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public long? ReceiverId { get; set; }
        public User? Receiver { get; set; }

        public string MessageContent { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
    }
}
