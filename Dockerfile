#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Authenticator/Authenticator.csproj", "Authenticator/"]
RUN dotnet restore "Authenticator/Authenticator.csproj"
COPY . .
WORKDIR "/src/Authenticator"
RUN dotnet build "Authenticator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authenticator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authenticator.dll"]