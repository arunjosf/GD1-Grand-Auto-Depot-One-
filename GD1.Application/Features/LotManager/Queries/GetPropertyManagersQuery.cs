using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManagement.Queries
{
    using GD1.Application.Features.LotManager.Queries; // For PerformanceGraphItemDto
    public class LotManagerDto
    {
        public long LotManagerRecordId { get; set; }
        public long ManagerUserId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string? ManagerPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime AddedAt { get; set; }
        public string? SelfieUrl { get; set; }
        public string? IdProofUrl { get; set; }
        public decimal? Salary { get; set; }
        public bool IsAvailable { get; set; }
        public List<PerformanceGraphItemDto> PerformanceGraphData { get; set; } = new();
    }

    public class GetPropertyManagersQuery : IRequest<BaseResponse<List<LotManagerDto>>>
    {
        /// <summary>The authenticated LotOwner's user ID.</summary>
        public long LotOwnerId { get; set; }

        /// <summary>The property whose managers to retrieve. If null, retrieves all managers for all properties owned by the user.</summary>
        public long? PropertyId { get; set; }

        /// <summary>Optional check date to determine manager availability.</summary>
        public DateTime? CheckDate { get; set; }
    }

    public class GetPropertyManagersQueryHandler : IRequestHandler<GetPropertyManagersQuery, BaseResponse<List<LotManagerDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly GD1.Application.Interfaces.Repositories.IManagerReadRepository _managerReadRepo;

        public GetPropertyManagersQueryHandler(
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<PickupRequest> pickupRepo,
            GD1.Application.Interfaces.Repositories.IManagerReadRepository managerReadRepo)
        {
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _pickupRepo = pickupRepo;
            _managerReadRepo = managerReadRepo;
        }

        public async Task<BaseResponse<List<LotManagerDto>>> Handle(GetPropertyManagersQuery query, CancellationToken ct)
        {
            IEnumerable<GD1.Domain.Entities.LotManager> managerRecords;

            if (query.PropertyId.HasValue)
            {
                // 1. Verify the property exists and belongs to the calling owner
                var property = await _propertyRepo.GetByIdAsync(query.PropertyId.Value);
                if (property is null)
                    return BaseResponse<List<LotManagerDto>>.Fail("Property not found.");

                if (property.LotOwnerId != query.LotOwnerId)
                    return BaseResponse<List<LotManagerDto>>.Fail("You do not own this property.");

                // 2. Fetch manager records for this property
                managerRecords = await _lotManagerRepo.FindAsync(m => m.PropertyId == query.PropertyId.Value);
            }
            else
            {
                // 1. Fetch all properties owned by this user
                var properties = await _propertyRepo.FindAsync(p => p.LotOwnerId == query.LotOwnerId);
                var propertyIds = properties.Select(p => p.Id).ToList();

                if (!propertyIds.Any())
                    return BaseResponse<List<LotManagerDto>>.Ok(new List<LotManagerDto>(), "You do not own any properties.");

                // 2. Fetch manager records for all these properties
                managerRecords = await _lotManagerRepo.FindAsync(m => propertyIds.Contains(m.PropertyId));
            }

            if (!managerRecords.Any())
                return BaseResponse<List<LotManagerDto>>.Ok(new List<LotManagerDto>(), "No managers found.");

            // Fetch active pickup requests to determine manager availability
            var activePickups = await _pickupRepo.FindAsync(p => p.ManagerId != null && p.Status != PickupStatus.Stored && p.Status != PickupStatus.Declined);
            if (query.CheckDate.HasValue)
            {
                var targetDate = query.CheckDate.Value.Date;
                activePickups = activePickups.Where(p => 
                    (p.RequestedPickupTime.HasValue && p.RequestedPickupTime.Value.Date == targetDate) ||
                    (!p.RequestedPickupTime.HasValue && p.CreatedAt.Date == targetDate)
                ).ToList();
            }
            var activeManagerIds = activePickups.Select(p => p.ManagerId!.Value).ToHashSet();

            // 3. Resolve user details for each manager
            var result = new List<LotManagerDto>();
            foreach (var record in managerRecords)
            {
                var user = await _userRepo.GetByIdAsync(record.ManagerId);
                if (user is null) continue;

                result.Add(new LotManagerDto
                {
                    LotManagerRecordId = record.Id,
                    ManagerUserId = user.Id,
                    ManagerName = user.FullName,
                    ManagerEmail = user.Email,
                    ManagerPhone = user.PhoneNumber,
                    IsActive = record.IsActive,
                    AddedAt = record.CreatedAt,
                    SelfieUrl = record.SelfieUrl,
                    IdProofUrl = record.IdProofUrl,
                    Salary = record.Salary,
                    IsAvailable = !activeManagerIds.Contains(record.Id),
                    PerformanceGraphData = (await _managerReadRepo.GetDashboardMetricsAsync(user.Id)).PerformanceGraphData
                });
            }

            return BaseResponse<List<LotManagerDto>>.Ok(result, $"{result.Count} manager(s) found.");
        }
    }
}
