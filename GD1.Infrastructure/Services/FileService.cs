using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GD1.Application.Common.Settings;
using GD1.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _config;
        private Cloudinary? _cloudinary;

        public FileService(IConfiguration config)
        {
            _config = config;
        }

        private Cloudinary GetCloudinary()
        {
            if (_cloudinary != null) return _cloudinary;

            var cloudName = _config["Cloudinary:CloudName"];
            var apiKey = _config["Cloudinary:ApiKey"];
            var apiSecret = _config["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new System.Exception("Cloudinary credentials are missing in appsettings.json. Please add CloudName, ApiKey, and ApiSecret.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            return _cloudinary;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            var client = GetCloudinary();

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"GD1_Auto_Depot/{folder}",
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await client.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new System.Exception($"Cloudinary Upload Error: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
        }
    }
}
