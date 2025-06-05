using RedlockDeneme.Data.Entity;

namespace RedlockDeneme.Data.Models
{
    public class SepetDTO
    {
        public int SepetId { get; set; }
        public int UrunId { get; set; }
        public string UrunAdi { get; set; }
        public int Adet { get; set; }
    }
    public class SepetWithUrunDTO
    {
        public int SepetId { get; set; }
        public int Adet { get; set; }
        public StokDTO Urun { get; set; }
    }

    public class CreateSepetRequestDTO
    {
        public int UrunId { get; set; }
        public string UrunAdi { get; set; }
        public int Adet { get; set; }
    }
    public class UpdateSepetRequestDTO
    {
        public int UrunId { get; set; }

        public string UrunAdi { get; set; }
        public int Adet { get; set; }

    }
}

