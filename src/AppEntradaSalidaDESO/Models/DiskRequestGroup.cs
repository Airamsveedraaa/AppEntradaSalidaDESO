using System.Collections.Generic;

namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Representa un grupo de peticiones que llegan en el mismo instante
    /// </summary>
    public class DiskRequestGroup
    {
        public double ArrivalTime { get; set; }
        public string TracksInput { get; set; }

        public DiskRequestGroup(double arrivalTime = 0.0, string tracksInput = "")
        {
            ArrivalTime = arrivalTime;
            TracksInput = tracksInput;
        }
    }
}
