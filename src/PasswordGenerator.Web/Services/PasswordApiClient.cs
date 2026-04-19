using System.Net.Http.Headers;
using System.Net.Http.Json;
using PasswordGenerator.Application.DTOs;

namespace PasswordGenerator.Web.Services;

public class PasswordApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public PasswordApiClient(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    private void SetAuthHeader()
    {
        if (_tokenService.IsAuthenticated)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenService.Token);
        }
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public async Task<RegisterResult?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return await response.Content.ReadFromJsonAsync<RegisterResult>();
    }

    public async Task<GenerateResult?> GeneratePasswordAsync(GenerateRequest request)
    {
        SetAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/generator/password", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GenerateResult>();
    }

    public async Task<GenerateResult?> GeneratePassphraseAsync(GenerateRequest request)
    {
        SetAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/generator/passphrase", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GenerateResult>();
    }

    public async Task<GenerateResult?> CalculateStrengthAsync(string password)
    {
        SetAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/generator/strength", password);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GenerateResult>();
    }

    public async Task<List<PasswordEntryDto>> GetVaultEntriesAsync()
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<List<PasswordEntryDto>>("api/vault") ?? [];
    }

    public async Task<PasswordEntryDto?> GetVaultEntryAsync(int id)
    {
        SetAuthHeader();
        return await _httpClient.GetFromJsonAsync<PasswordEntryDto>($"api/vault/{id}");
    }

    public async Task<PasswordEntryDto?> SaveVaultEntryAsync(PasswordEntryDto entry)
    {
        SetAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/vault", entry);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PasswordEntryDto>();
    }

    public async Task<PasswordEntryDto?> UpdateVaultEntryAsync(int id, PasswordEntryDto entry)
    {
        SetAuthHeader();
        var response = await _httpClient.PutAsJsonAsync($"api/vault/{id}", entry);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PasswordEntryDto>();
    }

    public async Task DeleteVaultEntryAsync(int id)
    {
        SetAuthHeader();
        var response = await _httpClient.DeleteAsync($"api/vault/{id}");
        response.EnsureSuccessStatusCode();
    }
}
