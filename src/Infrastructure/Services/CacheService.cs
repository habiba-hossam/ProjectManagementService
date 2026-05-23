using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using System.Text.Json;

namespace ProjectManagementAPI.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            return data is null ? default : JsonSerializer.Deserialize<T>(data);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
            };
            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options, cancellationToken);
        }
        catch { 
            /* To stop Cache failures from breaking the application */             
            }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch { 
            /* To stop Cache failures from breaking the application */             
            }
    }
    public async Task<int> GetVersionAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(key, cancellationToken);
 _logger.LogInformation(
        "Cache version incremented. Key: {CacheKey}, version: {value},",key, value);


        return value is null ? 1 : int.Parse(value);
    }

    public async Task IncrementVersionAsync(string key, CancellationToken cancellationToken = default)
    {
        var current = await GetVersionAsync(key, cancellationToken);
        var newValue = current + 1;
 _logger.LogInformation(
        "Cache version incremented. Key: {CacheKey}, OldVersion: {OldVersion}, NewVersion: {NewVersion}",
        key, current, newValue);
        await _cache.SetStringAsync(key, newValue.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },
            cancellationToken);
    }
}
