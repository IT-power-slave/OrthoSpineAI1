using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Services;

public class AuthService
{
    private readonly IUserRepository _repo;

    public AuthService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<SystemUser?> AuthenticateAsync(string login, string plainPassword, CancellationToken ct = default)
    {
        var user = await _repo.GetByLoginAsync(login, ct);
        if (user is null) return null;
        return BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash) ? user : null;
    }
}
