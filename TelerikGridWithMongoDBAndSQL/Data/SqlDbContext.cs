using Microsoft.EntityFrameworkCore;

namespace TelerikGridWithMongoDBAndSQL.Data
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
           : base(options)
        {
        }
        public DbSet<People> Peoples { get; set; }
    }
}
