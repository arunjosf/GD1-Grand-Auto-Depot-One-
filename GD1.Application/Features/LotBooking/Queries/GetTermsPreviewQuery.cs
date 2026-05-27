using GD1.Application.Common;
using GD1.Application.Features.LotBooking.Templates;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class GetTermsPreviewQuery : IRequest<BaseResponse<string>>
    {
        public long VehicleId { get; set; }
        public long PropertyId { get; set; }
        public long OwnerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetTermsPreviewQueryHandler : IRequestHandler<GetTermsPreviewQuery, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;

        public GetTermsPreviewQueryHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo)
        {
            _vehicleRepo = vehicleRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<string>> Handle(GetTermsPreviewQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                return BaseResponse<string>.Fail("Vehicle not found.");

            var property = await _propertyRepo.GetByIdAsync(request.PropertyId);
            if (property == null)
                return BaseResponse<string>.Fail("Property not found.");

            var user = await _userRepo.GetByIdAsync(request.OwnerId);
            if (user == null)
                return BaseResponse<string>.Fail("User not found.");

            var html = AgreementTemplate.Generate(
                customerName: user.FullName,
                customerEmail: user.Email,
                vehicleBrand: vehicle.Brand,
                vehicleModel: vehicle.Model,
                vehicleYear: vehicle.Year.ToString(),
                registrationNo: vehicle.RegistrationNo,
                vehicleType: vehicle.Category,
                propertyName: property.Name,
                propertyAddress: property.AddressLine,
                propertyCity: property.City,
                propertyState: property.State,
                startDate: request.StartDate.ToString("dd MMM yyyy"),
                endDate: request.EndDate.ToString("dd MMM yyyy"),
                pricePerDay: property.PricePerDay,
                agreementDate: DateTime.UtcNow.ToString("dd MMM yyyy")
            );

            return BaseResponse<string>.Ok(html, "Terms preview generated.");
        }
    }
}
