[CmdletBinding()]
param (
	[Parameter()]
	[switch]
	[Alias("r")]
	$rebuild,
	# Set to True when testing the script to prevent actual publishing to PowerShell Gallery, and to create a Draft of the GitHub Release instead of a published Release.
	[Alias("t")]
	[switch]
	$test
)
$metatool = "M:\Workspace\metatool"
$publish = "$metatool\exe\publishing"

# . $PSScriptRoot\publishing.ps1 -r: $rebuild
. $PSScriptRoot\push.ps1 -t:$test -p:"$publish\metatool.exe"