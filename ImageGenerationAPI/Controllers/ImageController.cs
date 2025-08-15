using ImageGenerationAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Drawing;

namespace ImageGenerationAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class ImageController : Controller
    {
        private readonly IImageProcessingService _imageService;

        public ImageController(IImageProcessingService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("generate-image")]
        public async Task<IActionResult> GenerateImage([FromForm] IFormFile image)
        {
            const string VALID_API_KEY = "your-secret-api-key";

            // Validate API key
            if (!Request.Headers.TryGetValue("X-API-KEY", out var apiKey) || apiKey != VALID_API_KEY)
                return Unauthorized("Invalid API Key");

            if (image == null || image.Length == 0)
                return BadRequest("No image provided.");

            // Downscale and upscale (dulling) before sending to Python
            var dulled = await _imageService.ProcessImageAsync(image);
            var guid = await _imageService.PaintImageAsync(dulled);

            // Return the guid for frontend to track via SignalR
            return Ok(new { id = guid, status = "Processing started" });
        }
    }
}
