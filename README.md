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

Prerequisites:
- Docker Desktop must be installed and running

#### Option 1: Using Docker Compose (Recommended)

The easiest way to run Shr2 is with Docker Compose, which sets up both the application and Azurite (Azure Storage emulator) for local development:

```bash
# Build and run with Docker Compose
docker compose up -d --build
```

This will:
- Build the Shr2 application container
- Start an Azurite container for local Azure Storage emulation
- Configure the application to use Azurite automatically
- Expose the application on ports 5000 (HTTP) and 5001 (HTTPS)

Note: We use the modern `docker compose` command (without hyphen) which is now the recommended approach.

#### Option 2: Using Docker Directly

You can also build and run the Docker image directly:

```bash
# Build the Docker image
docker build -t shr2 .

# Run the container with environment variables
docker run -p 5000:80 -p 5001:443 \
  -e SHR2_STORAGE_CONNECTION_STRING="Your Azure Storage connection string" \
  -e SHR2_DOMAIN="https://your.domain/" \
  shr2
```

### Configuration with Environment Variables

Shr2 can be configured using environment variables, which is the recommended approach for Docker deployments:

| Environment Variable | Description | Default |
|----------------------|-------------|--------|
| `SHR2_STORAGE_CONNECTION_STRING` | Azure Storage connection string | From config file |
| `SHR2_STORAGE_PROVIDER` | Storage provider to use | AzTableStorage |
| `SHR2_DOMAIN` | Domain for shortened URLs | From config file |
| `SHR2_ENCODE_WITH_PERMISSION_KEY` | Whether to require API keys | false |
| `SHR2_PERMISSION_KEYS` | Comma-separated list of API keys | From config file |
| `SHR2_CONFIG_PATH` | Path to config file | shr2.config.json |

### Using Azurite for Local Development

The Docker Compose configuration includes Azurite, an Azure Storage emulator, for local development. The connection is pre-configured in the docker-compose.yml file.

If you need to connect to Azurite manually, use this connection string:

```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://azurite:10002/devstoreaccount1;
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

## Continuous Integration & Deployment

The project includes GitHub Actions workflows for automated builds and releases:

### Build and Test Workflow

Triggered on every push to main and pull requests:
- Builds the application
- Runs all tests
- Validates Docker build

### Release Workflow

Triggered when a new release is published:
- Builds the application in Release configuration
- Creates and uploads build artifacts
- Builds a Docker image with version tags
- Attaches build artifacts to the GitHub Release
- Optionally pushes Docker image to a container registry (commented by default)

To create a new release:
1. Go to the GitHub repository
2. Click on "Releases" in the sidebar
3. Click "Create a new release"
4. Enter a tag version (e.g., v1.0.0)
5. Add release notes
6. Click "Publish release"

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
