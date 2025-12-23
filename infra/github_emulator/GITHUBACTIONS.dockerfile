FROM mcr.microsoft.com/azure-cli:2.59.0

# Install Terraform
ENV TERRAFORM_VERSION=1.9.0
RUN apk update && \
    apk add --no-cache wget unzip gnupg curl openjdk17-jre

# Install Node.js (LTS)
RUN apk add --no-cache nodejs npm

# Install Terraform binary
RUN wget https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    unzip terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    mv terraform /usr/local/bin/ && \
    rm terraform_${TERRAFORM_VERSION}_linux_amd64.zip

# Pre-cache Terraform providers in the global plugin directory
RUN mkdir -p /root/.terraform.d/plugin-cache && \
    mkdir -p /tmp/terraform-init && \
    cd /tmp/terraform-init && \
    echo 'terraform {' > main.tf && \
    echo '  required_providers {' >> main.tf && \
    echo '    azurerm = {' >> main.tf && \
    echo '      source  = "hashicorp/azurerm"' >> main.tf && \
    echo '      version = "4.57.0"' >> main.tf && \
    echo '    }' >> main.tf && \
    echo '    azuread = {' >> main.tf && \
    echo '      source  = "hashicorp/azuread"' >> main.tf && \
    echo '      version = "3.7.0"' >> main.tf && \
    echo '    }' >> main.tf && \
    echo '  }' >> main.tf && \
    echo '}' >> main.tf && \
    TF_PLUGIN_CACHE_DIR=/root/.terraform.d/plugin-cache terraform init && \
    cd / && \
    rm -rf /tmp/terraform-init

# Set Terraform plugin cache directory
ENV TF_PLUGIN_CACHE_DIR=/root/.terraform.d/plugin-cache

# Install sqlcmd (latest version)
ENV SQLCMD_VERSION=1.9.0
RUN wget -O /tmp/sqlcmd.tar.bz2 https://github.com/microsoft/go-sqlcmd/releases/download/v${SQLCMD_VERSION}/sqlcmd-linux-amd64.tar.bz2 && \
    mkdir -p /opt/sqlcmd && \
    tar -xjf /tmp/sqlcmd.tar.bz2 -C /opt/sqlcmd && \
    mv /opt/sqlcmd/sqlcmd /usr/local/bin/sqlcmd && \
    chmod +x /usr/local/bin/sqlcmd && \
    rm -rf /tmp/sqlcmd.tar.bz2 /opt/sqlcmd

# Install SchemaSpy
ENV SCHEMASPY_VERSION=6.2.4
RUN mkdir -p /opt/schemaspy && \
    wget -O /opt/schemaspy/schemaspy.jar https://github.com/schemaspy/schemaspy/releases/download/v${SCHEMASPY_VERSION}/schemaspy-${SCHEMASPY_VERSION}.jar

RUN apk add --no-cache tar

RUN rm -rf /var/cache/apk/*