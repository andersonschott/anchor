using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aschott.Anchor.AspNetCore.Errors;

public static class ApiResults
{
    /// <summary>
    /// Maps a FluentResults error collection to an RFC 7807 ProblemDetails 400 response.
    /// </summary>
    public static IResult Problem(IEnumerable<IError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", errors.Select(e => e.Message)),
        };
        return Results.Problem(problem);
    }
}
