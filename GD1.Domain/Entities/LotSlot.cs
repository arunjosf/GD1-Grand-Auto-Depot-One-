using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class LotSlot : BaseEntity
    {
        public Guid LotId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string SlotType { get; set; } = string.Empty;

        public bool IsOccupied { get; set; } = false;
        public string? QRCodeUrl { get; set; }

        public StorageLot Lot { get; set; } = null!;
    }
}
