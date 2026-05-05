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
        public long ApplicationId { get; set; }
        public long LotUnitId { get; set; }
        public long AgentId { get; set; }
        public long AssignedBy { get; set; }

        public string AccessToken { get; set; } = string.Empty;

        public string PasscodeHash { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string ChecklistJson { get; set; } = "[]";
        public string? AgentRemarks { get; set; }

        public string Status { get; set; } = "Assigned";

        public string? Result { get; set; }

        public string? AdminDecision { get; set; }
        public string? AdminRemarks { get; set; }
        public DateTime? DecisionAt { get; set; }

        public FranchiseApplication Application { get; set; } = null!;
        public GD1Agents Agent { get; set; } = null!;
        public LotUnit LotUnit { get; set; } = null!;
    }
}
