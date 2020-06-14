using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Repository;

namespace WebApplication1.Models
{
    public class UsuarioSolicitacaoRepository : IUsuarioSolicitacaoRepository
    {
        private readonly ApplicationContext applicationContext;

        public UsuarioSolicitacaoRepository(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public UsuarioSolicitacao Adicionar(UsuarioSolicitacao entidade)
        {
            applicationContext.Set<UsuarioSolicitacao>().Add(entidade);
            applicationContext.SaveChanges();

            return entidade;
        }

        public UsuarioSolicitacao Alterar(UsuarioSolicitacao entidade)
        {
            applicationContext.Set<UsuarioSolicitacao>().Update(entidade);
            applicationContext.SaveChanges();

            return entidade;
        }

        public UsuarioSolicitacao Excluir(UsuarioSolicitacao entidade)
        {
            applicationContext.Set<UsuarioSolicitacao>().Remove(entidade);
            applicationContext.SaveChanges();

            return entidade;
        }

        public IList<UsuarioSolicitacao> Get()
        {
            return applicationContext.Set<UsuarioSolicitacao>().ToList();
        }
    }
}
