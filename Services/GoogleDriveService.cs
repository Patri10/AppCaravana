using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace AppCaravana.Services
{
    public class GoogleDriveService
    {
        private readonly string _credentialsPath;
        private readonly string _tokenPath;
        private DriveService _driveService;

        public GoogleDriveService()
        {
            _credentialsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AppCaravana",
                "credentials.json"
            );
            _tokenPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AppCaravana",
                "google_drive_token"
            );
        }

        /// <summary>
        /// Busca un archivo por nombre dentro de una carpeta (opcional). Retorna el primer archivo encontrado o null.
        /// </summary>
        public async Task<Google.Apis.Drive.v3.Data.File?> FindFileByNameInFolderAsync(string fileName, string? folderId = null)
        {
            try
            {
                var service = await GetServiceAsync();

                var escapedName = fileName.Replace("'", "\\'");
                var query = $"name='{escapedName}' and trashed=false";
                if (!string.IsNullOrEmpty(folderId))
                    query += $" and '{folderId}' in parents";

                var request = service.Files.List();
                request.Q = query;
                request.Fields = "files(id, name, mimeType, size)";
                request.PageSize = 10;

                var result = await request.ExecuteAsync();
                if (result.Files != null && result.Files.Count > 0)
                    return result.Files[0];

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar archivo en Drive: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sube un archivo a Drive y si ya existe uno con el mismo nombre en la carpeta indicada lo reemplaza.
        /// Retorna el id del archivo en Drive.
        /// </summary>
        public async Task<string> UploadOrReplaceFileAsync(string localFilePath, string? folderId = null, string? targetFileName = null)
        {
            try
            {
                var service = await GetServiceAsync();
                var fileName = targetFileName ?? Path.GetFileName(localFilePath);

                var existing = await FindFileByNameInFolderAsync(fileName, folderId);

                using (var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    if (existing != null)
                    {
                        // Actualizar contenido del archivo existente
                        var updateRequest = service.Files.Update(new Google.Apis.Drive.v3.Data.File(), existing.Id, stream, GetMimeType(localFilePath));
                        updateRequest.Fields = "id, webViewLink";

                        var result = updateRequest.Upload();
                        if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                            throw new Exception($"Error al actualizar archivo: {result.Exception?.Message}");

                        return updateRequest.ResponseBody.Id;
                    }
                    else
                    {
                        // Crear nuevo archivo
                        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                        {
                            Name = fileName,
                            MimeType = GetMimeType(localFilePath)
                        };

                        if (!string.IsNullOrEmpty(folderId))
                            fileMetadata.Parents = new List<string> { folderId };

                        var createRequest = service.Files.Create(fileMetadata, stream, GetMimeType(localFilePath));
                        createRequest.Fields = "id, webViewLink";

                        var result = createRequest.Upload();
                        if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                            throw new Exception($"Error al crear archivo: {result.Exception?.Message}");

                        return createRequest.ResponseBody.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en UploadOrReplaceFileAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene o crea el servicio de Google Drive autenticado
        /// </summary>
        public async Task<DriveService> GetServiceAsync()
        {
            if (_driveService != null)
                return _driveService;

            // Crear directorio si no existe
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AppCaravana"
            );
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            UserCredential credential;

            // Usar credenciales almacenadas si existen
            // Primero intentar la ruta esperada
            var credentialsToUse = _credentialsPath;

            // Si no existe, buscar en Documents\Cred por client_secret_*.json y copiarlo a AppData
            if (!File.Exists(credentialsToUse))
            {
                try
                {
                    var docsCredDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Cred");
                    if (Directory.Exists(docsCredDir))
                    {
                        var matches = Directory.GetFiles(docsCredDir, "client_secret_*.json");
                        if (matches.Length > 0)
                        {
                            // Tomar el primer match
                            var found = matches[0];
                            // Asegurar directorio AppData
                            var appDataDir = Path.GetDirectoryName(_credentialsPath) ?? docsCredDir;
                            if (!Directory.Exists(appDataDir))
                                Directory.CreateDirectory(appDataDir);

                            // Copiar para que futuras ejecuciones usen la ruta estándar
                            File.Copy(found, _credentialsPath, true);
                            credentialsToUse = _credentialsPath;
                        }
                    }
                }
                catch
                {
                    
                }
            }

            if (File.Exists(credentialsToUse))
            {
                using (var stream = new FileStream(credentialsToUse, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(_tokenPath, true)
                    );
                }
            }
            else
            {
                throw new FileNotFoundException(
                    $"Archivo de credenciales no encontrado en {_credentialsPath}. " +
                    "Por favor, descargue las credenciales de la API de Google Drive y colóquelas en %APPDATA%\\AppCaravana o en Documents\\Cred."
                );
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AppCaravana"
            });

            return _driveService;
        }

        /// <summary>
        /// Sube un archivo a Google Drive
        /// </summary>
        public async Task<string> UploadFileAsync(string filePath, string? folderId = null)
        {
            try
            {
                var service = await GetServiceAsync();
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(filePath),
                    MimeType = GetMimeType(filePath)
                };

                if (!string.IsNullOrEmpty(folderId))
                {
                    fileMetadata.Parents = new List<string> { folderId };
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var request = service.Files.Create(fileMetadata, stream, GetMimeType(filePath));
                    request.Fields = "id, webViewLink";

                    var result = request.Upload();
                    if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                        throw new Exception($"Error al subir archivo: {result.Exception?.Message}");

                    return request.ResponseBody.Id;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al subir archivo a Google Drive: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene o crea una carpeta en Google Drive
        /// </summary>
        public async Task<string> GetOrCreateFolderAsync(string folderName, string? parentFolderId = null)
        {
            try
            {
                var service = await GetServiceAsync();

                // Buscar carpeta existente
                var query = $"name='{folderName}' and mimeType='application/vnd.google-apps.folder' and trashed=false";
                if (!string.IsNullOrEmpty(parentFolderId))
                    query += $" and '{parentFolderId}' in parents";

                var request = service.Files.List();
                request.Q = query;
                request.Fields = "files(id, name)";
                request.PageSize = 10;

                var result = await request.ExecuteAsync();
                if (result.Files.Count > 0)
                    return result.Files[0].Id;

                // Crear carpeta si no existe
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    folderMetadata.Parents = new List<string> { parentFolderId };
                }

                var createRequest = service.Files.Create(folderMetadata);
                createRequest.Fields = "id";

                var folder = await createRequest.ExecuteAsync();
                return folder.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener o crear carpeta en Google Drive: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el MIME type según la extensión del archivo
        /// </summary>
        private string GetMimeType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".csv" => "text/csv",
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Obtiene la URL pública de visualización de un archivo en Drive
        /// </summary>
        public async Task<string> GetFileWebViewLinkAsync(string fileId)
        {
            try
            {
                var service = await GetServiceAsync();
                var request = service.Files.Get(fileId);
                request.Fields = "webViewLink";

                var result = await request.ExecuteAsync();
                return result.WebViewLink;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener link de Drive: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si las credenciales están disponibles
        /// </summary>
        public bool HasCredentials()
        {
            return File.Exists(_credentialsPath);
        }

        /// <summary>
        /// Obtiene la ruta donde deben guardarse las credenciales
        /// </summary>
        public string GetCredentialsPath()
        {
            return _credentialsPath;
        }

        /// <summary>
        /// Verifica si existe un token de usuario almacenado
        /// </summary>
        public bool HasToken()
        {
            if (Directory.Exists(_tokenPath))
            {
                var files = Directory.GetFiles(_tokenPath);
                return files.Length > 0;
            }
            return false;
        }
    }
}
