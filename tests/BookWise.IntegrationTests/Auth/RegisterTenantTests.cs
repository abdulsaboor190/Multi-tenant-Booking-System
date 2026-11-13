using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookWise.Application.Auth;
using BookWise.Domain.Users;
using BookWise.Infrastructure.Persistence;
using BookWise.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookWise.IntegrationTests.Auth;

[Collection(IntegrationCollection.Name)]
public class RegisterTenantTests(ApiFactory factory)
{
    [Fact]
    public async Task Valid_registration_returns_201_and_creates_tenant_and_owner()
    {
        var client = factory.CreateClient();
        var request = ApiHelpers.NewRegisterRequest(email: "Owner@Example.COM");

        var response = await client.PostAsJsonAsync("/api/auth/register-tenant", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RegisterTenantResponse>();

        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.TenantId);
        Assert.Equal(request.Slug, body.Slug);
        Assert.NotEqual(Guid.Empty, body.OwnerUserId);

        using var scope = TestData.CreateScope(factory, body.TenantId);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True(await db.Tenants.AnyAsync(t => t.Id == body.TenantId));

        var owner = await db.Users.SingleAsync(u => u.Id == body.OwnerUserId);
        Assert.Equal(UserRole.Owner, owner.Role);
        Assert.Equal("owner@example.com", owner.Email);
    }

    [Fact]
    public async Task Password_is_stored_hashed_never_in_plain_text()
    {
        var client = factory.CreateClient();
        var body = await client.RegisterTenantAsync(email: $"hashed-{Guid.NewGuid():N}@example.com");

        using var scope = TestData.CreateScope(factory, body.TenantId);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var owner = await db.Users.SingleAsync(u => u.Id == body.OwnerUserId);

        Assert.NotEqual(ApiHelpers.DefaultPassword, owner.PasswordHash);
        Assert.DoesNotContain(ApiHelpers.DefaultPassword, owner.PasswordHash);
    }

    [Fact]
    public async Task Duplicate_slug_returns_409_problem_details()
    {
        var client = factory.CreateClient();
        var slug = ApiHelpers.NewSlug();

        await client.RegisterTenantAsync(slug);

        var response = await client.PostAsJsonAsync(
            "/api/auth/register-tenant", ApiHelpers.NewRegisterRequest(slug, "other@example.com"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Invalid_input_returns_400_with_field_errors()
    {
        var client = factory.CreateClient();
        var request = new RegisterTenantRequest(
            "Test Business", "Bad Slug", "Mars/Olympus", "owner@example.com", "weak");

        var response = await client.PostAsJsonAsync("/api/auth/register-tenant", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty("Slug", out _));
        Assert.True(errors.TryGetProperty("TimeZone", out _));
        Assert.True(errors.TryGetProperty("Password", out _));
    }

    [Fact]
    public async Task Request_without_a_body_returns_400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/auth/register-tenant", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Parallel_registrations_of_the_same_slug_create_exactly_one_tenant()
    {
        var client = factory.CreateClient();
        var slug = ApiHelpers.NewSlug();

        var registrations = Enumerable.Range(0, 5)
            .Select(i => client.PostAsJsonAsync(
                "/api/auth/register-tenant",
                ApiHelpers.NewRegisterRequest(slug, $"owner{i}@example.com")))
            .ToArray();

        var responses = await Task.WhenAll(registrations);

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(4, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        using var scope = TestData.CreateScope(factory, null);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tenant = await db.Tenants.SingleAsync(t => t.Slug == slug);

        Assert.Equal(1, await db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenant.Id));
    }
}
