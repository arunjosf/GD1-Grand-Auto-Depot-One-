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

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class CreateBookingCommand : IRequest<BaseResponse<long>>
    {
        public CreateBookingRequest Request { get; set; } = null!;
        public long OwnerId { get; set; }
    }

    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.OwnerId).GreaterThan(0);
            RuleFor(x => x.Request.VehicleId).GreaterThan(0);
            RuleFor(x => x.Request.LotId).GreaterThan(0);
            RuleFor(x => x.Request.StartDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past.");
            RuleFor(x => x.Request.EndDate).GreaterThan(x => x.Request.StartDate).WithMessage("End date must be after start date.");
            RuleFor(x => x.Request.Plan).NotEmpty();
        }
    }

    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _repo;

        public CreateBookingCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Booking> repo)
            => _repo = repo;

        public async Task<BaseResponse<long>> Handle(CreateBookingCommand cmd, CancellationToken cancellationToken)
        {
            var req = cmd.Request;

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
