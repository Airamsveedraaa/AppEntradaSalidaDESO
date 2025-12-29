using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// C-SCAN (Circular SCAN) - Se mueve en una dirección, al llegar al final vuelve al inicio
    /// </summary>
    public class CSCANAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "C-SCAN";
        public string Description => "C-SCAN (Circular SCAN) - Recorre en una dirección, vuelve al inicio circularmente";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests) { Direction = direction };
            int currentPosition = initialPosition;
            int totalMovement = 0;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones: [{string.Join(", ", requests)}]");
            result.AddStep($"Dirección: {(direction == "up" ? "Hacia arriba (↑)" : "Hacia abajo (↓)")}");
            result.AddStep("Algoritmo C-SCAN: Se mueve en una dirección, al llegar al final vuelve al inicio");
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

                // Si hay peticiones pendientes, ir al final y volver al inicio
                if (leftRequests.Count > 0)
                {
                    int movementToEnd = maxCylinder - currentPosition;
                    totalMovement += movementToEnd;
                    result.AddStep($"Paso {step}: Mover hasta el extremo superior ({maxCylinder})");
                    result.AddStep($"  Movimiento: {movementToEnd} cilindros | Acumulado: {totalMovement}");
                    step++;

                    int returnMovement = maxCylinder - minCylinder;
                    totalMovement += returnMovement;
                    result.AddStep($"Paso {step}: Retorno circular al inicio ({minCylinder})");
                    result.AddStep($"  Movimiento: {returnMovement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = minCylinder;
                    step++;

                    result.AddStep("");

                    // Atender peticiones desde el inicio
                    foreach (var request in leftRequests)
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

                // Si hay peticiones pendientes, ir al inicio y volver al final
                if (rightRequests.Count > 0)
                {
                    int movementToStart = currentPosition - minCylinder;
                    totalMovement += movementToStart;
                    result.AddStep($"Paso {step}: Mover hasta el extremo inferior ({minCylinder})");
                    result.AddStep($"  Movimiento: {movementToStart} cilindros | Acumulado: {totalMovement}");
                    step++;

                    int returnMovement = maxCylinder - minCylinder;
                    totalMovement += returnMovement;
                    result.AddStep($"Paso {step}: Retorno circular al final ({maxCylinder})");
                    result.AddStep($"  Movimiento: {returnMovement} cilindros | Acumulado: {totalMovement}");
                    currentPosition = maxCylinder;
                    step++;

                    result.AddStep("");

                    // Atender peticiones desde el final
                    foreach (var request in rightRequests.OrderByDescending(r => r))
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
