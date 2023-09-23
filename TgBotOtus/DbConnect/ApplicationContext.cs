using Microsoft.EntityFrameworkCore;
using TgBotOtus.Models;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace TgBotOtus
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<OtusUsers> OtusUsers { get; set; } = null!;
        public DbSet<Dishes> Dishes { get; set; } = null!;
        public DbSet<DietName> DietName { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=.\SQLEXPRESS;Database=TgDB;Trusted_Connection=True;TrustServerCertificate=True");
           // optionsBuilder.UseSqlServer(@"Data Source=.\SQLEXPRESS;Initial Catalog=TgDB;Integrated Security=True");
        }
    }
}
