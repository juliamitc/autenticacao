using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public interface IRepository<T>
    {
        T Adicionar(T entidade);
        T Alterar(T entidade);
        T Excluir(T entidade);
        IList<T> Get();
    }
}
