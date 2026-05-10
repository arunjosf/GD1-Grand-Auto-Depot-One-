using GD1.Domain.Entities.Base;
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
        
        public string? AgentRemarks { get; set; }
        public string? OverallDescription { get; set; }
        
        public string? AdminDecision { get; set; } // Approved, Rejected
        public string? AdminRemarks { get; set; }
        public DateTime? DecisionAt { get; set; }

        public InspectionAssignment Assignment { get; set; } = null!;
        public ICollection<InspectionItem> Items { get; set; } = new List<InspectionItem>();
    }
}
