using Microsoft.AspNetCore.DataProtection;
using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;
using RedlockDeneme.Services;

namespace RedlockDeneme.Common.Extensions
{
    public static class SepetExten
    {
        public static SepetDTO ToSepetDto(this Sepet SepetModel)
        {
            return new SepetDTO
            {
                SepetId = SepetModel.SepetId,
                Adet = SepetModel.Adet

            };
        }
        public static SepetWithUrunDTO ToSepetUrunDto(this Sepet sepet)
        {
            return new SepetWithUrunDTO
            {
                SepetId = sepet.SepetId,
                Adet = sepet.Adet,
                Urun = sepet.Urun == null ? null : new StokDTO
                {
                    StokId = sepet.Urun.StokId,
                    StokAdi = sepet.Urun.StokAdi,
                    StokSayisi = sepet.Urun.StokSayisi
                }

            };
        }
        public static Sepet ToSepetFromCreatedDTO(this CreateSepetRequestDTO createsepetDto)
        {
            return new Sepet
            {
                UrunId = createsepetDto.UrunId,
                UrunAdi = createsepetDto.UrunAdi,
                Adet = createsepetDto.Adet
            };
        }
    }
}
