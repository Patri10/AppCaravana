using System;

namespace AppCaravana.Models
{
    /// <summary>
    /// Ítem de una venta, permite asociar varias caravanas a una misma venta.
    /// </summary>
    public class VentaCaravana
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta Venta { get; set; }

        public int CaravanaId { get; set; }
        public Caravana Caravana { get; set; }

        public decimal Importe { get; set; } // Precio de la caravana al momento de la venta
    }
}
