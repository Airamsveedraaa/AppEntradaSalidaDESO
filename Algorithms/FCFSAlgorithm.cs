using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// First Come First Served - Atiende las peticiones en el orden en que llegan
    /// </summary>
    public class FCFSAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "FCFS";
        public string Description => "First Come First Served - Atiende las peticiones en orden de llegada";
        public bool RequiresDirection => false;

        public ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up")
        {
            var result = new ExerciseResult(Name, initialPosition, requests);
            int currentPosition = initialPosition;
            int totalMovement = 0;

            result.AddStep($"Posición inicial del cabezal: {initialPosition}");
            result.AddStep($"Cola de peticiones: [{string.Join(", ", requests)}]");
            result.AddStep("Algoritmo FCFS: Se atienden las peticiones en orden de llegada");
            result.AddStep("");

            for (int i = 0; i < requests.Count; i++)
            {
                int request = requests[i];
                int movement = Math.Abs(request - currentPosition);
                totalMovement += movement;

                result.ProcessingOrder.Add(request);
                result.AddStep($"Paso {i + 1}: Mover de {currentPosition} a {request}");
                result.AddStep($"  Movimiento: |{request} - {currentPosition}| = {movement} cilindros");
                result.AddStep($"  Movimiento acumulado: {totalMovement} cilindros");
                result.AddStep("");

                currentPosition = request;
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
