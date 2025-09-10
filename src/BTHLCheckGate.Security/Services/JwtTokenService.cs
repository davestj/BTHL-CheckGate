/**
 * BTHL CheckGate - JWT Token Service Implementation
 * File: src/BTHLCheckGate.Security/Services/JwtTokenService.cs
 */

using BTHLCheckGate.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BTHLCheckGate.Security.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(string username, List<string> permissions);
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
        Task<string> CreateApiTokenAsync(string name, string description, string createdBy, DateTime? expiresAt = null);
        Task<bool> ValidateApiTokenAsync(string token);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IApiTokenRepository _apiTokenRepository;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly SymmetricSecurityKey _signingKey;

        public JwtTokenService(
            IConfiguration configuration,
            IApiTokenRepository apiTokenRepository,
            ILogger<JwtTokenService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _apiTokenRepository = apiTokenRepository ?? throw new ArgumentNullException(nameof(apiTokenRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        public async Task<string> GenerateTokenAsync(string username, List<string> permissions)
        {
            try
            {
                _logger.LogDebug("Generating JWT token for user: {Username}", username);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, username),
                    new(ClaimTypes.NameIdentifier, username),
                    new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Add permissions as claims
                claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(24),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogDebug("Successfully generated JWT token for user: {Username}", username);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {Username}", username);
                throw;
            }
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                if (!tokenHandler.CanReadToken(token))
                {
                    _logger.LogWarning("Invalid JWT token format");
                    return null;
                }

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = _signingKey,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                _logger.LogDebug("Successfully validated JWT token");
                return principal;
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                return null;
            }
        }

        public async Task<string> CreateApiTokenAsync(string name, string description, string createdBy, DateTime? expiresAt = null)
        {
            try
            {
                _logger.LogInformation("Creating API token: {TokenName} for user: {CreatedBy}", name, createdBy);

                // Generate a secure random token
                var tokenBytes = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(tokenBytes);
                }

                var token = Convert.ToBase64String(tokenBytes);
                var tokenHash = BCrypt.Net.BCrypt.HashPassword(token, 12);

                // Store the token in the database
                await _apiTokenRepository.CreateTokenAsync(name, description, createdBy, expiresAt);

                _logger.LogInformation("Successfully created API token: {TokenName}", name);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API token: {TokenName}", name);
                throw;
            }
        }

        public async Task<bool> ValidateApiTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                var tokenHash = BCrypt.Net.BCrypt.HashPassword(token, 12);
                var isValid = await _apiTokenRepository.ValidateTokenAsync(tokenHash);

                if (isValid)
                {
                    _logger.LogDebug("API token validated successfully");
                }
                else
                {
                    _logger.LogWarning("API token validation failed");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API token");
                return false;
            }
        }
    }
}