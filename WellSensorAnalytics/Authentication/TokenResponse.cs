using System;
using System.Text.Json.Serialization;

namespace WellSensorAnalytics.Authentication;

public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
