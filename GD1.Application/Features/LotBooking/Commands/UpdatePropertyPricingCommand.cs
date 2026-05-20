using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class UpdatePropertyPricingCommand : IRequest<BaseResponse<string>>
    {
        public long PropertyId { get; set; }
        public long OwnerId { get; set; }
        public decimal PricePerDay { get; set; }
    }

    public class UpdatePropertyPricingCommandHandler : IRequestHandler<UpdatePropertyPricingCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;

        public UpdatePropertyPricingCommandHandler(IGenericRepository<VehicleStorageProperty> propertyRepo)
        {
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<string>> Handle(UpdatePropertyPricingCommand request, CancellationToken cancellationToken)
        {
            var property = await _propertyRepo.GetByIdAsync(request.PropertyId);
            
            if (property == null)
            {
                return BaseResponse<string>.Fail("Property not found.");
            }

            if (property.LotOwnerId != request.OwnerId)
            {
                return BaseResponse<string>.Fail("You are not authorized to update this property.");
            }

            if (request.PricePerDay <= 0)
            {
                return BaseResponse<string>.Fail("Price per day must be greater than 0.");
            }

            property.PricePerDay = request.PricePerDay;
            await _propertyRepo.UpdateAsync(property);

            return BaseResponse<string>.Ok(string.Empty, "Property pricing updated successfully.");
        }
    }
}
