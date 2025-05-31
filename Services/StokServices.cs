using Microsoft.EntityFrameworkCore;
using RedlockDeneme.Data.Context;
using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;
using RedLockNet;
using StackExchange.Redis;
using System.Text.Json;

namespace RedlockDeneme.Services
{
    public class StokServices : IStok
    {
        private readonly ApplicationDBContext _context;
        private readonly IDatabase _redisDb;
        private readonly IDistributedLockFactory _lockFactory;

        public StokServices(ApplicationDBContext context, IDatabase redisDb, IDistributedLockFactory lockFactory)
        {
            _context = context;
            _redisDb = redisDb;
            _lockFactory = lockFactory;
        }
public async Task<string> SiparisVerAsync(int stokId, string stokAdi)
        {
            string lockKey = $"lock:stok:{stokId}";

            using (var redLock = await _lockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(10)))
            {
                if (!redLock.IsAcquired)
                    return $"{stokAdi} - Kilit alınamadı.";
                Console.WriteLine($"lockkey: {lockKey}");
   
                var cacheKey = $"stok:{stokId}";
                Stok? stok = null;

                if (await _redisDb.KeyExistsAsync(cacheKey))
                {
                    var cachedValue = await _redisDb.StringGetAsync(cacheKey);
                    stok = JsonSerializer.Deserialize<Stok>(cachedValue);
                    Console.WriteLine($"cacheKey: {cacheKey}");
                    Console.WriteLine($"cachedValue: {cachedValue}");
                }
                else
                {
                    stok = await _context.Stoks.FirstOrDefaultAsync(s => s.StokId == stokId && s.StokAdi == stokAdi);
                    if (stok != null)
                    {
                        await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(stok));
                    }
                }
                if (stok == null)
                    return $"{stokAdi} - Ürün bulunamadı.";

                if (stok.StokSayisi <= 0)
                    return $"{stokAdi} - Stok kalmadı.";

                var stokFromDb = await _context.Stoks.FirstOrDefaultAsync(s => s.StokId == stokId);
                if (stokFromDb == null || stokFromDb.StokSayisi <= 0)
                    return $"{stokAdi} - Stok kalmadı.";

                stokFromDb.StokSayisi--;
                await _context.SaveChangesAsync();

                stok.StokSayisi = stokFromDb.StokSayisi;
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(stok));

                return $"{stokAdi} - Sipariş alındı. Kalan stok: {stok.StokSayisi}";
            }
        }
        public async Task<List<Stok>> GetAllAsync()
        {
            return await _context.Stoks.ToListAsync();
        }
        public async Task<Stok?> GetByIdAsync(int id)
        {
            return await _context.Stoks.FirstOrDefaultAsync(e => e.StokId == id);
        }
        public async Task<Stok?> CreateAsync(Stok StokModel)
        {
            await _context.Stoks.AddAsync(StokModel);
            await _context.SaveChangesAsync();
            return StokModel;
        }

        public async Task<Stok?> UpdateAsync(int id, UpdateStokRequestDto StokDto)
        {
            var existingAdmin = await _context.Stoks.FindAsync(id);
            if (existingAdmin == null)
            {
                return null;
            }
            existingAdmin.StokAdi = StokDto.StokAdi;
            existingAdmin.StokSayisi = StokDto.StokSayisi;

            await _context.SaveChangesAsync();
            return existingAdmin;
        }

        public async Task<Stok?> DeleteAsync(int id)
        {
            var adminModel = await _context.Stoks.FirstOrDefaultAsync(x => x.StokId == id);
            if (adminModel == null)
            {
                return null;
            }
            _context.Stoks.Remove(adminModel);
            await _context.SaveChangesAsync();
            return adminModel;
        }
    }
}
