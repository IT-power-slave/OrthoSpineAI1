using Microsoft.Extensions.Logging;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repo, ILogger<AuthService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<SystemUser?> AuthenticateAsync(string login, string plainPassword, CancellationToken ct = default)
    {
        _logger.LogInformation("Authentication attempt for login: {Login}", login);
        var user = await _repo.GetByLoginAsync(login, ct);
        if (user is null)
        {
            _logger.LogWarning("Authentication failed — unknown login: {Login}", login);
            return null;
        }
        if (!BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash))
        {
            _logger.LogWarning("Authentication failed — wrong password for login: {Login}", login);
            return null;
        }
        _logger.LogInformation("Authentication succeeded for login: {Login} (UserId={UserId})", login, user.SystemUserId);
        return user;
    }
}

