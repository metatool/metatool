# kill Metatool.exe if its running instance is from the publish dir
$publishDir = "$metatoolDir\exe\publish"
Get-Process -Name metatool -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -and $_.Path -like "$publishDir\*"
} | Stop-Process -Force
