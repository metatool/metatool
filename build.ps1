[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
    $release,
    [switch]
    [Alias("re")]
    $rebuild
)
$source = "M:\Workspace\metatool" 

Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell a4cdb433
Set-Location $source
$target = $rebuild ? "rebuild" : "build"
$config = $release ? "Release" : "Debug"
msbuild src\Metatool.sln -t:$target /p:Configuration=$config