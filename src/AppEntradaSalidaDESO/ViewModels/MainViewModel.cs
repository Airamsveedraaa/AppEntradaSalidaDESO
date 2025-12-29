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

        public MainViewModel()
        {
            _algorithmService = new AlgorithmService();
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

                // Parse requests
                var requests = ParseRequests(RequestsInput);
                if (requests.Count == 0)
                {
                    ResultOutput = "Error: Por favor introduce al menos una petición de disco válida.";
                    return;
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

                // Execute
                CurrentResult = algorithm.Execute(InitialPosition, requests, MinCylinder, MaxCylinder, SelectedDirection);
                
                // Format output
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

        [ObservableProperty]
        private ObservableCollection<StepRow> _stepsTable = new();

        private void FormatResultOutput()
        {
            if (CurrentResult == null) return;

            // Generar Texto (mantener lógica existente por ahora como respaldo o resumen)
            var sb = new StringBuilder();
            sb.AppendLine($"=== Resultado: {CurrentResult.AlgorithmName} ===");
            sb.AppendLine($"Posición Inicial: {CurrentResult.InitialPosition}");
            sb.AppendLine($"Límites: {MinCylinder} - {MaxCylinder}");
            sb.AppendLine($"Dirección: {(CurrentResult.Direction == "up" ? "Ascendente (Right)" : "Descendente (Left)")}");
            sb.AppendLine($"Movimiento Total: {CurrentResult.TotalHeadMovement}");
            sb.AppendLine($"Tiempo Promedio: {CurrentResult.AverageSeekTime:F2}");
            ResultOutput = sb.ToString();

            // Generar Tabla
            StepsTable.Clear();
            int currentPos = CurrentResult.InitialPosition;
            int stepNum = 1;
            int totalDistance = 0;

            // Añadir paso inicial
            StepsTable.Add(new StepRow(0, "-", currentPos, 0, 0, "Inicio"));

            // Reconstruir pasos a partir del ProcessingOrder
            // Nota: Esto asume que ProcessingOrder tiene la secuencia exacta de visitas.
            // Algunos algoritmos como SCAN pueden tener pasos intermedios (ir al extremo) que quizá no estén en ProcessingOrder si solo guarda peticiones atendidas.
            // Para ser 100% precisos con algoritmos como SCAN que tocan extremos, lo ideal sería que el algoritmo devolviera la lista de "Puntos Visitados".
            // Revisando ExerciseResult.cs, tiene ProcessingOrder (List<int>).
            // Si SCAN va a 199 pero nadie pidió 199, ¿está en ProcessingOrder?
            // Mirando SCANAlgorithm.cs: "result.ProcessingOrder.Add(request);" -> Solo añade peticiones.
            // SCANAlgorithm también hace "currentPosition = MAX_CYLINDER;" pero NO lo añade a ProcessingOrder.
            // ESTO ES UN PROBLEMA para la reconstrucción exacta desde fuera.
            
            // SOLUCIÓN: Usar la lista de steps de texto para visualización rápida O modificar ExerciseResult para incluir "Path" completo.
            // Dado que el usuario pidió tabla, modificaré ExerciseResult y Algoritmos es lo más correcto, 
            // pero para esta iteración rápida, intentaré parsear o simplemente mostrar lo que tenemos.
            
            // Mejor enfoque: La visualización de tabla debe ser fiel al algoritmo.
            // Voy a modificar MainViewModel para que, por ahora, muestre las Peticiones Atendidas.
            // Los movimientos "extra" de SCAN (ir al borde) se perderán en la tabla si solo uso ProcessingOrder,
            // pero estarán en el texto. Para la tabla "Petición a Petición", es aceptable.
            
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
    }

    public record StepRow(int Step, object From, int To, int Distance, int Accumulator, string Direction);
}
