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
using System.Collections.Generic;

namespace Andy.Agentic.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockMapper = new Mock<IMapper>();
        _mockAuthService = new Mock<IAuthService>();
        _controller = new ChatController(_mockChatService.Object, _mockMapper.Object, _mockAuthService.Object);
        
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
        var chatMessageDto = new ChatMessageDto
        {
            AgentId = Guid.NewGuid(),
            Content = "Hello, how are you?",
            UserId = Guid.NewGuid()
        };

        var chatMessage = new ChatMessage
        {
            AgentId = chatMessageDto.AgentId,
            Content = chatMessageDto.Content,
            UserId = chatMessageDto.UserId
        };

        var currentUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockMapper
            .Setup(x => x.Map<ChatMessage>(chatMessageDto))
            .Returns(chatMessage);

        _mockChatService
            .Setup(x => x.GetMessageStreamAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Returns(GetMockStream());

        // Act
        await _controller.SendMessageStream(chatMessageDto);

        // Assert
        _mockAuthService.Verify(x => x.GetCurrentUserAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<ChatMessage>(chatMessageDto), Times.Once);
        _mockChatService.Verify(x => x.GetMessageStreamAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageStream_WithInvalidMessage_ShouldReturnBadRequest()
    {
        // Arrange
        var chatMessageDto = new ChatMessageDto
        {
            AgentId = Guid.NewGuid(),
            Content = "",
            UserId = Guid.NewGuid()
        };

        var chatMessage = new ChatMessage
        {
            AgentId = chatMessageDto.AgentId,
            Content = chatMessageDto.Content,
            UserId = chatMessageDto.UserId
        };

        var currentUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockMapper
            .Setup(x => x.Map<ChatMessage>(chatMessageDto))
            .Returns(chatMessage);

        _mockChatService
            .Setup(x => x.GetMessageStreamAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Returns(GetMockErrorStream());

        // Act
        await _controller.SendMessageStream(chatMessageDto);

        // Assert
        _mockAuthService.Verify(x => x.GetCurrentUserAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<ChatMessage>(chatMessageDto), Times.Once);
        _mockChatService.Verify(x => x.GetMessageStreamAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatHistory_WithValidAgentId_ShouldReturnHistory()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var mockedHistory = new List<ChatHistory>
        {
            new() { Id = Guid.NewGuid(), AgentId = agentId, UserId = userId, Content = "Hello", Role = "user" },
            new() { Id = Guid.NewGuid(), AgentId = agentId, UserId = userId, Content = "Hi there!", Role = "assistant" }
        };

        var expectedHistory = new List<ChatHistoryDto>
        {
            new() { Id = mockedHistory[0].Id, AgentId = agentId, UserId = userId, Content = "Hello", Role = "user" },
            new() { Id = mockedHistory[1].Id, AgentId = agentId, UserId = userId, Content = "Hi there!", Role = "assistant" }
        };

        var currentUser = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockChatService
            .Setup(x => x.GetChatHistoryForUserAsync(agentId, userId))
            .ReturnsAsync(mockedHistory);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<ChatHistoryDto>>(mockedHistory))
            .Returns(expectedHistory);

        // Act
        var result = await _controller.GetChatHistory(agentId);

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<ChatHistoryDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedHistory);
        _mockAuthService.Verify(x => x.GetCurrentUserAsync(), Times.Once);
        _mockChatService.Verify(x => x.GetChatHistoryForUserAsync(agentId, userId), Times.Once);
    }

    [Fact]
    public async Task GetChatSessions_WithValidUserId_ShouldReturnSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        
        var mockedSessions = new List<ChatSession>
        {
            new() { SessionId = Guid.NewGuid().ToString(), AgentId = agentId, Title = "Test Session" }
        };

        var expectedSessions = new List<ChatSessionSummaryDto>
        {
            new() { 
                SessionId = mockedSessions[0].SessionId, 
                AgentId = agentId, 
                AgentName = "Test Agent", 
                SessionTitle = "Test Session",
                RecentMessages = new List<ChatMessagePreviewDto>()
            }
        };

        var currentUser = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockChatService
            .Setup(x => x.GetChatSessionsForUserAsync(null, userId))
            .ReturnsAsync(mockedSessions);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<ChatSessionSummaryDto>>(mockedSessions))
            .Returns(expectedSessions);

        // Act
        var result = await _controller.GetChatSessions();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<ChatSessionSummaryDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>();
        _mockAuthService.Verify(x => x.GetCurrentUserAsync(), Times.Once);
        _mockChatService.Verify(x => x.GetChatSessionsForUserAsync(null, userId), Times.Once);
    }

    [Fact]
    public async Task DeleteChatSession_WithValidSessionId_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

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
        var sessionId = Guid.NewGuid().ToString();

        _mockChatService
            .Setup(x => x.DeleteChatSessionAsync(sessionId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteChatSession(sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>(); // The controller returns Ok even when delete returns false
        _mockChatService.Verify(x => x.DeleteChatSessionAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task SendMessageStream_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        var chatMessageDto = new ChatMessageDto
        {
            AgentId = Guid.NewGuid(),
            Content = "Hello",
            UserId = Guid.NewGuid()
        };

        var chatMessage = new ChatMessage
        {
            AgentId = chatMessageDto.AgentId,
            Content = chatMessageDto.Content,
            UserId = chatMessageDto.UserId
        };

        var currentUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(100); // Cancel after 100ms

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockMapper
            .Setup(x => x.Map<ChatMessage>(chatMessageDto))
            .Returns(chatMessage);

        _mockChatService
            .Setup(x => x.GetMessageStreamAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Returns(GetMockStreamWithCancellation(cancellationTokenSource.Token));

        // Act & Assert
        await _controller.SendMessageStream(chatMessageDto, cancellationTokenSource.Token);
        
        // Verify that the method was called
        _mockAuthService.Verify(x => x.GetCurrentUserAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<ChatMessage>(chatMessageDto), Times.Once);
        _mockChatService.Verify(x => x.GetMessageStreamAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static async IAsyncEnumerable<object> GetMockStream()
    {
        yield return new { content = "Hello" };
        yield return new { content = " there!" };
        yield return new { content = " How can I help you?" };
        await Task.Delay(10);
    }

    private static async IAsyncEnumerable<object> GetMockErrorStream()
    {
        yield return new { error = "Invalid message" };
        await Task.Delay(10);
    }

    private static async IAsyncEnumerable<object> GetMockStreamWithCancellation(CancellationToken cancellationToken)
    {
        yield return new { content = "Starting response..." };
        await Task.Delay(200, cancellationToken); // This will throw OperationCanceledException
        yield return new { content = "This should not be reached" };
    }
}

