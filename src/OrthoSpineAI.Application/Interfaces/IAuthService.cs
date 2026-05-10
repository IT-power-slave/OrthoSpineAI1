using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Application.Interfaces;

public interface IAuthService
{
    Task<SystemUser?> AuthenticateAsync(string login, string plainPassword, CancellationToken ct = default);
}
