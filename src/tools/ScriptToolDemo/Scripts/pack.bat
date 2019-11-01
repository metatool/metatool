@echo off
start nuget.exe pack .\pack.nuspec -exclude "bin\**" -exclude "*.bat" -exclude ".vscode\**"
exit
