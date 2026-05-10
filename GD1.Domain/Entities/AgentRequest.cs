using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class AgentRequest : BaseEntity
    {
        public long AssignmentId { get; set; }
        
        public string Description { get; set; } = string.Empty;
        public DateTime? RequestedDate { get; set; } // For reschedules
        public AppealStatus Status { get; set; } = AppealStatus.Pending;
        public string? AdminRemarks { get; set; }

        public InspectionAssignment Assignment { get; set; } = null!;
    }
}
