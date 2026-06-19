using LogicaDeNegoci.Abstractions.Parametres;
using LogicaDeNegoci.Abstractions.Projeccions;

namespace LogicaDeNegoci.Abstractions;

public interface ILogicaNegociAlumne
{
	Task<ProjeccioAlumne> AfegirAsync(AfegirAlumneParametres parametres);
	Task<ProjeccioAlumne> CanviarDadesAsync(CanviarDadesAlumneParametres parametres);
	Task EliminarAsync(EliminarAlumneParametres parametres);
	Task<ProjeccioAlumne> SeleccionarPerIdAsync(SeleccionarPerIdAlumneParametres parametres);
	Task<ProjeccioAlumnes> SeleccionarTotsAsync(SeleccionarTotsAlumnesParametres parametres);
	Task<ProjeccioAlumne> PromocionarAsync(PromocionarAlumneParametres parametres);
}
