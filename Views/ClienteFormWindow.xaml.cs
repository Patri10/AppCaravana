using System;
using System.Linq;
using System.Windows;
using AppCaravana.Data;
using AppCaravana.Models;
using AppCaravana.Services;

namespace AppCaravana.Views
{
    public partial class ClienteFormWindow : Window
    {
        private int? _clienteId;
        private readonly QRCodeService _qrService;

        public ClienteFormWindow(int? clienteId = null)
        {
            InitializeComponent();
            _clienteId = clienteId;
            _qrService = new QRCodeService();

            if (_clienteId.HasValue)
                CargarCliente();
        }

        private void CargarCliente()
        {
            using (var db = new AppDbContext())
            {
                var cliente = db.Clientes.FirstOrDefault(c => c.Id == _clienteId.Value);
                if (cliente == null) return;

                txtNombre.Text = cliente.Nombre;
                txtApellido.Text = cliente.Apellido;
                txtDNI.Text = cliente.DNI;
                txtTelefono.Text = cliente.Telefono;
                txtEmail.Text = cliente.Email;
            }
        }

        public void consultarVentas()
        {
            using (var db = new AppDbContext())
            {
                var cliente = db.Clientes
                    .Where(c => c.Id == _clienteId.Value)
                    .Select(c => new
                    {
                        c.Nombre,
                        c.Apellido,
                        Ventas = c.Ventas.Select(v => new
                        {
                            v.Id,
                            v.Fecha,
                            v.Importe
                        }).ToList()
                    })
                    .FirstOrDefault();

                if (cliente != null)
                {
                    string mensaje = $"Ventas del cliente {cliente.Nombre} {cliente.Apellido}:\n\n";
                    foreach (var venta in cliente.Ventas)
                    {
                        mensaje += $"ID: {venta.Id}, Fecha: {venta.Fecha.ToShortDateString()}, Monto: {venta.Importe:C}\n";
                    }

                    MessageBox.Show(mensaje, "Ventas del Cliente", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Cliente no encontrado o sin ventas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones simples
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text))
            {
                MessageBox.Show("Nombre, Apellido y DNI son obligatorios.");
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    Cliente cliente;

                    if (_clienteId.HasValue)
                        cliente = db.Clientes.First(c => c.Id == _clienteId.Value);
                    else
                        cliente = new Cliente();

                    cliente.Nombre = txtNombre.Text;
                    cliente.Apellido = txtApellido.Text;
                    cliente.DNI = txtDNI.Text;
                    cliente.Telefono = txtTelefono.Text;
                    cliente.Email = txtEmail.Text;

                    if (!_clienteId.HasValue)
                        cliente.FechaRegistro = DateTime.Now;

                    if (_clienteId.HasValue == false)
                        db.Clientes.Add(cliente);

                    db.SaveChanges();

                    // Generar o regenerar QR después de guardar (una vez que tiene ID)
                    try
                    {
                        var (imagePath, qrContent, publicCode) = _qrService.GenerateQRCodeForClient(
                            cliente.Id,
                            $"{cliente.Nombre} {cliente.Apellido}",
                            cliente.CodigoQR // reutiliza código si ya existía
                        );

                        // Actualizar referencia al QR en la BD (Código público + ruta)
                        cliente.CodigoQR = publicCode;
                        cliente.RutaImagenQR = imagePath;
                        db.SaveChanges();
                    }
                    catch (Exception qrEx)
                    {
                        // Log pero no detener el flujo
                        System.Diagnostics.Debug.WriteLine($"Error generando QR: {qrEx.Message}");
                    }
                }

                MessageBox.Show("Cliente guardado correctamente.");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cliente: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
