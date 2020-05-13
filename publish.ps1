[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
	$rebuild,
	[Alias("t")]
	[switch]
	$test
)
$target = $rebuild ? "rebuild": "build"
$metatool = "M:\Workspace\metatool" 
$tools = "$metatool\exe\release\tools"
$publish = "$metatool\exe\publishing"
$published = "$metatool\exe\published"

Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell a4cdb433
Set-Location $metatool

if (Test-Path $publish) {
    Remove-Item $publish -Force -Recurse
}

msbuild /t:publish "$metatool\src\app\Metaseed.Metatool.csproj"  /p:DeployOnBuild=true  /p:Configuration=Release /p:PublishDir="$publish" /p:CopyOutputSymbolsToPublishDirectory=false /p:SolutionDir="$metatool\src\" /p:PublishProfile="$metatool\src\app\Properties\PublishProfiles\metatool.pubxml"

"Metatool.Tools.MetaKeyboard", "Metatool.Tools.Software" | ForEach-Object {
    msbuild "$metatool\src\tool\$_\$_.csproj" -t:$target /p:SolutionDir=$metatool\src\ /p:Configuration=Release
    Copy-Item "$tools\$_" -Destination "$publish\tools\$_" -Recurse -Exclude *.nupkg, *.pdb, Metatool.Service*.dll
}
$metaSoftware = "$metatool\exe\publish\tools\Metatool.Tools.Software"
$metaSoftwarePublishing = "$metatool\exe\publishing\tools\Metatool.Tools.Software"

Copy-Item "$metaSoftware\software" -Destination "$metaSoftwarePublishing\software" -Recurse
Copy-Item "$metaSoftware\softwareConfig" -Destination "$metaSoftwarePublishing\softwareConfig" -Recurse

## zip
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$publish\metatool.exe").ProductVersion

if(!(Test-Path $published)){
	New-Item -ItemType Directory -Force -Path $published
}

$target = "$published\Metatool.$version.zip"
$entries = New-Object System.Collections.Generic.List[System.Object]
Get-ChildItem $publish | ForEach-Object { $entries.Add($_.FullName) }

Compress-Archive -LiteralPath $entries -CompressionLevel Optimal -DestinationPath $target -Force


## release
# Set to True when testing the script to prevent actual publishing to PowerShell Gallery, and to create a Draft of the GitHub Release instead of a published Release.
$isTestingThisScript = $test
$gitHubUsername = 'metatool'
$gitHubRepositoryName = 'metatool'

$THIS_SCRIPTS_DIRECTORY = Split-Path $script:MyInvocation.MyCommand.Path
$scripts = Join-Path -Path $THIS_SCRIPTS_DIRECTORY -ChildPath 'scripts'
$commonFunctionsScriptFilePath = Join-Path -Path $scripts -ChildPath 'CommonFunctions.ps1'
$publishToGitHubScriptFilePath = Join-Path -Path $scripts -ChildPath 'Publish-NewReleaseToGitHub.ps1'

. $commonFunctionsScriptFilePath
. $publishToGitHubScriptFilePath

$newReleaseNotes = Read-MultiLineInputBoxDialog -WindowTitle 'Release Notes' -Message 'What release notes should be included with this version?' -DefaultText $currentManifestReleaseNotes
if ($null -eq $newReleaseNotes) { throw 'You cancelled out of the release notes prompt.' }
if ($newReleaseNotes.Contains("'"))
{
	$errorMessage = 'Single quotes are not allowed in the Release Notes, as they break our ability to parse them with PowerShell. Exiting script.'
	Read-MessageBoxDialog -Message $errorMessage -WindowTitle 'Single Quotes Not Allowed In Release Notes'
	throw $errorMessage
}
$newReleaseNotes = $newReleaseNotes.Trim()

$versionNumberIsAPreReleaseVersion = $version -match '-+|[a-zA-Z]+' # (e.g. 1.2.3-alpha). i.e. contains a dash or letters.
$gitHubReleaseParameters =
@{
	GitHubUsername = $gitHubUsername
	GitHubRepositoryName = $gitHubRepositoryName
	GitHubAccessToken = $GitHubAccessToken
	ReleaseName = "$gitHubRepositoryName v" + $version
	TagName = "v" + $version
	ReleaseNotes = $newReleaseNotes
	AssetFilePaths = @($target)
	IsPreRelease = $versionNumberIsAPreReleaseVersion
	IsDraft = $isTestingThisScript
}
Publish-NewReleaseToGitHub -gitHubReleaseParameters $gitHubReleaseParameters