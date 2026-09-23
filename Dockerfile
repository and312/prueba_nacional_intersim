FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/NacionalSeguros.Api/NacionalSeguros.Api.csproj", "src/NacionalSeguros.Api/"]
COPY ["src/NacionalSeguros.Application/NacionalSeguros.Application.csproj", "src/NacionalSeguros.Application/"]
COPY ["src/NacionalSeguros.Contracts/NacionalSeguros.Contracts.csproj", "src/NacionalSeguros.Contracts/"]
COPY ["src/NacionalSeguros.Domain/NacionalSeguros.Domain.csproj", "src/NacionalSeguros.Domain/"]
COPY ["src/NacionalSeguros.Infrastructure/NacionalSeguros.Infrastructure.csproj", "src/NacionalSeguros.Infrastructure/"]
COPY ["src/NacionalSeguros.Persistence/NacionalSeguros.Persistence.csproj", "src/NacionalSeguros.Persistence/"]
COPY ["src/NacionalSeguros.Shared/NacionalSeguros.Shared.csproj", "src/NacionalSeguros.Shared/"]

RUN dotnet restore "./src/NacionalSeguros.Api/NacionalSeguros.Api.csproj"
COPY . .
WORKDIR "/src/src/NacionalSeguros.Api"
RUN dotnet build "./NacionalSeguros.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NacionalSeguros.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NacionalSeguros.Api.dll"]
