namespace DbModels;

public class Alumne
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public string Email { get; set; } = "";
    public int Curs { get; set; } = 1;
    public bool EstudisFinalitzats { get; set; } = false;
}
