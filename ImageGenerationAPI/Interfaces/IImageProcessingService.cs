using System.Drawing;

namespace ImageGenerationAPI.Interfaces
{
    public interface IImageProcessingService
    {
        Task<Image> ProcessImageAsync(IFormFile image);
        Task<string> PaintImageAsync(Image image);
    }
}
