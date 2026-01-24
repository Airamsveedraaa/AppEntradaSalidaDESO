using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// LOOK - Similar a SCAN pero solo llega hasta la última petición (no al extremo)
    /// </summary>
    public class LOOKAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "LOOK";
        public string Description => "Como SCAN, pero el cabezal solo llega hasta la última petición en esa dirección.";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0, int nStep = 2)
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
            var processedRequests = new List<DiskRequest>();

            while (activeQueue.Count > 0 || pendingQueue.Count > 0)
            {
                // 1. Mover peticiones
                while (pendingQueue.Count > 0 && pendingQueue[0].ArrivalTime <= currentTime)
                {
                    activeQueue.Add(pendingQueue[0]);
                    pendingQueue.RemoveAt(0);
                }

                // 2. Esperar
                if (activeQueue.Count == 0 && pendingQueue.Count > 0)
                {
                    double nextArrival = pendingQueue[0].ArrivalTime;
                    if (currentTime < nextArrival)
                    {
                        result.AddStep($"T={currentTime:F2}: Esperando hasta T={nextArrival:F2}...");
                        currentTime = nextArrival;
                        continue;
                    }
                }

                if (activeQueue.Count == 0) break;

                bool isAscending = currentDirection == "up";

                // 3. Buscar peticiones en dirección actual
                // LOOK debe procesar en ORDEN DE POSICIÓN, no por distancia más cercana
                var requestsInDirection = activeQueue.Where(r => 
                    isAscending ? r.Position >= currentPosition : r.Position <= currentPosition)
                    .OrderBy(r => isAscending ? r.Position : -r.Position) // Orden por posición en la dirección
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
                    // No hay peticiones en esta dirección.
                    // En LOOK, si no hay más adelante, invertimos dirección INMEDIATAMENTE.
                    // No vamos al límite.
                    if (activeQueue.Count > 0)
                    {
                        currentDirection = isAscending ? "down" : "up";
                        result.AddStep($"T={currentTime:F2}: No hay peticiones en dirección {direction}. Invirtiendo a {currentDirection}.");
                        continue; // Reevaluar
                    }
                    else
                    {
                        break;
                    }
                }

                // 4. Verificar Intercepciones
                var intercept = SimulationHelper.FindEarliestIntercept(
                    currentPosition,
                    targetTrack,
                    currentTime,
                    timePerTrack,
                    pendingQueue,
                    currentDirection);

                bool isIntercepted = false;
                if (intercept != null)
                {
                    targetRequest = intercept.Request;
                    targetTrack = targetRequest.Position;
                    isIntercepted = true;
                }

                // 5. Mover y Procesar
                int distance = Math.Abs(targetTrack - currentPosition);
                double travelTime = distance * timePerTrack;

                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = targetTrack,
                    Distance = distance,
                    Instant = currentTime,
                    ArrivalInstant = targetRequest.ArrivalTime,
                    Remaining = activeQueue.Where(r => r != targetRequest).Select(r => r.Position).ToList()
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime; // Viajar
                currentTime += timePerRequest; // Procesar
                totalMovement += distance;
                currentPosition = targetTrack;

                result.ProcessingOrder.Add(targetTrack);
                result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetTrack} (Dist: {distance}){(isIntercepted ? " [INTERCEPT]" : "")}");

                if (isIntercepted) pendingQueue.Remove(targetRequest);
                else activeQueue.Remove(targetRequest);
                
                processedRequests.Add(targetRequest);
            }

            result.TotalHeadMovement = totalMovement;
            result.TotalTime = currentTime;
            result.CalculateMetrics();

            return result;
        }
    }
}
