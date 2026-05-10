using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class InspectionAssignment : BaseEntity
    {
        public long ApplicationId { get; set; }
        public long AgentId { get; set; }
        
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = "Assigned"; // Assigned, InProgress, Completed, Cancelled

        public GD1.Domain.Entities.FranchiseApplication Application { get; set; } = null!;
        public Agent Agent { get; set; } = null!;
        
        public ICollection<AgentRequest> Requests { get; set; } = new List<AgentRequest>();
        public InspectionReport? Report { get; set; }
    }
}
