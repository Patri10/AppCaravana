using System;

namespace AppCaravana.Models
{
    /// <summary>
    /// Entidad Stock (Inventario por tipo de caravana).
    /// Requisito Funcional 3.
    /// </summary>
    public class Stock
    {
        public int StockID { get; set; }
        public string TipoCaravana { get; set; } // FDX-B, Visual, etc.
        public int CantidadDisponible { get; set; }
        public int CantidadTotal { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}