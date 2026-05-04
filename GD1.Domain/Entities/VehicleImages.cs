using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class VehicleImage : BaseEntity
    {
        public long VehicleId { get; set; }
        public long? EventId { get; set; }

        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public string UploadedBy { get; set; } = string.Empty;
        public string? Remark { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
        public VehicleJourneyEvent? Event { get; set; }
    }
}
