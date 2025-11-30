FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
RUN addgroup -g 1000 -S appgroup && \
    adduser -u 1000 -S appuser -G appgroup

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["src/FCGPagamentos.API/FCGPagamentos.API.csproj", "src/FCGPagamentos.API/"]
COPY ["src/FCGPagamentos.Application/FCGPagamentos.Application.csproj", "src/FCGPagamentos.Application/"]
COPY ["src/FCGPagamentos.Domain/FCGPagamentos.Domain.csproj", "src/FCGPagamentos.Domain/"]
COPY ["src/FCGPagamentos.Infrastructure/FCGPagamentos.Infrastructure.csproj", "src/FCGPagamentos.Infrastructure/"]
RUN dotnet restore "src/FCGPagamentos.API/FCGPagamentos.API.csproj" --verbosity quiet -r linux-musl-x64

COPY . .
WORKDIR "/src/src/FCGPagamentos.API"

FROM build AS publish
RUN dotnet publish "FCGPagamentos.API.csproj" -c Release -o /app/publish \
    -r linux-musl-x64 \
    /p:UseAppHost=false \
    /p:PublishTrimmed=true \
    /p:TrimMode=partial

FROM base AS final
WORKDIR /app

RUN apk add --no-cache \
    icu-libs \
    tzdata \
    && rm -rf /var/cache/apk/*

COPY --from=publish /app/publish .

USER appuser

ENTRYPOINT ["dotnet", "FCGPagamentos.API.dll"]

