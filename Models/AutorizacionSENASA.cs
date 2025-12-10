using System;

namespace AppCaravana.Models
{
    /// <summary>
    /// Entidad AutorizacionSENASA (Registro de autorizaciones).
    /// Requisitos Funcionales 1, 11.
    /// </summary>
  public class AutorizacionSENASA
{
    public int Id { get; set; }

    public string Numero { get; set; }
    public DateTime FechaEmision { get; set; }
    public string Estado { get; set; }

    public int CaravanaId { get; set; }
    public Caravana Caravana { get; set; }
}

}