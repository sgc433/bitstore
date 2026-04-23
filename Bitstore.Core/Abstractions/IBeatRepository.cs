using Bitstore.Core.Models;

namespace Bitstore.Core.Abstractions;

public interface IBeatRepository
{
    Task<List<Beat>> GetAll();
    Task Create(Beat beat);
    Task<bool> Delete(Guid beatId);
    Task Update(Beat beat);
    Task<Beat> GetById(Guid beatId);
    Task<Beat> GetByIdWithDetails(Guid beatId);
    Task<List<Beat>> GetByUserId(Guid userId);
}