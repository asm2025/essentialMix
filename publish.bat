@echo off
if "%1"=="" (
	echo Key is missing.
	goto :EOF
)

SET root=%~dp0nuget\

if not exist %root% (
	echo nuget directory not found
	goto :EOF
)

for /f "usebackq delims=|" %%f in (`dir /b "%root%*.nupkg"`) do (
	echo publishing %%f
	dotnet nuget push "%root%%%f" -s https://api.nuget.org/v3/index.json --skip-duplicate -k %1
)