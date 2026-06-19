using LogicaDeNegoci.Abstractions;
using LogicaDeNegoci.Abstractions.Parametres;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class AlumnesController(ILogicaNegociAlumne logicaNegoci) : Controller
{
    public async Task<IActionResult> Index()
    {
        var alumnes = await logicaNegoci.SeleccionarTotsAsync(new SeleccionarTotsAlumnesParametres());
        return View(alumnes);
    }

    public async Task<IActionResult> Detalls(int id)
    {
        var alumne = await logicaNegoci.SeleccionarPerIdAsync(new SeleccionarPerIdAlumneParametres { Id = id });
        return View(alumne);
    }

    public IActionResult Crear()
    {
        return View(new AfegirAlumneParametres());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(AfegirAlumneParametres parametres)
    {
        if (!ModelState.IsValid)
        {
            return View(parametres);
        }

        await logicaNegoci.AfegirAsync(parametres);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var alumne = await logicaNegoci.SeleccionarPerIdAsync(new SeleccionarPerIdAlumneParametres { Id = id });

        return View(new CanviarDadesAlumneParametres
        {
            Id = alumne.Id,
            Nom = alumne.Nom,
            Email = alumne.Email,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(CanviarDadesAlumneParametres parametres)
    {
        if (!ModelState.IsValid)
        {
            return View(parametres);
        }

        await logicaNegoci.CanviarDadesAsync(parametres);
        return RedirectToAction(nameof(Detalls), new { id = parametres.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promocionar(int id)
    {
        await logicaNegoci.PromocionarAsync(new PromocionarAlumneParametres { Id = id });
        return RedirectToAction(nameof(Detalls), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await logicaNegoci.EliminarAsync(new EliminarAlumneParametres { Id = id });
        return RedirectToAction(nameof(Index));
    }
}
