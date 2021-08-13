[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
    $rebuild,
    [switch]
    [Alias("l")]
    $localRelease
)
$metatool = Resolve-Path $PSScriptRoot\..
$publishing = "$metatool\exe\publishing"

. $PSScriptRoot/lib/msbuild.ps1

if (Test-Path $publishing) {
    Remove-Item $publishing -Force -Recurse
}

msbuild /t:publish  "$metatool\src\app\Metaseed.Metatool\Metaseed.Metatool.csproj"  /p:DeployOnBuild=true  /p:Configuration=Release /p:PublishDir="$publishing" /p:CopyOutputSymbolsToPublishDirectory=false /p:DebugType=None /p:DebugSymbols=false /p:SolutionDir="$metatool\src\" /p:PublishProfile="$metatool\src\app\Metaseed.Metatool\Properties\PublishProfiles\metatool.pubxml"
if ( $LASTEXITCODE -ne 0 ) {    
    throw "publish fail!"
}
if ($localRelease) {
    Copy-Item "$metatool\exe\publishing\Metatool.exe" "$metatool\exe\publish" -Force
}
. $PSScriptRoot/lib/Build-Tool.ps1

"Metatool.Tools.MetaKeyboard", "Metatool.Tools.Software" | ForEach-Object {
    Build-Tool $_ -release: $true -rebuild: $rebuild
    if ($localRelease) {
        Copy-Item "$metatool\exe\publishing\tools\$_" "$metatool\exe\publish\tools\$_" -Force
    }
}


$metaSoftware = "$metatool\exe\publish\tools\Metatool.Tools.Software"
$metaSoftwarePublishing = "$metatool\exe\publishing\tools\Metatool.Tools.Software"

Copy-Item "$metaSoftware\software" -Destination "$metaSoftwarePublishing\software" -Recurse
Copy-Item "$metaSoftware\softwareConfig" -Destination "$metaSoftwarePublishing\softwareConfig" -Recurse

sl $PSScriptRoot