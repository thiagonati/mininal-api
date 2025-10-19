using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MInimalApi.DTOs;
using MInimalApi.Infraestrutura.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContexto>(
    options => options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    )
);
var app = builder.Build();

app.MapGet("/", () => "hello world");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@example.com" && loginDTO.Senha == "123456")
    {
        return Results.Ok("Login efetuado com sucesso");
    }
    else
        return Results.BadRequest("Login inválido");
});

app.Run();
