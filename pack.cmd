@echo off
set /p version="Enter Version Number to Build With: "

@echo on
dotnet pack ".\TomLonghurst.PullRequestScanner\TomLonghurst.PullRequestScanner.csproj" -c Release /p:Version=%version%