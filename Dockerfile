# Stage 1: Build stage using .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for caching restore layers
COPY ["TutorPlatform.Domain/TutorPlatform.Domain.csproj", "TutorPlatform.Domain/"]
COPY ["TutorPlatform.Application/TutorPlatform.Application.csproj", "TutorPlatform.Application/"]
COPY ["TutorPlatform.Infrastructure/TutorPlatform.Infrastructure.csproj", "TutorPlatform.Infrastructure/"]
COPY ["TutorPlatform.API/TutorPlatform.API.csproj", "TutorPlatform.API/"]

# Restore dependencies
RUN dotnet restore "TutorPlatform.API/TutorPlatform.API.csproj"

# Copy remaining source files
COPY . .

# Build and publish release output
WORKDIR "/src/TutorPlatform.API"
RUN dotnet publish "TutorPlatform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage using ASP.NET Core 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TutorPlatform.API.dll"]
