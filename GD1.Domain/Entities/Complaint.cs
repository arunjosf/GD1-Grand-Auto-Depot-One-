using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Complaint : BaseEntity
    {
        public long ComplainantId { get; set; } 
        public long LotId { get; set; }
        public long? BookingId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";
        public string? AdminResponse { get; set; }
        public bool IsDeleted { get; set; } = false;

        public User Complainant { get; set; } = null!;
        public StorageLot Lot { get; set; } = null!;
        public Booking? Booking { get; set; }
    }
}
