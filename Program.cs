using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MInimalApi.Dominio.DTOs;
using MInimalApi.Dominio.Entidades;
using MInimalApi.Dominio.Interfaces;
using MInimalApi.Dominio.ModelViews;
using MInimalApi.Dominio.Servicos;
using MInimalApi.DTOs;
using MInimalApi.Infraestrutura.Db;

#region builder
var builder = WebApplication.CreateBuilder(args);

var Key = builder.Configuration["Jwt:Key"].ToString();
if (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"].ToString())) Key = "minimal-api-alunos-vamos-lá-2025";

builder.Services.AddAuthentication(opiton =>
{
    opiton.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opiton.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opiton =>
{
    opiton.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<DbContexto>(
    options => options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    )
);
var app = builder.Build();
#endregion
#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrador
string gerarToken(Administrador administrador)
{
    if (string.IsNullOrEmpty(Key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Email, administrador.Email),
        new Claim(ClaimTypes.Role, administrador.Perfil),
        new Claim("Perfil", administrador.Perfil),
    };
    var token = new JwtSecurityToken(
        claims: claims, expires: DateTime.Now.AddMinutes(1),
        signingCredentials: credentials
        );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return tokenString;
}
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        string token = gerarToken(administradorServico.Login(loginDTO));
        return Results.Ok(new administradorLogado
        {
            Email = administradorServico.Login(loginDTO).Email,
            Perfil = administradorServico.Login(loginDTO).Perfil,
            Token = token
        }
        );
    }
    else
        return Results.BadRequest("Login inválido");
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administradores/{administrador.Id}", administrador);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, [FromQuery] string? email, IAdministradorServico administradorServico) =>
{
    var administradores = administradorServico.Todos(pagina, email);
    return Results.Ok(administradores);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromQuery] int id, IAdministradorServico administradorServico) =>
{
    var administradores = administradorServico.BuscarPorId(id);

    if (administradores == null) return Results.NotFound();

    return Results.Ok(administradores);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");

app.MapPut("/administradores/{id}", ([FromQuery] int id, [FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var administrador = new Administrador
    {
        Id = id,
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil
    };
    administradorServico.Atualizar(administrador);
    return Results.Ok(administrador);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromQuery] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);

    if (administrador == null) return Results.NotFound();

    administradorServico.Apagar(id);
    return Results.NoContent();
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");
#endregion

#region informações do veiculo
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos")
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.RequireAuthorization();

app.MapGet("/veiculos/{id}", ([FromQuery] int id, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.BuscarPorId(id);

    if (veiculos == null) return Results.NotFound();

    return Results.Ok(veiculos);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
.WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromQuery] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var Validacao = new ErrosDeValidacao();

    if (string.IsNullOrEmpty(veiculoDTO.Nome)) Validacao.Mensagens.Add("O campo Nome é obrigatório");

    if (string.IsNullOrEmpty(veiculoDTO.Marca)) Validacao.Mensagens.Add("O campo Marca é obrigatório");

    if (veiculoDTO.Ano < 1950) Validacao.Mensagens.Add("veiculo muito antigo para ser cadastrado");

    if (Validacao.Mensagens.Count > 0) return Results.BadRequest(Validacao);

    var veiculo = new Veiculo
    {
        Id = id,
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromQuery] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);

    if (veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(id);

    return Results.NoContent();

}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "-" })
.WithTags("Veiculos");
#endregion

#region app
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
#endregion
