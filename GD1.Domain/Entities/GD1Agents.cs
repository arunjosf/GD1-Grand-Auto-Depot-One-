using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
       public class GD1Agents : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string CoverageArea { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        public ICollection<InspectionReport> InspectionReports { get; set; } = [];
       
    }
}
