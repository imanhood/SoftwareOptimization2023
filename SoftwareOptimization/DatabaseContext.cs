using Microsoft.EntityFrameworkCore;
using SoftwareOptimization.Models.Entities;
using System.Threading.Tasks;

namespace SoftwareOptimization {
    public class DatabaseContext : DbContext {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) {}
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public new async Task<int> SaveChanges() {
            return await base.SaveChangesAsync();
        }
    }
}
