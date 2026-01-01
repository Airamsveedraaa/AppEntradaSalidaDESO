Release Notes - v0.2.0 Beta
LOOK-N, SCAN-N & Logic Converter Update

This release introduces advanced batch-processing algorithms (SCAN-N, LOOK-N) and significantly improves the Disk Geometry Converter with bi-directional solving and index management.

🚀 Features / Novedades

🇪🇸 Español
- **Nuevos Algoritmos**: Implementación de **SCAN-N** y **LOOK-N** para procesar peticiones en lotes (pasos de tamaño N).
- **Conversor de Geometría Pro**:
  - **Modo Resolver**: Calcula Cilindros a partir de Capacidad y viceversa.
  - **Índice de Pista**: Alterna entre índice 0 y 1 para ajustar la numeración de pistas.
  - **Copiar Pistas**: Genera una lista formateada de pistas para pegar directamente en la simulación.
- **Mejoras Visuales**: Cabeceras de tabla estandarizadas (`Instante`, `Pendientes`, `Recorridas`) y gráficos ajustados.
- **Simulación Robusta**: Corrección en la lógica de separación de colas para algoritmos N-Step.

🇺🇸 English
- **New Algorithms**: Implementation of **SCAN-N** and **LOOK-N** for batched request processing (N-Step).
- **Pro Geometry Converter**:
  - **Solve Mode**: Calculate Cylinders from Capacity and vice-versa.
  - **Track Indexing**: Toggle between 0-based and 1-based track numbering.
  - **Copy Tracks**: Generate formatted track lists ready for simulation input.
- **Visual Improvements**: Standardized table headers (`Instante`, `Pendientes`, `Recorridas`) and tweaked visualizations.
- **Robust Simulation**: Fixed queue separation logic for N-Step algorithms.

🐛 Bug Fixes
- Fixed "Simulate" button state when using dynamic arrivals.
- Corrected logic for LOOK-N direction reversal (now reverses at last request, not disk edge).
- Fixed UI binding for First Track Index.

📦 Installation / Instalación
1. **Download/Descarga**: `SimuladorDiscos_v0.2.0_Beta.zip`
2. **Run/Ejecuta**: `AppEntradaSalidaDESO.exe`
