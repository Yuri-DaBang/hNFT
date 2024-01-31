using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace API_web_yuri__CrDa.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserAccountsController : ControllerBase
    {
        private static List<UserAccount> userAccounts;
        private static string authToken; // Simple authentication token for demonstration

        static UserAccountsController()
        {
            // Load user accounts from the JSON file
            string jsonContent = System.IO.File.ReadAllText("./UsrAccounts.json");
            userAccounts = JsonSerializer.Deserialize<List<UserAccount>>(jsonContent);
        }

        // Authentication with username and password
        [HttpPost("auth")]
        public IActionResult Authenticate([FromBody] UserAccount credentials)
        {
            var user = userAccounts.FirstOrDefault(u => u.Username == credentials.Username && u.Password == credentials.Password);

            if (user == null)
                Thread.Sleep(1000); return NotFound("Invalid username or password");

            if (user.IsLocked)
                Thread.Sleep(1000); return BadRequest("User account is locked");

            // Generate and store a simple authentication token (replace this with a proper authentication mechanism)
            authToken = Guid.NewGuid().ToString();

            Thread.Sleep(1000); return Ok(new { Token = authToken, User = user });
        }

        // Add money by +1
        [HttpPost("add-money/{username}")]
        public IActionResult AddMoney(string username)
        {
            if (!IsUserAuthenticated())
                Thread.Sleep(1000); return Unauthorized("User not authenticated");

            var user = userAccounts.FirstOrDefault(u => u.Username == username);

            if (user == null)
                Thread.Sleep(1000); return NotFound("User not found");

            if (user.IsLocked)
                Thread.Sleep(1000); return BadRequest("User account is locked");

            user.AccountBalance += 1;
            Thread.Sleep(1000); return Ok(user);
        }

        // Deduct money by -1
        [HttpPost("deduct-money/{username}")]
        public IActionResult DeductMoney(string username)
        {
            if (!IsUserAuthenticated())
                Thread.Sleep(1000); return Unauthorized("User not authenticated");

            var user = userAccounts.FirstOrDefault(u => u.Username == username);

            if (user == null)
                Thread.Sleep(1000); return NotFound("User not found");

            if (user.IsLocked)
                Thread.Sleep(1000); return BadRequest("User account is locked");

            if (user.AccountBalance < 1)
            {
                user.AccountBalance = 0;
                Thread.Sleep(1000); return BadRequest("Insufficient funds");
            }

            user.AccountBalance -= 1;
            Thread.Sleep(1000); return Ok(user);
        }

        // Send money to another user
        [HttpPost("send-money")]
        public IActionResult SendMoney([FromBody] MoneyTransferRequest transferRequest)
        {
            if (!IsUserAuthenticated())
                Thread.Sleep(1000); return Unauthorized("User not authenticated");

            var fromUser = userAccounts.FirstOrDefault(u => u.Username == transferRequest.From);
            var toUser = userAccounts.FirstOrDefault(u => u.Username == transferRequest.To);

            if (fromUser == null || toUser == null)
                Thread.Sleep(1000); return NotFound("User not found");

            if (fromUser.AccountBalance < transferRequest.Amount)
                Thread.Sleep(1000); return BadRequest("Insufficient funds");

            fromUser.AccountBalance -= transferRequest.Amount;
            toUser.AccountBalance += transferRequest.Amount;

            Thread.Sleep(1000); return Ok(new { FromUser = fromUser, ToUser = toUser });
        }

        // Save user data
        [HttpPost("save-user-data")]
        public IActionResult SaveUserData()
        {
            if (!IsUserAuthenticated())
                Thread.Sleep(1000); return Unauthorized("User not authenticated");

            string jsonContent = JsonSerializer.Serialize(userAccounts);
            System.IO.File.WriteAllText("./UsrAccounts.json", jsonContent);
            Thread.Sleep(1000); return Ok("User data saved");
        }

        // Get user balance
        //[HttpGet("get-user-balance/{username}")]
        //public IActionResult GetUserBalance(string username)
        //{
        //    if (!IsUserAuthenticated())
        //        Thread.Sleep(1000); return Unauthorized("User not authenticated");

        //    var user = userAccounts.FirstOrDefault(u => u.Username == username);

        //    if (user == null)
        //        Thread.Sleep(1000); return NotFound("User not found");

        //    Thread.Sleep(1000); return Ok(new { Username = user.Username, Balance = user.AccountBalance });
        //}

        //// Lock user account
        //[HttpPost("lock-user-account/{username}")]
        //public IActionResult LockUserAccount(string username)
        //{
        //    if (!IsUserAuthenticated())
        //        Thread.Sleep(1000); return Unauthorized("User not authenticated");

        //    var user = userAccounts.FirstOrDefault(u => u.Username == username);

        //    if (user == null)
        //        Thread.Sleep(1000); return NotFound("User not found");

        //    user.IsLocked = true;
        //    Thread.Sleep(1000); return Ok(new { Username = user.Username, IsLocked = user.IsLocked });
        //}

        //// Unlock user account
        //[HttpPost("unlock-user-account/{username}")]
        //public IActionResult UnlockUserAccount(string username)
        //{
        //    //if (!IsUserAuthenticated())
        //    //    Thread.Sleep(1000); return Unauthorized("User not authenticated");

        //    var user = userAccounts.FirstOrDefault(u => u.Username == username);

        //    if (user == null)
        //        Thread.Sleep(1000); return NotFound("User not found");

        //    user.IsLocked = false;
        //    Thread.Sleep(1000); return Ok(new { Username = user.Username, IsLocked = user.IsLocked });
        //}

        private bool IsUserAuthenticated()
        {
            // Replace this check with your actual authentication logic
            Thread.Sleep(1000); return !string.IsNullOrEmpty(authToken);
        }

        public class UserAccount
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public decimal AccountBalance { get; set; }
            public bool IsLocked { get; set; }
        }

        public class MoneyTransferRequest
        {
            public string From { get; set; }
            public string To { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
