using AppEntradaSalidaDESO.Models;
using System;

namespace AppEntradaSalidaDESO.Services
{
    /// <summary>
    /// Servicio para cálculos relacionados con geometría del disco y tiempos de acceso
    /// </summary>
    public class DiskCalculationService
    {
        /// <summary>
        /// Calcula el número de bloques por cilindro basado en las especificaciones del disco
        /// </summary>
        /// <param name="specs">Especificaciones del disco</param>
        /// <returns>Número de bloques por cilindro</returns>
        public double CalculateBlocksPerCylinder(DiskSpecs specs)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));

            if (specs.BlockSize <= 0)
                throw new ArgumentException("El tamaño de bloque debe ser mayor que 0", nameof(specs));

            return specs.BlocksPerCylinder;
        }

        /// <summary>
        /// Convierte un número de bloque a número de pista (cilindro)
        /// </summary>
        /// <param name="blockNumber">Número de bloque a convertir</param>
        /// <param name="specs">Especificaciones del disco</param>
        /// <returns>Número de pista correspondiente</returns>
        public int BlockToTrack(int blockNumber, DiskSpecs specs)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));

            if (blockNumber < 0)
                throw new ArgumentException("El número de bloque no puede ser negativo", nameof(blockNumber));

            double blocksPerCylinder = CalculateBlocksPerCylinder(specs);
            
            if (blocksPerCylinder <= 0)
                throw new InvalidOperationException("Los bloques por cilindro deben ser mayor que 0");

            // Fórmula: track = floor(block / blocksPerCylinder)
            return (int)Math.Floor(blockNumber / blocksPerCylinder);
        }

        /// <summary>
        /// Convierte un número de bloque a una tupla (Cilindro, Cara, Sector)
        /// </summary>
        /// <param name="blockNumber">Número de bloque a convertir</param>
        /// <param name="specs">Especificaciones del disco</param>
        /// <returns>Tupla (Cilindro, Cara, Sector)</returns>
        public (int Cylinder, int Head, int Sector) BlockToCHS(int blockNumber, DiskSpecs specs)
        {
            if (specs == null) throw new ArgumentNullException(nameof(specs));
            if (blockNumber < 0) throw new ArgumentException("El número de bloque no puede ser negativo", nameof(blockNumber));

            // Primero encontramos el bloque físico absoluto (considerando sectores por bloque)
            // Asumimos bloque lógico = bloque físico * sectores por bloque si blockNumber es "bloque lógico de sistema de archivos"
            // Pero usualmente en estos ejercicios blockNumber se trata como direccionamiento lineal de sectores o bloques fijos.
            // Dado el contexto de "Conversor Bloque -> Pista", asumiremos que el input es un índice lineal de bloque 
            // y que cada bloque ocupa 'specs.BlockSize' bytes.
            
            // Si el bloque no está alineado con sectores, esto puede ser complejo.
            // Simplificación: Asumimos BlockSize es múltiplo de SectorSize.
            int sectorsPerBlock = CalculateSectorsPerBlock(specs);
            
            // El sector de inicio absoluto
            long startSector = (long)blockNumber * sectorsPerBlock;

            // Geometría CHS Standard:
            // LBA = (C × HPC + H) × SPT + (S - 1)
            // Donde:
            // C = Cilindro, H = Cara (Head), S = Sector (1-based), 
            // HPC = Heads Per Cylinder (Caras), SPT = Sectors Per Track
            
            // Inversamente:
            // S = (LBA % SPT) + 1
            // Temp = LBA / SPT
            // H = Temp % HPC
            // C = Temp / HPC

            long lba = startSector;
            int spt = specs.SectorsPerTrack;
            int hpc = specs.Faces; // Caras

            if (spt <= 0 || hpc <= 0) return (0, 0, 0);

            // Cálculo
            int sector = (int)(lba % spt) + 1; // Sector es 1-based tradicionalmente
            long temp = lba / spt;
            int head = (int)(temp % hpc);
            int cylinder = (int)(temp / hpc);

            return (cylinder, head, sector);
        }

        /// <summary>
        /// Convierte una lista de números de bloque a números de pista
        /// </summary>
        /// <param name="blockNumbers">Lista de números de bloque</param>
        /// <param name="specs">Especificaciones del disco</param>
        /// <returns>Lista de números de pista correspondientes</returns>
        public System.Collections.Generic.List<int> BlocksToTracks(System.Collections.Generic.List<int> blockNumbers, DiskSpecs specs)
        {
            if (blockNumbers == null)
                throw new ArgumentNullException(nameof(blockNumbers));

            var tracks = new System.Collections.Generic.List<int>();
            foreach (var block in blockNumbers)
            {
                tracks.Add(BlockToTrack(block, specs));
            }
            return tracks;
        }

        /// <summary>
        /// Calcula los tiempos de acceso al disco basado en el movimiento total y número de peticiones
        /// </summary>
        /// <param name="totalTracks">Número total de pistas movidas</param>
        /// <param name="numRequests">Número de peticiones atendidas</param>
        /// <param name="timeSpecs">Especificaciones de tiempo</param>
        /// <param name="diskSpecs">Especificaciones del disco (opcional, para cálculo preciso de transferencia)</param>
        /// <returns>Resultado con los tiempos calculados</returns>
        public AccessTimeResult CalculateAccessTime(
            int totalTracks,
            int numRequests,
            TimeSpecs timeSpecs,
            DiskSpecs? diskSpecs = null)
        {
            if (timeSpecs == null)
                throw new ArgumentNullException(nameof(timeSpecs));

            if (totalTracks < 0)
                throw new ArgumentException("El total de pistas no puede ser negativo", nameof(totalTracks));

            if (numRequests <= 0)
                throw new ArgumentException("El número de peticiones debe ser mayor que 0", nameof(numRequests));

            var result = new AccessTimeResult();

            // 1. Tiempo de búsqueda (Seek Time)
            // Tiempo total = pistas movidas × tiempo por pista
            result.SeekTimeMs = totalTracks * timeSpecs.SeekTimePerTrack;

            // 2. Tiempo de latencia (Rotational Delay)
            // Tiempo promedio de latencia = media vuelta × número de peticiones
            // Media vuelta = (60 * 1000 / RPM) / 2
            result.LatencyTimeMs = timeSpecs.AverageLatencyMs * numRequests;

            // 3. Tiempo de transferencia
            // Depende de cuántos sectores hay que leer por bloque
            if (diskSpecs != null && diskSpecs.SectorsPerTrack > 0)
            {
                // Tiempo para leer un bloque = (sectores por bloque / sectores por pista) × tiempo de rotación
                double timePerBlock = (timeSpecs.SectorsPerBlock / (double)diskSpecs.SectorsPerTrack) * timeSpecs.RotationTimeMs;
                result.TransferTimeMs = timePerBlock * numRequests;
            }
            else
            {
                // Aproximación si no tenemos especificaciones completas del disco
                // Asumimos ~10 sectores por pista como estándar
                double timePerBlock = (timeSpecs.SectorsPerBlock / 10.0) * timeSpecs.RotationTimeMs;
                result.TransferTimeMs = timePerBlock * numRequests;
            }

            // 4. Calcular tiempo total
            result.CalculateTotal();

            return result;
        }

        /// <summary>
        /// Valida que un número de pista esté en formato correcto (sin ceros a la izquierda)
        /// </summary>
        /// <param name="trackString">String del número de pista</param>
        /// <returns>True si el formato es válido</returns>
        public bool ValidateTrackFormat(string trackString)
        {
            if (string.IsNullOrWhiteSpace(trackString))
                return false;

            // Verificar que no tenga ceros a la izquierda (excepto si es solo "0")
            if (trackString.Length > 1 && trackString.StartsWith("0"))
                return false;

            // Verificar que sea un número válido
            return int.TryParse(trackString, out _);
        }

        /// <summary>
        /// Calcula el número de sectores por bloque basado en las especificaciones
        /// </summary>
        public int CalculateSectorsPerBlock(DiskSpecs specs)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));

            if (specs.SectorSize <= 0)
                throw new ArgumentException("El tamaño de sector debe ser mayor que 0");

            return (int)Math.Ceiling((double)specs.BlockSize / specs.SectorSize);
        }
    }
}
