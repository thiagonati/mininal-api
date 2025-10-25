using MInimalApi.Dominio.Entidades;
using MInimalApi.DTOs;

namespace MInimalApi.Dominio.Interfaces
{
    public interface IAdministradorServico
    {

        Administrador? Login(LoginDTO loginDTO);
        List<Administrador> Todos(int? pagia = 1, string? email = null);

        Administrador? BuscarPorId(int id);

        void Incluir(Administrador veiculo);

        void Atualizar(Administrador veiculo);

        void Apagar(int id);
    }
}