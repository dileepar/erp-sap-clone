# SAP Clone Development Run Script for Windows
param(
    [switch]$SkipKill,
    [switch]$BackendOnly,
    [switch]$FrontendOnly,
    [switch]$Help
)

function Write-Info($Message) {
    Write-Host $Message -ForegroundColor Green
}

function Write-Warning($Message) {
    Write-Host $Message -ForegroundColor Yellow
}

function Write-Error($Message) {
    Write-Host $Message -ForegroundColor Red
}

function Kill-DevProcesses {
    Write-Warning "Killing existing development processes..."
    
    # Kill .NET processes
    try {
        $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
        if ($dotnetProcesses) {
            $dotnetProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Info "Killed .NET processes"
        }
    } catch {
        Write-Error "Could not kill .NET processes"
    }

    # Kill Node.js processes
    try {
        $nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
        if ($nodeProcesses) {
            $nodeProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Info "Killed Node.js processes"
        }
    } catch {
        Write-Error "Could not kill Node.js processes"
    }

    # Kill Bun processes
    try {
        $bunProcesses = Get-Process -Name "bun" -ErrorAction SilentlyContinue
        if ($bunProcesses) {
            $bunProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Info "Killed Bun processes"
        }
    } catch {
        Write-Error "Could not kill Bun processes"
    }

    Write-Info "Process cleanup completed"
    Start-Sleep -Seconds 2
}

function Start-Backend {
    Write-Info "Starting .NET Backend API on https://localhost:7000"
    
    Push-Location "src\API\SAP.API"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet watch run --urls=https://localhost:7000"
    Pop-Location
    
    Write-Info "Backend started in new window"
}

function Start-Frontend {
    Write-Info "Starting React Frontend on http://localhost:3000"
    
    Push-Location "src\Web\sap-web"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "bun run dev"
    Pop-Location
    
    Write-Info "Frontend started in new window"
}

# Main execution
Clear-Host

if ($Help) {
    Write-Host "SAP Clone Development Script" -ForegroundColor Blue
    Write-Host "Usage: .\run-dev.ps1 [options]" -ForegroundColor Yellow
    Write-Host "Options:"
    Write-Host "  -SkipKill      Skip killing existing processes"
    Write-Host "  -BackendOnly   Only start the .NET backend API"
    Write-Host "  -FrontendOnly  Only start the React frontend"
    Write-Host "  -Help          Show this help message"
    exit 0
}

Write-Host "SAP Clone Development Environment" -ForegroundColor Blue
Write-Host "=================================" -ForegroundColor Blue

# Kill existing processes unless skipped
if (-not $SkipKill) {
    Kill-DevProcesses
}

# Start services based on parameters
if ($BackendOnly) {
    Start-Backend
} elseif ($FrontendOnly) {
    Start-Frontend
} else {
    Start-Backend
    Start-Sleep -Seconds 3
    Start-Frontend
}

Write-Host ""
Write-Info "Development servers are running!"
Write-Host "Backend API:  https://localhost:7000" -ForegroundColor Yellow
Write-Host "Frontend:     http://localhost:3000" -ForegroundColor Yellow
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 