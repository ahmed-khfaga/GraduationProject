# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FitZone.APIs/FitZone.APIs.csproj", "FitZone.APIs/"]
COPY ["FitZone.Core/FitZone.Core.csproj", "FitZone.Core/"]
COPY ["FitZone.Repository/FitZone.Repository.csproj", "FitZone.Repository/"]
COPY ["FitZone.Service/FitZone.Service.csproj", "FitZone.Service/"]

RUN dotnet restore "FitZone.APIs/FitZone.APIs.csproj"

COPY . .
WORKDIR "/src/FitZone.APIs"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FitZone.APIs.dll"]
