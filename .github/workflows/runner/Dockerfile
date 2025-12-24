# Custom GitHub Actions Runner with Pre-installed Tools
# Base: Ubuntu 22.04 (matches GitHub Actions ubuntu-latest)
# Compatible with: act (local testing), GitHub Actions (cloud)
FROM ubuntu:22.04

# Metadata labels
LABEL org.opencontainers.image.title="Consilient GitHub Actions Runner"
LABEL org.opencontainers.image.description="Custom runner with Azure CLI, sqlcmd, and SchemaSpy pre-installed"
LABEL org.opencontainers.image.version="1.0.0"
LABEL org.opencontainers.image.tools="azure-cli=latest,sqlcmd=1.6.0,schemaspy=6.2.4"

# Prevent interactive prompts during package installation
ENV DEBIAN_FRONTEND=noninteractive

# Install system dependencies in a single layer
RUN apt-get update && \
    apt-get install -y \
        curl \
        ca-certificates \
        gnupg \
        lsb-release \
        tar \
        bzip2 \
        openjdk-11-jre-headless \
        git \
        jq \
    && rm -rf /var/lib/apt/lists/*

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
# Verification Layer
# ============================================================
RUN echo "=== Tool Verification ===" && \
    echo "Azure CLI: $(az --version | head -n 1)" && \
    echo "sqlcmd: $(sqlcmd --version)" && \
    echo "Java: $(java -version 2>&1 | head -n 1)" && \
    echo "SchemaSpy: $(ls -lh ${SCHEMASPY_PATH})" && \
    echo "========================="

# ============================================================
# Working Directory & Health Check
# ============================================================
WORKDIR /github/workspace

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD az --version && sqlcmd --version && java -version

ENV DEBIAN_FRONTEND=

CMD ["/bin/bash"]
