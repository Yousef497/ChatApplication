using ChatApplication.Database;
using ChatApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApplication.ViewComponents
{
    public class RoomViewComponent : ViewComponent
    {
        private AppDbContext _ctx;

        public RoomViewComponent(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public IViewComponentResult Invoke()
        {

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var chats = _ctx.ChatUsers
                .Include(x => x.Chat)
                .Where(x => x.UserId == userId && x.Chat.Type == ChatType.Room)
                .Select(x => x.Chat)
                .ToList();
            return View(chats);
        }
    }
}
