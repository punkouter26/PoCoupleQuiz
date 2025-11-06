using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PoCoupleQuiz.Core.Validators;

namespace PoCoupleQuiz.Server.Filters;

/// <summary>
/// Action filter that validates team names using the registered IValidator.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ValidateTeamNameAttribute : ActionFilterAttribute
{
    private readonly string _parameterName;

    /// <summary>
    /// Initializes a new instance of the ValidateTeamNameAttribute.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to validate (default: "teamName").</param>
    public ValidateTeamNameAttribute(string parameterName = "teamName")
    {
        _parameterName = parameterName;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Get the validator from DI
        var validator = context.HttpContext.RequestServices
            .GetService(typeof(IValidator<string>)) as IValidator<string>;

        if (validator == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Get the team name from action arguments
        if (!context.ActionArguments.TryGetValue(_parameterName, out var teamNameObj) || 
            teamNameObj is not string teamName)
        {
            return; // Parameter not found or not a string, let the action handle it
        }

        // Validate the team name
        var validationResult = validator.Validate(teamName);
        if (!validationResult.IsValid)
        {
            context.Result = new BadRequestObjectResult(validationResult.ErrorMessage);
        }

        base.OnActionExecuting(context);
    }
}
