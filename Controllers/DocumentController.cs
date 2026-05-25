using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Services;

namespace QuizForge.Controllers;

[ApiController]
[Route("documents")]
public class DocumentController(IDocumentService documentService) : ControllerBase
{
    private readonly IDocumentService _documentService = documentService;

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Upload(
        UploadDocumentRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();
        var uploadRes = await _documentService.UploadAsync(dto, userId, cancellationToken);
        var response = ApiResponseDto<object>.Success(uploadRes);
        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDocumentRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();
        var documentId = await _documentService.CreateAsync(dto, userId, cancellationToken);
        var response = ApiResponseDto<object>.Success(new
        {
            id = documentId.ToString(),
        }, "Thêm tài liệu thành công");
        return Ok(response);
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!long.TryParse(sub, out var userId))
            throw new UnauthorizedException("Access token không hợp lệ");

        return userId;
    }

}