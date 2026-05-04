using GD1.Application.Common;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Enums;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class CreateBookingCommand
    {
        public CreateBookingRequest Request { get; set; } = null!;
        public long OwnerId { get; set; }
    }

    public class CreateBookingCommandHandler
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _repo;

        public CreateBookingCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Booking> repo)
            => _repo = repo;

        public async Task<BaseResponse<long>> HandleAsync(CreateBookingCommand cmd)
        {
            var req = cmd.Request;

            if (req.StartDate >= req.EndDate)
                throw new InvalidOperationException(
                    "End date must be after start date.");

            if (req.StartDate.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException(
                    "Start date cannot be in the past.");

            var booking = new GD1.Domain.Entities.Booking
            {
                VehicleId = req.VehicleId,
                LotId = req.LotId,
                OwnerId = cmd.OwnerId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Plan = req.Plan,
                Status = BookingStatus.Pending,
               
            };

            await _repo.AddAsync(booking);

            return BaseResponse<long>.Ok(booking.Id,
                "Booking created. Awaiting lot confirmation.");
        }
    }
}
