function Build-Tool {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string]
        $tool,
        [switch]
        [Alias("r")]
        $release,
        [switch]
        [Alias("re")]
        $rebuild
    )
    $target = $rebuild ? "rebuild": "build"
    $config = $release ? "Release" : "Debug"
    $metatool = "M:\Workspace\metatool" 
    $tools = "$metatool\exe\release\tools"
    $publish = "$metatool\exe\publishing"

# . $PSScriptRoot\msbuild.ps1
    # Set-Location $metatool

    msbuild "$metatool\src\tool\$tool\$tool.csproj" -t:$target /p:SolutionDir=$metatool\src\ /p:Configuration=$config
    if ( $LASTEXITCODE -ne 0 ) {
        throw "build fail!"
    }
    Copy-Item "$tools\$tool" -Destination "$publish\tools\$tool" -Recurse -Exclude *.nupkg, *.pdb, Metatool.Service*.dll -Force
}