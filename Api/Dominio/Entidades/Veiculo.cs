using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Dominio.Entidades;

public enum StatusVeiculo
{
    Disponivel,
    EmManutencao,
    Reservado,
    Indisponivel
}

public class Veiculo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get;set; } = default!;

    [Required]
    [StringLength(150)]
    public string Nome { get;set; } = default!;

    [Required]
    [StringLength(100)]
    public string Marca { get;set; } = default!;

    [Required]
    public int Ano { get;set; } = default!;

    [Required]
    [StringLength(8)]
    public string Placa { get;set; } = default!;

    [Required]
    public StatusVeiculo Status { get;set; } = StatusVeiculo.Disponivel;

    [Required]
    [StringLength(50)]
    public string Cor { get;set; } = default!;

    [Required]
    public decimal Quilometragem { get;set; } = 0;

    [Required]
    public decimal Preco { get;set; } = default!;

    [StringLength(500)]
    public string? Descricao { get;set; }

    public virtual ICollection<VeiculoFoto> Fotos { get;set; } = new List<VeiculoFoto>();

    public virtual ICollection<AvaliacaoVeiculo> Avaliacoes { get;set; } = new List<AvaliacaoVeiculo>();
}