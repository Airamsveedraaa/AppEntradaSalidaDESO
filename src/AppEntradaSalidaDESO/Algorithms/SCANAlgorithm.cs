using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// SCAN (Elevator) - Se mueve de un extremo al otro atendiendo peticiones
    /// Soporta tiempos de llegada dinámicos e intercepciones
    /// </summary>
    public class SCANAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "SCAN";
        public string Description => "El cabezal se mueve de un extremo al otro atendiendo peticiones.";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0, int nStep = 2)
        {
            var result = new ExerciseResult(Name, initialPosition, requests.Select(r => r.Position).ToList());
            result.Direction = direction;
            int currentPosition = initialPosition;
            double currentTime = 0.0;
            int totalMovement = 0;
            string currentDirection = direction; // "up" or "down"

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

                // 3. Buscar peticiones en la dirección actual
                var requestsInDirection = activeQueue.Where(r => 
                    isAscending ? r.Position >= currentPosition : r.Position <= currentPosition)
                    .OrderBy(r => Math.Abs(r.Position - currentPosition))
                    .ToList();

                // Caso especial: SCAN debe ir al extremo si va a cambiar de dirección
                // Si NO hay peticiones en la dirección actual, pero SÍ hay en la otra, debemos ir al extremo
                DiskRequest targetRequest = null;
                int targetTrack = -1;
                bool goingToLimit = false;

                if (requestsInDirection.Count > 0)
                {
                    // Hay peticiones en el camino, vamos a la más cercana
                    targetRequest = requestsInDirection[0];
                    targetTrack = targetRequest.Position;
                    
                    // Si estamos exactamente en la posición y es la misma, la procesamos (ej: llegamos a 50 y hay request en 50)
                    if (targetTrack == currentPosition)
                    {
                        // Procesar inmediatamente (no movement)
                        // ... logic below for processing
                    }
                }
                else
                {
                    // No hay peticiones en esta dirección. Verificar si hay en la otra.
                    if (activeQueue.Count > 0)
                    {
                        // Hay peticiones pendientes en el otro lado.
                        // SCAN dice: continúa hasta el extremo, luego invierte.
                        targetTrack = isAscending ? maxCylinder : minCylinder;
                        goingToLimit = true;
                        
                        // Si ya estamos en el límite, invertimos inmediatamente
                        if (currentPosition == targetTrack)
                        {
                            currentDirection = isAscending ? "down" : "up";
                            result.AddStep($"T={currentTime:F2}: Llegó al límite {currentPosition}. Invirtiendo dirección a {currentDirection}.");
                            continue; // Reevaluar en nueva dirección
                        }
                    }
                    else
                    {
                        // No hay peticiones en ninguna dirección (pero activeQueue > 0?? Impossible por check arriba)
                        break;
                    }
                }

                // 4. Verificar Intercepciones en el camino al target
                var intercept = SimulationHelper.FindEarliestIntercept(
                    currentPosition,
                    targetTrack, // Nota: si vamos al límite, targetTrack es el límite
                    currentTime,
                    timePerTrack,
                    pendingQueue,
                    currentDirection);

                bool isIntercepted = false;
                if (intercept != null)
                {
                    // Encontramos una nueva petición en el camino!
                    targetRequest = intercept.Request;
                    targetTrack = targetRequest.Position;
                    isIntercepted = true;
                    goingToLimit = false; // Ya no vamos directo al límite, paramos en la intercepción
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
                    ArrivalInstant = isIntercepted ? targetRequest.ArrivalTime : (goingToLimit ? 0 : targetRequest.ArrivalTime), // 0 o dummy si es límite
                    Remaining = activeQueue.Where(r => r != targetRequest).Select(r => r.Position).ToList()
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime;
                totalMovement += distance;
                currentPosition = targetTrack;

                if (goingToLimit && !isIntercepted)
                {
                    // Llegamos al límite (pivot), no es una petición real
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Mover al límite {targetTrack} (Dist: {distance})");
                    // Una vez en el límite, invertimos dirección en la siguiente iteración
                    currentDirection = isAscending ? "down" : "up";
                    result.AddStep($"T={currentTime:F2}: Invirtiendo dirección a {currentDirection}.");
                }
                else
                {
                    // Es una petición (normal o interceptada)
                    currentTime += timePerRequest;
                    result.ProcessingOrder.Add(targetTrack);
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetTrack} (Dist: {distance}){(isIntercepted ? " [INTERCEPT]" : "")}");

                    if (isIntercepted)
                    {
                        pendingQueue.Remove(targetRequest);
                    }
                    else
                    {
                        activeQueue.Remove(targetRequest);
                    }
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
