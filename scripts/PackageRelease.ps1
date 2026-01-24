$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "      GENERANDO RELEASE PACKAGED         " -ForegroundColor Cyan
Write-Host "========================================="
Write-Host ""

# Configuracion
$ProjectRoot = Resolve-Path "$PSScriptRoot\.."
$OutputDir = "$ProjectRoot\Release_Build"
$PublishDir = "$ProjectRoot\bin\Release\net8.0-windows\win-x64\publish"

# 1. Limpiar releases anteriores
if (Test-Path $OutputDir) {
    Write-Host "[-] Limpiando directorio de salida anterior..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# 2. Publicar la App (Single File)
Write-Host "[*] Compilando y Publicando proyecto..." -ForegroundColor Green
Set-Location $ProjectRoot
dotnet publish "src\AppEntradaSalidaDESO\AppEntradaSalidaDESO.csproj" -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo en la compilacion."
    exit $LASTEXITCODE
}

# 2.1 Copiar al directorio de salida manualmente (para evitar errores de -o con WPF)
$DefaultPublishDir = "$ProjectRoot\src\AppEntradaSalidaDESO\bin\Release\net8.0-windows\win-x64\publish"
Write-Host "[*] Copiando archivos desde $DefaultPublishDir..." -ForegroundColor Green
New-Item -ItemType Directory -Force -Path "$OutputDir\App" | Out-Null
Copy-Item -Path "$DefaultPublishDir\*" -Destination "$OutputDir\App" -Recurse -Force

# 3. Copiar script de instalacion
Write-Host "[*] Agregando script de instalacion..." -ForegroundColor Green
Copy-Item "$ProjectRoot\scripts\Setup.bat" "$OutputDir\App\INSTALAR (Crear Acceso Directo).bat"

# 3.1 Validar
$ExePath = "$OutputDir\App\AppEntradaSalidaDESO.exe"
if (-not (Test-Path $ExePath)) {
    Write-Error "ERROR CRITICO: No se encuentra el .exe en $ExePath"
    Write-Error "La publicacion fallo o no genero el archivo esperado."
    exit 1
}

# 4. Crear ZIP
Write-Host "[*] Esperando desbloqueo de archivos (5s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

$ZipPath = "$OutputDir\SimuladorDiscos_v1.0.0.zip"
Write-Host "[*] Comprimiendo en ZIP: $ZipPath" -ForegroundColor Green
if (Test-Path $ZipPath) { Remove-Item $ZipPath }
Compress-Archive -Path "$OutputDir\App\*" -DestinationPath $ZipPath -Force

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " [EXITO] PAQUETE LISTO: $ZipPath" -ForegroundColor Cyan
Write-Host "========================================="
Write-Host "Enviale este archivo ZIP a la gente. Ellos solo tienen que:"
Write-Host "1. Descomprimirlo."
Write-Host "2. Ejecutar 'INSTALAR.bat' para tener el icono en el escritorio."
explorer $OutputDir
