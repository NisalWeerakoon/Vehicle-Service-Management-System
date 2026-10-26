
$ErrorActionPreference = "Stop"
$folder = $PSScriptRoot

$auth = @{}

Get-Content (Join-Path $folder "local-jwt.env") |
    ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            $auth[$matches[1]] = $matches[2]
        }
    }

foreach ($name in @("JWT_KEY", "JWT_ISSUER", "JWT_AUDIENCE")) {
    if ([string]::IsNullOrWhiteSpace($auth[$name])) {
        throw "Missing local JWT configuration: $name"
    }
}

function ConvertTo-Base64Url([string]$value) {
    [Convert]::ToBase64String(
        [Text.Encoding]::UTF8.GetBytes($value)
    ).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

$header = @{
    alg = "HS256"
    typ = "JWT"
} | ConvertTo-Json -Compress

$payload = @{
    iss = $auth["JWT_ISSUER"]
    aud = $auth["JWT_AUDIENCE"]
    sub = "1"
    role = "Customer"
    customerId = "1"
    iat = $now
    nbf = $now
    exp = $now + 3600
} | ConvertTo-Json -Compress

$data = "$(ConvertTo-Base64Url $header).$(ConvertTo-Base64Url $payload)"

$hmac = [Security.Cryptography.HMACSHA256]::new(
    [Text.Encoding]::UTF8.GetBytes($auth["JWT_KEY"])
)

try {
    $signature = [Convert]::ToBase64String(
        $hmac.ComputeHash(
            [Text.Encoding]::UTF8.GetBytes($data)
        )
    ).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}
finally {
    $hmac.Dispose()
}

$token = "$data.$signature"

[IO.File]::WriteAllText(
    (Join-Path $folder "token.csv"),
    $token,
    [Text.UTF8Encoding]::new($false)
)

Write-Host "Fresh local JWT generated successfully."
Write-Host "Valid for 60 minutes."
