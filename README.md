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
- .NET 10.0 SDK o superior
- Visual Studio 2022 (recomendado) o VS Code

## 📦 Instalación

1. Clonar el repositorio:
```bash
git clone https://github.com/Airamsveedraaa/AppEntradaSalidaDESO.git
cd AppEntradaSalidaDESO
```

2. Restaurar dependencias:
```bash
dotnet restore
```

3. Compilar el proyecto:
```bash
dotnet build
```

4. Ejecutar la aplicación:
```bash
dotnet run
```

## 🛠️ Desarrollo

### Compilar en modo Release
```bash
dotnet build --configuration Release
```

### Publicar para Windows
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 📚 Características Planeadas

- [x] Implementación de algoritmos core (FCFS, SSTF, SCAN, C-SCAN, LOOK, C-LOOK)
- [x] Modelos de datos y servicios
- [ ] Interfaz gráfica WPF
- [ ] Visualización de movimientos del cabezal
- [ ] Sistema de estadísticas y progreso
- [ ] Visor de PDFs integrado
- [ ] Generación aleatoria de ejercicios
- [ ] Exportación de resultados
- [ ] Modo oscuro/claro

## 👥 Contribuir

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 📧 Contacto

Proyecto creado para la asignatura de Diseño de Sistemas Operativos (DESO).

---

**Nota**: Este es un proyecto educativo en desarrollo activo.
