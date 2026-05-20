using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class InspectionReport : BaseEntity
    {
        public long AssignmentId { get; set; }
        
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? OverallDescription { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public InspectionDecision AdminDecision { get; set; } = InspectionDecision.Pending;
        public string? AdminRemarks { get; set; }

        public InspectionAssignment Assignment { get; set; } = null!;
        
        // Direct Slot verifications
        public ICollection<InspectionSlotItem> SlotVerifications { get; set; } = [];
        
        // Agent uploaded site images
        public ICollection<PropertyImage> SiteImages { get; set; } = [];
    }
}
