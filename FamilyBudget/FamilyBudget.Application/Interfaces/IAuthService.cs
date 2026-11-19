using FamilyBudget.Application.Dtos.Auth;

namespace FamilyBudget.Application.Interfaces;

public interface IAuthService
{
    Task<Guid> RegisterAsync(
        RegisterLocalRequest request,
        CancellationToken cancellationToken = default);
}
