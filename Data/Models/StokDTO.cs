namespace RedlockDeneme.Data.Models
{
    public class StokDTO
    {
        public int StokId { get; set; }
        public string StokAdi { get; set; } = string.Empty;
        public int StokSayisi { get; set; }
    }
    public class CreateStokRequestDto
    {
        public string StokAdi { get; set; } = string.Empty;
        public int StokSayisi { get; set; }

    }
    public class UpdateStokRequestDto
    {
        public string StokAdi { get; set; } = string.Empty;
        public int StokSayisi { get; set; }

    }
}

