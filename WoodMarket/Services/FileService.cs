namespace WoodMarket.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile file, int entityId, string folder = "products")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Файл не выбран");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException($"Недопустимый формат файла. Разрешены: {string.Join(", ", allowedExtensions)}");

                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("Размер файла не должен превышать 5MB");

                var fileName = $"{entityId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var uploadPath = Path.Combine(_env.WebRootPath, "images", folder);

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/images/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении изображения");
                throw;
            }
        }

        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            try
            {
                var fileName = Path.GetFileName(imagePath);
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                var filePath = Path.Combine(uploadPath, fileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении изображения {ImagePath}", imagePath);
            }
        }
    }
}
