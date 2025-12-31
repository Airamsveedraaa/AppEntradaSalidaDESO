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
        ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction = "up", double timePerTrack = 1.0, double timePerRequest = 0.0);
    }
}
