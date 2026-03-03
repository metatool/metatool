[CmdletBinding()]
param(
	[Parameter()]
	[switch]
	[Alias("r")]
	$rebuild,
	[Parameter()]
	[switch]
	[Alias("l")]
	$localRelease = $true
)

$metatoolDir = Resolve-Path $PSScriptRoot\..

. $PSScriptRoot/lib/Build-Tool.ps1

"Metatool.Tools.MetaKeyboard",
"Metatool.Tools.WinShell",
"Metatool.Tools.Software",
"Metatool.Tools.KeyMouse"
| ForEach-Object {
	ri "$metatoolDir\exe\publishing\tools\$_" -Force -Recurse -ErrorAction Ignore

	Build-Tool $_ -release:$true -rebuild:$rebuild
	if ($localRelease) {
		ri "$metatoolDir\exe\publish\tools\$_" -Force -Recurse -ErrorAction Ignore
		# /E: copy subdirectories, including empty ones.
		# /SL: copy symbolic links as symbolic links.
		# /S: copy subdirectories, but not empty ones.
		robocopy "$metatoolDir\exe\publishing\tools\$_" "$metatoolDir\exe\publish\tools\$_" /SL /S
	}
}
$error