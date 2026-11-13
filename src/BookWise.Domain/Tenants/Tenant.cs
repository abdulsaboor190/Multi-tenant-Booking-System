namespace BookWise.Domain.Tenants;

public class Tenant
{
    private Tenant() { }

    public Tenant(string name, string slug, string timeZone, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Slug = slug;
        TimeZone = timeZone;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string TimeZone { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}
