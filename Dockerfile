# Use the official .NET 9.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Andy.Agentic/Andy.Agentic.csproj", "src/Andy.Agentic/"]
COPY ["src/Andy.Agentic.Application/Andy.Agentic.Application.csproj", "src/Andy.Agentic.Application/"]
COPY ["src/Andy.Agentic.Domain/Andy.Agentic.Domain.csproj", "src/Andy.Agentic.Domain/"]
COPY ["src/Andy.Agentic.Infrastructure/Andy.Agentic.Infrastructure.csproj", "src/Andy.Agentic.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Andy.Agentic/Andy.Agentic.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Andy.Agentic"
RUN dotnet build "Andy.Agentic.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Andy.Agentic.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Andy.Agentic.dll"]
