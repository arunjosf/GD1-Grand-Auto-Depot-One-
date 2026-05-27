using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GD1.Application.Features.Vehicle.DTOs
{
    public class AddVehicleRequest
    {
        [Required]
        public long VehicleId { get; set; }

        [Range(1900, 2100, ErrorMessage = "Invalid vehicle year.")]
        public int Year { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^[a-zA-Z0-9-\s]+$", ErrorMessage = "Registration number can only contain letters, numbers, hyphens, and spaces.")]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        public string OwnerIdProofUrl { get; set; } = string.Empty;

        [Required]
        public string VehicleRcUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FuelType { get; set; } = string.Empty;
        
        [DefaultValue(false)]
        public bool IsHybrid { get; set; } = false;

        [MinLength(1, ErrorMessage = "At least one image is required.")]
        public List<VehicleImageRequest> Images { get; set; } = [];
    }

    public class VehicleImageRequest
    {
        [StringLength(100)]
        public string? Label { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid image URL.")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
