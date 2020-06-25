using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AutenticacaoController : Controller
    {
        private readonly IUsuarioRepositorio usuarioRepositorio;
        private readonly IUsuarioSolicitacaoRepository usuarioSolicitacaoRepository;
        private readonly IEmailSender _emailSender;

        public AutenticacaoController(IUsuarioRepositorio usuarioRepositorio, IUsuarioSolicitacaoRepository usuarioSolicitacaoRepository, IEmailSender emailSender)
        {
            this.usuarioRepositorio = usuarioRepositorio;
            this.usuarioSolicitacaoRepository = usuarioSolicitacaoRepository;
            this._emailSender = emailSender;
        }

        public IActionResult Registrar()
        {
            return View();
        }

        public async Task<IActionResult> CriarUsuario(string usuario, string email)
        {
            var usuarioDuplicado = usuarioRepositorio.Get().Where(x => x.Username.ToUpper().Trim() == usuario.ToUpper().Trim()).FirstOrDefault();

            if (usuarioDuplicado != null)
                return BadRequest("Usuario duplicado");

            usuarioDuplicado = usuarioRepositorio.Get().Where(x => x.Email.ToUpper().Trim() == email.ToUpper().Trim()).FirstOrDefault();

            if (usuarioDuplicado != null)
                return BadRequest("Email duplicado");


            Usuario user = new Usuario()
            {
                Username = usuario,
                Email = email
            };

            usuarioRepositorio.Adicionar(user);

            UsuarioSolicitacao usuarioSolicitacao = new UsuarioSolicitacao()
            {
                ExpiraEm = DateTime.Now.AddMinutes(2),
                Usuario = user,
                SenhaTemporaria = Guid.NewGuid().ToString().Substring(5, 8)
            };

            usuarioSolicitacaoRepository.Adicionar(usuarioSolicitacao);

            var sb = new StringBuilder();
            sb.AppendLine($"Olá {user.Username}");
            sb.AppendLine($"\nRealize seu primeiro login com a senha a seguir utilizando o link abaixo antes do tempo de expiração");
            sb.AppendLine($"\nSenha: {usuarioSolicitacao.SenhaTemporaria}");
            sb.AppendLine($"\nExpiração: {usuarioSolicitacao.ExpiraEm.ToString()}");
            sb.AppendLine($"\nLink: http://localhost:53551/Autenticacao/ConfirmarLogin?usuarioSolicitacaoId={usuarioSolicitacao.Id}");

            await _emailSender.SendEmailAsync(user.Email, "Confirmação de cadastro", sb.ToString());

            return Ok();
        }

        public IActionResult ConfirmarLogin(int usuarioSolicitacaoId)
        {
            ViewBag.UsuarioSolicitacao = JsonConvert.SerializeObject(usuarioSolicitacaoRepository.Get().Where(x => x.Id == usuarioSolicitacaoId).FirstOrDefault());

            return View();
        }

        public IActionResult ConfirmarCriacao(int usuarioSolicitacaoId, string usuario, string senha)
        {
            UsuarioSolicitacao usuarioSolicitacao = usuarioSolicitacaoRepository.Get().Where(x => x.Id == usuarioSolicitacaoId).FirstOrDefault();
            Usuario user = null;

            if (usuarioSolicitacao != null)
            {
                user = usuarioRepositorio.Get().Where(x => x.Id == usuarioSolicitacao.UsuarioId).FirstOrDefault();
            }

            if (usuarioSolicitacao == null || usuarioSolicitacao.ExpiraEm < DateTime.Now)
            {
                if (usuarioSolicitacao != null)
                {
                    usuarioRepositorio.Excluir(user);
                    //usuarioSolicitacaoRepository.Excluir(usuarioSolicitacao);
                }

                return NotFound();
            }

            if (usuario != user.Username || usuarioSolicitacao.SenhaTemporaria != senha)
            {
                return Unauthorized();
            }

            return Ok();
        }

        public IActionResult NovaSenha(int usuarioSolicitacaoId)
        {
            ViewBag.UsuarioSolicitacao = JsonConvert.SerializeObject(usuarioSolicitacaoRepository.Get().Where(x => x.Id == usuarioSolicitacaoId).FirstOrDefault());

            return View();
        }

        public async Task<IActionResult> AlterarSenha(int usuarioSolicitacaoId, string senha)
        {
            UsuarioSolicitacao usuarioSolicitacao = usuarioSolicitacaoRepository.Get().Where(x => x.Id == usuarioSolicitacaoId).FirstOrDefault();
            Usuario user = null;

            if (usuarioSolicitacao != null)
            {
                user = usuarioRepositorio.Get().Where(x => x.Id == usuarioSolicitacao.UsuarioId).FirstOrDefault();
            }
            else
            {
                return NotFound();
            }

            senha = Encrypt256(senha);

            if(user.Senha == senha)
            {
                return BadRequest();
            }

            user.Senha = senha;

            usuarioRepositorio.Alterar(user);
            usuarioSolicitacaoRepository.Excluir(usuarioSolicitacao);

            //List<Claim> claims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.Name, user.Username)
            //};

            //ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
            //ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            //await HttpContext.SignInAsync(principal);


            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,
            };

            try
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(principal), authProperties);
            }
            catch (Exception)
            {

                throw;
            }




            if (User.Identity.IsAuthenticated)
                return Ok();
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        public IActionResult PaginaAutenticada()
        {
            return View();
        }

        public IActionResult LoginView()
        {
            return View();
        }

        public IActionResult RecuperarSenha()
        {
            return View();
        }

        public async Task<IActionResult> EsqueciSenha(string email)
        {
            var user = usuarioRepositorio.Get().Where(x => x.Email.ToUpper().Trim() == email.ToUpper().Trim()).FirstOrDefault();

            if (user == null)
                return BadRequest();

            UsuarioSolicitacao usuarioSolicitacao = new UsuarioSolicitacao()
            {
                ExpiraEm = DateTime.Now.AddMinutes(2),
                Usuario = user,
                SenhaTemporaria = Guid.NewGuid().ToString().Substring(5, 8)
            };

            usuarioSolicitacaoRepository.Adicionar(usuarioSolicitacao);

            var sb = new StringBuilder();
            sb.AppendLine($"Olá {user.Username}");
            sb.AppendLine($"\nRealize sua recuperação de conta com a senha a seguir utilizando o link abaixo antes do tempo de expiração");
            sb.AppendLine($"\nSenha: {usuarioSolicitacao.SenhaTemporaria}");
            sb.AppendLine($"\nExpiração: {usuarioSolicitacao.ExpiraEm.ToString()}");
            sb.AppendLine($"\nLink: http://localhost:53551/Autenticacao/ConfirmarLogin?usuarioSolicitacaoId={usuarioSolicitacao.Id}");

            await _emailSender.SendEmailAsync(user.Email, "Recuperação de senha", sb.ToString());

            return Ok();
        }


        public async Task<IActionResult> Login(string usuario, string senha)
        {
            senha = Encrypt256(senha);

            var user = usuarioRepositorio.Get().Where(x => x.Username.ToUpper().Trim() == usuario.ToUpper().Trim()).FirstOrDefault();

            if (user == null)
                return BadRequest();

            if (user.Senha != senha)
                return Unauthorized();

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,
            };

            try
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(principal), authProperties);
            }
            catch (Exception)
            {

                throw;
            }

            if (User.Identity.IsAuthenticated)
                return Ok();
            else
            {
                return Unauthorized();
            }
        }


        private const string AesIV256 = @"!QAZ2WSX#EDC4RFV";
        private const string AesKey256 = @"5TGB&YHN7UJM(IK<5TGB&YHN7UJM(IK<";


        private string Encrypt256(string text)
        {
            // AesCryptoServiceProvider
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(AesIV256);
            aes.Key = Encoding.UTF8.GetBytes(AesKey256);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert string to byte array
            byte[] src = Encoding.Unicode.GetBytes(text);

            // encryption
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // Convert byte array to Base64 strings
                return Convert.ToBase64String(dest);
            }
        }

        /// <summary>
        /// AES decryption
        /// </summary>
        private string Decrypt256(string text)
        {
            // AesCryptoServiceProvider
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(AesIV256);
            aes.Key = Encoding.UTF8.GetBytes(AesKey256);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert Base64 strings to byte array
            byte[] src = System.Convert.FromBase64String(text);

            // decryption
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }
    }
}