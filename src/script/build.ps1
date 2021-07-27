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
$source = Split-Path $script:MyInvocation.MyCommand.Path

. $source/script/msbuild.ps1

Set-Location $source
$target = $rebuild ? "rebuild" : "build"
$config = $release ? "Release" : "Debug"
msbuild src\Metatool.sln -t:$target /p:Configuration=$config
if ( $LASTEXITCODE -ne 0 ) {
    throw "build fail!"
}