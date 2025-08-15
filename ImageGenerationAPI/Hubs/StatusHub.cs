using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

namespace ImageGenerationAPI.Hubs
{
    public class StatusHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var id = Context.GetHttpContext()?.Request.Query["id"].ToString();
            if (!string.IsNullOrEmpty(id))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, id);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var id = Context.GetHttpContext()?.Request.Query["id"].ToString();
            if (!string.IsNullOrEmpty(id))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
