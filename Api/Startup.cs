using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
    }

    private string key = "";
    public IConfiguration Configuration { get;set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option => {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option => {
            option.TokenValidationParameters = new TokenValidationParameters{
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();
        services.AddScoped<IAvaliacaoVeiculoServico, AvaliacaoVeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference 
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddDbContext<DbContexto>(options => {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
            );
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors();

            app.UseEndpoints(endpoints => {
                // Endpoints de Avaliação
                endpoints.MapPost("/veiculos/{veiculoId}/avaliacoes", async (
                    [FromRoute] int veiculoId,
                    [FromBody] AvaliacaoVeiculoDTO avaliacaoDTO,
                    [FromServices] IAvaliacaoVeiculoServico avaliacaoServico,
                    [FromServices] ILogger<Startup> logger,
                    ClaimsPrincipal user) =>
                {
                    try
                    {
                        if (avaliacaoDTO == null)
                        {
                            logger.LogError("AvaliacaoDTO é nulo");
                            return Results.BadRequest(new { message = "Dados da avaliação não fornecidos" });
                        }

                        logger.LogInformation($"Dados recebidos: VeiculoId={veiculoId}, Estrelas={avaliacaoDTO.Estrelas}, Comentario={avaliacaoDTO.Comentario}");
                        logger.LogInformation($"Iniciando adição de avaliação para veículo {veiculoId}");
                        
                        // Verificar se o veículo existe
                        var veiculoExists = await avaliacaoServico.VerificarVeiculoExiste(veiculoId);
                        if (!veiculoExists)
                        {
                            logger.LogWarning($"Veículo {veiculoId} não encontrado");
                            return Results.NotFound(new { message = "Veículo não encontrado" });
                        }

                        var nameIdentifierClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                        if (nameIdentifierClaim == null)
                        {
                            logger.LogError("NameIdentifier claim não encontrada no token");
                            return Results.Unauthorized();
                        }

                        if (!int.TryParse(nameIdentifierClaim.Value, out int adminId))
                        {
                            logger.LogError($"Não foi possível converter o ID do administrador: {nameIdentifierClaim.Value}");
                            return Results.Unauthorized();
                        }

                        if (adminId == 0)
                        {
                            logger.LogError("ID do administrador é 0");
                            return Results.Unauthorized();
                        }

                        logger.LogInformation($"Tentando adicionar avaliação. VeiculoId: {veiculoId}, AdminId: {adminId}");
                        
                        // Criar novo DTO com o veiculoId da URL
                        var novaAvaliacaoDTO = new AvaliacaoVeiculoDTO
                        {
                            VeiculoId = veiculoId,
                            Estrelas = avaliacaoDTO.Estrelas,
                            Comentario = avaliacaoDTO.Comentario
                        };
                        var avaliacao = await avaliacaoServico.AdicionarAvaliacao(adminId, novaAvaliacaoDTO);
                        return Results.Created($"/veiculos/{veiculoId}/avaliacoes/{avaliacao.Id}", avaliacao);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        logger.LogError(ex, "Recurso não encontrado ao adicionar avaliação");
                        return Results.NotFound(new { message = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Erro ao adicionar avaliação");
                        return Results.BadRequest(new { message = ex.Message });
                    }
                })
                .RequireAuthorization()
                .WithTags("Avaliações");

                endpoints.MapGet("/veiculos/{veiculoId}/avaliacoes", async (
                    [FromRoute] int veiculoId,
                    [FromServices] IAvaliacaoVeiculoServico avaliacaoServico) =>
                {
                    try
                    {
                        var avaliacoes = await avaliacaoServico.ObterAvaliacoesDoVeiculo(veiculoId);
                        return Results.Ok(avaliacoes);
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                })
                .WithTags("Avaliações");

                endpoints.MapGet("/veiculos/{veiculoId}/avaliacoes/media", async (
                    [FromRoute] int veiculoId,
                    [FromServices] IAvaliacaoVeiculoServico avaliacaoServico) =>
                {
                    try
                    {
                        var media = await avaliacaoServico.ObterMediaAvaliacoesDoVeiculo(veiculoId);
                        return Results.Ok(new { media });
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                })
                .WithTags("Avaliações");

                endpoints.MapDelete("/avaliacoes/{avaliacaoId}", async (
                    [FromRoute] int avaliacaoId,
                    [FromServices] IAvaliacaoVeiculoServico avaliacaoServico,
                    ClaimsPrincipal user) =>
                {
                    try
                    {
                        var adminId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                        if (adminId == 0) return Results.Unauthorized();

                        var avaliacao = await avaliacaoServico.ObterAvaliacaoPorId(avaliacaoId);
                        if (avaliacao == null) return Results.NotFound();
                        if (avaliacao.AdministradorId != adminId) return Results.Forbid();

                        await avaliacaoServico.DeletarAvaliacao(avaliacaoId);
                        return Results.NoContent();
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                })
                .RequireAuthorization()
                .WithTags("Avaliações");

            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administradores
            string GerarTokenJwt(Administrador administrador){
                if(string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, administrador.Id.ToString()),
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil),
                };
                
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
                var adm = administradorServico.Login(loginDTO);
                if(adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministradorLogado
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administradores");

            endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => {
                var adms = new List<AdministradorModelView>();
                var administradores = administradorServico.Todos(pagina);
                foreach(var adm in administradores)
                {
                    adms.Add(new AdministradorModelView{
                        Id = adm.Id,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }
                return Results.Ok(adms);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/Administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
                var administrador = administradorServico.BuscaPorId(id);
                if(administrador == null) return Results.NotFound();
                return Results.Ok(new AdministradorModelView{
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                });
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
                var validacao = new ErrosDeValidacao{
                    Mensagens = new List<string>()
                };

                if(string.IsNullOrEmpty(administradorDTO.Email))
                    validacao.Mensagens.Add("Email não pode ser vazio");
                if(string.IsNullOrEmpty(administradorDTO.Senha))
                    validacao.Mensagens.Add("Senha não pode ser vazia");
                if(administradorDTO.Perfil == null)
                    validacao.Mensagens.Add("Perfil não pode ser vazio");

                if(validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);
                
                var administrador = new Administrador{
                    Email = administradorDTO.Email,
                    Senha = administradorDTO.Senha,
                    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                };

                administradorServico.Incluir(administrador);

                return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView{
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
                });
                
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");
            #endregion

            #region Veiculos
            ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
            {
                var validacao = new ErrosDeValidacao{
                    Mensagens = new List<string>()
                };

                if(string.IsNullOrEmpty(veiculoDTO.Nome))
                    validacao.Mensagens.Add("O nome não pode ser vazio");

                if(string.IsNullOrEmpty(veiculoDTO.Marca))
                    validacao.Mensagens.Add("A Marca não pode ficar em branco");

                if(veiculoDTO.Ano < 1950)
                    validacao.Mensagens.Add("Veículo muito antigo, aceito somete anos superiores a 1950");

                if(string.IsNullOrEmpty(veiculoDTO.Placa))
                    validacao.Mensagens.Add("A placa não pode ser vazia");

                if(string.IsNullOrEmpty(veiculoDTO.Cor))
                    validacao.Mensagens.Add("A cor não pode ser vazia");

                if(veiculoDTO.Preco <= 0)
                    validacao.Mensagens.Add("O preço deve ser maior que zero");

                return validacao;
            }

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
                var validacao = validaDTO(veiculoDTO);
                if(validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);
                
                var veiculo = new Veiculo{
                    Nome = veiculoDTO.Nome,
                    Marca = veiculoDTO.Marca,
                    Ano = veiculoDTO.Ano,
                    Placa = veiculoDTO.Placa,
                    Status = veiculoDTO.Status,
                    Cor = veiculoDTO.Cor,
                    Quilometragem = veiculoDTO.Quilometragem,
                    Preco = veiculoDTO.Preco,
                    Descricao = veiculoDTO.Descricao
                };
                veiculoServico.Incluir(veiculo);

                return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
                var veiculos = veiculoServico.Todos(pagina);

                return Results.Ok(veiculos);
            }).RequireAuthorization().WithTags("Veiculos");

            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();
                
                var validacao = validaDTO(veiculoDTO);
                if(validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);
                
                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Marca = veiculoDTO.Marca;
                veiculo.Ano = veiculoDTO.Ano;

                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculos");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);

                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculos");
            #endregion
        });
    }
}