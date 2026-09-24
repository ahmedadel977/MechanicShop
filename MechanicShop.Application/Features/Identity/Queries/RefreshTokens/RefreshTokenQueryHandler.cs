using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryHandler(ILogger<RefreshTokenQueryHandler> logger, IIdentityService identityService, IAppDbContext context, ITokenProvider tokenProvider)
    : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IAppDbContext _context = context;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken ct)
    {
      var principal = _tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);
        if (principal is null)
        {
            _logger.LogWarning("Invalid expired access token.");
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }
        var userid= principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userid))
        {
            _logger.LogWarning("UserId claim is missing or invalid in the expired access token.");
            return ApplicationErrors.UserIdClaimInvalid;
        }
        var getuserResult = await _identityService.GetUserByIdAsync(userid);
        if (getuserResult.IsError)
        {
            _logger.LogWarning("User not found for UserId: {UserId}", userid);
            return ApplicationErrors.UserNotFound;
        }
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userid, ct);
        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.LogError("Refresh token has expired");

            return ApplicationErrors.RefreshTokenExpired;
        }
        var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(getuserResult.Value,ct);
        if (generateTokenResult.IsError)
        {
            _logger.LogError("Token generation failed for UserId: {UserId}", userid);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}