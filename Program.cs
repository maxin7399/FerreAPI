using FerreAPI.Categorias;
using FerreAPI.Data;
using FerreAPI.Facturas;
using FerreAPI.Productos;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<FerreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FerreApi")));

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapProductos();
app.MapCategorias();
app.MapFacturas();
app.Run();
