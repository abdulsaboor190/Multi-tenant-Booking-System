// TEMPORARY (remove in Phase 2)
namespace BookWise.Api.Dev;

public static class DevEndpoints
{
    public static void MapDevEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/dev").WithTags("Dev (temporary)");

        group.MapGet("/boom", (Func<IResult>)(() =>
            throw new InvalidOperationException("Deliberate test exception: this text must never reach the client.")));
    }
}
