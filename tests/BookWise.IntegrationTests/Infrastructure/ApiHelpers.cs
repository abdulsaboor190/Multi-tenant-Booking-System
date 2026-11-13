using System.Net.Http.Json;
using BookWise.Application.Auth;

namespace BookWise.IntegrationTests.Infrastructure;

public static class ApiHelpers
{
    public const string DefaultPassword = "CorrectHorse9Battery";

    public static string NewSlug() => $"t-{Guid.NewGuid():N}";

    public static RegisterTenantRequest NewRegisterRequest(
        string? slug = null, string email = "owner@example.com", string password = DefaultPassword)
        => new("Test Business", slug ?? NewSlug(), "Asia/Karachi", email, password);

    public static async Task<RegisterTenantResponse> RegisterTenantAsync(
        this HttpClient client, string? slug = null, string email = "owner@example.com",
        string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register-tenant", NewRegisterRequest(slug, email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RegisterTenantResponse>())!;
    }
}
