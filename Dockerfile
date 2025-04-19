# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image for compilation
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# Copy project files first for better layer caching
COPY ["Shr2/Shr2.csproj", "Shr2/"]
COPY ["Shr2.Interfaces/Shr2.Interfaces.csproj", "Shr2.Interfaces/"]
RUN dotnet restore "Shr2/Shr2.csproj"
# Copy the rest of the source code
COPY . .
WORKDIR "/src/Shr2"
RUN dotnet build "Shr2.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Shr2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a directory for configuration
RUN mkdir -p /app/config

# Copy the default configuration file
COPY --from=publish /app/publish/shr2.config.json /app/config/shr2.config.json

# Set environment variables for configuration
ENV SHR2_CONFIG_PATH=/app/config/shr2.config.json

# Command to run the application
ENTRYPOINT ["dotnet", "Shr2.dll"]
