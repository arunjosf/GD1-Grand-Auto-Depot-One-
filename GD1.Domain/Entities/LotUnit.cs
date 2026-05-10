using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class LotUnit : BaseEntity
    {
        public long FranchiseApplicationId { get; set; }

        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string Tier { get; set; } = "Tier1";
        public int Capacity { get; set; }

        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasFireSafety { get; set; }
        public string? ExtraFacilities { get; set; }

        public string Status { get; set; } = "Pending";
        public string? AssignedLotCode { get; set; }

        public GD1.Domain.Entities.FranchiseApplication Application { get; set; } = null!;
        public ICollection<LotUnitImage> Images { get; set; } = [];
    }
}

