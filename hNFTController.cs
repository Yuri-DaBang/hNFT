using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Xml;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string SocialName { get; set; }
    public string SocialDescription { get; set; }
    public string Password { get; set; } // Add Password property

    public bool IsLocked { get; set; }

    public List<Asset> Assets { get; set; }
}

public class Asset
{
    public int AssetId { get; set; }
    public string TokenId { get; set; }
    public string AssetName { get; set; }
    public decimal Value { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class NFTUserController : ControllerBase
{
    private readonly List<User> _users;

    public NFTUserController()
    {
        // Read user data from JSON file
        string jsonFilePath = "NFTUsers.json";
        string jsonData = System.IO.File.ReadAllText(jsonFilePath);
        _users = JsonConvert.DeserializeObject<List<User>>(jsonData);
    }


    [HttpPost("account/auth")]
    public ActionResult<bool> AuthenticateAccount([FromBody] UserCredentials credentials)
    {
        // Check if the user exists in the JSON file
        var user = _users.FirstOrDefault(u => u.Username == credentials.Username && u.Password == credentials.Password);

        if (user == null || user.IsLocked)
            return NotFound();

        return !user.IsLocked;
    }


    [HttpGet("account/assets/{userId}")]
    public ActionResult<IEnumerable<Asset>> ListAssets(int userId)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        return user.Assets;
    }

    [HttpGet("account/social/{userId}")]
    public ActionResult<User> GetSocialInfo(int userId)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        string social_des = user.SocialDescription;

        // Wrap the string within an Ok response
        return Ok(social_des); // Assuming User is your user model
    }


    [HttpPost("account/social/set/{userId}")]
    public IActionResult SetSocialInfo(int userId, [FromBody] User updatedUser)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        user.SocialName = updatedUser.SocialName;
        user.SocialDescription = updatedUser.SocialDescription;

        SaveChangesToJSONFile();

        return NoContent();
    }

    [HttpGet("account/market/{userId}")]
    public ActionResult<decimal> GetMarketInfo(int userId)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        decimal totalSales = user.Assets.Sum(a => a.Value);
        decimal marketCap = _users.Where(u => !u.IsLocked).Sum(u => u.Assets.Sum(a => a.Value));

        return totalSales / marketCap;
    }

    [HttpPost("account/bank/transfer")]
    public IActionResult TransferAsset([FromBody] TransferRequest request)
    {
        var fromUser = GetUserById(request.FromUserId);
        var toUser = GetUserById(request.ToUserId);

        if (fromUser == null || toUser == null || fromUser.IsLocked || toUser.IsLocked)
            return NotFound();

        var asset = fromUser.Assets.FirstOrDefault(a => a.TokenId == request.TokenId);
        if (asset == null)
            return NotFound();

        asset.UserId = toUser.UserId;
        toUser.Assets.Add(asset);
        fromUser.Assets.Remove(asset);

        SaveChangesToJSONFile();

        return NoContent();
    }

    [HttpGet("account/dashboard/{userId}")]
    public IActionResult Dashboard(int userId)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        // Your logic to redirect user to dashboard page
        return Ok("Goto https://web.yuri.server-313.com/orgs/hnft/dev/workspace.html and login with your credientials!!!");
    }

    [HttpPost("account/delete/{userId}")]
    public IActionResult DeleteAccount(int userId)
    {
        var user = GetUserById(userId);
        if (user == null)
            return NotFound();

        user.IsLocked = true;

        SaveChangesToJSONFile();

        return NoContent();
    }
    [HttpPost("account/create")]
    public IActionResult CreateAccount([FromBody] CreateUserRequest request)
    {
        // Validate request
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email))
        {
            return BadRequest("Username and Email are required.");
        }

        // Check if the email is already registered
        if (_users.Any(u => u.Email == request.Email))
        {
            return Conflict("Email is already registered.");
        }

        // Create a new user
        var newUser = new User
        {
            UserId = _users.Count + 1, // Generate new UserId
            Username = request.Username,
            Email = request.Email,
            IsLocked = false,
            Assets = new List<Asset>(),
            SocialName = string.Empty,
            SocialDescription = string.Empty
        };

        // Add the new user to the list
        _users.Add(newUser);

        // Save changes to JSON file
        SaveChangesToJSONFile();

        return Ok(newUser);
    }

    [HttpPost("account/email/update/{userId}")]
    public IActionResult UpdateUserEmail(int userId, [FromBody] string newEmail)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        user.Email = newEmail;

        SaveChangesToJSONFile();

        return Ok(user);
    }

    [HttpPost("account/asset/add/{userId}")]
    public IActionResult AddAssetToUser(int userId, [FromBody] Asset newAsset)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        newAsset.UserId = userId;
        user.Assets.Add(newAsset);

        SaveChangesToJSONFile();

        return Ok(user);
    }

    [HttpDelete("account/asset/remove/{userId}/{assetId}")]
    public IActionResult RemoveAssetFromUser(int userId, int assetId)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        var assetToRemove = user.Assets.FirstOrDefault(a => a.AssetId == assetId);
        if (assetToRemove == null)
            return NotFound();

        user.Assets.Remove(assetToRemove);

        SaveChangesToJSONFile();

        return NoContent();
    }

    [HttpPost("account/password/reset/{userId}")]
    public IActionResult ResetPassword(int userId, [FromBody] string newPassword)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        user.Password = newPassword;

        SaveChangesToJSONFile();

        return Ok(user);
    }

    [HttpGet("account/details/{userId}")]
    public IActionResult GetUserDetails(int userId)
    {
        var user = GetUserById(userId);
        if (user == null || user.IsLocked)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("account/save")]
    public IActionResult SaveUserData([FromBody] User updatedUser)
    {
        var user = GetUserById(updatedUser.UserId);
        if (user == null || user.IsLocked)
            return NotFound();

        // Update user data
        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;
        user.SocialName = updatedUser.SocialName;
        user.SocialDescription = updatedUser.SocialDescription;
        user.Password = updatedUser.Password; // Add if necessary

        SaveChangesToJSONFile();

        return Ok(user);
    }


    private User GetUserById(int userId)
    {
        return _users.FirstOrDefault(u => u.UserId == userId);
    }

    private void SaveChangesToJSONFile()
    {
        // Save changes to JSON file
        string jsonFilePath = "NFTUsers.json";
        string jsonData = JsonConvert.SerializeObject(_users, Newtonsoft.Json.Formatting.Indented);
        System.IO.File.WriteAllText(jsonFilePath, jsonData);
    }
}
public class CreateUserRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
}
public class TransferRequest
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public string TokenId { get; set; }
    public string Description { get; set; }
}

public class UserCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}
