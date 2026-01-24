using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// C-SCAN (Circular SCAN) - Se mueve en una dirección llegando al extremo y salta al inicio
    /// Soporta tiempos de llegada dinámicos e intercepciones
    /// </summary>
    public class CSCANAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "C-SCAN";
        public string Description => "Circular SCAN - Atiende en una dirección, al llegar al final salta al inicio sin atender.";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0, int nStep = 2)
        {
            var result = new ExerciseResult(Name, initialPosition, requests.Select(r => r.Position).ToList());
            result.Direction = direction;
            int currentPosition = initialPosition;
            double currentTime = 0.0;
            int totalMovement = 0;
            string currentDirection = direction; // Siempre mantiene la misma dirección lógica (ej: up)

            result.AddStep($"Posición inicial: {initialPosition}, Dirección: {currentDirection}");

            var pendingQueue = SimulationHelper.CloneRequests(requests).OrderBy(r => r.ArrivalTime).ThenBy(r => r.OriginalIndex).ToList();
            var activeQueue = new List<DiskRequest>();
            var processedRequests = new List<DiskRequest>();

            while (activeQueue.Count > 0 || pendingQueue.Count > 0)
            {
                // 1. Mover peticiones llegadas a active
                while (pendingQueue.Count > 0 && pendingQueue[0].ArrivalTime <= currentTime)
                {
                    activeQueue.Add(pendingQueue[0]);
                    pendingQueue.RemoveAt(0);
                }

                // 2. Esperar si es necesario
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

                // 3. Buscar peticiones en la dirección actual desde currentPosition
                // C-SCAN debe procesar en ORDEN DE POSICIÓN, no por distancia más cercana
                var requestsInDirection = activeQueue.Where(r => 
                    isAscending ? r.Position >= currentPosition : r.Position <= currentPosition)
                    .OrderBy(r => isAscending ? r.Position : -r.Position) // Orden por posición en la dirección
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
                        // Procesar immediately
                    }
                }
                else
                {
                    // No hay peticiones en esta dirección desde la posición actual.
                    // En C-SCAN, si no hay más adelante, debemos ir al LÍMITE FINAL, saltar al LÍMITE INICIAL y seguir.
                    // Pero espera, ¿hay peticiones en "la vuelta" (detrás)?
                    // Si activeQueue tiene items, y no están adelante, están atrás.
                    if (activeQueue.Count > 0)
                    {
                        targetTrack = isAscending ? maxCylinder : minCylinder;
                        goingToLimit = true;

                        // Si ya estamos en el límite final, hacemos el SALTO
                        if (currentPosition == targetTrack)
                        {
                            int startLimit = isAscending ? minCylinder : maxCylinder;
                            // Salto mágico (a veces no cuenta distancia, o cuenta distancia completa)
                            // En simuladores típicos, cuenta distancia 0 o distancia completa pero NO tiempo de servicio.
                            // Asumiremos distancia física de retorno.
                            int jumpDist = Math.Abs(targetTrack - startLimit);
                            double jumpTime = jumpDist * timePerTrack; // El tiempo pasa al volver
                            
                            var jumpStep = new AlgorithmStep
                            {
                                From = currentPosition,
                                To = startLimit,
                                Distance = jumpDist,
                                Instant = currentTime,
                                ArrivalInstant = 0,
                                Remaining = activeQueue.Select(r => r.Position).ToList()
                            };
                            result.DetailedSteps.Add(jumpStep);

                            result.AddStep($"T={currentTime:F2} -> T={(currentTime+jumpTime):F2}: Salto circular de {currentPosition} a {startLimit} (Dist: {jumpDist})");
                            
                            totalMovement += jumpDist;
                            currentTime += jumpTime;
                            currentPosition = startLimit;
                            continue; // Reevaluar desde el inicio
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // No hay lógica de intercepción - las peticiones se procesan completamente una vez seleccionadas

                // 5. Mover y Procesar
                int distance = Math.Abs(targetTrack - currentPosition);
                double travelTime = distance * timePerTrack;

                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = targetTrack,
                    Distance = distance,
                    Instant = currentTime,
                    ArrivalInstant = goingToLimit ? 0 : targetRequest.ArrivalTime,
                    Remaining = activeQueue.Where(r => r != targetRequest).Select(r => r.Position).ToList()
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime;
                totalMovement += distance;
                currentPosition = targetTrack;

                if (goingToLimit)
                {
                    // Solo llegamos al límite final, en la siguiente iteración haremos el salto
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Mover al límite {targetTrack}");
                }
                else
                {
                    currentTime += timePerRequest;
                    result.ProcessingOrder.Add(targetTrack);
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetTrack} (Dist: {distance})");

                    activeQueue.Remove(targetRequest);
                    
                    processedRequests.Add(targetRequest);
                }
            }

            result.TotalHeadMovement = totalMovement;
            result.TotalTime = currentTime;
            result.CalculateMetrics();

            return result;
        }
    }
}
