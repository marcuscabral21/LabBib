using System.ComponentModel.DataAnnotations;

namespace LabBib.Models
{
    public class Livros
    {
        [Key]
        public int Id_Livros { get; set; }

        [Required (ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(20)]
        public string ISBN_Livros { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(150)]
        public string Titulo_Livros { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(100)]
        public string Autor_Livros { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(50)]
        public string Genero_Livros { get; set; }

        [DataType(DataType.Date)] // Limita para ano-mês-dia
        public DateTime DataPublicacao_Livros { get; set; } // Data de publicação yyyy-MM-dd (ano-mês-dia)

        public bool Disponivel_Livros { get; set; } // true = disponível, false = indisponível

        public string? Imagem_Livros { get; set; } // ? = pode ser null
    }
}
