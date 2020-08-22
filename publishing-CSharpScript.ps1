[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
    $rebuild
)
$metatool = "M:\Workspace\metatool" 
$publishCSharpScript = "$metatool\exe\publishing-cs"

. ./script/msbuild.ps1
Set-Location $metatool

if (Test-Path $publish) {
    Remove-Item $publish -Force -Recurse
}

msbuild /t:publish  "$metatool\src\app\Metaseed.CSharpScript\Metaseed.CSharpScript.csproj"  /p:DeployOnBuild=true  /p:Configuration=Release /p:PublishDir="$publishCSharpScript" /p:CopyOutputSymbolsToPublishDirectory=false /p:SolutionDir="$metatool\src\" /p:PublishProfile="$metatool\src\app\Metaseed.CSharpScript\Properties\PublishProfiles\FolderProfile.pubxml"
if ( $LASTEXITCODE -ne 0 ) {
    throw "publish fail!"
}
