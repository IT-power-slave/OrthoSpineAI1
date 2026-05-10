using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Application.Interfaces;

/// <summary>
/// Provides user-authentication logic against the local credential store.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Verifies <paramref name="login"/> and <paramref name="plainPassword"/> against the
    /// stored BCrypt hash and returns the matching <see cref="SystemUser"/>, or
    /// <see langword="null"/> if authentication fails.
    /// </summary>
    Task<SystemUser?> AuthenticateAsync(string login, string plainPassword, CancellationToken ct = default);
}
