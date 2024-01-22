using ChatApplication.Database;
using ChatApplication.Hubs;
using ChatApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Claims;

namespace ChatApplication.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        
        private AppDbContext _ctx { get; }
        public static ObservableCollection<string> ConnectedUsers = new ObservableCollection<string>();

        public HomeController(AppDbContext context)
        {
            _ctx = context;
        }


        public IActionResult Index()
        {
            var chats = _ctx.Chats
                .Include(x => x.Users)
                .Where(
                    x => x.Type == ChatType.Room &&
                    !x.Users.Any(y => y.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
                )
                .ToList();

            return View(chats);
        }


        public IActionResult Find()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Get the list of user IDs with whom the current user has private chats
            var usersInPrivateChats = _ctx.Chats
                .Where(c => c.Type == ChatType.Private && c.Users.Any(u => u.UserId == currentUserId))
                .SelectMany(c => c.Users.Select(u => u.UserId))
                .ToList();

            // Exclude users with whom the current user has private chats
            var users = _ctx.Users
                .Where(x => x.Id != currentUserId && !usersInPrivateChats.Contains(x.Id))
                .ToList();

            return View(users);

        }

        public IActionResult OnlineUsers()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //var onlineUsers = ConnectedUsersHub.GetConnectedUsers();
            var onlineUsers = ConnectedUsers;
            var chats = _ctx.Users
                .Where(x => x.Id != currentUserId && onlineUsers.Contains(x.Id))
                .ToList();
            return View(chats);
        }


        public IActionResult Private()
        {
            var chats = _ctx.Chats
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .Where(x => x.Type == ChatType.Private
                    && x.Users.Any(y => y.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value
                    ))
                .ToList();

            return View(chats);
        }

        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the room already exists between the same users
            var existingChat = _ctx.Chats
                .Where(c => c.Type == ChatType.Private)
                .Include(c => c.Users)
                .Where(c => c.Users.Any(u => u.UserId == userId) && c.Users.Any(u => u.UserId == currentUserId))
                .FirstOrDefault();

            if (existingChat != null)
            {
                // Room already exists, redirect to the existing chat
                return RedirectToAction("Chat", new { id = existingChat.Id });
            }

            // Room doesn't exist, create a new one
            var name = _ctx.Users.Where(x => x.Id == userId).FirstOrDefault()?.UserName;

            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Private
            };

            
            chat.Users.Add(new ChatUser
            {
                UserId = userId
            });

            chat.Users.Add(new ChatUser
            {
                UserId = currentUserId
            });

            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = chat.Id });
        }


        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            var chat = _ctx.Chats
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.Id == id);
            return View(chat);
        }


        [HttpPost]
        public async Task<IActionResult> CreateMessage(int chatId, string message)
        {
            var msg = new Message
            {
                ChatId = chatId,
                Text = message,
                Name = User.Identity.Name,
                TimeStamp = DateTime.Now
            };

            _ctx.Messages.Add(msg);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Chat", new {id = chatId });
        }


        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            // Check if a room with the same name already exists
            var existingRoom = _ctx.Chats
                .Where(c => c.Type == ChatType.Room && c.Name == name)
                .Include(x => x.Users)
                .FirstOrDefault();

            if (existingRoom != null)
            {
                // Room with the same name already exists, join this room
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var existingUser = existingRoom.Users
                    .FirstOrDefault(u => u.UserId == userId);


                if (existingUser == null)
                {
                    existingRoom.Users.Add(new ChatUser
                    {
                        UserId = userId,
                        Role = UserRole.Member
                    });

                    await _ctx.SaveChangesAsync();
                }

                return RedirectToAction("Chat", new { id = existingRoom.Id });
            }

            // Room doesn't exist, create a new one
            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Room,
            };
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Admin
            });

            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = chat.Id });
        }


        [HttpGet]
        public async Task<IActionResult> JoinRoom(int id)
        {
            var chatUser = new ChatUser
            {
                ChatId = id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Member
            };

            _ctx.ChatUsers.Add(chatUser);

            await _ctx.SaveChangesAsync();

            return RedirectToAction("Chat", "Home", new { id = id});
        }

 

    }

}
