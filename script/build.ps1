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

. "$PSScriptRoot/lib/msbuild.ps1"

$source = Resolve-Path $PSScriptRoot\..
$target = $rebuild ? "rebuild" : "build"
$config = $release ? "Release" : "Debug"
msbuild $source\src\Metatool.sln -t:$target /p:Configuration=$config
if ( $LASTEXITCODE -ne 0 ) {
    throw "build fail!"
}