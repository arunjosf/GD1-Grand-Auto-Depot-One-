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
        public long VehicleId { get; set; }
        public long UploadedBy { get; set; }
        public List<ImageDto> Images { get; set; } = [];
    }

    public class ImageDto
    {
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
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
            foreach (var img in cmd.Images)
            {
                var vehicleImage = new GD1.Domain.Entities.VehicleImage
                {
                    VehicleId = cmd.VehicleId,
                    UploadedBy = cmd.UploadedBy.ToString(),
                    Label = img.Label,
                    ImageUrl = img.ImageUrl,
                    Remark = img.Remark
                };
                await _imageRepo.AddAsync(vehicleImage);
            }

            return BaseResponse<string>.Ok(string.Empty, "Vehicle images uploaded successfully.");
        }
    }
}
