# Future Enhancements

This document tracks planned improvements and enhancements for the Consilient application.

---

## Build System

### CI/CD Integration with NUKE

**Priority**: Medium
**Effort**: Low-Medium
**Status**: Planned

Refactor GitHub Actions workflows to call `build.cmd` instead of raw `dotnet` commands.

**Benefits**:
- Consistency between local and CI builds
- Single source of truth for build logic
- Easier to test CI changes locally
- Reduces duplication between workflow files and NUKE targets

**Implementation Example**:
```yaml
# Before (raw commands)
- run: dotnet restore
- run: dotnet build --configuration Release
- run: dotnet test

# After (NUKE)
- run: ./build.cmd Compile Test --configuration Release
```

**Considerations**:
- Requires `build.cmd` to be executable in CI environment
- May need to handle NUKE tool restore in CI
- Consider caching NUKE tool installation

---

### Migrate Database Docs CI to NUKE

**Priority**: Low
**Effort**: Low
**Status**: Planned

Once `GenerateDatabaseDocs` NUKE target is validated, simplify the GitHub Actions workflow to:
1. Call NUKE for documentation generation
2. Keep only the GitHub Pages deployment in the workflow

**Current State**:
- `.github/workflows/database-docs.yml` contains all generation logic (bash scripts, SchemaSpy invocation)
- NUKE `GenerateDatabaseDocs` target duplicates this for local use

**Migration Steps**:

1. **Update `database-docs.yml`**:
   - Remove `process-all-databases.sh` invocation
   - Remove `generate-index.sh` invocation
   - Replace with: `./build.cmd GenerateDatabaseDocs --use-docker`

2. **Simplified Workflow**:
   ```yaml
   jobs:
     generate:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - name: Setup .NET
           uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '9.0.x'
         - name: Generate Database Docs
           run: ./build.cmd GenerateDatabaseDocs --use-docker
         - uses: actions/upload-pages-artifact@v3
           with:
             path: './docs'

     deploy:
       needs: generate
       runs-on: ubuntu-latest
       environment:
         name: github-pages
       steps:
         - uses: actions/deploy-pages@v4
   ```

3. **Files to Delete** (after migration):
   - `.github/workflows/database-docs/process-all-databases.sh`
   - `.github/workflows/database-docs/generate-index.sh`
   - `.github/workflows/database-docs/index.template.html`
   - `.github/workflows/database-docs/database.template.html`

**Benefits**:
- Single source of truth for docs generation logic
- Easier testing - same command works locally and in CI
- Simplified workflow file

---

## How to Add New Items

When adding new future enhancements, include:

1. **Title** - Clear, descriptive name
2. **Priority** - High / Medium / Low
3. **Effort** - Low / Medium / High
4. **Status** - Planned / In Progress / Blocked / Complete
5. **Description** - What the enhancement involves
6. **Benefits** - Why it's valuable
7. **Considerations** - Any risks or dependencies
