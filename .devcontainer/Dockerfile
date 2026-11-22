FROM mcr.microsoft.com/devcontainers/dotnet:10.0

ENV DEBIAN_FRONTEND=noninteractive
ENV FLUTTER_HOME=/opt/flutter
ENV DOTNET_TOOLS=/opt/dotnet-tools
ENV PATH=/opt/flutter/bin:/opt/flutter/bin/cache/dart-sdk/bin:/opt/dotnet-tools:$PATH

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
       openssl \
    && rm -rf /var/lib/apt/lists/*

RUN git clone --depth 1 --branch stable https://github.com/flutter/flutter.git ${FLUTTER_HOME} \
    && ${FLUTTER_HOME}/bin/flutter config --enable-web \
    && ${FLUTTER_HOME}/bin/flutter config --no-analytics \
    && ${FLUTTER_HOME}/bin/flutter precache --web \
    && chown -R vscode:vscode ${FLUTTER_HOME}

RUN mkdir -p ${DOTNET_TOOLS} \
    && dotnet tool install --tool-path ${DOTNET_TOOLS} dotnet-ef --version 10.0.14 \
    && chmod -R a+rx ${DOTNET_TOOLS}

WORKDIR /workspaces/smart-diabetes-health-platform

CMD ["bash"]
