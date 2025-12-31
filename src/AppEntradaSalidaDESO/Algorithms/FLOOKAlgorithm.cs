using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// F-LOOK (Frozen LOOK) - Congela la cola y usa LOOK.
    /// </summary>
    public class FLOOKAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "F-LOOK";
        public string Description => "Como F-SCAN, pero usa lógica LOOK (sin ir al extremo si no es necesario) para el lote congelado.";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0)
        {
            var result = new ExerciseResult(Name, initialPosition, requests.Select(r => r.Position).ToList());
            result.Direction = direction;
            int currentPosition = initialPosition;
            double currentTime = 0.0;
            int totalMovement = 0;
            string currentDirection = direction;

            result.AddStep($"Posición inicial: {initialPosition}, Dirección: {currentDirection}");

            var pendingQueue = SimulationHelper.CloneRequests(requests).OrderBy(r => r.ArrivalTime).ThenBy(r => r.OriginalIndex).ToList();
            var activeQueue = new List<DiskRequest>();

            while (activeQueue.Count > 0 || pendingQueue.Count > 0)
            {
                // 1. Fase de Llenado (Freeze)
                if (activeQueue.Count == 0)
                {
                    if (pendingQueue.Count > 0 && pendingQueue[0].ArrivalTime > currentTime)
                    {
                        double nextArrival = pendingQueue[0].ArrivalTime;
                        result.AddStep($"T={currentTime:F2}: Cola vacía. Esperando lote hasta T={nextArrival:F2}...");
                        currentTime = nextArrival;
                    }

                    while (pendingQueue.Count > 0 && pendingQueue[0].ArrivalTime <= currentTime)
                    {
                        activeQueue.Add(pendingQueue[0]);
                        pendingQueue.RemoveAt(0);
                    }
                    
                    if (activeQueue.Count > 0)
                    {
                        result.AddStep($"T={currentTime:F2}: Lote congelado con {activeQueue.Count} peticiones.");
                    }
                    else if (pendingQueue.Count == 0)
                    {
                        break;
                    }
                    else 
                    {
                        continue;
                    }
                }

                // 2. Procesar lote usando lógica LOOK
                bool isAscending = currentDirection == "up";
                
                var requestsInDirection = activeQueue.Where(r => 
                    isAscending ? r.Position >= currentPosition : r.Position <= currentPosition)
                    .OrderBy(r => Math.Abs(r.Position - currentPosition))
                    .ToList();

                DiskRequest targetRequest = null;
                int targetTrack = -1;

                if (requestsInDirection.Count > 0)
                {
                    targetRequest = requestsInDirection[0];
                    targetTrack = targetRequest.Position;
                }
                else
                {
                    // No hay más en esta dirección dentro del lote.
                    // Invertir dirección si hay pendientes en el lote.
                    if (activeQueue.Count > 0)
                    {
                         currentDirection = isAscending ? "down" : "up";
                         result.AddStep($"T={currentTime:F2}: No hay peticiones en dirección {direction} (en lote). Invirtiendo a {currentDirection}.");
                         continue;
                    }
                }

                int distance = Math.Abs(targetTrack - currentPosition);
                double travelTime = distance * timePerTrack;

                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = targetTrack,
                    Distance = distance,
                    Instant = currentTime,
                    ArrivalInstant = targetRequest.ArrivalTime,
                    Remaining = activeQueue.Where(r => r != targetRequest).Select(r => r.Position).ToList(),
                    Buffer = pendingQueue.Select(r => r.Position).ToList()
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime; // Viajar
                currentTime += timePerRequest; // Procesar
                totalMovement += distance;
                currentPosition = targetTrack;

                result.ProcessingOrder.Add(targetTrack);
                result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetTrack} (Dist: {distance}) [Lote]");
                
                activeQueue.Remove(targetRequest);
            }

            result.TotalHeadMovement = totalMovement;
            result.TotalTime = currentTime;
            result.CalculateMetrics();
            return result;
        }
    }
}
