using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppEntradaSalidaDESO.Models;
using AppEntradaSalidaDESO.Services;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<BlockRow> _conversionResults = new ObservableCollection<BlockRow>();
        
        // kept for compatibility if needed, but unused now
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
        private long _totalTracks;

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

        [ObservableProperty]
        private bool _isSolveForCapacity = true; // true = Cylinders -> Capacity; false = Capacity -> Cylinders

        [ObservableProperty]
        private string _capacityInput = string.Empty;

        [ObservableProperty]
        private string _selectedCapacityUnit = "MB";

        [ObservableProperty]
        private string _convertedTracksString = string.Empty;

        [ObservableProperty]
        private int _firstTrackIndex = 0; // 0 or 1

        public bool IsFirstTrackIndexOne
        {
            get => FirstTrackIndex == 1;
            set => FirstTrackIndex = value ? 1 : 0;
        }

        [ObservableProperty]
        private string _trackRangeString = string.Empty;

        public GeometryCalculatorViewModel()
        {
            _calculationService = new DiskCalculationService();
            // Default initialization
            UpdateCalculations();
        }

        partial void OnIsSolveForCapacityChanged(bool value)
        {
            // Reset or re-calculate? 
            UpdateCalculations();
        }

        partial void OnCapacityInputChanged(string value)
        {
             if (!IsSolveForCapacity) UpdateCalculations();
        }

        partial void OnSelectedCapacityUnitChanged(string value)
        {
             if (!IsSolveForCapacity) UpdateCalculations();
        }

        partial void OnFirstTrackIndexChanged(int value)
        {
            OnPropertyChanged(nameof(IsFirstTrackIndexOne));
            UpdateCalculations();
        }

        [RelayCommand]
        private void Calculate()
        {
             UpdateCalculations();
        }

        // Removed CalculateFromDiskSize as it's now integrated into UpdateCalculations via mode
        
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
                // SOLVE MODE LOGIC
                if (!IsSolveForCapacity)
                {
                    // CAPACITY -> CYLINDERS
                    // Capacity = Cylinders * Heads * Sectors * SectorSize
                    // Cylinders = Capacity / (Heads * Sectors * SectorSize)

                    if (double.TryParse(CapacityInput, out double capVal) && Specs.Faces > 0 && Specs.SectorsPerTrack > 0 && Specs.SectorSize > 0)
                    {
                        long capBytes = ConvertToBytes(capVal, SelectedCapacityUnit);
                        long denominator = (long)Specs.Faces * Specs.SectorsPerTrack * Specs.SectorSize;
                        
                        if (denominator > 0)
                        {
                            Specs.Cylinders = (int)(capBytes / denominator);
                        }
                    }
                }
                
                // Existing Calculations based on Stats (which now might be updated from capacity)

                // Cálculos básicos
                BlocksPerCylinder = _calculationService.CalculateBlocksPerCylinder(Specs);
                
                if (Specs.BlockSize > 0)
                {
                    double trackCapacity = Specs.SectorsPerTrack * Specs.SectorSize;
                    BlocksPerTrack = trackCapacity / (double)Specs.BlockSize;
                }

                SectorsPerCylinder = Specs.SectorsPerTrack * Specs.Faces;
                
                // Fórmula: nº de sectores = nº de pistas (cilindros) * nº de caras * nº de sectores/pista
                TotalSectors = (long)Specs.Cylinders * Specs.Faces * Specs.SectorsPerTrack;
                
                // Fórmula: Total Pistas = Cilindros * Caras
                TotalTracks = (long)Specs.Cylinders * Specs.Faces;

                // Track Range String (visual aid)
                long maxTrackIndex = TotalTracks > 0 ? TotalTracks - 1 + FirstTrackIndex : 0;
                long startTrack = TotalTracks > 0 ? FirstTrackIndex : 0;
                TrackRangeString = $"{TotalTracks:N0} ({startTrack} - {maxTrackIndex})";

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
                ConversionResults.Clear();
                var trackList = new List<int>();
                if (!string.IsNullOrWhiteSpace(BlockInputs))
                {
                    var parts = BlockInputs.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int blockNum))
                        {
                            var chs = _calculationService.BlockToCHS(blockNum, Specs);
                            
                            // Adjust for FirstTrackIndex
                            int finalCylinder = chs.Cylinder + FirstTrackIndex;

                            // Note: Head/Sector usually are 0-based and 1-based respectively in physics, 
                            // but user specifically requested offset for "resulting track" (cylinder).
                            
                            ConversionResults.Add(new BlockRow(blockNum, finalCylinder, chs.Head, chs.Sector));
                            trackList.Add(finalCylinder);
                        }
                    }
                }
                ConvertedTracksString = string.Join(", ", trackList);
            }
            catch (Exception ex)
            {
                // Simple error handling for now explanation in tooltip or simple message
                // ConversionResult = $"Error: {ex.Message}"; // We removed string property
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

    public record BlockRow(int Block, int Cylinder, int Head, int Sector);
}
