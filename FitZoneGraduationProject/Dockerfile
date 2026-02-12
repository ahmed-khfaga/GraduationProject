# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FitZoneGraduationProject/ ./FitZoneGraduationProject/

RUN dotnet restore "FitZoneGraduationProject/FitZone.APIs/FitZone.APIs.csproj"
RUN dotnet publish "FitZoneGraduationProject/FitZone.APIs/FitZone.APIs.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FitZone.APIs.dll"]
