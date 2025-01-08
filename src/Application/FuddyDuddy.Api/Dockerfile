FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first to optimize layer caching
COPY ["src/Application/FuddyDuddy.Api/FuddyDuddy.Api.csproj", "Application/FuddyDuddy.Api/"]
COPY ["src/Core/FuddyDuddy.Core.Application/FuddyDuddy.Core.Application.csproj", "Core/FuddyDuddy.Core.Application/"]
COPY ["src/Core/FuddyDuddy.Core.Domain/FuddyDuddy.Core.Domain.csproj", "Core/FuddyDuddy.Core.Domain/"]
COPY ["src/Core/FuddyDuddy.Core.Infrastructure/FuddyDuddy.Core.Infrastructure.csproj", "Core/FuddyDuddy.Core.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Application/FuddyDuddy.Api/FuddyDuddy.Api.csproj"

# Copy the rest of the source code
COPY src/. .

# Build the application
RUN dotnet build "Application/FuddyDuddy.Api/FuddyDuddy.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Application/FuddyDuddy.Api/FuddyDuddy.Api.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN useradd -m -s /bin/bash appuser && chown -R appuser:appuser /app
USER appuser

EXPOSE 80
ENTRYPOINT ["dotnet", "FuddyDuddy.Api.dll"] 