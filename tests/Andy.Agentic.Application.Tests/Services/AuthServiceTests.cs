using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Andy.Agentic.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(_userRepository.Object, new HttpContextAccessor());
    }

    [Fact]
    public async Task CreateOrUpdateUserAsync_WhenEmailExistsWithDifferentExternalId_UpdatesExistingUser()
    {
        var existingId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = existingId,
            AzureAdId = "legacy-msal-object-id",
            Email = "user@contoso.com",
            DisplayName = "Old Name",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
        };

        _userRepository
            .Setup(r => r.GetByAzureAdIdAsync("new-gateway-sub"))
            .ReturnsAsync((UserEntity?)null);
        _userRepository
            .Setup(r => r.GetByEmailAsync("user@contoso.com"))
            .ReturnsAsync(existingUser);
        _userRepository
            .Setup(r => r.UpdateAsync(existingUser))
            .Returns(Task.CompletedTask);

        var result = await _authService.CreateOrUpdateUserAsync(
            "new-gateway-sub",
            "user@contoso.com",
            "New Name",
            "New",
            "User");

        result.Id.Should().Be(existingId);
        result.DisplayName.Should().Be("New Name");
        existingUser.AzureAdId.Should().Be("new-gateway-sub");
        _userRepository.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Never);
        _userRepository.Verify(r => r.UpdateAsync(existingUser), Times.Once);
    }
}
