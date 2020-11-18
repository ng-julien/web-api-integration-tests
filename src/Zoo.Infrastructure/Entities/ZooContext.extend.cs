namespace Zoo.Infrastructure.Entities
{
    using Microsoft.EntityFrameworkCore;

    using Store;

    public partial class ZooContext : IDbContext
    {
        protected ZooContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
