using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AppCaravana.Data;
using AppCaravana.Services;
using Microsoft.EntityFrameworkCore;

namespace AppCaravana.Views
{
    public partial class InformesView : UserControl
    {
        private ReportService? _reportService;

        public InformesView()
        {
            InitializeComponent();
            InitializeReportService();
            this.Loaded += InformesView_Loaded;
        }

        private async void InformesView_Loaded(object sender, RoutedEventArgs e)
        {
            await GenerateReportsAsync();
        }

        private void InitializeReportService()
        {
            try
            {
                var dbContext = new AppDbContext();
                // Obtener la ruta de la base de datos del contexto
                var connectionString = dbContext.Database.GetConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se pudo obtener la cadena de conexión");
                }

                // Extraer la ruta del archivo de la cadena de conexión
                string dbPath = connectionString.Split("=")[1].Replace("\"", "");
                
                // Asegurar ruta absoluta
                if (!Path.IsPathRooted(dbPath))
                {
                    dbPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbPath));
                }

                string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "generate_reports.py");
                _reportService = new ReportService(pythonScriptPath, dbPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inicializando servicio de reportes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnGenerarReportes_Click(object sender, RoutedEventArgs e)
        {
            await GenerateReportsAsync();
        }

        private async System.Threading.Tasks.Task GenerateReportsAsync()
        {
            try
            {
                if (_reportService == null)
                {
                    // Intentar inicializar de nuevo si falló antes
                    InitializeReportService();
                    if (_reportService == null)
                    {
                        txtEstado.Text = "Error: Servicio no disponible";
                        return;
                    }
                }

                btnGenerarReportes.IsEnabled = false;
                txtEstado.Text = "Generando reportes...";
                txtEstado.ToolTip = null;

                var reports = await _reportService.GenerateReportsAsync();

                reportsContainer.Children.Clear();

                if (reports.Count == 0)
                {
                    reportsContainer.Children.Add(new TextBlock
                    {
                        Text = "No hay datos para generar reportes o ocurrió un error en el script.",
                        FontSize = 16,
                        Foreground = System.Windows.Media.Brushes.Orange,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    });
                }
                else
                {
                    foreach (var report in reports)
                    {
                        AddReportImage(report.Key, report.Value);
                    }
                }

                txtEstado.Text = $"✓ {reports.Count} reportes generados correctamente";
            }
            catch (Exception ex)
            {
                // Mostrar error detallado
                txtEstado.Text = "Error al generar reportes (ver detalle)";
                txtEstado.ToolTip = ex.Message;
                
                // También mostrar en MessageBox para que el usuario lo vea claramente
                MessageBox.Show($"Error al generar reportes:\n{ex.Message}", "Error de Reportes", MessageBoxButton.OK, MessageBoxImage.Error);
                
                System.Diagnostics.Debug.WriteLine($"Error generando reportes: {ex.Message}");
            }
            finally
            {
                btnGenerarReportes.IsEnabled = true;
            }
        }

        private void AddReportImage(string reportName, string reportPath)
        {
            if (!File.Exists(reportPath))
            {
                reportsContainer.Children.Add(new TextBlock
                {
                    Text = $"Archivo no encontrado: {reportPath}",
                    Foreground = System.Windows.Media.Brushes.Red,
                    Margin = new Thickness(0, 10, 0, 10)
                });
                return;
            }

            // Título del reporte
            var title = new TextBlock
            {
                Text = reportName.Replace("_", " ").ToUpper(),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10),
                Foreground = System.Windows.Media.Brushes.DarkBlue
            };
            reportsContainer.Children.Add(title);

            // Imagen del reporte
            var image = new Image
            {
                Height = 400,
                Margin = new Thickness(0, 0, 0, 20),
                VerticalAlignment = VerticalAlignment.Top
            };

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(reportPath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            image.Source = bitmap;
            reportsContainer.Children.Add(image);

            // Separador
            var separator = new Separator
            {
                Margin = new Thickness(0, 10, 0, 10)
            };
            reportsContainer.Children.Add(separator);
        }
    }
}
