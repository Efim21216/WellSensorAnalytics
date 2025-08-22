using System;

namespace WellSensorAnalytics.Authentication;

public class InMemoryTokenRepository : ITokenRepository
{
    private TokenResponse? _tokens;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<TokenResponse?> GetTokensAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _tokens;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetTokensAsync(TokenResponse tokens)
    {
        await _semaphore.WaitAsync();
        try
        {
            _tokens = tokens;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ClearTokensAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _tokens = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
