using System.Collections.Generic;

namespace AppEntradaSalidaDESO.Models
{
    public class AlgorithmStep
    {
        public int From { get; set; }
        public int To { get; set; }
        public int Distance { get; set; }
        public List<int> Remaining { get; set; } = new();
        public double Instant { get; set; }
        public double ArrivalInstant { get; set; }
        public List<int> Buffer { get; set; } // Nullable, or empty if unused

        public override string ToString()
        {
            return $"T={Instant:F2}: {From} -> {To} ({Distance})";
        }
    }
}
