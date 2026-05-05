namespace QuizForge.DTOs;

public class ApiResponseDto<T>
{
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponseDto<T> Success(T? data, string message = "Thao tác thành công") => new()
    {
        Message = message,
        Data = data
    };

    public static ApiResponseDto<T> Fail(T? data, string message) => new()
    {
        Message = message,
        Data = data
    };
}