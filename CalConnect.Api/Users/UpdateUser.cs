using CalConnect.Api.Database;
using CalConnect.Api.Users.Infrastructure;
using Microsoft.AspNetCore.OutputCaching;

namespace CalConnect.Api.Users;

internal sealed class UpdateUser(AppDbContext context, IOutputCacheStore cacheStore)
{
    internal sealed record Request(string FirstName, string LastName);

    internal sealed record Command(Guid UserId, string FirstName, string LastName);

    public async Task Handle(Command command)
    {
        User? user = await context.Users.GetById(command.UserId);

        if (user is null || !user.EmailVerified)
        {
            throw new InvalidOperationException("The user was not found.");
        }

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;

        await context.SaveChangesAsync();

        await cacheStore.EvictByTagAsync(UserEndpoints.Tag, default);
    }
}
