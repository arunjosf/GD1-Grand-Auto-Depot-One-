using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class VehicleStorageSlot : BaseEntity
    {
        public long PropertyId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string SlotType { get; set; } = "Private Garage";
        public bool IsOccupied { get; set; }
        
        // Dimensions at Slot level
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }

        public string? ImageUrl { get; set; }

        public VehicleStorageProperty Property { get; set; } = null!;
    }
}
