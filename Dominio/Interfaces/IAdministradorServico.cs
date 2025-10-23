using MInimalApi.Dominio.Entidades;
using MInimalApi.DTOs;

namespace MInimalApi.Dominio.Interfaces
{
    public interface IAdministradorServico
    {

        Administrador? Login(LoginDTO loginDTO);
    }
}