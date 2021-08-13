[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
    $rebuild
)
$metatool = Resolve-Path $PSScriptRoot\..
$publishing = "$metatool\exe\publishing"

. $PSScriptRoot/lib/msbuild.ps1

if (Test-Path $publishing) {
    Remove-Item $publishing -Force -Recurse
}

msbuild /t:publish  "$metatool\src\app\Metaseed.Metatool\Metaseed.Metatool.csproj"  /p:DeployOnBuild=true  /p:Configuration=Release /p:Publis   hDir="$publishing" /p:CopyOutputSymbolsToPublishDirectory=false /p:DebugType=None /p:DebugSymbols=false /p:SolutionDir="$metatool\src\" /p:PublishProfile="$metatool\src\app\Metaseed.Metatool\Properties\PublishProfiles\metatool.pubxml"
if ( $LASTEXITCODE -ne 0 ) {    
    throw "publish fail!"
}

. $PSScriptRoot/lib/Build-Tool.ps1

"Metatool.Tools.MetaKeyboard", "Metatool.Tools.Software" | ForEach-Object {
    Build-Tool $_ -release: $true -rebuild: $rebuild
}

$metaSoftware = "$metatool\exe\publish\tools\Metatool.Tools.Software"
$metaSoftwarePublishing = "$metatool\exe\publishing\tools\Metatool.Tools.Software"

Copy-Item "$metaSoftware\software" -Destination "$metaSoftwarePublishing\software" -Recurse
Copy-Item "$metaSoftware\softwareConfig" -Destination "$metaSoftwarePublishing\softwareConfig" -Recurse

sl $PSScriptRoot    