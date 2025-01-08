using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabBib.Models
{
    public class LivrosRequerimento
    {
        [Key]
        public int Id_LivrosRequerimento { get; set; }

        public int LivrosId { get; set; }

        public bool Aprovado_LivrosRequerimento { get; set; } // true = aprovado, false = não aprovado

        public bool Devolvido_LivrosRequerimento { get; set; } // true = devolvido, false = não devolvido

        public string? AceitoPor_LivrosRequerimento { get; set; } // ? = pode ser null

        public string? RecusadoPor_LivrosRequerimento { get; set; } // ? = pode ser null

        public DateTime? DataAprovacao_LivrosRequerimento { get; set; } // Data de publicação yyyy-MM-dd (ano-mês-dia); ? = pode ser null

        public DateTime? DataDevolucao_LivrosRequerimento { get; set; } // Data de publicação yyyy-MM-dd (ano-mês-dia); ? = pode ser null

        public DateTime DataRequerimento_LivrosRequerimento { get; set; } // Data de publicação yyyy-MM-dd (ano-mês-dia)

        public DateTime DataPrevisaoDevolucao_LivrosRequerimento { get; set; } // Data de publicação yyyy-MM-dd (ano-mês-dia)

        public string? TituloLivro_LivrosRequerimento { get; set; } // ? = pode ser null

        public string? NomeUtilizador_LivrosRequerimento { get; set; } // ? = pode ser null

        public bool Recusado_LivrosRequerimento { get; set; } // true = recusado, false = não recusado

    }
}
