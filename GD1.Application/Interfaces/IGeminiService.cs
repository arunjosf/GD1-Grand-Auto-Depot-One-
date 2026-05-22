using System.Collections.Generic;
using System.Threading.Tasks;
using GD1.Application.Features.GD1Admin.DTOs;

namespace GD1.Application.Interfaces
{
    public interface IGeminiService
    {
        Task<AiRecommendationResponse> GetBestLotRecommendationAsync(List<StoragePropertyListDto> lots, string userPreference);
        Task<ImageReadabilityResponse> VerifyImageReadabilityAsync(string imageUrl, string expectedSubject);
    }

    public class AiRecommendationResponse
    {
        public long BestLotId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string AiAnalysis { get; set; } = string.Empty;
    }

    public class ImageReadabilityResponse
    {
        public bool IsReadable { get; set; }
        public int ConfidenceScore { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
