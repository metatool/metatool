# create symbolic link in debug folder for the \exe\publish\tools\Metatool.Tools.Software\softwareConfig and exe\publish\tools\Metatool.Tools.Software\software
$SolutionDir = $PSScriptRoot.Replace("tool\Metatool.Tools.Software","")
$TargetDir = "$SolutionDir..\exe\debug\tools\Metatool.Tools.Software"
$sourceSoftwarePath = "$SolutionDir..\exe\publish\tools\Metatool.Tools.Software\software"
$targetSoftwarePath = "$TargetDir\software"
$sourceSoftwareConfigPath = "$SolutionDir..\exe\publish\tools\Metatool.Tools.Softcware\softwareConfig"
$targetSoftwareConfigPath = "$TargetDir\softwareConfig"
if (!(Test-Path -Path $targetSoftwarePath)) {
	New-Item -ItemType SymbolicLink -Path $targetSoftwarePath -Target $sourceSoftwarePath
}
if (!(Test-Path -Path $targetSoftwareConfigPath)) {
	New-Item -ItemType SymbolicLink -Path $targetSoftwareConfigPath -Target $sourceSoftwareConfigPath
}