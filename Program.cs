using FerreAPI.Data;
using FerreAPI.Productos;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ProductoStore>();
builder.Services.AddDbContext<FerreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FerreApi")));

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();   // UI en /scalar
app.MapProductos();            // extension method que tú vas a crear
app.Run();
