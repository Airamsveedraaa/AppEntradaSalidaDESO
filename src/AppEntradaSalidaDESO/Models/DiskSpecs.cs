namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Especificaciones de geometría del disco
    /// </summary>
    public class DiskSpecs
    {
        /// <summary>
        /// Número de sectores por pista
        /// </summary>
        public int SectorsPerTrack { get; set; }

        /// <summary>
        /// Número total de cilindros (pistas)
        /// </summary>
        public int Cylinders { get; set; }

        /// <summary>
        /// Número de caras (superficies) del disco
        /// </summary>
        public int Faces { get; set; }

        /// <summary>
        /// Tamaño de un sector en bytes
        /// </summary>
        public int SectorSize { get; set; }

        /// <summary>
        /// Tamaño de un bloque en bytes
        /// </summary>
        public int BlockSize { get; set; }

        public DiskSpecs()
        {
            // Valores por defecto razonables
            SectorsPerTrack = 10;
            Cylinders = 100;
            Faces = 2;
            SectorSize = 512;
            BlockSize = 1024;
        }

        public DiskSpecs(int sectorsPerTrack, int cylinders, int faces, int sectorSize, int blockSize)
        {
            SectorsPerTrack = sectorsPerTrack;
            Cylinders = cylinders;
            Faces = faces;
            SectorSize = sectorSize;
            BlockSize = blockSize;
        }

        /// <summary>
        /// Calcula el número de bytes por pista
        /// </summary>
        public int BytesPerTrack => SectorsPerTrack * SectorSize;

        /// <summary>
        /// Calcula el número de bloques por pista
        /// </summary>
        public double BlocksPerTrack => (double)BytesPerTrack / BlockSize;

        /// <summary>
        /// Calcula el número de bloques por cilindro
        /// </summary>
        public double BlocksPerCylinder => BlocksPerTrack * Faces;

        public override string ToString()
        {
            return $"Disco: {Cylinders} cilindros, {SectorsPerTrack} sectores/pista, {Faces} caras, " +
                   $"Sector: {SectorSize}B, Bloque: {BlockSize}B";
        }
    }
}
