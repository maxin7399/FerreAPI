using FerreAPI.Auth;
using FerreAPI.Categorias;
using FerreAPI.Comun;
using FerreAPI.Data;
using FerreAPI.Facturas;
using FerreAPI.Productos;
using FerreAPI.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<FerreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FerreApi")));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalExcepciones>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.Configure<JwtOpciones>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<GeneradorTokens>();
builder.Services.AddValidation();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Politicas.Personal, p =>
        p.RequireRole(nameof(Rol.Vendedor), nameof(Rol.Administrador)))
    .AddPolicy(Politicas.SoloAdministrador, p =>
        p.RequireRole(nameof(Rol.Administrador)))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference().AllowAnonymous();
app.MapProductos();
app.MapCategorias();
app.MapFacturas();
app.MapAuth();
app.MapUsuarios();
await app.CrearAdministradorInicialAsync();
app.Run();

public partial class Program { }