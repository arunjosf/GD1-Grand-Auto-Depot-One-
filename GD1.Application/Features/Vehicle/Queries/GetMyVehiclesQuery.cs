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
        public long? Id { get; set; }
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
            var vehicles = await _repo.GetByOwnerIdAsync(query.OwnerId, query.Id);
            return BaseResponse<IEnumerable<VehicleDto>>.Ok(vehicles);
        }
    }
}
