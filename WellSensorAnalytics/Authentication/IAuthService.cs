using System;

namespace WellSensorAnalytics.Authentication;

public interface IAuthService
{
    Task<bool> LoginAsync();
    Task<bool> RefreshTokenAsync();
}
