namespace PoCoupleQuiz.Core.Validators;

public interface IValidator<T>
{
    ValidationResult Validate(T value);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static ValidationResult Success() => new ValidationResult { IsValid = true, ErrorMessage = null };
    public static ValidationResult Failure(string errorMessage) =>
        new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
}

public class TeamNameValidator : IValidator<string>
{
    private const int MinTeamNameLength = 2;
    private const int MaxTeamNameLength = 50;

    public ValidationResult Validate(string teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return ValidationResult.Failure("Team name is required");
        }

        if (teamName.Length < MinTeamNameLength)
        {
            return ValidationResult.Failure($"Team name length must be at least {MinTeamNameLength} characters");
        }

        if (teamName.Length > MaxTeamNameLength)
        {
            return ValidationResult.Failure($"Team name length cannot exceed {MaxTeamNameLength} characters");
        }

        return ValidationResult.Success();
    }
}
