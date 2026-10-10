param([string]$BaseUri = "http://localhost:5001")

$ErrorActionPreference = "Stop"

function Assert-HttpFailure {
    param([scriptblock]$Action, [int]$ExpectedStatus, [string]$Label)
    try {
        $null = & $Action
        throw "Expected HTTP $ExpectedStatus for: $Label"
    }
    catch {
        $response = $_.Exception.Response
        if ($null -eq $response) { throw }
        if ([int]$response.StatusCode -ne $ExpectedStatus) { throw }
        Write-Host "PASS: $Label -> HTTP $ExpectedStatus" -ForegroundColor Green
    }
}

$health = Invoke-RestMethod "$BaseUri/health"
if ($health.database -ne "Connected") { throw "Database is not connected." }

$ranch = Invoke-RestMethod -Uri "$BaseUri/api/v1/ranches" -Method Post `
    -ContentType "application/json" -Body (@{ name = "Fundo SmartFarm - prueba" } | ConvertTo-Json)
if ([string]::IsNullOrWhiteSpace($ranch.id)) { throw "Ranch ID was not returned." }
Write-Host "PASS: ranch registered" -ForegroundColor Green

$body = @{ ranchId = $ranch.id; earTag = "PE-APU-001"; lotId = $null } | ConvertTo-Json
$animal = Invoke-RestMethod -Uri "$BaseUri/api/v1/cattle" -Method Post `
    -ContentType "application/json" -Body $body
if ($animal.earTag -ne "PE-APU-001") { throw "Unexpected cattle response." }
Write-Host "PASS: animal registered" -ForegroundColor Green

$read = Invoke-RestMethod "$BaseUri/api/v1/cattle/$($animal.id)?ranchId=$($ranch.id)"
if ($read.id -ne $animal.id -or $read.ranchId -ne $ranch.id) { throw "Stored animal differs." }
Write-Host "PASS: stored animal retrieved" -ForegroundColor Green

$duplicate = @{ ranchId = $ranch.id; earTag = " pe-apu-001 "; lotId = $null } | ConvertTo-Json
Assert-HttpFailure -ExpectedStatus 409 -Label "duplicate normalized ear tag" -Action {
    Invoke-RestMethod -Uri "$BaseUri/api/v1/cattle" -Method Post -ContentType "application/json" -Body $duplicate
}

$invalid = @{ ranchId = $ranch.id; earTag = " "; lotId = $null } | ConvertTo-Json
Assert-HttpFailure -ExpectedStatus 400 -Label "empty ear tag" -Action {
    Invoke-RestMethod -Uri "$BaseUri/api/v1/cattle" -Method Post -ContentType "application/json" -Body $invalid
}

$missingId = [guid]::NewGuid().ToString()
Assert-HttpFailure -ExpectedStatus 404 -Label "unknown animal" -Action {
    Invoke-RestMethod "$BaseUri/api/v1/cattle/${missingId}?ranchId=$($ranch.id)"
}

$otherRanch = [guid]::NewGuid().ToString()
Assert-HttpFailure -ExpectedStatus 404 -Label "animal queried from another ranch" -Action {
    Invoke-RestMethod "$BaseUri/api/v1/cattle/$($animal.id)?ranchId=$otherRanch"
}

$invalidRanch = @{ ranchId = $otherRanch; earTag = "PE-APU-002"; lotId = $null } | ConvertTo-Json
Assert-HttpFailure -ExpectedStatus 404 -Label "registration in unknown ranch" -Action {
    Invoke-RestMethod -Uri "$BaseUri/api/v1/cattle" -Method Post -ContentType "application/json" -Body $invalidRanch
}

$listed = Invoke-RestMethod "$BaseUri/api/v1/cattle?ranchId=$($ranch.id)"
if (@($listed).Count -ne 1) { throw "Expected exactly one animal after duplicate rejection." }
Write-Host "PASS: duplicate did not create another animal" -ForegroundColor Green

Write-Host "All checks passed. Capture this output as real execution evidence." -ForegroundColor Green
Write-Host "Ranch ID: $($ranch.id)"
Write-Host "Cattle ID: $($animal.id)"
$animal | ConvertTo-Json
