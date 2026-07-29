using System.Security.Claims;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace CalConnect.Api.Users.Infrastructure;

internal static class ClaimsPrincipalExtensions
{
    public static Guid UserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        Claim? claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                 ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                 ?? principal.FindFirst("sub");

        if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
        {
            throw new UnauthorizedAccessException("O claim de ID do usuário ('sub' ou 'NameIdentifier') não foi encontrado no token.");
        }

        if (Guid.TryParse(claim.Value, out Guid userId))
        {
            return userId;
        }

        throw new FormatException($"O valor do claim de ID do usuário não é um GUID válido. Valor recebido: {claim.Value}");
    }
}
