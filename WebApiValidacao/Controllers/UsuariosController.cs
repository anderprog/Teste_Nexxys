using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApiValidacao.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;


namespace WebApiValidacao.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class UsuariosController : ControllerBase
    {
        private IConfiguration _config;
        private readonly IConfiguration _configuration;
        private readonly ApiContext _context;
        
        public UsuariosController(            
            ApiContext context,
            IConfiguration configuration)
        {
            
           
            _configuration = configuration;
            _config = configuration;
            _context = context;
        }
        [HttpGet]
        public ActionResult<string> Get()
        {
            return " << Controlador UsuariosController :: WebApiUsuarios >> ";
        }

        [AllowAnonymous]
        [HttpPost, Route("Login")]
        public IActionResult Login([FromBody]Usuario loginDetalhes)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            

            bool resultado = ValidarUsuarioBanco(loginDetalhes);
            if (resultado)
            {
                var token =  GerarTokenJWT();

                
                var stringToken = tokenHandler.WriteToken(token);


               var dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0,DateTimeKind.Utc);

        
               dateTime = dateTime.AddSeconds (token.Payload.Exp.Value);
     


                return Ok(new {Usuario_Autenticado = "true" , token = stringToken , Data_Expiração = dateTime } );
            }
            else
            {
                return Ok(new { Usuario_Autenticado = "false", token = "Não Autorizado" });
            }

            
        }

        [Authorize]
        [HttpPost, Route("ValidarSenha")]
        [Authorize]
        public IActionResult ValidarSenha([FromBody]Usuario loginDetalhes)
        {



            RetornoMensagem resultado = ValidaSenha(loginDetalhes);
            if (resultado.resultado)
            {
               return Ok(new { Senha_Valida = "true"});
            }
            else
            {
                return Ok(new { Aviso = resultado.mensagem });
            }


        }

        [Authorize]
        [HttpGet, Route("CriarSenha")]
        public IActionResult CriarSenha()
        {




            string resultado = GeraSenha();

            while (LetraDuplicada(resultado) == true)
            {
                resultado = GeraSenha();

            }

            return Ok(new { Senha= resultado});
            


        }

        private string GeraSenha()
        {

            const string CAIXA_BAIXA = "abcdefghijklmnopqrstuvwxyz";
            const string CAIXA_ALTA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            const string NUMEROS = "0123456789";
            const string ESPECIAIS = @"@#_-!";


            string permitido = "";
            permitido += CAIXA_BAIXA;
            permitido += CAIXA_ALTA;
            permitido += NUMEROS;
            permitido += ESPECIAIS;

            string guid = permitido;

            Random clsRan = new Random();
            Int32 tamanhoSenha = 15;
            int num = 0;
            string senha = "";

            num = clsRan.Next(0, CAIXA_BAIXA.Length);
            senha += CAIXA_BAIXA[num];

            num = clsRan.Next(0, CAIXA_ALTA.Length);
            senha += CAIXA_ALTA[num];


            num = clsRan.Next(0, NUMEROS.Length);
            senha += NUMEROS[num];


            num = clsRan.Next(0, ESPECIAIS.Length);
            senha += ESPECIAIS[num];
            for (Int32 i = senha.Length+1; i <= tamanhoSenha; i++)
            {
                senha += guid.Substring(clsRan.Next(1, guid.Length), 1);
            }

            string senhaValida = "";
            int numsorteado = 0;
            List<int> numerosSorteados = new List<int>();

            for (Int32 i = 0; i <= senha.Length -1; i++)
            {
                numsorteado = clsRan.Next(0, senha.Length);

                while (numerosSorteados.Contains(numsorteado) == true)
                {
                    numsorteado = clsRan.Next(0, senha.Length);
                }

                senhaValida += senha.Substring(numsorteado, 1);
                numerosSorteados.Add(numsorteado);
                
                
            }

           
            
                return senhaValida;
            
        }

        private string GeraSenhaAleatoria()
        {
            const string CAIXA_BAIXA = "abcdefghijklmnopqrstuvwxyz";
            const string CAIXA_ALTA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            const string NUMEROS = "0123456789";
            const string ESPECIAIS = @"(@,#,_,-,!";
            
            
            string permitido = "";
            permitido += CAIXA_BAIXA;
            permitido += CAIXA_ALTA;
            permitido += NUMEROS;
            permitido += ESPECIAIS;

            // Obtem o numero de caracteres .

            int numero_caracteres = 15;

            // Satisfaz as definições
            string _senha = "";
            Random rand = new Random();
            int num = 0;

            num = rand.Next(0, CAIXA_BAIXA.Length);
            _senha += CAIXA_BAIXA[num];

            num = rand.Next(0, CAIXA_ALTA.Length);
            _senha += CAIXA_ALTA[num];


            num = rand.Next(0, NUMEROS.Length);
            _senha += NUMEROS[num];


            num = rand.Next(0, ESPECIAIS.Length);
            _senha += ESPECIAIS[num];

            while (_senha.Length < numero_caracteres)
                num = rand.Next(0, permitido.Length);
                _senha += permitido[num];
            // mistura os caracteres requeridos 
            //_senha = RandomizeString(_senha);
            return _senha;
        }

        private JwtSecurityToken GerarTokenJWT()
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiry = DateTime.Now.AddMinutes(5);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: issuer, audience: audience,
            expires: DateTime.UtcNow.AddSeconds(300), signingCredentials: credentials);
            return token;
        }

        private bool ValidarUsuarioBanco(Usuario loginDetalhes)
        {

            var options = new DbContextOptionsBuilder<ApiContext>()
           .UseInMemoryDatabase(databaseName: "Usuario")
           .Options;



                var count = _context.Usuarios.Count(t => t.Login  == loginDetalhes.Login );

                var usuarios = from st in _context.Usuarios
                               where st.Login == loginDetalhes.Login && st.Senha == loginDetalhes.Senha
                               select st;

                

                if (usuarios.Count<Usuario>() == 0)
                {
                    return false;
                }
                else
                    return true;

            

        }




        private RetornoMensagem ValidaSenha(Usuario loginDetalhes)
        {


            var resultado = new RetornoMensagem();

            var input = loginDetalhes.Senha;
            

           
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
            var hasDuplicateSequence = new Regex(@"(\w)*.*\1");

            if (loginDetalhes.Senha.Length < 15)
            {
                resultado.mensagem = "Senha precisa conter pelo no minimo 15 caracteres";
                resultado.resultado =false;
            }
            else if (!hasLowerChar.IsMatch(input))
            {
                resultado.mensagem  = "Senha precisa conter pelo menos uma letra minúscula";
                resultado.resultado = false;
            }
            else if (!hasUpperChar.IsMatch(input))
            {
                resultado.mensagem  = "Senha precisa conter pelo menos uma letra maiúscula";
                resultado.resultado = false;
            }
            else if (!hasSymbols.IsMatch(input))
            {
                resultado.mensagem  = "Senha precisa conter caracteres especiais";
                resultado.resultado = false;
            }
            else if (LetraDuplicada(input))
            {
                resultado.mensagem  = "Senha não poderá conter sequencia de letras repetidas";
                resultado.resultado = false;
            }
            else
            {
                resultado.resultado = true;
            }

            return resultado;

        }

        private bool LetraDuplicada(string psenha)

        {

            string textoOriginal = psenha;
            bool encontrouLetraDuplicada = false;

            if (textoOriginal != string.Empty)
            {
                for (int indiceLetraAtual = 0; indiceLetraAtual < textoOriginal.Length; indiceLetraAtual++)
                {
                    if (indiceLetraAtual < textoOriginal.Length - 1)
                    {
                        string  letraAtual = (string)textoOriginal.Substring(indiceLetraAtual, 1);
                        string proximaLetra = (string)textoOriginal.Substring(indiceLetraAtual + 1, 1);
                        if (letraAtual == proximaLetra )
                        {
                            encontrouLetraDuplicada = true;
                            break;
                        }
                    }
                }
            }
            if (encontrouLetraDuplicada)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ValidarUsuario(Usuario loginDetalhes)
        {


            if (loginDetalhes.Login  == "Macoratti" && loginDetalhes.Senha == "Numsey$19")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private UserToken BuildToken(Usuario  userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Login ),
                new Claim("meuValor", "oque voce quiser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // tempo de expiração do token: 1 hora
            var expiration = DateTime.UtcNow.AddHours(1);
            JwtSecurityToken token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds);
            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

    }

    class RetornoMensagem
    {
        public bool  resultado;
        public string mensagem;
    }

}