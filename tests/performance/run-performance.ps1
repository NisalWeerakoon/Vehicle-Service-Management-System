param(
    [string]$JMeter = ""
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot

$plan = Join-Path $root "billing-invoices-load.jmx"
$tokenPath = Join-Path $root "token.csv"

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

$resultsDir = Join-Path $root "reports"

$jtl = Join-Path $resultsDir "load-$timestamp.jtl"
$dashboard = Join-Path $resultsDir "dashboard-$timestamp"

# Verify files exist
foreach ($file in @($JMeter, $plan, $tokenPath)) {
    if (!(Test-Path $file)) {
        throw "Required file missing: $file"
    }
}

New-Item -ItemType Directory -Force `
    -Path $resultsDir | Out-Null

# Confirm local Billing is accessible and authentication works
$jwt = (Get-Content $tokenPath -Raw).Trim()

$httpCode = curl.exe -sS `
    --max-time 15 `
    -o NUL `
    -w "%{http_code}" `
    -H "Authorization: Bearer $jwt" `
    "http://127.0.0.1:5180/api/invoices/me"

if ($LASTEXITCODE -ne 0 -or "$httpCode" -ne "200") {
    throw "Billing API preflight failed. HTTP: $httpCode"
}

Write-Host "Billing API preflight passed."
Write-Host "Starting JMeter performance test..."

& $JMeter -n `
    -t $plan `
    "-JtokenFile=$tokenPath" `
    -l $jtl `
    -e `
    -o $dashboard

if ($LASTEXITCODE -ne 0) {
    throw "JMeter execution failed."
}

Write-Host "JMeter completed."
Write-Host "Results: $jtl"
Write-Host "Dashboard: $dashboard"

Start-Process (Join-Path $dashboard "index.html")
