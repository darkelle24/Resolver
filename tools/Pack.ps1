param(
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$')][string]$Version = '0.1.0',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/packages')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = [IO.Path]::GetFullPath($OutputDirectory)
foreach ($project in @('core/Resolver.Core.csproj', 'packaging/Resolver.Godot.csproj')) {
    & dotnet pack (Join-Path $root $project) -c Release "-p:Version=$Version" --output $output
    if ($LASTEXITCODE -ne 0) { throw "Packaging failed: $project" }
}
Write-Host "Packages ready: $output"
