using System;
using System.Collections.Generic;
using System.Linq;
using AppEntradaSalidaDESO.Models;

namespace AppEntradaSalidaDESO.Algorithms
{
    public class FSCANAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "F-SCAN";
        public string Description => "F-SCAN - Utiliza dos colas para congelar peticiones entrantes mientras atiende las actuales (en simulación estática equivale a SCAN)";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up")
        {
            // En una simulación estática (todas las peticiones disponibles t=0), 
            // F-SCAN congela todas las peticiones disponibles y las procesa como SCAN.
            // Por lo tanto, reutilizamos la lógica de SCAN pero mantenemos la clase separada para distinción conceptual.
            
            var result = new ExerciseResult(Name, initialPosition, requests) { Direction = direction };
            int currentPosition = initialPosition;
            int totalMovement = 0;
            int step = 1;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones (Congelada): [{string.Join(", ", requests)}]");
            result.AddStep($"Dirección inicial: {(direction == "up" ? "Arriba (Incrementando)" : "Abajo (Decrementando)")}");

            // Separar peticiones
            var rightRequests = requests.Where(r => r >= currentPosition).OrderBy(r => r).ToList();
            var leftRequests = requests.Where(r => r < currentPosition).OrderByDescending(r => r).ToList();

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

                // Ir hasta el final si hay peticiones pendientes en el otro lado
                if (leftRequests.Count > 0)
                {
                    int movementToEnd = maxCylinder - currentPosition;
                    totalMovement += movementToEnd;
                    result.AddStep($"Paso {step}: Mover hasta el extremo superior ({maxCylinder})");
                    result.AddStep($"  Movimiento: {movementToEnd} cilindros | Acumulado: {totalMovement}");
                    currentPosition = maxCylinder;
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

                // Ir hasta el inicio si hay peticiones pendientes en el otro lado
                if (rightRequests.Count > 0)
                {
                    int movementToStart = currentPosition - minCylinder;
                    totalMovement += movementToStart;
                    result.AddStep($"Paso {step}: Mover hasta el extremo inferior ({minCylinder})");
                    result.AddStep($"  Movimiento: {movementToStart} cilindros | Acumulado: {totalMovement}");
                    currentPosition = minCylinder;
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
            result.AddStep($"Movimiento total de cabezales: {totalMovement}");
            result.AddStep($"Orden de procesamiento: {string.Join(" -> ", result.ProcessingOrder)}");

            return result;
        }
    }
}
