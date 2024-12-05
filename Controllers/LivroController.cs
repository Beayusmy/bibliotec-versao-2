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
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            //Criar uma lista de livros
            List<Livro> listaLivros = context.Livro.ToList();

            //Verificar se o livro tem reserva ou não
            //ToDictionay(chave, valor)
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }

        [Route("Cadastro")]
        //Método que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();
            //Retorna a View de cadastro:
            return View();
        }

        // Método para cadastrar um livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {

            //PRIMEIRA PARTE: Cadastrar um livro na tabela Livro
            Livro novoLivro = new Livro();

            //O que meu usuário escrever no formulário será atribuido ao novoLivro
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();

            //Trabalhar com imagens:

            if (form.Files.Count > 0)
            {
                //Primeiro passo:
                // Armazenaremos o arquivo/foto enviado pelo usuário
                var arquivo = form.Files[0];

                //Segundo passo:
                //Criar variavel do caminho da minha pasta para colocar as fotos dos livros
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");
                //Validaremos se a pasta que será armazenada as imagens, existe. Caso não exista, criaremos uma nova pasta.
                if (!Directory.Exists(pasta))
                {
                    //Criar a pasta:
                    Directory.CreateDirectory(pasta);
                }
                //Terceiro passo:
                //Criar a variavel para armazenar o caminho em que meu arquivo estara, além do nome dele
                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    //Copiou o arquivo para o meu diretório
                    arquivo.CopyTo(stream);
                }

                novoLivro.Imagem = arquivo.FileName;
            }else{
                novoLivro.Imagem = "padrao.png";
            }


            context.Livro.Add(novoLivro);
            context.SaveChanges();

            // SEGUNDA PARTE: É adicionar dentro de LivroCategoria a categoria que pertence ao novoLivro
            //Lista a tabela LivroCategoria:
            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();

            //Array que possui as categorias selecionadas pelo usuário
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');
            //Ação, terror, suspense
            //3, 5, 7

            foreach (string categoria in categoriasSelecionadas)
            {
                //string categoria possui a informação do id da categoria ATUAL selecionada.
                LivroCategoria livroCategoria = new LivroCategoria();

                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                //Adicionamos o obj livroCategoria dentro da lista listaLivroCategorias
                listaLivroCategorias.Add(livroCategoria);
            }

            //Peguei a coleção da listaLivroCategorias e coloquei na tabela LivroCategoria
            context.LivroCategoria.AddRange(listaLivroCategorias);

            context.SaveChanges();

            return LocalRedirect("/Cadastro");

        }

            [Route ("Editar/{id}")]
            public IActionResult Editar(int id){

                 ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

                ViewBag.CategoriasDoSistema = context.Categoria.ToList();

                Livro livroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

                var categoriasDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id).Select(livro => livro.Categoria).ToList();

                ViewBag.Livro = livroEncontrado;
                ViewBag.Categoria = categoriasDoLivroEncontrado;



                return View();
            }

            //metodo que atualiza as informacoes do livro:
            [Route("Atualizar")]
            public IActionResult Atualizar(IFormCollection form, int id, IFormFile imagem){
                //Buscar um livro especifico pelo ID 
                Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

                livroAtualizado.Nome = form["Nome"];
                livroAtualizado.Escritor = form["Escritor"];
                livroAtualizado.Editora = form["Editora"];
                livroAtualizado.Idioma = form["Idioma"];
                livroAtualizado .Descricao = form["Descricao"];


                //Upluad de imagem
                if(imagem! == null && imagem.Length > 0){ 
                    //Definir o caminho da minha imagem do livro ATUAL, que eu quero alterar:
                    var caminhoimagem = Path.Combine("wwwroot/images/Livros", imagem.FileName);

                    //Verificar se minha imagem ainda existe no meu caminho
                    //Caso exista, ela ira ser apagado

                    if(string.IsNullOrEmpty(livroAtualizado.Imagem)){
                        //Caso exista, ela ira ser apagada
                        var caminhoImagemAntiga = Path.Combine("wwwroot/images/Livros", livroAtualizado.Imagem);
                        //Ver se existe uma imagem no caminho antigo
                        if(System.IO.File.Exists(caminhoImagemAntiga)){
                            System.IO.File.Delete(caminhoImagemAntiga);
                        }
                    }

                    //Salvar a imagem nova
                    using(var stream = new FileStream(caminhoimagem, FileMode.Create)){
                        imagem.CopyTo(stream);
                    }

                    //Subir essa mudanca para o meu banco de dados
                    livroAtualizado.Imagem = imagem.FileName;
                }

                    //CATEGORIAS:
                    //PRIMEIRO: Precisamos pegar as categorias selecionadas do usuario
                    var categoriasSelecionadas = form["Categoria"].ToList();
                    //SEGUNDO: Pegaremos as categorias ATUAIS do livro
                    var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();
                    //TERCEIRO: Removeremos as categorias antigas
                    foreach(var categoria in categoriasAtuais){
                        if(!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString())){
                            //Nos vamos remover a categoria do nosso context
                            context.LivroCategoria.Remove(categoria);
                        }
                    }
                    //QUARTO: Adicionaremos as novas categorias
                    foreach(var categoria in categoriasSelecionadas){
                        //Verificando se nao existe
                        if(!categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria)){
                            context.LivroCategoria.Add(new LivroCategoria{
                                LivroID = id,
                                CategoriaID = int.Parse(categoria)



                            });
                        }
                    }
                    context.SaveChanges();
                    return LocalRedirect("/Livro");
            }

            //metodo de excluir o livro
            [Route("Excluir")]
            public IActionResult Excluir(int id){
                //Buscar qual o livro do id que precisamos excluir
                Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);

                //Buscar as categorias desse livro:
                var categoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

                //Precisa excluir primeiro o registro da tabela intermediaria
                foreach(var categoria in categoriasDoLivro){
                    context.LivroCategoria.Remove(categoria);
                }

                context.Livro.Remove(livroEncontrado);
                context.SaveChanges();

                return LocalRedirect("/Livro");
            }
        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}