# need to run on host machine
# $devType = 'debug'
$devType = 'release'
$PSScriptRoot
New-Item -ItemType Junction -Path "$PSScriptRoot\$devType\tools\Metatool.Tools.Software\software" -Value "$PSScriptRoot\publish\tools\Metatool.Tools.Software\software" -Force
New-Item -ItemType Junction -Path "$PSScriptRoot\$devType\tools\Metatool.Tools.Software\softwareConfig" -Value "$PSScriptRoot\publish\tools\Metatool.Tools.Software\softwareConfig" -Force
New-Item -ItemType Junction -Path "$PSScriptRoot\$devType\_private"  -Value "$PSScriptRoot\publish\_private" -Force
