param(
    [Parameter(Mandatory)][string]$From,
    [Parameter(Mandatory)][string]$To,
    [string]$Path = $PSScriptRoot
)

# Usage:
#   .\upgrade-tfm.ps1 -From 10 -To 11
#   .\upgrade-tfm.ps1 -From 10 -To 11 -Path "C:\my\project"

$allFiles = Get-ChildItem -Path $Path -Recurse -Include *.csproj, *.props, *.pubxml
$updatedCount = 0

$replacements = @(
    @{ Pattern = "net${From}\.0-windows"; Replace = "net${To}.0-windows" },
    @{ Pattern = "(?<=<TargetFrameworks?>)net${From}\.0(?![-\w])"; Replace = "net${To}.0" }
)

foreach ($file in $allFiles) {
    $content = Get-Content $file.FullName -Raw
    $original = $content

    foreach ($r in $replacements) {
        $content = $content -replace $r.Pattern, $r.Replace
    }

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $updatedCount++
        $rel = [System.IO.Path]::GetRelativePath($Path, $file.FullName)
        Write-Host "  Updated: $rel"
    }
}

Write-Host "`nDone. Updated $updatedCount file(s)."
