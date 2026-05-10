using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Domain.Interfaces;

public interface IUserRepository
{
    Task<SystemUser?> GetByLoginAsync(string login, CancellationToken ct = default);
    Task<SystemUser?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(SystemUser user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
