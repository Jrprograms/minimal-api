
using System.ComponentModel.DataAnnotations;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.DTOs;

public record VeiculoDTO
{
    [Required(ErrorMessage = "O nome do veículo é obrigatório")]
    [StringLength(150, ErrorMessage = "O nome não pode ter mais de 150 caracteres")]
    public string Nome { get;set; } = default!;

    [Required(ErrorMessage = "A marca do veículo é obrigatória")]
    [StringLength(100, ErrorMessage = "A marca não pode ter mais de 100 caracteres")]
    public string Marca { get;set; } = default!;

    [Required(ErrorMessage = "O ano do veículo é obrigatório")]
    [Range(1900, 2100, ErrorMessage = "O ano deve estar entre 1900 e 2100")]
    public int Ano { get;set; } = default!;

    [Required(ErrorMessage = "A placa do veículo é obrigatória")]
    [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres")]
    public string Placa { get;set; } = default!;

    [Required(ErrorMessage = "O status do veículo é obrigatório")]
    public StatusVeiculo Status { get;set; } = StatusVeiculo.Disponivel;

    [Required(ErrorMessage = "A cor do veículo é obrigatória")]
    [StringLength(50, ErrorMessage = "A cor não pode ter mais de 50 caracteres")]
    public string Cor { get;set; } = default!;

    [Required(ErrorMessage = "A quilometragem do veículo é obrigatória")]
    [Range(0, double.MaxValue, ErrorMessage = "A quilometragem deve ser maior ou igual a zero")]
    public decimal Quilometragem { get;set; } = 0;

    [Required(ErrorMessage = "O preço do veículo é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser maior ou igual a zero")]
    public decimal Preco { get;set; } = default!;

    [StringLength(500, ErrorMessage = "A descrição não pode ter mais de 500 caracteres")]
    public string? Descricao { get;set; }

    public List<string> FotosUrls { get;set; } = new();
}