<#
Script: clean-git-secret.ps1
Purpose: Guide and run commands to remove sensitive files from Git history using git-filter-repo or BFG.
USAGE (review before running):
  1. Rotate/revoke all exposed keys (do this first!)
  2. Run this script from a PowerShell prompt with admin/dev privileges.
  3. This will create a mirror clone, run the cleanup, and push --force to origin. All collaborators must re-clone afterwards.

IMPORTANT: Review and test on a backup/mirror clone. This rewrites history and requires force-push.
#>
param(
	[string]$RepoUrl = "https://github.com/TebohoMosakoaCIDB/iTender.Compliance.git",
	[string]$WorkingDir = "$env:TEMP\iTenderCompliance_git_cleanup",
	[string]$PathToRemove = "iTender.Compliance.Client/appsettings.json"
)

function Ensure-ToolExists {
	param([string]$Tool)
	if ($Tool -eq 'git-filter-repo') {
		git filter-repo --help > $null 2>&1
		return $LASTEXITCODE -eq 0
	}
	if ($Tool -eq 'bfg') {
		return Test-Path -Path "$PSScriptRoot\bfg.jar"
	}
	return $false
}

Write-Host "This script will help remove $PathToRemove from repository history of $RepoUrl" -ForegroundColor Yellow
Write-Host "1) Make sure you've revoked/rotated the exposed keys before proceeding." -ForegroundColor Yellow
Write-Host "2) This operation rewrites history and requires force-push. Coordinate with collaborators." -ForegroundColor Yellow

# Create working dir
if (Test-Path $WorkingDir) { Remove-Item -Recurse -Force $WorkingDir }
New-Item -ItemType Directory -Path $WorkingDir | Out-Null
Set-Location $WorkingDir

# Clone mirror
Write-Host "Cloning mirror repository..." -ForegroundColor Cyan
git clone --mirror $RepoUrl iTender.Compliance.git
if ($LASTEXITCODE -ne 0) { Write-Error "git clone failed"; exit 1 }
Set-Location iTender.Compliance.git

# Prefer git-filter-repo if available
if (Ensure-ToolExists 'git-filter-repo') {
	Write-Host "Using git-filter-repo to remove path: $PathToRemove" -ForegroundColor Green
	# Remove the file path entirely from history
	git filter-repo --invert-paths --path $PathToRemove
	if ($LASTEXITCODE -ne 0) { Write-Error "git-filter-repo failed"; exit 1 }
} elseif (Ensure-ToolExists 'bfg') {
	Write-Host "Using BFG (bfg.jar) to remove path: $PathToRemove" -ForegroundColor Green
	java -jar "$PSScriptRoot\bfg.jar" --delete-files $PathToRemove
	if ($LASTEXITCODE -ne 0) { Write-Error "BFG failed"; exit 1 }
	git reflog expire --expire=now --all
	git gc --prune=now --aggressive
} else {
	Write-Error "Neither git-filter-repo nor BFG found. Install git-filter-repo (recommended) or place bfg.jar next to this script." -ForegroundColor Red
	exit 1
}

# Force push cleaned history
Write-Host "Pushing cleaned history (force) to origin..." -ForegroundColor Cyan
git push --force --all
if ($LASTEXITCODE -ne 0) { Write-Error "git push --force --all failed"; exit 1 }
git push --force --tags

Write-Host "Cleanup complete. All collaborators must re-clone the repository." -ForegroundColor Green

