using System.Security.Claims;
using CalConnect.Api.Users.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace CalConnect.Api.Users;

internal static class UserEndpoints
{
    public const string Tag = "Users";
    public const string VerifyEmail = "VerifyEmail";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("users/register", async (RegisterUser.Request request, RegisterUser useCase) =>
            await useCase.Handle(request))
            .WithTags(Tag);

        builder.MapPut("users/profile/{id:guid}", async (Guid id, UpdateUser.Request request, UpdateUser useCase, ClaimsPrincipal claimsPrincipal) =>
        {
            if (id != claimsPrincipal.UserId())
            {
                return Results.Forbid();
            }

            await useCase.Handle(new UpdateUser.Command(id, request.FirstName, request.LastName));

            return Results.NoContent();
        })
        .WithTags(Tag)
        .RequireAuthorization();

        builder.MapPost("users/login", async (LoginUser.Request request, LoginUser useCase) =>
            await useCase.Handle(request))
            .WithTags(Tag);

        builder.MapGet("users/verify-email", async (Guid token, VerifyEmail useCase) =>
        {
            bool success = await useCase.Handle(token);

            return success ? Results.Ok() : Results.BadRequest("Verification token expired");
        })
        .WithTags(Tag)
        .WithName(VerifyEmail);

        builder.MapGet("users/{id:guid}", async (Guid id, GetUser useCase, ClaimsPrincipal claimsPrincipal) =>
        {
            if (id != claimsPrincipal.UserId())
            {
                return Results.Forbid();
            }

            GetUser.UserResponse? user = await useCase.Handle(id);

            return user is not null ? Results.Ok(user) : Results.NotFound();
        })
        .WithTags(Tag)
        .RequireAuthorization()
        .CacheOutput(builder => builder
            .Expire(TimeSpan.FromMinutes(1))
            .Tag(UserEndpoints.Tag)
            .VaryByValue((httpcontext, _) =>
            {
                return ValueTask.FromResult(new KeyValuePair<string, string>(
                    nameof(ClaimsPrincipalExtensions.UserId),
                    httpcontext.User.UserId().ToString()));
            }), 
            true);

        return builder;
    }
}
