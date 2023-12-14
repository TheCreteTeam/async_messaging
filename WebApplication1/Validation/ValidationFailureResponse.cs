namespace WebApplication1.Validation;

public class ValidationFailureResponse
{
    public List<string>? Errors { get; init; } = new();
}