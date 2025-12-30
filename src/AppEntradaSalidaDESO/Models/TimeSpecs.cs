namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Especificaciones de tiempo para cálculos de acceso al disco
    /// </summary>
    public class TimeSpecs
    {
        /// <summary>
        /// Tiempo de búsqueda por pista en milisegundos
        /// </summary>
        public double SeekTimePerTrack { get; set; }

        /// <summary>
        /// Revoluciones por minuto del disco
        /// </summary>
        public int RPM { get; set; }

        /// <summary>
        /// Número de sectores por bloque (para cálculo de transferencia)
        /// </summary>
        public int SectorsPerBlock { get; set; }

        public TimeSpecs()
        {
            // Valores por defecto razonables
            SeekTimePerTrack = 1.0; // 1 ms por pista
            RPM = 7200; // 7200 RPM (disco típico)
            SectorsPerBlock = 2; // 2 sectores por bloque (si bloque = 1KB y sector = 512B)
        }

        public TimeSpecs(double seekTimePerTrack, int rpm, int sectorsPerBlock)
        {
            SeekTimePerTrack = seekTimePerTrack;
            RPM = rpm;
            SectorsPerBlock = sectorsPerBlock;
        }

        /// <summary>
        /// Calcula el tiempo de una rotación completa en milisegundos
        /// </summary>
        public double RotationTimeMs => (60.0 * 1000.0) / RPM;

        /// <summary>
        /// Calcula el tiempo de latencia promedio (media vuelta) en milisegundos
        /// </summary>
        public double AverageLatencyMs => RotationTimeMs / 2.0;

        public override string ToString()
        {
            return $"Seek: {SeekTimePerTrack}ms/pista, RPM: {RPM}, Sectores/bloque: {SectorsPerBlock}";
        }
    }
}
