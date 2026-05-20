using GD1.Application.Features.LotBooking.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IBookingReadRepository
    {
        Task<IEnumerable<BookingDto>> GetByOwnerIdAsync(long ownerId);
        Task<BookingDto?> GetDetailAsync(long bookingId, long ownerId);
        Task<IEnumerable<BookingDto>> GetByPropertyIdAsync(long propertyId);
        Task<Dictionary<long, int>> GetOccupiedCountsAsync();
    }
}
