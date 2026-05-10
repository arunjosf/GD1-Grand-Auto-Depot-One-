using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.DTOs
{
    public class BookingDto
    {
        public long Id { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Invalid VehicleId.")]
        public long VehicleId { get; set; }

        [Required]
        [StringLength(100)]
        public string VehicleBrand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string VehicleModel { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RegistrationNo { get; set; } = string.Empty;

        [Range(1, long.MaxValue, ErrorMessage = "Invalid LotId.")]
        public long LotId { get; set; }

        [Required]
        [StringLength(150)]
        public string LotName { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string LotAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string? SlotNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Plan { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be non-negative.")]
        public decimal TotalCost { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
