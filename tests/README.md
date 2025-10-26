# Backend Test Suite

This directory contains comprehensive unit tests for the Andy Agentic backend using Moq, FluentAssertions, and xUnit.

## Test Structure

### Application Layer Tests (`Andy.Agentic.Application.Tests`)

#### Services Tests
- **SimpleAgentServiceTests.cs** - Tests for the AgentService
  - `GetAllAgentsAsync_ShouldReturnAllAgents()` - Verifies retrieval of all agents
  - `GetAgentByIdAsync_WithValidId_ShouldReturnAgent()` - Tests agent retrieval by ID
  - `GetAgentByIdAsync_WithInvalidId_ShouldReturnNull()` - Tests null return for invalid ID
  - `CreateAgentAsync_WithValidAgent_ShouldReturnCreatedAgent()` - Tests agent creation
  - `UpdateAgentAsync_WithValidAgent_ShouldReturnUpdatedAgent()` - Tests agent updates
  - `DeleteAgentAsync_WithValidId_ShouldReturnTrue()` - Tests successful deletion
  - `DeleteAgentAsync_WithInvalidId_ShouldReturnFalse()` - Tests failed deletion

- **SimpleToolServiceTests.cs** - Tests for the ToolService
  - `GetAllToolsAsync_ShouldReturnAllTools()` - Verifies retrieval of all tools
  - `GetToolByIdAsync_WithValidId_ShouldReturnTool()` - Tests tool retrieval by ID
  - `GetToolByIdAsync_WithInvalidId_ShouldReturnNull()` - Tests null return for invalid ID
  - `CreateToolAsync_WithValidTool_ShouldReturnCreatedTool()` - Tests tool creation
  - `UpdateToolAsync_WithValidTool_ShouldReturnUpdatedTool()` - Tests tool updates
  - `DeleteToolAsync_WithValidId_ShouldReturnTrue()` - Tests successful deletion
  - `DeleteToolAsync_WithInvalidId_ShouldReturnFalse()` - Tests failed deletion

### Infrastructure Layer Tests (`Andy.Agentic.Infrastructure.Tests`)

#### Tools Tests
- **ApiToolFactoryTests.cs** - Tests for the ApiToolFactory
  - Tests for different HTTP methods (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
  - Error handling for invalid configurations
  - Validation of required fields

### Controller Tests (`Andy.Agentic.Tests`)

#### Controllers Tests
- **ChatControllerTests.cs** - Tests for the ChatController
- **AgentsControllerTests.cs** - Tests for the AgentsController
- **ToolsControllerTests.cs** - Tests for the ToolsController
- **LLMControllerTests.cs** - Tests for the LLMController

### Integration Tests (`Andy.Agentic.Tests`)

#### API Integration Tests
- **ApiIntegrationTests.cs** - End-to-end API tests
  - Health check tests
  - CRUD operations for all entities
  - Authentication and authorization tests
  - Error handling tests

## Test Configuration

### Dependencies
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Fluent assertion library
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing

### Test Data
- **TestBase.cs** - Common test utilities and data factories
- **appsettings.Test.json** - Test-specific configuration

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/Andy.Agentic.Application.Tests/Andy.Agentic.Application.Tests.csproj
```

### Run Specific Test Class
```bash
dotnet test --filter "SimpleAgentServiceTests"
```

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Results

### Current Status
- **Total Tests**: 14
- **Passed**: 14
- **Failed**: 0
- **Skipped**: 0

### Coverage Areas
- ✅ AgentService CRUD operations
- ✅ ToolService CRUD operations
- ✅ Error handling and validation
- ✅ Mock verification
- ✅ FluentAssertions for readable test assertions

## Best Practices

### Test Naming Convention
- `MethodName_Scenario_ExpectedResult()`
- Example: `GetAgentByIdAsync_WithValidId_ShouldReturnAgent()`

### Test Structure (AAA Pattern)
1. **Arrange** - Set up test data and mocks
2. **Act** - Execute the method under test
3. **Assert** - Verify the results

### Mocking Guidelines
- Mock external dependencies (database, services)
- Verify mock interactions
- Use `Times.Once()` for single calls
- Use `Times.Never()` for calls that shouldn't happen

### Assertion Guidelines
- Use FluentAssertions for readable assertions
- Test both success and failure scenarios
- Verify return values and side effects
- Check exception handling

## Future Enhancements

### Planned Test Coverage
- [ ] ChatService streaming tests
- [ ] LLMService provider tests
- [ ] ToolExecutionService tests
- [ ] Repository layer tests
- [ ] Authentication and authorization tests
- [ ] Performance tests
- [ ] Load tests

### Test Data Management
- [ ] Test data builders
- [ ] Database seeding for integration tests
- [ ] Test data cleanup strategies

### CI/CD Integration
- [ ] Automated test execution
- [ ] Code coverage reporting
- [ ] Test result publishing
- [ ] Performance regression testing


