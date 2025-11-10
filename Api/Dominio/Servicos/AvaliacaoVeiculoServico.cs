using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AvaliacaoVeiculoServico : IAvaliacaoVeiculoServico
{
    private readonly DbContexto _contexto;

    public AvaliacaoVeiculoServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<bool> VerificarVeiculoExiste(int veiculoId)
    {
        return await _contexto.Veiculos.AnyAsync(v => v.Id == veiculoId);
    }

    public async Task<AvaliacaoVeiculo> AdicionarAvaliacao(int administradorId, AvaliacaoVeiculoDTO avaliacaoDTO)
    {
        if (avaliacaoDTO == null)
            throw new ArgumentNullException(nameof(avaliacaoDTO));

        if (avaliacaoDTO.Estrelas < 1 || avaliacaoDTO.Estrelas > 5)
            throw new ArgumentException("A avaliação deve ser entre 1 e 5 estrelas");

        // Verifica o veículo usando o método existente
        var veiculoExiste = await VerificarVeiculoExiste(avaliacaoDTO.VeiculoId);
        if (!veiculoExiste)
            throw new KeyNotFoundException($"Veículo com ID {avaliacaoDTO.VeiculoId} não encontrado");

        var administrador = await _contexto.Administradores
            .FirstOrDefaultAsync(a => a.Id == administradorId);
        
        if (administrador == null)
            throw new KeyNotFoundException($"Administrador com ID {administradorId} não encontrado");

        // Verifica se já existe uma avaliação deste administrador para este veículo
        var avaliacaoExistente = await _contexto.AvaliacoesVeiculos
            .FirstOrDefaultAsync(a => a.VeiculoId == avaliacaoDTO.VeiculoId && a.AdministradorId == administradorId);

        if (avaliacaoExistente != null)
            throw new InvalidOperationException("Você já avaliou este veículo anteriormente");

        var avaliacao = new AvaliacaoVeiculo
        {
            VeiculoId = avaliacaoDTO.VeiculoId,
            AdministradorId = administradorId,
            Estrelas = avaliacaoDTO.Estrelas,
            Comentario = avaliacaoDTO.Comentario,
            DataAvaliacao = DateTime.UtcNow
        };

        await _contexto.AvaliacoesVeiculos.AddAsync(avaliacao);
        
        try
        {
            await _contexto.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("Erro ao salvar a avaliação no banco de dados", ex);
        }

        return avaliacao;
    }

    public async Task<IEnumerable<AvaliacaoVeiculo>> ObterAvaliacoesDoVeiculo(int veiculoId)
    {
        return await _contexto.AvaliacoesVeiculos
            .Where(a => a.VeiculoId == veiculoId)
            .Include(a => a.Administrador)
            .OrderByDescending(a => a.DataAvaliacao)
            .ToListAsync();
    }

    public async Task<double> ObterMediaAvaliacoesDoVeiculo(int veiculoId)
    {
        return await _contexto.AvaliacoesVeiculos
            .Where(a => a.VeiculoId == veiculoId)
            .AverageAsync(a => a.Estrelas);
    }

    public async Task<AvaliacaoVeiculo?> ObterAvaliacaoPorId(int avaliacaoId)
    {
        return await _contexto.AvaliacoesVeiculos
            .Include(a => a.Administrador)
            .Include(a => a.Veiculo)
            .FirstOrDefaultAsync(a => a.Id == avaliacaoId);
    }

    public async Task DeletarAvaliacao(int avaliacaoId)
    {
        var avaliacao = await ObterAvaliacaoPorId(avaliacaoId)
            ?? throw new KeyNotFoundException("Avaliação não encontrada");

        _contexto.AvaliacoesVeiculos.Remove(avaliacao);
        await _contexto.SaveChangesAsync();
    }
}