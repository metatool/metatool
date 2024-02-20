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

Push-Location .
. "$PSScriptRoot/lib/msbuild.ps1"
try {
    $source = Resolve-Path $PSScriptRoot\..
    $target = $rebuild ? "rebuild" : "build"
    $config = $release ? "Release" : "Debug"
    dotnet restore $source\src\Metatool.sln
    msbuild $source\src\Metatool.sln -t:$target /p:Configuration=$config
    if ( $LASTEXITCODE -ne 0 ) {
        throw "build fail!"
    }
}
finally {
    Pop-Location
}