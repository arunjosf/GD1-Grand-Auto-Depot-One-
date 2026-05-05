using GD1.Application.Common;
using GD1.Application.Features.LotBooking.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Interfaces.Repositories;

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class GetMyBookingsQuery : IRequest<BaseResponse<IEnumerable<BookingDto>>>
    {
        public long OwnerId { get; set; }
    }

    public class GetMyBookingsQueryValidator : AbstractValidator<GetMyBookingsQuery>
    {
        public GetMyBookingsQueryValidator()
        {
            RuleFor(x => x.OwnerId).GreaterThan(0);
        }
    }

    public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, BaseResponse<IEnumerable<BookingDto>>>
    {
        private readonly IBookingReadRepository _repo;

        public GetMyBookingsQueryHandler(IBookingReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<BookingDto>>> Handle(
            GetMyBookingsQuery query, CancellationToken cancellationToken)
        {
            var bookings = await _repo.GetByOwnerIdAsync(query.OwnerId);
            return BaseResponse<IEnumerable<BookingDto>>.Ok(bookings);
        }
    }

    public class GetBookingDetailQuery : IRequest<BaseResponse<BookingDto>>
    {
        public long BookingId { get; set; }
        public long OwnerId { get; set; }
    }

    public class GetBookingDetailQueryValidator : AbstractValidator<GetBookingDetailQuery>
    {
        public GetBookingDetailQueryValidator()
        {
            RuleFor(x => x.BookingId).GreaterThan(0);
            RuleFor(x => x.OwnerId).GreaterThan(0);
        }
    }

    public class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, BaseResponse<BookingDto>>
    {
        private readonly IBookingReadRepository _repo;

        public GetBookingDetailQueryHandler(IBookingReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<BookingDto>> Handle(
            GetBookingDetailQuery query, CancellationToken cancellationToken)
        {
            var booking = await _repo.GetDetailAsync(
                query.BookingId, query.OwnerId);

            if (booking is null)
                throw new KeyNotFoundException("Booking not found.");

            return BaseResponse<BookingDto>.Ok(booking);
        }
    }
}
