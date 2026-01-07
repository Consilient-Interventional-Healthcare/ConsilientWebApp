# AZURE_CREDENTIALS Removal - Migration Guide

## Summary

The `AZURE_CREDENTIALS` secret (JSON blob format) has been removed from all GitHub Actions workflows and composite actions. The same functionality is now achieved using individual secrets that were already present in the codebase.

## Timeline

- **Current**: AZURE_CREDENTIALS removed in favor of individual secrets (AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID)
- **Impact**: Only affects local testing with `act`
- **Production**: No impact - OIDC authentication is unaffected

## What Changed

### Before (Old Format)
```bash
# Single JSON blob
AZURE_CREDENTIALS={"clientId":"xxx","clientSecret":"yyy","tenantId":"zzz"}
```

### After (New Format)
```bash
# Individual secrets
AZURE_CLIENT_ID=xxx
AZURE_CLIENT_SECRET=yyy
AZURE_TENANT_ID=zzz
```

## Why This Change?

1. **Simplification**: Removed 47 lines of JSON parsing and validation code
2. **Security**: Better separation of concerns, easier credential rotation
3. **Consistency**: Aligns with GitHub Actions best practices
4. **Maintainability**: Less code to maintain and test
5. **Clarity**: Each secret's purpose is explicit

## Who Needs to Update?

### Local Testing with `act`
If you run workflows locally using `act`, you need to update your `.env.act` file:

**Update Required For:**
- Users running `act` to test workflows locally
- CI/CD developers testing GitHub Actions workflows

**No Action Needed For:**
- Production GitHub Actions runs (OIDC authentication is unaffected)
- Users not using `act` locally

## How to Update `.env.act`

### Option 1: Manual Update (Recommended)

1. Open `infra/act/.env.act`
2. Find the `AZURE_CREDENTIALS` line (if it exists) and delete it
3. Verify these lines are present:
   ```bash
   AZURE_CLIENT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
   AZURE_CLIENT_SECRET=your-client-secret-here
   AZURE_TENANT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
   ```
4. Save the file

### Option 2: Automated Migration Script

**PowerShell:**
```powershell
$envFile = "infra/act/.env.act"
$content = Get-Content $envFile -Raw

# Remove AZURE_CREDENTIALS line if present
if ($content -match 'AZURE_CREDENTIALS=.*\n') {
    $content = $content -replace 'AZURE_CREDENTIALS=.*\n', ''
    Set-Content $envFile $content
    Write-Host "✅ Removed AZURE_CREDENTIALS from .env.act"
} else {
    Write-Host "ℹ️  AZURE_CREDENTIALS not found (already migrated)"
}
```

**Bash:**
```bash
envFile="infra/act/.env.act"

# Remove AZURE_CREDENTIALS line if present
if grep -q "^AZURE_CREDENTIALS=" "$envFile"; then
    sed -i '/^AZURE_CREDENTIALS=/d' "$envFile"
    echo "✅ Removed AZURE_CREDENTIALS from .env.act"
else
    echo "ℹ️  AZURE_CREDENTIALS not found (already migrated)"
fi
```

## Verification

After updating `.env.act`, verify the changes:

```bash
# Check that individual secrets are present
grep -E "^AZURE_(CLIENT_ID|CLIENT_SECRET|TENANT_ID)=" infra/act/.env.act

# Expected output:
# AZURE_CLIENT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
# AZURE_CLIENT_SECRET=your-client-secret-here
# AZURE_TENANT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

# Verify AZURE_CREDENTIALS is removed
grep "^AZURE_CREDENTIALS=" infra/act/.env.act || echo "✅ AZURE_CREDENTIALS successfully removed"
```

## Testing After Migration

### Test Local Workflows with `act`

1. **Simple test - Terraform workflow:**
   ```bash
   act -W .github/workflows/terraform.yml -j terraform
   ```

2. **Expected behavior:**
   - Service principal login succeeds
   - No errors about missing AZURE_CREDENTIALS
   - Azure CLI commands work (az account show)

3. **If authentication fails:**
   - Verify AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID are set in .env.act
   - Verify the values are correct (should match Azure service principal credentials)
   - Run: `act -W .github/workflows/terraform.yml -j terraform --bind -e infra/act/.env.act`

### Test Production Workflows

Production workflows use OIDC authentication and are not affected by this change. No testing needed.

## Troubleshooting

### Error: "Azure credentials not fully provided for local testing"

**Cause**: One or more of AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, or AZURE_TENANT_ID is missing or empty

**Solution**:
1. Check `infra/act/.env.act` and ensure all three secrets are present
2. Verify the values are not empty strings
3. Verify the values are valid Azure credentials

### Error: "az login" command fails

**Cause**: Invalid credentials or service principal doesn't have required permissions

**Solution**:
1. Verify credentials match an active Azure service principal
2. Verify the service principal has permissions for the target Azure resources
3. Test credentials manually: `az login --service-principal -u CLIENT_ID -p CLIENT_SECRET --tenant TENANT_ID`

### Error: "No such file: .env.act"

**Cause**: The `.env.act` file is missing from `infra/act/` directory

**Solution**:
1. Copy from template: `cp infra/act/.env.act.template infra/act/.env.act`
2. Fill in values from a working setup or Azure credentials

## GitHub Repository Settings

### For Repository Administrators

If your repository has `AZURE_CREDENTIALS` defined as a GitHub secret, you can optionally remove it:

1. Go to Repository Settings → Secrets and variables → Actions
2. Find `AZURE_CREDENTIALS` in the Secrets tab
3. Click the delete icon
4. Confirm deletion

**Note**: This is optional. The secret will simply not be used if undefined.

## FAQ

### Q: Will this affect production deployments?
**A**: No. Production GitHub Actions workflows use OIDC authentication, which is completely separate from AZURE_CREDENTIALS.

### Q: Do I need to update my workflows if I don't use `act`?
**A**: The workflow files have been updated, but if you don't use `act` locally, this change won't affect you. Your production deployments will continue to work exactly as before.

### Q: Can I still use AZURE_CREDENTIALS if I want?
**A**: The code that handled AZURE_CREDENTIALS has been removed. If you need to provide credentials to `act`, use the individual secrets: AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.

### Q: How do I extract credentials from the old AZURE_CREDENTIALS JSON?
**A**: The JSON structure was:
```json
{
  "clientId": "...",
  "clientSecret": "...",
  "tenantId": "..."
}
```

Extract these values and set them as:
- `AZURE_CLIENT_ID` = clientId
- `AZURE_CLIENT_SECRET` = clientSecret
- `AZURE_TENANT_ID` = tenantId

### Q: What if I have AZURE_CREDENTIALS defined in GitHub as a secret?
**A**: It will simply be ignored. You can remove it from GitHub repository settings, but it's not required.

## Support

If you encounter issues during or after migration:

1. Check the troubleshooting section above
2. Verify your `.env.act` file contains all required secrets
3. Test manually: `az login --service-principal -u CLIENT_ID -p CLIENT_SECRET --tenant TENANT_ID`
4. Review workflow logs for detailed error messages

## Related Documentation

- [Local Testing Guide](local-testing.md)
- [Authentication Architecture](authentication.md)
- [GitHub Actions Setup](github-actions.md)
