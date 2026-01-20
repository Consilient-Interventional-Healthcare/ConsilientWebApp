# Claude Desktop MCP Configuration

This configuration allows Claude Desktop to directly access project files, including the OpenAPI specification.

## Team-Friendly Setup

This configuration uses a **placeholder-based approach** that works across different developer machines:

- **Template:** `claude_desktop_config.json` contains `${REPO_ROOT}` placeholders
- **Install Script:** Replaces placeholders with each developer's actual repository path
- **Version Control:** Template is committed, actual paths are machine-specific

## What This Enables

When configured, Claude Desktop can:
- ? Read `docs/openapi.json` to understand API endpoints
- ? Access API source code in `Consilient.Api/`
- ? View PowerShell scripts in `Scripts/`
- ? Provide more accurate answers about your codebase

## Quick Installation

From the repository root:

```powershell
# One-command installation
pwsh .claude/Install-ClaudeConfig.ps1

# Or with force overwrite
pwsh .claude/Install-ClaudeConfig.ps1 -Force
```

This automatically:
1. Detects your repository location
2. Replaces `${REPO_ROOT}` with your path
3. Installs to Claude Desktop config
4. Validates Node.js is available

## Manual Installation

If you prefer manual setup:

### Windows

1. **Open the template:**
   ```powershell
   code .claude/claude_desktop_config.json
   ```

2. **Find your repository path:**
   ```powershell
   # Run from repository root
   (Get-Location).Path
   # Example: C:\Work\ConsilientWebApp
   ```

3. **Replace placeholders:**
   - Replace all `${REPO_ROOT}` with your path
   - Use double backslashes: `C:\\Work\\ConsilientWebApp`
   - Example: `${REPO_ROOT}/docs` ? `C:\\Work\\ConsilientWebApp\\docs`

4. **Save to Claude config:**
   ```powershell
   # Edit (or create) this file
   notepad "$env:APPDATA\Claude\claude_desktop_config.json"
   
   # Paste your modified configuration
   ```

5. **Restart Claude Desktop**

### macOS

1. **Find your repository path:**
   ```bash
   pwd
   # Example: /Users/yourname/Work/ConsilientWebApp
   ```

2. **Replace placeholders in template:**
   - Replace `${REPO_ROOT}` with your path
   - Use forward slashes: `/Users/yourname/Work/ConsilientWebApp`
   - Example: `${REPO_ROOT}/docs` ? `/Users/yourname/Work/ConsilientWebApp/docs`

3. **Save to Claude config:**
   ```bash
   code ~/Library/Application\ Support/Claude/claude_desktop_config.json
   # Paste your modified configuration
   ```

4. **Restart Claude Desktop**

### Linux

Similar to macOS, but config location is:
```bash
code ~/.config/Claude/claude_desktop_config.json
```

## What Each Server Provides

### `consilient-docs`
- **Absolute Path (at repo root):** `docs/` 
- **Purpose:** OpenAPI specification and documentation
- **Note:** `docs/` is at repository root, NOT in `src/`
- **Use case:** "Show me the API endpoints" or "What's in the OpenAPI spec?"

### `consilient-api`
- **Absolute Path:** `src/Consilient.Api/`
- **Purpose:** API source code (controllers, services, configuration)
- **Use case:** "How is authentication configured?" or "Show me the Patient controller"

### `consilient-scripts`
- **Absolute Path:** `src/Scripts/`
- **Purpose:** PowerShell automation scripts
- **Use case:** "How does OpenAPI generation work?" or "What scripts are available?"

## Verifying Installation

1. **Check config file exists:**
   ```powershell
   # Windows
   Test-Path "$env:APPDATA\Claude\claude_desktop_config.json"
   
   # macOS
   test -f ~/Library/Application\ Support/Claude/claude_desktop_config.json
   
   # Linux
   test -f ~/.config/Claude/claude_desktop_config.json
   ```

2. **Verify paths are absolute (not ${REPO_ROOT}):**
   ```powershell
   # Windows
   Get-Content "$env:APPDATA\Claude\claude_desktop_config.json"
   
   # macOS/Linux
   cat ~/Library/Application\ Support/Claude/claude_desktop_config.json
   ```

3. **Test with Claude Desktop:**
   - Restart Claude Desktop completely
   - Ask: "Can you read the file docs/openapi.json?"
   - If working, Claude will be able to read and discuss it

## Team Workflow

Each team member should:

1. **Clone the repository**
   ```powershell
   git clone https://github.com/Consilient-Interventional-Healthcare/ConsilientWebApp
   cd ConsilientWebApp
   ```

2. **Run the installer**
   ```powershell
   pwsh .claude/Install-ClaudeConfig.ps1
   ```

3. **Restart Claude Desktop**

4. **Generate OpenAPI doc** (if not already present)
   ```powershell
   pwsh Scripts/openapi-generation/Generate-OpenApiDoc.ps1
   ```

5. **Test the configuration**
   - Ask Claude: "What API endpoints are available?"

**Important:** Do NOT commit your personal `claude_desktop_config.json` from `%APPDATA%\Claude\` - that's machine-specific.

## Updating Configuration

When new directories need to be added:

1. **Update the template** in repository:
   ```json
   "new-server": {
     "command": "npx",
     "args": [
       "-y",
       "@modelcontextprotocol/server-filesystem",
       "${REPO_ROOT}/NewFolder"
     ]
   }
   ```

2. **Commit the template:**
   ```powershell
   git add .claude/claude_desktop_config.json
   git commit -m "Add NewFolder to Claude MCP configuration"
   ```

3. **Team members reinstall:**
   ```powershell
   pwsh .claude/Install-ClaudeConfig.ps1 -Force
   ```

## Troubleshooting

### Install script fails to find repository

**Symptom:** Error about `Consilient.Api\Consilient.Api.csproj` not found

**Solution:** Run the script from repository root:
```powershell
cd C:\Work\ConsilientWebApp
pwsh .claude/Install-ClaudeConfig.ps1
```

### Paths still show ${REPO_ROOT}

**Symptom:** Config file contains literal `${REPO_ROOT}` text

**Solution:** 
- The installed config should NOT have placeholders
- Check the actual Claude config file (not the template):
  ```powershell
  # Windows - check the INSTALLED config
  Get-Content "$env:APPDATA\Claude\claude_desktop_config.json"
  ```
- If placeholders remain, run the install script again

### Claude can't access files

**Common causes:**
1. Claude Desktop not restarted after installation
2. Node.js not installed
3. Incorrect path separators (backslashes vs forward slashes)
4. Repository moved after configuration

**Solutions:**
```powershell
# 1. Completely quit and restart Claude Desktop

# 2. Check Node.js
node --version

# 3. Reinstall configuration
pwsh .claude/Install-ClaudeConfig.ps1 -Force

# 4. Verify paths in installed config
Get-Content "$env:APPDATA\Claude\claude_desktop_config.json"
```

### "Cannot find module" errors

**Cause:** Node.js or npm not installed

**Solution:**
```powershell
# Install Node.js from https://nodejs.org/
# Verify:
node --version
npm --version
```

## Security Notes

?? **Important Security Considerations:**

- MCP servers have **read-only** access
- Claude can read files but cannot modify them
- Only directories explicitly listed are accessible
- Sensitive folders (secrets, credentials) are NOT included
- Each developer's config is isolated to their machine

## Files in This Directory

- **`claude_desktop_config.json`** - Template with `${REPO_ROOT}` placeholders (committed to Git)
- **`Install-ClaudeConfig.ps1`** - Automated installation script (committed to Git)
- **`README.md`** - This documentation (committed to Git)

**Not committed:**
- Your actual Claude Desktop config at `%APPDATA%\Claude\claude_desktop_config.json` (machine-specific)

## Related Documentation

- **OpenAPI Spec:** [`docs/openapi.json`](../docs/openapi.json)
- **Generation Script:** [`Scripts/openapi-generation/Generate-OpenApiDoc.ps1`](../Scripts/openapi-generation/Generate-OpenApiDoc.ps1)
- **Copilot Instructions:** [`../.github/copilot-instructions.md`](../.github/copilot-instructions.md)

---

**Repository:** [ConsilientWebApp](https://github.com/Consilient-Interventional-Healthcare/ConsilientWebApp)  
**MCP Documentation:** [Model Context Protocol](https://modelcontextprotocol.io/)
