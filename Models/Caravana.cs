using System;

namespace AppCaravana.Models
{
    public class Caravana
    {
        public int Id { get; set; }

        // Identificación general
        public string Serie { get; set; }         
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Año { get; set; }

        // Identificación legal / SENASA
        public string Matricula { get; set; }
        public string NumeroSenasa { get; set; }   

        // Tipo comercial (simple, trailer, liviana, etc.)
        public string Tipo { get; set; }

        // Datos comerciales
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }

        // Detalles descriptivos
        public string Caracteristicas { get; set; }
        public string Descripcion { get; set; }
    }
}
