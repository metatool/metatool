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

$published = "$PSScriptRoot\exe\published"
$exeName = [System.IO.Path]::GetFileNameWithoutExtension($path)

## zip
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion

if (!(Test-Path $published)) {
	New-Item -ItemType Directory -Force -Path $published
}

$target = "$published\${exeName}_v$version.zip"
$entries = New-Object System.Collections.Generic.List[System.Object]
Get-ChildItem $publish | ForEach-Object { $entries.Add($_.FullName) }

Compress-Archive -LiteralPath $entries -CompressionLevel Optimal -DestinationPath $target -Force

## release
$isTestingThisScript = $test
$gitHubUsername = 'metatool'
$gitHubRepositoryName = 'metatool'

$script = Join-Path -Path $PSScriptRoot -ChildPath 'lib'
$commonFunctionsScriptFilePath = Join-Path -Path $script -ChildPath 'CommonFunctions.ps1'
$publishToGitHubScriptFilePath = Join-Path -Path $script -ChildPath 'Publish-NewReleaseToGitHub.ps1'

. $commonFunctionsScriptFilePath
. $publishToGitHubScriptFilePath

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
	TagName              = "$exeName"+"_v" + $version
	ReleaseNotes         = $newReleaseNotes
	AssetFilePaths       = @($target)
	IsPreRelease         = $versionNumberIsAPreReleaseVersion
	IsDraft              = $isTestingThisScript
}
Publish-NewReleaseToGitHub -gitHubReleaseParameters $gitHubReleaseParameters