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
        Task<IEnumerable<BookingDto>> GetByLotOwnerIdAsync(long lotOwnerId);
        Task<BookingDto?> GetLotOwnerBookingDetailAsync(long bookingId, long lotOwnerId);
        Task<BookingDto?> GetDetailAdminAsync(long bookingId);
        Task<IEnumerable<BookingDto>> GetByPropertyIdAsync(long propertyId);
        Task<Dictionary<long, int>> GetOccupiedCountsAsync();
        Task<IEnumerable<BookingDto>> GetAllAsync();
        Task<IEnumerable<GD1.Application.Features.LotManager.Queries.ManagerVehicleDto>> GetLotOwnerVehiclesAsync(long lotOwnerId);
        Task<IEnumerable<GD1.Application.Features.LotManager.Queries.SelfDropDto>> GetLotOwnerSelfDropsAsync(long lotOwnerId, bool isCompleted);
        Task<GD1.Application.Features.LotManager.Queries.ManagerVehicleDetailDto> GetLotOwnerVehicleDetailAsync(long lotOwnerId, long vehicleId, long? bookingId = null);
    }
}
