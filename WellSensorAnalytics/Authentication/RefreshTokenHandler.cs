using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace WellSensorAnalytics.Authentication;

public class RefreshTokenHandler(
    ITokenRepository tokenRepository,
    IAuthService authService,
    ILogger<RefreshTokenHandler> logger) : DelegatingHandler
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Пытаемся добавить токен к запросу
        var tokens = await tokenRepository.GetTokensAsync();
        if (tokens is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        }

        // 2. Отправляем запрос
        var response = await base.SendAsync(request, cancellationToken);

        // 3. Если ответ не 401 Unauthorized, просто возвращаем его
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        logger.LogWarning("Request to {RequestUri} returned 401 Unauthorized. Attempting token refresh.", request.RequestUri);
        
        // 4. Логика обновления токена при 401
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Проверяем, не обновил ли токен другой поток, пока мы ждали семафор
            var newTokens = await tokenRepository.GetTokensAsync();
            if (newTokens is not null && newTokens.AccessToken != tokens?.AccessToken)
            {
                logger.LogDebug("Token was already refreshed by another thread. Retrying request.");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);
                return await base.SendAsync(request, cancellationToken);
            }

            // Если токен все еще старый, обновляем его
            var refreshed = await authService.RefreshTokenAsync();
            if (!refreshed)
            {
                logger.LogError("Failed to refresh token. Returning original 401 response.");
                return response; // Не удалось обновить, возвращаем оригинальный 401
            }

            // 5. Повторяем исходный запрос с новым токеном
            logger.LogDebug("Token refreshed successfully. Retrying the original request.");
            newTokens = await tokenRepository.GetTokensAsync();
            if(newTokens is null)
            {
                 logger.LogError("Tokens are null after successful refresh. This should not happen.");
                 return response;
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
