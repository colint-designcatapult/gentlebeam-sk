[CmdletBinding()]
param(
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$solutionRoot = Split-Path -Parent $PSScriptRoot
$configuration = "Debug"

$relayProject = Join-Path $solutionRoot "GcbTelemetryRelay\GcbTelemetryRelay.csproj"
$externalProject = Join-Path $solutionRoot "Xcc\Heracles\Heracles.Outdoor\Heracles.External.csproj"
$indoorProject = Join-Path $solutionRoot "Xcc\Heracles\Heracles.Indoor\Heracles.Indoor.csproj"

if (-not $NoBuild) {
    foreach ($project in @($relayProject, $externalProject, $indoorProject)) {
        & dotnet build $project --configuration $configuration --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed for $project."
        }
    }
}

$relayDirectory = Join-Path $solutionRoot "GcbTelemetryRelay\bin\Debug\net8.0"
$externalDirectory = Join-Path $solutionRoot "Xcc\Heracles\Heracles.Outdoor\bin\Debug\net8.0-windows7.0"
$indoorDirectory = Join-Path $solutionRoot "Xcc\Heracles\Heracles.Indoor\bin\Debug\net8.0-windows7.0"

$relayExecutable = Join-Path $relayDirectory "GcbTelemetryRelay.exe"
$externalExecutable = Join-Path $externalDirectory "Heracles.External.exe"
$indoorExecutable = Join-Path $indoorDirectory "Heracles.Indoor.exe"
$externalSettings = Join-Path $solutionRoot "Xcc\Heracles\Heracles.Outdoor\appsettings.local-dual.json"
$indoorSettings = Join-Path $solutionRoot "Xcc\Heracles\Heracles.Indoor\appsettings.local-dual.json"

foreach ($path in @($relayExecutable, $externalExecutable, $indoorExecutable, $externalSettings, $indoorSettings)) {
    if (-not (Test-Path $path)) {
        throw "Required local-development artifact was not found: $path"
    }
}

$processes = [System.Collections.Generic.List[System.Diagnostics.Process]]::new()

try {
    $relay = Start-Process -FilePath $relayExecutable -WorkingDirectory $relayDirectory -PassThru
    $processes.Add($relay)
    Start-Sleep -Milliseconds 300
    $relay.Refresh()
    if ($relay.HasExited) {
        throw "The telemetry relay could not bind UDP port 40020. Ensure no Heracles application is already using it."
    }

    $externalArgument = "--appsettings=`"$externalSettings`""
    $external = Start-Process -FilePath $externalExecutable -ArgumentList $externalArgument -WorkingDirectory $externalDirectory -PassThru
    $processes.Add($external)

    $indoorArgument = "--appsettings=`"$indoorSettings`""
    $indoor = Start-Process -FilePath $indoorExecutable -ArgumentList $indoorArgument -WorkingDirectory $indoorDirectory -PassThru
    $processes.Add($indoor)

    Write-Host "Local dual-CNC configuration started."
    Write-Host "  Firmware telemetry: UDP 40020 -> one relay socket"
    Write-Host "  External CNC:      127.0.0.1:40021"
    Write-Host "  Indoor CNC:        127.0.0.1:40022"
    Write-Host "Closing either CNC application stops the complete local configuration."

    while (-not $external.HasExited -and -not $indoor.HasExited) {
        Start-Sleep -Seconds 1
        $external.Refresh()
        $indoor.Refresh()
    }
}
finally {
    foreach ($process in $processes) {
        $process.Refresh()
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id
        }
    }
}
