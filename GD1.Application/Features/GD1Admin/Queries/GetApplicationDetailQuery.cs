using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GD1.Domain.Entities.Enums;

// Using aliases to prevent any naming collision
using FranchiseDTOs = GD1.Application.Features.FranchiseApplication.DTOs;
using AdminDTOs = GD1.Application.Features.GD1Admin.DTOs;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetApplicationDetailQuery : IRequest<BaseResponse<AdminDTOs.AdminApplicationDto>>
    {
        public long Id { get; set; }
    }

    public class GetApplicationDetailQueryHandler : IRequestHandler<GetApplicationDetailQuery, BaseResponse<AdminDTOs.AdminApplicationDto>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetApplicationDetailQueryHandler(IFranchiseReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<AdminDTOs.AdminApplicationDto>> Handle(GetApplicationDetailQuery req, CancellationToken ct)
        {
            // For admin view, we pass 0 as applicantId to ignore ownership check in repository
            var app = await _repo.GetByIdAsync(req.Id, 0);
            if (app == null) return BaseResponse<AdminDTOs.AdminApplicationDto>.Fail("Application not found.");

            var dto = new AdminDTOs.AdminApplicationDto
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
                Status = app.Status ?? FranchiseStatus.Pending,
                AdminNotes = app.AdminNotes,
                ApplicationFee = app.ApplicationFee,
                FeeStatus = app.FeeStatus ?? "Pending",
                CreatedAt = app.CreatedAt,
                PreferredInspectionDate = app.PreferredInspectionDate,
                PropertyFrontImageUrl = app.FrontImageUrl,
                OtherImageUrls = app.OtherImageUrls,
                LotUnits = app.LotUnits.Select(u => new AdminDTOs.AdminLotUnitDto
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
                    LotImages = u.OwnerImages.Select(img => new AdminDTOs.AdminPropertyImageDto
                    {
                        Id = img.Id,
                        Label = img.Label,
                        ImageUrl = img.ImageUrl,
                        UploadedBy = img.UploadedBy,
                        Remark = img.Remark
                    }).ToList()
                }).ToList()
            };

            // Get the report from the latest assignment
            var currentAssignment = app.Assignments.OrderByDescending(a => a.ScheduledDate).FirstOrDefault();
            if (currentAssignment != null)
            {
                dto.AssignedAgent = new AdminDTOs.AdminAgentSummaryDto
                {
                    Id = currentAssignment.AgentId,
                    Name = currentAssignment.AgentName,
                    City = currentAssignment.AgentCity,
                    SelfieUrl = currentAssignment.AgentSelfieUrl,
                    PhoneNumber = currentAssignment.PhoneNumber 
                };

                if (currentAssignment.Report != null)
                {
                    // Explicitly use the unique Franchise version of the DTO
                    var sourceReport = currentAssignment.Report;

                    dto.InspectionReport = new AdminDTOs.AdminInspectionReportDto
                    {
                        Id = sourceReport.Id,
                        StartedAt = sourceReport.StartedAt,
                        CompletedAt = sourceReport.CompletedAt,
                        OverallDescription = sourceReport.OverallDescription,
                        PropertyImages = sourceReport.PropertyImages.Select(i => new AdminDTOs.AdminPropertyImageDto
                        {
                            Id = i.Id,
                            Label = i.Label,
                            ImageUrl = i.ImageUrl,
                            UploadedBy = i.UploadedBy,
                            Remark = i.Remark
                        }).ToList(),
                        Items = sourceReport.Items.Select(item => new AdminDTOs.AdminInspectionItemDto
                        {
                            Id = item.Id,
                            LotUnitId = item.LotUnitId,
                            LotLabel = item.LotLabel,
                            TaskName = item.TaskName,
                            IsVerified = item.IsVerified,
                            Remarks = item.Remarks,
                            UnitImages = item.UnitImages.Select(img => new AdminDTOs.AdminPropertyImageDto
                            {
                                Id = img.Id,
                                Label = img.Label,
                                ImageUrl = img.ImageUrl,
                                UploadedBy = img.UploadedBy,
                                Remark = img.Remark
                            }).ToList()
                        }).ToList()
                    };
                }
            }

            return BaseResponse<AdminDTOs.AdminApplicationDto>.Ok(dto);
        }
    }
}
