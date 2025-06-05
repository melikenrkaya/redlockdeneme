using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RedlockDeneme.Data.Entity
{
    public class Sepet
    {
        public int SepetId { get; set; }
        [JsonIgnore]  // <-- dışarı gönderilmesin

        public int UrunId { get; set; }
        [JsonIgnore]  // <-- dışarı gönderilmesin

        public string UrunAdi { get; set; }
        public Stok Urun { get; set; } // navigation property
        public int Adet { get; set; }

    }
}
