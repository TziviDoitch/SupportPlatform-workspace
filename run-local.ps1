<#
.SYNOPSIS
  Run SupportPlatform locally WITHOUT Docker.

.DESCRIPTION
  The primary way to run SupportPlatform locally on Windows: the whole stack on
  the host, no Docker. (A cross-platform Docker Compose setup is also provided
  under infra/ — see the README — but it is not required and needs WSL2 on Windows.)

    * db     - uses the SQL Server LocalDB instance already installed on Windows
               (MSSQLLocalDB). No container. A preflight step first clears a
               stale SupportPlatform DB left by a previously force-killed run.
    * api    - dotnet run (src/Api). In Development it auto-applies EF migrations
               and seeds demo data into the LocalDB database.
    * client - npm run dev (Vite), with /api/* proxied to the api above.

  Press Ctrl+C to stop both servers (do not just close the window).
#>
[CmdletBinding()]
param(
    [int]$ApiPort    = 5080,
    [int]$ClientPort = 5173,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$root       = $PSScriptRoot
$serverDir  = Join-Path $root 'server'
$clientDir  = Join-Path $root 'client'
$connString = 'Server=(localdb)\MSSQLLocalDB;Database=SupportPlatform;Trusted_Connection=True;TrustServerCertificate=True'

function Find-LocalDb {
    $c = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    if ($c) { return $c.Source }
    $glob = 'C:\Program Files\Microsoft SQL Server\*\Tools\Binn\SqlLocalDB.exe'
    $hit  = Get-ChildItem $glob -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($hit) { return $hit.FullName }
    return $null
}

Write-Host '== SupportPlatform local run (no Docker) ==' -ForegroundColor Cyan

# 1. LocalDB ---------------------------------------------------------------
$localDb = Find-LocalDb
if (-not $localDb) {
    Write-Error 'SQL Server LocalDB not found. Install it, or install Docker Desktop and run: cd infra ; docker compose up --build'
}
Write-Host "-> starting LocalDB (MSSQLLocalDB) via $localDb"
& $localDb start MSSQLLocalDB | Out-Host

# 1b. Reconcile a stale SupportPlatform DB ------------------------------------
# A previous run that was force-killed (window closed instead of Ctrl+C) can leave
# the DB half-attached, or its .mdf/.ldf on disk with no catalog entry. Either way
# the API's CREATE DATABASE then fails with SQL error 5170 / 1801. Make the state
# consistent so EF can just (re)create it. Best effort - never blocks the run.
function Invoke-LocalDbScalar([string]$database, [string]$sql) {
    $cs = "Server=(localdb)\MSSQLLocalDB;Database=$database;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
    try { $cmd = $conn.CreateCommand(); $cmd.CommandText = $sql; return $cmd.ExecuteScalar() }
    finally { $conn.Dispose() }
}
try {
    $inCatalog = Invoke-LocalDbScalar 'master' "SELECT 1 FROM sys.databases WHERE name = 'SupportPlatform'"
    if ($inCatalog) {
        $usable = $false
        try { Invoke-LocalDbScalar 'SupportPlatform' 'SELECT 1' | Out-Null; $usable = $true } catch { }
        if (-not $usable) {
            Write-Host '-> SupportPlatform DB is present but unusable (stale) - dropping so it can be recreated' -ForegroundColor Yellow
            try { Invoke-LocalDbScalar 'master' 'ALTER DATABASE [SupportPlatform] SET SINGLE_USER WITH ROLLBACK IMMEDIATE' | Out-Null } catch { }
            Invoke-LocalDbScalar 'master' 'DROP DATABASE [SupportPlatform]' | Out-Null
            $inCatalog = $null
        }
    }
    if (-not $inCatalog) {
        foreach ($f in @("$env:USERPROFILE\SupportPlatform.mdf", "$env:USERPROFILE\SupportPlatform_log.ldf")) {
            if (Test-Path $f) { Write-Host "-> removing orphan DB file $f" -ForegroundColor Yellow; Remove-Item $f -Force }
        }
    }
} catch {
    Write-Warning "LocalDB preflight skipped ($($_.Exception.Message)). If the API fails with 'CREATE DATABASE ... already exists', see README - Troubleshooting."
}

# 2. Client deps ---------------------------------------------------------------
if (-not (Test-Path (Join-Path $clientDir 'node_modules'))) {
    Write-Host '-> npm install (first run)'
    Push-Location $clientDir; npm install; Pop-Location
}

# 3. Build server ---------------------------------------------------------------
if (-not $SkipBuild) {
    Write-Host '-> dotnet build'
    Push-Location $serverDir; dotnet build SupportPlatform.sln --nologo; Pop-Location
}

# 4. Launch API + client ---------------------------------------------------------------
# Child processes inherit these; set on the current process (works in PS 5.1 and 7).
$env:ASPNETCORE_ENVIRONMENT       = 'Development'
$env:ASPNETCORE_HTTP_PORTS        = "$ApiPort"
$env:ConnectionStrings__SqlServer = $connString
$env:VITE_API_PROXY_TARGET        = "http://localhost:$ApiPort"

# Run each server through cmd.exe /c so PATH resolves 'dotnet' / 'npm' (npm is npm.cmd
# on Windows and cannot be launched directly by Start-Process). taskkill /T on cleanup
# takes down the whole child tree (cmd -> dotnet/node -> app).
$procs = @()
try {
    Write-Host "-> API    http://localhost:$ApiPort  (Swagger at /swagger)" -ForegroundColor Green
    $procs += Start-Process -PassThru -NoNewWindow -WorkingDirectory $serverDir `
        -FilePath $env:ComSpec -ArgumentList '/c','dotnet','run','--project','src/Api','--no-build'

    Write-Host "-> client http://localhost:$ClientPort" -ForegroundColor Green
    $procs += Start-Process -PassThru -NoNewWindow -WorkingDirectory $clientDir `
        -FilePath $env:ComSpec -ArgumentList '/c','npm','run','dev','--','--port',"$ClientPort"

    Write-Host ''
    Write-Host "Open http://localhost:$ClientPort   -   Ctrl+C to stop both." -ForegroundColor Cyan
    while ($true) {
        Start-Sleep -Seconds 1
        if ($procs | Where-Object { $_ -and $_.HasExited }) { break }
    }
}
finally {
    Write-Host ''
    Write-Host '-> stopping servers'
    foreach ($p in $procs) {
        if ($p -and -not $p.HasExited) {
            & taskkill /T /F /PID $p.Id 2>$null | Out-Null
        }
    }
}
