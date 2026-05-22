using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllServiceCenterApplicationsQuery : IRequest<BaseResponse<IEnumerable<AdminServiceCenterApplicationDto>>>
    {
        public long? Id { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool Descending { get; set; } = true;
    }

    public class GetAllServiceCenterApplicationsQueryHandler : IRequestHandler<GetAllServiceCenterApplicationsQuery, BaseResponse<IEnumerable<AdminServiceCenterApplicationDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _scRepo;

        public GetAllServiceCenterApplicationsQueryHandler(IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> scRepo)
        {
            _scRepo = scRepo;
        }

        public async Task<BaseResponse<IEnumerable<AdminServiceCenterApplicationDto>>> Handle(GetAllServiceCenterApplicationsQuery query, CancellationToken ct)
        {
            var allApps = await _scRepo.FindAsync(x => true, "Images");

            var queryable = allApps.AsQueryable();

            if (query.Id.HasValue)
            {
                queryable = queryable.Where(a => a.Id == query.Id.Value);
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                queryable = queryable.Where(a => a.Status.Equals(query.Status, System.StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                queryable = queryable.Where(a =>
                    a.Name.ToLower().Contains(term) ||
                    (a.SupportedBrand != null && a.SupportedBrand.ToLower().Contains(term)) ||
                    a.OwnerName.ToLower().Contains(term) ||
                    a.City.ToLower().Contains(term)
                );
            }

            if (query.Descending)
            {
                queryable = query.SortBy switch
                {
                    "Name" => queryable.OrderByDescending(a => a.Name),
                    _ => queryable.OrderByDescending(a => a.CreatedAt)
                };
            }
            else
            {
                queryable = query.SortBy switch
                {
                    "Name" => queryable.OrderBy(a => a.Name),
                    _ => queryable.OrderBy(a => a.CreatedAt)
                };
            }

            var resultDtos = queryable.AsEnumerable().Select(sc => 
            {
                string brandVerifyUrl = "";
                string googleMapUrl = "";

                if (!string.IsNullOrEmpty(sc.SupportedBrand))
                {
                    bool isIndia = string.Equals(sc.Country, "India", System.StringComparison.OrdinalIgnoreCase);

                    var brandMapUS = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
                    {
                        { "Ford", "https://www.ford.com/dealerships/" },
                        { "Toyota", "https://www.toyota.com/dealers/search" },
                        { "Nissan", "https://www.nissanusa.com/dealer-locator.html" },
                        { "Hyundai", "https://www.hyundaiusa.com/us/en/dealer-locator" },
                        { "Kia", "https://www.kia.com/us/en/dealer-locator" }
                    };

                    var brandMapIndia = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
                    {
                        { "Ford", "https://www.india.ford.com/locate-a-dealer/" },
                        { "Toyota", "https://www.toyotabharat.com/find-a-dealer/" },
                        { "Nissan", "https://www.nissan.in/dealer-locator.html" },
                        { "Hyundai", "https://www.hyundai.com/in/en/find-a-dealer" },
                        { "Kia", "https://www.kia.com/in/buy/find-a-dealer.html" }
                    };

                    var activeMap = isIndia ? brandMapIndia : brandMapUS;

                    if (activeMap.TryGetValue(sc.SupportedBrand, out var template))
                    {
                        brandVerifyUrl = template;
                    }
                    
                    var zipString = !string.IsNullOrEmpty(sc.PostalCode) ? $"+{sc.PostalCode}" : "";
                    googleMapUrl = $"https://www.google.com/maps/search/{sc.SupportedBrand}+service+center{zipString}";
                }

                return new AdminServiceCenterApplicationDto
                {
                    Id = sc.Id,
                    AdminId = sc.ApplicantId,
                    Name = sc.Name,
                    OwnerName = sc.OwnerName,
                    PhoneNumber = sc.PhoneNumber,
                    Email = sc.Email,
                    AddressLine = sc.AddressLine,
                    City = sc.City,
                    District = sc.District,
                    State = sc.State,
                    Country = sc.Country,
                    PostalCode = sc.PostalCode,
                    Latitude = sc.Latitude ?? 0,
                    Longitude = sc.Longitude ?? 0,
                    Status = sc.Status,
                    IsVerified = false,
                    AdminNotes = sc.AdminNotes,
                    CreatedAt = sc.CreatedAt,
                    OemCertificateUrl = sc.OemCertificateUrl,
                    SupportedBrand = sc.SupportedBrand,
                    OwnerIdProofUrl = sc.OwnerIdProofUrl,
                    BrandVerifyUrl = brandVerifyUrl,
                    GoogleMapVerifyUrl = googleMapUrl,
                    Images = sc.Images?.Select(i => i.ImageUrl).ToList() ?? new System.Collections.Generic.List<string>()
                };
            }).ToList();

            return BaseResponse<IEnumerable<AdminServiceCenterApplicationDto>>.Ok(resultDtos);
        }
    }
}
