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
				Email = "invalid@example.com",
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
				Email = "existinguser@example.com",
				Password = "Test@123"
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
				Email = "testuser@example.com",
				Password = "Test@123",
				Name = "Test",
				FirstName = "User",
				PhoneNumber = "1234567890",
				isBusiness = false
			};

			// Act
			var response = await _client.PostAsJsonAsync("/auth/register", authModel);

			// Assert
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Contains("User created, please confirm your email", responseContent);
		}

		[Fact]
		public async Task RegisterAsync_ReturnsFailed_WhenUserAlreadyExists()
		{
			// Arrange
			var authModel = new AuthModel
			{
				Email = "existinguser@example.com",
				Password = "Test@123",
				Name = "Existing",
				FirstName = "User",
				PhoneNumber = "1234567890",
				isBusiness = false
			};

			// Register the user for the first time
			await _client.PostAsJsonAsync("/auth/register", authModel);

			// Act
			// Attempt to register the same user again
			var response = await _client.PostAsJsonAsync("/auth/register", authModel);

			// Assert
			var responseContent = await response.Content.ReadAsStringAsync();
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Contains("User already exists", responseContent);
		}
		[Fact]
		public async Task PasswordResetTokenAsync_ReturnsSuccess_WhenPasswordTokenGenerated()
		{
			// Arrange
			string email = "existinguser@example.com"; // Make sure this user exists in the test database

			// Act
			var response = await _client.PostAsync($"/auth/password-reset-token?email={email}", null);

			// Assert
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Contains("Password reset token generated", responseContent);
		}

		[Fact]
		public async Task PasswordResetTokenAsync_ReturnUnauthorized_WhenPasswordTokenFailed()
		{
			// Arrange
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database

			// Act
			var response = await _client.PostAsync($"/auth/password-reset-token?email={email}", null);

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
		[Fact]
		public async Task ChangePasswordAsync_ReturnSuccess_WhenPasswordChanged()
		{
			// Arrange
			string email = "existinguser@example.com"; // Make sure this user exists in the test database
			string oldPassword = "Test@123";
			string newPassword = "NewPassword123!";
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
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Contains("Password changed", responseContent);
		}

		[Fact]
		public async Task ChangePasswordAsync_ReturnUnauthorized_WhenPasswordNotChanged()
		{
			// Arrange
			string email = "existinguser@example.com"; // Make sure this user exists in the test database
			string oldPassword = "Test@123";
			string incorrectPassword = "IncorrectPassword123!";
			string newPassword = "NewPassword123!";
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
			string email = "existinguser@example.com";
			string password = "NewPassword123!";// Make sure this user exists in the test database
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
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

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
			string email = "existinguser@example.com"; // Make sure this user exists in the test database
			string password = "NewPassword123!";// Make sure this user exists in the test database

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
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Contains("Account deleted", responseContent);
		}

		[Fact]
		public async Task DeleteAccountAsync_Unauthorized()
		{
			// Arrange
			string email = "nonexistentuser@example.com"; // Make sure this user does NOT exist in the test database
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
			var response = await _client.PostAsJsonAsync("/auth/delete-account", new { email });

			// Assert
			_outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

	}
}
