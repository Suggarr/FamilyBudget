using FamilyBudget.Application.Dtos.Auth;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudget.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _registration;

    public AuthController(IAuthService registration)
    {
        _registration = registration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterLocalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountId = await _registration.RegisterAsync(request, cancellationToken);
            return Ok(new { AccountId = accountId });
        }
        catch (ArgumentException exception)
        {
            return Problem(
                title: "Invalid registration data.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (EmailAlreadyRegisteredException)
        {
            return Problem(
                title: "Email is already registered.",
                statusCode: StatusCodes.Status409Conflict);
        }
    }
}
