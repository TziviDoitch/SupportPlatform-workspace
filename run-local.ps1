<#
.SYNOPSIS
  Run SupportPlatform locally WITHOUT Docker.

.DESCRIPTION
  Runs the stack directly on the host, without Docker — useful when Docker Desktop
  isn't available. The intended one-command flow is still `cd infra; docker compose up --build`.

    * db     - uses the SQL Server LocalDB instance already installed on Windows
               (MSSQLLocalDB). No container.
    * api    - dotnet run (src/Api). In Development it auto-applies EF migrations
               and seeds demo data into the LocalDB database.
    * client - npm run dev (Vite), with /api/* proxied to the api above.

  It does NOT start Docker. If you later install Docker Desktop, use the intended
  one-command flow instead:  cd infra ; docker compose up --build

  Press Ctrl+C to stop both servers.
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

# 2. Client deps ---------------------------------------------------------------
# Always run: node_modules can exist but be stale after package.json changes
# (a missing dep only surfaces later as a Vite "could not be resolved" error).
# npm install is a fast no-op when everything is already up to date.
Write-Host '-> npm install'
Push-Location $clientDir; npm install; Pop-Location

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
