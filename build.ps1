# Build variables
$dnvmVersion = "1.0.0-beta4";

Push-Location $PSScriptRoot

# Clean
if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

# Install DNVM
& where.exe dnvm 2>&1 | Out-Null
if($LASTEXITCODE -ne 0)
{
    &{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}
}

# Install DNX
dnvm install $dnvmVersion -r CoreCLR
dnvm install $dnvmVersion -r CLR
dnvm use $dnvmVersion -r CLR

# Package restore
Get-ChildItem -Path . -Filter *.xproj -Recurse | ForEach-Object { dnu restore ("""" + $_.DirectoryName + """") }

# Set build number
$env:DNX_BUILD_VERSION = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1}[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
Write-Host "Build number:" $env:DNX_BUILD_VERSION

# Build/package
Get-ChildItem -Path .\src -Filter *.xproj -Recurse | ForEach-Object { dnu pack ("""" + $_.DirectoryName + """") --configuration Release --out .\artifacts\packages; if($LASTEXITCODE -ne 0) { throw "Build failed on "  + $_.DirectoryName } }

# Test
Get-ChildItem -Path .\test -Filter *.xproj -Recurse | ForEach-Object { dnx ("""" + $_.DirectoryName + """") test; if($LASTEXITCODE -ne 0) { throw "Tests failed on "  + $_.DirectoryName } }

Pop-Location