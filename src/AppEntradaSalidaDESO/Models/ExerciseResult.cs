using System.Collections.Generic;

namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Resultado de la ejecución de un algoritmo de planificación
    /// </summary>
    public class ExerciseResult
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public int InitialPosition { get; set; }
        public List<int> RequestQueue { get; set; } = new();
        public List<int> ProcessingOrder { get; set; } = new();
        public List<string> Steps { get; set; } = new();
        public List<AlgorithmStep> DetailedSteps { get; set; } = new();
        public int TotalHeadMovement { get; set; }
        public double AverageSeekTime { get; set; }
        public double TotalTime { get; set; }
        public string Direction { get; set; } = string.Empty;

        /// <summary>
        /// Resultado de los cálculos de tiempo de acceso (opcional)
        /// </summary>
        public AccessTimeResult? AccessTime { get; set; }

        public ExerciseResult()
        {
        }

        public ExerciseResult(string algorithmName, int initialPosition, List<int> requestQueue)
        {
            AlgorithmName = algorithmName;
            InitialPosition = initialPosition;
            RequestQueue = new List<int>(requestQueue);
            ProcessingOrder = new();
            Steps = new();
        }

        public void AddStep(string step)
        {
            Steps.Add(step);
        }

        public void CalculateMetrics()
        {
            if (ProcessingOrder.Count > 0)
            {
                AverageSeekTime = (double)TotalHeadMovement / ProcessingOrder.Count;
            }
        }
    }
}
