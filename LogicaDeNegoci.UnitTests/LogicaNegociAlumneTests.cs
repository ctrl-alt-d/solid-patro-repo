using LogicaDeNegoci.Abstractions;
using LogicaDeNegoci.Abstractions.Parametres;
using LogicaDeNegoci.Abstractions.Projeccions;
using LogicaDeNegoci;
using DbModels;
using FluentAssertions;
using NSubstitute;
using Repositori.Abstractions;

namespace LogicaDeNegoci.UnitTests;

public class LogicaNegociAlumneTests
{
	private readonly IRepositoriAlumne repositori = Substitute.For<IRepositoriAlumne>();
	private readonly ILogicaNegociAlumne sut;

	public LogicaNegociAlumneTests()
	{
		sut = new LogicaNegociAlumne(repositori);
	}

	[Fact]
	public async Task AfegirAsync_Construeix_Alumne_I_Retorna_Projeccio()
	{
		Alumne? alumneCreat = null;
		repositori.AfegirAlumneAsync(Arg.Do<Alumne>(alumne => alumneCreat = alumne))
			.Returns(Task.CompletedTask);

		var resultat = await sut.AfegirAsync(new AfegirAlumneParametres
		{
			Nom = "Ada",
			Email = "ada@demo.cat",
		});

		alumneCreat.Should().NotBeNull();
		alumneCreat!.Nom.Should().Be("Ada");
		alumneCreat.Email.Should().Be("ada@demo.cat");
		alumneCreat.Curs.Should().Be(1);
		alumneCreat.EstudisFinalitzats.Should().BeFalse();

		resultat.Should().BeEquivalentTo(new ProjeccioAlumne
		{
			Id = 0,
			Nom = "Ada",
			Email = "ada@demo.cat",
			Curs = 1,
			EstudisFinalitzats = false,
		});
	}

	[Fact]
	public async Task CanviarDadesAsync_Actualitza_I_Retorna_Projeccio()
	{
		var alumne = new Alumne
		{
			Id = 7,
			Nom = "Ada",
			Email = "ada@demo.cat",
			Curs = 2,
			EstudisFinalitzats = false,
		};

		repositori.ObtenirPerIdAsync(7).Returns(alumne);

		var resultat = await sut.CanviarDadesAsync(new CanviarDadesAlumneParametres
		{
			Id = 7,
			Nom = "Ada Lovelace",
			Email = "ada.lovelace@demo.cat",
		});

		await repositori.Received(1).ActualitzarAlumneAsync(Arg.Is<Alumne>(a =>
			a.Id == 7 &&
			a.Nom == "Ada Lovelace" &&
			a.Email == "ada.lovelace@demo.cat" &&
			a.Curs == 2 &&
			!a.EstudisFinalitzats));

		resultat.Should().BeEquivalentTo(new ProjeccioAlumne
		{
			Id = 7,
			Nom = "Ada Lovelace",
			Email = "ada.lovelace@demo.cat",
			Curs = 2,
			EstudisFinalitzats = false,
		});
	}

	[Fact]
	public async Task EliminarAsync_Delega_Al_Repositori()
	{
		await sut.EliminarAsync(new EliminarAlumneParametres { Id = 3 });

		await repositori.Received(1).EsborrarAlumneAsync(3);
	}

	[Fact]
	public async Task SeleccionarPerIdAsync_Retorna_La_Projeccio()
	{
		repositori.ObtenirPerIdAsync(5).Returns(new Alumne
		{
			Id = 5,
			Nom = "Ada",
			Email = "ada@demo.cat",
			Curs = 2,
			EstudisFinalitzats = false,
		});

		var resultat = await sut.SeleccionarPerIdAsync(new SeleccionarPerIdAlumneParametres { Id = 5 });

		resultat.Should().BeEquivalentTo(new ProjeccioAlumne
		{
			Id = 5,
			Nom = "Ada",
			Email = "ada@demo.cat",
			Curs = 2,
			EstudisFinalitzats = false,
		});
	}

	[Fact]
	public async Task PromocionarAsync_Delega_I_Retorna_Projeccio_Actualitzada()
	{
		repositori.ObtenirPerIdAsync(5).Returns(new Alumne
		{
			Id = 5,
			Nom = "Ada",
			Email = "ada@demo.cat",
			Curs = 3,
			EstudisFinalitzats = false,
		});

		var resultat = await sut.PromocionarAsync(new PromocionarAlumneParametres { Id = 5 });

		await repositori.Received(1).ActualitzarAlumneAsync(Arg.Is<Alumne>(a =>
			a.Id == 5 &&
			a.Nom == "Ada" &&
			a.Email == "ada@demo.cat" &&
			a.Curs == 3 &&
			a.EstudisFinalitzats));
		resultat.Curs.Should().Be(3);
		resultat.Id.Should().Be(5);
		resultat.EstudisFinalitzats.Should().BeTrue();
	}

	[Fact]
	public async Task SeleccionarTotsAsync_Retorna_Totes_Les_Projeccions()
	{
		repositori.LlistaAlumnesAsync().Returns(new[]
		{
			new Alumne { Id = 1, Nom = "Ada", Email = "ada@demo.cat", Curs = 1 },
			new Alumne { Id = 2, Nom = "Alan", Email = "alan@demo.cat", Curs = 4, EstudisFinalitzats = true },
		});

		var resultat = await sut.SeleccionarTotsAsync(new SeleccionarTotsAlumnesParametres());

		resultat.Alumnes.Should().BeEquivalentTo(new[]
		{
			new ProjeccioAlumne { Id = 1, Nom = "Ada", Email = "ada@demo.cat", Curs = 1, EstudisFinalitzats = false },
			new ProjeccioAlumne { Id = 2, Nom = "Alan", Email = "alan@demo.cat", Curs = 4, EstudisFinalitzats = true },
		});
	}
}
