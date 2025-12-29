@echo off
echo Creando acceso directo en el Escritorio...

set "TARGET=%~dp0AppEntradaSalidaDESO.exe"
set "SHORTCUT=%UserProfile%\Desktop\Simulador Discos.lnk"
set "ICON=%~dp0Resources\disk_icon.png"

powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%SHORTCUT%'); $Shortcut.TargetPath = '%TARGET%'; $Shortcut.WindowStyle = 1; $Shortcut.IconLocation = '%ICON%'; $Shortcut.WorkingDirectory = '%~dp0'; $Shortcut.Description = 'Simulador de Planificación de Discos'; $Shortcut.Save()"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [EXITO] Acceso directo creado en el Escritorio!
    echo Puede cerrar esta ventana.
) else (
    echo.
    echo [ERROR] No se pudo crear el acceso directo.
)
pause
