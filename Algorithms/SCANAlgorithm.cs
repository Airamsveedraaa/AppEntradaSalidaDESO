using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// SCAN (Elevador) - Se mueve en una dirección atendiendo peticiones hasta el final, luego invierte
    /// </summary>
    public class SCANAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "SCAN";
        public string Description => "SCAN (Elevador) - Recorre en una dirección hasta el final, luego invierte";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<int> requests, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests) { Direction = direction };
            int currentPosition = initialPosition;
            int totalMovement = 0;
            const int MAX_CYLINDER = 199;
            const int MIN_CYLINDER = 0;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones: [{string.Join(", ", requests)}]");
            result.AddStep($"Dirección inicial: {(direction == "up" ? "Hacia arriba (↑)" : "Hacia abajo (↓)")}");
            result.AddStep("Algoritmo SCAN: Se mueve en una dirección hasta el extremo, luego invierte");
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

                // Ir hasta el final si hay peticiones pendientes
                if (leftRequests.Count > 0)
                {
                    int movementToEnd = MAX_CYLINDER - currentPosition;
                    totalMovement += movementToEnd;
                    result.AddStep($"Paso {step}: Mover hasta el extremo superior ({MAX_CYLINDER})");
                    result.AddStep($"  Movimiento: {movementToEnd} cilindros | Acumulado: {totalMovement}");
                    currentPosition = MAX_CYLINDER;
                    step++;

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

                // Ir hasta el inicio si hay peticiones pendientes
                if (rightRequests.Count > 0)
                {
                    int movementToStart = currentPosition - MIN_CYLINDER;
                    totalMovement += movementToStart;
                    result.AddStep($"Paso {step}: Mover hasta el extremo inferior ({MIN_CYLINDER})");
                    result.AddStep($"  Movimiento: {movementToStart} cilindros | Acumulado: {totalMovement}");
                    currentPosition = MIN_CYLINDER;
                    step++;

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
