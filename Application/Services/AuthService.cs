using Application.Interfaces;
using Common.Exceptions;
using Common.Extensions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.RequestModels.AuthRequestModels;
using Domain.Models.ResponseModels;
using Domain.Models.ResponseModels.AuthResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IClientService _clientService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IClientService clientService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _clientService = clientService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequest)
    {
        _logger.LogInformation("Login process started for email: {Email}", loginRequest.Email);

        var user = await GetActiveUserByEmailAsync(loginRequest.Email);

        if (!await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            _logger.LogError("Invalid password for user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid password");
        }

        var client = await _clientService.GetClientByApplicationUserIdAsync(user.Id);
        var token = await _tokenService.GenerateJwtTokenAsync(user, client.ClientId);
        var userResponse = await CreateUserResponseAsync(user, client.ClientId);

        _logger.LogInformation("Login successful for user: {UserId}", user.Id);

        return new LoginResponseModel
        {
            Token = token,
            ExpiresIn = DateTime.Now.AddHours(3),
            User = userResponse
        };
    }

    public async Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel registerRequest)
    {
        _logger.LogInformation("User registration started for email: {Email}", registerRequest.Email);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await CreateOrReactivateUserAsync(registerRequest);
            var clientResponse = await CreateOrUpdateClientAsync(registerRequest, user.Id);
            await _unitOfWork.CommitAsync();

            var token = await _tokenService.GenerateJwtTokenAsync(user, clientResponse.ClientId);
            var userResponse = await CreateUserResponseAsync(user, clientResponse.ClientId);

            _logger.LogInformation("User registration completed. UserId: {UserId}", user.Id);

            return new RegisterResponseModel
            {
                Token = token,
                User = userResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User registration failed for email: {Email}", registerRequest.Email);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<UserProfileResponseModel> GetUserProfileAsync(string userId)
    {
        _logger.LogInformation("User profile retrieval started for UserId: {UserId}", userId);

        var user = await GetActiveUserByIdAsync(userId);
        var clientResponse = await _clientService.GetClientByApplicationUserIdAsync(userId);
        var userResponse = await CreateUserResponseAsync(user, clientResponse.ClientId);

        _logger.LogInformation("User profile retrieved for UserId: {UserId}", userId);

        return new UserProfileResponseModel
        {
            User = userResponse
        };
    }

    public async Task UpdateUserProfileAsync(string currentUserId, int? targetClientId, ClientRequestModel clientRequestModel)
    {
        _logger.LogInformation("Client profile update started. CurrentUserId: {CurrentUserId}", currentUserId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var (client, targetUserId) = await ResolveTargetClientAndUserAsync(currentUserId, targetClientId);
            await ValidateAndUpdateEmailAsync(client.Email, clientRequestModel.Email, clientRequestModel.Name, targetUserId);
            await _clientService.UpdateClient(client.ClientId, clientRequestModel);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Client profile updated. ClientId: {ClientId}", client.ClientId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Client profile update failed. CurrentUserId: {CurrentUserId}", currentUserId);
            throw;
        }
    }

    public async Task DeleteUserProfileAsync(string currentUserId, int? targetClientId = null)
    {
        _logger.LogInformation("User deletion started. CurrentUserId: {CurrentUserId}", currentUserId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var (client, targetUserId) = await ResolveTargetClientAndUserAsync(currentUserId, targetClientId);
            await _clientService.DeleteClient(client.ClientId);
            await SoftDeleteUserAsync(targetUserId);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User deleted successfully. UserId: {UserId}", targetUserId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "User deletion failed. CurrentUserId: {CurrentUserId}", currentUserId);
            throw;
        }
    }

    #region Private Methods

    private async Task<ApplicationUser> GetActiveUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            _logger.LogError("User not found or inactive for email: {Email}", email);
            throw new UnauthorizedAccessException("User not found");
        }
        return user;
    }

    private async Task<ApplicationUser> GetActiveUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            _logger.LogError("User not found or inactive for UserId: {UserId}", userId);
            throw new UnauthorizedAccessException("User not found");
        }
        return user;
    }

    private async Task<ApplicationUser> CreateOrReactivateUserAsync(RegisterRequestModel registerRequest)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);

        if (existingUser != null && existingUser.IsActive)
        {
            _logger.LogError("Attempt to register with active email: {Email}", registerRequest.Email);
            throw new InvalidOperationException("Email already registered");
        }

        if (existingUser != null && !existingUser.IsActive)
        {
            _logger.LogInformation("Re-activating user with email: {Email}", registerRequest.Email);
            return await ReactivateUserAsync(existingUser, registerRequest);
        }

        return await CreateNewUserAsync(registerRequest);
    }

    private async Task<ApplicationUser> ReactivateUserAsync(ApplicationUser user, RegisterRequestModel registerRequest)
    {
        user.IsActive = true;
        user.DeletedAt = null;
        user.FirstName = registerRequest.FirstName;
        user.LastName = registerRequest.LastName;

        await _userManager.RemovePasswordAsync(user);
        var addPasswordResult = await _userManager.AddPasswordAsync(user, registerRequest.Password);

        if (!addPasswordResult.Succeeded)
        {
            var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password update failed: {errors}");
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User reactivation failed: {errors}");
        }

        _logger.LogInformation("User reactivated successfully. UserId: {UserId}", user.Id);
        return user;
    }

    private async Task<ApplicationUser> CreateNewUserAsync(RegisterRequestModel registerRequest)
    {
        var user = new ApplicationUser
        {
            UserName = registerRequest.Email,
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName
        };

        var result = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed for email: {Email}", registerRequest.Email);
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        _logger.LogInformation("New user created successfully. UserId: {UserId}", user.Id);
        return user;
    }

    private async Task<ClientResponseModel> CreateOrUpdateClientAsync(RegisterRequestModel registerRequest, string userId)
    {
        var clientRequest = new ClientRequestModel
        {
            Name = $"{registerRequest.FirstName} {registerRequest.LastName}",
            Email = registerRequest.Email,
            Telephone = registerRequest.Telephone,
            BirthDate = registerRequest.BirthDate
        };

        try
        {
            var existingClient = await _clientService.GetClientByApplicationUserIdAsync(userId);

            _logger.LogInformation("Updating existing client for UserId: {UserId}", userId);
            await _clientService.UpdateClient(existingClient.ClientId, clientRequest, reactivateIfInactive: true);

            return await _clientService.GetClientById(existingClient.ClientId);
        }
        catch (NotFoundException)
        {
            _logger.LogInformation("Creating new client for UserId: {UserId}", userId);
            return await _clientService.CreateClient(clientRequest, userId);
        }
    }

    private async Task<UserResponseModel> CreateUserResponseAsync(ApplicationUser user, int clientId)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponseModel
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList(),
            ClientId = clientId
        };
    }

    private async Task<(ClientResponseModel client, string targetUserId)> ResolveTargetClientAndUserAsync(
        string currentUserId, int? targetClientId)
    {
        var currentUser = await _userManager.FindByIdAsync(currentUserId);
        var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

        if (isAdmin && targetClientId.HasValue)
        {
            var client = await _clientService.GetClientById(targetClientId.Value);
            return (client, client.ApplicationUserId);
        }
        else
        {
            var client = await _clientService.GetClientByApplicationUserIdAsync(currentUserId);
            return (client, currentUserId);
        }
    }

    private async Task ValidateAndUpdateEmailAsync(string currentEmail, string newEmail, string newName, string targetUserId)
    {
        var targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (targetUser == null)
        {
            throw new NotFoundException($"User not found for ID: {targetUserId}");
        }

        targetUser.Email = newEmail;
        targetUser.UserName = newEmail;

        if (!string.IsNullOrEmpty(newName))
            (targetUser.FirstName, targetUser.LastName) = newName.SplitFullName();

        var result = await _userManager.UpdateAsync(targetUser);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Email update failed: {errors}");
        }
    }

    private async Task SoftDeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User not found for ID: {userId}");
        }

        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User deletion failed: {errors}");
        }
    }

    #endregion
}
