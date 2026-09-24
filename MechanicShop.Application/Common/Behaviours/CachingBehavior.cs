using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicShop.Application.Common.Behaviours
{
    public class CachingBehavior<TRequest, TResponse>(
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        HybridCache cache)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly HybridCache _cache = cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (request is not ICachedQuery cacheableRequest)
            {
                return await next();
            }
            _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);
            var result = await _cache.GetOrCreateAsync<TResponse>(
                cacheableRequest.CacheKey,
                _ => new ValueTask<TResponse>((TResponse)(object)null!),
                new HybridCacheEntryOptions
                {
                    Flags = HybridCacheEntryFlags.DisableUnderlyingData
                },
                cancellationToken:ct);
            if  (result is null)
            {
                _logger.LogInformation("Cache miss for {RequestName}", typeof(TRequest).Name);
               
                result = await next();
                if ( result is IResult res && res.IsSuccess)
                {
                    await _cache.SetAsync(
                  cacheableRequest.CacheKey,
                  result,
                  new HybridCacheEntryOptions
                  {
                      Expiration = cacheableRequest.Expiration,
                  },
                  cancellationToken: ct);
                }
              

                  
            }
            else
            {
                _logger.LogInformation("Cache hit for {RequestName}", typeof(TRequest).Name);
            }
            return result;

        }
    }
}
