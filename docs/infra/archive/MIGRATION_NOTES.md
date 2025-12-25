# Documentation Migration Notes

This document maps old documentation files to their new locations in the restructured documentation system.

## What Changed?

The infrastructure documentation was restructured from 15 scattered files into a focused, AI-navigable system with clear organization, inline code references, and optimized for multiple use cases (onboarding, AI navigation, troubleshooting, change planning).

**Summary:**
- **Before:** 15 separate files (155-1,727 lines each) with overlapping content
- **After:** 15 well-organized files + archive
  - 5 core navigation files (README, QUICK_START, ARCHITECTURE, TROUBLESHOOTING, KNOWN_ISSUES)
  - 6 component deep-dive files (terraform, github-actions, azure-resources, authentication, databases, local-testing)
  - 4 reference files (secrets-checklist, naming-conventions, cost-management, code-references)
  - 15 original files archived here

## File Migration Map

### From Old Structure → New Structure

#### 1. INFRASTRUCTURE.md (1,727 lines)
**Old Location:** `docs/infra/INFRASTRUCTURE.md`

**Content Distributed To:**
- [components/terraform.md](../components/terraform.md) - Terraform configuration guide
- [components/azure-resources.md](../components/azure-resources.md) - Azure services configuration
- [reference/naming-conventions.md](../reference/naming-conventions.md) - Resource naming patterns
- [reference/cost-management.md](../reference/cost-management.md) - Cost tiers and SKU configuration
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System architecture and design
- [TROUBLESHOOTING.md](../TROUBLESHOOTING.md) - Terraform error diagnosis

**Key Sections Moved:**
| Old Section | New File | New Lines |
|-------------|----------|-----------|
| Architecture Overview | ARCHITECTURE.md | L1-50 |
| Terraform Guide | components/terraform.md | L1-80 |
| Naming Conventions | reference/naming-conventions.md | L1-100 |
| Cost Management | reference/cost-management.md | L1-150 |
| Resource Types | components/azure-resources.md | L1-100 |
| Security | components/azure-resources.md | L150-200 |
| Troubleshooting | TROUBLESHOOTING.md | L5-150 |

**Why Split?** Original file was too large (1,727 lines) and mixed concerns. Splitting improves:
- Navigation (easier to find specific topics)
- Reusability (reference files for quick lookups)
- Maintenance (changes isolated to relevant files)
- AI navigation (clear separation of concerns)

---

#### 2. DATABASE_DEPLOYMENT.md (870 lines)
**Old Location:** `docs/infra/DATABASE_DEPLOYMENT.md`

**Content Location:**
- ✅ **Primary:** [components/databases.md](../components/databases.md) - Complete database guide
- **Secondary:** [QUICK_START.md](../QUICK_START.md#2-deploy-database-change) - "Deploy Database Change" task

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Overview | components/databases.md | L1-30 |
| Directory Structure | components/databases.md | L31-80 |
| Auto-Discovery | components/databases.md | L81-150 |
| SQL Script Execution | components/databases.md | L151-220 |
| Deployment Pipeline | components/databases.md | L221-280 |
| Troubleshooting | TROUBLESHOOTING.md | L290-370 |
| Quick Start | QUICK_START.md | Task: Deploy Database Change |

**Why Minimal Changes?** This file was well-organized and focused. Moved mostly as-is to components/databases.md with minimal restructuring.

---

#### 3. authentication-guide.md (490 lines)
**Old Location:** `docs/infra/authentication-guide.md`

**Content Location:**
- ✅ **Primary:** [components/authentication.md](../components/authentication.md) - Complete authentication guide
- **Secondary:** [TROUBLESHOOTING.md](../TROUBLESHOOTING.md#authentication-issues) - Auth troubleshooting (8+ scenarios)

**Key Sections:**
| Old Section | New File | New Lines |
|-------------|----------|-----------|
| Three-Tier Architecture | components/authentication.md | L5-100 |
| OIDC Configuration | components/authentication.md | L100-200 |
| Service Principal | components/authentication.md | L200-280 |
| Secret Configuration | reference/secrets-checklist.md | Checklist version |
| Setup Guide | components/authentication.md | L280-350 |
| Troubleshooting | TROUBLESHOOTING.md | L218-290 |

**Why Kept Intact?** The authentication guide was comprehensive and well-written. Content moved mostly unchanged due to its quality and focus.

---

#### 4. secrets-reference.md (351 lines)
**Old Location:** `docs/infra/secrets-reference.md`

**Content Location:**
- ✅ **Primary:** [reference/secrets-checklist.md](../reference/secrets-checklist.md) - Secret setup reference
- **Secondary:** [components/authentication.md](../components/authentication.md) - Secret configuration section

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Secret List | reference/secrets-checklist.md | Quick reference table |
| Setup Steps | reference/secrets-checklist.md | Step-by-step instructions |
| Validation | reference/secrets-checklist.md | Checklist format |
| Detailed Explanation | components/authentication.md | Full context |

**Why Reorganized?** Converted from dense reference into two formats:
- **Quick reference** (secrets-checklist.md) - Copy-paste friendly for setup
- **Detailed guide** (components/authentication.md) - Full context and understanding

---

#### 5. COMPOSITE_ACTIONS_GUIDE.md (610 lines)
**Old Location:** `docs/infra/COMPOSITE_ACTIONS_GUIDE.md`

**Content Location:**
- ✅ **Primary:** [components/github-actions.md](../components/github-actions.md#composite-actions) - Composite actions section
- **Secondary:** [reference/code-references.md](../reference/code-references.md#composite-actions) - Code locations

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Overview | components/github-actions.md | L1-50 |
| azure-login | components/github-actions.md | L180-220 |
| validate-inputs | components/github-actions.md | L220-260 |
| debug-variables | components/github-actions.md | L260-300 |
| sqlcmd-execute | components/github-actions.md | L300-340 |
| Code References | reference/code-references.md | Composite Actions section |

**Why Condensed?** Original guide was detailed but lengthy. New version summarizes with links to code for deep dives.

---

#### 6. COMPOSITE_ACTIONS_SUMMARY.md (306 lines)
**Old Location:** `docs/infra/COMPOSITE_ACTIONS_SUMMARY.md`

**Content Location:**
- ✅ Merged into [components/github-actions.md](../components/github-actions.md#composite-actions) - Summary format

**Consolidation:** Duplicate of COMPOSITE_ACTIONS_GUIDE.md content. Merged into single authoritative source.

---

#### 7. README.md (340 lines - Composite Actions Guide)
**Old Location:** `docs/infra/README.md` (Note: This was a composite actions guide, not a main README)

**Content Location:**
- ✅ Merged into [components/github-actions.md](../components/github-actions.md) - Composite actions best practices
- **Replaced by:** [README.md](../README.md) - New main navigation hub

**Key Sections:**
| Old Content | New File | Details |
|-------------|----------|---------|
| Composite Actions Guide | components/github-actions.md | Best practices section |
| Usage Examples | components/github-actions.md | Implementation examples |
| Code Reduction Metrics | components/github-actions.md | Benefits quantified |

**Important:** Old README.md was a composite actions guide, not a main index. New [README.md](../README.md) is the main navigation hub for all documentation.

---

#### 8. DEPLOYMENT.md (450 lines)
**Old Location:** `docs/infra/DEPLOYMENT.md`

**Content Location:**
- ✅ **Primary:** [components/github-actions.md](../components/github-actions.md#active-workflows) - Workflow details
- **Secondary:** [QUICK_START.md](../QUICK_START.md) - Deployment tasks
- **Tertiary:** [ARCHITECTURE.md](../ARCHITECTURE.md#ci-cd-pipeline-flow) - Deployment flow

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Workflow Overview | components/github-actions.md | Workflow architecture |
| App Service Deployment | components/github-actions.md | Detailed workflow steps |
| Health Checks | components/github-actions.md | Rollback mechanism |
| Troubleshooting | TROUBLESHOOTING.md | Deployment issues |

**Why Reorganized?** Split deployment concepts across multiple files for better organization:
- High-level flows → ARCHITECTURE.md
- Implementation details → components/github-actions.md
- Quick tasks → QUICK_START.md
- Error diagnosis → TROUBLESHOOTING.md

---

#### 9. GITHUB_VARIABLES_SETUP.md (180 lines)
**Old Location:** `docs/infra/GITHUB_VARIABLES_SETUP.md`

**Content Location:**
- ✅ **Primary:** [reference/code-references.md](../reference/code-references.md#github-variables--secrets) - Variables and secrets index
- **Secondary:** [QUICK_START.md](../QUICK_START.md#5-configure-github-secrets) - "Configure GitHub Secrets" task
- **Tertiary:** [components/github-actions.md](../components/github-actions.md#github-variables) - Variables section

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Variables Table | reference/code-references.md | Complete reference |
| Setup Steps | QUICK_START.md | Quick start task |
| Used By Workflows | reference/code-references.md | Workflow mapping |

**Why Reorganized?** Split into:
- **Task-oriented** (QUICK_START.md) - For developers setting up
- **Reference** (code-references.md) - For AI navigation and lookup
- **Implementation** (github-actions.md) - For understanding workflows

---

#### 10. IMPLEMENTATION_SUMMARY.md (300 lines)
**Old Location:** `docs/infra/IMPLEMENTATION_SUMMARY.md`

**Content Location:**
- ✅ **Primary:** [ARCHITECTURE.md](../ARCHITECTURE.md) - System design and decisions
- **Secondary:** [KNOWN_ISSUES.md](../KNOWN_ISSUES.md) - Implementation considerations

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Design Decisions | ARCHITECTURE.md | Design rationale |
| Component Overview | ARCHITECTURE.md | System components |
| Implementation Notes | KNOWN_ISSUES.md | Considerations and limitations |

**Why Reorganized?** Content better fits architectural documentation with known issues/considerations separated.

---

#### 11. ACR_SETUP.md (200 lines)
**Old Location:** `docs/infra/ACR_SETUP.md`

**Content Location:**
- ✅ **Primary:** [components/azure-resources.md](../components/azure-resources.md#container-registry-acr) - ACR configuration
- **Secondary:** [components/authentication.md](../components/authentication.md#acr-authentication) - ACR auth details
- **Tertiary:** [reference/code-references.md](../reference/code-references.md#azure-resources-workflows) - ACR file locations

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Overview | components/azure-resources.md | ACR introduction |
| Authentication | components/authentication.md | Auth methods |
| Setup | components/azure-resources.md | Configuration steps |
| Troubleshooting | TROUBLESHOOTING.md | ACR issues |

**Why Reorganized?** Split ACR content across Azure resources (infrastructure), authentication (security), and code references (locations).

---

#### 12. LOCAL_TESTING.md (175 lines)
**Old Location:** `docs/infra/LOCAL_TESTING.md`

**Content Location:**
- ✅ **Primary:** [components/local-testing.md](../components/local-testing.md) - Comprehensive act guide
- **Secondary:** [QUICK_START.md](../QUICK_START.md#3-test-workflows-locally-with-act) - "Test with Act" task

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Overview | components/local-testing.md | Act tool introduction |
| Setup | components/local-testing.md | Prerequisites and configuration |
| Usage | components/local-testing.md | How to run act |
| Troubleshooting | TROUBLESHOOTING.md | Local testing issues |

**Status:** Original file was concise. New component file significantly expanded with:
- run-act.ps1 parameter documentation
- Custom runner image details
- Performance optimization (--bind flag)
- Comprehensive troubleshooting

---

#### 13. QUICK_START_TIER2.md (250 lines)
**Old Location:** `docs/infra/QUICK_START_TIER2.md`

**Content Location:**
- ✅ **Primary:** [QUICK_START.md](../QUICK_START.md) - New quick start guide
- **Secondary:** [README.md](../README.md#quick-start) - Quick links

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Deploy Infrastructure | QUICK_START.md | Task 1 |
| Deploy Database | QUICK_START.md | Task 2 |
| Test with Act | QUICK_START.md | Task 3 |
| Add Resource | QUICK_START.md | Task 4 |
| Configure Secrets | QUICK_START.md | Task 5 |

**Note:** New QUICK_START.md is more comprehensive with:
- Time estimates for each task
- Multiple checklists (initial setup, production, daily development)
- Better troubleshooting per task
- Links to detailed guides

---

#### 14. TIER_2_SETUP_CHECKLIST.md (120 lines)
**Old Location:** `docs/infra/TIER_2_SETUP_CHECKLIST.md`

**Content Location:**
- ✅ Merged into [QUICK_START.md](../QUICK_START.md#checklists) - Checklists section

**Key Sections:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Initial Setup | QUICK_START.md | Initial Setup Checklist |
| Daily Development | QUICK_START.md | Daily Development Checklist |
| Production Deploy | QUICK_START.md | Production Deployment Checklist |

**Consolidation:** Checklist content merged into new QUICK_START.md for single source of truth.

---

#### 15. IMPORT_LOOP_FIX.md (155 lines)
**Old Location:** `docs/infra/IMPORT_LOOP_FIX.md`

**Content Location:**
- ✅ **Primary:** [components/local-testing.md](../components/local-testing.md#8-performance-optimization) - Performance optimization section
- **Also:** `infra/act/IMPORT_LOOP_FIX.md` - Original file (performance details)

**Key Content:**
| Old Section | New File | Details |
|-------------|----------|---------|
| Problem | components/local-testing.md | Import loop issue explained |
| Solution | components/local-testing.md | --bind flag solution |
| Verification | components/local-testing.md | How to verify |
| Results | components/local-testing.md | 5x speedup achieved |

**Status:** Content incorporated into local-testing.md with reference to original file for deep details. [`infra/act/IMPORT_LOOP_FIX.md`](../../../infra/act/IMPORT_LOOP_FIX.md) remains as source of truth for performance optimization.

---

## New Files Created

### Core Navigation Files

1. **[README.md](../README.md)** (354 lines)
   - Main index and navigation hub
   - Quick start links at top
   - Component guide navigation
   - Reference materials directory
   - For AI agents section

2. **[QUICK_START.md](../QUICK_START.md)** (408 lines)
   - 5 common tasks with time estimates
   - Step-by-step instructions
   - Multiple checklists
   - Troubleshooting per task

3. **[ARCHITECTURE.md](../ARCHITECTURE.md)** (400+ lines)
   - System design diagrams
   - Component relationships
   - Authentication flow
   - Database deployment flow
   - Resource dependencies

4. **[TROUBLESHOOTING.md](../TROUBLESHOOTING.md)** (600+ lines)
   - Error diagnosis and solutions
   - Organized by category
   - Code references
   - Prevention tips

5. **[KNOWN_ISSUES.md](../KNOWN_ISSUES.md)** (500+ lines)
   - Active issues and limitations
   - Performance considerations
   - Future enhancements
   - Community contribution opportunities

### Component Deep-Dive Files

6. **[components/terraform.md](../components/terraform.md)** (280 lines)
7. **[components/github-actions.md](../components/github-actions.md)** (350 lines)
8. **[components/azure-resources.md](../components/azure-resources.md)** (220 lines)
9. **[components/authentication.md](../components/authentication.md)** (440 lines)
10. **[components/databases.md](../components/databases.md)** (330 lines)
11. **[components/local-testing.md](../components/local-testing.md)** (450 lines)

### Reference Files

12. **[reference/secrets-checklist.md](../reference/secrets-checklist.md)** (320 lines)
13. **[reference/naming-conventions.md](../reference/naming-conventions.md)** (280 lines)
14. **[reference/cost-management.md](../reference/cost-management.md)** (380 lines)
15. **[reference/code-references.md](../reference/code-references.md)** (540 lines)

## How to Navigate

### For Developers (Onboarding)

**Start here:** [README.md](../README.md) → [QUICK_START.md](../QUICK_START.md)

1. Read README.md overview (5 minutes)
2. Follow one of 5 Quick Start tasks (5-20 minutes each)
3. Reference detailed component guides as needed

### For Operations/DevOps

**Start here:** [README.md](../README.md) → [ARCHITECTURE.md](../ARCHITECTURE.md) → [TROUBLESHOOTING.md](../TROUBLESHOOTING.md)

1. Understand system architecture
2. Learn deployment flow
3. Troubleshoot issues

### For AI Agents

**Start here:** [README.md](../README.md) → [reference/code-references.md](../reference/code-references.md)

1. Use code-references.md to locate files
2. Follow inline code links to source
3. Reference component docs for context

### For Making Changes

**Workflow:**
1. Check [KNOWN_ISSUES.md](../KNOWN_ISSUES.md) for relevant issues
2. Review [ARCHITECTURE.md](../ARCHITECTURE.md) for dependencies
3. Locate relevant code in [reference/code-references.md](../reference/code-references.md)
4. Follow [TROUBLESHOOTING.md](../TROUBLESHOOTING.md) after changes

## Archive Contents

This `archive/` directory contains the original 15 documentation files that have been consolidated into the new structure above.

**Original Files:**
- INFRASTRUCTURE.md (1,727 lines) → Split across multiple new files
- DATABASE_DEPLOYMENT.md (870 lines) → components/databases.md
- authentication-guide.md (490 lines) → components/authentication.md
- secrets-reference.md (351 lines) → reference/secrets-checklist.md
- COMPOSITE_ACTIONS_GUIDE.md (610 lines) → components/github-actions.md
- COMPOSITE_ACTIONS_SUMMARY.md (306 lines) → Merged into github-actions.md
- README.md (340 lines) → Merged into components/github-actions.md
- DEPLOYMENT.md (450 lines) → Split across multiple files
- GITHUB_VARIABLES_SETUP.md (180 lines) → reference/code-references.md + QUICK_START.md
- IMPLEMENTATION_SUMMARY.md (300 lines) → ARCHITECTURE.md + KNOWN_ISSUES.md
- ACR_SETUP.md (200 lines) → components/azure-resources.md + authentication.md
- LOCAL_TESTING.md (175 lines) → components/local-testing.md
- QUICK_START_TIER2.md (250 lines) → QUICK_START.md
- TIER_2_SETUP_CHECKLIST.md (120 lines) → QUICK_START.md checklists
- IMPORT_LOOP_FIX.md (155 lines) → components/local-testing.md

**Keep for Reference?** No. Old files are preserved here for historical reference only. **Always use new documentation** in parent directory for:
- Onboarding new developers
- AI agent navigation
- Troubleshooting
- Updating documentation

## Benefits of New Structure

### ✅ Improved Navigation
- Clear hierarchy (core → components → reference)
- Inline code references for AI
- Progressive disclosure (simple → complex)

### ✅ Reduced Duplication
- 15 files → consolidated to 15 well-organized files
- Single source of truth for each topic
- No conflicting information

### ✅ Better Maintenance
- Changes isolated to relevant files
- Easier to update specific topics
- Less context switching

### ✅ AI-Friendly
- Consistent code reference format: `[file](path#L123)`
- Clear file structure for parsing
- Index file for quick lookup

### ✅ Multiple Use Cases
- Onboarding: QUICK_START.md
- Navigation: README.md + code-references.md
- Troubleshooting: TROUBLESHOOTING.md
- Change planning: ARCHITECTURE.md + relevant components

## Questions?

If you have questions about:
- **Where specific information moved?** → Check migration map above
- **How to find something?** → Start at README.md
- **How to troubleshoot?** → See TROUBLESHOOTING.md
- **How to understand architecture?** → See ARCHITECTURE.md
- **Where is specific code?** → See reference/code-references.md

---

**Last Updated:** December 2025
**Migration Date:** December 2025
**Previous Structure:** 15 files in docs/infra/ root
**New Structure:** 15 files in organized hierarchy (core, components/, reference/, archive/)
