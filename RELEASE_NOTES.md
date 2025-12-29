# Release Notes - v0.1.0 Beta

**Initial Beta Release for Testing**

This release marks the first public beta of the Disk Scheduling Simulator. It includes the core logic for 8 algorithms and a functional WPF User Interface.

## 🚀 Features

### Core Algorithms
- **Classic**: FCFS, SSTF
- **Scanning**: SCAN, C-SCAN, LOOK, C-LOOK
- **Freezing Variants**: F-SCAN, F-LOOK
- **Dynamic Configuration**: Support for custom disk limits (Min/Max cylinders), initial head position, and direction.

### User Interface (WPF)
- **Configuration Panel**: Easy setup for request queues and disk parameters.
- **Visual Results**:
    - **Step-by-Step Table**: Detailed breakdown of every head movement, distance, and direction.
    - **Summary Metrics**: Total Head Movement and Seek Time analysis.
- **Modern Design**: Clean MVVM architecture with a custom application icon.

## 🐛 Known Issues (Beta)
- Graphical visualization (Canvas drawing of head movement) is planned but not yet implemented.
- Build process may require manual cleaning of `obj/` folders if switching configurations rapidly.

## 📦 Installation
Download the source and run:
```bash
dotnet run
```
Or publish a standalone executable:
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```
