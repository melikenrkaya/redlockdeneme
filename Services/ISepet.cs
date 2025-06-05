using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;

namespace RedlockDeneme.Services
{
    public interface ISepet
    {
        Task<List<SepetWithUrunDTO>> GetAllAsync();
        Task<Sepet?> GetByIdAsync(int id);
        Task<SepetWithUrunDTO?> CreateAsync(Sepet SepetModel);
        Task<SepetWithUrunDTO?> UpdateAsync(int id, UpdateSepetRequestDTO SepetDto);
        Task SepetiTemizleAsync(int urunId);
        Task<List<SepetWithUrunDTO>> TemizleStoguBitmisUrunleriAsync();

    }
}
