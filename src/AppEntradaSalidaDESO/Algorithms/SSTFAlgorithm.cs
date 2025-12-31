using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// Shortest Seek Time First - Atiende la petición más cercana a la posición actual
    /// Soporta tiempos de llegada dinámicos e intercepciones
    /// </summary>
    public class SSTFAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "SSTF";
        public string Description => "Shortest Seek Time First - Atiende la petición más cercana";
        public bool RequiresDirection => false;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0)
        {
            var result = new ExerciseResult(Name, initialPosition, requests.Select(r => r.Position).ToList());
            result.Direction = direction;
            int currentPosition = initialPosition;
            double currentTime = 0.0;
            int totalMovement = 0;

            result.AddStep($"Posición inicial: {initialPosition}");

            // Colas de trabajo
            var pendingQueue = SimulationHelper.CloneRequests(requests).OrderBy(r => r.ArrivalTime).ThenBy(r => r.OriginalIndex).ToList();
            var activeQueue = new List<DiskRequest>();
            var processedRequests = new List<DiskRequest>();

            while (activeQueue.Count > 0 || pendingQueue.Count > 0)
            {
                // 1. Mover peticiones que ya llegaron a la cola activa
                while (pendingQueue.Count > 0 && pendingQueue[0].ArrivalTime <= currentTime)
                {
                    activeQueue.Add(pendingQueue[0]);
                    pendingQueue.RemoveAt(0);
                }

                // 2. Si no hay activas pero sí pendientes, esperar (avanzar tiempo)
                if (activeQueue.Count == 0 && pendingQueue.Count > 0)
                {
                    double nextArrival = pendingQueue[0].ArrivalTime;
                    if (currentTime < nextArrival)
                    {
                        result.AddStep($"T={currentTime:F2}: Esperando (Idle) hasta T={nextArrival:F2}...");
                        currentTime = nextArrival;
                        continue; // Volver al inicio del ciclo para mover a activeQueue
                    }
                }

                if (activeQueue.Count == 0) break;

                // 3. Encontrar la más cercana en activeQueue
                DiskRequest targetRequest = activeQueue[0];
                int minDistance = Math.Abs(targetRequest.Position - currentPosition);

                foreach (var req in activeQueue)
                {
                    int dist = Math.Abs(req.Position - currentPosition);
                    // En caso de empate, mantenemos el primero encontrado (FIFO para empates) o podríamos usar lógica de dirección
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        targetRequest = req;
                    }
                }

                int targetPos = targetRequest.Position;
                string moveDirection = targetPos > currentPosition ? "up" : "down";
                
                // 4. Verificar Intercepciones (Solo si hay peticiones pendientes que podrían llegar mientras nos movemos)
                // Usamos una lógica de "intercepción" para ser más eficientes
                var intercept = SimulationHelper.FindEarliestIntercept(
                    currentPosition, 
                    targetPos, 
                    currentTime, 
                    timePerTrack, 
                    pendingQueue, 
                    moveDirection);

                DiskRequest finalRequest = targetRequest;
                bool isIntercepted = false;

                if (intercept != null)
                {
                    // ¡Intercepción! Cambiamos destino
                    finalRequest = intercept.Request;
                    targetPos = finalRequest.Position;
                    isIntercepted = true;
                    // Nota: No actualizamos minDistance aquí, calculamos de nuevo abajo
                }

                // 5. Mover y procesar
                int distance = Math.Abs(targetPos - currentPosition);
                double travelTime = distance * timePerTrack;

                var step = new AlgorithmStep
                {
                    From = currentPosition,
                    To = targetPos,
                    Distance = distance,
                    Instant = currentTime,
                    ArrivalInstant = finalRequest.ArrivalTime,
                    Remaining = activeQueue.Where(r => r != finalRequest).Select(r => r.Position).ToList() // Aproximado
                };
                result.DetailedSteps.Add(step);

                currentTime += travelTime; // Viajar
                currentTime += timePerRequest; // Procesar
                totalMovement += distance;
                currentPosition = targetPos;

                result.ProcessingOrder.Add(targetPos);
                string interceptNote = isIntercepted ? " [INTERCEPTADA]" : "";
                result.AddStep($"T={step.Instant:F2} -> T={currentTime:F2}: Atender {targetPos} (Dist: {distance}){interceptNote}");

                // 6. Eliminar petición procesada
                if (isIntercepted)
                {
                    pendingQueue.Remove(finalRequest);
                }
                else
                {
                    activeQueue.Remove(finalRequest);
                }
                processedRequests.Add(finalRequest);
            }

            result.TotalHeadMovement = totalMovement;
            result.TotalTime = currentTime;
            result.CalculateMetrics();

            return result;
        }
    }
}
