using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Commands
{
    public class AddVehicleImagesCommand : IRequest<BaseResponse<string>>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public long VehicleId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public long UploadedBy { get; set; }

        // Specific image labels
        public string FrontImageUrl { get; set; } = string.Empty;
        public string RearImageUrl { get; set; } = string.Empty;
        public string LeftSideImageUrl { get; set; } = string.Empty;
        public string RightSideImageUrl { get; set; } = string.Empty;
        public string InteriorImageUrl { get; set; } = string.Empty;
        public string OdometerImageUrl { get; set; } = string.Empty;
    }

    public class AddVehicleImagesCommandHandler : IRequestHandler<AddVehicleImagesCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleImage> _imageRepo;

        public AddVehicleImagesCommandHandler(IGenericRepository<GD1.Domain.Entities.VehicleImage> imageRepo)
        {
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<string>> Handle(AddVehicleImagesCommand cmd, CancellationToken cancellationToken)
        {
            var labels = new[] { "Front", "Rear", "LeftSide", "RightSide", "Interior", "Odometer" };
            var urls = new[] { cmd.FrontImageUrl, cmd.RearImageUrl, cmd.LeftSideImageUrl, cmd.RightSideImageUrl, cmd.InteriorImageUrl, cmd.OdometerImageUrl };

            for (int i = 0; i < labels.Length; i++)
            {
                if (!string.IsNullOrEmpty(urls[i]))
                {
                    var vehicleImage = new GD1.Domain.Entities.VehicleImage
                    {
                        VehicleId = cmd.VehicleId,
                        UploadedBy = cmd.UploadedBy.ToString(),
                        Label = labels[i],
                        ImageUrl = urls[i]
                    };
                    await _imageRepo.AddAsync(vehicleImage);
                }
            }

            return BaseResponse<string>.Ok(string.Empty, "Vehicle images uploaded successfully.");
        }
    }
}
