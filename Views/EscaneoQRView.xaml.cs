using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppCaravana.Data;
using AppCaravana.Services;
using Microsoft.EntityFrameworkCore;

namespace AppCaravana.Views
{
    public partial class EscaneoQRView : UserControl
    {
        private readonly AppDbContext _dbContext;
        private readonly QRCodeService _qrService;

        public EscaneoQRView()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _qrService = new QRCodeService();
        }

        private void TxtCodigoQR_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                BtnBuscar_Click(null, null);
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string codigo = TxtCodigoQR.Text.Trim();

                if (string.IsNullOrEmpty(codigo))
                {
                    MessageBox.Show("Por favor, ingresa un código o ID del cliente.", "Campo vacío", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obtener código público (no exponer ID)
                string? publicCode = null;

                if (codigo.StartsWith("CLIENTE_CODE|", StringComparison.OrdinalIgnoreCase))
                {
                    publicCode = _qrService.ExtractPublicCodeFromQRContent(codigo);
                }
                else
                {
                    // Asumir que el input es directamente el código público
                    publicCode = codigo;
                }

                if (string.IsNullOrWhiteSpace(publicCode))
                {
                    MessageBox.Show("Formato de código no reconocido. Usa el código QR o el código público del cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var cliente = await _dbContext.Clientes
                    .Include(c => c.Ventas)
                        .ThenInclude(v => v.VentaCaravanas)
                            .ThenInclude(vc => vc.Caravana)
                    .FirstOrDefaultAsync(c => c.CodigoQR == publicCode);

                if (cliente == null)
                {
                    MessageBox.Show($"Cliente con código {publicCode} no encontrado.", "No encontrado", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClienteDataGrid.Visibility = Visibility.Collapsed;
                    TxtNoCliente.Visibility = Visibility.Visible;
                    VentasDataGrid.Visibility = Visibility.Collapsed;
                    TxtNoVentas.Visibility = Visibility.Visible;
                    ResumenGrid.Visibility = Visibility.Collapsed;
                    TxtNoResumen.Visibility = Visibility.Visible;
                    return;
                }

                TxtNombreCliente.Text = cliente.Nombre;
                TxtApellidoCliente.Text = cliente.Apellido;
                TxtDNICliente.Text = cliente.DNI;
                TxtEmailCliente.Text = cliente.Email;
                TxtTelefonoCliente.Text = cliente.Telefono;
                ClienteDataGrid.Visibility = Visibility.Visible;
                TxtNoCliente.Visibility = Visibility.Collapsed;

                var ventasVm = cliente.Ventas?
                    .Select(v => new
                    {
                        v.Id,
                        v.Fecha,
                        v.Importe,
                        CantidadCaravanas = v.VentaCaravanas?.Count ?? 0,
                        Caravanas = string.Join(", ", v.VentaCaravanas?.Select(vc => vc.Caravana?.Serie) ?? Array.Empty<string>())
                    })
                    .ToList() ?? new();

                if (ventasVm.Any())
                {
                    VentasDataGrid.ItemsSource = ventasVm;
                    VentasDataGrid.Visibility = Visibility.Visible;
                    TxtNoVentas.Visibility = Visibility.Collapsed;
                }
                else
                {
                    VentasDataGrid.Visibility = Visibility.Collapsed;
                    TxtNoVentas.Visibility = Visibility.Visible;
                }

                var ventasTotal = ventasVm.Sum(v => v.Importe);
                var ventasCount = ventasVm.Count;
                var totalCaravanas = ventasVm.Sum(v => v.CantidadCaravanas);
                var totalGasto = ventasTotal; // gasto total = importe vendido (no hay campo de gasto separado)

                TxtTotalVentas.Text = $"${ventasTotal:F2}";
                TxtTotalGasto.Text = $"${totalGasto:F2}";
                TxtTotalCaravanas.Text = totalCaravanas.ToString();
                TxtCantidadVentas.Text = ventasCount.ToString();
                ResumenGrid.Visibility = Visibility.Visible;
                TxtNoResumen.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
