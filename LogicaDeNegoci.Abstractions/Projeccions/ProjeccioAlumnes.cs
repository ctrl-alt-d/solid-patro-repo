namespace LogicaDeNegoci.Abstractions.Projeccions;

public class ProjeccioAlumnes
{
    public IReadOnlyCollection<ProjeccioAlumne> Alumnes { get; set; } = Array.Empty<ProjeccioAlumne>();
}
