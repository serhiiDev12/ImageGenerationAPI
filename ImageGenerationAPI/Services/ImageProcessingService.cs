using ImageGenerationAPI.Config;
using ImageGenerationAPI.Interfaces;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;

namespace ImageGenerationAPI.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly string _workerUrl;
        private readonly HttpClient _httpClient;

        public ImageProcessingService(HttpClient httpClient, IOptions<PythonWorkerOptions> options)
        {
            _workerUrl = options.Value.BaseUrl.TrimEnd('/');
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<Image> ProcessImageAsync(IFormFile image)
        {
            using var stream = new MemoryStream();
            await image.CopyToAsync(stream);
            stream.Position = 0;

            using var original = Image.FromStream(stream);
            return DownscaleUpscale(original);
        }

        public async Task<string> PaintImageAsync(Image image)
        {
            var guid = Guid.NewGuid().ToString();
            await SendImageAsync(image, "dulled_image.jpg", guid);
            return guid; // Return the ID for tracking with SignalR
        }

        private async Task SendImageAsync(Image image, string fileName, string guid)
        {
            using var clone = new Bitmap(image);
            using var stream = new MemoryStream();
            clone.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            using var content = new MultipartFormDataContent();
            var imageContent = new StreamContent(stream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            content.Add(imageContent, "file", fileName);
            content.Add(new StringContent(guid), "id"); // Add the guid here

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_workerUrl}/process-section")
            {
                Content = content
            };

            request.Headers.Add("X-API-KEY", "your-secret-api-key");
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private Image DownscaleUpscale(Image original, int scaleFactor = 2)
        {
            int originalSize = original.Width * original.Height;
            const int FullHdSize = 1920 * 1080;

            if (originalSize <= FullHdSize)
                return (Image)original.Clone();

            int newWidth = original.Width / scaleFactor;
            int newHeight = original.Height / scaleFactor;

            using var downscaled = new Bitmap(original, new Size(newWidth, newHeight));
            var upscaled = new Bitmap(downscaled, original.Size);
            return (Image)upscaled.Clone();
        }
    }
}