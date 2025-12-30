namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Resultado de los cálculos de tiempo de acceso al disco
    /// </summary>
    public class AccessTimeResult
    {
        /// <summary>
        /// Tiempo total de búsqueda (seek time) en milisegundos
        /// </summary>
        public double SeekTimeMs { get; set; }

        /// <summary>
        /// Tiempo total de latencia rotacional en milisegundos
        /// </summary>
        public double LatencyTimeMs { get; set; }

        /// <summary>
        /// Tiempo total de transferencia en milisegundos
        /// </summary>
        public double TransferTimeMs { get; set; }

        /// <summary>
        /// Tiempo total de acceso en milisegundos
        /// </summary>
        public double TotalTimeMs { get; set; }

        public AccessTimeResult()
        {
            SeekTimeMs = 0;
            LatencyTimeMs = 0;
            TransferTimeMs = 0;
            TotalTimeMs = 0;
        }

        public AccessTimeResult(double seekTime, double latencyTime, double transferTime)
        {
            SeekTimeMs = seekTime;
            LatencyTimeMs = latencyTime;
            TransferTimeMs = transferTime;
            TotalTimeMs = seekTime + latencyTime + transferTime;
        }

        /// <summary>
        /// Calcula el tiempo total sumando los componentes
        /// </summary>
        public void CalculateTotal()
        {
            TotalTimeMs = SeekTimeMs + LatencyTimeMs + TransferTimeMs;
        }

        public override string ToString()
        {
            return $"Seek: {SeekTimeMs:F2}ms, Latencia: {LatencyTimeMs:F2}ms, " +
                   $"Transferencia: {TransferTimeMs:F2}ms, Total: {TotalTimeMs:F2}ms";
        }

        /// <summary>
        /// Formatea el resultado en un string multi-línea para visualización
        /// </summary>
        public string ToDetailedString()
        {
            return $"Tiempo de Búsqueda:      {SeekTimeMs,10:F2} ms\n" +
                   $"Tiempo de Latencia:      {LatencyTimeMs,10:F2} ms\n" +
                   $"Tiempo de Transferencia: {TransferTimeMs,10:F2} ms\n" +
                   $"───────────────────────────────────────\n" +
                   $"Tiempo Total:            {TotalTimeMs,10:F2} ms";
        }
    }
}
