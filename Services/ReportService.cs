using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppCaravana.Services
{
    public class ReportService
    {
        private readonly string _pythonScriptPath;
        private readonly string _dbPath;

        public ReportService(string pythonScriptPath, string dbPath)
        {
            _pythonScriptPath = pythonScriptPath;
            _dbPath = dbPath;
        }

        public async Task<Dictionary<string, string>> GenerateReportsAsync()
        {
            try
            {
                // Encontrar la ruta de Python
                var pythonPath = GetPythonExecutablePath();
                if (string.IsNullOrEmpty(pythonPath))
                {
                    throw new Exception("Python no está instalado o no está en el PATH");
                }

                // Crear proceso de Python
                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{_pythonScriptPath}\" \"{_dbPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                        throw new Exception("No se pudo iniciar el proceso de Python");

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    process.WaitForExit(60000); // Esperar máximo 1 minuto

                    process.WaitForExit(60000); // Esperar máximo 1 minuto

                    if (string.IsNullOrEmpty(output))
                    {
                        // Si no hay salida estandar, entonces si revisamos el error
                        if (!string.IsNullOrEmpty(error))
                            throw new Exception($"Error al generar reportes: {error}");
                        else
                            throw new Exception("No se recibió salida del script de Python");
                    }

                    // Parsear la salida JSON
                    var reports = JsonSerializer.Deserialize<Dictionary<string, string>>(output);
                    return reports ?? new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ReportService: {ex.Message}");
                throw;
            }
        }

        private string? GetPythonExecutablePath()
        {
            // Rutas comunes donde Python puede estar instalado
            string[] pythonPaths = new[]
            {
                "py",
                "python",
                "python3",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData\\Local\\Programs\\Python\\Python313\\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData\\Local\\Programs\\Python\\Python312\\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData\\Local\\Programs\\Python\\Python311\\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData\\Local\\Programs\\Python\\Python310\\python.exe"),
                "C:\\Python313\\python.exe",
                "C:\\Python312\\python.exe",
                "C:\\Python311\\python.exe",
                "C:\\Python310\\python.exe",
            };

            foreach (var path in pythonPaths)
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(processInfo))
                    {
                        if (process != null && process.WaitForExit(5000) && process.ExitCode == 0)
                        {
                            return path;
                        }
                    }
                }
                catch
                {
                    // Continuar con la siguiente ruta
                }
            }

            return null;
        }

        public string GetReportsDirectory()
        {
            var dbDir = Path.GetDirectoryName(_dbPath);
            if (string.IsNullOrEmpty(dbDir))
                dbDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(dbDir, "reports_output");
        }

        public List<string> GetAvailableReports()
        {
            var reportsDir = GetReportsDirectory();
            var reports = new List<string>();

            if (Directory.Exists(reportsDir))
            {
                var files = Directory.GetFiles(reportsDir, "*.png");
                foreach (var file in files)
                {
                    reports.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            return reports;
        }

        public string GetReportPath(string reportName)
        {
            var reportsDir = GetReportsDirectory();
            return Path.Combine(reportsDir, $"{reportName}.png");
        }
    }
}
