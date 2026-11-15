using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleFailure(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Problem();
        }
        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return BuildValidationProblem(errors);
        }

        return MapSingleError(errors[0]);
    }

    private BadRequestObjectResult BuildValidationProblem(List<Error> errors)
    {
        var validationErrors = errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        return BadRequest(new ValidationProblemDetails(validationErrors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation.Failed"
        });
    }

    private IActionResult MapSingleError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => NotFound(BuildProblem(StatusCodes.Status404NotFound, error)),
            ErrorType.Unauthorized => Unauthorized(BuildProblem(StatusCodes.Status401Unauthorized, error)),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, BuildProblem(StatusCodes.Status403Forbidden, error)),
            ErrorType.Conflict => Conflict(BuildProblem(StatusCodes.Status409Conflict, error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, BuildProblem(StatusCodes.Status500InternalServerError, error))
        };
    }

    private static ProblemDetails BuildProblem(int statusCode, Error error)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description
        };
    }
}
