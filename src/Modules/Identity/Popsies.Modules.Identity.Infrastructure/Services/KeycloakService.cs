using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Popsies.Modules.Identity.Core.Services;

namespace Popsies.Modules.Identity.Infrastructure.Services;

internal sealed class KeycloakService : IKeycloakService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _realm;
    private readonly string _authServerUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _adminApiUrl;

    public KeycloakService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;

        var keycloakSection = configuration.GetSection("Keycloak");
        _realm = keycloakSection["Realm"] ?? throw new InvalidOperationException("Keycloak Realm is not configured");
        _authServerUrl = keycloakSection["AuthServerUrl"] ?? throw new InvalidOperationException("Keycloak AuthServerUrl is not configured");
        _clientId = keycloakSection["ClientId"] ?? throw new InvalidOperationException("Keycloak ClientId is not configured");
        _clientSecret = keycloakSection["ClientSecret"] ?? throw new InvalidOperationException("Keycloak ClientSecret is not configured");
        _adminApiUrl = keycloakSection["AdminApiUrl"] ?? $"{_authServerUrl}/admin/realms/{_realm}";
    }

    public async Task<KeycloakUserCreationResult> CreateUserAsync(
        string displayName,
        int discriminator,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync(cancellationToken);

            var username = $"{displayName}#{discriminator:D4}";

            var userRepresentation = new
            {
                username = username,
                email = email,
                enabled = true,
                emailVerified = false,
                attributes = new Dictionary<string, string[]>
                {
                    ["discriminator"] = new[] { discriminator.ToString() },
                    ["displayName"] = new[] { displayName }
                },
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                }
            };

            using var httpClient = CreateConfiguredHttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.PostAsJsonAsync(
                $"{_adminApiUrl}/users",
                userRepresentation,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new KeycloakUserCreationResult(null, false, $"Failed to create user: HTTP {response.StatusCode} - {errorContent}");
            }

            // Get the created user's ID from Location header
            var locationHeader = response.Headers.Location?.ToString();
            var keycloakUserId = locationHeader?.Split('/').Last();

            return new KeycloakUserCreationResult(keycloakUserId, true);
        }
        catch (TaskCanceledException ex)
        {
            return new KeycloakUserCreationResult(null, false, $"Request to Keycloak timed out: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return new KeycloakUserCreationResult(null, false, $"Failed to connect to Keycloak: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new KeycloakUserCreationResult(null, false, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<KeycloakAuthResult> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        var tokenEndpoint = $"{_authServerUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("username", usernameOrEmail),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("scope", "openid profile email")
        });

        using var httpClient = CreateConfiguredHttpClient();
        var response = await httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        if (tokenResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize token response");
        }

        return new KeycloakAuthResult(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.ExpiresIn);
    }

    public async Task<KeycloakTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenEndpoint = $"{_authServerUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });

        using var httpClient = CreateConfiguredHttpClient();
        var response = await httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        if (tokenResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize token response");
        }

        return new KeycloakTokenResult(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.ExpiresIn);
    }

    public async Task UpdateUserAttributesAsync(
        string keycloakUserId,
        Dictionary<string, string> attributes,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var attributesDict = attributes.ToDictionary(
            kvp => kvp.Key,
            kvp => new[] { kvp.Value });

        var updatePayload = new
        {
            attributes = attributesDict
        };

        using var httpClient = CreateConfiguredHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.PutAsJsonAsync(
            $"{_adminApiUrl}/users/{keycloakUserId}",
            updatePayload,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        using var httpClient = CreateConfiguredHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.DeleteAsync(
            $"{_adminApiUrl}/users/{keycloakUserId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private HttpClient CreateConfiguredHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        return httpClient;
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"{_authServerUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret)
        });

        using var httpClient = CreateConfiguredHttpClient();
        var response = await httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get admin token");
    }

    private sealed class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;

        // Custom property names for JSON deserialization
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessTokenJson
        {
            get => AccessToken;
            set => AccessToken = value;
        }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string RefreshTokenJson
        {
            get => RefreshToken;
            set => RefreshToken = value;
        }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresInJson
        {
            get => ExpiresIn;
            set => ExpiresIn = value;
        }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string TokenTypeJson
        {
            get => TokenType;
            set => TokenType = value;
        }
    }
}
