using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabBib.Data;
using LabBib.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace LabBib.Controllers
{
    [Authorize(Roles = "Reader")]

    public class UtilizadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UtilizadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var UserName = User.Identity.Name;
            var requests = await _context.LivrosRequerimentos
                                         .Where(r => r.NomeUtilizador_LivrosRequerimento == UserName)
                                         .ToListAsync();

            return View(requests);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var request = await _context.LivrosRequerimentos.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if (!request.Aprovado_LivrosRequerimento && !request.Devolvido_LivrosRequerimento)
            {
                var livro = await _context.Livros.FindAsync(request.LivrosId);

                _context.LivrosRequerimentos.Remove(request);
                await _context.SaveChangesAsync();
                TempData["SucessMessage"] = "Pedido cancelado com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Não é possível cancelar o pedido!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}