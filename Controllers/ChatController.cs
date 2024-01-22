using Microsoft.AspNetCore.Mvc;
using ChatApplication.Hubs;
using ChatApplication.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ChatApplication.Models;

namespace ChatApplication.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private IHubContext<ChatHub> _chat;

        public ChatController(IHubContext<ChatHub> chat)
        {
            _chat = chat;
        }


        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> JoinRoom(string connectionId, string roomName)
        {
            await _chat.Groups.AddToGroupAsync(connectionId, roomName);
            return Ok();
        }

        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomName)
        {
            await _chat.Groups.RemoveFromGroupAsync(connectionId, roomName);
            return Ok();
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(
            int chatId,
            string message, 
            string roomName,
            [FromServices] AppDbContext ctx)
        {

            var msg = new Message
            {
                ChatId = chatId,
                Text = message,
                Name = User.Identity.Name,
                TimeStamp = DateTime.Now
            };

            ctx.Messages.Add(msg);
            await ctx.SaveChangesAsync();

            await _chat.Clients.Group(roomName)
                .SendAsync("RecieveMessage", new
                {
                    Text = msg.Text,
                    Name = msg.Name,
                    TimeStamp = msg.TimeStamp.ToString("M/dd/yyyy hh:mm:ss tt")
                });
            return Ok();
        }

    }
}
