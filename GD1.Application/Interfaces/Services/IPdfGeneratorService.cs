namespace GD1.Application.Interfaces.Services
{
    /// <summary>Converts HTML content to a PDF byte array.</summary>
    public interface IPdfGeneratorService
    {
        byte[] GenerateFromHtml(string html);
    }
}
