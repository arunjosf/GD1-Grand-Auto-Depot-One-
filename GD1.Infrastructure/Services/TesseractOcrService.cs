using GD1.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Tesseract;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq;
using SkiaSharp; // Added for robust WEBP support

namespace GD1.Infrastructure.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;

        public TesseractOcrService(IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> ExtractText(string url)
        {
            try
            {
                byte[] imageBytes;
                if (string.IsNullOrWhiteSpace(url)) return string.Empty;

                if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var client = _httpClientFactory.CreateClient();
                    imageBytes = await client.GetByteArrayAsync(url);
                }
                else
                {
                    var fullPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, url.TrimStart('/'));
                    if (!File.Exists(fullPath)) return string.Empty;
                    imageBytes = await File.ReadAllBytesAsync(fullPath);
                }

                if (imageBytes == null || imageBytes.Length < 100) return "OCR Error: Invalid image file.";

                // DEBUG: See what we actually downloaded
                var snippet = System.Text.Encoding.UTF8.GetString(imageBytes.Take(50).ToArray());

                // CONVERSION: Use SkiaSharp to convert
                using (var ms = new MemoryStream(imageBytes))
                using (var skBitmap = SKBitmap.Decode(ms))
                {
                    if (skBitmap == null) 
                        return $"OCR Error: Decoding failed. File starts with: '{snippet}'. Size: {imageBytes.Length} bytes.";
                    
                    using (var image = SKImage.FromBitmap(skBitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        imageBytes = data.ToArray();
                    }
                }

                var rootPath = _env.WebRootPath ?? _env.ContentRootPath;
                var tessDataPath = Path.Combine(rootPath, "tessdata");
                
                if (!Directory.Exists(tessDataPath))
                {
                    tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                    if (!Directory.Exists(tessDataPath))
                        return "OCR Error: 'tessdata' folder not found.";
                }

                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);
                
                return page.GetText() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return "OCR Error: " + ex.Message;
            }
        }

        public bool NamesMatch(string idText, string rcText, string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return false;
            
            var cleanId = Normalize(idText);
            var cleanRc = Normalize(rcText);
            var cleanProfile = Normalize(profileName);

            // 1. Check if name is found in either document
            bool foundInId = cleanId.Contains(cleanProfile) || IsVeryFuzzyMatch(cleanId, cleanProfile);
            bool foundInRc = cleanRc.Contains(cleanProfile) || IsVeryFuzzyMatch(cleanRc, cleanProfile);

            // 2. If it's found in BOTH, it's a perfect match
            if (foundInId && foundInRc) return true;

            // 3. Advanced: If it's found in ONE, check the parts of the name in the other
            var nameParts = cleanProfile.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (foundInId || foundInRc)
            {
                // If one is perfect, we only need a "hint" of the name in the other
                return nameParts.Any(part => IsFuzzyMatch(cleanId, part)) && 
                       nameParts.Any(part => IsFuzzyMatch(cleanRc, part));
            }

            return false;
        }

        private bool IsVeryFuzzyMatch(string text, string name)
        {
            // Handles cases like "SHIBIL" being read as "SN S M(D)"
            // by checking if major letters exist in sequence
            if (name.Length < 3) return false;
            int matchCount = 0;
            int lastIndex = -1;

            foreach (char c in name)
            {
                int index = text.IndexOf(c, lastIndex + 1);
                if (index > -1)
                {
                    matchCount++;
                    lastIndex = index;
                }
            }

            return matchCount >= (name.Length * 0.7); // 70% of characters found in order
        }

        private string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return new string(text.ToLower().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray())
                   .Replace("\n", " ").Replace("\r", " ");
        }

        private bool IsFuzzyMatch(string source, string target)
        {
            if (source.Contains(target)) return true;
            if (target.Length <= 2) return source.Contains(target);

            var words = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (LevenshteinDistance(word, target) <= 1) return true;
            }
            return false;
        }

        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length, m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0) return m;
            if (m == 0) return n;
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + ((t[j - 1] == s[i - 1]) ? 0 : 1));
            return d[n, m];
        }
    }
}
