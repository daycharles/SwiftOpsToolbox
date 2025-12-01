<#
PowerShell helper to publish the WPF app and optionally build an Inno Setup installer.
Usage:
  .\publish-and-package.ps1 -Configuration Release -Runtime win-x64 -OutDir ./publish -PackageInstaller

Requirements:
- .NET 8 SDK installed and on PATH
- (Optional) Inno Setup 6 installed and its ISCC.exe available on PATH to create installer
#>
[CmdletBinding()]
param(
    [string]$ProjectPath = "./SwiftOpsToolbox.csproj",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutDir = "./publish",
    [switch]$PackageInstaller
)

Set-StrictMode -Version Latest

$projectFull = Resolve-Path $ProjectPath
Write-Host "Publishing project: $projectFull" -ForegroundColor Cyan

# Publish single-file, self-contained for target runtime
$publishArgs = @(
    'publish', "$projectFull",
    '-c', $Configuration,
    '-r', $Runtime,
    '/p:PublishSingleFile=true',
    '/p:SelfContained=true',
    '/p:PublishTrimmed=false',
    '/p:IncludeAllContentForSelfExtract=true',
    '-o', (Resolve-Path $OutDir)
)

Write-Host "Running: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
$proc = Start-Process -FilePath dotnet -ArgumentList $publishArgs -NoNewWindow -Wait -PassThru
if ($proc.ExitCode -ne 0) { throw "dotnet publish failed with exit code $($proc.ExitCode)" }
Write-Host "Publish complete. Output: $(Resolve-Path $OutDir)" -ForegroundColor Green

if ($PackageInstaller)
{
    # Locate Inno Setup Compiler (ISCC.exe)
    $iscc = Get-Command -ErrorAction SilentlyContinue iscc.exe
    if (-not $iscc)
    {
        Write-Warning "ISCC.exe (Inno Setup Compiler) not found on PATH. Please install Inno Setup or add ISCC to PATH. Skipping installer packaging."
        return
    }

    $issPath = Join-Path (Resolve-Path ..\installer) 'SwiftOpsToolbox.iss'
    if (-not (Test-Path $issPath)) { Write-Warning "Installer script not found: $issPath"; return }

    Write-Host "Building Inno Setup installer with ISCC: $($iscc.Path)" -ForegroundColor Cyan
    $args = @($issPath)
    $p = Start-Process -FilePath $iscc.Path -ArgumentList $args -NoNewWindow -Wait -PassThru
    if ($p.ExitCode -ne 0) { throw "ISCC failed with exit code $($p.ExitCode)" }
    Write-Host "Installer build finished." -ForegroundColor Green
}
else
{
    Write-Host "PackageInstaller not requested. Skipping installer creation." -ForegroundColor Yellow
}
