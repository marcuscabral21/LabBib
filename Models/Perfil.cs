using System.ComponentModel.DataAnnotations;

namespace LabBib.Models
{
    public class Perfil
    {
        [Key]
        public int Id_Perfil { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataNascimento_Perfil { get; set; } // Data de nascimento yyyy-MM-dd (ano-mês-dia)

        public string? NomeUtilizador_Perfil { get; set; } // ? = pode ser null

        public string? Foto_Perfil { get; set; } // ? = pode ser null
    }
}
