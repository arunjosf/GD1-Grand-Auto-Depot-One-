using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.DTOs
{
    public class CreateBookingRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "Invalid VehicleId.")]
        public long VehicleId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Invalid LotId.")]
        public long LotId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Plan { get; set; } = "Basic";

        [Range(1, long.MaxValue, ErrorMessage = "Invalid TermsId.")]
        public long TermsId { get; set; }
    }
}
