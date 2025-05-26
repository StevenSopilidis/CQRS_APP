using Microsoft.EntityFrameworkCore;

namespace Post.Query.Infrastructure.DataAccess
{
    public class DatabaseContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configDbContext;

        public DatabaseContextFactory(Action<DbContextOptionsBuilder> configDbContext)
        {
            _configDbContext = configDbContext;
        }

        public DatabaseContext CreateDbContext() {
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new();
            _configDbContext(optionsBuilder);

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}