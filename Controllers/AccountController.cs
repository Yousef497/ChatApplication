using ChatApplication.Hubs;
using ChatApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;

namespace ChatApplication.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IHubContext<ConnectedUsersHub> _connectedUser;


        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IHubContext<ConnectedUsersHub> connectedUser
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _connectedUser = connectedUser;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if(username == null || password == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByNameAsync(username);

            if(user != null)
            {
                var res = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (res.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

            }

            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(string username, string password)
        {
            if (username == null || password == null)
            {
                return RedirectToAction("Register", "Account");
            }

            var user = new User
            {
                UserName = username,
            };
            var res = await _userManager.CreateAsync(user, password);
            if(res.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Register", "Account");
        }


        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            ConnectedUsersHub.Disconnect(true, User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _connectedUser.Clients.All.SendAsync("ReceiveUpdate");
            return RedirectToAction("Index", "Home");
        }
    }
}
