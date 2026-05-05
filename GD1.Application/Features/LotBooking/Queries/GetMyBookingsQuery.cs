using GD1.Application.Common;
using GD1.Application.Features.LotBooking.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Interfaces.Repositories;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class GetMyBookingsQuery
    {
        public long OwnerId { get; set; }
    }

    public class GetMyBookingsQueryHandler
    {
        private readonly IBookingReadRepository _repo;

        public GetMyBookingsQueryHandler(IBookingReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<BookingDto>>> HandleAsync(
            GetMyBookingsQuery query)
        {
            var bookings = await _repo.GetByOwnerIdAsync(query.OwnerId);
            return BaseResponse<IEnumerable<BookingDto>>.Ok(bookings);
        }
    }

    public class GetBookingDetailQuery
    {
        public long BookingId { get; set; }
        public long OwnerId { get; set; }
    }

    public class GetBookingDetailQueryHandler
    {
        private readonly IBookingReadRepository _repo;

        public GetBookingDetailQueryHandler(IBookingReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<BookingDto>> HandleAsync(
            GetBookingDetailQuery query)
        {
            var booking = await _repo.GetDetailAsync(
                query.BookingId, query.OwnerId);

            if (booking is null)
                throw new KeyNotFoundException("Booking not found.");

            return BaseResponse<BookingDto>.Ok(booking);
        }
    }
}
