using DbModels;
using Microsoft.EntityFrameworkCore;
using Repositori.Abstractions;

namespace Repositori;

public class RepositoriAlumne(AlumnesDbContext dbContext) : IRepositoriAlumne
{
    public async Task ActualitzarAlumneAsync(Alumne alumne)
    {
        dbContext.Alumnes.Update(alumne);
        await dbContext.SaveChangesAsync();
    }

    public async Task AfegirAlumneAsync(Alumne alumne)
    {
        await dbContext.Alumnes.AddAsync(alumne);
        await dbContext.SaveChangesAsync();
    }

    public async Task EsborrarAlumneAsync(int id)
    {
        var alumne = await dbContext.Alumnes.FirstOrDefaultAsync(a => a.Id == id);
        if (alumne is null)
        {
            return;
        }

        dbContext.Alumnes.Remove(alumne);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Alumne>> LlistaAlumnesAsync()
    {
        return await dbContext.Alumnes.ToListAsync();
    }

    public async Task<Alumne?> ObtenirPerIdAsync(int id)
    {
        return await dbContext.Alumnes.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task PromocionarAlumneAsync(int id)
    {
        var alumne = await dbContext.Alumnes.FirstOrDefaultAsync(a => a.Id == id);
        if (alumne is null)
        {
            return;
        }

        alumne.Curs++;
        if (alumne.Curs > 4)
        {
            alumne.EstudisFinalitzats = true;
        }

        dbContext.Alumnes.Update(alumne);
        await dbContext.SaveChangesAsync();
    }
}
