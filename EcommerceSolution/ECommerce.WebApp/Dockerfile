FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EcommerceSolution.sln", "EcommerceSolution.sln"]
RUN ls -la /src/EcommerceSolution.sln
COPY ["ECommerce.WebApp/ECommerce.WebApp.csproj", "ECommerce.WebApp/"]
COPY ["ECommerce.Application/ECommerce.Application.csproj", "ECommerce.Application/"]
COPY ["ECommerce.Domain/ECommerce.Domain.csproj", "ECommerce.Domain/"]
COPY ["ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "ECommerce.Infrastructure/"]
COPY ["Ecommerce.Models/Ecommerce.Models.csproj", "ECommerce.Models/"]
COPY ["ECommerce.Client/ECommerce.Client.csproj", "ECommerce.Client/"]

RUN dotnet restore "EcommerceSolution.sln"
COPY . .
WORKDIR "/src/ECommerce.WebApp"
RUN dotnet build "./ECommerce.WebApp.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ECommerce.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.WebApp.dll"]
