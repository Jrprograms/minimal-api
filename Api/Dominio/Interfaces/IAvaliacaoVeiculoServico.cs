using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IAvaliacaoVeiculoServico
{
    Task<bool> VerificarVeiculoExiste(int veiculoId);
    Task<AvaliacaoVeiculo> AdicionarAvaliacao(int administradorId, AvaliacaoVeiculoDTO avaliacaoDTO);
    Task<IEnumerable<AvaliacaoVeiculo>> ObterAvaliacoesDoVeiculo(int veiculoId);
    Task<double> ObterMediaAvaliacoesDoVeiculo(int veiculoId);
    Task<AvaliacaoVeiculo?> ObterAvaliacaoPorId(int avaliacaoId);
    Task DeletarAvaliacao(int avaliacaoId);
}