FROM mcr.microsoft.com/dotnet/sdk:10.0

ENV DEBIAN_FRONTEND=noninteractive
ENV FLUTTER_HOME=/opt/flutter
ENV PATH=/opt/flutter/bin:/opt/flutter/bin/cache/dart-sdk/bin:/root/.dotnet/tools:$PATH

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       git \
       curl \
       ca-certificates \
       unzip \
       xz-utils \
       zip \
       libglu1-mesa \
       libstdc++6 \
       lib32z1 \
       clang \
       cmake \
       ninja-build \
       pkg-config \
       libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

RUN git clone --depth 1 --branch stable https://github.com/flutter/flutter.git ${FLUTTER_HOME} \
    && flutter config --enable-web \
    && flutter config --no-analytics \
    && flutter precache --web

RUN dotnet tool install --global dotnet-ef --version 10.0.14

WORKDIR /workspaces/smart-diabetes-health-platform

CMD ["bash"]
