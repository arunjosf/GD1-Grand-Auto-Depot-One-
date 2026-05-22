using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetApplicationDetailQuery : IRequest<BaseResponse<AdminApplicationDto>>
    {
        public long Id { get; set; }
    }

    public class GetApplicationDetailQueryHandler : IRequestHandler<GetApplicationDetailQuery, BaseResponse<AdminApplicationDto>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetApplicationDetailQueryHandler(IFranchiseReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<AdminApplicationDto>> Handle(GetApplicationDetailQuery query, CancellationToken ct)
        {
            var app = await _repo.GetByIdAsync(query.Id, 0); // Admin can see all
            if (app == null) return BaseResponse<AdminApplicationDto>.Fail("Application not found.");

            var adminDto = new AdminApplicationDto
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
                OemCertificateUrl = app.OemCertificateUrl,
                SupportedBrand = app.SupportedBrand,
                Status = app.Status ?? FranchiseStatus.Pending,
                IsAiVerified = app.IsAiVerified,
                AdminNotes = app.AdminNotes,
                ApplicationFee = app.ApplicationFee,
                PricePerDay = app.PricePerDay,
                FeeStatus = app.FeeStatus ?? "Pending",
                CreatedAt = app.CreatedAt,
                PreferredInspectionDate = app.PreferredInspectionDate,
                
                HasCCTV = app.HasCCTV,
                HasSecurity = app.HasSecurity,
                HasWorkshop = app.HasWorkshop,
                HasWashingArea = app.HasWashingArea,
                HasFireSafety = app.HasFireSafety,

                PropertyFrontImageUrl = app.PropertyImages.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? app.FrontImageUrl,
                OtherImageUrls = app.PropertyImages.Where(i => !i.IsMain).Select(i => i.ImageUrl).ToList(),
                
                Slots = app.Slots.Select(s => new AdminFranchiseSlotDto
                {
                    Id = s.Id,
                    SlotNumber = s.SlotNumber,
                    SquareFeet = s.SquareFeet,
                    HeightFeet = s.HeightFeet,
                    ImageUrl = s.ImageUrl
                }).ToList(),

                Assignments = app.Assignments.Select(a => new GD1.Application.Features.FranchiseApplication.DTOs.InspectionAssignmentDto
                {
                    Id = a.Id,
                    ScheduledDate = a.ScheduledDate,
                    Status = a.Status,
                    AgentName = a.AgentName
                }).ToList()
            };

            var latestAssignment = app.Assignments.OrderByDescending(a => a.ScheduledDate).FirstOrDefault(a => a.Report != null);
            if (latestAssignment?.Report != null)
            {
                adminDto.InspectionReport = new AdminInspectionReportDto
                {
                    Id = latestAssignment.Report.Id,
                    StartedAt = latestAssignment.Report.StartedAt,
                    CompletedAt = latestAssignment.Report.CompletedAt,
                    OverallDescription = latestAssignment.Report.OverallDescription,
                    SlotVerifications = latestAssignment.Report.SlotVerifications.Select(sv => new AdminInspectionSlotDto
                    {
                        SlotNumber = sv.SlotNumber,
                        IsVerified = sv.IsVerified,
                        SquareFeet = sv.SquareFeet,
                        HeightFeet = sv.HeightFeet,
                        ImageUrl = sv.ImageUrl
                    }).ToList(),
                    SiteImages = latestAssignment.Report.SiteImages.Select(img => new AdminPropertyImageDto
                    {
                        Id = img.Id,
                        Label = img.Label,
                        ImageUrl = img.ImageUrl,
                        UploadedBy = img.UploadedBy,
                        Remark = img.Remark
                    }).ToList()
                };
            }

            return BaseResponse<AdminApplicationDto>.Ok(adminDto);
        }
    }
}
