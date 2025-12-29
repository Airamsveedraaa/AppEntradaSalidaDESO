# Simulador de Ejercicios - Entrada/Salida DESO

Aplicación de escritorio en C# WPF para practicar algoritmos de planificación de E/S de disco.

## 📋 Descripción

Esta aplicación permite a los estudiantes practicar y comprender los diferentes algoritmos de planificación de entrada/salida de disco utilizados en sistemas operativos. Incluye visualizaciones interactivas, soluciones paso a paso y seguimiento de estadísticas.

## 🎯 Algoritmos Implementados

- **FCFS** (First Come First Served) - Atiende las peticiones en orden de llegada
- **SSTF** (Shortest Seek Time First) - Atiende primero la petición más cercana
- **SCAN** (Elevador) - Recorre en una dirección hasta el final, luego invierte
- **C-SCAN** (Circular SCAN) - Recorre en una dirección y vuelve al inicio circularmente
- **LOOK** - Como SCAN pero solo va hasta la última petición
- **C-LOOK** - Como C-SCAN pero solo va hasta la última petición
- **F-SCAN** - Congela peticiones entrantes, procesa por lotes (equivale a SCAN estático)
- **F-LOOK** - Variante congelada de LOOK (equivale a LOOK estático)
- **Mejora**: Soporte para límites de disco dinámicos (min/max cylinder configurable)

## 🏗️ Estructura del Proyecto

```
AppEntradaSalidaDESO/
├── Models/              # Modelos de datos
│   ├── DiskRequest.cs
│   ├── ExerciseResult.cs
│   └── Statistics.cs
├── Algorithms/          # Implementación de algoritmos
│   ├── IDiskSchedulingAlgorithm.cs
│   ├── FCFSAlgorithm.cs
│   ├── SSTFAlgorithm.cs
│   ├── SCANAlgorithm.cs
│   ├── CSCANAlgorithm.cs
│   ├── LOOKAlgorithm.cs
│   └── CLOOKAlgorithm.cs
├── Services/            # Servicios de la aplicación
│   └── AlgorithmService.cs
├── ViewModels/          # ViewModels (MVVM)
├── Views/               # Vistas XAML
└── Resources/           # Recursos (imágenes, estilos)
```

## 🚀 Requisitos

- Windows 10/11
- .NET 10.0 SDK (Solo para compilar)
- **Para usuarios finales**: Solo necesitan el archivo `.exe` generado.

## 📦 Instalación y Ejecución Rápida

### Opción A (Desarrolladores)
1. Clonar y ejecutar:
```bash
git clone https://github.com/Airamsveedraaa/AppEntradaSalidaDESO.git
cd AppEntradaSalidaDESO
dotnet run
```

### Opción B (Generar Ejecutable para "Producción")
Para crear una aplicación portátil (sin necesidad de instalar .NET en la máquina destino) o un ejecutable simple:

1. Ejecuta el comando de publicación:
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```
2. El archivo `AppEntradaSalidaDESO.exe` estará en:
   `bin\Release\net10.0-windows\win-x64\publish\`

Este archivo `.exe` es todo lo que necesitas compartir.

## 🖥️ Interfaz de Usuario

La aplicación cuenta con una interfaz gráfica moderna (WPF):

1. **Configuración**:
   - Selecciona el algoritmo (FCFS, SSTF, SCAN, etc.).
   - Introduce la cola de peticiones (ej: `98, 183, 37`).
   - Define los límites del disco (`min` y `max`).
   - Elige la posición inicial del cabezal.

2. **Resultados**:
   - Visualiza métricas clave (Movimiento Total, Tiempo Promedio).
   - Tabla detallada paso a paso con distancias y direcciones.

## 📚 Características Completadas

- [x] **8 Algoritmos**: FCFS, SSTF, SCAN, C-SCAN, LOOK, C-LOOK, F-SCAN, F-LOOK.
- [x] **Configuración Dinámica**: Soporte para discos de cualquier tamaño.
- [x] **Interfaz Gráfica**: Panel de control intuitivo y tabla de resultados.
- [x] **Visualización**: Detalle paso a paso de cada movimiento.
- [x] **Icono Personalizado**: Identidad visual básica.

## 🛠️ Desarrollo

Si quieres contribuir:
1. Abre el proyecto en Visual Studio 2022 o VS Code.
2. La arquitectura sigue el patrón **MVVM**:
   - `ViewModels/MainViewModel.cs`: Lógica de presentación.
   - `Views/MainWindow.xaml`: Interfaz de usuario.
   - `Algorithms/`: Lógica del núcleo.

## 📄 Licencia

Este proyecto está distribuido bajo la licencia **GNU General Public License v3 (GPLv3)**. Consulta `LICENSE.md`.

## 📧 Contacto

Proyecto creado para la asignatura de Diseño de Sistemas Operativos (DESO).

---

**Nota**: Este es un proyecto educativo en desarrollo activo.
