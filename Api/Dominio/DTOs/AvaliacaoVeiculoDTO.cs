using System.ComponentModel.DataAnnotations;

namespace MinimalApi.DTOs;

public class AvaliacaoVeiculoDTO
{
    [Required]
    public int VeiculoId { get;set; }

    [Required]
    [Range(1, 5, ErrorMessage = "A avaliação deve ser entre 1 e 5 estrelas")]
    public int Estrelas { get;set; }

    [StringLength(500, ErrorMessage = "O comentário não pode ter mais de 500 caracteres")]
    public string? Comentario { get;set; }
}