using System;
using System.Collections.Generic;
using System.Linq;
using AppEntradaSalidaDESO.Models;

namespace AppEntradaSalidaDESO.Algorithms
{
    public class FLOOKAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "F-LOOK";
        public string Description => "F-LOOK - Variante de LOOK que 'congela' la cola de peticiones actual";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests) { Direction = direction };
            int currentPosition = initialPosition;
            int totalMovement = 0;
            int step = 1;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones (Congelada): [{string.Join(", ", requests)}]");
            result.AddStep($"Dirección inicial: {(direction == "up" ? "Arriba (Incrementando)" : "Abajo (Decrementando)")}");

            // Ordenamos las peticiones para facilitar el barrido
            // LOOK no va hasta los extremos (minCylinder/maxCylinder), solo hasta la última petición
            var rightRequests = requests.Where(r => r >= currentPosition).OrderBy(r => r).ToList();
            var leftRequests = requests.Where(r => r < currentPosition).OrderByDescending(r => r).ToList();

            if (direction == "up")
            {
                // Procesar hacia arriba (derecha)
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

                // Si hay peticiones a la izquierda, cambiamos de dirección
                if (leftRequests.Count > 0)
                {
                    result.AddStep($"Cambio de dirección: Ahora hacia abajo (↓) desde la última petición atendida");
                    
                    // Procesar hacia abajo (izquierda)
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
                // Procesar hacia abajo (izquierda)
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

                // Si hay peticiones a la derecha, cambiamos de dirección
                if (rightRequests.Count > 0)
                {
                    result.AddStep($"Cambio de dirección: Ahora hacia arriba (↑) desde la última petición atendida");

                    // Procesar hacia arriba (derecha)
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
