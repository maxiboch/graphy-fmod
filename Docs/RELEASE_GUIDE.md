# Release Automation Guide

This repository includes two ways to automate the release process.

## Quick Release Commands

### Option 1: PowerShell Script (Recommended for local use)

```powershell
# Simple release with just version and message
.\Tools\release.ps1 -Version "3.1.13" -Message "Fix bug in FMOD integration"

# Release with custom release notes
.\Tools\release.ps1 -Version "3.1.13" -Message "Major update" -ReleaseNotes "## Features`n- New feature 1`n- New feature 2"
```

**What it does:**
1. Commits any uncommitted changes
2. Pushes to GitHub
3. Updates package.json version
4. Creates and pushes version tag (e.g., v3.1.13)
5. Updates the 'latest' tag
6. Creates GitHub release with notes

### Option 2: GitHub Actions Workflow

1. Go to your repository on GitHub
2. Click on "Actions" tab
3. Select "Create Release" workflow
4. Click "Run workflow"
5. Fill in:
   - Version number (e.g., 3.1.13)
   - Release message/title
   - Release notes (optional)
6. Click "Run workflow"

**What it does:**
Same as the PowerShell script, but runs on GitHub's servers.

## Manual Process (if needed)

If you prefer to do it manually or need more control:

```powershell
# 1. Commit your changes
git add .
git commit -m "Your commit message"
git push origin main

# 2. Update package.json version (edit manually or via script)

# 3. Create and push tag
git tag -a v3.1.13 -m "v3.1.13: Your message"
git push origin v3.1.13

# 4. Update latest tag
git tag -f latest v3.1.13
git push origin latest --force

# 5. Create GitHub release
gh release create v3.1.13 --title "v3.1.13: Your title" --notes "Your release notes"
```

## Best Practices

1. Always ensure your code is tested before releasing
2. Use semantic versioning (MAJOR.MINOR.PATCH)
3. Write clear, descriptive release messages
4. Include breaking changes in release notes
5. Tag Unity-specific releases appropriately