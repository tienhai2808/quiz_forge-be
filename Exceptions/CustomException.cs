namespace QuizForge.Exceptions;

public class CustomException(string message, int status) : Exception(message)
{
    public int Status { get; } = status;
}

public class NotFoundException(string message) : CustomException(message, 404);

public class ValidationException(string message) : CustomException(message, 400);

public class UnauthorizedException(string message) : CustomException(message, 401);

public class ConflictException(string message) : CustomException(message, 409);

public class ForbiddenException(string message) : CustomException(message, 403);

public class InternalServerException(string message) : CustomException(message, 500);

public class ExternalServiceException(string message) : CustomException(message, 502);