using System.Data.Common;
using MInimalApi.Dominio.Entidades;
using MInimalApi.Dominio.Interfaces;
using MInimalApi.DTOs;
using MInimalApi.Infraestrutura.Db;

namespace MInimalApi.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly DbContexto _contexto;
        public VeiculoServico(DbContexto db)
        {
            _contexto = db;
        }

        public void Apagar(int id)
        {
            _contexto.Veiculos.Remove(_contexto.Veiculos.Find(id)!);
            _contexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscarPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            IQueryable<Veiculo> query = _contexto.Veiculos;


            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => v.Nome.ToLower().Contains(nome.ToLower()));
            }

            if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(v => v.Marca.ToLower().Contains(marca.ToLower()));
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