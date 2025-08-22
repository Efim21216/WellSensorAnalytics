using System;

namespace WellSensorAnalytics.Authentication;

public class OAuthConfig
{
    public required string TokenEndpoint { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}
