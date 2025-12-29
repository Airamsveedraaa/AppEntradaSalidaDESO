using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// C-LOOK - Similar a C-SCAN pero solo va hasta la última petición, no hasta el extremo
    /// </summary>
    public class CLOOKAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "C-LOOK";
        public string Description => "C-LOOK - Como C-SCAN pero solo va hasta la última petición";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests) { Direction = direction };
            int currentPosition = initialPosition;
            int totalMovement = 0;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones: [{string.Join(", ", requests)}]");
            result.AddStep($"Dirección: {(direction == "up" ? "Hacia arriba (↑)" : "Hacia abajo (↓)")}");
            result.AddStep("Algoritmo C-LOOK: Se mueve en una dirección, vuelve circularmente a la primera petición");
            result.AddStep("");

            var sortedRequests = requests.OrderBy(r => r).ToList();
            var leftRequests = sortedRequests.Where(r => r < initialPosition).OrderBy(r => r).ToList();
            var rightRequests = sortedRequests.Where(r => r >= initialPosition).OrderBy(r => r).ToList();

            int step = 1;

            if (direction == "up")
            {
                // Primero hacia arriba
                foreach (var request in rightRequests)
                {
                    int movement = Math.Abs(request - currentPosition);
                    totalMovement += movement;
                    result.ProcessingOrder.Add(request);
                    result.AddStep($"Paso {step}: Mover de {currentPosition} a {request} (↑)");
                    result.AddStep($"  Movimiento: {movement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = request;
                    step++;
                }

                // Si hay peticiones pendientes, volver circularmente
                if (leftRequests.Count > 0)
                {
                    int firstRequest = leftRequests.First();
                    int returnMovement = Math.Abs(currentPosition - firstRequest);
                    totalMovement += returnMovement;
                    result.AddStep($"Paso {step}: Retorno circular a {firstRequest}");
                    result.AddStep($"  Movimiento: {returnMovement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = firstRequest;
                    result.ProcessingOrder.Add(firstRequest);
                    step++;

                    result.AddStep("");

                    // Atender el resto de peticiones
                    foreach (var request in leftRequests.Skip(1))
                    {
                        int movement = Math.Abs(request - currentPosition);
                        totalMovement += movement;
                        result.ProcessingOrder.Add(request);
                        result.AddStep($"Paso {step}: Mover de {currentPosition} a {request} (↑)");
                        result.AddStep($"  Movimiento: {movement} cilindros | Acumulado: {totalMovement}");
                        currentPosition = request;
                        step++;
                    }
                }
            }
            else // direction == "down"
            {
                // Primero hacia abajo
                foreach (var request in leftRequests.OrderByDescending(r => r))
                {
                    int movement = Math.Abs(request - currentPosition);
                    totalMovement += movement;
                    result.ProcessingOrder.Add(request);
                    result.AddStep($"Paso {step}: Mover de {currentPosition} a {request} (↓)");
                    result.AddStep($"  Movimiento: {movement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = request;
                    step++;
                }

                // Si hay peticiones pendientes, volver circularmente
                if (rightRequests.Count > 0)
                {
                    int lastRequest = rightRequests.Last();
                    int returnMovement = Math.Abs(currentPosition - lastRequest);
                    totalMovement += returnMovement;
                    result.AddStep($"Paso {step}: Retorno circular a {lastRequest}");
                    result.AddStep($"  Movimiento: {returnMovement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = lastRequest;
                    result.ProcessingOrder.Add(lastRequest);
                    step++;

                    result.AddStep("");

                    // Atender el resto de peticiones
                    foreach (var request in rightRequests.Take(rightRequests.Count - 1).OrderByDescending(r => r))
                    {
                        int movement = Math.Abs(request - currentPosition);
                        totalMovement += movement;
                        result.ProcessingOrder.Add(request);
                        result.AddStep($"Paso {step}: Mover de {currentPosition} a {request} (↓)");
                        result.AddStep($"  Movimiento: {movement} cilindros | Acumulado: {totalMovement}");
                        currentPosition = request;
                        step++;
                    }
                }
            }

            result.TotalHeadMovement = totalMovement;
            result.CalculateMetrics();

            result.AddStep("");
            result.AddStep("=== RESUMEN ===");
            result.AddStep($"Orden de atención: [{string.Join(" → ", result.ProcessingOrder)}]");
            result.AddStep($"Movimiento total del cabezal: {totalMovement} cilindros");
            result.AddStep($"Tiempo promedio de búsqueda: {result.AverageSeekTime:F2} cilindros");

            return result;
        }
    }
}
