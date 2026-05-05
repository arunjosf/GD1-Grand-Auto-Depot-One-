using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class LotUnitRequest
    {
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Tier { get; set; } = "Tier1";
        public int Capacity { get; set; }
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public List<PropertyImageRequest> Images { get; set; } = [];
    }
}
