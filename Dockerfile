FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine-amd64 AS build
COPY ./ /src/
WORKDIR /src/Dawdler/
RUN dotnet publish "Dawdler.csproj" -p:PublishSingleFile=true -r linux-musl-x64 --self-contained true -p:PublishTrimmed=True -p:TrimMode=Link -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine-amd64
LABEL maintainer="HMBSbige"
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/app/configs", "/app/Logs"]
HEALTHCHECK CMD ["pidof", "Dawdler"]
ENTRYPOINT ["./Dawdler"]