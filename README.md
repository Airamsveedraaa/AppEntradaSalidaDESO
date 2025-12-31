# Simulador de Ejercicios - Entrada/Salida DESO

Aplicación de escritorio en C# WPF para practicar algoritmos de planificación de E/S de disco, diseñada para resolver ejercicios académicos de la asignatura DESO.

## 📋 Descripción

Esta aplicación permite a los estudiantes practicar y comprender los diferentes algoritmos de planificación de entrada/salida de disco utilizados en sistemas operativos. Incluye visualizaciones interactivas, soporte para tiempos de llegada, conversor de bloques y soluciones paso a paso detalladas.

## 🎯 Algoritmos Implementados

- **FCFS** (First Come First Served)
- **SSTF** (Shortest Seek Time First)
- **SCAN** (Elevador)
- **C-SCAN** (Circular SCAN)
- **LOOK**
- **C-LOOK**
- **F-SCAN** (Freeze SCAN)
- **F-LOOK** (Freeze LOOK)
- **SCAN-N** (N-Step SCAN) - **¡Nuevo!** Procesa peticiones en lotes de tamaño N.

## ✨ Nuevas Características (v2.0)

### 1. Simulación Temporal Realista
- Soporte para **Tiempos de Llegada**: Formato `Pista:Tiempo` (ej: `50:1.5`).
- Configuración de tiempos detallada: **Tiempo por Pista** (Búsqueda) y **Tiempo por Petición** (Transferencia/Latencia).
- Simulación de "intercepciones" en algoritmos como SCAN o LOOK cuando llegan nuevas peticiones durante el movimiento.

### 2. Visualización Gráfica
- **Gráfico de Movimiento**: Visualización tipo "line chart" que muestra el recorrido del cabezal en el tiempo.
- Indicadores visuales para saltos circulares (líneas rojas punteadas).

### 3. Herramientas de Cálculo
- **Conversor de Geometría**: Nueva ventana (`🛠️ Conversor`) para calcular:
    - Bloques por Cilindro.
    - Conversión automática de **Número de Bloque -> Número de Pista**.
    - Configurable: Sectores, Caras, Tamaño de Sector/Bloque.

### 4. Tabla de Resultados Mejorada
- Columnas detalladas: **Cola Pendiente** y **Buffer**.
- Muestra el estado exacto de las peticiones en espera en cada paso de la simulación.

## 🚀 Requisitos

- Windows 10/11
- .NET 8.0 Desktop Runtime (Si se usa la versión dependiente del framework)
- **Para usuarios finales**: Solo necesitan el archivo `.exe` generado (versión autocontenida).

## 📦 Instalación y Ejecución

### Opción A (Usuarios - Release)
1. Descarga el archivo `.zip` de la última release.
2. Descomprímelo.
3. Ejecuta `AppEntradaSalidaDESO.exe` o usa el script `INSTALAR (Crear Acceso Directo).bat`.

### Opción B (Desarrolladores)
1. Clonar el repositorio:
```bash
git clone https://github.com/Airamsveedraaa/AppEntradaSalidaDESO.git
```
2. Abrir en Visual Studio 2022 o VS Code.
3. Ejecutar:
```bash
dotnet restore
dotnet run --project src/AppEntradaSalidaDESO
```

## 🛠️ Generación de Instalador (Script)

El proyecto incluye scripts automatizados para generar una release portátil:

1. Ejecuta el archivo `GenerarZip.bat` en la raíz del proyecto.
2. El script compilará, publicará y empaquetará la aplicación en un `.zip` dentro de la carpeta `Release_Build`.

## 🖥️ Interfaz de Usuario

1. **Configuración**:
   - Selecciona el algoritmo y el paso (N) si aplica.
   - Introduce peticiones (`98, 183` o `98:0, 183:5`).
   - Define límites y tiempos.

2. **Resultados**:
   - Gráfico visual del recorrido.
   - Estadísticas completas (Tiempos totales, pistas recorridas).
   - Tabla paso a paso con estado de colas.

## 📄 Licencia

Este proyecto está distribuido bajo la licencia **GNU General Public License v3 (GPLv3)**.

---
**Nota**: Proyecto educativo para la asignatura de Diseño de Sistemas Operativos (DESO).
