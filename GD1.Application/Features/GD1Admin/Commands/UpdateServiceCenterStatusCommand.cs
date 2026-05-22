using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class UpdateServiceCenterStatusCommand : IRequest<BaseResponse<long>>
    {
        public long Id { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationReviewDecision Decision { get; set; }
        public string? AdminNotes { get; set; }
        public long AdminId { get; set; }
    }

    public class UpdateServiceCenterStatusCommandHandler : IRequestHandler<UpdateServiceCenterStatusCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> _imageRepo;

        public UpdateServiceCenterStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> imageRepo)
        {
            _appRepo = appRepo;
            _scRepo = scRepo;
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<long>> Handle(UpdateServiceCenterStatusCommand cmd, CancellationToken ct)
        {
            var app = await _appRepo.GetByIdAsync(cmd.Id);
            if (app == null) return BaseResponse<long>.Fail("Service Center application not found.");

            if (cmd.Decision == GD1.Domain.Entities.Enums.ApplicationReviewDecision.Approved)
            {
                app.Status = "Approved";

                // Provision the actual Service Center
                var sc = new GD1.Domain.Entities.ServiceCenter
                {
                    AdminId = app.ApplicantId,
                    Name = app.Name,
                    OwnerName = app.OwnerName,
                    Email = app.Email,
                    PhoneNumber = app.PhoneNumber,
                    AddressLine = app.AddressLine,
                    City = app.City,
                    District = app.District,
                    State = app.State,
                    Country = app.Country,
                    PostalCode = app.PostalCode,
                    Latitude = app.Latitude,
                    Longitude = app.Longitude,
                    
                    OemCertificateUrl = app.OemCertificateUrl,
                    SupportedBrand = app.SupportedBrand,
                    OwnerIdProofUrl = app.OwnerIdProofUrl,

                    Status = "Approved",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _scRepo.AddAsync(sc);

                var images = await _imageRepo.FindAsync(i => i.ApplicationId == app.Id);
                foreach (var img in images)
                {
                    img.ServiceCenterId = sc.Id;
                    await _imageRepo.UpdateAsync(img);
                }
            }
            else if (cmd.Decision == GD1.Domain.Entities.Enums.ApplicationReviewDecision.Rejected)
            {
                app.Status = "Rejected";
            }
            else
            {
                return BaseResponse<long>.Fail("Invalid decision.");
            }

            app.AdminNotes = cmd.AdminNotes;

            await _appRepo.UpdateAsync(app);

            return BaseResponse<long>.Ok(app.Id, "Service Center status updated successfully.");
        }
    }
}
