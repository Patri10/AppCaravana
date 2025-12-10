using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppCaravana.Data;
using AppCaravana.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AppCaravana.Services
{
    public class ExportService
    {
        private readonly AppDbContext _dbContext;

        public ExportService()
        {
            _dbContext = new AppDbContext();
        }

        /// <summary>
        /// Exporta todos los datos a un archivo Excel con múltiples hojas
        /// </summary>
        public async Task<string> ExportAllDataAsync(string? outputPath = null)
        {
            try
            {
                // Determinar ruta de salida
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "AppCaravana_Export"
                    );
                }

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                // Crear nombre de archivo con timestamp
                string timestamp = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
                string excelPath = Path.Combine(outputPath, $"Reporte_Fecha{timestamp}.xlsx");

                // Obtener datos de todas las tablas
                var clientes = await _dbContext.Clientes.ToListAsync();
                var caravanas = await _dbContext.Caravanas.ToListAsync();
                var ventas = await _dbContext.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaCaravanas)
                        .ThenInclude(vc => vc.Caravana)
                    .ToListAsync();

                // Crear workbook
                using (var workbook = new XLWorkbook())
                {
                    // Hoja de Clientes
                    var wsClientes = workbook.Worksheets.Add("Clientes");
                    wsClientes.Cell(1, 1).Value = "Id";
                    wsClientes.Cell(1, 2).Value = "Nombre";
                    wsClientes.Cell(1, 3).Value = "Apellido";
                    wsClientes.Cell(1, 4).Value = "DNI";
                    wsClientes.Cell(1, 5).Value = "Teléfono";
                    wsClientes.Cell(1, 6).Value = "Email";

                    // Formatear encabezado
                    wsClientes.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    wsClientes.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var cliente in clientes)
                    {
                        wsClientes.Cell(row, 1).Value = cliente.Id;
                        wsClientes.Cell(row, 2).Value = cliente.Nombre;
                        wsClientes.Cell(row, 3).Value = cliente.Apellido;
                        wsClientes.Cell(row, 4).Value = cliente.DNI;
                        wsClientes.Cell(row, 5).Value = cliente.Telefono;
                        wsClientes.Cell(row, 6).Value = cliente.Email;
                        row++;
                    }
                    wsClientes.Columns().AdjustToContents();

                    // Hoja de Caravanas
                    var wsCaravanas = workbook.Worksheets.Add("Caravanas");
                    wsCaravanas.Cell(1, 1).Value = "Id";
                    wsCaravanas.Cell(1, 2).Value = "Serie";
                    wsCaravanas.Cell(1, 3).Value = "Marca";
                    wsCaravanas.Cell(1, 4).Value = "Modelo";
                    wsCaravanas.Cell(1, 5).Value = "Año";
                    wsCaravanas.Cell(1, 6).Value = "Matrícula";
                    wsCaravanas.Cell(1, 7).Value = "Número SENASA";
                    wsCaravanas.Cell(1, 8).Value = "Tipo";
                    wsCaravanas.Cell(1, 9).Value = "Precio";
                    wsCaravanas.Cell(1, 10).Value = "Disponible";

                    // Formatear encabezado
                    wsCaravanas.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    wsCaravanas.Row(1).Style.Font.Bold = true;

                    row = 2;
                    foreach (var caravana in caravanas)
                    {
                        wsCaravanas.Cell(row, 1).Value = caravana.Id;
                        wsCaravanas.Cell(row, 2).Value = caravana.Serie;
                        wsCaravanas.Cell(row, 3).Value = caravana.Marca;
                        wsCaravanas.Cell(row, 4).Value = caravana.Modelo;
                        wsCaravanas.Cell(row, 5).Value = caravana.Año;
                        wsCaravanas.Cell(row, 6).Value = caravana.Matricula;
                        wsCaravanas.Cell(row, 7).Value = caravana.NumeroSenasa;
                        wsCaravanas.Cell(row, 8).Value = caravana.Tipo;
                        wsCaravanas.Cell(row, 9).Value = caravana.Precio;
                        wsCaravanas.Cell(row, 10).Value = caravana.Disponible;
                        row++;
                    }
                    wsCaravanas.Columns().AdjustToContents();

                    // Hoja de Ventas
                    var wsVentas = workbook.Worksheets.Add("Ventas");
                    wsVentas.Cell(1, 1).Value = "Id";
                    wsVentas.Cell(1, 2).Value = "Fecha";
                    wsVentas.Cell(1, 3).Value = "Cliente";
                    wsVentas.Cell(1, 4).Value = "Caravana";
                    wsVentas.Cell(1, 4).Value = "Caravanas";
                    wsVentas.Cell(1, 5).Value = "Cant. Caravanas";
                    wsVentas.Cell(1, 6).Value = "Importe";

                    // Formatear encabezado
                    wsVentas.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    wsVentas.Row(1).Style.Font.Bold = true;

                    row = 2;
                    foreach (var venta in ventas)
                    {
                        wsVentas.Cell(row, 1).Value = venta.Id;
                        wsVentas.Cell(row, 2).Value = venta.Fecha;
                        wsVentas.Cell(row, 3).Value = $"{venta.Cliente?.Nombre} {venta.Cliente?.Apellido}";
                        var listaCaravanas = string.Join(", ", venta.VentaCaravanas.Select(vc => vc.Caravana?.Serie));

                        wsVentas.Cell(row, 4).Value = listaCaravanas;
                        wsVentas.Cell(row, 5).Value = venta.VentaCaravanas.Count;
                        wsVentas.Cell(row, 6).Value = venta.Importe;
                        row++;
                    }
                    wsVentas.Columns().AdjustToContents();

                    // Hoja de Resumen
                    var wsResumen = workbook.Worksheets.Add("Resumen");
                    wsResumen.Cell(1, 1).Value = "Concepto";
                    wsResumen.Cell(1, 2).Value = "Cantidad";
                    wsResumen.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    wsResumen.Row(1).Style.Font.Bold = true;

                    wsResumen.Cell(2, 1).Value = "Total Clientes";
                    wsResumen.Cell(2, 2).Value = clientes.Count;
                    wsResumen.Cell(3, 1).Value = "Total Caravanas";
                    wsResumen.Cell(3, 2).Value = caravanas.Count;
                    wsResumen.Cell(4, 1).Value = "Total Ventas";
                    wsResumen.Cell(4, 2).Value = ventas.Count;
                    wsResumen.Cell(5, 1).Value = "Ingresos Totales";
                    wsResumen.Cell(5, 2).Value = ventas.Sum(v => v.Importe);
                    wsResumen.Cell(5, 2).Style.NumberFormat.Format = "$#,##0.00";

                    wsResumen.Columns().AdjustToContents();

                    // Guardar
                    workbook.SaveAs(excelPath);
                }

                return excelPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar datos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exporta solo los clientes a Excel
        /// </summary>
        public async Task<string> ExportClientesAsync(string? outputPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outputPath))
                    outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AppCaravana_Export");

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string excelPath = Path.Combine(outputPath, $"Clientes_{timestamp}.xlsx");

                var clientes = await _dbContext.Clientes.ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Clientes");
                    ws.Cell(1, 1).Value = "Id";
                    ws.Cell(1, 2).Value = "Nombre";
                    ws.Cell(1, 3).Value = "Apellido";
                    ws.Cell(1, 4).Value = "DNI";
                    ws.Cell(1, 5).Value = "Teléfono";
                    ws.Cell(1, 6).Value = "Email";

                    ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var cliente in clientes)
                    {
                        ws.Cell(row, 1).Value = cliente.Id;
                        ws.Cell(row, 2).Value = cliente.Nombre;
                        ws.Cell(row, 3).Value = cliente.Apellido;
                        ws.Cell(row, 4).Value = cliente.DNI;
                        ws.Cell(row, 5).Value = cliente.Telefono;
                        ws.Cell(row, 6).Value = cliente.Email;
                        row++;
                    }
                    ws.Columns().AdjustToContents();

                    workbook.SaveAs(excelPath);
                }

                return excelPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar clientes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exporta solo las caravanas a Excel
        /// </summary>
        public async Task<string> ExportCaravanasAsync(string? outputPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outputPath))
                    outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AppCaravana_Export");

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string excelPath = Path.Combine(outputPath, $"Caravanas_{timestamp}.xlsx");

                var caravanas = await _dbContext.Caravanas.ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Caravanas");
                    ws.Cell(1, 1).Value = "Id";
                    ws.Cell(1, 2).Value = "Serie";
                    ws.Cell(1, 3).Value = "Marca";
                    ws.Cell(1, 4).Value = "Modelo";
                    ws.Cell(1, 5).Value = "Año";
                    ws.Cell(1, 6).Value = "Matrícula";
                    ws.Cell(1, 7).Value = "Número SENASA";
                    ws.Cell(1, 8).Value = "Tipo";
                    ws.Cell(1, 9).Value = "Precio";
                    ws.Cell(1, 10).Value = "Disponible";

                    ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var caravana in caravanas)
                    {
                        ws.Cell(row, 1).Value = caravana.Id;
                        ws.Cell(row, 2).Value = caravana.Serie;
                        ws.Cell(row, 3).Value = caravana.Marca;
                        ws.Cell(row, 4).Value = caravana.Modelo;
                        ws.Cell(row, 5).Value = caravana.Año;
                        ws.Cell(row, 6).Value = caravana.Matricula;
                        ws.Cell(row, 7).Value = caravana.NumeroSenasa;
                        ws.Cell(row, 8).Value = caravana.Tipo;
                        ws.Cell(row, 9).Value = caravana.Precio;
                        ws.Cell(row, 10).Value = caravana.Disponible;
                        row++;
                    }
                    ws.Columns().AdjustToContents();

                    workbook.SaveAs(excelPath);
                }

                return excelPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar caravanas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exporta solo las ventas a Excel
        /// </summary>
        public async Task<string> ExportVentasAsync(string? outputPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outputPath))
                    outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AppCaravana_Export");

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string excelPath = Path.Combine(outputPath, $"Ventas_{timestamp}.xlsx");

                var ventas = await _dbContext.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaCaravanas)
                        .ThenInclude(vc => vc.Caravana)
                    .ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Ventas");
                    ws.Cell(1, 1).Value = "Id";
                    ws.Cell(1, 2).Value = "Fecha";
                    ws.Cell(1, 3).Value = "Cliente";
                    ws.Cell(1, 4).Value = "Caravanas";
                    ws.Cell(1, 5).Value = "Cant. Caravanas";
                    ws.Cell(1, 6).Value = "Importe";

                    ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var venta in ventas)
                    {
                        ws.Cell(row, 1).Value = venta.Id;
                        ws.Cell(row, 2).Value = venta.Fecha;
                        ws.Cell(row, 3).Value = $"{venta.Cliente?.Nombre} {venta.Cliente?.Apellido}";
                        var listaCaravanas = string.Join(", ", venta.VentaCaravanas.Select(vc => vc.Caravana?.Serie));
                        ws.Cell(row, 4).Value = listaCaravanas;
                        ws.Cell(row, 5).Value = venta.VentaCaravanas.Count;
                        ws.Cell(row, 6).Value = venta.Importe;
                        row++;
                    }
                    ws.Columns().AdjustToContents();

                    workbook.SaveAs(excelPath);
                }

                return excelPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar ventas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Abre la carpeta de exportación en el explorador de archivos
        /// </summary>
        public void OpenExportFolder()
        {
            try
            {
                string exportPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "AppCaravana_Export"
                );

                if (!Directory.Exists(exportPath))
                    Directory.CreateDirectory(exportPath);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = exportPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al abrir carpeta: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exporta datos a Excel y los sube a Google Drive
        /// </summary>
        public async Task<(string localPath, string driveFileId)> ExportAllDataAndUploadToDriveAsync(string? outputPath = null, string? driveFolderId = null)
        {
            try
            {
                // Primero generar el archivo localmente
                var localPath = await ExportAllDataAsync(outputPath);

                // Luego subirlo a Google Drive
                var driveService = new GoogleDriveService();

                if (!driveService.HasCredentials())
                {
                    throw new Exception(
                        "No se encontraron credenciales de Google Drive. " +
                        "Por favor, configure las credenciales en: " + driveService.GetCredentialsPath()
                    );
                }

                // Obtener o crear carpeta de exportación en Drive
                if (string.IsNullOrEmpty(driveFolderId))
                {
                    driveFolderId = await driveService.GetOrCreateFolderAsync("AppCaravana_Export");
                }

                // Subir archivo
                var fileId = await driveService.UploadFileAsync(localPath, driveFolderId);

                return (localPath, fileId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar y subir a Drive: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ejecuta el script de Python para generar gráficos
        /// </summary>
        private async Task GenerateReportsAsync(string dbPath)
        {
            try
            {
                // Obtener ruta del script Python
                var scriptPath = GetScriptPath();
                if (!File.Exists(scriptPath))
                {
                    // Script no encontrado, pero no es error crítico
                    return;
                }

                // Obtener ruta del intérprete Python
                var pythonPath = GetPythonExecutablePath();
                if (string.IsNullOrEmpty(pythonPath))
                {
                    return;
                }

                // Ejecutar script de forma asíncrona
                await Task.Run(() =>
                {
                    try
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = pythonPath,
                            Arguments = $"\"{scriptPath}\" \"{dbPath}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (var process = Process.Start(processInfo))
                        {
                            process?.WaitForExit(30000); // Esperar máximo 30 segundos
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error generando reportes: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GenerateReportsAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la ruta del script Python
        /// </summary>
        private string GetScriptPath()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var scriptPath = Path.Combine(appDir, "Reports", "generate_reports.py");
            return File.Exists(scriptPath) ? scriptPath : string.Empty;
        }

        /// <summary>
        /// Obtiene la ruta del ejecutable de Python
        /// </summary>
        private string? GetPythonExecutablePath()
        {
            var pythonPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python313", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python312", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python311", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python313", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python312", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python", "python.exe"),
                "python.exe"
            };

            foreach (var path in pythonPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}
