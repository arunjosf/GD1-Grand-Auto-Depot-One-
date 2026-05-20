using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class InspectionSlotItem : BaseEntity
    {
        public long ReportId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? ImageUrl { get; set; }
        
        // Verified dimensions by agent
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }

        public InspectionReport Report { get; set; } = null!;
    }
}
