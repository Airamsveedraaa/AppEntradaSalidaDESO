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

                // Parse requests (pueden ser bloques o pistas, formato Track:Time opcional)
                var inputRequests = ParseRequests(RequestsInput);
                if (inputRequests.Count == 0)
                {
                    ResultOutput = "Error: Por favor introduce al menos una petición de disco válida.";
                    return;
                }

                // Convertir bloques a pistas si está habilitado
                List<DiskRequest> requests;
                string conversionInfo = "";
                
                if (UseBlockConversion)
                {
                    try
                    {
                        var diskSpecs = new DiskSpecs(SectorsPerTrack, Cylinders, Faces, SectorSize, BlockSize);
                        var trackInts = _calculationService.BlocksToTracks(inputRequests.Select(r => r.Position).ToList(), diskSpecs);
                        
                        // Reconstruct DiskRequests with converted tracks but original times
                        requests = new List<DiskRequest>();
                        conversionInfo = "=== Conversión de Bloques a Pistas ===\n";
                        
                        for (int i = 0; i < inputRequests.Count; i++)
                        {
                            requests.Add(new DiskRequest(trackInts[i], inputRequests[i].Order, inputRequests[i].ArrivalTime));
                            conversionInfo += $"  Bloque {inputRequests[i].Position} (T={inputRequests[i].ArrivalTime}) → Pista {trackInts[i]}\n";
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
                    if (req.Position < MinCylinder || req.Position > MaxCylinder)
                    {
                        ResultOutput = $"Error: La petición al cilindro {req.Position} está fuera de los límites ({MinCylinder}-{MaxCylinder}).";
                        return;
                    }
                }

                var algorithm = _algorithmService.GetAlgorithm(SelectedAlgorithmName);
                if (algorithm == null)
                {
                    ResultOutput = "Error: Algoritmo no encontrado.";
                    return;
                }

                // Preparar parámetros de tiempo para la SIMULACIÓN
                double timePerTrack = SeekTimePerTrack;
                double timePerRequest = 0.0; // Latencia + Transferencia

                if (CalculateAccessTime)
                {
                    // Calcular Latencia Promedio + Tiempo de Transferencia
                    // Latencia = (60000 / RPM) / 2
                    // Transferencia = (60000 / RPM) * (SectorsPerBlock / SectorsPerTrack)
                    // Nota: Para transferencia exacta necesitamos DiskSpecs si usamos conversión, o inputs manuales.
                    // Usamos las propiedades del VM que ya tienen defaults.
                    
                    double rotationTimeMs = 60000.0 / Rpm;
                    double latencyMs = rotationTimeMs / 2.0;
                    
                    // Si usamos conversión, SectorsPerBlock se calculó. Si no, usa el input directo.
                    double transferMs = rotationTimeMs * ((double)SectorsPerBlock / SectorsPerTrack);
                    
                    timePerRequest = latencyMs + transferMs;
                }

                // Execute algorithm (SIMULACIÓN con tiempos)
                CurrentResult = algorithm.Execute(InitialPosition, requests, MinCylinder, MaxCylinder, SelectedDirection, timePerTrack, timePerRequest);
                
                // Los tiempos ya vienen calculados en CurrentResult.TotalTime por la simulación
                // Pero podemos enriquecer el AccessTimeResult para desglose teórico si se desea.
                // Por coherencia, usaremos el TotalTime de la simulación como el "Access Time Real" en el reporte.
                // Crearemos un AccessTimeResult basado en la simulación.
                
                if (CalculateAccessTime && CurrentResult != null)
                {
                     CurrentResult.AccessTime = new AccessTimeResult
                     {
                         SeekTimeMs = CurrentResult.TotalHeadMovement * timePerTrack,
                         // El resto del tiempo total es "Latency + Transfer + Idle"
                         // TotalTime = SeekTime + (NumRequests * TimePerRequest) + IdleTime
                         TotalTimeMs = CurrentResult.TotalTime,
                         // Aproximaciones para display:
                         LatencyTimeMs = requests.Count * (60000.0 / Rpm) / 2.0,
                         TransferTimeMs = requests.Count * (60000.0 / Rpm) * ((double)SectorsPerBlock / SectorsPerTrack)
                     };
                }
                
                // Format output
                if (!string.IsNullOrEmpty(conversionInfo))
                {
                    ResultOutput = conversionInfo;
                }
                else 
                {
                    ResultOutput = "";
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

        private List<DiskRequest> ParseRequests(string input)
        {
            var list = new List<DiskRequest>();
            if (string.IsNullOrWhiteSpace(input)) return list;

            var parts = input.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int order = 1;
            foreach (var part in parts)
            {
                // Formato admitido: "100" o "100:0.5" (Pista:Tiempo)
                var segments = part.Split(':');
                if (int.TryParse(segments[0], out int pos))
                {
                    double time = 0.0;
                    if (segments.Length > 1 && double.TryParse(segments[1], out double t))
                    {
                        time = t;
                    }
                    list.Add(new DiskRequest(pos, order++, time));
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
            sb.AppendLine($"Tiempo Total Simulación: {CurrentResult.TotalTime:F2} ms"); // Nuevo
            
            // Agregar información de tiempos si está disponible
            if (CurrentResult.AccessTime != null)
            {
                sb.AppendLine();
                sb.AppendLine("=== Tiempos de Acceso (Estimados) ===");
                sb.AppendLine(CurrentResult.AccessTime.ToDetailedString());
            }
            
            ResultOutput = sb.ToString();

            // Generar Tabla
            StepsTable.Clear();
            
            // Si tenemos pasos detallados (Fase 2), usarlos
            if (CurrentResult.DetailedSteps.Count > 0)
            {
                int stepNum = 1;
                int totalDistance = 0;
                
                // Mostrar estado inicial
                StepsTable.Add(new StepRow(0, "-", CurrentResult.InitialPosition, 0, 0, "Inicio", 0.0));

                foreach (var step in CurrentResult.DetailedSteps)
                {
                    totalDistance += step.Distance;
                    string note = "";
                    if (step.Distance == 0 && step.Remaining != null && step.Remaining.Count > 0)
                        note = " (Wait/Jump)"; // O salto sin movimiento

                    StepsTable.Add(new StepRow(
                        stepNum++, 
                        step.From, 
                        step.To, 
                        step.Distance, 
                        totalDistance, 
                        step.To > step.From ? "↑" : (step.To < step.From ? "↓" : "-"),
                        step.Instant // Timestamp al INICIO del movimiento
                    ));
                }
            }
            else
            {
                // Fallback a lógica simple (Fase 1)
                int currentPos = CurrentResult.InitialPosition;
                int stepNum = 1;
                int totalDistance = 0;

                StepsTable.Add(new StepRow(0, "-", currentPos, 0, 0, "Inicio", 0.0));

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
                        target > currentPos ? "Arriba ↑" : "Abajo ↓",
                        0.0
                    ));
                    currentPos = target;
                }
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

    public record StepRow(int Step, object From, int To, int Distance, int Accumulator, string Direction, double Instant = 0.0);
}
