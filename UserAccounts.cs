using System;

namespace API_web_yuri__CrDa.Controllers
{
    public class UserAccount
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public decimal AccountBalance { get; set; }
        public string Rank { get; set; }
    }
}
