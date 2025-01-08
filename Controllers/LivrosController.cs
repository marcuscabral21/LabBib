using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabBib.Data;
using LabBib.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using static System.Reflection.Metadata.BlobBuilder;

namespace LabBib.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class LivrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LivrosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var books = await _context.Livros.ToListAsync();
            return View(books);
        }

        public async Task<IActionResult> Librarian()
        {
            var temEntregasAtrasadas = await _context.LivrosRequerimentos
                .AnyAsync(r => r.Aprovado_LivrosRequerimento && !r.Devolvido_LivrosRequerimento && r.DataPrevisaoDevolucao_LivrosRequerimento < DateTime.Now && r.DataDevolucao_LivrosRequerimento == null);

            var temEntregasPendentes = await _context.LivrosRequerimentos
                .AnyAsync(r => !r.Aprovado_LivrosRequerimento && !r.Recusado_LivrosRequerimento);

            var naotemEntregasAtrasadas = !temEntregasAtrasadas;
            var naotemEntregasPendentes = !temEntregasPendentes;

            ViewBag.TemEntregasAtrasadas = temEntregasAtrasadas;
            ViewBag.TemEntregasPendentes = temEntregasPendentes;

            ViewBag.NaotemEntregasAtrasadas = naotemEntregasAtrasadas;
            ViewBag.NaotemEntregasPendentes = naotemEntregasPendentes;

            return View();
        }


        // GET
        public async Task<IActionResult> AdicionarOuEditarLivros(int? id)
        {
            if (id == null)
            {
                return View(new Livros());
            }

            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
            {
                return NotFound();
            }

            return View(livro);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarOuEditarLivros(Livros livro, IFormFile? Imagem_Livros)
        {
            if (ModelState.IsValid)
            {
                string existeImagemLivro = null;

                if (livro.Id_Livros != 0)
                {
                    var livroExistente = await _context.Livros.AsNoTracking().FirstOrDefaultAsync(b => b.Id_Livros == livro.Id_Livros);
                    if (livroExistente != null)
                    {
                        existeImagemLivro = livroExistente.Imagem_Livros;
                    }
                }

                if (Imagem_Livros != null && Imagem_Livros.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/capa");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var sanitizedTitle = string.Join("_", livro.Titulo_Livros.Split(Path.GetInvalidFileNameChars()));
                    var fileName = $"{sanitizedTitle}_cover{Path.GetExtension(Imagem_Livros.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    try
                    {
                        if (!string.IsNullOrEmpty(existeImagemLivro))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existeImagemLivro.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Imagem_Livros.CopyToAsync(stream);
                        }

                        livro.Imagem_Livros = "/images/capa/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Erro ao salvar a imagem: " + ex.Message);
                        return View(livro);
                    }
                }
                else if (livro.Id_Livros != 0)
                {
                    livro.Imagem_Livros = existeImagemLivro;
                }

                if (livro.Id_Livros == 0)
                {
                    _context.Add(livro);
                }
                else
                {
                    _context.Update(livro);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(livro);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarLivro(int id)
        {
            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(livro.Imagem_Livros))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", livro.Imagem_Livros.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Livros.Remove(livro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET
        public async Task<IActionResult> GerenciarRequerimentos()
        {
            var requerimentos = await _context.LivrosRequerimentos.ToListAsync();
            return View(requerimentos);
        }

        // GET
        public async Task<IActionResult> AceitarRequerimentos(int id)
        {
            var requerimento = await _context.LivrosRequerimentos.FindAsync(id);
            if (requerimento == null)
            {
                return NotFound();
            }

            if (requerimento.DataRequerimento_LivrosRequerimento <= DateTime.Now && requerimento.DataPrevisaoDevolucao_LivrosRequerimento >= DateTime.Now)
            {
                requerimento.Aprovado_LivrosRequerimento = true;
                requerimento.AceitoPor_LivrosRequerimento = User.Identity.Name;
                requerimento.DataAprovacao_LivrosRequerimento = DateTime.Now;

                _context.Update(requerimento);
                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "O pedido foi enviado fora do prazo permitido.";
            }

            return RedirectToAction(nameof(GerenciarRequerimentos));
        }


        // GET
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

            var livro = await _context.Livros.FindAsync(requerimento.LivrosId);
            if (livro != null)
            {
                _context.Update(livro);
            }

            _context.Update(requerimento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GerenciarRequerimentos));
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarRequerimentos(int id)
        {
            var requerimento = await _context.LivrosRequerimentos.FindAsync(id);
            if (requerimento == null)
            {
                return NotFound();
            }

            requerimento.Devolvido_LivrosRequerimento = true;

            var livro = await _context.Livros.FindAsync(requerimento.LivrosId);
            if (livro != null)
            {
                _context.Update(livro);
            }

            _context.Update(requerimento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GerenciarRequerimentos));
        }

        // GET
        public async Task<IActionResult> NotificacoesBiblioteca()
        {
            var requerimentos = await _context.LivrosRequerimentos
                .Where(r => r.Aprovado_LivrosRequerimento && !r.Devolvido_LivrosRequerimento && r.DataPrevisaoDevolucao_LivrosRequerimento < DateTime.Now && r.DataDevolucao_LivrosRequerimento == null)
                .ToListAsync();

            return View(requerimentos);
        }
    }
}