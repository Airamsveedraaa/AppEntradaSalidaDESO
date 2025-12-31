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

        [ObservableProperty]
        private double _blocksPerCylinder;

        [ObservableProperty]
        private string _specsSummary = string.Empty;

        // Nuevos campos calculados
        [ObservableProperty]
        private long _totalSectors;

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
                SpecsSummary = Specs.ToString();

                // Cálculos derivados (fórmulas del PDF)
                SectorsPerCylinder = Specs.SectorsPerTrack * Specs.Faces;
                
                // Total de sectores = Cilindros × Caras × Sectores/Pista
                TotalSectors = (long)Specs.Cylinders * Specs.Faces * Specs.SectorsPerTrack;
                
                // Capacidad total = Total sectores × Tamaño sector
                TotalCapacityBytes = TotalSectors * Specs.SectorSize;
                TotalCapacityMB = TotalCapacityBytes / (1024.0 * 1024.0);
                TotalCapacityGB = TotalCapacityMB / 1024.0;

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
    }
}
