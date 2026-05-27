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

    public class GetLotOwnerBookingsQuery : IRequest<BaseResponse<IEnumerable<BookingDto>>>
    {
        public long LotOwnerId { get; set; }
    }

    public class GetLotOwnerBookingsQueryHandler : IRequestHandler<GetLotOwnerBookingsQuery, BaseResponse<IEnumerable<BookingDto>>>
    {
        private readonly IBookingReadRepository _repo;

        public GetLotOwnerBookingsQueryHandler(IBookingReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<BookingDto>>> Handle(
            GetLotOwnerBookingsQuery query, CancellationToken cancellationToken)
        {
            var bookings = await _repo.GetByLotOwnerIdAsync(query.LotOwnerId);
            return BaseResponse<IEnumerable<BookingDto>>.Ok(bookings);
        }
    }

    public class GetBookingDetailQuery : IRequest<BaseResponse<BookingDto>>
    {
        public long BookingId { get; set; }
        public long UserId { get; set; }
        public GD1.Domain.Entities.Enums.UserRole UserRole { get; set; }
    }

    public class GetBookingDetailQueryValidator : AbstractValidator<GetBookingDetailQuery>
    {
        public GetBookingDetailQueryValidator()
        {
            RuleFor(x => x.BookingId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
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
            BookingDto? booking = null;

            if (query.UserRole == GD1.Domain.Entities.Enums.UserRole.GD1Admin)
            {
                booking = await _repo.GetDetailAdminAsync(query.BookingId);
            }
            else if (query.UserRole == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                booking = await _repo.GetLotOwnerBookingDetailAsync(query.BookingId, query.UserId);
            }
            else
            {
                booking = await _repo.GetDetailAsync(query.BookingId, query.UserId);
            }

            if (booking is null)
                throw new KeyNotFoundException("Booking not found or you don't have permission to view it.");

            if (query.UserRole == GD1.Domain.Entities.Enums.UserRole.GD1Admin || query.UserRole == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                // Removed the IsAgreementSigned == 1 check so they can view rejected and pending bookings.
            }

            return BaseResponse<BookingDto>.Ok(booking);
        }
    }
}
