using Accountable.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Accountable.DataStructures.ResponseRequestData;

namespace Accountable
{
    public class JWTHelper
    {
        public static UserIDs FromUserClaims(IEnumerable<Claim> claims)
        {
            var ids = new UserIDs();
            foreach (var claim in claims)
            {
                if (claim.Type == ClaimTypes.PrimarySid)
                    ids.UserAccountID = int.Parse(claim.Value);
                if (claim.Type == ClaimTypes.Sid)
                    ids.UserID = int.Parse(claim.Value);
            }
            return ids;
        }

        public static string GenerateJWT(User user, UserAccount account, String secret)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.PrimarySid, account.Id.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer: "Accountable API",
                audience: "Accountable Users",
                claims: claims,
                expires: DateTime.Now.AddDays(7), // week expiration by default
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
