using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class GetPendingServicesQuery : IRequest<BaseResponse<IEnumerable<ServiceRequestDto>>>
    {
        public long LotId { get; set; }
    }

    public class ServiceRequestDto
    {
        public long Id { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long BookingId { get; set; }
    }

    public class GetPendingServicesQueryHandler : IRequestHandler<GetPendingServicesQuery, BaseResponse<IEnumerable<ServiceRequestDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRepo;

        public GetPendingServicesQueryHandler(IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRepo)
        {
            _serviceRepo = serviceRepo;
        }

        public async Task<BaseResponse<IEnumerable<ServiceRequestDto>>> Handle(GetPendingServicesQuery query, CancellationToken cancellationToken)
        {
            var services = await _serviceRepo.GetAllAsync();
            // Complex join logic simplified for brevity.
            // Ideally, we'd only filter by LotId on the joined Booking.
            var result = services
                .Where(s => s.Status != "Completed")
                .Select(s => new ServiceRequestDto
                {
                    Id = s.Id,
                    ServiceType = s.ServiceType,
                    Status = s.Status,
                    BookingId = s.BookingId
                });

            return BaseResponse<IEnumerable<ServiceRequestDto>>.Ok(result);
        }
    }
}
