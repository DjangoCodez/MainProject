using Microsoft.IdentityModel.Tokens;
using SoftOne.Common.KeyVault;
using SoftOne.Soe.Business.Util;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SoftOne.Soe.Web
{
    public class TokenHelper
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly byte[] _symmetricKey;
        public TokenHelper()
        {
            _tokenHandler = new JwtSecurityTokenHandler();
            var key = KeyVaultSecretsFetcher.GetSecret(SoftOneIdUtil.SoftOneIdSecuritySecret); 
            _symmetricKey = !string.IsNullOrEmpty(key) 
                ? GenerateKey(key)
                : GenerateKey();
        }

        public byte[] GenerateKey(string base64Encoded = null)
        {
            if (!string.IsNullOrEmpty(base64Encoded))
                return Convert.FromBase64String(base64Encoded);

            const int keySizeBytes = 32;
            var key = new byte[keySizeBytes];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        public string GenerateToken(params Claim[] claims)
        {
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = "soxe",
                Audience = "SoftOne",
                Expires = now.AddMinutes(8*60),
                SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(_symmetricKey),
                        "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256"),                


            };
            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}