@echo off
echo ==========================================
echo   LANZADOR DE DEBUG
echo ==========================================
echo Intentando arrancar la aplicacion...
echo.

if not exist "Release_Build\App\AppEntradaSalidaDESO.exe" (
    echo [ERROR] No encuentro el .exe. Ejecuta GenerarZip.bat primero.
    pause
    exit /b
)

"Release_Build\App\AppEntradaSalidaDESO.exe"

echo.
echo ==========================================
echo   FIN DE EJECUCION
echo ==========================================
echo Si arriba ves algun mensaje de error (Exception, DLL not found, etc),
echo por favor copiamelo.
echo.
pause
