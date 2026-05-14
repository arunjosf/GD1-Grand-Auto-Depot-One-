using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.DTOs
{
    public class VehicleLookupDto
    {
        public string Brand { get; set; } = string.Empty;
        public string? Model { get; set; } 
        public string DisplayName => string.IsNullOrEmpty(Model) ? Brand : $"{Brand} {Model}";
        public string LogoUrl { get; set; } = string.Empty;
    }

}
