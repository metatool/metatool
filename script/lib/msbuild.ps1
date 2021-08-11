#old way
# Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
# Enter-VsDevShell -InstanceId f86c8b33
$VsWherePath = "`"${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe`""
$config = Invoke-Expression "& $VsWherePath -latest -format json" | ConvertFrom-Json
$base = $config.InstallationPath
. "$base\Common7\Tools\Launch-VsDevShell.ps1"