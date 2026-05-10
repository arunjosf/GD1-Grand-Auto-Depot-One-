using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace GD1.Application.Features.Vehicle.DTOs
    {
        public class EditVehicleRequest
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

            [StringLength(50)]
            public string? Color { get; set; }

            [StringLength(50)]
            public string? FuelType { get; set; }

            [Required]
            [StringLength(50)]
            public string VehicleType { get; set; } = string.Empty;

            [Url(ErrorMessage = "Invalid document URL.")]
            public string? DocumentUrls { get; set; }
        }
    }

