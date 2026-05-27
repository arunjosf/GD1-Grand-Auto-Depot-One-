using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;

namespace GD1.Domain.Entities
{
    public class VehicleCatalogItem : BaseEntity
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double LengthFeet { get; set; }
        public double WidthFeet { get; set; }
        public double HeightFeet { get; set; }
        public string ValidYearsCsv { get; set; } = string.Empty;
    }
}
