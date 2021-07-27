# install folder
$programFolder = Read-Host "Enter the foler path to unzip the metatool software: (press 'enter' to install to {$env:ProgramData}"

if([string]::IsNullOrWhiteSpace($programFolder)) {
    $programFolder = $env:ProgramData;
} else {
    if(!(Test-Path $programFolder -PathType Container)){
        New-Item -ItemType Directory -Force -Path $programFolder
    }
}

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
# Create a temporary folder to download to.
$tempFolder = Join-Path $env:TEMP "metatool"
New-Item $tempFolder -ItemType Directory -Force

# Get the latest release
$latestRelease = Invoke-WebRequest "https://api.github.com/repos/metatool/metatool/releases/latest" |
ConvertFrom-Json | 
Select-Object tag_name
$tag_name =  $latestRelease.tag_name

# Download the zip
Write-Host "Downloading latest version ($tag_name)"
$client = New-Object "System.Net.WebClient"
$url = "https://github.com/metatool/metatool/releases/download/$tag_name/metatool_$tag_name.zip"
$zipFile = Join-Path $tempFolder "metatool.zip"
$client.DownloadFile($url,$zipFile)

$installationFolder = Join-Path $programFolder "metatool"
Microsoft.PowerShell.Archive\Expand-Archive $zipFile -DestinationPath $installationFolder -Force
Remove-Item $tempFolder -Recurse -Force

$path = [System.Environment]::GetEnvironmentVariable("path", [System.EnvironmentVariableTarget]::User);
# Get all paths except paths to old dotnet.script installations. 
$paths = $path.Split(";") -inotlike "*metatool*" 
# Add the installation folder to the path
$paths += Join-Path $installationFolder "metatool"
# Create the new path string
$path = $paths -join ";"

[System.Environment]::SetEnvironmentVariable("path", $path, [System.EnvironmentVariableTarget]::User)

Write-Host "Successfully installed version ($tag_name): $installationFolder\Metatool.exe "