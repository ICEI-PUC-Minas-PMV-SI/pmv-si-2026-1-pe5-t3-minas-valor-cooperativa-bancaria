# dev-db.ps1 - manage the local portable PostgreSQL used by Minas Valor (dev only)
# Usage:  .\dev-db.ps1 start | stop | status
#
# The database lives entirely under your user profile (no admin required):
#   %LocalAppData%\minasvalor-pg
# Connection (see appsettings.json): Host=localhost;Port=5432;Database=minas-valor;Username=postgres;Password=123456
# (initialized with "trust" auth, so the password is accepted as-is)

param([ValidateSet('start', 'stop', 'status')][string]$Action = 'status')

$base    = Join-Path $env:LocalAppData 'minasvalor-pg'
$pg      = Join-Path $base 'pgsql'
$data    = Join-Path $base 'data'
$log     = Join-Path $base 'pg.log'
$pgctl   = Join-Path $pg 'bin\pg_ctl.exe'
$isready = Join-Path $pg 'bin\pg_isready.exe'

if (-not (Test-Path $pgctl)) {
    Write-Error "PostgreSQL not found at $pg. Re-run the setup first."
    exit 1
}

switch ($Action) {
    'start' { & $pgctl -D $data -l $log -o '-p 5432' -w start }
    'stop'  { & $pgctl -D $data -m fast stop }
    'status' {
        & $isready -h localhost -p 5432
        if ($LASTEXITCODE -eq 0) { 'PostgreSQL: running on localhost:5432' }
        else { 'PostgreSQL: NOT running  (run: .\dev-db.ps1 start)' }
    }
}
