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

namespace GD1.Application.Features.Complaints.Commands
{
    public class SubmitComplaintCommand : IRequest<BaseResponse<long>>
    {
        public long ComplainantId { get; set; }
        public long PropertyId { get; set; }
        public long? BookingId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SubmitComplaintCommandValidator
        : AbstractValidator<SubmitComplaintCommand>
    {
        public SubmitComplaintCommandValidator()
        {
            RuleFor(x => x.PropertyId).GreaterThan(0);
            RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        }
    }

    public class SubmitComplaintCommandHandler
        : IRequestHandler<SubmitComplaintCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<Complaint> _repo;

        public SubmitComplaintCommandHandler(IGenericRepository<Complaint> repo)
            => _repo = repo;

        public async Task<BaseResponse<long>> Handle(
            SubmitComplaintCommand cmd, CancellationToken ct)
        {
            var complaint = new Complaint
            {
                ComplainantId = cmd.ComplainantId,
                PropertyId = cmd.PropertyId,
                BookingId = cmd.BookingId,
                Subject = cmd.Subject.Trim(),
                Description = cmd.Description.Trim(),
                Status = "Open"
            };

            await _repo.AddAsync(complaint);
            return BaseResponse<long>.Ok(complaint.Id, "Complaint submitted.");
        }
    }
}
