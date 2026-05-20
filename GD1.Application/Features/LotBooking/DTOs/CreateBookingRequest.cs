using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GD1.Application.Features.LotBooking.DTOs
{
    public class CreateBookingRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "Invalid VehicleId.")]
        public long VehicleId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Invalid PropertyId.")]
        public long PropertyId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Invalid SlotId.")]
        public long? SlotId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
