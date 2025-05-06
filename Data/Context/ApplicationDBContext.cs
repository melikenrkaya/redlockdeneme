using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RedlockDeneme.Data.Entity;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace RedlockDeneme.Data.Context
{
    public class ApplicationDBContext :DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
      : base(dbContextOptions)
        {
        }

        public DbSet<Stok> Stoks { get; set; }  // Stok Entity örneği

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}


