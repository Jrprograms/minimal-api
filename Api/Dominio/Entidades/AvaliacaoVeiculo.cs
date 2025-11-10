using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Dominio.Entidades;

public class AvaliacaoVeiculo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get;set; }

    [Required]
    public int VeiculoId { get;set; }

    [Required]
    public int AdministradorId { get;set; }

    [Required]
    [Range(1, 5)]
    public int Estrelas { get;set; }

    [StringLength(500)]
    public string? Comentario { get;set; }

    [Required]
    public DateTime DataAvaliacao { get;set; } = DateTime.UtcNow;

    [ForeignKey("VeiculoId")]
    public virtual Veiculo Veiculo { get;set; } = null!;

    [ForeignKey("AdministradorId")]
    public virtual Administrador Administrador { get;set; } = null!;
}