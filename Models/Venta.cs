using System;
using System.Collections.Generic;

namespace AppCaravana.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        // Monto total de la venta (suma de las caravanas asociadas)
        public decimal Importe { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public List<VentaCaravana> VentaCaravanas { get; set; } = new();
    }
}