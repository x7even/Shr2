# Shr2

Shr2 is a URL shortening API built with .NET 9, designed to be cross-platform and run on both Windows and Linux. It uses Azure Table Storage for URL index storage and provides a Google URL Shortener-compatible API.

## Features

- RESTful API for URL shortening
- Google URL Shortener-compatible API format
- Cross-platform support (Windows/Linux)
- Docker containerization
- Memory caching for improved performance
- Comprehensive logging
- Health checks with Azure Storage monitoring
- API rate limiting for security
- Swagger API documentation
- Improved error handling

## API Usage

### Shorten a URL

```http
POST /api/v1/url
Content-Type: application/json

{
  "longUrl": "https://github.com/x7even/Shr2"
}
```

Response:

```json
{
  "kind": "urlshortener#url",
  "id": "https://your.domain/fbsS",
  "longUrl": "https://github.com/x7even/Shr2"
}
```

### Access a shortened URL

Simply navigate to the shortened URL:

```
https://your.domain/fbsS
```

## Running the Application

### Prerequisites

- .NET 9 SDK
- Docker (optional, for containerized deployment)

### Configuration

Edit the `shr2.config.json` file to configure your application:

```json
{
  "StorageConnectionString": "Your Azure Storage connection string",
  "StorageProvider": "AzTableStorage",
  "Domain": "https://your.domain/",
  "EncodeWithPermissionKey": false,
  "PermissionKeys": []
}
```

### Running Locally

```bash
# Clone the repository
git clone https://github.com/x7even/Shr2.git
cd Shr2

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project Shr2/Shr2.csproj
```

### Using Docker

```bash
# Build and run with Docker Compose
docker-compose up -d

# Or build and run the Docker image directly
docker build -t shr2 .
docker run -p 5000:80 -p 5001:443 shr2
```

### Using Azurite for Local Development

The Docker Compose configuration includes Azurite, an Azure Storage emulator, for local development:

```bash
# Start the services
docker-compose up -d

# Configure your application to use Azurite
# Connection string: DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://azurite:10002/devstoreaccount1;
```

## Architecture

Shr2 follows a clean architecture with dependency injection:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Implement business logic
- **Providers**: Handle data storage and retrieval
- **Models**: Define data structures
- **Interfaces**: Define contracts between components

## Cross-Platform Compatibility

Shr2 is designed to run on both Windows and Linux environments:

- Uses cross-platform Azure.Data.Tables SDK
- Implements path normalization for file operations
- Containerized with Docker for consistent deployment
- CI/CD pipeline with GitHub Actions

## Security Features

### Rate Limiting

The API includes rate limiting to protect against abuse:

- Limits requests to 100 per minute per IP address
- Returns HTTP 429 (Too Many Requests) when limit is exceeded
- Configurable limits in Program.cs

### API Key Authentication

Optional API key authentication for URL shortening:

- Enable by setting `EncodeWithPermissionKey` to `true` in config
- Add allowed keys to the `PermissionKeys` array
- Pass the key as a query parameter: `/api/v1/url?key=your-api-key`

## Monitoring and Reliability

### Health Checks

The application includes health checks to monitor system status:

- Azure Storage connectivity check
- Accessible at `/health` endpoint
- Returns HTTP 200 when all systems are operational
- Returns HTTP 503 when any dependency is unavailable

### Logging

Comprehensive structured logging throughout the application:

- Request/response logging
- Error tracking with exception details
- Performance metrics
- Security events (authentication failures, rate limit hits)
- Configurable log levels

## Performance Optimizations

### Memory Caching

The application uses in-memory caching to improve performance:

- URL lookup results are cached to reduce database queries
- Configurable cache expiration (sliding and absolute)
- Automatic cache invalidation for modified data

### Async Processing

All operations use proper async/await patterns:

- Non-blocking I/O operations
- Efficient thread utilization
- Improved scalability under load
- Background processing for non-critical operations

## Contributions Welcome

Contributions are welcome! Please feel free to submit a Pull Request.
