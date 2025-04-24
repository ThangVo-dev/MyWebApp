using System.IdentityModel.Tokens.Jwt;

public class TokenValidator
{
    public static bool IsTokenValid(string? token, out string? message)
    {
        message = null;

        if (string.IsNullOrEmpty(token))
        {
            message = "Token is null - Please login.";
            return false;
        }

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtToken;

        try
        {
            jwtToken = handler.ReadJwtToken(token);
        }
        catch (Exception)
        {
            message = "Token is invalid.";
            return false;
        }

        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            message = "Token has expired.";
            return false;
        }

        message = $"Token is valid.";
        return true;
    }
}