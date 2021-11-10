FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
ARG TARGETARCH
ARG TARGETOS
COPY ./ /src/
WORKDIR /src/Dawdler/
RUN dotnet --info && \
    case "$TARGETOS" in \
      "linux") ;; \
      *) echo "ERROR: Unsupported OS: ${TARGETOS}"; exit 1 ;; \
    esac && \
    case "$TARGETARCH" in \
      "amd64") dotnet_rid="linux-musl-x64" ;; \
      *) echo "ERROR: Unsupported CPU architecture: ${TARGETARCH}"; exit 1 ;; \
    esac && \
    dotnet publish "Dawdler.csproj" \
    -r "$dotnet_rid" \
    -p:PublishSingleFile=true --self-contained true \
    -p:PublishTrimmed=True \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine
LABEL maintainer="HMBSbige"
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/app/configs", "/app/Logs"]
HEALTHCHECK CMD ["pidof", "Dawdler"]
ENTRYPOINT ["./Dawdler"]