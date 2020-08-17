Set-Location ./src/test

dotnet restore Metatool.Tests.csproj
$ReportGeneratorVersion = "4.6.4"
$ReportDir = "./coveragereport"
if (Test-Path -path  "$ReportDir") {
    Remove-Item "$ReportDir" -Force -Recurse
}

$ResultDir = "./test/TestResults"
if (Test-Path -path "$ResultDir") {
    Remove-Item  "$ResultDir" -Force -Recurse
}

dotnet test --collect:"XPlat Code Coverage" --results-directory "$ResultDir"
$dir = Get-ChildItem "$ResultDir"
dotnet $env:USERPROFILE/.nuget/packages/reportgenerator/$ReportGeneratorVersion/tools/netcoreapp3.0/ReportGenerator.dll -reports:$dir/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
Start-Process ./coveragereport/index.html

Set-Location ../..