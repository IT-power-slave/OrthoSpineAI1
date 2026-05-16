# ==============================================================================
#  Create-DevCertificate.ps1
#  Self-signed certificate for local development / sideloading.
#  DO NOT use self-signed certificates for Microsoft Store submissions.
# ==============================================================================
#  Usage:
#    .\Create-DevCertificate.ps1
#
#  This script:
#    1. Creates a self-signed code-signing certificate.
#    2. Exports it as OrthoSpineAI.pfx  (referenced in .wapproj).
#    3. Installs the public cert into Trusted Root so Windows trusts the package.
# ==============================================================================

param(
    [string]$Publisher  = "CN=OrthoSpineAI",
    [string]$PfxPath    = "$PSScriptRoot\OrthoSpineAI.pfx",
    [string]$PfxPassword = "DevCert123!"   # Change before sharing
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "Creating self-signed certificate for '$Publisher'..." -ForegroundColor Cyan

$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject $Publisher `
    -KeyUsage DigitalSignature `
    -FriendlyName "OrthoSpineAI Dev Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") `
    -NotAfter (Get-Date).AddYears(3)

Write-Host "Certificate thumbprint: $($cert.Thumbprint)" -ForegroundColor Green

# Export PFX (private key + cert)
$securePassword = ConvertTo-SecureString -String $PfxPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $PfxPath -Password $securePassword | Out-Null
Write-Host "PFX exported to: $PfxPath" -ForegroundColor Green

# Install public cert to Trusted Root (requires admin elevation)
Write-Host "Installing certificate to Trusted Root (requires elevation)..." -ForegroundColor Yellow
$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","LocalMachine")
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close()
Write-Host "Certificate installed to Trusted Root." -ForegroundColor Green

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Copy the thumbprint above into PackageCertificateThumbprint in OrthoSpineAI.Package.wapproj"
Write-Host "  2. Build the package:  msbuild OrthoSpineAI.Package.wapproj /p:Configuration=Release"
Write-Host "  3. Sign manually (if needed):"
Write-Host "       signtool sign /fd SHA256 /a /f OrthoSpineAI.pfx /p $PfxPassword OrthoSpineAI.msix"
