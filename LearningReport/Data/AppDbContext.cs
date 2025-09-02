using LearningReport.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningReport.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

    }
}
