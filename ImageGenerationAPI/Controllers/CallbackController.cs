using ImageGenerationAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ImageGenerationAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class CallbackController : Controller
    {
        private readonly IHubContext<StatusHub> _hubContext;

        public CallbackController(IHubContext<StatusHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("status-callback")]
        public async Task<IActionResult> ReceiveStatus([FromBody] StatusUpdate update)
        {
            if (update == null || string.IsNullOrWhiteSpace(update.Id) || string.IsNullOrWhiteSpace(update.Status))
                return BadRequest("Invalid status update");

            await _hubContext.Clients.Group(update.Id).SendAsync("StatusUpdate", update.Status);
            return Ok();
        }

        [HttpPost("callback")]
        public async Task<IActionResult> ReceiveProcessedImage([FromForm] IFormFile file, [FromForm] string id)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(id))
                return BadRequest("Invalid image callback");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var imageBytes = stream.ToArray();
            var base64Image = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";

            // Broadcast the image to the client via SignalR
            await _hubContext.Clients.Group(id).SendAsync("ImageReceived", base64Image);

            return Ok(new { message = "Image received and sent to SignalR clients", id });
        }

        public class StatusUpdate
        {
            public string Id { get; set; }
            public string Status { get; set; }
        }
    }
}
