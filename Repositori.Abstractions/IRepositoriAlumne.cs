using DbModels;

namespace Repositori.Abstractions;

public interface IRepositoriAlumne
{
    Task<Alumne?> ObtenirPerIdAsync(int id);
    Task<IEnumerable<Alumne>> LlistaAlumnesAsync();
    Task AfegirAlumneAsync(Alumne alumne);
    Task ActualitzarAlumneAsync(Alumne alumne);
    Task EsborrarAlumneAsync(int id);

}
