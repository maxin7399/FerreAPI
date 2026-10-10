using FerreAPI.Categorias;
using FerreAPI.Comun;
using FerreAPI.Data;
using FerreAPI.Facturas;
using FerreAPI.Productos;
using FerreAPI.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<FerreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FerreApi")));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalExcepciones>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapProductos();
app.MapCategorias();
app.MapFacturas();
await app.CrearAdministradorInicialAsync();
app.Run();
