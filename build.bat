@echo off

echo Removing old packages...
del Package\*.nupkg >nul 2>&1

echo Building...
dotnet build -c Release

echo.
echo ------------------------------------------------
echo ---- BUILD DONE: check the 'package' folder ----
echo ------------------------------------------------
echo.

pause