using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class LotManager : BaseEntity
    {
        public long PropertyId { get; set; }
        public long ManagerId { get; set; }
        public long AddedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public AgentApprovalStatus ApprovalStatus { get; set; } = AgentApprovalStatus.Pending;
        public string? SelfieUrl { get; set; }
        public string? IdProofUrl { get; set; }
        public decimal? Salary { get; set; } = 15000m;

        public VehicleStorageProperty Property { get; set; } = null!;
        public User Manager { get; set; } = null!;
    }
}
