# 💿 Simulador de Planificación de Discos | Disk Scheduling Simulator

[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/Airamsveedraaa/AppEntradaSalidaDESO/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

> **Aplicación de escritorio completa para simular y analizar algoritmos de planificación de discos con soporte para llegadas dinámicas y conversión de geometría de disco.**
>
> **Complete desktop application for simulating and analyzing disk scheduling algorithms with support for dynamic arrivals and disk geometry conversion.**

---

## 📋 Tabla de Contenidos | Table of Contents

- [🌟 Características](#-características--features)
- [🎯 Algoritmos Implementados](#-algoritmos-implementados--implemented-algorithms)
- [🛠️ Conversor de Geometría](#️-conversor-de-geometría--geometry-converter)
- [📥 Instalación](#-instalación--installation)
- [🚀 Uso](#-uso--usage)
- [📊 Ejemplos](#-ejemplos--examples)
- [🏗️ Arquitectura](#️-arquitectura--architecture)
- [🤝 Contribuir](#-contribuir--contributing)

---

## 🌟 Características | Features

### Español
- ✅ **11 Algoritmos de Planificación** implementados y verificados
- ✅ **Llegadas Dinámicas** con soporte para múltiples instantes de tiempo
- ✅ **Visualización Gráfica** del movimiento del cabezal
- ✅ **Métricas Detalladas** de rendimiento (movimiento total, tiempo de búsqueda, etc.)
- ✅ **Conversor de Geometría de Disco** bidireccional (Capacidad ↔ Cilindros)
- ✅ **Conversión Bloque → Pista** con soporte para indexación 0/1
- ✅ **Modo Académico/Técnico** para visualización de pistas
- ✅ **Interfaz Intuitiva** con validación en tiempo real
- ✅ **Exportación de Resultados** en formato tabular

### English
- ✅ **11 Scheduling Algorithms** implemented and verified
- ✅ **Dynamic Arrivals** with support for multiple time instants
- ✅ **Graphical Visualization** of head movement
- ✅ **Detailed Performance Metrics** (total movement, seek time, etc.)
- ✅ **Bidirectional Disk Geometry Converter** (Capacity ↔ Cylinders)
- ✅ **Block → Track Conversion** with 0/1 indexing support
- ✅ **Academic/Technical Mode** for track visualization
- ✅ **Intuitive Interface** with real-time validation
- ✅ **Results Export** in tabular format

---

## 🎯 Algoritmos Implementados | Implemented Algorithms

### Algoritmos Básicos | Basic Algorithms
| Algoritmo | Descripción ES | Description EN |
|-----------|---------------|----------------|
| **FCFS** | Primero en Llegar, Primero en Ser Atendido | First Come, First Served |
| **SSTF** | Buscar el Más Cercano Primero | Shortest Seek Time First |

### Algoritmos de Barrido | Sweep Algorithms
| Algoritmo | Descripción ES | Description EN |
|-----------|---------------|----------------|
| **SCAN** | Barrido en una dirección hasta el extremo | Sweep in one direction to the end |
| **C-SCAN** | Barrido circular (salta al inicio) | Circular sweep (jumps to start) |
| **LOOK** | Como SCAN, pero solo hasta la última petición | Like SCAN, but only to last request |
| **C-LOOK** | Como C-SCAN, pero solo hasta la última petición | Like C-SCAN, but only to last request |

### Algoritmos con Pasos Dinámicos | Dynamic Step Algorithms
| Algoritmo | Descripción ES | Description EN |
|-----------|---------------|----------------|
| **SCAN-N** | SCAN con procesamiento en lotes de N peticiones | SCAN with N-request batch processing |
| **LOOK-N** | LOOK con procesamiento en lotes de N peticiones | LOOK with N-request batch processing |
| **F-SCAN** | SCAN con cola congelada (freeze) | SCAN with frozen queue |
| **F-LOOK** | LOOK con cola congelada (freeze) | LOOK with frozen queue |
| **N-Step-SCAN** | SCAN con paso dinámico de N peticiones | SCAN with dynamic N-step |

---

## 🛠️ Conversor de Geometría | Geometry Converter

### Funcionalidades | Features

#### Modo Bidireccional | Bidirectional Mode
- **Capacidad → Cilindros**: Calcula cilindros necesarios para una capacidad dada
- **Cilindros → Capacidad**: Calcula capacidad total del disco

#### Conversión de Bloques | Block Conversion
- Convierte números de bloque a coordenadas CHS (Cilindro, Cabeza, Sector)
- Soporte para indexación basada en 0 o 1
- Visualización de rango de pistas (modo académico/técnico)

#### Parámetros Configurables | Configurable Parameters
- Sectores por pista
- Número de caras/cabezales
- Tamaño de sector (bytes)
- Tamaño de bloque (bytes)
- Unidades de capacidad (KB, MB, GB)

---

## 📥 Instalación | Installation

### Requisitos | Requirements
- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** o superior | or higher

### Descarga | Download

#### Opción 1: Release Binario | Binary Release
1. Descarga la última versión desde [Releases](https://github.com/Airamsveedraaa/AppEntradaSalidaDESO/releases)
2. Extrae el archivo ZIP
3. Ejecuta `AppEntradaSalidaDESO.exe`

#### Opción 2: Compilar desde Código | Build from Source
```bash
# Clonar repositorio | Clone repository
git clone https://github.com/Airamsveedraaa/AppEntradaSalidaDESO.git
cd AppEntradaSalidaDESO

# Compilar | Build
dotnet build src/AppEntradaSalidaDESO/AppEntradaSalidaDESO.csproj -c Release

# Ejecutar | Run
dotnet run --project src/AppEntradaSalidaDESO/AppEntradaSalidaDESO.csproj
```

---

## 🚀 Uso | Usage

### Modo Simple | Simple Mode
1. Selecciona un algoritmo
2. Configura los parámetros del disco (límites, posición inicial)
3. Ingresa las peticiones separadas por comas: `14, 37, 98, 122`
4. Haz clic en **Simular**

### Modo Dinámico | Dynamic Mode
1. Activa "Peticiones llegan en distintos instantes"
2. Añade grupos de peticiones con sus instantes de llegada
3. Configura tiempos de búsqueda y latencia
4. Haz clic en **Simular**

### Conversor de Geometría | Geometry Converter
1. Abre el conversor desde el menú "Herramientas"
2. Selecciona el modo de cálculo (Capacidad o Cilindros)
3. Ingresa los parámetros del disco
4. Opcionalmente, convierte bloques a pistas
5. Copia los resultados con los botones de copiado

---

## 📊 Ejemplos | Examples

### Ejemplo 1: SCAN con Llegadas Dinámicas
```
Algoritmo: SCAN
Posición Inicial: 50
Dirección: up
Límites: 0 - 199

Grupo T=0:   98, 183, 37, 122, 14
Grupo T=50:  124, 65, 67
```

**Resultado**: El algoritmo procesa primero las peticiones >= 50 en orden ascendente (98, 122, 124, 183), luego invierte dirección para procesar las restantes.

### Ejemplo 2: Conversor de Geometría
```
Modo: Tengo Capacidad
Capacidad: 4 MB
Sectores/Pista: 8
Caras: 4
Tamaño Sector: 512 bytes
Tamaño Bloque: 2048 bytes

Resultado:
- Cilindros: 1024
- Bloques/Pista: 2.00
- Pistas Totales: 4096
- Rango (modo académico): 1024 (1-1024)
```

---

## 🏗️ Arquitectura | Architecture

### Tecnologías | Technologies
- **Framework**: .NET 8.0 (WPF)
- **Patrón**: MVVM (Model-View-ViewModel)
- **UI**: XAML con Material Design
- **Gráficos**: LiveCharts2

### Estructura del Proyecto | Project Structure
```
AppEntradaSalidaDESO/
├── src/
│   └── AppEntradaSalidaDESO/
│       ├── Algorithms/          # Implementaciones de algoritmos
│       ├── Models/              # Modelos de datos
│       ├── ViewModels/          # Lógica de presentación
│       ├── Views/               # Interfaces de usuario
│       └── Services/            # Servicios compartidos
├── docs/                        # Documentación académica
└── scripts/                     # Scripts de compilación
```

---

## 🐛 Problemas Conocidos | Known Issues

Ninguno en la versión 1.0. Si encuentras algún problema, por favor [abre un issue](https://github.com/Airamsveedraaa/AppEntradaSalidaDESO/issues).

---

## 📝 Changelog

### v1.0.0 (2026-01-24) - Versión Oficial
- ✅ Corrección crítica: Eliminada lógica de intercepción incorrecta
- ✅ Corrección: Selección de dirección en algoritmos SCAN
- ✅ Corrección: Ordenamiento por posición en lugar de distancia
- ✅ Nueva funcionalidad: Modo de visualización de pistas (Académico/Técnico)
- ✅ Mejora: Conversor bidireccional de geometría
- ✅ Mejora: Conversión de bloques a pistas con indexación configurable
- ✅ Todos los algoritmos verificados y funcionando correctamente

### v0.2.0-beta (2026-01-23)
- Añadidos algoritmos SCAN-N y LOOK-N
- Mejoras en el conversor de geometría
- Actualización de encabezados de tabla

### v0.1.0-alpha (2026-01-20)
- Versión inicial con algoritmos básicos

---

## 🤝 Contribuir | Contributing

Las contribuciones son bienvenidas. Por favor:
1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📄 Licencia | License

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

---

## 👨‍💻 Autor | Author

**Airam** - [@Airamsveedraaa](https://github.com/Airamsveedraaa)

### Colaboradores | Contributors

- **Sebastián** - [@sebascm-dev](https://github.com/sebascm-dev)

---

## 🙏 Agradecimientos | Acknowledgments

- Desarrollado como herramienta de estudio para la asignatura de Diseño y Estructura de Sistemas Operativos
- Basado en material académico de gestión de E/S
- Gracias a todos los que han probado y reportado issues

---

<div align="center">

**⭐ Si este proyecto te ha sido útil, considera darle una estrella en GitHub ⭐**

Made with ❤️ for students and educators

</div>
