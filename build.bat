@echo off

echo Removing old packages...
del Package\*.nupkg >nul 2>&1

echo Building...
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" "WpfScreenHelper.sln" /property:Configuration=Release

echo.
echo ------------------------------------------------
echo ---- BUILD DONE: check the 'package' folder ----
echo ------------------------------------------------
echo.

pause