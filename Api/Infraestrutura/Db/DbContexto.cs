using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;

public class DbContexto : DbContext
{
    private readonly IConfiguration _configuracaoAppSettings;
    public DbContexto(IConfiguration configuracaoAppSettings)
    {
        _configuracaoAppSettings = configuracaoAppSettings;
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;
    public DbSet<AvaliacaoVeiculo> AvaliacoesVeiculos { get; set; } = default!;
    public DbSet<VeiculoFoto> VeiculoFotos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador {
                Id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"
             }
        );

        modelBuilder.Entity<AvaliacaoVeiculo>()
            .HasOne(a => a.Veiculo)
            .WithMany(v => v.Avaliacoes)
            .HasForeignKey(a => a.VeiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AvaliacaoVeiculo>()
            .HasOne(a => a.Administrador)
            .WithMany()
            .HasForeignKey(a => a.AdministradorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(
                    stringConexao,
                    ServerVersion.AutoDetect(stringConexao)
                );
            }
        }
    }
}