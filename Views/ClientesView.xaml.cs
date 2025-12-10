using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppCaravana.Data;
using AppCaravana.Models;
using AppCaravana.Services;
using System.IO;
using System.Windows.Input;

namespace AppCaravana.Views
{
    public partial class ClientesView : UserControl
    {
        private readonly QRCodeService _qrService;

        public ClientesView()
        {
            InitializeComponent();
            _qrService = new QRCodeService();
            CargarClientes();
        }

        private void CargarClientes()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var clientes = db.Clientes
                        .OrderBy(c => c.Apellido)
                        .ThenBy(c => c.Nombre)
                        .ToList();

                    bool updated = false;

                    foreach (var cliente in clientes)
                    {
                        if (cliente.Id > 0)
                        {
                            if (string.IsNullOrWhiteSpace(cliente.CodigoQR))
                            {
                                cliente.CodigoQR = _qrService.GeneratePublicCode();
                                updated = true;
                            }

                            string imagePath = _qrService.GetQRImagePath(cliente.Id);
                            if (!File.Exists(imagePath))
                            {
                                var result = _qrService.GenerateQRCodeForClient(
                                    cliente.Id,
                                    $"{cliente.Nombre} {cliente.Apellido}",
                                    cliente.CodigoQR
                                );
                                cliente.RutaImagenQR = result.imagePath;
                                cliente.CodigoQR = result.publicCode;
                                updated = true;
                            }
                            else
                            {
                                cliente.RutaImagenQR = imagePath;
                            }
                        }
                    }

                    if (updated)
                    {
                        db.SaveChanges();
                    }

                    dgClientes.ItemsSource = clientes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los clientes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarClientes();
        }

        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            ClienteFormWindow form = new ClienteFormWindow();
            form.ShowDialog();
            CargarClientes();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            Cliente cliente = (sender as Button).DataContext as Cliente;
            if (cliente == null) return;

            ClienteFormWindow form = new ClienteFormWindow(cliente.Id);
            form.ShowDialog();
            CargarClientes();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            Cliente cliente = (sender as Button).DataContext as Cliente;
            if (cliente == null) return;

            if (MessageBox.Show("¿Desea eliminar este cliente?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        bool tieneVentas = db.Ventas.Any(v => v.ClienteId == cliente.Id);
                        if (tieneVentas)
                        {
                            MessageBox.Show("No se puede eliminar el cliente porque tiene ventas asociadas.", "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Rehidratar el cliente en este contexto para evitar tracking cruzado
                        var toDelete = db.Clientes.FirstOrDefault(c => c.Id == cliente.Id);
                        if (toDelete == null)
                        {
                            MessageBox.Show("El cliente ya no existe.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        db.Clientes.Remove(toDelete);
                        db.SaveChanges();
                    }

                    try
                    {
                        _qrService.DeleteQRImage(cliente.Id);
                    }
                    catch
                    {
                        // Ignorar errores de limpieza de archivo para no abortar la operación
                    }

                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "No se pudo eliminar el cliente. Verifique que no tenga ventas u otros registros asociados.\n\nDetalle: " + ex.Message,
                        "Error al eliminar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void BtnCopiarCodigo_Click(object sender, RoutedEventArgs e)
        {
            var cliente = (sender as Button)?.DataContext as Cliente;
            if (cliente == null || string.IsNullOrWhiteSpace(cliente.CodigoQR)) return;

            Clipboard.SetText(cliente.CodigoQR);
            MessageBox.Show("Código copiado al portapapeles.", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
