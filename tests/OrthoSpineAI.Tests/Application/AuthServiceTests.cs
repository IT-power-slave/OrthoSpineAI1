using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrthoSpineAI.Application.Services;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class AuthServiceTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_repo, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsUser()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("secret");
        var user = new SystemUser { Login = "admin", PasswordHash = hash };
        _repo.GetByLoginAsync("admin", default).Returns(user);

        var result = await _service.AuthenticateAsync("admin", "secret");

        Assert.NotNull(result);
        Assert.Equal("admin", result.Login);
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsNull()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        var user = new SystemUser { Login = "admin", PasswordHash = hash };
        _repo.GetByLoginAsync("admin", default).Returns(user);

        var result = await _service.AuthenticateAsync("admin", "wrong");

        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_UnknownLogin_ReturnsNull()
    {
        _repo.GetByLoginAsync("nobody", default).Returns((SystemUser?)null);

        var result = await _service.AuthenticateAsync("nobody", "any");

        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyPassword_ReturnsNull()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        var user = new SystemUser { Login = "admin", PasswordHash = hash };
        _repo.GetByLoginAsync("admin", default).Returns(user);

        var result = await _service.AuthenticateAsync("admin", string.Empty);

        Assert.Null(result);
    }
}
