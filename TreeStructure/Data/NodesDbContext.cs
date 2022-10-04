using Microsoft.EntityFrameworkCore;
using TreeStructure.Models;

namespace TreeStructure.Data;

public class NodesDbContext : DbContext
{
    public NodesDbContext(DbContextOptions<NodesDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Node> Nodes => Set<Node>();
}