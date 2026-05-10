using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllApplicationsQuery : IRequest<BaseResponse<IEnumerable<AdminApplicationDto>>>
    {
        public FranchiseStatus? Status { get; set; }
        public string? SearchTerm { get; set; } 
        public string? SortBy { get; set; } = "CreatedAt";
        public bool Descending { get; set; } = true;
    }

    public class GetAllApplicationsQueryHandler : IRequestHandler<GetAllApplicationsQuery, BaseResponse<IEnumerable<AdminApplicationDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetAllApplicationsQueryHandler(IFranchiseReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<AdminApplicationDto>>> Handle(GetAllApplicationsQuery query, CancellationToken ct)
        {
            var result = await _repo.GetAllApplicationsAsync(query.Status, query.SearchTerm, query.SortBy, query.Descending);

            var adminDtos = result.Select(app =>
            {
                var dto = new AdminApplicationDto
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
                    Latitude = app.Latitude,
                    Longitude = app.Longitude,
                    Status = app.Status,
                    AdminNotes = app.AdminNotes,
                    ApplicationFee = app.ApplicationFee,
                    FeeStatus = app.FeeStatus,
                    CreatedAt = app.CreatedAt,
                    PreferredInspectionDate = app.PreferredInspectionDate,
                    
                    PropertyFrontImageUrl = app.FrontImageUrl,
                    OtherImageUrls = app.OtherImageUrls,
                    ExtraFacilities = app.ExtraFacilities,

                    // Re-application Logic
                    IsReapplication = app.PastRejections.Any(),
                    RejectionHistory = app.PastRejections,

                    LotUnits = app.LotUnits.Select(lot => new AdminLotUnitDto
                    {
                        Id = lot.Id,
                        Label = lot.Label,
                        Tier = lot.Tier,
                        Capacity = lot.Capacity,
                        HasCCTV = lot.HasCCTV,
                        HasSecurity = lot.HasSecurity,
                        HasWorkshop = lot.HasWorkshop,
                        HasWashingArea = lot.HasWashingArea,
                        HasFireSafety = lot.HasFireSafety,
                        ExtraFacilities = lot.ExtraFacilities,
                        Status = lot.Status,
                        
                        LotImages = lot.OwnerImages.Select(img => new AdminPropertyImageDto
                        {
                            Id = img.Id,
                            Label = img.Label,
                            ImageUrl = img.ImageUrl,
                            UploadedBy = img.UploadedBy,
                            Remark = img.Remark
                        }).ToList()
                    }).ToList()
                };

                // Conditional Data Mapping
                var currentAssignment = app.Assignments.OrderByDescending(a => a.ScheduledDate).FirstOrDefault();
                if (currentAssignment != null)
                {
                    dto.AssignedAgent = new AdminAgentSummaryDto
                    {
                        Id = currentAssignment.AgentId,
                        Name = currentAssignment.AgentName,
                        City = currentAssignment.AgentCity,
                        SelfieUrl = currentAssignment.AgentSelfieUrl
                    };

                    if (currentAssignment.Report != null)
                    {
                        dto.InspectionReport = currentAssignment.Report;
                    }
                }

                return dto;
            });

            return BaseResponse<IEnumerable<AdminApplicationDto>>.Ok(adminDtos);
        }
    }
}
