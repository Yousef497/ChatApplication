using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {

        public string GetConnectionId() => Context.ConnectionId;

        

    }
}
