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
        public Guid ApplicationId { get; set; }
        public Guid LotUnitId { get; set; }
        public Guid AgentId { get; set; }
        public Guid AssignedBy { get; set; }

        public string AccessToken { get; set; } = string.Empty; 
        public string PasscodeHash { get; set; } = string.Empty; 
        public DateTime ExpiryDate { get; set; }

        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string ChecklistJson { get; set; } = string.Empty;

        public string? AgentLotFrontImageUrl { get; set; }
        public string? AgentFullPropertyImageUrl { get; set; }
        public string? AgentWorkshopImageUrl { get; set; }
        public string? AgentExtraImages { get; set; }


        public string? Result { get; set; } 
        public string Status { get; set; } = "Assigned";

        public string? AdminDecision { get; set; } 
        public string? AdminRemarks { get; set; }

        public FranchiseApplication Application { get; set; } = null!;
        public GD1Agents Agent { get; set; } = null!;
        public LotUnit LotUnit { get; set; } = null!;
    }
}
