# Database Deployment & Management

<!-- AI_CONTEXT: SQL database deployment automation. Auto-discovery from src/Databases/. Matrix-based parallel deployment per database. Azure AD authentication. -->

## For Non-Technical Stakeholders

Database changes are deployed automatically when you push SQL scripts to the repository. The system discovers your database scripts, executes them in order, and verifies the deployment succeeded. Each database is deployed in parallel for speed. No manual scripts or connections needed.

---

Complete guide to database deployment automation via GitHub Actions.

## Quick Reference

**Auto-Discovery Pattern:**
- Scripts location: [`src/Databases/`](../../../src/Databases/)
- Workflow: [`.github/workflows/databases.yml`](../../../.github/workflows/databases.yml)
- Naming: `{folder}_{environment}` (Main → consilient_main_dev)
- Execution: Parallel matrix jobs per database

## Directory Structure

```
src/Databases/
├── Main/
│   ├── Schema/
│   │   ├── 001_create_users.sql
│   │   ├── 002_add_email.sql
│   │   └── ...NNN_*.sql (executed in order)
│   └── Seeds/
│       └── seed_*.sql (only with recreate flag in dev)
│
├── Hangfire/
│   └── Schema/
│       └── 001_create_jobs.sql
│
└── CustomDB/
    └── Schema/
        └── 001_setup.sql
```

## Naming Convention

| Directory | Database Created | Environment |
|-----------|-----------------|------------|
| `Main` | `consilient_main_dev` | Dev |
| `Main` | `consilient_main_prod` | Prod |
| `Hangfire` | `consilient_hangfire_dev` | Dev |
| `Hangfire` | `consilient_hangfire_prod` | Prod |
| `CustomDB` | `consilient_customdb_dev` | Dev |

## Auto-Discovery Mechanism

**Flow:**
1. Discover databases (find directories in `src/Databases/`)
2. Create JSON array of discovered database names
3. For each database (matrix job, parallel):
   - Create database if not exists
   - Execute schema scripts in order (NNN_*.sql)
   - Optionally include seed data (seed_*.sql)
   - Verify deployment

**Code:** [`.github/workflows/databases.yml:45-60`](../../../.github/workflows/databases.yml#L45-L60)

## SQL Script Rules

**Schema Scripts:** `NNN_*.sql`
- Executed in numeric order (001, 002, etc.)
- Always execute (on every deployment)
- Example: `001_create_users_table.sql`

**Seed Scripts:** `seed_*.sql`
- Test data only
- Skip by default
- Include only with `recreate_database_objects=true`
- Example: `seed_default_users.sql`

**System Scripts:** `_*.sql` or `.*`
- Skipped (documentation, temp files)
- Example: `_notes.sql` or `.backup.sql`

## Authentication

**Method:** Azure AD (no SQL passwords in workflows)

**Command:**
```bash
sqlcmd -S {server} -d {database} -G -i {script.sql}
```

**Flags:**
- `-G` - Azure AD authentication (uses logged-in context)
- `-S` - Server FQDN
- `-d` - Database name
- `-i` - Input script file

**Setup:** Login via `azure-login` composite action using OIDC

See [components/authentication.md](authentication.md) for auth details.

## Deployment Pipeline

**Process:**
1. **Trigger** - Push to main or manual workflow dispatch
2. **Validate** - Check secrets configured
3. **Azure Login** - OIDC authentication
4. **Discover** - Find databases in `src/Databases/`
5. **Deploy** - For each database (matrix):
   - Create database (if missing)
   - Get list of scripts to apply
   - Execute each script with sqlcmd
   - Verify: check tables, counts, constraints
6. **Verify** - List deployed databases and summary
7. **Notify** - Report success/failure

**Time:** ~2-5 minutes total

## Environment-Specific Behavior

### Dev Environment
- **Recreate Allowed:** Yes (with flag)
- **Drop Objects:** Can drop all objects and recreate
- **Seed Data:** Included in deployment
- **Backup:** Optional

### Production Environment
- **Recreate Allowed:** No (protection)
- **Destructive Operations:** Blocked
- **Seed Data:** Not applied
- **Backup:** Required before changes

## Manual Deployment

For local testing:

```powershell
# Set variables
$server = "your-server.database.windows.net"
$db = "consilient_main_dev"
$script = "src/Databases/Main/Schema/001_create_users.sql"

# Execute with Azure AD auth
sqlcmd -S $server -d $db -G -i $script
```

**Prerequisites:**
- sqlcmd installed
- Azure CLI logged in (`az login`)
- SQL database exists

## Matrix Jobs & Parallelization

**Benefit:** Deploy multiple databases simultaneously

**Example:**
```
Main database deploy ─┐
                      ├─ All 3 databases in parallel
Hangfire deploy ──────┤   Typical time: 2-3 minutes
                      │   Without parallelization: 5+ minutes
Custom deploy ────────┘
```

## Verification Steps

**Performed After Deployment:**
1. List tables in each database
2. Count records in key tables
3. Check foreign key constraints
4. Verify indexes created
5. Confirm no errors in logs

## Automated Documentation Generation

Interactive HTML documentation is automatically generated for your database schemas using SchemaSpy, an open-source reverse-engineering tool.

**Workflow:** [`.github/workflows/docs_db.yml`](../../../.github/workflows/docs_db.yml)

**Configuration:** Per-database control via [`db_docs.yml`](../../../src/Databases/)

**What Gets Generated:**
- Interactive schema diagrams (entity relationship diagrams)
- Table and column documentation
- Foreign key relationships and constraints
- Indexes and keys information
- Navigable HTML documentation with search

**How It Works:**
1. Database discovery: Scan `src/Databases/` for configuration files
2. Schema discovery: Query database for user-created schemas
3. Configuration filtering: Apply exclusions from `db_docs.yml`
4. Documentation generation: SchemaSpy generates HTML for each schema (parallel)
5. Artifact upload: Documentation packaged as workflow artifact

**Schema Exclusion:**
Control which schemas are documented via the `schemas.exclude` list in your database's `db_docs.yml` configuration file. This allows you to exclude internal, temporary, or staging schemas from the documentation.

**Example Configuration:**
```yaml
database:
  name: "ConsilientDB"
  generate_docs: true

schemas:
  exclude:
    - "internal_schema"
    - "temp_schema"
```

See [components/database-documentation.md](database-documentation.md) for complete guide, configuration options, and troubleshooting.

---

## Terraform Integration

**Database Resources:** [`sql.tf`](../../../infra/terraform/sql.tf)

**Created by Terraform:**
- SQL Server
- Databases (initial empty databases)
- Firewall rules
- Backup policies
- Auditing configuration

**Updated by GitHub Actions:**
- Schema (via SQL scripts)
- Data (via seed scripts)

**Two-Phase Approach:**
1. Terraform creates infrastructure
2. GitHub Actions populates databases

## Troubleshooting

**Common Issues:**
- Database not found → Terraform hasn't created it yet
- Script syntax error → Test locally with sqlcmd
- Permission denied → Check Azure AD authentication
- Script timeout → Increase timeout (default 600s)

See [TROUBLESHOOTING.md#database-deployment](../TROUBLESHOOTING.md#database-deployment) for detailed solutions.

## Related Documentation

- [components/terraform.md](terraform.md) - SQL Server creation
- [components/authentication.md](authentication.md) - Auth details
- [components/database-documentation.md](database-documentation.md) - Automated documentation generation
- [ARCHITECTURE.md#database-deployment-flow](../ARCHITECTURE.md#database-deployment-flow) - Database deployment process diagram
- [ARCHITECTURE.md#database-documentation-generation-flow](../ARCHITECTURE.md#database-documentation-generation-flow) - Documentation generation process
- [DATABASE_DEPLOYMENT.md](../../DATABASE_DEPLOYMENT.md) - Original detailed guide

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
