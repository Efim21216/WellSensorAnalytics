using System;

namespace WellSensorAnalytics.Authentication;

public interface ITokenRepository
{
    Task<TokenResponse?> GetTokensAsync();
    Task SetTokensAsync(TokenResponse tokens);
    Task ClearTokensAsync();
}
