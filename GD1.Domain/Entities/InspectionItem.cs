using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class InspectionItem : BaseEntity
    {
        public long ReportId { get; set; }
        public long LotUnitId { get; set; }
        
        public string TaskName { get; set; } = string.Empty; // e.g. "CCTV matches", "Security present"
        public bool IsVerified { get; set; }
        public string? Remarks { get; set; }

        public InspectionReport Report { get; set; } = null!;
        public LotUnit LotUnit { get; set; } = null!;
    }
}
