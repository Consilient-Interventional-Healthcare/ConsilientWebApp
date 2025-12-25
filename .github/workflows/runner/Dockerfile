# Custom GitHub Actions Runner with Pre-installed Tools
# Base: catthehacker/ubuntu:act-22.04 (optimized for act + GitHub Actions)
# This base image includes Node.js 20, Docker CLI, git, and common tools
# Compatible with: act (local testing), GitHub Actions (cloud)
FROM ghcr.io/catthehacker/ubuntu:act-22.04

# Metadata labels
LABEL org.opencontainers.image.title="Consilient GitHub Actions Runner"
LABEL org.opencontainers.image.description="Custom runner based on catthehacker/ubuntu:act-22.04 with Azure CLI, sqlcmd, and SchemaSpy"
LABEL org.opencontainers.image.version="2.0.0"
LABEL org.opencontainers.image.tools="azure-cli=latest,sqlcmd=1.6.0,schemaspy=6.2.4,terraform=1.9.0,nodejs=20,docker-cli=latest,java=17"

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
#
# Note: Docker CLI requires /var/run/docker.sock to be mounted
#       when running this container to access the host's Docker daemon
# ============================================================

# Prevent interactive prompts during package installation
ENV DEBIAN_FRONTEND=noninteractive

# Install only Java (required for SchemaSpy)
# Other system dependencies are already in the catthehacker base image
RUN apt-get update && \
    apt-get install -y openjdk-17-jre-headless && \
    rm -rf /var/lib/apt/lists/*

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
# LAYER 3: SchemaSpy 6.2.4 + JDBC Driver 12.4.2
# ============================================================
ENV SCHEMASPY_VERSION=6.2.4
ENV JDBC_VERSION=12.4.2
ENV SCHEMASPY_PATH=/opt/schemaspy

RUN mkdir -p ${SCHEMASPY_PATH} && \
    curl -L -o ${SCHEMASPY_PATH}/schemaspy.jar \
        "https://github.com/schemaspy/schemaspy/releases/download/v${SCHEMASPY_VERSION}/schemaspy-${SCHEMASPY_VERSION}.jar" && \
    curl -L -o ${SCHEMASPY_PATH}/mssql-jdbc.jar \
        "https://repo1.maven.org/maven2/com/microsoft/sqlserver/mssql-jdbc/${JDBC_VERSION}.jre11/mssql-jdbc-${JDBC_VERSION}.jre11.jar"

# Add SchemaSpy to PATH
ENV PATH="${SCHEMASPY_PATH}:${PATH}"

# ============================================================
# LAYER 4: Terraform v1.9.0 with Pre-cached Providers
# ============================================================
ENV TERRAFORM_VERSION=1.9.0
ENV TERRAFORM_PLUGIN_CACHE_DIR=/terraform-plugins

RUN curl -OsL https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    unzip terraform_${TERRAFORM_VERSION}_linux_amd64.zip -d /usr/local/bin && \
    rm terraform_${TERRAFORM_VERSION}_linux_amd64.zip

# Pre-cache Terraform providers to avoid downloading during terraform init
# This significantly speeds up terraform init in CI/CD workflows

# azurerm v4.57.0
RUN mkdir -p ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azurerm/4.57.0/linux_amd64 && \
    curl -L -o /tmp/azurerm.zip \
      https://releases.hashicorp.com/terraform-provider-azurerm/4.57.0/terraform-provider-azurerm_4.57.0_linux_amd64.zip && \
    unzip -q /tmp/azurerm.zip -d /tmp/azurerm_extracted && \
    mv /tmp/azurerm_extracted/terraform-provider-azurerm_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azurerm/4.57.0/linux_amd64/ && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azurerm/4.57.0/linux_amd64/terraform-provider-azurerm_* && \
    rm -rf /tmp/azurerm.zip /tmp/azurerm_extracted

# azuread v3.7.0
RUN mkdir -p ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azuread/3.7.0/linux_amd64 && \
    curl -L -o /tmp/azuread.zip \
      https://releases.hashicorp.com/terraform-provider-azuread/3.7.0/terraform-provider-azuread_3.7.0_linux_amd64.zip && \
    unzip -q /tmp/azuread.zip -d /tmp/azuread_extracted && \
    mv /tmp/azuread_extracted/terraform-provider-azuread_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azuread/3.7.0/linux_amd64/ && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/azuread/3.7.0/linux_amd64/terraform-provider-azuread_* && \
    rm -rf /tmp/azuread.zip /tmp/azuread_extracted

# local v2.6.1
RUN mkdir -p ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/local/2.6.1/linux_amd64 && \
    curl -L -o /tmp/local.zip \
      https://releases.hashicorp.com/terraform-provider-local/2.6.1/terraform-provider-local_2.6.1_linux_amd64.zip && \
    unzip -q /tmp/local.zip -d /tmp/local_extracted && \
    mv /tmp/local_extracted/terraform-provider-local_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/local/2.6.1/linux_amd64/ && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/local/2.6.1/linux_amd64/terraform-provider-local_* && \
    rm -rf /tmp/local.zip /tmp/local_extracted

# null v3.2.4
RUN mkdir -p ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/null/3.2.4/linux_amd64 && \
    curl -L -o /tmp/null.zip \
      https://releases.hashicorp.com/terraform-provider-null/3.2.4/terraform-provider-null_3.2.4_linux_amd64.zip && \
    unzip -q /tmp/null.zip -d /tmp/null_extracted && \
    mv /tmp/null_extracted/terraform-provider-null_* ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/null/3.2.4/linux_amd64/ && \
    chmod +x ${TERRAFORM_PLUGIN_CACHE_DIR}/registry.terraform.io/hashicorp/null/3.2.4/linux_amd64/terraform-provider-null_* && \
    rm -rf /tmp/null.zip /tmp/null_extracted

# ============================================================
# Verification Layer
# ============================================================
# Note: Docker CLI is already included in the catthehacker base image
RUN echo "=== Tool Verification ===" && \
    echo "Azure CLI: $(az --version | head -n 1)" && \
    echo "sqlcmd: $(sqlcmd --version)" && \
    echo "Terraform: $(terraform --version | head -n 1)" && \
    echo "Docker CLI: $(docker --version)" && \
    echo "Java: $(java -version 2>&1 | head -n 1)" && \
    echo "Node.js: $(node --version)" && \
    echo "npm: $(npm --version)" && \
    echo "SchemaSpy: $(ls -lh ${SCHEMASPY_PATH})" && \
    echo "========================="

# ============================================================
# Working Directory & Health Check
# ============================================================
WORKDIR /github/workspace

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD az --version && sqlcmd --version && terraform --version && docker --version && java -version && node --version

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
