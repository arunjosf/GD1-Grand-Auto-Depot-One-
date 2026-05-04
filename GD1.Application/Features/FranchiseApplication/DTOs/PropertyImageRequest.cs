using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class PropertyImageRequest
    {
        public long? LotUnitId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }
}
