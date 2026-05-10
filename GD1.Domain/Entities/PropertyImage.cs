using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class PropertyImage : BaseEntity
    {
        public long ApplicationId { get; set; }

        public string UploadedBy { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public string? Remark { get; set; }

        public GD1.Domain.Entities.FranchiseApplication Application { get; set; } = null!;
    }
}
