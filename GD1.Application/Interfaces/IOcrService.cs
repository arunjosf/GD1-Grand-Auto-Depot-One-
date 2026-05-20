using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtractText(string url);
        bool NamesMatch(string idText, string rcText, string profileName);
        bool IsNamePresent(string text, string profileName);
    }
}
