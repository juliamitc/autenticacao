using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [JsonIgnore]
        public string Senha { get; set; }
        public virtual IList<UsuarioSolicitacao> UsuarioSolicitacaos { get; set; }
    }
}
