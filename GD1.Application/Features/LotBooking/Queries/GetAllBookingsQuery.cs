using GD1.Application.Common;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class GetAllBookingsQuery : IRequest<BaseResponse<IEnumerable<BookingDto>>>
    {
    }

    public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, BaseResponse<IEnumerable<BookingDto>>>
    {
        private readonly IBookingReadRepository _repo;

        public GetAllBookingsQueryHandler(IBookingReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<BookingDto>>> Handle(
            GetAllBookingsQuery query, CancellationToken cancellationToken)
        {
            var bookings = await _repo.GetAllAsync();
            return BaseResponse<IEnumerable<BookingDto>>.Ok(bookings);
        }
    }
}
