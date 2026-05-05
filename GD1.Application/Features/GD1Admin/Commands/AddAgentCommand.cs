using FluentValidation;
using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class AddAgentCommand : IRequest<BaseResponse<long>>
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string CoverageArea { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class AddAgentCommandValidator : AbstractValidator<AddAgentCommand>
    {
        public AddAgentCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.State).NotEmpty();
        }
    }

    public class AddAgentCommandHandler
        : IRequestHandler<AddAgentCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1Agents> _repo;

        public AddAgentCommandHandler(IGenericRepository<GD1Agents> repo)
            => _repo = repo;

        public async Task<BaseResponse<long>> Handle(
            AddAgentCommand cmd, CancellationToken ct)
        {
            var agent = new GD1Agents
            {
                FullName = cmd.FullName.Trim(),
                PhoneNumber = cmd.PhoneNumber.Trim(),
                Email = cmd.Email?.Trim(),
                City = cmd.City.Trim(),
                State = cmd.State.Trim(),
                CoverageArea = cmd.CoverageArea.Trim(),
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
                IsActive = true
            };

            await _repo.AddAsync(agent);
            return BaseResponse<long>.Ok(agent.Id, "Agent added successfully.");
        }
    }
}
