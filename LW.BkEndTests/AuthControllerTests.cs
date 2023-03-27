using LW.BkEndApi.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Newtonsoft.Json;
using System.Text;
using LW.BkEndApi;
using Xunit.Abstractions;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace LW.BkEndTests
{
	public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<MockStartup>>
	{
		private readonly WebApplicationFactory<MockStartup> _factory;
		private readonly HttpClient _client;
		private readonly ITestOutputHelper _outputHelper;
		private static string _testEmail = "office@topodvlp.website";
		private static string _testPass = "Test@123";
		private static string _testNewPass = "NewPassword123!";
		private static string _testName = "Test";
		private static string _testFirstName = "User";
		private static string _testPhoneNumber = "1234567890";
		private static bool _testIsBusiness = false;

		public AuthControllerIntegrationTests(WebApplicationFactory<MockStartup> factory, ITestOutputHelper outputHelper)
		{
			_factory = factory;
			_client = _factory.CreateClient();
			_outputHelper = outputHelper;
		}

		[Fact]
		public async Task LoginAsync_ReturnsUnauthorized_WhenCredentialsAreInvalid()
		{
			// Arrange
			var loginModel = new AuthModel
			{
				Email = _testEmail,
				Password = "InvalidPassword"
			};

			// Act
			var response = await _client.PostAsJsonAsync("/auth/login", loginModel);

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task LoginAsync_ReturnsSuccess_WhenCredentialsAreValid()
		{
			// Arrange
			var loginModel = new AuthModel
			{
				Email = _testEmail,
				Password = _testPass
			};

			// Act
			var response = await _client.PostAsJsonAsync("/auth/login", loginModel);

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}
		[Fact]
		public async Task RegisterAsync_ReturnsSuccess_WhenUserIsCreated()
		{
			// Arrange
			var authModel = new AuthModel
			{
				Email = _testEmail,
				Password = _testPass,
				Name = _testName,
				FirstName = _testFirstName,
				PhoneNumber = _testPhoneNumber,
				isBusiness = _testIsBusiness
			};

			// Act
			var response = await _client.PostAsJsonAsync("/auth/register", authModel);

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("User created, please confirm your email", responseContent);
		}

		[Fact]
		public async Task RegisterAsync_ReturnsFailed_WhenUserAlreadyExists()
		{
			// Arrange
			var authModel = new AuthModel
			{
				Email = "sa@sa.com",
				Password = _testPass,
				Name = _testName,
				FirstName = _testFirstName,
				PhoneNumber = _testPhoneNumber,
				isBusiness = _testIsBusiness
			};

			// Register the user for the first time
			await _client.PostAsJsonAsync("/auth/register", authModel);

			// Act
			// Attempt to register the same user again
			var response = await _client.PostAsJsonAsync("/auth/register", authModel);

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("User already exists", responseContent);
		}
		[Fact]
		public async Task PasswordResetTokenAsync_ReturnsSuccess_WhenPasswordTokenGenerated()
		{
			// Arrange
			string email = _testEmail; // Make sure this user exists in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/password-reset-token?email={email}", new { });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Password reset token generated", responseContent);
		}

		[Fact]
		public async Task PasswordResetTokenAsync_ReturnUnauthorized_WhenPasswordTokenFailed()
		{
			// Arrange
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/password-reset-token?email={email}", new { });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task ChangePasswordAsync_ReturnSuccess_WhenPasswordChanged()
		{
			// Arrange
			string email = _testEmail; // Make sure this user exists in the test database
			string oldPassword = _testPass;
			string newPassword = _testNewPass;
			var loginModel = new AuthModel
			{
				Email = email,
				Password = oldPassword
			};

			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/change-password", new { email, password = oldPassword, newPassword });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Password changed", responseContent);
		}
		[Fact]
		public async Task ChangePasswordAsync_ReturnSuccess_WhenPasswordChanged2()
		{
			// Arrange
			string email = _testEmail; // Make sure this user exists in the test database
			string oldPassword = _testNewPass;
			string newPassword = _testPass;
			var loginModel = new AuthModel
			{
				Email = email,
				Password = oldPassword
			};

			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/change-password", new { email, password = oldPassword, newPassword });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Password changed", responseContent);
		}
		[Fact]
		public async Task ChangePasswordAsync_ReturnUnauthorized_WhenPasswordNotChanged()
		{
			// Arrange
			string email = _testEmail; // Make sure this user exists in the test database
			string oldPassword = _testPass;
			string incorrectPassword = "IncorrectPassword123!";
			string newPassword = _testNewPass;
			var loginModel = new AuthModel
			{
				Email = email,
				Password = oldPassword
			};

			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/change-password", new { email, password = incorrectPassword, newPassword });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task UpdateProfileAsync_Success()
		{
			// Arrange
			string email = _testEmail;
			string password = _testPass;// Make sure this user exists in the test database
			string name = "Updated Name";
			string firstName = "Updated First Name";
			string phoneNumber = "123-456-7890";
			var loginModel = new AuthModel
			{
				Email = email,
				Password = password,
			};
			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/update-profile", new { email, name, firstName, phoneNumber });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Profile updated", responseContent);
		}

		[Fact]
		public async Task UpdateProfileAsync_Unauthorized()
		{
			// Arrange
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database
			string name = "Updated Name";
			string firstName = "Updated First Name";
			string phoneNumber = "123-456-7890";
			string anotherEmail = "existinguser@example.com";
			string password = "NewPassword123!";// Make sure this user exists in the test database
			var loginModel = new AuthModel
			{
				Email = anotherEmail,
				Password = password,
			};
			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/update-profile", new { email, name, firstName, phoneNumber });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task DeleteAccountAsync_Success()
		{
			// Arrange
			string email = _testEmail; // Make sure this user exists in the test database
			string password = _testPass;// Make sure this user exists in the test database

			var loginModel = new AuthModel
			{
				Email = email,
				Password = password,
			};
			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/delete-account", new { email });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Account deleted", responseContent);
		}

		[Fact]
		public async Task DeleteAccountAsync_Unauthorized()
		{
			// Arrange
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database
			string anotherEmail = _testEmail;
			string password = _testPass;// Make sure this user exists in the test database
			var loginModel = new AuthModel
			{
				Email = anotherEmail,
				Password = password,
			};
			// Act
			var responseToken = await _client.PostAsJsonAsync("/auth/login", loginModel);
			var token = JsonConvert.DeserializeObject<JObject>(await responseToken.Content.ReadAsStringAsync());

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token["token"]);
			var response = await _client.PostAsJsonAsync("/auth/delete-account", new { email });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task PasswordResetAsync_Success()
		{
			// Arrange
			string resetPasswordToken = "Q2ZESjhGeTVMemZEb1NCT2dtSmw5ZHV3cURKMjlKa0w0RmQ3QnVtK3BHTnpyOHR3RTB2eW12bk16elRzM0ttQm1FRGZaU1pHaFI0V1ZRQ0svRldpeHQ1VldIZkQ5cFRGUDdYemliMlJUYUh5ZktMV1l6VUtjMWlBcDVWd2crbU9BZ1BqdEY2cGs3eFJ3a0pEanl1d2kxdnFjU0ljeHlhRnl2V245NDIzdkZvRzMwbElXY3I2WmZXTG5pd0swMEtRY05IWDhwUlJNNlFQZ0VodW96RFVXN2c0R3FHdVU4K21JQkVydmRjQVNrYjRHdVNWR1ppZzVaZFM2OUZzMDlQWFdTTUF0QT09";
			string email = _testEmail; // Make sure this user exists in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/password-reset?resetPasswordToken={resetPasswordToken}", new { email, password = _testPass });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Password has been reseted", responseContent);
		}

		[Fact]
		public async Task PasswordResetAsync_Unauthorized()
		{
			// Arrange
			string resetPasswordToken = "your_invalid_reset_password_token_here"; // Replace with an invalid reset password token
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/password-reset?resetPasswordToken={resetPasswordToken}", new { email, password = "NewPassword123!" });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task EmailConfirmationAsync_Success()
		{
			// Arrange
			string emailConfirmationToken = "Q2ZESjhGeTVMemZEb1NCT2dtSmw5ZHV3cURKVFM2eWs5YloyNGZjd0hoVStGUVhhNUVNbWlLZHR0YVBFMHYxZSs0NkZyRWNNU1RPbld0YnNDWnduR0h2L1JDSkUvcHAxL3c1T3QvVUNpc0VkVHFJbXFFS0l6YUNlcVd6cXNpaFJuTitHVkd1aVdiTlp4RG5HNU9kRklBWEJvTWJXWUtTQytsdUFJV3Q0Q1BBU0tsMUk1eXludUdBRG1pWG5TZmVXUEJrcEVobVdMWkxRMmtDSGs1UVo0UDVsWHUwUWRQMllBamYyd2V2TjFmd3pLZDlKclhlbnpWOXZxeUY4TjJIKy9Pakx2dz09";
			string email = _testEmail; // Make sure this user exists in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/confirm-email?emailConfirmationToken={emailConfirmationToken}&email={email}", new { });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Email has been confirmed", responseContent);
		}

		[Fact]
		public async Task EmailConfirmationAsync_Unauthorized()
		{
			// Arrange
			string emailConfirmationToken = "your_invalid_email_confirmation_token_here"; // Replace with an invalid email confirmation token
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/confirm-email?emailConfirmationToken={emailConfirmationToken}&email={email}", new { });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task ResendEmailConfirmationAsync_Success()
		{
			// Arrange
			string email = _testEmail; // Make sure this user exists in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/resend-confirmation-email?email={email}", new { });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.Contains("Email confirmation token generated", responseContent);
		}

		[Fact]
		public async Task ResendEmailConfirmationAsync_Unauthorized()
		{
			// Arrange
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database

			// Act
			var response = await _client.PostAsJsonAsync($"/auth/resend-confirmation-email?email={email}", new { });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

	}
}
