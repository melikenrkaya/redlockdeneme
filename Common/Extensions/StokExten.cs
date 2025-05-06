using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;


namespace RedlockDeneme.Common.Extensions
{
    public static class StokExten
    {
        public static StokDTO ToStokDto(this Stok StokModel)
        {
            return new StokDTO
            {
                StokId = StokModel.StokId,
                StokAdi = StokModel.StokAdi,
                StokSayisi = StokModel.StokSayisi
            };
        }
        public static Stok ToStokFromCreatedDTO(this CreateStokRequestDto createstokDto)
        {
            return new Stok
            {
                StokAdi = createstokDto.StokAdi,
                StokSayisi = createstokDto.StokSayisi
            };
        }
    }
}
