using AppEntradaSalidaDESO.Models;
using System.Collections.Generic;

namespace AppEntradaSalidaDESO.Algorithms
{
    /// <summary>
    /// Interfaz base para todos los algoritmos de planificación de disco
    /// </summary>
    public interface IDiskSchedulingAlgorithm
    {
        string Name { get; }
        string Description { get; }
        bool RequiresDirection { get; }
        ExerciseResult Execute(int initialPosition, List<int> requests, int minCylinder, int maxCylinder, string direction = "up");
    }
}
