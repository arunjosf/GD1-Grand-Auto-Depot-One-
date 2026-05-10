using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class LotUnitImage : BaseEntity
    {
        public long LotUnitId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public string? Remark { get; set; }
        public string UploadedBy { get; set; } = "Owner";

        public LotUnit LotUnit { get; set; } = null!;
    }
}
