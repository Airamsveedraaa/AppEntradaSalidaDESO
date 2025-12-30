using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppEntradaSalidaDESO.Models;
using AppEntradaSalidaDESO.Services;
using System.Collections.ObjectModel;

namespace AppEntradaSalidaDESO.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AlgorithmService _algorithmService;
        private readonly DiskCalculationService _calculationService;

        [ObservableProperty]
        private ObservableCollection<string> _algorithms;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
        private string? _selectedAlgorithmName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
        private string _requestsInput = string.Empty;

        [ObservableProperty]
        private int _initialPosition = 50;

        [ObservableProperty]
        private int _minCylinder = 0;

        [ObservableProperty]
        private int _maxCylinder = 199;

        [ObservableProperty]
        private bool _isDirectionRequired;

        [ObservableProperty]
        private string _selectedDirection = "up";

        [ObservableProperty]
        private string _resultOutput = string.Empty;

        [ObservableProperty]
        private ExerciseResult? _currentResult;

        // Propiedades para geometría del disco
        [ObservableProperty]
        private bool _useBlockConversion = false;

        [ObservableProperty]
        private int _sectorsPerTrack = 10;

        [ObservableProperty]
        private int _cylinders = 100;

        [ObservableProperty]
        private int _faces = 2;

        [ObservableProperty]
        private int _sectorSize = 512;

        [ObservableProperty]
        private int _blockSize = 1024;

        [ObservableProperty]
        private double _blocksPerCylinder = 0;

        // Propiedades para cálculos de tiempo
        [ObservableProperty]
        private bool _calculateAccessTime = false;

        [ObservableProperty]
        private double _seekTimePerTrack = 1.0;

        [ObservableProperty]
        private int _rpm = 7200;

        [ObservableProperty]
        private int _sectorsPerBlock = 2;

        [ObservableProperty]
        private ObservableCollection<StepRow> _stepsTable = new();

        public MainViewModel()
        {
            _algorithmService = new AlgorithmService();
            _calculationService = new DiskCalculationService();
            Algorithms = new ObservableCollection<string>(_algorithmService.GetAlgorithmNames());
            SelectedAlgorithmName = Algorithms.FirstOrDefault();
        }

        partial void OnSelectedAlgorithmNameChanged(string? value)
        {
            if (value != null)
            {
                var alg = _algorithmService.GetAlgorithm(value);
                IsDirectionRequired = alg?.RequiresDirection ?? false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteAlgorithm))]
        private void Execute()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedAlgorithmName)) return;

                // Parse requests (pueden ser bloques o pistas)
                var inputRequests = ParseRequests(RequestsInput);
                if (inputRequests.Count == 0)
                {
                    ResultOutput = "Error: Por favor introduce al menos una petición de disco válida.";
                    return;
                }

                // Convertir bloques a pistas si está habilitado
                List<int> requests;
                string conversionInfo = "";
                
                if (UseBlockConversion)
                {
                    try
                    {
                        var diskSpecs = new DiskSpecs(SectorsPerTrack, Cylinders, Faces, SectorSize, BlockSize);
                        requests = _calculationService.BlocksToTracks(inputRequests, diskSpecs);
                        
                        // Agregar información de conversión
                        conversionInfo = "=== Conversión de Bloques a Pistas ===\n";
                        for (int i = 0; i < inputRequests.Count; i++)
                        {
                            conversionInfo += $"  Bloque {inputRequests[i]} → Pista {requests[i]}\n";
                        }
                        conversionInfo += "\n";
                    }
                    catch (Exception ex)
                    {
                        ResultOutput = $"Error en conversión de bloques: {ex.Message}";
                        return;
                    }
                }
                else
                {
                    requests = inputRequests;
                }

                // Validaciones
                if (InitialPosition < MinCylinder || InitialPosition > MaxCylinder)
                {
                    ResultOutput = $"Error: La posición inicial ({InitialPosition}) debe estar entre el cilindro mínimo ({MinCylinder}) y máximo ({MaxCylinder}).";
                    return;
                }

                foreach (var req in requests)
                {
                    if (req < MinCylinder || req > MaxCylinder)
                    {
                        ResultOutput = $"Error: La petición al cilindro {req} está fuera de los límites ({MinCylinder}-{MaxCylinder}).";
                        return;
                    }
                }

                var algorithm = _algorithmService.GetAlgorithm(SelectedAlgorithmName);
                if (algorithm == null)
                {
                    ResultOutput = "Error: Algoritmo no encontrado.";
                    return;
                }

                // Execute algorithm
                CurrentResult = algorithm.Execute(InitialPosition, requests, MinCylinder, MaxCylinder, SelectedDirection);
                
                // Calcular tiempos de acceso si está habilitado
                if (CalculateAccessTime && CurrentResult != null)
                {
                    try
                    {
                        var timeSpecs = new TimeSpecs(SeekTimePerTrack, Rpm, SectorsPerBlock);
                        var diskSpecs = UseBlockConversion 
                            ? new DiskSpecs(SectorsPerTrack, Cylinders, Faces, SectorSize, BlockSize)
                            : null;

                        CurrentResult.AccessTime = _calculationService.CalculateAccessTime(
                            CurrentResult.TotalHeadMovement,
                            CurrentResult.ProcessingOrder.Count,
                            timeSpecs,
                            diskSpecs
                        );
                    }
                    catch (Exception ex)
                    {
                        conversionInfo += $"\nAdvertencia: No se pudieron calcular los tiempos de acceso: {ex.Message}\n";
                    }
                }
                
                // Format output (incluir info de conversión si existe)
                if (!string.IsNullOrEmpty(conversionInfo))
                {
                    ResultOutput = conversionInfo;
                }
                
                FormatResultOutput();
            }
            catch (Exception ex)
            {
                ResultOutput = $"Error inesperado: {ex.Message}";
            }
        }

        private bool CanExecuteAlgorithm()
        {
            return !string.IsNullOrWhiteSpace(SelectedAlgorithmName) && !string.IsNullOrWhiteSpace(RequestsInput);
        }

        private List<int> ParseRequests(string input)
        {
            var list = new List<int>();
            if (string.IsNullOrWhiteSpace(input)) return list;

            var parts = input.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int val))
                {
                    list.Add(val);
                }
            }
            return list;
        }

        private void FormatResultOutput()
        {
            if (CurrentResult == null) return;

            // Generar Texto
            var sb = new StringBuilder();
            
            // Mantener info de conversión si ya existe
            if (!string.IsNullOrEmpty(ResultOutput) && ResultOutput.Contains("Conversión"))
            {
                sb.Append(ResultOutput);
            }
            
            sb.AppendLine($"=== Resultado: {CurrentResult.AlgorithmName} ===");
            sb.AppendLine($"Posición Inicial: {CurrentResult.InitialPosition}");
            sb.AppendLine($"Límites: {MinCylinder} - {MaxCylinder}");
            sb.AppendLine($"Dirección: {(CurrentResult.Direction == "up" ? "Ascendente (↑)" : "Descendente (↓)")}");
            sb.AppendLine($"Movimiento Total: {CurrentResult.TotalHeadMovement} cilindros");
            sb.AppendLine($"Tiempo Promedio: {CurrentResult.AverageSeekTime:F2} cilindros/petición");
            
            // Agregar información de tiempos si está disponible
            if (CurrentResult.AccessTime != null)
            {
                sb.AppendLine();
                sb.AppendLine("=== Tiempos de Acceso ===");
                sb.AppendLine(CurrentResult.AccessTime.ToDetailedString());
            }
            
            ResultOutput = sb.ToString();

            // Generar Tabla
            StepsTable.Clear();
            int currentPos = CurrentResult.InitialPosition;
            int stepNum = 1;
            int totalDistance = 0;

            // Añadir paso inicial
            StepsTable.Add(new StepRow(0, "-", currentPos, 0, 0, "Inicio"));

            foreach (var target in CurrentResult.ProcessingOrder)
            {
                int distance = Math.Abs(target - currentPos);
                totalDistance += distance;
                StepsTable.Add(new StepRow(
                    stepNum++, 
                    currentPos, 
                    target, 
                    distance, 
                    totalDistance, 
                    target > currentPos ? "Arriba ↑" : "Abajo ↓"
                ));
                currentPos = target;
            }
        }

        /// <summary>
        /// Comando para calcular bloques por cilindro
        /// </summary>
        [RelayCommand]
        private void CalculateBlocksPerCylinder()
        {
            try
            {
                var diskSpecs = new DiskSpecs(SectorsPerTrack, Cylinders, Faces, SectorSize, BlockSize);
                BlocksPerCylinder = _calculationService.CalculateBlocksPerCylinder(diskSpecs);
                
                // Actualizar también SectorsPerBlock automáticamente
                SectorsPerBlock = _calculationService.CalculateSectorsPerBlock(diskSpecs);
                
                ResultOutput = $"=== Cálculos de Geometría del Disco ===\n" +
                              $"Bloques por cilindro: {BlocksPerCylinder:F2}\n" +
                              $"Bloques por pista: {diskSpecs.BlocksPerTrack:F2}\n" +
                              $"Bytes por pista: {diskSpecs.BytesPerTrack}\n" +
                              $"Sectores por bloque: {SectorsPerBlock}";
            }
            catch (Exception ex)
            {
                ResultOutput = $"Error al calcular: {ex.Message}";
            }
        }

        /// <summary>
        /// Comando para convertir un bloque específico a pista (para pruebas rápidas)
        /// </summary>
        [RelayCommand]
        private void ConvertSingleBlock(string blockNumberStr)
        {
            try
            {
                if (int.TryParse(blockNumberStr, out int blockNumber))
                {
                    var diskSpecs = new DiskSpecs(SectorsPerTrack, Cylinders, Faces, SectorSize, BlockSize);
                    int track = _calculationService.BlockToTrack(blockNumber, diskSpecs);
                    ResultOutput = $"Conversión: Bloque {blockNumber} → Pista {track}";
                }
                else
                {
                    ResultOutput = "Error: Número de bloque inválido";
                }
            }
            catch (Exception ex)
            {
                ResultOutput = $"Error en conversión: {ex.Message}";
            }
        }
    }

    public record StepRow(int Step, object From, int To, int Distance, int Accumulator, string Direction);
}
