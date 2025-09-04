# Andy Agentic

An agentic platform that enables seamless integration of any Large Language Model (LLM), API, and Model Context Protocol (MCP) server through an intuitive web interface.

> ⚠️ **ALPHA RELEASE WARNING** ⚠️
> 
> This software is in ALPHA stage. **NO GUARANTEES** are made about its functionality, stability, or safety.
> 
> **CRITICAL WARNINGS:**
> - This library performs **DESTRUCTIVE OPERATIONS** on files and directories
> - Permission management is **NOT FULLY TESTED** and may have security vulnerabilities
> - **DO NOT USE** in production environments
> - **DO NOT USE** on systems with critical or irreplaceable data
> - **DO NOT USE** on systems without complete, verified backups
> - The authors assume **NO RESPONSIBILITY** for data loss, system damage, or security breaches
> 
> **USE AT YOUR OWN RISK**

## Overview

Andy Agentic provides a unified web-based interface for orchestrating AI agents across different LLM providers, APIs, and MCP servers. It acts as a bridge between various AI services and tools, allowing you to create powerful agentic workflows without being locked into a single provider or protocol.

## Key Features

- **Universal LLM Support**: Connect to any LLM provider (OpenAI, Anthropic, Google, local models, etc.)
- **API Integration**: Seamlessly integrate with any REST or GraphQL API
- **MCP Server Compatibility**: Full support for Model Context Protocol servers
- **Web-Based Interface**: Modern, responsive UI for configuring and managing agents
- **Agent Orchestration**: Create complex multi-agent workflows
- **Real-time Monitoring**: Track agent activities and performance
- **Extensible Architecture**: Plugin system for custom integrations
- **Streaming Chat**: Real-time streaming responses with OpenAI-compatible format
- **Tool Execution**: Dynamic tool calling and execution framework
- **Session Management**: Persistent chat sessions with history tracking
- **Tag System**: Organize and categorize agents with flexible tagging

## Technology Stack

- **Backend**: ASP.NET Core 9.0 Web API
- **Database**: MySQL with Entity Framework Core
- **Architecture**: Clean Architecture with Domain-Driven Design
- **Mapping**: AutoMapper for object-to-object mapping
- **Testing**: xUnit with code coverage
- **Platform**: Cross-platform (.NET 9)
- **Containerization**: Docker & Docker Compose
- **LLM Integration**: OpenAI API, Ollama local models
- **Real-time**: Server-Sent Events (SSE) for streaming

## Prerequisites

### Option 1: Docker (Recommended)
- Docker Desktop or Docker Engine
- Docker Compose

### Option 2: Local Development
- .NET 9.0 SDK or later
- MySQL Server 8.0 or later
- A modern web browser

## Getting Started

### Option 1: Docker (Recommended)

The easiest way to run Andy Agentic is using Docker Compose, which will set up both the application and MySQL database automatically.

```bash
# Clone the repository
git clone https://github.com/rivoli-ai/andy-agentic.git
cd andy-agentic

# Start the application with Docker Compose (builds automatically)
docker-compose up -d

# View logs
docker-compose logs -f app

# Stop the application
docker-compose down
```

The application will be available at:
- **HTTP**: `http://localhost`
- **HTTPS**: `https://localhost` (if SSL is configured)
- **API Documentation**: `http://localhost/swagger`

#### Docker Commands

```bash
# Start services in background
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (WARNING: This will delete all data)
docker-compose down -v

# Rebuild and restart services
docker-compose up --build --force-recreate -d

# Build only (without starting)
docker-compose build

# View running containers
docker-compose ps
```

### Option 2: Local Development

#### Database Setup

1. **Install MySQL Server** (8.0 or later)
2. **Create a database** for the application:
   ```sql
   CREATE DATABASE andy_agentic;
   ```
3. **Update connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=andy_agentic;Uid=your_username;Pwd=your_password;"
     }
   }
   ```

#### Running the Application

```bash
# Clone the repository
git clone https://github.com/rivoli-ai/andy-agentic.git
cd andy-agentic

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project src/Andy.Agentic
```

The application will start on `http://localhost:80` (HTTP) and `https://localhost:443` (HTTPS). Navigate to this URL in your browser to access the web interface.

### Configuration

The application uses a clean architecture with organized configuration in `Startup.cs`:

- **Web Services**: Controllers, Swagger, CORS
- **Database**: Entity Framework Core with MySQL
- **AutoMapper**: Object-to-object mapping
- **Repositories**: Data access layer
- **Services**: Business logic layer
- **HTTP Clients**: External API integration

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure

```
andy-agentic/
├── src/
│   ├── Andy.Agentic/                    # Main ASP.NET Core application
│   │   ├── Controllers/                 # API controllers
│   │   │   ├── AgentsController.cs      # Agent management
│   │   │   ├── ChatController.cs        # Chat and streaming
│   │   │   ├── LLMController.cs         # LLM provider management
│   │   │   ├── TagsController.cs        # Tag management
│   │   │   ├── ToolsController.cs       # Tool management
│   │   │   └── ToolExecutionController.cs # Tool execution
│   │   ├── Program.cs                   # Application entry point
│   │   ├── Startup.cs                   # Configuration and DI setup
│   │   ├── appsettings.json             # Application configuration
│   │   └── appsettings.Docker.json      # Docker-specific configuration
│   ├── Andy.Agentic.Application/        # Application layer
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Interfaces/                  # Service interfaces
│   │   ├── Mapping/                     # AutoMapper profiles
│   │   └── Services/                    # Business logic services
│   ├── Andy.Agentic.Domain/             # Domain layer
│   │   ├── Entities/                    # Domain entities
│   │   ├── Interfaces/                  # Repository interfaces
│   │   ├── Models/                      # Domain models
│   │   └── ValueObjects/                # Domain value objects
│   └── Andy.Agentic.Infrastructure/     # Infrastructure layer
│       ├── Data/                        # Database context
│       ├── Repositories/                # Repository implementations
│       └── Services/                    # Infrastructure services
├── tests/                               # Test projects
│   ├── Andy.Agentic.Tests/              # Main test project
│   ├── Andy.Agentic.Application.Tests/  # Application layer tests
│   ├── Andy.Agentic.Domain.Tests/       # Domain layer tests
│   └── Andy.Agentic.Infrastructure.Tests/ # Infrastructure tests
├── docs/                                # Documentation
├── examples/                            # Example configurations
├── Dockerfile                           # Docker container definition
├── docker-compose.yml                   # Docker Compose configuration
├── .dockerignore                        # Docker build context exclusions
└── shop_schema.sql                      # Database schema
```

## Docker Configuration

### Docker Services

The `docker-compose.yml` file defines two main services:

#### MySQL Database (`mysql`)
- **Image**: `mysql:8.0`
- **Port**: `3306` (mapped to host)
- **Database**: `andy_agentic`
- **Credentials**: 
  - Root: `root` / `rootpassword`
  - User: `andy_user` / `andy_password`
- **Data Persistence**: Named volume `mysql_data`
- **Health Check**: Automatic MySQL connectivity verification

#### Andy Agentic Application (`app`)
- **Build**: Multi-stage Docker build from source
- **Ports**: `80` (HTTP) and `443` (HTTPS)
- **Environment**: Production mode
- **Dependencies**: Waits for MySQL to be healthy
- **Logs**: Persistent volume `app_logs`

### Docker Environment Variables

The application uses the following environment variables in Docker:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
ConnectionStrings__DefaultConnection=Server=mysql;Database=andy_agentic;User=andy_user;Password=andy_password;Port=3306
```

### Docker Volumes

- **`mysql_data`**: Persistent MySQL database storage
- **`app_logs`**: Application log files

### Docker Network

All services run on the `andy-agentic-network` bridge network for secure internal communication.

### Troubleshooting Docker Issues

#### Build Issues
If you encounter build errors, try these steps:

```bash
# Clean build (removes cached layers)
docker-compose build --no-cache

# Force rebuild and restart
docker-compose up --build --force-recreate -d

# Check build logs
docker-compose logs app
```

#### Docker Desktop Not Running
If you get connection errors, ensure Docker Desktop is running:

```bash
# Check Docker status
docker version

# Start Docker Desktop if needed (Windows/Mac)
# Or restart Docker service (Linux)
sudo systemctl restart docker
```

#### Port Already in Use
If ports 80 or 3306 are already in use:

```bash
# Check what's using the ports
netstat -tulpn | grep :80
netstat -tulpn | grep :3306

# Stop conflicting services or modify docker-compose.yml ports
```

## API Documentation

When running in development mode, Swagger documentation is available at:
- `http://localhost/swagger` (HTTP)
- `https://localhost/swagger` (HTTPS)

### Available Endpoints

#### Agents
- `GET /api/agents` - List all agents
- `GET /api/agents/{id}` - Get agent by ID
- `POST /api/agents` - Create new agent
- `PUT /api/agents/{id}` - Update agent
- `DELETE /api/agents/{id}` - Delete agent

#### Chat
- `POST /api/chat/stream` - Stream chat messages (SSE)
- `GET /api/chat/history/{agentId}` - Get chat history
- `GET /api/chat/sessions` - List chat sessions
- `POST /api/chat/sessions` - Create new session

#### Tags
- `GET /api/tags` - List all tags
- `POST /api/tags` - Create new tag
- `PUT /api/tags/{id}` - Update tag
- `DELETE /api/tags/{id}` - Delete tag
- `GET /api/tags/search` - Search tags

#### Tools
- `GET /api/tools` - List all tools
- `POST /api/tools` - Create new tool
- `POST /api/tools/execute` - Execute tool

#### LLM Providers
- `GET /api/llm/providers` - List available providers
- `POST /api/llm/test-connection` - Test LLM connection

## Contributing

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## Security Considerations

This is an alpha release intended for development and testing purposes only. The application:
- May have unpatched security vulnerabilities
- Should not be exposed to public networks
- Should not be used with sensitive data
- Requires careful permission configuration

Always run in isolated environments with appropriate security measures.

## License

This project is licensed under the Apache License, Version 2.0. See the [LICENSE](LICENSE) file for details.

## Support

This is an alpha release. Community support is available through:
- GitHub Issues for bug reports and feature requests
- Discussions for general questions and ideas

## Architecture

Andy Agentic follows **Clean Architecture** principles with clear separation of concerns:

### Domain Layer (`Andy.Agentic.Domain`)
- **Entities**: Core business objects (Agent, Tool, Tag, etc.)
- **Interfaces**: Repository and service contracts
- **Models**: Domain models for business logic
- **Value Objects**: Immutable objects representing domain concepts

### Application Layer (`Andy.Agentic.Application`)
- **DTOs**: Data Transfer Objects for API communication
- **Services**: Business logic and orchestration
- **Interfaces**: Service contracts
- **Mapping**: AutoMapper profiles for object mapping

### Infrastructure Layer (`Andy.Agentic.Infrastructure`)
- **Data**: Entity Framework Core database context
- **Repositories**: Data access implementations
- **Services**: External service integrations (LLM providers, APIs)

### Presentation Layer (`Andy.Agentic`)
- **Controllers**: REST API endpoints
- **Startup**: Dependency injection and configuration
- **Program**: Application entry point

## Features Implemented

### ✅ Core Features
- [x] **Agent Management**: Create, read, update, delete agents
- [x] **LLM Integration**: OpenAI and Ollama provider support
- [x] **Tool System**: Dynamic tool creation and execution
- [x] **Chat System**: Real-time streaming with SSE
- [x] **Session Management**: Persistent chat sessions
- [x] **Tag System**: Organize agents with tags
- [x] **MCP Server Support**: Model Context Protocol integration
- [x] **Clean Architecture**: Domain-driven design implementation
- [x] **AutoMapper**: Object-to-object mapping
- [x] **Repository Pattern**: Data access abstraction
- [x] **Unit of Work**: Transaction management
- [x] **Comprehensive Testing**: xUnit test framework

### 🔄 In Progress
- [ ] **Authentication & Authorization**: User management and security
- [ ] **Web UI**: Frontend interface implementation
- [ ] **Performance Monitoring**: Metrics and analytics
- [ ] **Plugin System**: Extensible architecture

### 📋 Roadmap
- [ ] **Agent Workflow Designer**: Visual workflow creation
- [ ] **Multi-Agent Orchestration**: Complex agent interactions
- [ ] **Plugin Marketplace**: Third-party integrations
- [ ] **Advanced Analytics**: Usage patterns and insights
- [ ] **API Rate Limiting**: Request throttling and management
- [ ] **Caching Layer**: Performance optimization
- [ ] **Message Queuing**: Asynchronous processing
- [ ] **Webhook Support**: Event-driven integrations

## Disclaimer

This software is provided "as is" without warranty of any kind. Use at your own risk. The authors are not responsible for any damages or losses arising from its use.