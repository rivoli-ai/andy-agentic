# PostgreSQL with pgvector Setup

This project has been updated to use PostgreSQL with the pgvector extension instead of MySQL.

## Environment Variables

Create a `.env` file in the `andy-agentic` directory with the following variables:

```bash
# PostgreSQL Database Configuration
POSTGRES_CONTAINER_NAME=agentic-postgres
POSTGRES_DATABASE=agentic_db
POSTGRES_USER=agentic_user
POSTGRES_PASSWORD=your_secure_password_here
POSTGRES_PORT=5432

# Application Configuration
APP_CONTAINER_NAME=agentic-backend
APP_HTTP_PORT=5000
ASPNETCORE_ENVIRONMENT=Production
```

## Features

- **PostgreSQL 16**: Latest stable version
- **pgvector Extension**: For vector similarity search and RAG functionality
- **Health Checks**: Automatic health monitoring
- **Data Persistence**: Data is persisted in Docker volumes
- **Initialization Scripts**: Automatic pgvector extension setup

## Usage

1. Create your `.env` file with the variables above
2. Run the services:
   ```bash
   docker-compose up -d
   ```

3. Check the logs to ensure pgvector is properly initialized:
   ```bash
   docker-compose logs postgres
   ```

## Connection String

The application will automatically connect to PostgreSQL using the connection string:
```
Host=postgres;Database=agentic_db;Username=agentic_user;Password=your_password;Port=5432;Include Error Detail=true
```

## pgvector Features

The pgvector extension provides:
- Vector similarity search
- Cosine similarity
- L2 distance
- Inner product
- Support for high-dimensional vectors (up to 16,000 dimensions)

This is essential for the RAG (Retrieval Augmented Generation) functionality in the application.
