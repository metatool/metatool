<#
build metatool app and all the tool plugins and put the output into the exe/publishing folder
add the '-l' parameter to also output them into local 'publish' folder
#>
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

$metatoolDir = Resolve-Path $PSScriptRoot\..
$publishingDir = "$metatoolDir\exe\publishing"
Push-Location .
try {
    . $PSScriptRoot/lib/msbuild.ps1

    if (Test-Path $publishingDir) {
        Remove-Item $publishingDir -Force -Recurse
    }

    #publish
    msbuild /t:publish  "$metatoolDir\src\app\Metaseed.Metatool\Metaseed.Metatool.csproj" /p:DeployOnBuild=true  /p:Configuration=Release /p:PublishDir="$publishingDir" /p:CopyOutputSymbolsToPublishDirectory=false /p:DebugType=None /p:DebugSymbols=false /p:SolutionDir="$metatoolDir\src\" /p:PublishProfile="$metatoolDir\src\app\Metaseed.Metatool\Properties\PublishProfiles\metatool.pubxml"
    # dotnet publish "$metatoolDir\src\app\Metaseed.Metatool\Metaseed.Metatool.csproj" -p:DeployOnBuild=true  -p:Configuration=Release -p:PublishDir="$publishingDir" -p:CopyOutputSymbolsToPublishDirectory=false -p:DebugType=None -p:DebugSymbols=false -p:SolutionDir="$metatoolDir\src\" -p:PublishProfile="$metatoolDir\src\app\Metaseed.Metatool\Properties\PublishProfiles\metatool.pubxml"

    if (!$?) {#  $LASTEXITCODE -ne 0
        throw "publish build of metatool.exe fail!"
    }
    if ($localRelease) {
        spps -n metatool -ErrorAction Ignore
        # note: we need also close the metatool in the vm, otherwise error: The process cannot access the file 'M:\Workspace\metatool\exe\publish\Metatool.exe' because it is being used by another process.
        Copy-Item "$metatoolDir\exe\publishing\Metatool.exe" "$metatoolDir\exe\publish" -Force
    }
    . $PSScriptRoot/lib/Build-Tool.ps1

    "Metatool.Tools.MetaKeyboard", "Metatool.Tools.Software" | ForEach-Object {
        Build-Tool $_ -release: $true -rebuild: $rebuild
        if ($localRelease) {
            Copy-Item "$metatoolDir\exe\publishing\tools\$_" "$metatoolDir\exe\publish\tools" -Force -Recurse -Verbose
        }
    }


    $metaSoftware = "$metatoolDir\exe\publish\tools\Metatool.Tools.Software"
    $metaSoftwarePublishing = "$metatoolDir\exe\publishing\tools\Metatool.Tools.Software"

    Copy-Item "$metaSoftware\software" -Destination "$metaSoftwarePublishing\software" -Recurse -Force
    Copy-Item "$metaSoftware\softwareConfig" -Destination "$metaSoftwarePublishing\softwareConfig" -Recurse -Force

}
finally {
    Pop-Location
    # $Error[0]
}