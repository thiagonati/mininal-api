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

        public void Apagar(int id)
        {
            _contexto.Administradores.Remove(_contexto.Administradores.Find(id)!);
            _contexto.SaveChanges();
        }

        public void Atualizar(Administrador administrador)
        {
            _contexto.Administradores.Update(administrador);
            _contexto.SaveChanges();
        }

        public Administrador? BuscarPorId(int id)
        {
            return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public List<Administrador> Todos(int? pagina = 1, string? email = null)
        {
            IQueryable<Administrador> query = _contexto.Administradores;

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(v => v.Email.ToLower().Contains(email.ToLower()));
            }

            int tamanhoPagina = 10;

            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * tamanhoPagina).Take(tamanhoPagina);
            }
            return
                query.ToList();
        }
    }
}