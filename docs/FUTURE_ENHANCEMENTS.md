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

## How to Add New Items

When adding new future enhancements, include:

1. **Title** - Clear, descriptive name
2. **Priority** - High / Medium / Low
3. **Effort** - Low / Medium / High
4. **Status** - Planned / In Progress / Blocked / Complete
5. **Description** - What the enhancement involves
6. **Benefits** - Why it's valuable
7. **Considerations** - Any risks or dependencies
