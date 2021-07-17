using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiValidacao.Models;
using Microsoft.EntityFrameworkCore;


namespace WebApiValidacao.Controllers
{
    //[Produces("application/json")]
    [Route("api/UsuarioTeste")]

    public class UsuarioTesteController : ControllerBase
    {

        private readonly ApiContext _context;

        public UsuarioTesteController(ApiContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Get()
        {
            var usuarios = await _context.Usuarios
                                 .ToArrayAsync();

            var resposta = usuarios.Select(u => new
            {
                Login = u.Login,
                Senha = u.Senha
            });

            return Ok(resposta);

        }
    }
}