# ==============================================================================
#  Build-MsixPackage.ps1  —  Builds and (optionally) signs the MSIX package.
# ==============================================================================
#  Parameters
#    -Version        4-part version string.  Must match UI.csproj <Version>.
#    -PfxPath        Path to .pfx file used for signing.
#    -PfxPassword    Password for the .pfx file.
#    -OutputDir      Folder that receives the .msix and .appinstaller files.
#    -Sign           If present, signs the package after building.
# ==============================================================================

param(
    [string]$Version     = "1.0.0.0",
    [string]$PfxPath     = "$PSScriptRoot\OrthoSpineAI.pfx",
    [string]$PfxPassword = "",
    [string]$OutputDir   = "$PSScriptRoot\..\..\artifacts\msix",
    [switch]$Sign
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$wapproj = "$PSScriptRoot\OrthoSpineAI.Package.wapproj"
$uiproj  = "$PSScriptRoot\..\OrthoSpineAI.UI\OrthoSpineAI.UI.csproj"

# ── 1. Verify version parity ────────────────────────────────────────────────
Write-Host "Checking version parity..." -ForegroundColor Cyan
[xml]$ui = Get-Content $uiproj
$uiVersion = ($ui.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if ($uiVersion -ne $Version) {
    Write-Warning "UI project version ($uiVersion) differs from requested version ($Version)."
    Write-Warning "Updating OrthoSpineAI.UI.csproj..."
    $content = Get-Content $uiproj -Raw
    $content = $content -replace '<Version>[^<]+</Version>', "<Version>$Version</Version>"
    $content = $content -replace '<AssemblyVersion>[^<]+</AssemblyVersion>', "<AssemblyVersion>$Version</AssemblyVersion>"
    $content = $content -replace '<FileVersion>[^<]+</FileVersion>', "<FileVersion>$Version</FileVersion>"
    Set-Content $uiproj $content
}

# Update .wapproj PackageVersion
$wContent = Get-Content $wapproj -Raw
$wContent = $wContent -replace '<PackageVersion>[^<]+</PackageVersion>', "<PackageVersion>$Version</PackageVersion>"
Set-Content $wapproj $wContent

# Update Package.appxmanifest Identity Version
$manifest = "$PSScriptRoot\Package.appxmanifest"
$mContent = Get-Content $manifest -Raw
$mContent = $mContent -replace 'Version="[\d\.]+"', "Version=`"$Version`""
Set-Content $manifest $mContent

# Update .appinstaller versions
$appinstaller = "$PSScriptRoot\OrthoSpineAI.appinstaller"
$aiContent = Get-Content $appinstaller -Raw
$aiContent = $aiContent -replace 'Version="[\d\.]+"', "Version=`"$Version`""
Set-Content $appinstaller $aiContent

Write-Host "All version fields updated to $Version." -ForegroundColor Green

# ── 2. Build ────────────────────────────────────────────────────────────────
$outDir = [System.IO.Path]::GetFullPath($OutputDir)
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Write-Host "Building MSIX package (Release|x64)..." -ForegroundColor Cyan
msbuild $wapproj `
    /p:Configuration=Release `
    /p:Platform=x64 `
    /p:AppxPackageDir="$outDir\\" `
    /p:PackageVersion=$Version `
    /p:UapAppxPackageBuildMode=SideloadOnly `
    /p:AppxBundle=Never `
    /restore

Write-Host "Build complete. Output: $outDir" -ForegroundColor Green

# ── 3. Optional signing ─────────────────────────────────────────────────────
if ($Sign) {
    $msix = Get-ChildItem $outDir -Filter "*.msix" | Select-Object -First 1
    if (-not $msix) { throw "No .msix file found in $outDir" }

    Write-Host "Signing $($msix.Name) ..." -ForegroundColor Cyan
    $signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
    if (-not (Test-Path $signtool)) {
        $signtool = (Get-Command signtool -ErrorAction SilentlyContinue)?.Source
    }
    if (-not $signtool) { throw "signtool.exe not found. Install Windows SDK." }

    & $signtool sign /fd SHA256 /a /f $PfxPath /p $PfxPassword $msix.FullName
    Write-Host "Signed successfully." -ForegroundColor Green
}

# ── 4. Copy .appinstaller to output ─────────────────────────────────────────
Copy-Item $appinstaller $outDir -Force
Write-Host "Copied OrthoSpineAI.appinstaller to $outDir" -ForegroundColor Green

Write-Host ""
Write-Host "Done!  Upload the contents of '$outDir' to your hosting server." -ForegroundColor Cyan
