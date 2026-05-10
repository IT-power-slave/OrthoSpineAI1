using Microsoft.EntityFrameworkCore;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;
using OrthoSpineAI.Infrastructure.Persistence;

namespace OrthoSpineAI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<SystemUser?> GetByLoginAsync(string login, CancellationToken ct = default) =>
        await _db.SystemUsers.FirstOrDefaultAsync(u => u.Login == login, ct);

    public async Task<SystemUser?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.SystemUsers.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(SystemUser user, CancellationToken ct = default) =>
        _db.SystemUsers.Add(user);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
