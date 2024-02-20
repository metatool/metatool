<#
if $(ConfigurationName) == Release (
mklink $(SolutionDir)..\exe\publish\tools $(SolutionDir)..\exe\release\tools /J
exit 0
)
#>
param($ConfigurationName, $SolutionDir)
write-host "run post build event!"
if($ConfigurationName -eq 'Release') {
	$target = "${SolutionDir}..\exe\publish\tools"
	if(!(Test-Path $target)) {
		ni $target -type Junction  -Value "${SolutionDir}..\exe\release\tools"
		Write-Host "$target link created"
	} else {
		Write-Host "$target already exists"
	}
}
