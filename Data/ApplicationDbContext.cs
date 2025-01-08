using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LabBib.Models;

namespace LabBib.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Perfil> Perfils { get; set; }

        public DbSet<Livros> Livros { get; set; }

        public DbSet<LivrosRequerimento> LivrosRequerimentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>(b=>
            {
                b.Property<bool>("ContaBloqueada")
                    .HasDefaultValue(false);

                b.Property<bool>("ContaAtiva")
                    .HasDefaultValue(true);

                b.Property<string>("RazaoBloqueio")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });
        }
    }
}
