using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// F-SCAN (Frozen SCAN) - Congela la cola de peticiones y las atiende usando SCAN. 
    /// Las nuevas van a un buffer y se atienden después.
    /// </summary>
    public class FSCANAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "F-SCAN";
        public string Description => "Congela las peticiones actuales y usa SCAN. Las nuevas esperan hasta terminar el lote.";
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
            var activeQueue = new List<DiskRequest>(); // The Frozen Queue

            while (activeQueue.Count > 0 || pendingQueue.Count > 0)
            {
                // 1. Fase de Llenado (Freeze)
                // Solo si la cola activa está vacía, cargamos nuevas peticiones
                if (activeQueue.Count == 0)
                {
                    // Si no ha llegado nadie, esperamos
                    if (pendingQueue.Count > 0 && pendingQueue[0].ArrivalTime > currentTime)
                    {
                        double nextArrival = pendingQueue[0].ArrivalTime;
                        result.AddStep($"T={currentTime:F2}: Cola vacía. Esperando lote hasta T={nextArrival:F2}...");
                        currentTime = nextArrival;
                    }

                    // Cargar TODAS las que hayan llegado hasta currentTime
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
                        break; // Terminamos
                    }
                    else 
                    {
                        // Edge case: pending > 0 but > currentTime. handled by wait above logic loop.
                        continue;
                    }
                }

                // 2. Procesar lote usando lógica SCAN
                // BUSCAR destino en activeQueue
                bool isAscending = currentDirection == "up";
                
                var requestsInDirection = activeQueue.Where(r => 
                    isAscending ? r.Position >= currentPosition : r.Position <= currentPosition)
                    .OrderBy(r => Math.Abs(r.Position - currentPosition))
                    .ToList();

                DiskRequest targetRequest = null;
                int targetTrack = -1;
                bool goingToLimit = false;

                if (requestsInDirection.Count > 0)
                {
                    targetRequest = requestsInDirection[0];
                    targetTrack = targetRequest.Position;
                    
                    if (targetTrack == currentPosition)
                    {
                        // Procesar immediately logic
                    }
                }
                else
                {
                    // Nada en esta dirección.
                    // Si hay algo en activeQueue (que sabemos que hay porque estamos en el loop), debemos ir al LIMITE y cambiar dirección.
                    // F-SCAN usa SCAN.
                    targetTrack = isAscending ? maxCylinder : minCylinder;
                    goingToLimit = true;
                    
                    if (currentPosition == targetTrack)
                    {
                        currentDirection = isAscending ? "down" : "up";
                        result.AddStep($"T={currentTime:F2}: Llegó al límite {currentPosition}. Invirtiendo a {currentDirection}.");
                        continue;
                    }
                }

                // 3. Mover (SIN Intercepciones de pendingQueue, pues está congelada fuera)
                // Solo nos movemos al target.
                int distance = Math.Abs(targetTrack - currentPosition);
                double travelTime = distance * timePerTrack;

                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = targetTrack,
                    Distance = distance,
                    Instant = currentTime,
                    ArrivalInstant = goingToLimit ? 0 : targetRequest.ArrivalTime,
                    Remaining = activeQueue.Where(r => r != targetRequest).Select(r => r.Position).ToList(),
                    Buffer = pendingQueue.Select(r => r.Position).ToList() // Nuevo: Visualizar buffer
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime;
                totalMovement += distance;
                currentPosition = targetTrack;

                if (goingToLimit)
                {
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Mover al límite {targetTrack} (Scan Batch)");
                    currentDirection = isAscending ? "down" : "up";
                }
                else
                {
                    currentTime += timePerRequest;
                    result.ProcessingOrder.Add(targetTrack);
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetTrack} (Dist: {distance}) [Lote]");
                    
                    activeQueue.Remove(targetRequest);
                }
            }

            result.TotalHeadMovement = totalMovement;
            result.TotalTime = currentTime;
            result.CalculateMetrics();
            return result;
        }
    }
}
