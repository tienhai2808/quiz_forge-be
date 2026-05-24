using Microsoft.AspNetCore.Mvc;
using QuizForge.Services;

namespace QuizForge.Controllers;

[ApiController]
[Route("quizzes")]
public class QuizController(IQuizService quizService) : ControllerBase
{
    private readonly IQuizService _quizService = quizService;

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return Ok();
    }
}