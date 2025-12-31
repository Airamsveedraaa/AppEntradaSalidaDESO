using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// First Come First Served - Atiende las peticiones en el orden en que llegan
    /// Soporta tiempos de llegada dinámicos
    /// </summary>
    public class FCFSAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "FCFS";
        public string Description => "First Come First Served - Atiende las peticiones en orden de llegada";
        public bool RequiresDirection => false;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0, int nStep = 2)
        {
            var result = new ExerciseResult(Name, initialPosition, requests.Select(r => r.Position).ToList());
            result.Direction = direction;
            int currentPosition = initialPosition;
            double currentTime = 0.0;
            int totalMovement = 0;

            result.AddStep($"Posición inicial: {initialPosition}");
            result.AddStep($"Peticiones ({requests.Count}): {string.Join(", ", requests.Select(r => $"{r.Position}(T={r.ArrivalTime})"))}");
            result.AddStep("");

            // En FCFS, el orden está determinado únicamente por ArrivalTime y luego por OriginalIndex
            // Clonamos la lista para no modificar la original entrada
            var pendingRequests = requests.OrderBy(r => r.ArrivalTime).ThenBy(r => r.OriginalIndex).ToList();
            var processedRequests = new List<DiskRequest>();

            while (pendingRequests.Count > 0)
            {
                var nextRequest = pendingRequests[0];
                
                // Si la siguiente petición llega en el futuro, el disco debe esperar
                if (nextRequest.ArrivalTime > currentTime)
                {
                    result.AddStep($"T={currentTime:F2}: Esperando petición {nextRequest.Position} que llega en T={nextRequest.ArrivalTime:F2}...");
                    currentTime = nextRequest.ArrivalTime;
                }

                int movement = Math.Abs(nextRequest.Position - currentPosition);
                double travelTime = movement * timePerTrack;
                double processTime = timePerRequest;
                
                // Generar paso detallado
                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = nextRequest.Position,
                    Distance = movement,
                    Instant = currentTime,
                    ArrivalInstant = nextRequest.ArrivalTime,
                    Remaining = pendingRequests.Skip(1).Select(r => r.Position).ToList()
                };
                result.DetailedSteps.Add(step);

                // Avanzar tiempo y posición
                currentTime += travelTime; // Tiempo de viaje
                currentTime += processTime; // Tiempo de procesamiento
                totalMovement += movement;
                currentPosition = nextRequest.Position;

                // Registrar procesamiento
                result.ProcessingOrder.Add(nextRequest.Position);
                result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {nextRequest.Position} (Dist: {movement}, Llegó: {nextRequest.ArrivalTime})");

                processedRequests.Add(nextRequest);
                pendingRequests.RemoveAt(0);
            }

            result.TotalHeadMovement = totalMovement;
            result.TotalTime = currentTime;
            result.CalculateMetrics();

            return result;
        }
    }
}
