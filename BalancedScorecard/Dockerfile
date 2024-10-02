# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BalancedScorecard.csproj", "./"]
RUN dotnet restore "./BalancedScorecard.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BalancedScorecard.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "BalancedScorecard.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BalancedScorecard.dll"]
