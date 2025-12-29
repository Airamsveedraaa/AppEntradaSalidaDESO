using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// LOOK - Similar a SCAN pero solo va hasta la última petición, no hasta el extremo
    /// </summary>
    public class LOOKAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "LOOK";
        public string Description => "LOOK - Como SCAN pero solo va hasta la última petición";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests) { Direction = direction };
            int currentPosition = initialPosition;
            int totalMovement = 0;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones: [{string.Join(", ", requests)}]");
            result.AddStep($"Dirección inicial: {(direction == "up" ? "Hacia arriba (↑)" : "Hacia abajo (↓)")}");
            result.AddStep("Algoritmo LOOK: Se mueve en una dirección hasta la última petición, luego invierte");
            result.AddStep("");

            var sortedRequests = requests.OrderBy(r => r).ToList();
            var leftRequests = sortedRequests.Where(r => r < initialPosition).OrderByDescending(r => r).ToList();
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

                // Cambiar dirección si hay peticiones pendientes
                if (leftRequests.Count > 0)
                {
                    result.AddStep($"Cambio de dirección: Ahora hacia abajo (↓)");
                    result.AddStep("");

                    // Luego hacia abajo
                    foreach (var request in leftRequests)
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
            else // direction == "down"
            {
                // Primero hacia abajo
                foreach (var request in leftRequests)
                {
                    int movement = Math.Abs(request - currentPosition);
                    totalMovement += movement;
                    result.ProcessingOrder.Add(request);
                    result.AddStep($"Paso {step}: Mover de {currentPosition} a {request} (↓)");
                    result.AddStep($"  Movimiento: {movement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = request;
                    step++;
                }

                // Cambiar dirección si hay peticiones pendientes
                if (rightRequests.Count > 0)
                {
                    result.AddStep($"Cambio de dirección: Ahora hacia arriba (↑)");
                    result.AddStep("");

                    // Luego hacia arriba
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
