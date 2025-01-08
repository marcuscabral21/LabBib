using Microsoft.AspNetCore.Mvc;
using LabBib.Data;
using LabBib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LabBib.Controllers
{
    [Authorize(Roles = "Reader")]
    public class RequerimentoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequerimentoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        public async Task<IActionResult> Index(int id)
        {
            var livro = await _context.Livros.FirstOrDefaultAsync(b => b.Id_Livros == id);

            if (livro == null)
            {
                return NotFound();
            }

            var requisicaoLivro = new LivrosRequerimento
            {
                LivrosId = id
            };

            ViewData["Livro"] = livro;
            return View(requisicaoLivro);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LivrosRequerimento requisicaoLivro)
        {
            if (ModelState.IsValid)
            {
                if (requisicaoLivro.DataRequerimento_LivrosRequerimento == default(DateTime))
                {
                    requisicaoLivro.DataRequerimento_LivrosRequerimento = DateTime.Now;
                }

                if (requisicaoLivro.DataPrevisaoDevolucao_LivrosRequerimento == default(DateTime))
                {
                    requisicaoLivro.DataPrevisaoDevolucao_LivrosRequerimento = requisicaoLivro.DataRequerimento_LivrosRequerimento.AddDays(7);
                }
                else if (requisicaoLivro.DataPrevisaoDevolucao_LivrosRequerimento > requisicaoLivro.DataRequerimento_LivrosRequerimento.AddDays(7))
                {
                    ModelState.AddModelError("", "O prazo máximo é de 7 dias.");
                    return View("Index", requisicaoLivro);
                }

                var livro = await _context.Livros.FindAsync(requisicaoLivro.LivrosId);
                if (livro != null)
                {
                    requisicaoLivro.TituloLivro_LivrosRequerimento = livro.Titulo_Livros;
                }

                requisicaoLivro.NomeUtilizador_LivrosRequerimento = User.Identity.Name;

                _context.Add(requisicaoLivro);
                await _context.SaveChangesAsync();

                return RedirectToAction("ConfirmationScreen", "Requests");
            }

            return View("Index", requisicaoLivro);
        }


        // GET
        public IActionResult EcraConfirmacao()
        {
            return View();
        }

        // GET
        [HttpPost]
        public async Task<IActionResult> AceitarRequerimentos(int id)
        {
            var requerimento = await _context.LivrosRequerimentos.FindAsync(id);
            if (requerimento == null)
            {
                return NotFound();
            }

            requerimento.Aprovado_LivrosRequerimento = true;
            requerimento.AceitoPor_LivrosRequerimento = User.Identity.Name;
            requerimento.DataAprovacao_LivrosRequerimento = DateTime.Now;

            _context.Update(requerimento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GerenciarRequerimentos));
        }

        // GET
        [HttpPost]
        public async Task<IActionResult> PedidoDeRetorno(int id)
        {
            var requerimento = await _context.LivrosRequerimentos.FindAsync(id);
            if (requerimento == null)
            {
                return NotFound();
            }

            requerimento.Devolvido_LivrosRequerimento = true;
            requerimento.RecusadoPor_LivrosRequerimento = User.Identity.Name;
            requerimento.DataDevolucao_LivrosRequerimento = DateTime.Now;

            _context.Update(requerimento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GerenciarRequerimentos));
        }

        // GET
        public async Task<IActionResult> GerenciarRequerimentos()
        {
            var requerimentos = await _context.LivrosRequerimentos.ToListAsync();
            return View(requerimentos);
        }
    }
}