namespace WoodMarket.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file, int entityId, string folder = "products");
        void DeleteImage(string imagePath);
    }
}
