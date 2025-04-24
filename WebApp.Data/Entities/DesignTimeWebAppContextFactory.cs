using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApp.Data.Entities;

public class DesignTimeWebAppContextFactory : IDesignTimeDbContextFactory<WebAppContext>
{
    public WebAppContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WebAppContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=WebApp;Username=postgres;Password=12345");

        return new WebAppContext(optionsBuilder.Options);
    }
}