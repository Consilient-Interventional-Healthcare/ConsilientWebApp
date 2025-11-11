using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.Migrations.Factories
{
    public class DesignTimeConsilientDbContextFactory : DesignTimeDbContextFactoryBase<ConsilientDbContext>
    {
        protected override ConsilientDbContext InstantiateDbContext(DbContextOptionsBuilder<ConsilientDbContext> optionsBuilder)
        {
            return new ConsilientDbContext(optionsBuilder.Options);
        }
    }
}
