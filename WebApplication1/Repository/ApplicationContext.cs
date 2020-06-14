using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Repository
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().HasKey(p => p.Id);
            modelBuilder.Entity<UsuarioSolicitacao>().HasKey(p => p.Id);
            
            modelBuilder.Entity<Usuario>().HasMany(p => p.UsuarioSolicitacaos).WithOne(p => p.Usuario).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
