using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;

namespace RedlockDeneme.Services
{
    public interface IStok
    {
        Task<List<Stok>> GetAllAsync();
        Task<Stok?> GetByIdAsync(int id);
        Task<Stok?> CreateAsync(Stok StokModel);
        Task<Stok?> UpdateAsync(int id, UpdateStokRequestDto StokDto);
        Task<Stok?> DeleteAsync(int id);
        Task<string?> SiparisVerAsync(int stokId, string kullaniciAdi);

    }
}
