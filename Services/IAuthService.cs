public interface IAuthService
{
    Task<(bool isConflict, string message)> RegisterAsync(RegisterDto dto);
    Task<(bool isUnauthorized, string? token)> LoginAsync(LoginDto dto);
}