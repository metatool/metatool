[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
    $rebuild
)
$target = $rebuild ? "rebuild": "build"
$metatool = "M:\Workspace\metatool" 
$tools = "$metatool\exe\release\tools"
$publish = "$metatool\exe\publishing"

Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell a4cdb433
Set-Location $metatool

if (Test-Path $publish) {
    Remove-Item $publish -Force -Recurse
}

msbuild /t:publish "$metatool\src\app\Metaseed.Metatool.csproj"  /p:DeployOnBuild=true  /p:Configuration=Release /p:PublishDir="$publish" /p:CopyOutputSymbolsToPublishDirectory=false /p:SolutionDir="$metatool\src\" /p:PublishProfile="$metatool\src\app\Properties\PublishProfiles\metatool.pubxml"
if ( $LASTEXITCODE -ne 0 ) {
    throw "publish fail!"
}
. ./script/Build-Tool.ps1

"Metatool.Tools.MetaKeyboard", "Metatool.Tools.Software" | ForEach-Object {
    Build-Tool $_ $rebuild
}

$metaSoftware = "$metatool\exe\publish\tools\Metatool.Tools.Software"
$metaSoftwarePublishing = "$metatool\exe\publishing\tools\Metatool.Tools.Software"

Copy-Item "$metaSoftware\software" -Destination "$metaSoftwarePublishing\software" -Recurse
Copy-Item "$metaSoftware\softwareConfig" -Destination "$metaSoftwarePublishing\softwareConfig" -Recurse