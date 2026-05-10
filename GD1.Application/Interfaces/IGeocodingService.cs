using System;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IGeocodingService
    {
        Task<(double Lat, double Lon)?> GetCoordinatesAsync(string address);
    }
}
