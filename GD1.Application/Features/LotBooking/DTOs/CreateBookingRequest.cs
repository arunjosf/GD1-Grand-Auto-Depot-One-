using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.DTOs
{
    public class CreateBookingRequest
    {
        public long VehicleId { get; set; }
        public long LotId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Plan { get; set; } = "Basic";
        public long TermsId { get; set; }
    }
}
