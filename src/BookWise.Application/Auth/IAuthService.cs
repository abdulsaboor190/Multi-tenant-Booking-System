namespace BookWise.Application.Auth;

public interface IAuthService
{
    Task<RegisterTenantResponse> RegisterTenantAsync(RegisterTenantRequest request, CancellationToken cancellationToken);
}
