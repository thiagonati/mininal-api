using MInimalApi.Dominio.Entidades;
using MInimalApi.DTOs;

namespace MInimalApi.Dominio.Interfaces
{
    public interface IVeiculoServico
    {

        List<Veiculo> Todos(int? pagia = 1, string? nome = null, string? marca = null);

        Veiculo? BuscarPorId(int id);

        void Incluir(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);

        void Apagar(int id);

    }
}