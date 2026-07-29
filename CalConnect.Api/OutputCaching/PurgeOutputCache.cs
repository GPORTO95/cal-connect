using Microsoft.AspNetCore.OutputCaching;

namespace CalConnect.Api.OutputCaching;

internal sealed class PurgeOutputCache //: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("purge", async (IOutputCacheStore outputCacheStore) =>
        {
            await outputCacheStore.EvictByTagAsync("all", default);

            return Results.Ok();
        });
    }
}
