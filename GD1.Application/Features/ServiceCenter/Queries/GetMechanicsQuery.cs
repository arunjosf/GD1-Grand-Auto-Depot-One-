using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetMechanicsQuery : IRequest<BaseResponse<IEnumerable<MechanicDto>>>
    {
        public long AdminId { get; set; }
    }

    public class MechanicDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class GetMechanicsQueryHandler : IRequestHandler<GetMechanicsQuery, BaseResponse<IEnumerable<MechanicDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<Mechanics> _mechanicsRepo;

        public GetMechanicsQueryHandler(IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo, IGenericRepository<Mechanics> mechanicsRepo)
        {
            _scRepo = scRepo;
            _mechanicsRepo = mechanicsRepo;
        }

        public async Task<BaseResponse<IEnumerable<MechanicDto>>> Handle(GetMechanicsQuery request, CancellationToken cancellationToken)
        {
            var centers = await _scRepo.FindAsync(x => x.AdminId == request.AdminId);
            var sc = centers.FirstOrDefault();
            if (sc == null) return BaseResponse<IEnumerable<MechanicDto>>.Fail("Service center not found");

            var mechanics = await _mechanicsRepo.FindAsync(x => x.ServiceCenterId == sc.Id && !x.IsDeleted);

            var dtos = mechanics.Select(m => new MechanicDto
            {
                Id = m.Id,
                FullName = m.FullName,
                PhoneNumber = m.PhoneNumber,
                Email = m.Email,
                ImageUrl = m.ImageUrl,
                IsAvailable = m.IsAvailable
            });

            return BaseResponse<IEnumerable<MechanicDto>>.Ok(dtos, "Success");
        }
    }
}
