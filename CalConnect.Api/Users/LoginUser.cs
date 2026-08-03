using CalConnect.Api.Database;
using CalConnect.Api.Endpoints;
using CalConnect.Api.Users.Infrastructure;
using Microsoft.AspNetCore.Builder;

namespace CalConnect.Api.Users;

internal sealed class LoginUser(AppDbContext context, PasswordHasher passwordHasher, TokenProvider tokenProvider)
{
    public sealed record Request(string Email, string Password);

    public async Task<string> Handle(Request request)
    {
        User? user = await context.Users.GetByEmail(request.Email);

        if (user is null || !user.EmailVerified)
        {
            throw new Exception("The user was not found");
        }

        bool verified = passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!verified)
        {
            throw new Exception("The password is incorrect");
        }

        string token = tokenProvider.Create(user);

        return token;
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("users/login", async (Request request, LoginUser useCase) =>
                await useCase.Handle(request))
                .WithTags(UserEndpoints.Tag);
        }
    }
}
