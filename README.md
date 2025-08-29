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

## Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **Frontend**: [To be implemented]
- **Testing**: xUnit
- **Platform**: Cross-platform (.NET 8)

## Prerequisites

- .NET 8.0 SDK or later
- A modern web browser

## Getting Started

### Running the Application

```bash
# Clone the repository
git clone https://github.com/rivoli-ai/andy-agentic.git
cd andy-agentic

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project src/Andy.Agentic
```

The application will start on `http://localhost:5030`. Navigate to this URL in your browser to access the web interface.

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
│   └── Andy.Agentic/           # Main ASP.NET Core application
│       ├── Controllers/        # API controllers
│       ├── Models/            # Data models
│       ├── Services/          # Business logic
│       └── Program.cs         # Application entry point
├── tests/
│   └── Andy.Agentic.Tests/    # Unit and integration tests
├── docs/                       # Documentation
└── examples/                   # Example configurations and workflows
```

## Configuration

[Configuration documentation will be added as the project develops]

## API Documentation

When running in development mode, Swagger documentation is available at:
- `http://localhost:5030/swagger`

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

## Roadmap

- [ ] Core agent orchestration engine
- [ ] LLM provider integrations
- [ ] MCP server support
- [ ] Web UI implementation
- [ ] Authentication and authorization
- [ ] Agent workflow designer
- [ ] Performance monitoring dashboard
- [ ] Plugin marketplace

## Disclaimer

This software is provided "as is" without warranty of any kind. Use at your own risk. The authors are not responsible for any damages or losses arising from its use.