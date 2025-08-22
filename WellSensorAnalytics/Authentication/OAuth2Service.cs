using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WellSensorAnalytics.Authentication;

public class OAuth2Service(
    HttpClient httpClient,
    ITokenRepository tokenRepository,
    IOptions<OAuthConfig> config,
    ILogger<OAuth2Service> logger) : IAuthService
{
    private readonly OAuthConfig _config = config.Value;

    public Task<bool> LoginAsync()
    {
        logger.LogDebug("Attempting to login with password grant type...");
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["username"] = _config.Username,
            ["password"] = _config.Password
        });

        return GetAndStoreTokensAsync(content);
    }

    public async Task<bool> RefreshTokenAsync()
    {
        logger.LogDebug("Attempting to refresh the token...");
        var currentTokens = await tokenRepository.GetTokensAsync();
        if (currentTokens is null)
        {
            logger.LogWarning("No refresh token found. Cannot refresh.");
            return false;
        }

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["refresh_token"] = currentTokens.RefreshToken
        });

        if (!await GetAndStoreTokensAsync(content))
        {
            logger.LogWarning("Refresh token failed. Attempting to log in again with password.");
            await tokenRepository.ClearTokensAsync();
            return await LoginAsync();
        }
        
        return true;
    }

    private async Task<bool> GetAndStoreTokensAsync(FormUrlEncodedContent content)
    {
        try
        {
            var response = await httpClient.PostAsync(_config.TokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("OAuth request failed with status {StatusCode}: {Error}", response.StatusCode, error);
                return false;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var tokens = await JsonSerializer.DeserializeAsync<TokenResponse>(stream);

            if (tokens is null)
            {
                logger.LogError("Failed to deserialize token response.");
                return false;
            }

            await tokenRepository.SetTokensAsync(tokens);
            logger.LogDebug("Successfully retrieved and stored new tokens.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred during the token request.");
            return false;
        }
    }
}
