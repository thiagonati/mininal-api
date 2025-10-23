using System.Data.Common;
using MInimalApi.Dominio.Entidades;
using MInimalApi.Dominio.Interfaces;
using MInimalApi.DTOs;
using MInimalApi.Infraestrutura.Db;

namespace MInimalApi.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto db)
        {
            _contexto = db;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).First();
            return adm;
        }
    }
}