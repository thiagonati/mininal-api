using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MInimalApi.Dominio.DTOs;
using MInimalApi.Dominio.Entidades;
using MInimalApi.Dominio.Interfaces;
using MInimalApi.Dominio.ModelViews;
using MInimalApi.Dominio.Servicos;
using MInimalApi.DTOs;
using MInimalApi.Infraestrutura.Db;

#region builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login efetuado com sucesso");
    }
    else
        return Results.BadRequest("Login inválido");
}).WithTags("Administradores");
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
}).WithTags("Veiculos"); ;

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromQuery] int id, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.BuscarPorId(id);

    if (veiculos == null) return Results.NotFound();

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

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
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromQuery] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);

    if (veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(id);

    return Results.NoContent();

}).WithTags("Veiculos");
#endregion

#region app
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
#endregion
