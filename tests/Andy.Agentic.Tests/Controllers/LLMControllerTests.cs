using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Controllers;
using Andy.Agentic.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Moq;
using FluentAssertions;
using MapsterMapper;

namespace Andy.Agentic.Tests.Controllers;

public class LLMControllerTests
{
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly LlmController _controller;

    public LLMControllerTests()
    {
        _mockLlmService = new Mock<ILlmService>();
        _mockMapper = new Mock<IMapper>();
        _controller = new LlmController(_mockLlmService.Object, _mockMapper.Object);
        
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
        var mockedConfigs = new List<LlmConfig>
        {
            new() { Id = Guid.NewGuid(), Name = "Config 1", Provider = LLMProviderType.OpenAi, ApiKey = "key1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Config 2", Provider = LLMProviderType.AzureOpenAi, ApiKey = "key2", IsActive = true }
        };

        _mockLlmService
            .Setup(x => x.GetAllLlmConfigsAsync())
            .ReturnsAsync(mockedConfigs);

        // Act
        var result = await _controller.GetLlmConfigs();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<LlmConfigDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedConfigs);

        _mockLlmService.Verify(x => x.GetAllLlmConfigsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetLlmConfig_WithValidId_ShouldReturnConfig()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var mockedConfig = new LlmConfig
        {
            Id = configId,
            Name = "Test Config",
            Provider = LLMProviderType.OpenAi,
            ApiKey = "test-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true
        };

        var expectedConfig = new LlmConfigDto
        {
            Id = configId,
            Name = "Test Config",
            Provider = "openai",
            ApiKey = "test-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true
        };

        _mockLlmService
            .Setup(x => x.GetLlmConfigByIdAsync(configId))
            .ReturnsAsync(mockedConfig);

        _mockMapper
            .Setup(x => x.Map<LlmConfigDto>(mockedConfig))
            .Returns(expectedConfig);

        // Act
        var result = await _controller.GetLlmConfig(configId);

        // Assert
        result.Should().BeOfType<ActionResult<LlmConfigDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedConfig);
        _mockLlmService.Verify(x => x.GetLlmConfigByIdAsync(configId), Times.Once);
    }

    [Fact]
    public async Task GetLlmConfig_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var configId = Guid.NewGuid();

        _mockLlmService
            .Setup(x => x.GetLlmConfigByIdAsync(configId))
            .ReturnsAsync((LlmConfig?)null);

        // Act
        var result = await _controller.GetLlmConfig(configId);

        // Assert
        result.Should().BeOfType<ActionResult<LlmConfigDto>>()
            .Which.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockLlmService.Verify(x => x.GetLlmConfigByIdAsync(configId), Times.Once);
    }

    [Fact]
    public async Task CreateLlmConfig_WithValidConfig_ShouldReturnCreatedConfig()
    {
        // Arrange
        var createConfig = new LlmConfigDto
        {
            Name = "New Config",
            Provider = "openai",
            ApiKey = "new-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true
        };

        var mockedConfig = new LlmConfig
        {
            Id = Guid.NewGuid(),
            Name = createConfig.Name,
            Provider = LLMProviderType.OpenAi,
            ApiKey = createConfig.ApiKey,
            BaseUrl = createConfig.BaseUrl,
            IsActive = createConfig.IsActive
        };

        _mockMapper
            .Setup(x => x.Map<LlmConfig>(createConfig))
            .Returns(mockedConfig);

        _mockLlmService
            .Setup(x => x.CreateLlmConfigAsync(It.IsAny<LlmConfig>()))
            .ReturnsAsync(mockedConfig);

        // Act
        var result = await _controller.CreateLlmConfig(createConfig);


        result.Result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result.Result!).Value.Should().BeEquivalentTo(mockedConfig);

        _mockLlmService.Verify(x => x.CreateLlmConfigAsync(It.IsAny<LlmConfig>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLlmConfig_WithValidConfig_ShouldReturnUpdatedConfig()
    {
        // Arrange
        var updateConfig = new LlmConfigDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Config",
            Provider = "azure-openai",
            ApiKey = "updated-key",
            BaseUrl = "https://updated-api.azure.com",
            IsActive = true
        };

        var mockedConfig = new LlmConfig
        {
            Id = updateConfig.Id!.Value,
            Name = updateConfig.Name,
            Provider = LLMProviderType.AzureOpenAi,
            ApiKey = updateConfig.ApiKey,
            BaseUrl = updateConfig.BaseUrl,
            IsActive = updateConfig.IsActive
        };

        _mockMapper
            .Setup(x => x.Map<LlmConfig>(updateConfig))
            .Returns(mockedConfig);

        _mockLlmService
            .Setup(x => x.UpdateLlmConfigAsync(It.IsAny<LlmConfig>()))
            .ReturnsAsync(mockedConfig);

        _mockMapper
            .Setup(x => x.Map<LlmConfigDto>(mockedConfig))
            .Returns(updateConfig);

        // Act
        var result = await _controller.UpdateLlmConfig(updateConfig.Id!.Value, updateConfig);

        // Assert
        result.Should().BeOfType<ActionResult<LlmConfigDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedConfig);
        _mockLlmService.Verify(x => x.UpdateLlmConfigAsync(It.IsAny<LlmConfig>()), Times.Once);
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
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockLlmService.Verify(x => x.DeleteLlmConfigAsync(configId), Times.Once);
    }

    [Fact]
    public async Task TestConnection_WithValidConfigId_ShouldReturnSuccess()
    {
        // Arrange
        var testConnectionDto = new TestConnectionDto
        {
            BaseUrl = "https://api.openai.com",
            ApiKey = "test-key",
            Model = "gpt-4",
            Provider = "openai"
        };

        var testResult = new TestConnectionResult
        {
            Success = true,
            Message = "Connection successful",
            Latency = 100
        };

        _mockLlmService
            .Setup(x => x.TestConnectionAsync(It.IsAny<TestConnection>()))
            .ReturnsAsync(testResult);

        // Act
        var result = await _controller.TestConnection(testConnectionDto);

        // Assert
     result.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result.Result!).Value.Should().BeEquivalentTo(testResult);
        _mockLlmService.Verify(x => x.TestConnectionAsync(It.IsAny<TestConnection>()), Times.Once);
    }

    [Fact]
    public async Task TestConnection_WithInvalidConfigId_ShouldReturnFailure()
    {
        // Arrange
        var testConnectionDto = new TestConnectionDto
        {
            BaseUrl = "https://invalid-api.com",
            ApiKey = "invalid-key",
            Model = "gpt-4",
            Provider = "openai"
        };

        var testResult = new TestConnectionResult
        {
            Success = false,
            Message = "Connection failed",
            Latency = 0
        };

        _mockLlmService
            .Setup(x => x.TestConnectionAsync(It.IsAny<TestConnection>()))
            .ReturnsAsync(testResult);

        // Act
        var result = await _controller.TestConnection(testConnectionDto);

        // Assert
        result.Should().BeOfType<ActionResult<TestConnectionResultDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(testResult);
        _mockLlmService.Verify(x => x.TestConnectionAsync(It.IsAny<TestConnection>()), Times.Once);
    }
}

