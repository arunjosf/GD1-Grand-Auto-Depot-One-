using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GD1.Application.Features.Vehicle.DTOs
{
    public class AddVehicleRequest
    {
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Invalid vehicle year.")]
        public int Year { get; set; }

        [Required]
        [StringLength(20)]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        public string OwnerIdProofUrl { get; set; } = string.Empty;

        [Required]
        public string VehicleRcUrl { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? FuelType { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one image is required.")]
        public List<VehicleImageRequest> Images { get; set; } = [];
    }

    public class VehicleImageRequest
    {
        [Required]
        [StringLength(100)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [Url(ErrorMessage = "Invalid image URL.")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Remark { get; set; }
    }
}
