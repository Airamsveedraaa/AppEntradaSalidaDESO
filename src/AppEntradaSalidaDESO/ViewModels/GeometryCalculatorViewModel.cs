using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppEntradaSalidaDESO.Models;
using AppEntradaSalidaDESO.Services;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AppEntradaSalidaDESO.ViewModels
{
    public partial class GeometryCalculatorViewModel : ObservableObject
    {
        private readonly DiskCalculationService _calculationService;

        [ObservableProperty]
        private DiskSpecs _specs = new DiskSpecs();

        [ObservableProperty]
        private string _blockInputs = string.Empty;

        [ObservableProperty]
        private string _conversionResult = string.Empty;

        // Tamaño del disco (entrada opcional)
        [ObservableProperty]
        private double _diskSizeValue = 0;

        [ObservableProperty]
        private string _selectedSizeUnit = "MB";

        public List<string> SizeUnits { get; } = new List<string> { "Byte", "KB", "MB", "GB", "TB" };

        // Campos calculados
        [ObservableProperty]
        private double _blocksPerCylinder;

        [ObservableProperty]
        private double _blocksPerTrack;

        [ObservableProperty]
        private long _totalSectors;

        [ObservableProperty]
        private long _totalBlocks;

        [ObservableProperty]
        private long _totalCapacityBytes;

        [ObservableProperty]
        private double _totalCapacityMB;

        [ObservableProperty]
        private double _totalCapacityGB;

        [ObservableProperty]
        private int _sectorsPerCylinder;

        [ObservableProperty]
        private long _bytesPerCylinder;

        public GeometryCalculatorViewModel()
        {
            _calculationService = new DiskCalculationService();
            UpdateCalculations();
        }

        [RelayCommand]
        private void Calculate()
        {
             UpdateCalculations();
        }

        [RelayCommand]
        private void CalculateFromDiskSize()
        {
            try
            {
                // Convertir tamaño del disco a bytes
                long diskSizeBytes = ConvertToBytes(DiskSizeValue, SelectedSizeUnit);

                if (diskSizeBytes <= 0)
                {
                    ConversionResult = "Error: El tamaño del disco debe ser mayor que 0.";
                    return;
                }

                // Fórmula: nº de sectores = tam disco(bytes) / tam sector(bytes)
                if (Specs.SectorSize > 0)
                {
                    long calculatedSectors = diskSizeBytes / Specs.SectorSize;
                    TotalSectors = calculatedSectors;

                    // Fórmula: nº de pistas = nº de sectores / (nº de sectores/pista * nº de caras)
                    if (Specs.SectorsPerTrack > 0 && Specs.Faces > 0)
                    {
                        int calculatedCylinders = (int)(calculatedSectors / (Specs.SectorsPerTrack * Specs.Faces));
                        Specs.Cylinders = calculatedCylinders;
                    }

                    // Fórmula: nº de bloques = tam disco(bytes) / tam bloque(bytes)
                    if (Specs.BlockSize > 0)
                    {
                        TotalBlocks = diskSizeBytes / Specs.BlockSize;
                    }
                }

                UpdateCalculations();
            }
            catch (Exception ex)
            {
                ConversionResult = $"Error al calcular desde tamaño: {ex.Message}";
            }
        }

        partial void OnSpecsChanged(DiskSpecs value)
        {
            UpdateCalculations();
        }

        partial void OnBlockInputsChanged(string value)
        {
            UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            try 
            {
                // Cálculos básicos
                BlocksPerCylinder = _calculationService.CalculateBlocksPerCylinder(Specs);
                
                // Fórmula: nº de bloques/pista = (sectores/pista * tam sector) / tam bloque
                if (Specs.BlockSize > 0)
                {
                    BlocksPerTrack = (Specs.SectorsPerTrack * Specs.SectorSize) / (double)Specs.BlockSize;
                }

                SectorsPerCylinder = Specs.SectorsPerTrack * Specs.Faces;
                
                // Fórmula: nº de sectores = nº de pistas * nº de sectores/pista * nº de caras
                TotalSectors = (long)Specs.Cylinders * Specs.Faces * Specs.SectorsPerTrack;
                
                // Capacidad total = Total sectores × Tamaño sector
                TotalCapacityBytes = TotalSectors * Specs.SectorSize;
                TotalCapacityMB = TotalCapacityBytes / (1024.0 * 1024.0);
                TotalCapacityGB = TotalCapacityMB / 1024.0;

                // Fórmula: nº de bloques = tam disco(bytes) / tam bloque(bytes)
                if (Specs.BlockSize > 0)
                {
                    TotalBlocks = TotalCapacityBytes / Specs.BlockSize;
                }

                // Bytes por cilindro
                BytesPerCylinder = SectorsPerCylinder * Specs.SectorSize;

                // Convert Blocks if input exists
                if (!string.IsNullOrWhiteSpace(BlockInputs))
                {
                    var parts = BlockInputs.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var sb = new StringBuilder();
                    sb.AppendLine("Conversión Bloque → Pista:");
                    sb.AppendLine("---------------------------");

                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int blockNum))
                        {
                            int track = _calculationService.BlockToTrack(blockNum, Specs);
                            
                            // Fórmula: nº de bloques/pista = nº de bloques / nº de pistas
                            sb.AppendLine($"Bloque {blockNum} => Pista {track}");
                        }
                        else
                        {
                            sb.AppendLine($"'{part}': No es un número válido");
                        }
                    }
                    ConversionResult = sb.ToString();
                }
                else
                {
                    ConversionResult = "Introduce números de bloque para convertir.";
                }
            }
            catch (Exception ex)
            {
                ConversionResult = $"Error: {ex.Message}";
            }
        }

        private long ConvertToBytes(double value, string unit)
        {
            return unit switch
            {
                "Byte" => (long)value,
                "KB" => (long)(value * 1024),
                "MB" => (long)(value * 1024 * 1024),
                "GB" => (long)(value * 1024 * 1024 * 1024),
                "TB" => (long)(value * 1024L * 1024L * 1024L * 1024L),
                _ => 0
            };
        }
    }
}
