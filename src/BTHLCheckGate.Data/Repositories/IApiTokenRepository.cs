/**
 * BTHL CheckGate - API Token Repository Interface
 * File: src/BTHLCheckGate.Data/Repositories/IApiTokenRepository.cs
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Data.Repositories
{
    public interface IApiTokenRepository
    {
        Task<bool> ValidateTokenAsync(string tokenHash);
        Task<string> CreateTokenAsync(string name, string description, string createdBy, DateTime? expiresAt = null);
        Task RevokeTokenAsync(string tokenHash);
        Task<List<ApiTokenInfo>> GetTokensAsync();
    }

    public class ApiTokenInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}