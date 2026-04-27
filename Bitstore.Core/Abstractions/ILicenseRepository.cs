using Bitstore.Core.Abstractions;
using Bitstore.Core.Enums;
using Bitstore.Core.Models;

namespace Bitstore.Core.Abstractions;

public interface ILicenseRepository
{
    Task<List<License>> GetByBeatIdAsync(Guid beatId);
    Task<License> GetByIdAsync(Guid licenseId);
    Task<License> GetByIdWithDetailsAsync(Guid licenseId);
    Task CreateAsync(License license);
    Task UpdateAsync(License license);
    Task<bool> DeleteAsync(Guid licenseId);
}