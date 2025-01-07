using LabBib.Data;
using LabBib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LabBib.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministradorController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public AdministradorController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var userBlock = users
                .ToDictionary(u => u.UserName, u =>
                {
                    var entry = _context.Entry(u);
                    return entry.Property <bool>("ContaBloqueada").CurrentValue;
                });

            var contaAtiva = users
                .Where(u => _context.Entry(u).Property<bool>("ContaAtiva").CurrentValue)
                .Select(u => u.Id)
                .ToHashSet();

            var utilizadorInativo = users
                .Where (u => !_context.Entry(u).Property<bool>("ContaAtiva").CurrentValue)
                .ToList();

            var activeRoles = new Dictionary<string, List<IdentityUser>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    if (!_context.Entry(user).Property<bool>("ContaAtiva").CurrentValue) continue;

                    if (!activeRoles.ContainsKey(role))
                    {
                        activeRoles[role] = new List<IdentityUser>();
                    }
                    activeRoles[role].Add(user);
                }
            }

            var inactiveRoles = new Dictionary<string, List<IdentityUser>>();
            foreach (var user in utilizadorInativo)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    if (!inactiveRoles.ContainsKey(role))
                    {
                        inactiveRoles[role] = new List<IdentityUser>();
                    }
                    inactiveRoles[role].Add(user);
                }
            }

            ViewData["UserManager"] = _userManager;
            ViewData["UserBlock"] = userBlock;
            ViewData["ActiveAccounts"] = contaAtiva;
            ViewData["InactiveAccounts"] = utilizadorInativo;
            ViewData["ActiveRoles"] = activeRoles;
            ViewData["InactiveRoles"] = inactiveRoles;

            return View("Admin", users);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarBloqueioConta(string id, string razaoBloqueio = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var entry = _context.Entry(user);
            var bloqueado = entry.Property<bool>("ContaBloqueada").CurrentValue;

            entry.Property("ContaBloqueada").CurrentValue = !bloqueado;

            if (!bloqueado)
            {
                if (!string.IsNullOrEmpty(razaoBloqueio))
                {
                    entry.Property("razaoBloqueio").CurrentValue = razaoBloqueio;
                }
                else
                {
                    entry.Property("razaoBloqueio").CurrentValue = null;
                }
            }
            else
            {
                entry.Property("razaoBloqueio").CurrentValue = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtivarConta(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var entry = _context.Entry(user);
            entry.Property("contaAtiva").CurrentValue = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverUtilizador(string id)
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

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Error");
        }

        // GET
        public IActionResult CriarUtilizador()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarUtilizador(string email, string nomeUtilizador, string password, string confirmPassword, DateTime dataNascimento, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(nomeUtilizador) || string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("", "Todos os campos devem ser preenchidos.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "As palavras-passe inseridas são diferentes.");
                return View();
            }

            if (!new[] { "Admin", "Librarian", "Reader" }.Contains(role))
            {
                ModelState.AddModelError("", "A função selecionada é inválida.");
                return View();
            }

            var user = new IdentityUser
            {
                UserName = nomeUtilizador,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);

                if (role == "Admin" || role == "Librarian")
                {
                    var entry = _context.Entry(user);
                    entry.Property("contaAtiva").CurrentValue = false;
                    await _context.SaveChangesAsync();
                }

                var perfil = new Perfil
                {
                    NomeUtilizador_Perfil = nomeUtilizador,
                    DataNascimento_Perfil = dataNascimento,
                };

                _context.Perfils.Add(perfil);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // GET
        public async Task<IActionResult> EditarUtilizador(string id)
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
        public async Task<IActionResult> EditarUtilizador(string id, string email, string nomeUtilizador, DateTime dataNascimento)
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

            var existingUserByUserName = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == nomeUtilizador && u.Id != id);
            if (existingUserByUserName != null)
            {
                ModelState.AddModelError("UserName", "Esse nome de utilizador já está a ser utilizado.");
                ViewBag.DataNascimento_Perfil = dataNascimento;
                return View(user);
            }

            var existingUserByEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email && u.Id != id);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "Esse email de utilizador já está a ser utilizado.");
                ViewBag.DataNascimento_Perfil = dataNascimento;
                return View(user);
            }

            var perfil = await _context.Perfils.FirstOrDefaultAsync(p => p.NomeUtilizador_Perfil == user.UserName);
            if (perfil != null)
            {
                perfil.DataNascimento_Perfil = dataNascimento;
                perfil.NomeUtilizador_Perfil = nomeUtilizador;
                _context.Perfils.Update(perfil);
                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("", "Perfil não localizado. A atualização do perfil falhou.");
                ViewBag.DataNascimento_Perfil = dataNascimento;
                return View(user);
            }

            var isLibrarian = await _userManager.IsInRoleAsync(user, "librarian");

            var livrosRequerimentoToUpdate = await _context.LivrosRequerimento
                .Where(br => br.NomeUtilizador_LivrosRequerimento == user.UserName || br.AceitoPor_LivrosRequerimento == user.UserName || br.Devolvido_LivrosRequerimento == user.UserName)
                .ToListAsync();

            foreach (var request in livrosRequerimentoToUpdate)
            {
                if (!isLibrarian)
                {
                    request.UserName = nomeUtilizador;
                }

                if (isLibrarian)
                {
                    if (request.AceitoPor_LivrosRequerimento == user.UserName)
                    {
                        request.AceitoPor_LivrosRequerimento = nomeUtilizador;
                    }

                    if (request.Devolvido_LivrosRequerimento == user.UserName)
                    {
                        request.Devolvido_LivrosRequerimento = nomeUtilizador;
                    }
                }
            }

            await _context.SaveChangesAsync();

            user.Email = email;
            user.UserName = nomeUtilizador;

            var atualizarResultado = await _userManager.UpdateAsync(user);
            if (!atualizarResultado.Succeeded)
            {
                foreach (var error in atualizarResultado.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                var recarregarPerfil = await _context.Perfils.FirstOrDefaultAsync(p => p.NomeUtilizador_Perfil == user.UserName);
                if (recarregarPerfil != null)
                {
                    ViewBag.dataNascimento = recarregarPerfil.DataNascimento_Perfil;
                }

                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
