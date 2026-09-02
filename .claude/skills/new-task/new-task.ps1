#Requires -Version 5
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Task
)

$ErrorActionPreference = 'Stop'

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)]$GitArgs)
    & git @GitArgs
    if ($LASTEXITCODE -ne 0) { throw "git $($GitArgs -join ' ') failed ($LASTEXITCODE)" }
}

# task name -> safe slug for branch + folder
$slug = ($Task.Trim() -replace '[^\w.-]+', '-').Trim('-').ToLower()
if (-not $slug) { throw "Invalid task name: '$Task'" }

# primary checkout is always the first entry of `git worktree list`
$mainRoot = ((git worktree list --porcelain | Select-Object -First 1) -replace '^worktree\s+', '')
if (-not (Test-Path $mainRoot)) { throw "Could not resolve main repo root." }

# worktree lives directly under the repo root: <repo>\<slug>
$dest = Join-Path $mainRoot $slug
if (Test-Path $dest) { throw "Path already exists (folder name taken): $dest" }

Invoke-Git -C $mainRoot fetch origin main --quiet
Invoke-Git -C $mainRoot worktree add -b $slug $dest origin/main

# keep the primary checkout clean without touching tracked files
$exclude = Join-Path $mainRoot '.git\info\exclude'
$ignoreLine = "/$slug/"
if (-not ((Test-Path $exclude) -and (Select-String -Path $exclude -SimpleMatch -Pattern $ignoreLine -Quiet))) {
    Add-Content -Path $exclude -Value $ignoreLine
}

code $dest

Write-Host ""
Write-Host "worktree: $dest"
Write-Host "branch:   $slug (off origin/main)"
