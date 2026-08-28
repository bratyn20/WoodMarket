namespace WoodMarket.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file, int productId);
        void DeleteImage(string imagePath);
    }
}
