using LogicaDeNegoci;
using LogicaDeNegoci.Abstractions;
using Microsoft.EntityFrameworkCore;
using Repositori;
using Repositori.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AlumnesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Alumnes")));

builder.Services.AddScoped<IRepositoriAlumne, RepositoriAlumne>();
builder.Services.AddScoped<ILogicaNegociAlumne, LogicaNegociAlumne>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AlumnesDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
