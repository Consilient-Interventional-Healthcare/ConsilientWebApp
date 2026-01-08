# Monitoring & Observability

<!-- AI_CONTEXT: Monitoring infrastructure using Grafana Loki (logs) and Azure Managed Grafana (visualization). Per-environment deployment. -->

## For Non-Technical Stakeholders

Monitoring captures all application logs in one place and provides dashboards to visualize system health. When something goes wrong, logs tell us exactly what happened and when. This helps us fix problems faster and prevent future issues.

---

## Architecture

<!-- AI_CODE: Loki configuration from loki.tf -->

### Components

**Grafana Loki:** Log aggregation system running on Azure Container Apps
- Collects logs from API and React applications
- Stores logs in Azure Storage (blob storage)
- Provides query interface for searching logs

**Azure Managed Grafana:** Visualization and dashboard platform
- Version 11 (latest major version)
- Connects to Loki as data source
- Creates dashboards and alerts

**Storage:** Azure Storage Account for log persistence
- Private endpoint for secure access
- Container: `loki-data`
- Retention: 30 days (configurable via `var.loki_retention`)

### Configuration Files

- **Terraform:** [`loki.tf`](../../../infra/terraform/loki.tf) - Container App deployment
- **Terraform:** [`grafana.tf`](../../../infra/terraform/grafana.tf) - Managed Grafana
- **Terraform:** [`storage.tf`](../../../infra/terraform/storage.tf) - Log storage

---

## Resource Details

<!-- AI_TABLE: Loki resource specifications per environment -->

| Resource | Configuration | Environment |
|----------|---------------|-------------|
| Container App | consilient-loki-{env} | Per environment (dev, prod) |
| Image | grafana/loki:2.9.3 | All environments |
| CPU | 0.5 cores (request), 1.0 (limit) | Configurable |
| Memory | 1.0Gi (request), 2.0Gi (limit) | Configurable |
| Storage Account | consilientloki{env}{hash} | Per environment |
| Container App Environment | consilient-cae-{env} | Per environment |
| Data Retention | 30 days | Configurable |

### Configuration Variables

- `var.loki_cpu_request` - Default: 0.5
- `var.loki_cpu_limit` - Default: 1.0
- `var.loki_memory_request` - Default: "1.0Gi"
- `var.loki_memory_limit` - Default: "2.0Gi"
- `var.loki_retention` - Default: "30d"
- `var.grafana_public_network_access` - Default: false (set to true for debugging access from internet)

---

## Setup & Configuration

### Accessing Grafana

1. Navigate to Azure Portal → Azure Managed Grafana
2. Find resource: `consilient-grafana-{environment}`
3. Click "Endpoint" URL to open Grafana UI
4. Authenticate with Azure AD

### Adding Loki Data Source

The Loki data source is automatically configured by Terraform. Verify it's connected:

```yaml
# Loki data source configuration (auto-configured by Terraform)
name: Loki
type: loki
url: http://consilient-loki-{env}.<container-app-domain>
```

**Loki URL is stored in Key Vault:**
- Secret name: `grafana-loki-url`
- Reference: [`keyvault.tf`](../../../infra/terraform/keyvault.tf)

### Creating Dashboards

**Sample LogQL Query:**
```logql
{app="consilient-api"} |= "error" | json
```

**Common Queries:**
- All API errors: `{app="consilient-api"} |= "error"`
- React app logs: `{app="consilient-react"}`
- Last hour errors: `{app="consilient-api"} |= "error" [1h]`
- Specific error type: `{app="consilient-api"} |= "NullReferenceException"`

---

## Log Integration

### Application Logging

Both API and React applications are configured to send logs to Loki:

**API (.NET):**
- Configured via app configuration
- Uses Loki sink via Serilog
- Labels: `app=consilient-api`, environment

**React:**
- Client-side error logging
- Sends to API which forwards to Loki
- Labels: `app=consilient-react`, environment

### Log Labels

All logs include standard labels for filtering:
- `app` - Application name (consilient-api, consilient-react)
- `environment` - Deployment environment (dev, prod)
- `level` - Log level (info, warning, error)
- `timestamp` - When the log occurred

---

## Troubleshooting

### Logs Not Appearing

**Symptoms:** No logs in Grafana from applications

**Diagnosis:**
1. Check Loki container is running:
   ```bash
   az containerapp show --name consilient-loki-{env} --resource-group {rg-name}
   ```
2. Verify storage account access
3. Check application logging configuration

**Solution:**
```bash
# Restart Loki container app
az containerapp revision restart \
  --name consilient-loki-{env} \
  --resource-group {rg-name}
```

### Storage Quota Exceeded

**Symptoms:** Loki stops ingesting logs, "Storage quota exceeded" error

**Diagnosis:** Check storage account usage in Azure Portal

**Solution:**
- Reduce retention period: Update `var.loki_retention` in terraform.tfvars
- Increase storage quota
- Archive old logs to cheaper tier (cool/archive)

### Performance Issues

**Symptoms:** Slow query responses in Grafana, timeouts

**Diagnosis:**
- Check Loki CPU/memory usage in Container Apps metrics
- Review query complexity (large time ranges, many labels)

**Solution:**
```hcl
# Increase Loki resources in terraform.tfvars
loki_cpu_request    = 1.0
loki_memory_request = "2.0Gi"
```

Then apply: `terraform apply`

### Connection Refused

**Symptoms:** "Connection refused" when accessing Grafana

**Diagnosis:**
- Grafana not running or restarting
- Network connectivity issues
- Managed Grafana disabled

**Solution:**
1. Check Grafana status: `az grafana show --name consilient-grafana-{env} --resource-group {rg-name}`
2. Wait for startup if recently restarted (5-10 minutes)
3. Verify you have Azure AD permissions to access

---

## Cost Implications

<!-- AI_TABLE: Monitoring resource costs -->

| Resource | Dev Cost | Prod Cost |
|----------|----------|-----------|
| Container App (Loki) | ~$15/mo | ~$15/mo |
| Storage Account (logs) | ~$1/mo | ~$5/mo |
| Managed Grafana | ~$100/mo | ~$100/mo |
| **Total** | **~$116/mo** | **~$120/mo** |

<!-- AI_NOTE: Monitoring is same cost for both environments. Consider this when budgeting. -->

---

## Best Practices

1. **Retention:** Keep 30 days in prod, can reduce to 7 days in dev to save costs
2. **Alerting:** Set up alerts for error rates > 5% in prod
3. **Dashboards:** Create separate dashboards for API, React, and system health
4. **Labels:** Always include app and environment labels for filtering
5. **Queries:** Use time ranges appropriate for your debugging (last 1h for current issues, last 7d for trends)

---

## Related Documentation

- [components/azure-resources.md](azure-resources.md) - Container Apps and storage details
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System overview
- [TROUBLESHOOTING.md](../TROUBLESHOOTING.md) - General troubleshooting
- [Grafana Official Docs](https://grafana.com/docs/) - Advanced Grafana features
- [Loki Official Docs](https://grafana.com/docs/loki/) - Advanced Loki queries

---

**Last Updated:** January 2026
**Related Files:**
- [`infra/terraform/loki.tf`](../../../infra/terraform/loki.tf)
- [`infra/terraform/grafana.tf`](../../../infra/terraform/grafana.tf)
- [`infra/terraform/keyvault.tf`](../../../infra/terraform/keyvault.tf)
