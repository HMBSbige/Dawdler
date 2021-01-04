#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Dawdler/Dawdler.csproj", "Dawdler/"]
COPY ["Dawdler.Application/Dawdler.Application.csproj", "Dawdler.Application/"]
COPY ["Dawdler.Domain/Dawdler.Domain.csproj", "Dawdler.Domain/"]
COPY ["Dawdler.Domain.Shared/Dawdler.Domain.Shared.csproj", "Dawdler.Domain.Shared/"]
RUN dotnet restore "Dawdler/Dawdler.csproj"
COPY . .
WORKDIR "/src/Dawdler"
RUN dotnet build "Dawdler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dawdler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dawdler.dll"]