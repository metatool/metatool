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
    $localRelease =$true
)

$metatoolDir = Resolve-Path $PSScriptRoot\..
$publishingDir = "$metatoolDir\exe\publishing"
Push-Location .
try {
    . $PSScriptRoot/lib/msbuild.ps1

    . $PSScriptRoot/lib/kill-metatool.ps1

    if (Test-Path $publishingDir) {
        Remove-Item $publishingDir -Force -Recurse
    }

    #restore runtime packs for win-x64
    dotnet restore "$metatoolDir\src\app\Metaseed.Metatool\Metaseed.Metatool.csproj" -r win-x64 /p:SolutionDir="$metatoolDir\src\"
    if (!$?) {
        throw "dotnet restore failed!"
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
        ri "$metatoolDir\exe\publish\appsettings.Production.json" -ErrorAction SilentlyContinue
        ri "$metatoolDir\exe\publish\appsettings.json" -ErrorAction SilentlyContinue
        Copy-Item "$metatoolDir\exe\publishing\Metatool.exe" "$metatoolDir\exe\publish" -Force
    }

    & "$PSScriptRoot\publishing-tools.ps1" -rebuild:$rebuild -localRelease:$localRelease


    #$metaSoftwarePublish = "$metatoolDir\exe\publish\tools\Metatool.Tools.Software"

    # # Copy-Item "$metaSoftware\software" -Destination "$metaSoftwarePublishing\software" -Recurse -Force
    # # Copy-Item "$metaSoftware\softwareConfig" -Destination "$metaSoftwarePublishing\softwareConfig" -Recurse -Force
    # new-item -ItemType SymbolicLink -Path "$metaSoftwarePublish\software" -Target "M:\App\software" -Force
    # new-item -ItemType SymbolicLink -Path "$metaSoftwarePublish\softwareConfig" -Target "$metatoolDir\exe\softwareConfig" -Force
    start "$metatoolDir\exe\publish"
}
finally {
    Pop-Location
    # $Error[0]
}