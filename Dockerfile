# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for layer caching
COPY ["src/API/ProjectManagementAPI.API.csproj", "src/API/"]
COPY ["src/Core/ProjectManagementAPI.Core.csproj", "src/Core/"]
COPY ["src/Infrastructure/ProjectManagementAPI.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/ProjectManagementAPI.API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "ProjectManagementAPI.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProjectManagementAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectManagementAPI.API.dll"]
