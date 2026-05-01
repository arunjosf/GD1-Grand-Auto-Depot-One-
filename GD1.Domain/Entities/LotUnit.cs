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
        public int Capacity { get; set; }

        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }


        public string? OwnerLotFrontImageUrl { get; set; }
        public string? OwnerFullPropertyImageUrl { get; set; }
        public string? OwnerWorkshopImageUrl { get; set; }
        public string? OwnerExtraImages { get; set; }

        public string Status { get; set; } = "Pending";

        public FranchiseApplication Application { get; set; } = null!;
        public ICollection<InspectionReport> InspectionReports { get; set; } = [];
    }
}
