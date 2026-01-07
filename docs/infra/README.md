# Consilient Infrastructure Documentation

<!-- AI_CONTEXT: Main entry point for infrastructure docs. All navigation starts here. Two environments: dev ($45/mo) and prod ($2,800/mo). -->

## For Non-Technical Stakeholders

This infrastructure runs Consilient's web application on Microsoft Azure. The code automatically builds and deploys the application whenever developers push changes. All infrastructure is defined in code (Terraform), meaning it's versioned, reviewable, and can be recreated with a single command.

**Quick Facts:**
- Two environments: Development ($45/month) and Production ($2,800/month)
- Automated deployments via GitHub Actions (no manual steps)
- Zero-downtime deployments with automatic health checks
- All secrets managed securely in Azure Key Vault

---

## Quick Navigation

**New to infrastructure?** → [QUICK_START.md](QUICK_START.md)
**System design overview?** → [ARCHITECTURE.md](ARCHITECTURE.md)
**Something broken?** → [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
**Known limitations?** → [KNOWN_ISSUES.md](KNOWN_ISSUES.md)

## Documentation Structure

<!-- AI_TABLE: Documentation organized by purpose and audience -->

### Components (Deep Technical Guides)

How each infrastructure component works and how to use it:

| Component | Description |
|-----------|-------------|
| [Terraform](components/terraform.md) | Infrastructure as Code - defines all Azure resources |
| [GitHub Actions](components/github-actions.md) | CI/CD automation and deployment workflows |
| [Databases](components/databases.md) | SQL database deployment and auto-discovery |
| [Database Documentation](components/database-documentation.md) | SchemaSpy HTML documentation generation |
| [Authentication](components/authentication.md) | OIDC and Service Principal authentication |
| [Local Testing](components/local-testing.md) | Testing workflows locally with act |
| [Azure Resources](components/azure-resources.md) | Azure service details and configuration |
| [Monitoring](components/monitoring.md) | Grafana Loki logging and monitoring |

### Reference (Quick Lookup Tables)

Fast reference for values, commands, and configurations:

| Reference | Description |
|-----------|-------------|
| [Secrets & Variables](reference/secrets-variables.md) | All GitHub secrets/variables in one table |
| [Naming Conventions](reference/naming-conventions.md) | Resource naming patterns and multi-tier strategy |
| [Code References](reference/code-references.md) | File index with line numbers |
| [Cost Management](reference/cost-management.md) | Cost estimates and optimization strategies |
| [Workflows Reference](reference/workflows-reference.md) | Quick catalog of all workflows |
| [Resources Reference](reference/resources-reference.md) | Azure resource inventory |

---

## Common Tasks

<!-- AI_CONTEXT: Most frequent developer operations -->

**Deploy infrastructure:** [QUICK_START.md#1-deploy-infrastructure-to-dev](QUICK_START.md#1-deploy-infrastructure-to-dev)

**Deploy database changes:** [QUICK_START.md#2-deploy-a-database-change](QUICK_START.md#2-deploy-a-database-change)

**Test workflows locally:** [QUICK_START.md#3-test-workflows-locally-with-act](QUICK_START.md#3-test-workflows-locally-with-act)

**Configure secrets:** [QUICK_START.md#6-configure-github-secrets](QUICK_START.md#6-configure-github-secrets)

**Troubleshoot errors:** [TROUBLESHOOTING.md](TROUBLESHOOTING.md)

---

**Last Updated:** January 2026
**Version:** 1.0 (First consolidated version)
