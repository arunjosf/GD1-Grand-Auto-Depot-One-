using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    namespace GD1.Application.Features.Vehicle.DTOs
    {
        public class EditVehicleRequest
        {
            [StringLength(100)]
            public string? Brand { get; set; }

            [StringLength(100)]
            public string? Model { get; set; }

            [Range(1900, 2100, ErrorMessage = "Invalid vehicle year.")]
            public int? Year { get; set; }

            [StringLength(20)]
            [RegularExpression(@"^[a-zA-Z0-9-\s]+$", ErrorMessage = "Registration number can only contain letters, numbers, hyphens, and spaces.")]
            public string? RegistrationNo { get; set; }

            [StringLength(50)]
            public string? Color { get; set; }

            [StringLength(50)]
            public string? FuelType { get; set; }

            [StringLength(50)]
            public string? Category { get; set; }

            public string? OwnerIdProofUrl { get; set; }

            public string? VehicleRcUrl { get; set; }
        }
    }

