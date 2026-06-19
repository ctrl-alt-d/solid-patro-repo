namespace LogicaDeNegoci.Abstractions.Projeccions;

public class ProjeccioAlumne
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Curs { get; set; }
    public bool EstudisFinalitzats { get; set; }
}
