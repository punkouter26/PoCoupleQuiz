namespace PoCoupleQuiz.Core.Validators;

public interface IValidator<T>
{
    ValidationResult Validate(T value);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static ValidationResult Success() => new ValidationResult { IsValid = true };
    public static ValidationResult Failure(string errorMessage) => 
        new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
}

public class TeamNameValidator : IValidator<string>
{
    private const int MaxTeamNameLength = 100;

    public ValidationResult Validate(string teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return ValidationResult.Failure("Team name is required");
        }

        if (teamName.Length > MaxTeamNameLength)
        {
            return ValidationResult.Failure($"Team name cannot exceed {MaxTeamNameLength} characters");
        }

        return ValidationResult.Success();
    }
}
