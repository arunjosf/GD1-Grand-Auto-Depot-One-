using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;

namespace GD1.Domain.Entities
{
    public class Agreement : BaseEntity
    {
        public long UserId { get; set; }
        public AgreementType Type { get; set; }
        
        /// <summary>
        /// ID of the entity this agreement belongs to (e.g., BookingId, ApplicationId)
        /// </summary>
        public long ReferenceId { get; set; }
        
        public string Content { get; set; } = string.Empty;
        public string? PdfUrl { get; set; }
        
        public AgreementStatus Status { get; set; } = AgreementStatus.Pending;
        public DateTime? AcceptedAt { get; set; }
        public string? IpAddress { get; set; }

        public User User { get; set; } = null!;
    }
}
