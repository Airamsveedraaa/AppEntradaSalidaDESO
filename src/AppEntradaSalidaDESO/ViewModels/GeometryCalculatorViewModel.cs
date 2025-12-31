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

        private void UpdateCalculations()
        {
            try 
            {
                // Update derived stats
                BlocksPerCylinder = _calculationService.CalculateBlocksPerCylinder(Specs);
                SpecsSummary = Specs.ToString();

                // Convert Blocks if input exists
                if (!string.IsNullOrWhiteSpace(BlockInputs))
                {
                    var parts = BlockInputs.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var sb = new StringBuilder();
                    sb.AppendLine("Conversión Bloque -> Pista:");
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
