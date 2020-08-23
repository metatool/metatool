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
$metatool = Split-Path $script:MyInvocation.MyCommand.Path 
$publish = "$metatool\exe\publishing-cs"

. $PSScriptRoot\publishing-CSharpScript.ps1 -r: $rebuild
. $PSScriptRoot\push.ps1 -t:$test -p:"$publish\Metaseed.CSharpScript.exe"