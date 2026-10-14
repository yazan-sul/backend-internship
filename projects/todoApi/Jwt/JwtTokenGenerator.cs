using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
public class JwtTokenGenerator
{
    private readonly string _issuer = "TodoApi";
    private readonly string _audience = "TodoApiUsers";
    private readonly string _secret;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Set Jwt__Secret in .env or the environment.");
    }

    public string GenerateToken(string username)
    {
        var keyStr = Convert.FromBase64String(_secret);
        var securityKey = new SymmetricSecurityKey(keyStr);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // this for duplicate token sub
        };
        var issuer = _issuer;
        var audience = _audience;
        var expires = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token); // write token serialize token
        return tokenStr;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {

        var KeyStr = Convert.FromBase64String(_secret);
        var securityKey = new SymmetricSecurityKey(KeyStr); // decode key to pass it to validatior 
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
        return principal;
    }
}
