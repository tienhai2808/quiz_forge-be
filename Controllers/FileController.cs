using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizForge.DTOs;
using QuizForge.Services;

namespace QuizForge.Controllers;

[ApiController]
[Route("files")]
public class FileController(
    IFileService fileService
) : ControllerBase
{
    private readonly IFileService _fileService = fileService;

    [HttpPost("presigned-urls/uploads")]
    public async Task<IActionResult> UploadPresignedUrls(
        UploadPresignedUrlsRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var presignedUrls = await _fileService.CreatePresignedUrlsAsync(dto, cancellationToken);
        var response = ApiResponseDto<List<UploadPresignedUrlResponseDto>>.Success(
            presignedUrls
        );
        return Ok(response);
    }
}
