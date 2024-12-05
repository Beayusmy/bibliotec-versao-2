using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        // Criando um obj da classe Contex:
        Context context = new Context();

        // O método está retornando a View Usuario/Index.cshtml
        public IActionResult Index()
        {
            //Pegar as informacoes da session que sao necessárias para que aparece os detalhes do meu usuário
           int id = int.Parse(HttpContext.Session.GetString("UsuarioID")!);
           ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            // Busquei o usuário que está logado (Beatriz)
            Usuario usuarioEncontrado = context.Usuario.FirstOrDefault(usuario => usuario.UsuarioID == id)!;
            //se não for encontrado ninguem
            if(usuarioEncontrado == null){
                return NotFound();
            }

            //Procurar o curso que meu usuárioEncontrado está cadastrado
            Curso cursoEncontrado = context.Curso.FirstOrDefault(curso => curso.CursoID == usuarioEncontrado.CursoID)!;

            //Verificar se o usuario possui ou nao o curso
            if(cursoEncontrado == null){

                //Preciso que vc mande essa mensagem para a View:
                ViewBag.Curso = "O usuário não possui curso cadastrado";
            }else{
                //Preciso que vc mande p nome do curso para a View:
                ViewBag.Curso = cursoEncontrado.Nome;
            }

            ViewBag.Nome = usuarioEncontrado.Nome;
            ViewBag.Email = usuarioEncontrado.Email;
            ViewBag.Telefone = usuarioEncontrado.Contato;
            ViewBag.DtNasc = usuarioEncontrado.DtNascimento.ToString("dd/MM/yyyy");
            
            return View();
        }


        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}