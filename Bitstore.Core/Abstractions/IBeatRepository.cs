using Bitstore.Core.Models;

namespace Bitstore.Core.Abstractions;

public interface IBeatRepository
{
    Task<List<Beat>> GetAllAsync();
    Task CreateAsync(Beat beat);
    Task<bool> DeleteAsync(Guid beatId);
    Task UpdateAsync(Beat beat);
    Task<Beat> GetByIdAsync(Guid beatId);
    Task<List<Beat>> GetByUserIdAsync(Guid userId);
}