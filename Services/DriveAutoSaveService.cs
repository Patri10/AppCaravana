using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AppCaravana.Services
{
    public class DriveAutoSaveService : IDisposable
    {
        private readonly ExportService _exportService;
        private readonly GoogleDriveService _driveService;
        private readonly TimeSpan _interval;
        private readonly string _driveFolderName;
        private Timer? _timer;
        private string? _driveFolderId;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public DriveAutoSaveService(ExportService exportService, GoogleDriveService driveService, TimeSpan? interval = null, string driveFolderName = "reportes")
        {
            _exportService = exportService;
            _driveService = driveService;
            _interval = interval ?? TimeSpan.FromHours(1);
            _driveFolderName = driveFolderName;
        }

        public async Task StartAsync()
        {
            if (!_driveService.HasCredentials())
            {
                // Credenciales no encontradas, no iniciar autosave
                return;
            }

            // Obtener o crear carpeta en Drive
            _driveFolderId = await _driveService.GetOrCreateFolderAsync(_driveFolderName);

            // Ejecutar inmediatamente y luego cada intervalo
            _timer = new Timer(async _ => await TimerTickAsync(), null, TimeSpan.Zero, _interval);
        }

        private async Task TimerTickAsync()
        {
            if (!_semaphore.Wait(0))
                return;

            try
            {
                await DoExportAndUploadAsync();
            }
            catch
            {
                // Silenciar errores para no detener el timer; podrían registrarse en el futuro
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task TriggerNowAsync()
        {
            await TimerTickAsync();
        }

        private async Task DoExportAndUploadAsync()
        {
            // Generar archivo localmente
            var localPath = await _exportService.ExportAllDataAsync();

            // Nombre objetivo por hora (esto permitirá sobreescribir durante la misma hora)
            var targetFileName = $"AppCaravana_Completo_{DateTime.Now:yyyy-MM-dd_HH}.xlsx";

            // Subir o reemplazar en Drive (si _driveFolderId es null, crearlo de nuevo)
            if (string.IsNullOrEmpty(_driveFolderId))
            {
                if (_driveService.HasCredentials())
                    _driveFolderId = await _driveService.GetOrCreateFolderAsync(_driveFolderName);
            }

            if (!string.IsNullOrEmpty(_driveFolderId))
            {
                await _driveService.UploadOrReplaceFileAsync(localPath, _driveFolderId, targetFileName);
            }
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            Stop();
            _semaphore.Dispose();
        }
    }
}
