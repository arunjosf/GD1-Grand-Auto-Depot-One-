using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Saves a file (from live camera or folder) and returns its relative URL.
        /// </summary>
        Task<string> SaveFileAsync(IFormFile file, string folder);
    }
}
