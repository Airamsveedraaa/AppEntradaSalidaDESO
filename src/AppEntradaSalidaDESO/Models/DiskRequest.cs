namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Representa una petición de E/S al disco
    /// </summary>
    public class DiskRequest
    {
        public int Position { get; set; }
        public int Order { get; set; }
        public bool IsProcessed { get; set; }
        
        /// <summary>
        /// Tiempo de llegada de la petición (puede ser decimal)
        /// </summary>
        public double ArrivalTime { get; set; }

        /// <summary>
        /// Índice original en la lista de entrada (para referencias)
        /// </summary>
        public int OriginalIndex { get; set; }

        public DiskRequest(int position, int order, double arrivalTime = 0.0)
        {
            Position = position;
            Order = order;
            IsProcessed = false;
            ArrivalTime = arrivalTime;
            OriginalIndex = order; // Por defecto es el mismo que el orden inicial
        }

        public override string ToString()
        {
            return $"Petición #{Order}: Posición {Position}";
        }
    }
}
