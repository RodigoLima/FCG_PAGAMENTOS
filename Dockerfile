FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FCGPagamentos.API/FCGPagamentos.API.csproj", "src/FCGPagamentos.API/"]
COPY ["src/FCGPagamentos.Application/FCGPagamentos.Application.csproj", "src/FCGPagamentos.Application/"]
COPY ["src/FCGPagamentos.Domain/FCGPagamentos.Domain.csproj", "src/FCGPagamentos.Domain/"]
COPY ["src/FCGPagamentos.Infrastructure/FCGPagamentos.Infrastructure.csproj", "src/FCGPagamentos.Infrastructure/"]
RUN dotnet restore "src/FCGPagamentos.API/FCGPagamentos.API.csproj"
COPY . .
WORKDIR "/src/src/FCGPagamentos.API"
RUN dotnet build "FCGPagamentos.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCGPagamentos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCGPagamentos.API.dll"]

