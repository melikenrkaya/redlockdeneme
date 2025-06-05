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
        private readonly ISepet _sepetServices;

        public StokServices(ApplicationDBContext context, IDatabase redisDb, IDistributedLockFactory lockFactory, ISepet sepetServicea)
        {
            _context = context;
            _redisDb = redisDb;
            _lockFactory = lockFactory;
            _sepetServices = sepetServicea;
        }

        public async Task<string> SiparisVerAsync(int stokId, string stokAdi, int quantity)
        {
            string lockKey = $"lock:stok:{stokId}";

            using (var redLock = await _lockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(10)))
            {
                if (!redLock.IsAcquired)
                    return $"{stokAdi} - Kilit alınamadı. Lütfen tekrar deneyin.";

                string cacheKey = $"stok:{stokId}";
                Stok? stok = null;

                // Redis kontrolü
                if (await _redisDb.KeyExistsAsync(cacheKey))
                {
                    var cachedValue = await _redisDb.StringGetAsync(cacheKey);
                    stok = JsonSerializer.Deserialize<Stok>(cachedValue);
                }
                else
                {
                    stok = await _context.Stoks.FirstOrDefaultAsync(s => s.StokId == stokId && s.StokAdi == stokAdi);
                    if (stok != null)
                        await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(stok));
                }

                if (stok == null)
                    return $"{stokAdi} - Ürün bulunamadı.";

                if (stok.StokSayisi < quantity)
                    return $"{stokAdi} - Yeterli stok yok. Kalan: {stok.StokSayisi}, İstenen: {quantity}";


                // Güncel veriyle doğrulama
                var stokFromDb = await _context.Stoks.FirstOrDefaultAsync(s => s.StokId == stokId);

                if (stokFromDb == null || stokFromDb.StokSayisi < quantity)
                    return $"{stokAdi} - Stok kalmadı veya yetersiz.";


                // Stok düşürme kısmından önce gecikme:
                await Task.Delay(5000); // 5 saniye bekle


                // Stok düşürme
                stokFromDb.StokSayisi -= quantity;
                await _context.SaveChangesAsync();

                // Redis güncelleme
                stok.StokSayisi = stokFromDb.StokSayisi;
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(stok));
                
                if (stok.StokSayisi == 0)
                {
                    await _sepetServices.SepetiTemizleAsync(stokId);

                }

                return $"{stokAdi} - {quantity} adet sipariş alındı. Kalan stok: {stok.StokSayisi}";
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

        public async Task<Stok?> CreateAsync(Stok stokModel)
        {
            await _context.Stoks.AddAsync(stokModel);
            await _context.SaveChangesAsync();

            // Redis'e ekleme
            string cacheKey = $"stok:{stokModel.StokId}";
            await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(stokModel));

            return stokModel;
        }

        public async Task<Stok?> UpdateAsync(int id, UpdateStokRequestDto stokDto)
        {
            var existingStok = await _context.Stoks.FindAsync(id);
            if (existingStok == null)
                return null;

            existingStok.StokAdi = stokDto.StokAdi;
            existingStok.StokSayisi = stokDto.StokSayisi;
            await _context.SaveChangesAsync();

            // Redis güncelleme
            string cacheKey = $"stok:{existingStok.StokId}";
            await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(existingStok));

            return existingStok;
        }

        public async Task DeleteAsync(int id)
        {
            var stok = await _context.Stoks.FindAsync(id);

            if (stok == null)
                throw new Exception("Stok bulunamadı");

            // 1️⃣: Sepet tablosunda bu ürünü kullanan kayıtları bul
            var sepetler = await _context.Sepets
                .Where(s => s.UrunId == id)
                .ToListAsync();

            // 2️⃣: İlişkili sepet verilerini sil
            if (sepetler.Any())
                _context.Sepets.RemoveRange(sepetler);

            // 3️⃣: Son olarak stok verisini sil
            _context.Stoks.Remove(stok);

            // 4️⃣: Değişiklikleri kaydet
            await _context.SaveChangesAsync();
        }

    }
}
