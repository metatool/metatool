param ([string]$target)
Compress-Archive -Path Scripts\* -CompressionLevel Optimal -DestinationPath $target -Force