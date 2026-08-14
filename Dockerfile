FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SentinelCase.slnx ./
COPY global.json ./

COPY src/SentinelCase.Domain/SentinelCase.Domain.csproj \
    src/SentinelCase.Domain/
COPY src/SentinelCase.Application/SentinelCase.Application.csproj \
    src/SentinelCase.Application/
COPY src/SentinelCase.Infrastructure/SentinelCase.Infrastructure.csproj \
    src/SentinelCase.Infrastructure/
COPY src/SentinelCase.Api/SentinelCase.Api.csproj \
    src/SentinelCase.Api/

RUN dotnet restore \
    src/SentinelCase.Api/SentinelCase.Api.csproj

COPY src ./src

RUN dotnet publish \
    src/SentinelCase.Api/SentinelCase.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "SentinelCase.Api.dll"]
