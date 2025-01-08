using LabBib.Data;
using LabBib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace LabBib.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index(string pesquisarTitulo, string pesquisarAutor, string pesquisarGenero)
        {
            var pesquisaDeLivros = _context.Livros.AsQueryable();

            if (!string.IsNullOrEmpty(pesquisarTitulo))
            {
                pesquisaDeLivros = pesquisaDeLivros.Where(b => b.Titulo_Livros.Contains(pesquisarTitulo));
            }

            if (!string.IsNullOrEmpty(pesquisarAutor))
            {
                pesquisaDeLivros = pesquisaDeLivros.Where(b => b.Autor_Livros.Contains(pesquisarAutor));
            }

            if (!string.IsNullOrEmpty(pesquisarGenero))
            {
                pesquisaDeLivros = pesquisaDeLivros.Where(b => b.Genero_Livros.Contains(pesquisarGenero));
            }

            var livros = await pesquisaDeLivros.ToListAsync();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;

                var perfil = await _context.Perfils.FirstOrDefaultAsync(p => p.NomeUtilizador_Perfil == userName);

                var pedidosUtilizador = await _context.LivrosRequerimentos
                    .Where(r => r.NomeUtilizador_LivrosRequerimento == userName && !r.Devolvido_LivrosRequerimento && !r.Recusado_LivrosRequerimento)
                    .ToListAsync();
                ViewData["PedidosUtilizador"] = pedidosUtilizador;
            }

            ViewData["PesquisarTitulo"] = pesquisarTitulo;
            ViewData["PesquisarAutor"] = pesquisarAutor;
            ViewData["PesquisarGenero"] = pesquisarGenero;

            return View(livros);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("O ID do utilizador é obrigatório e não pode estar vazio.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Utilizador não identificado.");
            }

            var perfil = await _context.Perfils.FirstOrDefaultAsync(p => p.NomeUtilizador_Perfil == user.UserName);
            if (perfil == null)
            {
                return NotFound("Perfil não identificado.");
            }

            ViewBag.DataNascimento = perfil.DataNascimento_Perfil;

            return View(user);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(string id, string email, string userName, DateTime dataNascimento)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("O ID do utilizador é obrigatório e não pode estar vazio.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Utilizador não identificado.");
            }

            var emailRepetido = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email && u.Id != id);
            if (emailRepetido != null)
            {
                ModelState.AddModelError("Email", "Este email já está associado a outro utilizador.");
                return await EditarPerfil();
            }

            var userNameRepetido = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.Id != id);
            if (userNameRepetido != null)
            {
                ModelState.AddModelError("UserName", "Este nome de utilizador já está associado a outra conta.");
                return await EditarPerfil();
            }

            var perfil = await _context.Perfils.FirstOrDefaultAsync(p => p.NomeUtilizador_Perfil == user.UserName);
            if (perfil != null)
            {
                string oldUserName = user.UserName;               

                perfil.DataNascimento_Perfil = dataNascimento;
                perfil.NomeUtilizador_Perfil = userName;

                _context.Perfils.Update(perfil);
                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("", "Não foi possível encontrar o perfil para realizar a atualização.");
                return await EditarPerfil();
            }

            var isLibrarian = await _userManager.IsInRoleAsync(user, "librarian");

            var pedidosAtualizarLivro = await _context.LivrosRequerimentos
                .Where(br => br.NomeUtilizador_LivrosRequerimento == user.UserName || br.AceitoPor_LivrosRequerimento == user.UserName || br.RecusadoPor_LivrosRequerimento == user.UserName)
                .ToListAsync();

            foreach (var request in pedidosAtualizarLivro)
            {
                if (!isLibrarian)
                {
                    request.NomeUtilizador_LivrosRequerimento = userName;
                }

                if (isLibrarian)
                {
                    if (request.AceitoPor_LivrosRequerimento == user.UserName)
                    {
                        request.AceitoPor_LivrosRequerimento = userName;
                    }

                    if (request.RecusadoPor_LivrosRequerimento == user.UserName)
                    {
                        request.RecusadoPor_LivrosRequerimento = userName;
                    }
                }
            }

            await _context.SaveChangesAsync();

            user.Email = email;
            user.UserName = userName;

            var atualizacaoResultado = await _userManager.UpdateAsync(user);
            if (!atualizacaoResultado.Succeeded)
            {
                foreach (var error in atualizacaoResultado.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return await EditarPerfil();
            }

            await _signInManager.SignOutAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction(nameof(Index));
        }
    }
}
