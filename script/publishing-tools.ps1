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
| ForEach-Object -parellel {
	ri "$metatoolDir\exe\publishing\tools\$_" -Force -Recurse -ErrorAction SilentlyContinue

	Build-Tool $_ -release: $true -rebuild: $rebuild
	if ($localRelease) {
		ri "$metatoolDir\exe\publish\tools\$_" -Force -Recurse -ErrorAction SilentlyContinue
		Copy-Item "$metatoolDir\exe\publishing\tools\$_" "$metatoolDir\exe\publish\tools" -Force -Recurse -Verbose
	}
}
$error