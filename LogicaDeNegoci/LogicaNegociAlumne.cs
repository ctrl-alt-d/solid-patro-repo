using LogicaDeNegoci.Abstractions;
using LogicaDeNegoci.Abstractions.Parametres;
using LogicaDeNegoci.Abstractions.Projeccions;
using DbModels;
using Repositori.Abstractions;

namespace LogicaDeNegoci;

public class LogicaNegociAlumne(IRepositoriAlumne repositori) : ILogicaNegociAlumne
{
    public async Task<ProjeccioAlumne> AfegirAsync(AfegirAlumneParametres parametres)
    {
        var alumne = new Alumne
        {
            Nom = parametres.Nom,
            Email = parametres.Email,
            Curs = 1,
            EstudisFinalitzats = false,
        };

        await repositori.AfegirAlumneAsync(alumne);

        return new ProjeccioAlumne
        {
            Id = alumne.Id,
            Nom = alumne.Nom,
            Email = alumne.Email,
            Curs = alumne.Curs,
            EstudisFinalitzats = alumne.EstudisFinalitzats,
        };
    }

    public async Task<ProjeccioAlumne> CanviarDadesAsync(CanviarDadesAlumneParametres parametres)
    {
        var alumne = await repositori.ObtenirPerIdAsync(parametres.Id)
            ?? throw new InvalidOperationException($"No s'ha trobat l'alumne amb id {parametres.Id}.");

        alumne.Nom = parametres.Nom;
        alumne.Email = parametres.Email;

        await repositori.ActualitzarAlumneAsync(alumne);

        return new ProjeccioAlumne
        {
            Id = alumne.Id,
            Nom = alumne.Nom,
            Email = alumne.Email,
            Curs = alumne.Curs,
            EstudisFinalitzats = alumne.EstudisFinalitzats,
        };
    }

    public Task EliminarAsync(EliminarAlumneParametres parametres)
    {
        return repositori.EsborrarAlumneAsync(parametres.Id);
    }

    public async Task<ProjeccioAlumne> PromocionarAsync(PromocionarAlumneParametres parametres)
    {
        var alumne = await repositori.ObtenirPerIdAsync(parametres.Id)
            ?? throw new InvalidOperationException($"No s'ha trobat l'alumne amb id {parametres.Id}.");

        if (alumne.Curs < 3)
        {
            alumne.Curs++;
        }
        else
        {
            alumne.EstudisFinalitzats = true;
        }

        await repositori.ActualitzarAlumneAsync(alumne);

        return new ProjeccioAlumne
        {
            Id = alumne.Id,
            Nom = alumne.Nom,
            Email = alumne.Email,
            Curs = alumne.Curs,
            EstudisFinalitzats = alumne.EstudisFinalitzats,
        };
    }

    public async Task<ProjeccioAlumnes> SeleccionarTotsAsync(SeleccionarTotsAlumnesParametres parametres)
    {
        var alumnes = await repositori.LlistaAlumnesAsync();

        return new ProjeccioAlumnes
        {
            Alumnes = alumnes.Select(alumne => new ProjeccioAlumne
            {
                Id = alumne.Id,
                Nom = alumne.Nom,
                Email = alumne.Email,
                Curs = alumne.Curs,
                EstudisFinalitzats = alumne.EstudisFinalitzats,
            }).ToArray(),
        };
    }
}
