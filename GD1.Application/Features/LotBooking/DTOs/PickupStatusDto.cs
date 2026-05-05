using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.DTOs
{
    public class PickupStatusDto
    {
        public long BookingId { get; set; }
        public string PickupStatus { get; set; } = string.Empty;
        public bool OtpVerified { get; set; }
        public string? ManagerIdImageUrl { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
    }
}
