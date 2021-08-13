$metatool = Resolve-Path $PSScriptRoot\..

Copy-Item "$metatool\exe\publishing\Metatool.exe" "$metatool\exe\publish" -Force
'> metatool.exe deployed to local publish folder!'