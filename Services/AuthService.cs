using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

public class AuthService : IAuthService
{
    private readonly TodoDbContext _db;
    private const string SecretKey = "THIS_IS_MY_SUPER_SECRET_KEY_12345";

    public AuthService(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<(bool isConflict, string message)> RegisterAsync(RegisterDto dto)
    {
        var exists = _db.Users.Any(x => x.Username == dto.Username);
        if (exists)
            return (true, "そのユーザー名は既に使われてるよ！");

        var user = new User
        {
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password) // ← ハッシュ化
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return (false, "登録成功！");
    }

    public async Task<(bool isUnauthorized, string? token)> LoginAsync(LoginDto dto)
    {
        var user = _db.Users
            .FirstOrDefault(x => x.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            return (true, null);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: null,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (false, jwt);
    }
}