using GD1.Application.Common;
using GD1.Domain.Entities.Enums;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Queries
{
    public class GetMyApplicationsQuery : IRequest<BaseResponse<IEnumerable<UserApplicationDto>>>
    {
        public long ApplicantId { get; set; }
    }

    public class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, BaseResponse<IEnumerable<UserApplicationDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetMyApplicationsQueryHandler(IFranchiseReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<UserApplicationDto>>> Handle(GetMyApplicationsQuery query, CancellationToken ct)
        {
            var apps = await _repo.GetByApplicantIdAsync(query.ApplicantId);
            
            var userApps = apps.Select(app => new UserApplicationDto
            {
                Id = app.Id,
                ApplicationType = app.ApplicationType,
                BusinessName = app.BusinessName,
                OwnerName = app.OwnerName,
                ContactEmail = app.ContactEmail,
                PhoneNumber = app.PhoneNumber,
                AddressLine = app.AddressLine,
                City = app.City,
                State = app.State,
                PostalCode = app.PostalCode,
                PreferredInspectionDate = app.PreferredInspectionDate,
                Status = app.Status ?? FranchiseStatus.Pending,
                AdminNotes = app.AdminNotes,
                ApplicationFee = app.ApplicationFee,
                FeeStatus = app.FeeStatus,
                CreatedAt = app.CreatedAt,
                FrontImageUrl = app.FrontImageUrl,
                OtherImageUrls = app.OtherImageUrls,

                LotUnits = app.LotUnits.Select(u => new UserLotUnitDto
                {
                    Id = u.Id,
                    Label = u.Label,
                    Tier = u.Tier,
                    Capacity = u.Capacity,
                    HasCCTV = u.HasCCTV,
                    HasSecurity = u.HasSecurity,
                    HasWorkshop = u.HasWorkshop,
                    HasWashingArea = u.HasWashingArea,
                    HasFireSafety = u.HasFireSafety,
                    ExtraFacilities = u.ExtraFacilities,
                    Status = u.Status ?? FranchiseStatus.Pending,
                    OwnerImages = u.OwnerImages.Select(i => new UserPropertyImageDto
                    {
                        Id = i.Id,
                        Label = i.Label,
                        ImageUrl = i.ImageUrl
                    }).ToList()
                }).ToList()
            });

            return BaseResponse<IEnumerable<UserApplicationDto>>.Ok(userApps);
        }
    }
}
