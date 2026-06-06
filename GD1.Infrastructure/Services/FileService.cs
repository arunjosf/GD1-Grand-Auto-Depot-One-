using GD1.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GD1.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileService> _logger;
        private static Cloudinary _cloudinary;
        private static readonly object _lock = new object();

        public FileService(
            IConfiguration config,
            ILogger<FileService> logger)
        {
            _config = config;
            _logger = logger;
            
            if (_cloudinary == null)
            {
                lock (_lock)
                {
                    if (_cloudinary == null)
                    {
                        var cloudName = _config["Cloudinary:CloudName"];
                        var apiKey    = _config["Cloudinary:ApiKey"];
                        var apiSecret = _config["Cloudinary:ApiSecret"];

                        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                            throw new InvalidOperationException("Cloudinary credentials are missing in appsettings.json.");

                        var account = new Account(cloudName, apiKey, apiSecret);
                        _cloudinary = new Cloudinary(account);
                    }
                }
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            using var stream = file.OpenReadStream();
            var fileDesc = new FileDescription(file.FileName, stream);

            var isImage = file.ContentType != null && file.ContentType.StartsWith("image/");
            var isVideo = file.ContentType != null && file.ContentType.StartsWith("video/");

            if (isImage)
            {
                var uploadParams = new ImageUploadParams { File = fileDesc, Folder = folder };
                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.Error != null) throw new Exception($"Cloudinary error: {result.Error.Message}");
                return result.SecureUrl?.ToString() ?? string.Empty;
            }
            else if (isVideo)
            {
                var uploadParams = new VideoUploadParams { File = fileDesc, Folder = folder };
                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.Error != null) throw new Exception($"Cloudinary error: {result.Error.Message}");
                return result.SecureUrl?.ToString() ?? string.Empty;
            }
            else
            {
                var uploadParams = new RawUploadParams { File = fileDesc, Folder = folder };
                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.Error != null) throw new Exception($"Cloudinary error: {result.Error.Message}");
                return result.SecureUrl?.ToString() ?? string.Empty;
            }
        }
    }
}