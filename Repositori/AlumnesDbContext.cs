using DbModels;
using Microsoft.EntityFrameworkCore;

namespace Repositori;

public class AlumnesDbContext(DbContextOptions<AlumnesDbContext> options) : DbContext(options)
{
    public DbSet<Alumne> Alumnes { get; set; }
}
