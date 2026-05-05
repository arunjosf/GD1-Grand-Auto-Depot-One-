using GD1.Application.Common;
using GD1.Application.Features.Vehicle.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Interfaces.Repositories;
using BCrypt;

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class GetMyVehiclesQuery : IRequest<BaseResponse<IEnumerable<VehicleDto>>>
    {
        public long OwnerId { get; set; }
    }

    public class GetMyVehiclesQueryValidator : AbstractValidator<GetMyVehiclesQuery>
    {
        public GetMyVehiclesQueryValidator()
        {
            RuleFor(x => x.OwnerId).GreaterThan(0);
        }
    }

    public class GetMyVehiclesQueryHandler : IRequestHandler<GetMyVehiclesQuery, BaseResponse<IEnumerable<VehicleDto>>>
    {
        private readonly IVehicleReadRepository _repo;

        public GetMyVehiclesQueryHandler(IVehicleReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<VehicleDto>>> Handle(
            GetMyVehiclesQuery query, CancellationToken cancellationToken)
        {
            var vehicles = await _repo.GetByOwnerIdAsync(query.OwnerId);
            return BaseResponse<IEnumerable<VehicleDto>>.Ok(vehicles);
        }
    }

    public class GetVehicleDetailQuery : IRequest<BaseResponse<VehicleDto>>
    {
        public long VehicleId { get; set; }
        public long OwnerId { get; set; }
    }

    public class GetVehicleDetailQueryValidator : AbstractValidator<GetVehicleDetailQuery>
    {
        public GetVehicleDetailQueryValidator()
        {
            RuleFor(x => x.VehicleId).GreaterThan(0);
            RuleFor(x => x.OwnerId).GreaterThan(0);
        }
    }

    public class GetVehicleDetailQueryHandler : IRequestHandler<GetVehicleDetailQuery, BaseResponse<VehicleDto>>
    {
        private readonly IVehicleReadRepository _repo;

        public GetVehicleDetailQueryHandler(IVehicleReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<VehicleDto>> Handle(
            GetVehicleDetailQuery query, CancellationToken cancellationToken)
        {
            var vehicle = await _repo.GetDetailAsync(
                query.VehicleId, query.OwnerId);

            if (vehicle is null)
                throw new KeyNotFoundException("Vehicle not found.");

            return BaseResponse<VehicleDto>.Ok(vehicle);
        }
    }
}
