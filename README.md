## UserAccountsController

Here's a breakdown of the code:

- **Class Definition**: The `UserAccountsController` class inherits from `ControllerBase` in ASP.NET Core.

- **Static Constructor**: Loads user accounts from `UsrAccounts.json` using `File.ReadAllText` and deserializes JSON into `UserAccount` objects using `JsonSerializer.Deserialize`.

- **HTTP Endpoints**: Defined with attributes like `[HttpPost]` and `[HttpGet]`.

- **[HttpPost("auth")]**: Handles user authentication, comparing credentials and returning an authentication token and user object if successful.

- **[HttpPost("add-money/{username}")]**: Adds 1 unit of money to the specified user's account balance.

- **[HttpPost("deduct-money/{username}")]**: Deducts 1 unit of money from the specified user's account if sufficient funds are available.

- **[HttpPost("send-money")]**: Transfers money between users based on a `MoneyTransferRequest` object in the request body.

- **[HttpPost("save-user-data")]**: Saves updated user account data back to `UsrAccounts.json`.

- **Helper Method**: `IsUserAuthenticated` checks if a user is authenticated based on an authentication token (`authToken`).

- **UserAccount Class**: Represents a user account with properties for username, password, account balance, and account lock status.

- **MoneyTransferRequest Class**: Represents a money transfer request with properties for sender, recipient, and amount.

- **Note**: Some endpoints like `GetUserBalance`, `LockUserAccount`, and `UnlockUserAccount` are commented out and may be non-functional.

## hNFTController

Here's a breakdown of the provided code:

- **Using Statements**: Imports necessary namespaces.

- **Startup Class**: Configures services (`ConfigureServices`) and middleware (`Configure`) for the application.

- **Model Classes**: `User` and `Asset` define structures for user and asset objects.

- **NFTUserController**: Handles HTTP endpoints for user accounts and asset operations.

- **Endpoints**: Includes methods like `AuthenticateAccount`, `ListAssets`, `TransferAsset`, `DeleteAccount`, `CreateAccount`, `UpdateUserEmail`, `AddAssetToUser`, `RemoveAssetFromUser`, `ResetPassword`, `GetUserDetails`, `SaveUserData`, `GetUserById`, and helper methods.

- **Helper Classes**: `CreateUserRequest`, `TransferRequest`, `UserCredentials` as request/response models.

- **JSON Data Storage**: Constructor reads `NFTUsers.json`, deserializes into `User` objects stored in `_users`.

- **HTTP Responses**: Methods return `ActionResult`, `Ok`, `NotFound`, `BadRequest`, `Conflict`, `NoContent` to indicate response status and content.
