using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// Shortest Seek Time First - Atiende primero la petición más cercana
    /// </summary>
    public class SSTFAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "SSTF";
        public string Description => "Shortest Seek Time First - Atiende primero la petición más cercana";
        public bool RequiresDirection => false;

        public ExerciseResult Execute(int initialPosition, List<int> requests, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests);
            int currentPosition = initialPosition;
            int totalMovement = 0;
            var remainingRequests = new List<int>(requests);

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones: [{string.Join(", ", requests)}]");
            result.AddStep("Algoritmo SSTF: Se atiende siempre la petición más cercana");
            result.AddStep("");

            int step = 1;
            while (remainingRequests.Count > 0)
            {
                // Encontrar la petición más cercana
                int closestRequest = remainingRequests
                    .OrderBy(r => Math.Abs(r - currentPosition))
                    .First();

                int movement = Math.Abs(closestRequest - currentPosition);
                totalMovement += movement;

                result.ProcessingOrder.Add(closestRequest);
                result.AddStep($"Paso {step}: Peticiones pendientes: [{string.Join(", ", remainingRequests)}]");
                result.AddStep($"  Posición actual: {currentPosition}");
                result.AddStep($"  Petición más cercana: {closestRequest}");
                result.AddStep($"  Movimiento: |{closestRequest} - {currentPosition}| = {movement} cilindros");
                result.AddStep($"  Movimiento acumulado: {totalMovement} cilindros");
                result.AddStep("");

                currentPosition = closestRequest;
                remainingRequests.Remove(closestRequest);
                step++;
            }

            result.TotalHeadMovement = totalMovement;
            result.CalculateMetrics();

            result.AddStep("=== RESUMEN ===");
            result.AddStep($"Orden de atención: [{string.Join(" → ", result.ProcessingOrder)}]");
            result.AddStep($"Movimiento total del cabezal: {totalMovement} cilindros");
            result.AddStep($"Tiempo promedio de búsqueda: {result.AverageSeekTime:F2} cilindros");

            return result;
        }
    }
}
