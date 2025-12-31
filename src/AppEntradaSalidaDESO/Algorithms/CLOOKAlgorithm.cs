using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// C-LOOK - Circular LOOK. Atiende en una dirección, salta a la primera petición sin ir al extremo.
    /// Soporta tiempos de llegada dinámicos e intercepciones
    /// </summary>
    public class CLOOKAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "C-LOOK";
        public string Description => "Como C-SCAN, pero solo salta hasta la primera petición del otro extremo (no hasta el límite físico).";
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
                var requestsInDirection = activeQueue.Where(r => 
                    isAscending ? r.Position >= currentPosition : r.Position <= currentPosition)
                    .OrderBy(r => Math.Abs(r.Position - currentPosition))
                    .ToList();

                DiskRequest targetRequest = null;
                int targetTrack = -1;
                bool isJumping = false;

                if (requestsInDirection.Count > 0)
                {
                    targetRequest = requestsInDirection[0];
                    targetTrack = targetRequest.Position;
                }
                else
                {
                    // No hay peticiones adelante. C-LOOK salta al "inicio" lógico de las peticiones (no al límite físico).
                    // Buscamos la petición más extrema opuesta.
                    if (activeQueue.Count > 0)
                    {
                        // Si vamos UP, saltamos al MINIMO request.
                        // Si vamos DOWN, saltamos al MAXIMO request.
                        targetRequest = isAscending 
                            ? activeQueue.OrderBy(r => r.Position).First()
                            : activeQueue.OrderByDescending(r => r.Position).First();
                        
                        targetTrack = targetRequest.Position;
                        isJumping = true; // Es un salto sin procesar
                    }
                    else
                    {
                        break;
                    }
                }

                // 4. Verificar Intercepciones (SOLO si NO estamos saltando)
                // En saltos C-SCAN/C-LOOK típicamente no se atiende nada.
                DiskRequest interceptReq = null;
                if (!isJumping) 
                {
                    var intercept = SimulationHelper.FindEarliestIntercept(
                        currentPosition,
                        targetTrack,
                        currentTime,
                        timePerTrack,
                        pendingQueue,
                        currentDirection);
                    
                    if (intercept != null)
                    {
                        interceptReq = intercept.Request;
                        targetRequest = interceptReq;
                        targetTrack = targetRequest.Position;
                        // isJumping permanece false
                    }
                }
                else
                {
                    // Si saltamos, ¿podemos interceptar algo?
                    // "without servicing any requests on the return trip" -> NO.
                    // Simplemente vamos al target.
                    // PERO, debemos asegurarnos que "targetTrack" es el destino final del salto.
                    // Y el salto cuenta distancia.
                }

                // 5. Mover
                int distance = Math.Abs(targetTrack - currentPosition);
                // NOTA: En C-LOOK el salto cuenta distancia de movimiento total?
                // Generalmente sí.
                double travelTime = distance * timePerTrack;

                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = targetTrack,
                    Distance = distance,
                    Instant = currentTime,
                    ArrivalInstant = targetRequest.ArrivalTime,
                    Remaining = activeQueue.Select(r => r.Position).ToList() // simplified
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime;
                totalMovement += distance;
                currentPosition = targetTrack;

                if (isJumping)
                {
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Salto (sin atender) a {targetTrack} (Dist: {distance})");
                    // Después del salto, estamos EN la petición 'start', LISTOS para procesarla en la SIGUIENTE iteración (como 'closest' en direction)
                    // ¿O debemos procesarla YA?
                    // C-SCAN salta al 0 y luego procesa.
                    // C-LOOK salta al Request X.
                    // Típicamente el salto coloca el cabezal allí. La siguiente iteración la procesa pq ahora Distance=0.
                    // Así que CONTINUE.
                    continue;
                }
                else
                {
                    // Procesar
                    currentTime += timePerRequest;
                    result.ProcessingOrder.Add(targetTrack);
                    result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetTrack} (Dist: {distance}){(interceptReq != null ? " [INTERCEPT]" : "")}");

                    if (interceptReq != null) pendingQueue.Remove(targetRequest);
                    else activeQueue.Remove(targetRequest);
                    
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
