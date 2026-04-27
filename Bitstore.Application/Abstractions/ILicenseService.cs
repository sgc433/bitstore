using Bitstore.Application.DTO.License;
using Bitstore.Core.Models;

namespace Bitstore.Application.Abstractions;

public interface ILicenseService
{
    Task<List<LicenseResponse>> GetLicensesByBeat(Guid beatId);
    Task<LicenseResponse> GetLicenseById(Guid licenseId);
    Task CreateLicense(Guid beatId, LicenseRequest request);
    Task UpdateLicense(Guid licenseId, LicenseRequest request);
    Task<bool> DeleteLicense(Guid licenseId);
}