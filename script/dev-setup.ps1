# create symbolic link in debug folder for the \exe\publish\tools\Metatool.Tools.Software\softwareConfig and exe\publish\tools\Metatool.Tools.Software\software
$isDebug = $FALSE
$folder = $isDebug ? 'debug': 'release'
$SolutionDir = $PSScriptRoot.Replace("tool\Metatool.Tools.Software","")

## WebUI folder
$UiPathReal = "M:\Workspace\metatool\src\lib\Metaseed.WebUI\frontend\dist"
$UiPath = "M:\Workspace\metatool\exe\$folder\_ui"
if(Test-Path $UiPath) {
	New-Item -ItemType SymbolicLink -Path $UiPath -Target $UiPathReal
}

## Metatool.Tools.Software
$TargetDir = "$SolutionDir..\exe\$folder\tools\Metatool.Tools.Software"
$sourceSoftwarePath = "M:\App\software"#"$SolutionDir..\exe\publish\tools\Metatool.Tools.Software\software"
$targetSoftwarePath = "$TargetDir\software"
$sourceSoftwareConfigPath = "$SolutionDir..\exe\softwareConfig" #"$SolutionDir..\exe\publish\tools\Metatool.Tools.Software\softwareConfig"
$targetSoftwareConfigPath = "$TargetDir\softwareConfig"

if (!(Test-Path -Path $targetSoftwarePath)) {
	#rm -Force $targetSoftwarePath -ErrorAction SilentlyContinue
	New-Item -ItemType SymbolicLink -Path $targetSoftwarePath -Target $sourceSoftwarePath
}
if (!(Test-Path -Path $targetSoftwareConfigPath)) {
	#rm -Force $targetSoftwareConfigPath -ErrorAction SilentlyContinue
	New-Item -ItemType SymbolicLink -Path $targetSoftwareConfigPath -Target $sourceSoftwareConfigPath
}
