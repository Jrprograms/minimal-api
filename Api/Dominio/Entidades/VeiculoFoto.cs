using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Dominio.Entidades;

public class VeiculoFoto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int VeiculoId { get; set; }

    [Required]
    [StringLength(500)]
    public string Url { get; set; } = default!;

    [ForeignKey("VeiculoId")]
    public virtual Veiculo Veiculo { get; set; } = null!;
}