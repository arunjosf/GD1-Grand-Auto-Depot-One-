using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Mechanics : BaseEntity
    {
        public long ServiceCenterId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? IdProofUrl { get; set; }
        public string? CertificateUrl { get; set; }
        public bool IsAvailable { get; set; } = true;

        public ServiceCenter ServiceCenter { get; set; } = null!;
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
    }
}
