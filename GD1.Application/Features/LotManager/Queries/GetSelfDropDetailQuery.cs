using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetSelfDropDetailQuery : IRequest<BaseResponse<SelfDropDetailDto>>
    {
        public long BookingId { get; set; }
        public long UserId { get; set; }
        public UserRole Role { get; set; }
    }

    public class SelfDropDetailDto
    {
        public long BookingId { get; set; }
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string VehicleImage { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string? OwnerIdProofUrl { get; set; }
        public string? VehicleRcUrl { get; set; }
        public string Status { get; set; } = string.Empty;

        public string? FrontImageUrl { get; set; }
        public string? RearImageUrl { get; set; }
        public string? LeftSideImageUrl { get; set; }
        public string? RightSideImageUrl { get; set; }
        public string? InteriorImageUrl { get; set; }
        public string? OdometerImageUrl { get; set; }
        public string? ManagerRemarks { get; set; }
        public System.DateTime? VerifiedAt { get; set; }
    }

    public class GetSelfDropDetailQueryHandler : IRequestHandler<GetSelfDropDetailQuery, BaseResponse<SelfDropDetailDto>>
    {
        private readonly IBookingReadRepository _bookingRepo;

        public GetSelfDropDetailQueryHandler(IBookingReadRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<SelfDropDetailDto>> Handle(GetSelfDropDetailQuery request, CancellationToken cancellationToken)
        {
            var detail = await _bookingRepo.GetSelfDropDetailAsync(request.BookingId, request.UserId, request.Role);
            if (detail == null) return BaseResponse<SelfDropDetailDto>.Fail("Self drop detail not found or unauthorized");
            return BaseResponse<SelfDropDetailDto>.Ok(detail);
        }
    }
}
