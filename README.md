# Simulador de Ejercicios - Entrada/Salida DESO / Disk Scheduling Simulator

![Icon](https://img.shields.io/badge/Release-v0.2.0_Beta-blue) ![License](https://img.shields.io/badge/License-GPLv3-green)

🇪🇸 **Español** | 🇺🇸 **English** (Scroll down)

---

## 🇪🇸 Español

Aplicación de escritorio en C# WPF para practicar algoritmos de planificación de E/S de disco, diseñada para resolver ejercicios académicos de la asignatura DESO.

### 📋 Descripción
Permite practicar y comprender algoritmos de planificación de disco con visualizaciones interactivas, conversor de geometría física a lógica, y soporte para llegadas dinámicas.

### 🚀 Novedades v0.2.0 Beta
- **Nuevos Algoritmos**:
  - **SCAN-N**: N-Step SCAN (procesamiento por lotes).
  - **LOOK-N**: N-Step LOOK (procesamiento por lotes con inversión inteligente).
- **Mejoras en el Conversor**:
  - **Cálculo Bi-direccional**: Calcula Cilindros dada la Capacidad O Capacidad dadas las especificaciones.
  - **Índice de Pista (0/1)**: Alterna entre indexación basada en 0 o 1.
  - **Rango de Pistas**: Visualiza claramente el rango efectivo (ej: `0 - 511`).
  - **Copiar Pistas**: Genera listas de pistas listas para pegar en el simulador.
- **Interfaz Mejorada**:
  - Cabeceras de tabla actualizadas a terminología académica estándar (`Instante`, `Pendientes`, `Recorridas`).
  - Visualización gráfica mejorada.

### 🎯 Algoritmos Soportados
- **FCFS, SSTF**
- **SCAN, C-SCAN, LOOK, C-LOOK**
- **F-SCAN, F-LOOK**
- **SCAN-N, LOOK-N** (¡Nuevo!)

### 🛠️ Requisitos
- Windows 10/11
- .NET 8.0 Desktop Runtime

### 📦 Instalación
1. Descarga el `.zip` de la última release.
2. Descomprime y ejecuta `AppEntradaSalidaDESO.exe`.

---

## 🇺🇸 English

WPF Desktop Application for practicing Disk I/O Scheduling algorithms, designed for academic exercises (DESO).

### 📋 Description
Practice and understand disk scheduling algorithms with interactive visualizations, physical-to-logical geometry converter, and support for dynamic arrival times.

### 🚀 What's New in v0.2.0 Beta
- **New Algorithms**:
  - **SCAN-N**: N-Step SCAN (batch processing).
  - **LOOK-N**: N-Step LOOK (batch processing with smart reversal).
- **Converter Enhancements**:
  - **Bi-directional Calculation**: Solve for Cylinders OR Capacity.
  - **Track Index Toggle (0/1)**: Switch between 0-based and 1-based indexing.
  - **Track Range Display**: Clearly see the effective range (e.g., `0 - 511`).
  - **Copy Tracks**: Easily generate track lists for the simulator.
- **UI Improvements**:
  - Table headers updated to standard academic terminology.
  - Enhanced graph visualization.

### 🎯 Supported Algorithms
- **FCFS, SSTF**
- **SCAN, C-SCAN, LOOK, C-LOOK**
- **F-SCAN, F-LOOK**
- **SCAN-N, LOOK-N** (New!)

### 🛠️ Requirements
- Windows 10/11
- .NET 8.0 Desktop Runtime

### 📦 Installation
1. Download the `.zip` from the latest release.
2. Unzip and run `AppEntradaSalidaDESO.exe`.

---
**License**: GNU GPLv3
**Author**: Airamsveedraaa
