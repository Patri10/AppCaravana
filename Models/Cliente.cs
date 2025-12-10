using System;
using System.Collections.Generic;

namespace AppCaravana.Models
{
    /// <summary>
    /// Entidad Cliente (Gesti�n de la informaci�n del comprador).
    /// Requisito Funcional 4.
    /// </summary>
    public class Cliente
    {
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        public DateTime FechaRegistro { get; set; }
        public string? CodigoQR { get; set; }
        public string? RutaImagenQR { get; set; }

        public List<Venta> Ventas { get; set; } = new();
    }

}