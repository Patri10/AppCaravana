using System;
using System.Drawing;
using System.IO;
using QRCoder;

namespace AppCaravana.Services
{
    public class QRCodeService
    {
        private readonly string _qrImagePath;

        public QRCodeService()
        {
            // Carpeta para guardar imágenes QR
            _qrImagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AppCaravana",
                "QR_Codes"
            );

            if (!Directory.Exists(_qrImagePath))
                Directory.CreateDirectory(_qrImagePath);
        }

        /// <summary>
        /// Genera un código QR para un cliente y lo guarda como PNG.
        /// Retorna la ruta donde se guardó la imagen y el contenido del QR.
        /// Usa un código público (no expone el ID) para poder buscar al cliente.
        /// </summary>
        public (string imagePath, string qrContent, string publicCode) GenerateQRCodeForClient(int clienteId, string clienteName, string? existingPublicCode = null)
        {
            try
            {
                // Generar/reusar código público
                string publicCode = string.IsNullOrWhiteSpace(existingPublicCode)
                    ? GeneratePublicCode()
                    : existingPublicCode;

                // Crear contenido del QR sin exponer el ID
                string qrContent = GetQRContent(publicCode, clienteName, cantidadVentas: 0, totalVentas: 0);

                // Generar QR usando QRCoder
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);

                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        var qrImage = qrCode.GetGraphic(10); // 10 pixels por modulo

                        // Guardar imagen PNG
                        string fileName = $"Cliente_{clienteId}_QR.png";
                        string filePath = Path.Combine(_qrImagePath, fileName);

                        qrImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                        return (filePath, qrContent, publicCode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar código QR para cliente {clienteId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Devuelve el contenido estándar del QR para un cliente.
        /// Formato: CLIENTE_CODE|{publicCode}|{clienteName}
        /// </summary>
        public string GetQRContent(string publicCode, string clienteName, int cantidadVentas, decimal totalVentas)
        {
            // Evitar caracteres '|' en el nombre para no romper el formato
            var safeName = (clienteName ?? string.Empty).Replace("|", "-");
            
            return $"CLIENTE_CODE|{publicCode}|{safeName}" + "\n" + $"{cantidadVentas}" + "\n" + $"{totalVentas}";
        }

        /// <summary>
        /// Genera un código público aleatorio (no expone el ID real).
        /// Ejemplo: C-AB12CD34EF
        /// </summary>
        public string GeneratePublicCode()
        {
            var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
            return $"C-{guid.Substring(0, 10)}";
        }

        /// <summary>
        /// Extrae el código público del contenido del QR.
        /// Devuelve null si el formato no coincide.
        /// </summary>
        public string? ExtractPublicCodeFromQRContent(string qrContent)
        {
            try
            {
                var parts = qrContent.Split('|');
                if (parts.Length >= 2 && parts[0] == "CLIENTE_CODE")
                {
                    return parts[1];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parsea el contenido del QR y extrae el ID del cliente.
        /// Formato esperado: CLIENTE|{clienteId}|{clienteName}
        /// </summary>
        public int? ExtractClientIdFromQRContent(string qrContent)
        {
            try
            {
                var parts = qrContent.Split('|');
                if (parts.Length >= 2 && parts[0] == "CLIENTE" && int.TryParse(parts[1], out int clienteId))
                {
                    return clienteId;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene la ruta completa de una imagen QR guardada.
        /// </summary>
        public string GetQRImagePath(int clienteId)
        {
            return Path.Combine(_qrImagePath, $"Cliente_{clienteId}_QR.png");
        }

        /// <summary>
        /// Verifica si existe una imagen QR para un cliente.
        /// </summary>
        public bool QRImageExists(int clienteId)
        {
            return File.Exists(GetQRImagePath(clienteId));
        }

        /// <summary>
        /// Elimina la imagen QR de un cliente.
        /// </summary>
        public void DeleteQRImage(int clienteId)
        {
            try
            {
                var imagePath = GetQRImagePath(clienteId);
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar imagen QR: {ex.Message}", ex);
            }
        }
    }
}