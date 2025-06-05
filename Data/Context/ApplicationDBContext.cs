using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RedlockDeneme.Data.Entity;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace RedlockDeneme.Data.Context
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
      : base(dbContextOptions)
        {
        }

        public DbSet<Stok> Stoks { get; set; }  // Stok Entity örneği
        public DbSet<Sepet> Sepets { get; set; }  // Sepet Entity örneği


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Sepet>()
           .HasOne(s => s.Urun)
           .WithMany() // Sepetlerin listesi Stok sınıfında tutulmuyorsa boş bırakılır
           .HasForeignKey(s => s.UrunId)
           .OnDelete(DeleteBehavior.Restrict);

        }
    }

}
