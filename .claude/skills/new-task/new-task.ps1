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

$dest = Join-Path (Join-Path (Split-Path $mainRoot -Parent) 'worktrees') $slug
if (Test-Path $dest) { throw "Worktree already exists: $dest" }

Invoke-Git -C $mainRoot fetch origin main --quiet
Invoke-Git -C $mainRoot worktree add -b $slug $dest origin/main

code $dest

Write-Host ""
Write-Host "worktree: $dest"
Write-Host "branch:   $slug (off origin/main)"
