using System.Windows;
using AppCaravana.Services;
using AppCaravana.Views;

namespace AppCaravana
{
    public partial class MainWindow : Window
    {
        private ExportService _exportService;
        private GoogleDriveService _googleDriveService;
        private DriveAutoSaveService? _autoSaveService;

        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new CaravanasView();
            _exportService = new ExportService();
            _googleDriveService = new GoogleDriveService();
            _autoSaveService = new DriveAutoSaveService(_exportService, _googleDriveService);

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_googleDriveService.HasCredentials() && _googleDriveService.HasToken())
            {
                try
                {
                    await _googleDriveService.GetServiceAsync();
                    if (_autoSaveService != null)
                        await _autoSaveService.StartAsync();
                }
                catch
                {
                    // Silenciar errores de reconexión automática
                }
            }
        }

        private void BtnCaravanas_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CaravanasView();
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClientesView();
        }

        private void BtnVentas_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new VentasView();
        }

        private void BtnInformes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new InformesView();
        }

        private void BtnEscaneoQR_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new EscaneoQRView();
        }

        private async void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Iniciando exportación de datos...", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);

                string exportPath = await System.Threading.Tasks.Task.Run(() => _exportService.ExportAllDataAsync());

                MessageBox.Show(
                    $"Datos exportados exitosamente a:\n{exportPath}",
                    "Exportacion completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                _exportService.OpenExportFolder();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnConnectGoogle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Iniciando conexión con Google...", "Conectar", MessageBoxButton.OK, MessageBoxImage.Information);

                await _googleDriveService.GetServiceAsync();

                if (_autoSaveService != null)
                {
                    await _autoSaveService.StartAsync();
                    MessageBox.Show("Conexión establecida. Guardado automático activado.", "Conectado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"No se pudo conectar a Google: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            _autoSaveService?.Dispose();
            Application.Current.Shutdown();
        }
    }
}
