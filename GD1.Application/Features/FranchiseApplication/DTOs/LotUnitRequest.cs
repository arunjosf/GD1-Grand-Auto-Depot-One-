using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class LotUnitRequest
    {
        [StringLength(50)]
        public string? Label { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        public string Tier { get; set; } = "Tier1";

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasFireSafety { get; set; }
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public List<string>? ExtraFacilities { get; set; }
        public List<SlotRequest>? Slots { get; set; }

        public string BusinessRegistrationUrl { get; set; } = string.Empty;
        public string LicenseDocumentUrl { get; set; } = string.Empty;
        public string OwnerIdProofUrl { get; set; } = string.Empty;
        public string PropertyProofUrl { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one image is required.")]
        public List<string> Images { get; set; } = [];
    }
    public class SlotRequest
    {
        public string SlotNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
