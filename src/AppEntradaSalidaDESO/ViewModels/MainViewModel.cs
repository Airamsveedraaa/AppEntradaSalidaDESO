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



        // Propiedades de configuración de Tiempo (Simplificadas - Estilo Web)
        [ObservableProperty]
        private double _timePerTrack = 1.0;

        [ObservableProperty]
        private double _timePerRequest = 0.0;



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

                // (Conversión de bloques eliminada por simplificación)
                List<DiskRequest> requests = inputRequests;

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

                // Preparar parámetros de tiempo (Simplificado)
                // Se toman directamente los valores de la UI
                double timePerTrack = TimePerTrack;
                double timePerRequest = TimePerRequest;
                
                // Format output
                // Format output
                ResultOutput = "";
                
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



    }

    public record StepRow(int Step, object From, int To, int Distance, int Accumulator, string Direction, double Instant = 0.0);
}
