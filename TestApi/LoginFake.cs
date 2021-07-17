using System;
using System.Collections.Generic;
using System.Text;
using WebApiValidacao.Models;
using WebApiValidacao.Controllers;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace TestApi
{
    public class LoginFake 
    {

        UsuariosController _controller;
        ApiContext _context;
        IConfiguration _configuration;




        public LoginFake() 
        {

            var options = new DbContextOptionsBuilder<ApiContext>()
            .UseInMemoryDatabase(databaseName: "Usuario")
            .Options;

            _context = new ApiContext(options);


            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "testenexxysparadesenvolvedorseniorapifullstack"},
                {"Jwt:Issuer", "Emissor"},
                {"Jwt:Audience", "Publico"},
                //...populate as needed for the test
            };

           _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            
        }

        [Fact]
        public void ValidarUsuario()
        {

            Usuario _usu = new Usuario();
            _usu.Login = "anderson";
            _usu.Senha = "And3rson@1S!lva";

            AdicionarDadosTeste(_context);

            _controller = new UsuariosController(_context , _configuration);
            // Act
            var result = _controller.Login(_usu);
            var okResult = result as OkObjectResult;

            // Assert
            // assert
            Assert.NotNull(okResult);
            Assert.True(okResult is OkObjectResult);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public void ValidarSenha()
        {

            Usuario _usu = new Usuario();
            _usu.Login = "anderson";
            _usu.Senha = "And3rson@1S!lva";


            _controller = new UsuariosController(_context, _configuration);
            // Act
            var result = _controller.ValidarSenha(_usu);
            var okResult = result as OkObjectResult;

            // Assert
            // assert
            Assert.NotNull(okResult);
            Assert.True(okResult is OkObjectResult);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public void CriarSenha()
        {

            _controller = new UsuariosController(_context, _configuration);
            // Act
            var result = _controller.CriarSenha();
            var okResult = result as OkObjectResult;


            // assert
            Assert.NotNull(okResult);
            Assert.True(okResult is OkObjectResult);
            Assert.Equal(200, okResult.StatusCode);

        }

        private static void AdicionarDadosTeste(ApiContext context)
        {
            var testeUsuario1 = new Usuario 
            {
                Login = "anderson",
                Senha = "And3rson@1S!lva"
            };

            context.Usuarios.Add(testeUsuario1);


            var testeUsuario2 = new Usuario
            {
                Login = "nexxys",
                Senha = "Consultori@Nexx!s"
            };

            context.Usuarios.Add(testeUsuario2);

            context.SaveChanges();
        }

    }
}
