using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FilmotekaAPI.Data;

// Used only by EF Core CLI tools (dotnet ef migrations add).
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=filmoteka;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(opts);
    }
}
