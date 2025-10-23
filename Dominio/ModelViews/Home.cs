namespace MInimalApi.Dominio.ModelViews;

public struct Home
{
    public string Mensagem
    {
        get => "Bem vindo a API de veiculos - MInimalApi";
    }
    public string Documentacao
    {
        get => "/swagger";
    }
}