using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Agent : BaseEntity
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Agent;
        public string? Email { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string CoverageArea { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public string? InvitationToken { get; set; }
        public string? SelfieUrl { get; set; }
        public string? IdProofUrl { get; set; }
        public AgentApprovalStatus ApprovalStatus { get; set; } = AgentApprovalStatus.Pending;

        public User User { get; set; } = null!;
        public ICollection<InspectionAssignment> Assignments { get; set; } = [];
    }
}
