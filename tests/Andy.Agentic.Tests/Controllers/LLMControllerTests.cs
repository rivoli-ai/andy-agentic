using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Controllers;
using Andy.Agentic.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Andy.Agentic.Tests.Controllers;

public class LLMControllerTests
{
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<ILogger<LLMController>> _mockLogger;
    private readonly LLMController _controller;

    public LLMControllerTests()
    {
        _mockLlmService = new Mock<ILlmService>();
        _mockLogger = new Mock<ILogger<LLMController>>();
        _controller = new LLMController(_mockLlmService.Object, _mockLogger.Object);
        
        // Setup controller context with user claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "test@example.com"),
            new(ClaimTypes.Role, "Write")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task GetAllLlmConfigs_ShouldReturnAllConfigs()
    {
        // Arrange
        var expectedConfigs = new List<LLMConfig>
        {
            new() { Id = Guid.NewGuid(), Name = "Config 1", Provider = LLMProviderType.OpenAi, ApiKey = "key1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Config 2", Provider = LLMProviderType.Azure, ApiKey = "key2", IsActive = true }
        };

        _mockLlmService
            .Setup(x => x.GetAllLlmConfigsAsync())
            .ReturnsAsync(expectedConfigs);

        // Act
        var result = await _controller.GetAllLlmConfigs();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedConfigs);
        _mockLlmService.Verify(x => x.GetAllLlmConfigsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetLlmConfig_WithValidId_ShouldReturnConfig()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var expectedConfig = new LLMConfig
        {
            Id = configId,
            Name = "Test Config",
            Provider = LLMProviderType.OpenAi,
            ApiKey = "test-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true
        };

        _mockLlmService
            .Setup(x => x.GetLlmConfigByIdAsync(configId))
            .ReturnsAsync(expectedConfig);

        // Act
        var result = await _controller.GetLlmConfig(configId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedConfig);
        _mockLlmService.Verify(x => x.GetLlmConfigByIdAsync(configId), Times.Once);
    }

    [Fact]
    public async Task GetLlmConfig_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var configId = Guid.NewGuid();

        _mockLlmService
            .Setup(x => x.GetLlmConfigByIdAsync(configId))
            .ReturnsAsync((LLMConfig?)null);

        // Act
        var result = await _controller.GetLlmConfig(configId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockLlmService.Verify(x => x.GetLlmConfigByIdAsync(configId), Times.Once);
    }

    [Fact]
    public async Task CreateLlmConfig_WithValidConfig_ShouldReturnCreatedConfig()
    {
        // Arrange
        var createConfig = new LLMConfig
        {
            Name = "New Config",
            Provider = LLMProviderType.OpenAi,
            ApiKey = "new-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true
        };

        var expectedConfig = new LLMConfig
        {
            Id = Guid.NewGuid(),
            Name = createConfig.Name,
            Provider = createConfig.Provider,
            ApiKey = createConfig.ApiKey,
            BaseUrl = createConfig.BaseUrl,
            IsActive = createConfig.IsActive
        };

        _mockLlmService
            .Setup(x => x.CreateLlmConfigAsync(createConfig))
            .ReturnsAsync(expectedConfig);

        // Act
        var result = await _controller.CreateLlmConfig(createConfig);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedConfig);
        _mockLlmService.Verify(x => x.CreateLlmConfigAsync(createConfig), Times.Once);
    }

    [Fact]
    public async Task UpdateLlmConfig_WithValidConfig_ShouldReturnUpdatedConfig()
    {
        // Arrange
        var updateConfig = new LLMConfig
        {
            Id = Guid.NewGuid(),
            Name = "Updated Config",
            Provider = LLMProviderType.Azure,
            ApiKey = "updated-key",
            BaseUrl = "https://updated-api.azure.com",
            IsActive = true
        };

        _mockLlmService
            .Setup(x => x.UpdateLlmConfigAsync(updateConfig))
            .ReturnsAsync(updateConfig);

        // Act
        var result = await _controller.UpdateLlmConfig(updateConfig.Id, updateConfig);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updateConfig);
        _mockLlmService.Verify(x => x.UpdateLlmConfigAsync(updateConfig), Times.Once);
    }

    [Fact]
    public async Task UpdateLlmConfig_WithMismatchedId_ShouldReturnBadRequest()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var updateConfig = new LLMConfig
        {
            Id = Guid.NewGuid(), // Different ID
            Name = "Updated Config",
            Provider = LLMProviderType.OpenAi
        };

        // Act
        var result = await _controller.UpdateLlmConfig(configId, updateConfig);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockLlmService.Verify(x => x.UpdateLlmConfigAsync(It.IsAny<LLMConfig>()), Times.Never);
    }

    [Fact]
    public async Task DeleteLlmConfig_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var configId = Guid.NewGuid();

        _mockLlmService
            .Setup(x => x.DeleteLlmConfigAsync(configId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteLlmConfig(configId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockLlmService.Verify(x => x.DeleteLlmConfigAsync(configId), Times.Once);
    }

    [Fact]
    public async Task DeleteLlmConfig_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var configId = Guid.NewGuid();

        _mockLlmService
            .Setup(x => x.DeleteLlmConfigAsync(configId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteLlmConfig(configId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockLlmService.Verify(x => x.DeleteLlmConfigAsync(configId), Times.Once);
    }

    [Fact]
    public async Task GetActiveLlmConfigs_ShouldReturnOnlyActiveConfigs()
    {
        // Arrange
        var expectedConfigs = new List<LLMConfig>
        {
            new() { Id = Guid.NewGuid(), Name = "Active Config 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Active Config 2", IsActive = true }
        };

        _mockLlmService
            .Setup(x => x.GetActiveLlmConfigsAsync())
            .ReturnsAsync(expectedConfigs);

        // Act
        var result = await _controller.GetActiveLlmConfigs();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedConfigs);
        _mockLlmService.Verify(x => x.GetActiveLlmConfigsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetLlmConfigsByProvider_WithValidProvider_ShouldReturnConfigsOfProvider()
    {
        // Arrange
        var provider = LLMProviderType.OpenAi;
        var expectedConfigs = new List<LLMConfig>
        {
            new() { Id = Guid.NewGuid(), Name = "OpenAI Config 1", Provider = LLMProviderType.OpenAi },
            new() { Id = Guid.NewGuid(), Name = "OpenAI Config 2", Provider = LLMProviderType.OpenAi }
        };

        _mockLlmService
            .Setup(x => x.GetLlmConfigsByProviderAsync(provider))
            .ReturnsAsync(expectedConfigs);

        // Act
        var result = await _controller.GetLlmConfigsByProvider(provider);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedConfigs);
        _mockLlmService.Verify(x => x.GetLlmConfigsByProviderAsync(provider), Times.Once);
    }

    [Fact]
    public async Task TestConnection_WithValidConfigId_ShouldReturnSuccess()
    {
        // Arrange
        var configId = Guid.NewGuid();

        _mockLlmService
            .Setup(x => x.TestConnectionAsync(configId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestConnection(configId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
        _mockLlmService.Verify(x => x.TestConnectionAsync(configId), Times.Once);
    }

    [Fact]
    public async Task TestConnection_WithInvalidConfigId_ShouldReturnFailure()
    {
        // Arrange
        var configId = Guid.NewGuid();

        _mockLlmService
            .Setup(x => x.TestConnectionAsync(configId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.TestConnection(configId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(false);
        _mockLlmService.Verify(x => x.TestConnectionAsync(configId), Times.Once);
    }
}
