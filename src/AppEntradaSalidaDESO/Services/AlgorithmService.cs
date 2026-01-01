using AppEntradaSalidaDESO.Algorithms;
using System.Collections.Generic;

namespace AppEntradaSalidaDESO.Services
{
    /// <summary>
    /// Servicio para gestionar los algoritmos de planificación de disco
    /// </summary>
    public class AlgorithmService
    {
        private readonly Dictionary<string, IDiskSchedulingAlgorithm> _algorithms;

        public AlgorithmService()
        {
            _algorithms = new Dictionary<string, IDiskSchedulingAlgorithm>
            {
                { "FCFS", new FCFSAlgorithm() },
                { "SSTF", new SSTFAlgorithm() },
                { "SCAN", new SCANAlgorithm() },
                { "C-SCAN", new CSCANAlgorithm() },
                { "LOOK", new LOOKAlgorithm() },
                { "C-LOOK", new CLOOKAlgorithm() },
                { "F-SCAN", new FSCANAlgorithm() },
                { "F-LOOK", new FLOOKAlgorithm() },
                { "SCAN-N", new SCANNAlgorithm() },
                { "LOOK-N", new LOOKNAlgorithm() }
            };
        }

        public IDiskSchedulingAlgorithm? GetAlgorithm(string name)
        {
            return _algorithms.TryGetValue(name, out var algorithm) ? algorithm : null;
        }

        public IEnumerable<IDiskSchedulingAlgorithm> GetAllAlgorithms()
        {
            return _algorithms.Values;
        }

        public IEnumerable<string> GetAlgorithmNames()
        {
            return _algorithms.Keys;
        }
    }
}
