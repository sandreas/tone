ARG FFMPEG_IMAGE="mwader/static-ffmpeg:5.0.1-3"
ARG FFMPEG_PATH="/ffmpeg"

FROM ${FFMPEG_IMAGE} as ffmpeg_binary

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore

# linux-arm (Linux-Distributionen auf Arm wie Raspbian auf Raspberry Pi Model 2+)
# linux-arm64 \
    
# Build and publish a release
RUN case "$(uname -i)" in \
      aarch64|*arm*64*) RUNTIME_IDENTIFIER=linux-arm64 \
        ;; \
      *) \
        RUNTIME_IDENTIFIER=linux-musl-x64 \
        ;; \
    esac && dotnet publish -r $RUNTIME_IDENTIFIER --self-contained -c Release -o /app/publish tone.csproj


FROM alpine:3.16.0
ENV WORKDIR /mnt/

RUN echo "---- INSTALL RUNTIME PACKAGES ----" && \
  apk add --no-cache --update --upgrade \
  libstdc++ 
  

COPY --from=ffmpeg_binary "$FFMPEG_PATH" /usr/local/bin/
COPY --from=builder /app/publish/tone /usr/local/bin/

WORKDIR ${WORKDIR}
CMD ["--help"]
ENTRYPOINT ["tone"]