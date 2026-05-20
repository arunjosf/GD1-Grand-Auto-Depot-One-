using GD1.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.Complaints.DTOs;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;

namespace GD1.Application.Features.Complaints.Queries
{
    public class GetComplaintsQuery : IRequest<BaseResponse<IEnumerable<ComplaintDto>>>
    {
        public long? PropertyId { get; set; }   
        public long? ComplainantId { get; set; }   
        public string? Status { get; set; }
    }

    public class GetComplaintsQueryHandler
        : IRequestHandler<GetComplaintsQuery, BaseResponse<IEnumerable<ComplaintDto>>>
    {
        private readonly IGenericRepository<Complaint> _repo;

        public GetComplaintsQueryHandler(IGenericRepository<Complaint> repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<ComplaintDto>>> Handle(
            GetComplaintsQuery query, CancellationToken ct)
        {
            var complaints = await _repo.FindAsync(c => true, "Complainant", "Property");

            if (query.PropertyId.HasValue)
                complaints = complaints.Where(c => c.PropertyId == query.PropertyId);
            
            if (query.ComplainantId.HasValue)
                complaints = complaints.Where(c => c.ComplainantId == query.ComplainantId);
            
            if (!string.IsNullOrEmpty(query.Status))
                complaints = complaints.Where(c => c.Status == query.Status);

            var list = complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                Subject = c.Subject,
                Description = c.Description,
                Status = c.Status,
                AdminResponse = c.AdminResponse,
                ComplainantName = c.Complainant?.FullName ?? "Unknown",
                PropertyName = c.Property?.Name ?? "Unknown",
                CreatedAt = c.CreatedAt
            }).ToList();

            return BaseResponse<IEnumerable<ComplaintDto>>.Ok(list);
        }
    }
}
