# Custom GitHub Actions Runner with Pre-installed Tools
# Base: catthehacker/ubuntu:act-22.04 (optimized for act + GitHub Actions)
# This base image includes Node.js 20, Docker CLI, git, and common tools
# Compatible with: act (local testing), GitHub Actions (cloud)
FROM ghcr.io/catthehacker/ubuntu:act-22.04

# Metadata labels
LABEL org.opencontainers.image.title="Consilient GitHub Actions Runner"
LABEL org.opencontainers.image.description="Custom runner based on catthehacker/ubuntu:act-22.04 with Azure CLI, sqlcmd, and SchemaSpy"
LABEL org.opencontainers.image.version="2.2.4"
LABEL org.opencontainers.image.tools="azure-cli=latest,sqlcmd=1.6.0,schemaspy=6.2.4,msal4j=1.16.2,terraform=1.9.0,nodejs=20,docker-cli=latest,java=17,chrome=latest"

# ============================================================
# Available Tools in this Image:
# ============================================================
# System Utilities (from catthehacker base):
#   - curl, ca-certificates, gnupg, lsb-release
#   - tar, bzip2, unzip
#   - git, jq, build-essential, node.js 20
#   - Docker CLI, Docker buildx
#
# Cloud & Infrastructure:
#   - Azure CLI (latest stable) - ADDED
#   - Terraform v1.9.0 - ADDED
#
# Database Tools:
#   - sqlcmd v1.6.0 (with Azure AD auth support) - ADDED
#
# Development Tools:
#   - Java 17 (OpenJDK JRE) - ADDED
#   - SchemaSpy v6.2.4 - ADDED
#   - MSSQL JDBC Driver v12.4.2.jre11 - ADDED
#   - MSAL4J v1.16.2 (Azure AD auth for JDBC) with full dependency tree - ADDED
#
# Browser Automation & Testing:
#   - Google Chrome (latest stable from official repo) - ADDED
#
# Note: Docker CLI requires /var/run/docker.sock to be mounted
#       when running this container to access the host's Docker daemon
# ============================================================

# Prevent interactive prompts during package installation
ENV DEBIAN_FRONTEND=noninteractive

# Install Java (required for SchemaSpy)
# Note: git is already in the catthehacker base image, so no need to install it
RUN apt-get update && \
    apt-get install -y openjdk-17-jre-headless && \
    rm -rf /var/lib/apt/lists/*

# ============================================================
# LAYER 0: Chrome (required for Lighthouse CI)
# ============================================================
# Install Google Chrome from official repository and dependencies for headless browser operation
RUN apt-get update && \
    apt-get install -y wget ca-certificates gnupg lsb-release && \
    mkdir -p /etc/apt/keyrings && \
    wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | gpg --dearmor -o /etc/apt/keyrings/google-chrome-archive-keyring.gpg && \
    echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/google-chrome-archive-keyring.gpg] http://dl.google.com/linux/chrome/deb/ stable main" | tee /etc/apt/sources.list.d/google-chrome.list && \
    apt-get update && \
    apt-get install -y \
        google-chrome-stable \
        fonts-liberation \
        libappindicator3-1 \
        libasound2 \
        libatk-bridge2.0-0 \
        libatk1.0-0 \
        libcairo2 \
        libcups2 \
        libdbus-1-3 \
        libexpat1 \
        libfontconfig1 \
        libgbm1 \
        libgcc1 \
        libglib2.0-0 \
        libgtk-3-0 \
        libnspr4 \
        libnss3 \
        libpango-1.0-0 \
        libpangocairo-1.0-0 \
        libstdc++6 \
        libx11-6 \
        libx11-xcb1 \
        libxcb1 \
        libxcomposite1 \
        libxcursor1 \
        libxdamage1 \
        libxext6 \
        libxfixes3 \
        libxi6 \
        libxrandr2 \
        libxrender1 \
        libxss1 \
        libxtst6 \
        xdg-utils && \
    rm -rf /var/lib/apt/lists/*

# Set Chrome path for Lighthouse CI
ENV CHROME_PATH=/usr/bin/google-chrome-stable

# ============================================================
# LAYER 1: Azure CLI (Latest Stable)
# ============================================================
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# ============================================================
# LAYER 2: sqlcmd v1.6.0
# ============================================================
ENV SQLCMD_VERSION=1.6.0
ENV SQLCMD_PATH=/usr/local/bin

RUN curl -L -o /tmp/sqlcmd.tar.bz2 \
    "https://github.com/microsoft/go-sqlcmd/releases/download/v${SQLCMD_VERSION}/sqlcmd-v${SQLCMD_VERSION}-linux-x64.tar.bz2" && \
    tar -xf /tmp/sqlcmd.tar.bz2 -C /tmp && \
    mv /tmp/sqlcmd ${SQLCMD_PATH}/sqlcmd && \
    chmod +x ${SQLCMD_PATH}/sqlcmd && \
    rm -rf /tmp/sqlcmd.tar.bz2

# Verify sqlcmd installation
RUN sqlcmd --version

# ============================================================
# LAYER 3: SchemaSpy 6.2.4 + JDBC Driver 12.4.2 + Azure-Identity Dependencies
# ============================================================
ENV SCHEMASPY_VERSION=6.2.4
ENV JDBC_VERSION=12.4.2
ENV AZURE_IDENTITY_VERSION=1.9.0
ENV SCHEMASPY_PATH=/opt/schemaspy

# Maven Central Base URL for conciseness
ENV MAVEN_REPO="https://repo1.maven.org/maven2"

RUN mkdir -p ${SCHEMASPY_PATH} && \
    # SchemaSpy
    curl -L -o ${SCHEMASPY_PATH}/schemaspy.jar \
        "https://github.com/schemaspy/schemaspy/releases/download/v${SCHEMASPY_VERSION}/schemaspy-${SCHEMASPY_VERSION}.jar" && \
    # MSSQL JDBC Driver
    curl -L -o ${SCHEMASPY_PATH}/mssql-jdbc.jar \
        "${MAVEN_REPO}/com/microsoft/sqlserver/mssql-jdbc/${JDBC_VERSION}.jre11/mssql-jdbc-${JDBC_VERSION}.jre11.jar" && \
    # Azure-Identity + Core Libraries for Azure AD Authentication (JDBC Driver v12.4.2 requirement)
    curl -L -o ${SCHEMASPY_PATH}/azure-identity.jar \
        "${MAVEN_REPO}/com/azure/azure-identity/${AZURE_IDENTITY_VERSION}/azure-identity-${AZURE_IDENTITY_VERSION}.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/azure-core.jar \
        "${MAVEN_REPO}/com/azure/azure-core/1.39.0/azure-core-1.39.0.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/azure-core-http-netty.jar \
        "${MAVEN_REPO}/com/azure/azure-core-http-netty/1.13.3/azure-core-http-netty-1.13.3.jar" && \
    # Azure JSON (required by azure-core >= 1.35.0)
    curl -L -o ${SCHEMASPY_PATH}/azure-json.jar \
        "${MAVEN_REPO}/com/azure/azure-json/1.1.0/azure-json-1.1.0.jar" && \
    # MSAL4J 1.13.8 (required by azure-identity)
    curl -L -o ${SCHEMASPY_PATH}/msal4j.jar \
        "${MAVEN_REPO}/com/microsoft/azure/msal4j/1.13.8/msal4j-1.13.8.jar" && \
    # MSAL4J Persistence Extension
    curl -L -o ${SCHEMASPY_PATH}/msal4j-persistence-extension.jar \
        "${MAVEN_REPO}/com/microsoft/azure/msal4j-persistence-extension/1.2.0/msal4j-persistence-extension-1.2.0.jar" && \
    # JNA Platform (required by azure-identity)
    curl -L -o ${SCHEMASPY_PATH}/jna.jar \
        "${MAVEN_REPO}/net/java/dev/jna/jna/5.13.0/jna-5.13.0.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/jna-platform.jar \
        "${MAVEN_REPO}/net/java/dev/jna/jna-platform/5.6.0/jna-platform-5.6.0.jar" && \
    # Reactor Netty (The Driver - required by azure-core-http-netty)
    curl -L -o ${SCHEMASPY_PATH}/reactor-netty-http.jar \
        "${MAVEN_REPO}/io/projectreactor/netty/reactor-netty-http/1.0.24/reactor-netty-http-1.0.24.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/reactor-netty-core.jar \
        "${MAVEN_REPO}/io/projectreactor/netty/reactor-netty-core/1.0.24/reactor-netty-core-1.0.24.jar" && \
    # Reactor Pool (required by reactor-netty-core)
    curl -L -o ${SCHEMASPY_PATH}/reactor-pool.jar \
        "${MAVEN_REPO}/io/projectreactor/addons/reactor-pool/0.2.10/reactor-pool-0.2.10.jar" && \
    # Netty Core Modules (Granular approach to fix AddressResolverGroup error - replace netty-all)
    curl -L -o ${SCHEMASPY_PATH}/netty-common.jar \
        "${MAVEN_REPO}/io/netty/netty-common/4.1.86.Final/netty-common-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-buffer.jar \
        "${MAVEN_REPO}/io/netty/netty-buffer/4.1.86.Final/netty-buffer-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-transport.jar \
        "${MAVEN_REPO}/io/netty/netty-transport/4.1.86.Final/netty-transport-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-resolver.jar \
        "${MAVEN_REPO}/io/netty/netty-resolver/4.1.86.Final/netty-resolver-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-handler.jar \
        "${MAVEN_REPO}/io/netty/netty-handler/4.1.86.Final/netty-handler-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-codec.jar \
        "${MAVEN_REPO}/io/netty/netty-codec/4.1.86.Final/netty-codec-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-codec-http.jar \
        "${MAVEN_REPO}/io/netty/netty-codec-http/4.1.86.Final/netty-codec-http-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-codec-http2.jar \
        "${MAVEN_REPO}/io/netty/netty-codec-http2/4.1.86.Final/netty-codec-http2-4.1.86.Final.jar" && \
    # Netty DNS (CRITICAL: This contains AddressResolverGroup)
    curl -L -o ${SCHEMASPY_PATH}/netty-resolver-dns.jar \
        "${MAVEN_REPO}/io/netty/netty-resolver-dns/4.1.86.Final/netty-resolver-dns-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-codec-dns.jar \
        "${MAVEN_REPO}/io/netty/netty-codec-dns/4.1.86.Final/netty-codec-dns-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-handler-proxy.jar \
        "${MAVEN_REPO}/io/netty/netty-handler-proxy/4.1.86.Final/netty-handler-proxy-4.1.86.Final.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/netty-transport-native-unix-common.jar \
        "${MAVEN_REPO}/io/netty/netty-transport-native-unix-common/4.1.86.Final/netty-transport-native-unix-common-4.1.86.Final.jar" && \
    # Project Reactor (required by azure-core)
    curl -L -o ${SCHEMASPY_PATH}/reactor-core.jar \
        "${MAVEN_REPO}/io/projectreactor/reactor-core/3.4.27/reactor-core-3.4.27.jar" && \
    # Reactive Streams (required by reactor)
    curl -L -o ${SCHEMASPY_PATH}/reactive-streams.jar \
        "${MAVEN_REPO}/org/reactivestreams/reactive-streams/1.0.4/reactive-streams-1.0.4.jar" && \
    # SLF4J API and Implementation
    curl -L -o ${SCHEMASPY_PATH}/slf4j-api.jar \
        "${MAVEN_REPO}/org/slf4j/slf4j-api/2.0.7/slf4j-api-2.0.7.jar" && \
    # Jackson (for JSON serialization)
    curl -L -o ${SCHEMASPY_PATH}/jackson-core.jar \
        "${MAVEN_REPO}/com/fasterxml/jackson/core/jackson-core/2.15.3/jackson-core-2.15.3.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/jackson-databind.jar \
        "${MAVEN_REPO}/com/fasterxml/jackson/core/jackson-databind/2.15.3/jackson-databind-2.15.3.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/jackson-annotations.jar \
        "${MAVEN_REPO}/com/fasterxml/jackson/core/jackson-annotations/2.15.3/jackson-annotations-2.15.3.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/jackson-datatype-jsr310.jar \
        "${MAVEN_REPO}/com/fasterxml/jackson/datatype/jackson-datatype-jsr310/2.15.3/jackson-datatype-jsr310-2.15.3.jar" && \
    # OAuth2 and JOSE (required by msal4j)
    curl -L -o ${SCHEMASPY_PATH}/oauth2-oidc-sdk.jar \
        "${MAVEN_REPO}/com/nimbusds/oauth2-oidc-sdk/10.7.1/oauth2-oidc-sdk-10.7.1.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/nimbus-jose-jwt.jar \
        "${MAVEN_REPO}/com/nimbusds/nimbus-jose-jwt/9.37.3/nimbus-jose-jwt-9.37.3.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/content-type.jar \
        "${MAVEN_REPO}/com/nimbusds/content-type/2.3/content-type-2.3.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/lang-tag.jar \
        "${MAVEN_REPO}/com/nimbusds/lang-tag/1.7/lang-tag-1.7.jar" && \
    # JSON processing
    curl -L -o ${SCHEMASPY_PATH}/json-smart.jar \
        "${MAVEN_REPO}/net/minidev/json-smart/2.5.0/json-smart-2.5.0.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/accessors-smart.jar \
        "${MAVEN_REPO}/net/minidev/accessors-smart/2.5.0/accessors-smart-2.5.0.jar" && \
    # ASM (bytecode manipulation, required by accessors-smart)
    curl -L -o ${SCHEMASPY_PATH}/asm.jar \
        "${MAVEN_REPO}/org/ow2/asm/asm/9.3/asm-9.3.jar" && \
    # Annotations
    curl -L -o ${SCHEMASPY_PATH}/jcip-annotations.jar \
        "${MAVEN_REPO}/com/github/stephenc/jcip/jcip-annotations/1.0-1/jcip-annotations-1.0-1.jar"

# Add SchemaSpy to PATH
ENV PATH="${SCHEMASPY_PATH}:${PATH}"

# Set CLASSPATH to include all Azure-Identity dependencies for Azure AD authentication
ENV CLASSPATH="${SCHEMASPY_PATH}/azure-identity.jar:${SCHEMASPY_PATH}/azure-core.jar:${SCHEMASPY_PATH}/azure-core-http-netty.jar:${SCHEMASPY_PATH}/azure-json.jar:${SCHEMASPY_PATH}/msal4j.jar:${SCHEMASPY_PATH}/msal4j-persistence-extension.jar:${SCHEMASPY_PATH}/jna.jar:${SCHEMASPY_PATH}/jna-platform.jar:${SCHEMASPY_PATH}/reactor-netty-http.jar:${SCHEMASPY_PATH}/reactor-netty-core.jar:${SCHEMASPY_PATH}/reactor-pool.jar:${SCHEMASPY_PATH}/netty-common.jar:${SCHEMASPY_PATH}/netty-buffer.jar:${SCHEMASPY_PATH}/netty-transport.jar:${SCHEMASPY_PATH}/netty-resolver.jar:${SCHEMASPY_PATH}/netty-handler.jar:${SCHEMASPY_PATH}/netty-codec.jar:${SCHEMASPY_PATH}/netty-codec-http.jar:${SCHEMASPY_PATH}/netty-codec-http2.jar:${SCHEMASPY_PATH}/netty-resolver-dns.jar:${SCHEMASPY_PATH}/netty-codec-dns.jar:${SCHEMASPY_PATH}/netty-handler-proxy.jar:${SCHEMASPY_PATH}/netty-transport-native-unix-common.jar:${SCHEMASPY_PATH}/reactor-core.jar:${SCHEMASPY_PATH}/reactive-streams.jar:${SCHEMASPY_PATH}/slf4j-api.jar:${SCHEMASPY_PATH}/jackson-core.jar:${SCHEMASPY_PATH}/jackson-databind.jar:${SCHEMASPY_PATH}/jackson-annotations.jar:${SCHEMASPY_PATH}/jackson-datatype-jsr310.jar:${SCHEMASPY_PATH}/oauth2-oidc-sdk.jar:${SCHEMASPY_PATH}/nimbus-jose-jwt.jar:${SCHEMASPY_PATH}/content-type.jar:${SCHEMASPY_PATH}/lang-tag.jar:${SCHEMASPY_PATH}/json-smart.jar:${SCHEMASPY_PATH}/accessors-smart.jar:${SCHEMASPY_PATH}/asm.jar:${SCHEMASPY_PATH}/jcip-annotations.jar"

# ============================================================
# LAYER 3B: Bake GitHub Actions into the Image
# ============================================================
# Pre-clone frequently-used GitHub Actions to avoid network calls during workflow execution
# This significantly speeds up act and GitHub Actions workflows, and provides offline capability
# IMPORTANT: Directory structure must match act's expectations for --action-offline-mode
# Format: /github/actions/<owner>/<repo>@<ref>/

ENV GITHUB_ACTIONS_PATH=/github/actions
RUN mkdir -p ${GITHUB_ACTIONS_PATH}

# Pre-cache all GitHub Actions in a single layer for faster builds
# Combines 6 git clone operations to reduce layer count and enable better parallelization
RUN mkdir -p ${GITHUB_ACTIONS_PATH}/actions ${GITHUB_ACTIONS_PATH}/hashicorp ${GITHUB_ACTIONS_PATH}/azure ${GITHUB_ACTIONS_PATH}/docker && \
    git clone --depth 1 --branch v4 https://github.com/actions/checkout.git ${GITHUB_ACTIONS_PATH}/actions/checkout@v4 && \
    git clone --depth 1 --branch v3 https://github.com/hashicorp/setup-terraform.git ${GITHUB_ACTIONS_PATH}/hashicorp/setup-terraform@v3 && \
    git clone --depth 1 --branch v2 https://github.com/Azure/login.git ${GITHUB_ACTIONS_PATH}/azure/login@v2 && \
    git clone --depth 1 --branch v3 https://github.com/docker/setup-buildx-action.git ${GITHUB_ACTIONS_PATH}/docker/setup-buildx-action@v3 && \
    git clone --depth 1 --branch v5 https://github.com/docker/build-push-action.git ${GITHUB_ACTIONS_PATH}/docker/build-push-action@v5 && \
    git clone --depth 1 --branch v3 https://github.com/docker/login-action.git ${GITHUB_ACTIONS_PATH}/docker/login-action@v3 && \
    echo "✅ All GitHub Actions cloned successfully"

# ============================================================
# LAYER 4: Terraform v1.9.0 with Pre-cached Providers
# ============================================================
ENV TERRAFORM_VERSION=1.9.0
ENV TERRAFORM_PLUGIN_CACHE_DIR=/terraform-plugins

RUN curl -OsL https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    unzip terraform_${TERRAFORM_VERSION}_linux_amd64.zip -d /usr/local/bin && \
    chmod +x /usr/local/bin/terraform && \
    rm terraform_${TERRAFORM_VERSION}_linux_amd64.zip

# Pre-cache Terraform providers in a single layer for faster builds
# Combines 4 provider downloads to reduce layer count
RUN mkdir -p \
      ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azurerm/4.57.0/linux_amd64 \
      ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azuread/3.7.0/linux_amd64 \
      ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/local/2.6.1/linux_amd64 \
      ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/null/3.2.4/linux_amd64 && \
    curl -L -o /tmp/azurerm.zip https://releases.hashicorp.com/terraform-provider-azurerm/4.57.0/terraform-provider-azurerm_4.57.0_linux_amd64.zip && \
    curl -L -o /tmp/azuread.zip https://releases.hashicorp.com/terraform-provider-azuread/3.7.0/terraform-provider-azuread_3.7.0_linux_amd64.zip && \
    curl -L -o /tmp/local.zip https://releases.hashicorp.com/terraform-provider-local/2.6.1/terraform-provider-local_2.6.1_linux_amd64.zip && \
    curl -L -o /tmp/null.zip https://releases.hashicorp.com/terraform-provider-null/3.2.4/terraform-provider-null_3.2.4_linux_amd64.zip && \
    unzip -q /tmp/azurerm.zip -d /tmp/azurerm_extracted && \
    unzip -q /tmp/azuread.zip -d /tmp/azuread_extracted && \
    unzip -q /tmp/local.zip -d /tmp/local_extracted && \
    unzip -q /tmp/null.zip -d /tmp/null_extracted && \
    mv /tmp/azurerm_extracted/terraform-provider-azurerm_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azurerm/4.57.0/linux_amd64/ && \
    mv /tmp/azuread_extracted/terraform-provider-azuread_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azuread/3.7.0/linux_amd64/ && \
    mv /tmp/local_extracted/terraform-provider-local_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/local/2.6.1/linux_amd64/ && \
    mv /tmp/null_extracted/terraform-provider-null_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/null/3.2.4/linux_amd64/ && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azurerm/4.57.0/linux_amd64/terraform-provider-azurerm_* && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azuread/3.7.0/linux_amd64/terraform-provider-azuread_* && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/local/2.6.1/linux_amd64/terraform-provider-local_* && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/null/3.2.4/linux_amd64/terraform-provider-null_* && \
    rm -rf /tmp/*.zip /tmp/*_extracted && \
    echo "✅ All Terraform providers cached successfully"

# ============================================================
# Working Directory & Health Check
# ============================================================
WORKDIR /github/workspace

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD az --version && sqlcmd --version && terraform --version && docker --version && java -version && google-chrome-stable --version && node --version

# ============================================================
# Configuration
# ============================================================
# Configure Terraform to use pre-cached plugins
ENV TF_PLUGIN_CACHE_DIR=${TERRAFORM_PLUGIN_CACHE_DIR}
ENV DEBIAN_FRONTEND=

# Configure git to use credentials from environment variables
# This allows act to authenticate with GitHub when cloning actions
# The GIT_TERMINAL_PROMPT=0 prevents git from prompting for credentials
ENV GIT_TERMINAL_PROMPT=0
RUN git config --global url."https://".insteadOf git:// && \
    git config --global --add credential.helper store

# Create an entrypoint script that sets up git credentials before running commands
RUN mkdir -p /root && printf '#!/bin/bash\nset -e\n\nif [ -n "$GITHUB_TOKEN" ]; then\n    echo "Configuring git credentials..."\n    echo "https://x-access-token:${GITHUB_TOKEN}@github.com" > /root/.git-credentials\n    chmod 600 /root/.git-credentials\nfi\n\nexec "$@"\n' > /root/entrypoint.sh && chmod +x /root/entrypoint.sh

ENTRYPOINT ["/root/entrypoint.sh"]
CMD ["/bin/bash"]
