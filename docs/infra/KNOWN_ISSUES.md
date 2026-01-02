# Known Issues & Opportunities

Track current issues, limitations, and improvement opportunities for the Consilient infrastructure.

## Active Issues

(No active issues at this time)

---

## Resolved Issues

### .actrc Path Configuration Mismatch (RESOLVED)

**Status:** ✅ RESOLVED
**Resolution Date:** 2025-12-25

The `.actrc` file and `.secrets` file have been removed as part of dotfile cleanup. The `run-act.ps1` script now owns all act configuration, eliminating confusion about multiple configuration sources.

---

## Limitations

### Terraform Backend

**Issue:** Terraform state is local (not remote)

**Description:**
Currently, Terraform state is stored locally in `infra/terraform/terraform.tfstate`. This works for single-developer scenarios but has limitations for teams.

**Impact:**
- Team members can't share state safely
- No state history/versioning
- Risk of concurrent modifications

**Current Status:** Acceptable for dev/staging; not recommended for production

**Solution Path:** Migrate to Azure Storage backend (template in [`backend.tf`](../../../infra/terraform/backend.tf))

---

### Act Tool Limitations

**Issue:** Local testing has some differences from GitHub Actions

**Differences:**
- Docker-in-Docker may not work (nested containers)
- Network access limited to host only
- Some GitHub Actions features not supported
- Path handling (Windows vs Linux) can differ

**Impact:** Occasional "works locally, fails in cloud" scenarios

**Mitigation:** Always validate in GitHub Actions before merging

**Related Documentation:** [components/local-testing.md#limitations](components/local-testing.md#limitations)

---

### Azure Service Limitations

**Issue:** Container App Environment (CAE) free tier has constraints

**Details:**
- Shared CAE across environments (save costs, but less isolation)
- No auto-scaling in free tier
- Limited memory/CPU

**Impact:** Performance may vary in high-load scenarios

**Current Strategy:** Use shared CAE for dev/staging, premium for production

**Configuration:** [`infra/terraform/locals.tf:use_shared_container_environment`](../../../infra/terraform/locals.tf#L85-L120)

---

## Performance Considerations

### Terraform Import Loop (RESOLVED)

**Status:** ✅ FIXED

**Problem (Historical):**
- Initial terraform runs took ~5 minutes due to repeated resource imports
- Without state persistence, Terraform thought every run was fresh deployment

**Solution Implemented:**
- Added `--bind` flag to `run-act.ps1` to mount workspace
- State files now persist between act runs
- Performance improved 5x (5 minutes → 1 minute)

**Verification:**
```powershell
# The --bind flag is automatically enabled in run-act.ps1
# Check that state persists between runs
ls -la infra/terraform/terraform.tfstate*
```

**Related Documentation:** [infra/act/IMPORT_LOOP_FIX.md](../../../infra/act/IMPORT_LOOP_FIX.md) and [components/local-testing.md](components/local-testing.md#performance-optimization)

---

### Custom Runner Image Build Time

**Status:** Accepted limitation

**Description:**
Custom runner image builds take ~5-10 minutes (first time)

**Why:**
- Installing Terraform + providers
- Installing Azure CLI, sqlcmd, SchemaSpy, Java, Node.js
- Layer caching helps subsequent builds

**Mitigation:**
- Image is cached after first build
- Providers are pre-cached in Docker layers
- Rarely need to rebuild

**When to Rebuild:**
- Update `.github/workflows/runner/Dockerfile`
- Change tool versions
- Add new pre-installed tools

---

### Database Deployment Time

**Status:** Acceptable

**Description:**
Database deployment takes 2-5 minutes per environment

**Factors:**
- SQL script execution time
- Table creation and indexing
- Foreign key constraint setup

**Optimization:**
- Matrix jobs deploy databases in parallel (faster than serial)
- Pre-optimize SQL scripts

---

## Configuration Gaps

### Manual Steps Still Required

**Issue:** Some setup steps can't be automated

**Manual Steps:**
1. Create service principal (if not using OIDC)
2. Create Azure resources (first bootstrap)
3. Configure GitHub secrets (can't be done via API easily)
4. Set resource tags for cost tracking

**Why:** Security (no scripts with credentials) and Azure RBAC complexity

**Future:** Could create setup wizard/script (not yet available)

---

## Future Enhancements

### 1. Remote Terraform State Backend

**Priority:** High
**Effort:** Medium
**Impact:** Enables team collaboration

**Implementation:**
- Uncomment configuration in [`backend.tf`](../../../infra/terraform/backend.tf)
- Create Azure Storage Account for state
- Configure state lock blob

**Benefits:**
- Team members can work on infrastructure
- State history and versioning
- State locking prevents conflicts

---

### 2. Database Migration Versioning System

**Priority:** Medium
**Effort:** High
**Impact:** Better schema management

**Current Limitation:**
- Scripts execute every time (based on filename order)
- No versioning or rollback mechanism

**Proposed Solution:**
- Implement Flyway or Liquibase
- Track applied migrations
- Support rollback capability

**Example:** `2024-12-01_001_add_user_email.sql`

---

### 3. Automated Secret Rotation

**Priority:** High
**Effort:** Medium
**Impact:** Improved security

**Current State:**
- Secrets managed manually
- No automated rotation

**Proposed Solution:**
- Use Azure Key Vault
- Implement secret rotation Lambda/Function
- Auto-update GitHub secrets

**Timeline:** Q1 2026

---

### 4. Enhanced Monitoring & Alerting

**Priority:** Medium
**Effort:** Medium
**Impact:** Better operational visibility

**Current State:**
- Grafana dashboards exist
- Limited alerting configured

**Enhancements:**
- Cost alerts (warn if exceeding budget)
- Performance alerts (API response time, database CPU)
- Automated rollback triggers
- Slack/Teams integration

---

### 5. Cost Optimization Automation

**Priority:** Medium
**Effort:** Low
**Impact:** Reduce infrastructure costs

**Opportunities:**
- Auto-stop resources after hours (dev environments)
- Use spot instances where applicable
- Auto-scale databases based on usage
- Reserved instances for production

**Estimated Savings:** 20-30% monthly cost reduction

---

### 6. Infrastructure Compliance & Scanning

**Priority:** High
**Effort:** Medium
**Impact:** Security and compliance

**Additions Needed:**
- Terraform linting (tflint)
- Security scanning (checkov)
- Compliance reporting (CIS benchmarks)
- HIPAA/SOC2 audit logs

---

## Technical Debt

### 1. Terraform Code Organization

**Issue:** Some files are large (e.g., `locals.tf` is 235 lines)

**Solution:** Break into smaller modules
- `modules/naming/` - Naming logic
- `modules/security/` - Security configurations
- `modules/cost-management/` - Cost profiles

---

### 2. GitHub Workflows Duplication

**Issue:** Some step logic repeated across workflows

**Solution:** Extract to shared composite actions
- `composite-action-deploy-docker` - Build and deploy pattern
- `composite-action-health-check` - Health check logic
- `composite-action-rollback` - Rollback mechanism

**Current Actions:** 4 composite actions (good baseline)
**Potential:** Could consolidate to 3-4 more

---

### 3. Documentation Updates Needed

**Issue:** Some docs are outdated

**Files to Update:**
- [components/terraform.md](components/terraform.md) - Last updated December 2025
- [components/github-actions.md](components/github-actions.md) - Schema updates needed
- Code comments in workflow files - Outdated in places

**Schedule:** Quarterly review

---

## Improvement Opportunities

### 1. Better Error Messages

**Current State:** Some error messages are cryptic

**Improvements:**
- Add context-aware error messages in scripts
- Link to troubleshooting docs
- Suggest fixes based on error type

**Example:** Instead of `Error: client_id not found`, show:
```
Error: AZURE_CLIENT_ID secret not found

This is required for OIDC authentication.

Solutions:
1. Add AZURE_CLIENT_ID to GitHub Secrets
2. See reference/secrets-checklist.md for instructions
3. See TROUBLESHOOTING.md#secret-validation-errors
```

---

### 2. Enhanced Logging & Debugging

**Current State:** Standard log output

**Enhancements:**
- Structured logging (JSON format)
- Log levels (debug, info, warn, error)
- Log aggregation to Loki
- Correlation IDs across requests

**Tools:** Already have Loki configured; use it better

---

### 3. Workflow Optimization Possibilities

**Current State:** Workflows work well

**Possible Optimizations:**
- Parallel database deployments (already doing)
- Cache Docker layers
- Skip unchanged tests
- Matrix strategy for multi-environment deploys

---

### 4. Security Hardening

**Current State:** OIDC implemented, good baseline

**Enhancements:**
- MFA enforcement on service principals
- Subnet NSGs (network security groups)
- WAF (Web Application Firewall)
- Network segmentation
- Secrets scanning in pre-commit hooks

---

### 5. Cost Reduction Strategies

**Current State:** Cost-conscious design (3 tiers)

**Opportunities:**
- Scheduled auto-shutdown (dev environments)
- Reserved instances for prod (save 30%)
- Database auto-pause during off-hours
- App Service scale-down at night
- Estimated savings: 25-40% annually

---

## Community Contributions Wanted

### 1. Terraform Module Library

**Help Wanted:** Create reusable modules
- Networking (VNet + Subnets)
- Database (SQL Server + DBs)
- Monitoring stack
- Publish to Terraform Registry

---

### 2. GitHub Actions Enhancements

**Help Wanted:** Improve workflows
- Add cost estimation to `terraform plan` output
- Improve health check logic
- Add database backup automation
- Enhanced security scanning

---

### 3. Documentation Improvements

**Help Wanted:** Expand docs
- Video tutorials
- Interactive runbooks
- More troubleshooting scenarios
- Cost optimization guides

---

### 4. Performance Improvements

**Help Wanted:** Optimize performance
- Profile slow workflows
- Identify bottlenecks
- Suggest caching strategies
- Benchmark against other IaC tools

---

### 5. Integration Improvements

**Help Wanted:** Better integrations
- Slack/Teams notifications
- PagerDuty alerts
- Datadog monitoring
- Cost tracking dashboards

---

## Tracking & Updates

### Status Definitions

- **Active:** Currently affects operations, needs attention
- **Acknowledged:** Known but not being worked on
- **Planned:** Will be fixed in future release
- **Deferred:** Deprioritized for now
- **Resolved:** Fixed/implemented ✅

### Update Schedule

- **Weekly:** Check for new issues in GitHub Issues
- **Monthly:** Review and update this document
- **Quarterly:** Plan major improvements
- **Annually:** Strategic review and roadmap

### How to Report Issues

1. Check if issue already listed here
2. Verify against [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
3. Create GitHub Issue with:
   - Error message
   - Steps to reproduce
   - Expected vs actual behavior
   - Link to relevant docs

---

## Related Documentation

- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Problem solving guide
- [QUICK_START.md](QUICK_START.md) - Fast onboarding
- [components/](components/) - Detailed component guides
- [reference/](reference/) - Quick references

---

**Last Updated:** December 2025
**Maintainer:** Infrastructure Team
**Next Review:** January 2026
