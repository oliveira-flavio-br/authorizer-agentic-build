using Microsoft.EntityFrameworkCore;

namespace Authorizer.Infrastructure.Data;

/// <summary>
/// Main database context for the Authorizer application.
/// Currently minimal - DbSets will be added in Phase 1 as domain models are implemented.
/// </summary>
public class AuthorizerDbContext : DbContext
{
    public AuthorizerDbContext(DbContextOptions<AuthorizerDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Model configurations will be added in Phase 1
    }
}

