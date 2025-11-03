using Application.Interfaces;
using Application.Services;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.RequestModels.AuthRequestModels;
using Domain.Models.ResponseModels;
using Domain.Models.ResponseModels.AuthResponseModels;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userManagerMock = CreateUserManagerMock();
            _clientServiceMock = new Mock<IClientService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userManagerMock.Object,
                _clientServiceMock.Object,
                _tokenServiceMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object);
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var loginRequest = new LoginRequestModel
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            var clientResponse = new ClientResponseModel
            {
                ClientId = 1,
                ApplicationUserId = "user123",
                Name = "John Doe",
                Email = "test@example.com",
                Telephone = "123456789"
            };

            var token = "jwt-token-here";
            var userResponse = new UserResponseModel
            {
                Id = "user123",
                Username = "test@example.com",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                ClientId = 1,
                Roles = new List<string> { "Customer" }
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(true);

            _clientServiceMock
                .Setup(x => x.GetClientByApplicationUserIdAsync(user.Id))
                .ReturnsAsync(clientResponse);

            _tokenServiceMock
                .Setup(x => x.GenerateJwtTokenAsync(user, clientResponse.ClientId))
                .ReturnsAsync(token);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(token);
            result.User.Should().BeEquivalentTo(userResponse);
            result.ExpiresIn.Should().BeCloseTo(DateTime.Now.AddHours(3), TimeSpan.FromSeconds(1));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login process started for email")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login successful for user")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequestModel
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.LoginAsync(loginRequest));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found or inactive")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequestModel
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                IsActive = true
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.LoginAsync(loginRequest));

            _loggerMock.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid password for user")),
                   null,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsInactive()
        {
            // Arrange
            var loginRequest = new LoginRequestModel
            {
                Email = "inactive@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "inactive@example.com",
                Email = "inactive@example.com",
                IsActive = false
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.LoginAsync(loginRequest));

            _loggerMock.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found or inactive")),
                   null,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnRegisterResponse_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerRequest = new RegisterRequestModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "newuser@example.com",
                Password = "Password123!",
                Telephone = "123456789",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var capturedUser = new ApplicationUser();
            var clientResponse = new ClientResponseModel
            {
                ClientId = 1,
                ApplicationUserId = "newuser123",
                Name = "John Doe",
                Email = "newuser@example.com",
                Telephone = "987654321"
            };

            var token = "jwt-token-here";

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = "generated-user-id-123";
                    user.UserName = registerRequest.Email;
                    user.Email = registerRequest.Email;
                    user.FirstName = registerRequest.FirstName;
                    user.LastName = registerRequest.LastName;
                    capturedUser = user;
                })
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            _clientServiceMock
                .Setup(x => x.GetClientByApplicationUserIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException("Client not found"));

            _clientServiceMock
                .Setup(x => x.CreateClient(It.IsAny<ClientRequestModel>(), It.IsAny<string>()))
                .ReturnsAsync(clientResponse);

            _tokenServiceMock
                .Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>(), clientResponse.ClientId))
                .ReturnsAsync(token);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "Customer" });

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(token);

            result.User.Should().NotBeNull();
            result.User.Username.Should().Be(registerRequest.Email);
            result.User.Email.Should().Be(registerRequest.Email);
            result.User.FirstName.Should().Be(registerRequest.FirstName);
            result.User.LastName.Should().Be(registerRequest.LastName);
            result.User.ClientId.Should().Be(clientResponse.ClientId);
            result.User.Roles.Should().Contain("Customer");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User registration started")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User registration completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyRegistered()
        {
            // Arrange
            var registerRequest = new RegisterRequestModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com",
                Password = "Password123!",
                Telephone = "123456789",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var existingUser = new ApplicationUser
            {
                Id = "existing123",
                UserName = "existing@example.com",
                Email = "existing@example.com",
                IsActive = true
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync(existingUser);

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.RollbackAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync(registerRequest));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"User registration failed for email: {registerRequest.Email}")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReactivateUser_WhenUserExistsButInactive()
        {
            // Arrange
            var registerRequest = new RegisterRequestModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "inactive@example.com",
                Password = "NewPassword123!",
                Telephone = "123456789",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var inactiveUser = new ApplicationUser
            {
                Id = "inactive123",
                UserName = "inactive@example.com",
                Email = "inactive@example.com",
                FirstName = "Old",
                LastName = "Name",
                IsActive = false
            };

            var clientResponse = new ClientResponseModel
            {
                ClientId = 1,
                ApplicationUserId = "inactive123",
                Name = "John Doe",
                Email = "inactive@example.com",
                Telephone = "123456789"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync(inactiveUser);

            _userManagerMock
                .Setup(x => x.RemovePasswordAsync(inactiveUser))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddPasswordAsync(inactiveUser, registerRequest.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateAsync(inactiveUser))
                .ReturnsAsync(IdentityResult.Success);

            _clientServiceMock
                .Setup(x => x.GetClientByApplicationUserIdAsync(inactiveUser.Id))
                .ReturnsAsync(clientResponse);

            _clientServiceMock
                .Setup(x => x.UpdateClient(clientResponse.ClientId, It.IsAny<ClientRequestModel>(), true))
                .Returns(Task.CompletedTask);

            _clientServiceMock
                .Setup(x => x.GetClientById(clientResponse.ClientId))
                .ReturnsAsync(clientResponse);

            _tokenServiceMock
                .Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>(), clientResponse.ClientId))
                .ReturnsAsync("jwt-token");

            _userManagerMock
                .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "Customer" });

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNull();

            _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Re-activating user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task GetUserProfileAsync_ShouldReturnUserProfile_WhenUserExists()
        {
            // Arrange
            var userId = "user123";
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            var clientResponse = new ClientResponseModel
            {
                ClientId = 1,
                ApplicationUserId = userId,
                Name = "John Doe",
                Email = "test@example.com",
                Telephone = "123456789"
            };

            var userResponse = new UserResponseModel
            {
                Id = userId,
                Username = "test@example.com",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                ClientId = 1,
                Roles = new List<string> { "Customer" }
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _clientServiceMock
                .Setup(x => x.GetClientByApplicationUserIdAsync(userId))
                .ReturnsAsync(clientResponse);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });

            // Act
            var result = await _authService.GetUserProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().BeEquivalentTo(userResponse);

            _loggerMock.Verify(
          x => x.Log(
              LogLevel.Information,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User profile retrieval started")),
              null,
              It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);

            _loggerMock.Verify(
         x => x.Log(
             LogLevel.Information,
             It.IsAny<EventId>(),
             It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User profile retrieved")),
             null,
             It.IsAny<Func<It.IsAnyType, Exception, string>>()),
         Times.Once);
        }

        [Fact]
        public async Task GetUserProfileAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            // Arrange
            var userId = "nonexistent";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.GetUserProfileAsync(userId));

            _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found or inactive")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldUpdateProfile_WhenUserIsAdmin()
        {
            // Arrange
            var currentUserId = "admin123";
            var targetClientId = 1;
            var clientRequest = new ClientRequestModel
            {
                Name = "Updated Name",
                Email = "updated@example.com",
                Telephone = "987654321",
                BirthDate = new DateTime(1985, 1, 1)
            };

            var currentUser = new ApplicationUser
            {
                Id = currentUserId,
                UserName = "admin@example.com",
                Email = "admin@example.com"
            };

            var targetUser = new ApplicationUser
            {
                Id = "target123",
                UserName = "old@example.com",
                Email = "old@example.com",
                FirstName = "Old",
                LastName = "Name"
            };

            var clientResponse = new ClientResponseModel
            {
                ClientId = targetClientId,
                ApplicationUserId = "target123",
                Name = "Old Name",
                Email = "old@example.com",
                Telephone = "123456789"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);

            _userManagerMock
                .Setup(x => x.IsInRoleAsync(currentUser, "Admin"))
                .ReturnsAsync(true);

            _clientServiceMock
                .Setup(x => x.GetClientById(targetClientId))
                .ReturnsAsync(clientResponse);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(clientResponse.ApplicationUserId))
                .ReturnsAsync(targetUser);

            _userManagerMock
                .Setup(x => x.UpdateAsync(targetUser))
                .ReturnsAsync(IdentityResult.Success);

            _clientServiceMock
                .Setup(x => x.UpdateClient(targetClientId, clientRequest, false))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _authService.UpdateUserProfileAsync(currentUserId, targetClientId, clientRequest);

            // Assert
            _clientServiceMock.Verify(x => x.UpdateClient(targetClientId, clientRequest, false), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);

            _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Client profile updated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task DeleteUserProfileAsync_ShouldDeleteUser_WhenUserIsAdmin()
        {
            // Arrange
            var currentUserId = "admin123";
            var targetClientId = 1;

            var currentUser = new ApplicationUser
            {
                Id = currentUserId,
                UserName = "admin@example.com"
            };

            var targetUser = new ApplicationUser
            {
                Id = "target123",
                UserName = "target@example.com",
                IsActive = true
            };

            var clientResponse = new ClientResponseModel
            {
                ClientId = targetClientId,
                ApplicationUserId = "target123",
                Name = "Target User",
                Email = "client1@test.com",
                Telephone = "123456789",
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);

            _userManagerMock
                .Setup(x => x.IsInRoleAsync(currentUser, "Admin"))
                .ReturnsAsync(true);

            _clientServiceMock
                .Setup(x => x.GetClientById(targetClientId))
                .ReturnsAsync(clientResponse);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(clientResponse.ApplicationUserId))
                .ReturnsAsync(targetUser);

            _clientServiceMock
                .Setup(x => x.DeleteClient(targetClientId))
                .Returns(Task.CompletedTask);

            _userManagerMock
                .Setup(x => x.UpdateAsync(targetUser))
                .ReturnsAsync(IdentityResult.Success);

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _authService.DeleteUserProfileAsync(currentUserId, targetClientId);

            // Assert
            _clientServiceMock.Verify(x => x.DeleteClient(targetClientId), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u => u.IsActive == false)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);

            _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User deleted successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }
    }
}
