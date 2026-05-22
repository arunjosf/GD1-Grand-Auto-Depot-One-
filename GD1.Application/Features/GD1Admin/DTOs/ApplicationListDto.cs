using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class ApplicationListDto
    {
        public long Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string PropertyFrontImageUrl { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public FranchiseStatus Status { get; set; } = FranchiseStatus.Pending;
        public bool IsAiVerified { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationType ApplicationType { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public int SlotCount { get; set; }
    }
}
