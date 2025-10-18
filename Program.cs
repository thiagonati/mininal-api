using MInimalApi.DTOs;

var builder = WebApplication.CreateBuilder(args);
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
