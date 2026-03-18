using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.Pieces.Abstractions;
using Renova.Services.Features.Pieces.Contracts;

namespace Renova.Api.Features.Pieces.V1;

// Controller HTTP do modulo 06 com cadastro, consulta e upload de imagens da peca.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/pieces")]
public sealed class PiecesController : RenovaControllerBase
{
    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    ];

    private readonly IPieceService _pieceService;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Inicializa o controller com o service do modulo e o ambiente do host.
    /// </summary>
    public PiecesController(IPieceService pieceService, IWebHostEnvironment environment)
    {
        _pieceService = pieceService;
        _environment = environment;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega as opcoes e cadastros auxiliares da loja ativa para o modulo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _pieceService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista as pecas da loja ativa com filtros de busca e status.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<PieceSummaryResponse>>>> List(
        [FromQuery] PieceListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _pieceService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{pecaId:guid}")]
    /// <summary>
    /// Carrega o detalhe completo de uma peca da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceDetailResponse>>> GetById(Guid pecaId, CancellationToken cancellationToken)
    {
        var response = await _pieceService.ObterDetalheAsync(pecaId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.PecasCadastrar)]
    /// <summary>
    /// Cria uma nova peca e gera a entrada inicial de estoque.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceDetailResponse>>> Create(
        [FromBody] CreatePieceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _pieceService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{pecaId:guid}")]
    [RequirePermission(AccessPermissionCodes.PecasCadastrar)]
    /// <summary>
    /// Atualiza os dados cadastrais da peca informada.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceDetailResponse>>> Update(
        Guid pecaId,
        [FromBody] UpdatePieceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _pieceService.AtualizarAsync(pecaId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{pecaId:guid}/images")]
    [RequirePermission(AccessPermissionCodes.PecasCadastrar)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
    /// <summary>
    /// Armazena fisicamente uma imagem e registra o vinculo dela com a peca.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceImageResponse>>> UploadImage(
        Guid pecaId,
        [FromForm] IFormFile arquivo,
        [FromForm] int ordem,
        [FromForm] string tipoVisibilidade,
        CancellationToken cancellationToken)
    {
        var relativeUrl = await SavePieceImageAsync(pecaId, arquivo, cancellationToken);
        var response = await _pieceService.AdicionarImagemAsync(
            pecaId,
            new RegisterPieceImageRequest(relativeUrl, ordem, tipoVisibilidade),
            cancellationToken);

        return OkEnvelope(response);
    }

    [HttpPut("{pecaId:guid}/images/{imagemId:guid}")]
    [RequirePermission(AccessPermissionCodes.PecasCadastrar)]
    /// <summary>
    /// Atualiza os metadados de uma imagem ja vinculada a peca.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceImageResponse>>> UpdateImage(
        Guid pecaId,
        Guid imagemId,
        [FromBody] UpdatePieceImageRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _pieceService.AtualizarImagemAsync(pecaId, imagemId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpDelete("{pecaId:guid}/images/{imagemId:guid}")]
    [RequirePermission(AccessPermissionCodes.PecasCadastrar)]
    /// <summary>
    /// Remove uma imagem da peca e apaga o arquivo local quando aplicavel.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PieceImageResponse>>> DeleteImage(
        Guid pecaId,
        Guid imagemId,
        CancellationToken cancellationToken)
    {
        var response = await _pieceService.RemoverImagemAsync(pecaId, imagemId, cancellationToken);
        DeleteLocalImageIfExists(response.UrlArquivo);
        return OkEnvelope(response);
    }

    /// <summary>
    /// Salva o arquivo de imagem em disco e devolve a URL relativa publicada pela API.
    /// </summary>
    private async Task<string> SavePieceImageAsync(Guid pecaId, IFormFile arquivo, CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            throw new InvalidOperationException("Selecione uma imagem valida para a peca.");
        }

        if (!arquivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Envie apenas arquivos de imagem.");
        }

        var extension = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Formato de imagem nao suportado.");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var directory = Path.Combine(webRoot, "uploads", "pieces", pecaId.ToString("N"));
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await arquivo.CopyToAsync(stream, cancellationToken);

        return $"/uploads/pieces/{pecaId:N}/{fileName}";
    }

    /// <summary>
    /// Remove o arquivo local quando a imagem estiver armazenada sob a pasta de uploads da API.
    /// </summary>
    private void DeleteLocalImageIfExists(string urlArquivo)
    {
        if (!urlArquivo.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var relative = urlArquivo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, relative);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
