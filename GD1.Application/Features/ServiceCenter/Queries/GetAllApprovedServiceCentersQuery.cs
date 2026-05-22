using GD1.Application.Common;
using GD1.Application.Features.ServiceCenter.DTOs;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetAllApprovedServiceCentersQuery : IRequest<BaseResponse<IEnumerable<ServiceCenterDto>>>
    {
    }

    public class GetAllApprovedServiceCentersQueryHandler : IRequestHandler<GetAllApprovedServiceCentersQuery, BaseResponse<IEnumerable<ServiceCenterDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;

        public GetAllApprovedServiceCentersQueryHandler(IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo)
        {
            _scRepo = scRepo;
        }

        public async Task<BaseResponse<IEnumerable<ServiceCenterDto>>> Handle(GetAllApprovedServiceCentersQuery request, CancellationToken ct)
        {
            var all = await _scRepo.GetAllAsync();
            var approved = all.Where(sc => sc.Status == "Approved")
                              .Select(sc => new ServiceCenterDto
                              {
                                  Id = sc.Id,
                                  Name = sc.Name,
                                  PhoneNumber = sc.PhoneNumber,
                                  Email = sc.Email,
                                  AddressLine = sc.AddressLine,
                                  City = sc.City,
                                  District = sc.District,
                                  State = sc.State,
                                  Country = sc.Country,
                                  PostalCode = sc.PostalCode,
                                  SupportedBrand = sc.SupportedBrand,
                                  Latitude = sc.Latitude,
                                  Longitude = sc.Longitude
                              }).ToList();

            return BaseResponse<IEnumerable<ServiceCenterDto>>.Ok(approved);
        }
    }
}
