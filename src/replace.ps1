$match = "Metatool.Plugin" 
$replacement = Read-Host "Please enter a Module name"

$files = Get-ChildItem $(get-location) -filter *Metatool.Plugin* -Recurse

$files |
    Sort-Object -Descending -Property { $_.FullName } |
    Rename-Item -newname { $_.name -replace $match, $replacement } -force



$files = Get-ChildItem $(get-location) -include *.cs, *.csproj, *.sln, *.resx,*.targets,*.xml,*.config,*.json, *.xaml -Recurse 

foreach($file in $files) 
{ 
    ((Get-Content $file.fullname -Encoding UTF8) -creplace $match, $replacement) | set-content $file.fullname -Encoding UTF8
}

read-host -prompt "Done! Press any key to close."