[CmdletBinding()]
param (
	[Parameter()]
	# Set to True when testing the script to prevent actual publishing to PowerShell Gallery, and to create a Draft of the GitHub Release instead of a published Release.
	[Alias("t")]
	[switch]
	$test,
	# exe path
	[Alias("p")]
	$path
)
## zip
$metatool = Resolve-Path $PSScriptRoot\..

if (!$path) {
	$path = "$metatool\exe\publishing\metatool.exe"
}
$exeName = [System.IO.Path]::GetFileNameWithoutExtension($path)

$publishing = "$metatool\exe\publishing"
$published = "$metatool\exe\published"

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion

if (!(Test-Path $published)) {
	New-Item -ItemType Directory -Force -Path $published
}

$target = "$published\${exeName}_v$version.zip"
$entries = New-Object System.Collections.Generic.List[System.Object]
Get-ChildItem $publishing | ForEach-Object { $entries.Add($_.FullName) }

Compress-Archive -LiteralPath $entries -CompressionLevel Optimal -DestinationPath $target -Force

## release
$isTestingThisScript = $test
$gitHubUsername = 'metatool'
$gitHubRepositoryName = 'metatool'

. "$PSScriptRoot\lib\CommonFunctions.ps1"
. "$PSScriptRoot\lib\Publish-NewReleaseToGitHub.ps1"

$newReleaseNotes = Read-MultiLineInputBoxDialog -WindowTitle 'Release Notes' -Message 'What release notes should be included with this version?' -DefaultText $currentManifestReleaseNotes
if ($null -eq $newReleaseNotes) { throw 'You cancelled out of the release notes prompt.' }
if ($newReleaseNotes.Contains("'")) {
	$errorMessage = 'Single quotes are not allowed in the Release Notes, as they break our ability to parse them with PowerShell. Exiting script.'
	Read-MessageBoxDialog -Message $errorMessage -WindowTitle 'Single Quotes Not Allowed In Release Notes'
	throw $errorMessage
}
$newReleaseNotes = $newReleaseNotes.Trim()

$versionNumberIsAPreReleaseVersion = $version -match '-+|[a-zA-Z]+' # (e.g. 1.2.3-alpha). i.e. contains a dash or letters.
$gitHubReleaseParameters =
@{
	GitHubUsername       = $gitHubUsername
	GitHubRepositoryName = $gitHubRepositoryName
	GitHubAccessToken    = $GitHubAccessToken
	ReleaseName          = "$exeName v" + $version
	TagName              = "$exeName" + "_v" + $version
	ReleaseNotes         = $newReleaseNotes
	AssetFilePaths       = @($target)
	IsPreRelease         = $versionNumberIsAPreReleaseVersion
	IsDraft              = $isTestingThisScript
}
Publish-NewReleaseToGitHub -gitHubReleaseParameters $gitHubReleaseParameters