param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$true)]
    [string]$Message,
    
    [Parameter(Mandatory=$false)]
    [string]$ReleaseNotes = ""
)

# Change to repository root (parent directory of Tools)
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptPath
Set-Location $repoRoot

Write-Host "🚀 Starting release process for version $Version" -ForegroundColor Cyan

# Check for uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Host "⚠️  You have uncommitted changes. Committing them first..." -ForegroundColor Yellow
    git add .
    git commit -m "$Message"
}

# Push any unpushed commits
Write-Host "📤 Pushing changes to GitHub..." -ForegroundColor Green
git push origin main

# Update package.json version
Write-Host "📝 Updating package.json version..." -ForegroundColor Green
$packageJson = Get-Content "package.json" -Raw | ConvertFrom-Json
$oldVersion = $packageJson.version
$packageJson.version = $Version
$packageJson | ConvertTo-Json -Depth 10 | Set-Content "package.json"

# Commit version bump if changed
if ($oldVersion -ne $Version) {
    git add package.json
    git commit -m "Bump version to $Version"
    git push origin main
}

# Create and push tag
Write-Host "🏷️  Creating tag v$Version..." -ForegroundColor Green
git tag -a "v$Version" -m "v${Version}: $Message"
git push origin "v$Version"

# Update latest tag
Write-Host "🔄 Updating latest tag..." -ForegroundColor Green
git tag -f latest "v$Version"
git push origin latest --force

# Create GitHub release
Write-Host "📦 Creating GitHub release..." -ForegroundColor Green
if ([string]::IsNullOrEmpty($ReleaseNotes)) {
    $ReleaseNotes = "## Release v$Version`n`n$Message"
}

gh release create "v$Version" `
    --title "v${Version}: $Message" `
    --notes $ReleaseNotes

Write-Host "✅ Release v$Version completed successfully!" -ForegroundColor Green
Write-Host "🔗 View release at: https://github.com/maxiboch/graphy-fmod/releases/tag/v$Version" -ForegroundColor Cyan