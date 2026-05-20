using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class FranchiseSlot : BaseEntity
    {
        public long ApplicationId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        
        // Dimensions at Slot level
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }

        public GD1.Domain.Entities.FranchiseApplication Application { get; set; } = null!;
    }
}
