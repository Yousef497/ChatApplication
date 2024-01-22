using Microsoft.AspNetCore.Identity;

namespace ChatApplication.Models
{
    public class User : IdentityUser
    {
        public ICollection<ChatUser> Chats { get; set; }
    }
}
