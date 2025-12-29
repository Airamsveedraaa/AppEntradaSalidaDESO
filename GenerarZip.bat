@echo off
echo Iniciando generacion del ZIP de Release...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\PackageRelease.ps1"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Hubo un error.
    pause
)
