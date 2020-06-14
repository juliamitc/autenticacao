using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Repository;

namespace WebApplication1.Models
{
    public class UsuarioRepository : IUsuarioRepositorio
    {
        private readonly ApplicationContext applicationContext;

        public UsuarioRepository(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public Usuario Adicionar(Usuario usuario)
        {
            applicationContext.Set<Usuario>().Add(usuario);
            applicationContext.SaveChanges();

            return usuario;
        }

        public Usuario Alterar(Usuario usuario)
        {
            applicationContext.Set<Usuario>().Update(usuario);
            applicationContext.SaveChanges();

            return usuario;
        }

        public Usuario Excluir(Usuario usuario)
        {
            applicationContext.Set<Usuario>().Remove(usuario);
            applicationContext.SaveChanges();

            return usuario;
        }

        public IList<Usuario> Get()
        {
            return applicationContext.Set<Usuario>().ToList();
        }
    }
}
