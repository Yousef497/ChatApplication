using ChatApplication.Controllers;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApplication.Hubs
{
    public class ConnectedUsersHub : Hub
    {
        

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && !HomeController.ConnectedUsers.Contains(userId))
            {
                HomeController.ConnectedUsers.Add(userId);
                await Clients.All.SendAsync("ReceiveUpdate");
            }

            await base.OnConnectedAsync();
        }

        public static async Task Disconnect(bool disconnect, string id)
        {
            if(disconnect)
            {
                var userId = id;
                if (!string.IsNullOrEmpty(userId) && HomeController.ConnectedUsers.Contains(userId))
                {
                    HomeController.ConnectedUsers.Remove(userId);
                    //await Clients.All.SendAsync("ReceiveUpdate");
                }
            }
        }


        //public override async Task OnDisconnectedAsync(Exception exception)
        //{
        //    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!string.IsNullOrEmpty(userId) && HomeController.ConnectedUsers.Contains(userId))
        //    {
        //        HomeController.ConnectedUsers.Remove(userId);
        //    }

        //    await base.OnDisconnectedAsync(exception);
        //}


    }
}
