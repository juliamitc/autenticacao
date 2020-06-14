using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class UsuarioSolicitacao
    {
        public int Id { get; set; }
        public virtual Usuario Usuario { get; set; }
        public int UsuarioId { get; set; }
        public DateTime ExpiraEm { get; set; }
        [JsonIgnore]
        public string SenhaTemporaria { get; set; }
    }
}
