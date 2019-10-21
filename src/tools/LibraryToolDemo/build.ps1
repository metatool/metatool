
$dir = "."
$target = "..\..\app\"
$entries = New-Object System.Collections.Generic.List[System.Object] 
$sub = Get-ChildItem $dir -Exclude "bin", "obj" | ForEach-Object { $_.FullName }

foreach ($itm in $sub)
{
    $entries.Add($itm)
}
Compress-Archive -LiteralPath $entries -CompressionLevel Optimal -DestinationPath "C:\Filename01212019.zip"
Compress-Archive -Path Scripts\* -DestinationPath $target