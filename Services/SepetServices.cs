using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RedlockDeneme.Common.Extensions;
using RedlockDeneme.Data.Context;
using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;
using RedLockNet;
using StackExchange.Redis;
using System.Text.Json;

namespace RedlockDeneme.Services
{
    public class SepetServices : ISepet
    {
        private readonly ApplicationDBContext _context;
        private readonly IDatabase _redisDb;
        private readonly IDistributedLockFactory _lockFactory;

        public SepetServices(ApplicationDBContext context, IDatabase redisDb, IDistributedLockFactory lockFactory)
        {
            _context = context;
            _redisDb = redisDb;
            _lockFactory = lockFactory;
        }
        public async Task SepetiTemizleAsync(int urunId)
        {
            var endPoints = _redisDb.Multiplexer.GetEndPoints();
            var server = _redisDb.Multiplexer.GetServer(endPoints.First());
            var keys = server.Keys(pattern: "sepet:*").ToArray();

            foreach (var key in keys)
            {
                var sepetJson = await _redisDb.StringGetAsync(key);
                Console.WriteLine(sepetJson);
                var sepetList = JsonSerializer.Deserialize<Sepet>(sepetJson);
                Console.WriteLine(sepetList);
                Console.WriteLine($"Key: {key}, Redis UrunId: {sepetList?.UrunId}, Aranan UrunId: {urunId}");



                if (sepetList != null && sepetList.UrunId == urunId)
                {
                    // Redis'ten sil
                    await _redisDb.KeyDeleteAsync(key);
                }
            }

            // Aynı ürüne ait tüm Sepet'leri veritabanından sil
            var dbSepets = await _context.Sepets
                .Where(s => s.UrunId == urunId)
                .ToListAsync();

            if (dbSepets.Any())
            {
                _context.Sepets.RemoveRange(dbSepets);
                await _context.SaveChangesAsync();
            
        }
        }

        public async Task<List<SepetWithUrunDTO>> GetAllAsync()
        {
            var sepetEntities = await _context.Sepets.Include(s => s.Urun).ToListAsync();
            var sepetDto = sepetEntities.Select(s => s.ToSepetUrunDto()).ToList();

            return sepetDto;
        }


        public async Task<Sepet?> GetByIdAsync(int id)
        {
            return await _context.Sepets.FirstOrDefaultAsync(e => e.UrunId == id);
        }
        public async Task<SepetWithUrunDTO?> CreateAsync(Sepet SepetModel)
        {
            var stok = await _context.Stoks.FirstOrDefaultAsync(x => x.StokId == SepetModel.UrunId);

            if (stok == null || stok.StokSayisi <= 0)
            {
                // Sepete ekleme, null dön
                return null;
            }
            SepetModel.UrunId = stok.StokId;

            await _context.Sepets.AddAsync(SepetModel);
            await _context.SaveChangesAsync();

            // Redis'e ekleme
            string cacheKey = $"sepet:{SepetModel.UrunId}";
            await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(SepetModel));

            return SepetModel.ToSepetUrunDto();
        }
        public async Task<SepetWithUrunDTO?> UpdateAsync(int id, UpdateSepetRequestDTO SepetDto)
        {
            var existingSepet = await _context.Sepets.FindAsync(id);
            if (existingSepet == null)
                return null;

            existingSepet.UrunAdi = SepetDto.UrunAdi;
            existingSepet.Adet = SepetDto.Adet;
            await _context.SaveChangesAsync();

            // Redis güncelleme
            string cacheKey = $"stok:{existingSepet.UrunId}";
            await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(existingSepet));

            return existingSepet.ToSepetUrunDto();
        }
        public async Task<List<SepetWithUrunDTO>> TemizleStoguBitmisUrunleriAsync()
        {
            // 1. Sepet tablosundaki ürünlerin stok bilgilerini çek (stok sayısı 0 olanlar)
            var tukenenSepetler = await _context.Sepets
                .Include(s => s.Urun)
                .Where(s => s.Urun.StokSayisi == 0)
                .ToListAsync();

            if (!tukenenSepetler.Any())
            {
                // Silinecek yoksa mevcut sepeti DTO olarak döndür
                var current = await _context.Sepets.Include(s => s.Urun).ToListAsync();
                return current.Select(s => s.ToSepetUrunDto()).ToList();
            }

            // 2. Redis'ten sil
            foreach (var sepet in tukenenSepetler)
            {
                string redisKey = $"sepet:{sepet.SepetId}";
                await _redisDb.KeyDeleteAsync(redisKey);
            }

            // 3. Veritabanından sil
            _context.Sepets.RemoveRange(tukenenSepetler);
            await _context.SaveChangesAsync();

            // 4. Kalan sepetleri DTO olarak döndür
            var kalanSepetler = await _context.Sepets.Include(s => s.Urun).ToListAsync();
            return kalanSepetler.Select(s => s.ToSepetUrunDto()).ToList();
        }


    }
}
