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

        public async Task<string> SaveImageAsync(IFormFile file, int productId)
        {
            try
            {
                // 1. Проверяем файл
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Файл не выбран");

                // 2. Проверяем расширение
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException($"Недопустимый формат файла. Разрешены: {string.Join(", ", allowedExtensions)}");

                // 3. Проверяем размер (макс 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("Размер файла не должен превышать 5MB");

                // 4. Генерируем уникальное имя
                var fileName = $"{productId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");

                // 5. Создаём папку, если её нет
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // 6. Полный путь к файлу
                var filePath = Path.Combine(uploadPath, fileName);

                // 7. Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 8. Возвращаем относительный URL
                return $"/images/products/{fileName}";
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
                // Извлекаем физический путь из URL
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
