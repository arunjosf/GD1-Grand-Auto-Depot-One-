using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class DigitalAgreement : BaseEntity
    {
        public long UserId { get; set; }
        public long TermsId { get; set; }
        public long? BookingId { get; set; }
        public long? ApplicationId { get; set; }

        public string Context { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public TermsAndConditions Terms { get; set; } = null!;
    }
}
