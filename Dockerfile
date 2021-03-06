FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine3.12-amd64 AS base
LABEL maintainer="HMBSbige"
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.12-amd64 AS build
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
RUN dotnet publish "Dawdler.csproj" -p:PublishSingleFile=true -r linux-musl-x64 --self-contained true -p:PublishTrimmed=True -p:TrimMode=Link -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/configs", "/app/Logs"]
HEALTHCHECK CMD ["pidof", "Dawdler"]
ENTRYPOINT ["./Dawdler"]