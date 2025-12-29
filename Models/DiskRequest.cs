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

        public DiskRequest(int position, int order)
        {
            Position = position;
            Order = order;
            IsProcessed = false;
        }

        public override string ToString()
        {
            return $"Petición #{Order}: Posición {Position}";
        }
    }
}
