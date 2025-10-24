using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Controllers;
using Andy.Agentic.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Andy.Agentic.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<ILogger<ChatController>> _mockLogger;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockLogger = new Mock<ILogger<ChatController>>();
        _controller = new ChatController(_mockChatService.Object, _mockLogger.Object);
        
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
    public async Task SendMessageStream_WithValidMessage_ShouldReturnOk()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            AgentId = Guid.NewGuid(),
            Content = "Hello, how are you?",
            UserId = Guid.NewGuid()
        };

        var mockStream = GetMockStream();
        _mockChatService
            .Setup(x => x.SendMessageStreamAsync(chatMessage, It.IsAny<CancellationToken>()))
            .Returns(mockStream);

        // Act
        var result = await _controller.SendMessageStream(chatMessage);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockChatService.Verify(x => x.SendMessageStreamAsync(chatMessage, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageStream_WithInvalidMessage_ShouldReturnBadRequest()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            AgentId = null,
            Content = "",
            UserId = Guid.NewGuid()
        };

        var mockStream = GetMockErrorStream();
        _mockChatService
            .Setup(x => x.SendMessageStreamAsync(chatMessage, It.IsAny<CancellationToken>()))
            .Returns(mockStream);

        // Act
        var result = await _controller.SendMessageStream(chatMessage);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockChatService.Verify(x => x.SendMessageStreamAsync(chatMessage, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatHistory_WithValidAgentId_ShouldReturnHistory()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedHistory = new List<ChatHistoryDto>
        {
            new() { Role = "user", Content = "Hello" },
            new() { Role = "assistant", Content = "Hi there!" }
        };

        _mockChatService
            .Setup(x => x.GetChatHistoryAsync(agentId))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetChatHistory(agentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedHistory);
        _mockChatService.Verify(x => x.GetChatHistoryAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task GetChatSessions_WithValidUserId_ShouldReturnSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedSessions = new List<ChatSessionDto>
        {
            new() { Id = Guid.NewGuid(), AgentId = Guid.NewGuid(), AgentName = "Test Agent", LastMessage = "Hello" }
        };

        _mockChatService
            .Setup(x => x.GetChatSessionsAsync(userId))
            .ReturnsAsync(expectedSessions);

        // Act
        var result = await _controller.GetChatSessions(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSessions);
        _mockChatService.Verify(x => x.GetChatSessionsAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteChatSession_WithValidSessionId_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockChatService
            .Setup(x => x.DeleteChatSessionAsync(sessionId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteChatSession(sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockChatService.Verify(x => x.DeleteChatSessionAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task DeleteChatSession_WithInvalidSessionId_ShouldReturnNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockChatService
            .Setup(x => x.DeleteChatSessionAsync(sessionId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteChatSession(sessionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockChatService.Verify(x => x.DeleteChatSessionAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task SendMessageStream_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            AgentId = Guid.NewGuid(),
            Content = "Hello",
            UserId = Guid.NewGuid()
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(100); // Cancel after 100ms

        _mockChatService
            .Setup(x => x.SendMessageStreamAsync(chatMessage, It.IsAny<CancellationToken>()))
            .Returns(GetMockStreamWithDelay());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await _controller.SendMessageStream(chatMessage, cancellationTokenSource.Token);
        });
    }

    private static async IAsyncEnumerable<string> GetMockStream()
    {
        yield return "Hello";
        yield return " there!";
        yield return " How can I help you?";
        await Task.Delay(10);
    }

    private static async IAsyncEnumerable<string> GetMockErrorStream()
    {
        yield return "Error: Invalid message";
        await Task.Delay(10);
    }

    private static async IAsyncEnumerable<string> GetMockStreamWithDelay()
    {
        yield return "Starting response...";
        await Task.Delay(200); // Longer delay to allow cancellation
        yield return "This should not be reached";
    }
}
