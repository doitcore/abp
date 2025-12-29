param (
    [string]$version = "0.7.6",
    [string]$channel = "beta"
)
$installdir = "$env:LOCALAPPDATA\abp-studio\"

Write-Host "----------------------------------------"
Write-Host "Switching to ABP Studio version $version on channel $channel..."
Write-Host "----------------------------------------"
Write-Host "Installing to $installdir"


$url = "https://abp.io/api/abp-studio/download/r/windows/abp-studio-$version-$channel-full.nupkg"
$output = "abp-studio-$version-$channel-full.nupkg"

$outputPath = "$installdir\packages\$output"

if (Test-Path $outputPath) {
    Write-Host "File $output already exists. Skipping download."
} else {
    Write-Host "Downloading $url to $outputPath"
    Invoke-WebRequest -Uri $url -OutFile $outputPath
}
Write-Host "----------------------------------------"

$installdirUpdate = "$installdir\Update.exe"

Write-Host "Running $installdirUpdate apply --package $outputPath"
Invoke-Expression "$installdirUpdate apply --package $outputPath"

Write-Host "----------------------------------------"
Write-Host "ABP Studio version $version on channel $channel installed successfully."
Write-Host "----------------------------------------"