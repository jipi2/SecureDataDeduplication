#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY ./certificate.pfx /etc/ssl/certs/certificate.pfx
COPY ./backend.key /etc/ssl/private/backend.key

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FileStorageApp/Server/FileStorageApp.Server.csproj", "FileStorageApp/Server/"]
COPY ["FileStorageApp/Shared/FileStorageApp.Shared.csproj", "FileStorageApp/Shared/"]
RUN dotnet restore "./FileStorageApp/Server/FileStorageApp.Server.csproj"
COPY . .
WORKDIR "/src/FileStorageApp/Server"
RUN dotnet build "./FileStorageApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileStorageApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorageApp.Server.dll"]