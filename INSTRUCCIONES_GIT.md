# Instrucciones para Subir el Core al Repositorio

Como Git no está instalado en tu sistema, aquí están las instrucciones para subir los archivos manualmente a GitHub:

## Opción 1: Usar GitHub Desktop (Recomendado)

1. Descarga e instala [GitHub Desktop](https://desktop.github.com/)
2. Abre GitHub Desktop y clona tu repositorio
3. Copia todos los archivos del proyecto a la carpeta clonada
4. GitHub Desktop detectará los cambios automáticamente
5. Escribe un mensaje de commit: "feat: Add core C# WPF application with all disk scheduling algorithms"
6. Haz clic en "Commit to main"
7. Haz clic en "Push origin"

## Opción 2: Usar la Interfaz Web de GitHub

1. Ve a https://github.com/Airamsveedraaa/AppEntradaSalidaDESO
2. Haz clic en "Add file" → "Upload files"
3. Arrastra y suelta estos archivos/carpetas:
   - `Models/` (carpeta completa)
   - `Algorithms/` (carpeta completa)
   - `Services/` (carpeta completa)
   - `ViewModels/` (carpeta vacía por ahora)
   - `Views/` (carpeta vacía por ahora)
   - `Resources/` (carpeta vacía por ahora)
   - `App.xaml`
   - `App.xaml.cs`
   - `MainWindow.xaml`
   - `MainWindow.xaml.cs`
   - `AssemblyInfo.cs`
   - `AppEntradaSalidaDESO.csproj`
   - `README.md`
   - `.gitignore`
4. Escribe el mensaje de commit: "feat: Add core C# WPF application with all disk scheduling algorithms"
5. Haz clic en "Commit changes"

## Opción 3: Instalar Git y Usar Línea de Comandos

1. Descarga e instala Git desde https://git-scm.com/download/win
2. Reinicia PowerShell
3. Ejecuta estos comandos:

```bash
cd c:\Users\lsaav\Desktop\appDESO
git init
git add .
git commit -m "feat: Add core C# WPF application with all disk scheduling algorithms"
git branch -M main
git remote add origin https://github.com/Airamsveedraaa/AppEntradaSalidaDESO.git
git push -u origin main --force
```

## Archivos Creados en Este Commit

### Modelos (Models/)
- `DiskRequest.cs` - Representa una petición de E/S
- `ExerciseResult.cs` - Almacena resultados de ejecución
- `Statistics.cs` - Seguimiento de estadísticas del usuario

### Algoritmos (Algorithms/)
- `IDiskSchedulingAlgorithm.cs` - Interfaz base
- `FCFSAlgorithm.cs` - First Come First Served
- `SSTFAlgorithm.cs` - Shortest Seek Time First
- `SCANAlgorithm.cs` - SCAN (Elevador)
- `CSCANAlgorithm.cs` - C-SCAN (Circular SCAN)
- `LOOKAlgorithm.cs` - LOOK
- `CLOOKAlgorithm.cs` - C-LOOK

### Servicios (Services/)
- `AlgorithmService.cs` - Gestión de algoritmos

### Archivos del Proyecto
- `AppEntradaSalidaDESO.csproj` - Configuración del proyecto
- `App.xaml` / `App.xaml.cs` - Aplicación WPF
- `MainWindow.xaml` / `MainWindow.xaml.cs` - Ventana principal
- `README.md` - Documentación
- `.gitignore` - Archivos a ignorar

## Verificación

El proyecto compila correctamente:
```
✓ Compilación correcta
✓ 0 Advertencias
✓ 0 Errores
```

## Próximos Pasos

Una vez subido el core, los siguientes pasos serán:
1. Crear la interfaz gráfica XAML
2. Implementar ViewModels (patrón MVVM)
3. Añadir visualización de resultados
4. Integrar visor de PDFs
5. Implementar sistema de estadísticas
