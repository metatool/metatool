[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    [Alias("r")]
    $release,
    [switch]
    [Alias("re")]
    $rebuild
)
. $PSScriptRoot\..\..\..\script\Build-Tool.ps1

Build-Tool 'Metatool.Tools.Software' -release: $release -rebuild: $rebuild
