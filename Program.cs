using FerreAPI.Productos;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ProductoStore>();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();   // UI en /scalar
app.MapProductos();            // extension method que tú vas a crear
app.Run();
