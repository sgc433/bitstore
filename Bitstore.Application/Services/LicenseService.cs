using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.License;
using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Serilog;

namespace Bitstore.Application.Services;

public class LicenseService(
    ILicenseRepository licenseRepository,
    IBeatRepository beatRepository,
    ICurrentUserService currentUserService) : ILicenseService
{
    private readonly ILicenseRepository _licenseRepository = licenseRepository;
    private readonly IBeatRepository _beatRepository = beatRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<List<LicenseResponse>> GetLicensesByBeat(Guid beatId)
    {
        Log.Information("Getting licenses for beat {BeatId}", beatId);
        try
        {
            var beat = await _beatRepository.GetByIdAsync(beatId);
        
            if (beat == null)
                throw new NotFoundException($"Beat with id {beatId} not found");
        
            var currentUserId = _currentUserService.GetUserId();
            var currentUserRole = _currentUserService.GetUserRole();
        
            if (beat.UserId != currentUserId && currentUserRole != "Admin")
                throw new UnauthorizedAccessException("You don't have permission to view licenses for this beat");
        
            var licenses = await _licenseRepository.GetByBeatIdAsync(beatId);
        
            var response = licenses.Select(l => new LicenseResponse(
                l.Id,
                l.Type,
                l.Name,
                l.Price,
                l.BeatId
            )).ToList();
        
            Log.Information("Returned {Count} licenses for beat {BeatId}", response.Count, beatId);
            return response;
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not UnauthorizedAccessException)
        {
            Log.Error(ex, "Error getting licenses for beat {BeatId}", beatId);
            throw;
        }
    }

    public async Task<LicenseResponse> GetLicenseById(Guid licenseId)
    {
        Log.Information("Getting license by id {LicenseId}", licenseId);

        try
        {
            var license = await _licenseRepository.GetByIdAsync(licenseId);

            if (license == null)
                throw new NotFoundException($"License with id {licenseId} not found");
        
            var currentUserId = _currentUserService.GetUserId();
            var currentUserRole = _currentUserService.GetUserRole();
        
            if (license.Beat?.UserId != currentUserId && currentUserRole != "Admin")
            {
                Log.Warning("User {UserId} does not have permission to view license {LicenseId}", 
                    currentUserId, licenseId);
                throw new UnauthorizedAccessException("You don't have permission to view this license");
            }

            var response = new LicenseResponse(
                license.Id,
                license.Type,
                license.Name,
                license.Price,
                license.BeatId
            );

            Log.Information("License {LicenseId} retrieved successfully", licenseId);
            return response;
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not UnauthorizedAccessException)
        {
            Log.Error(ex, "Error getting license {LicenseId}", licenseId);
            throw;
        }
    }

    public async Task CreateLicense(Guid beatId, LicenseRequest request)
{
    if (request == null)
        throw new ArgumentNullException(nameof(request));
    
    var currentUserId = _currentUserService.GetUserId();
    var currentUserRole = _currentUserService.GetUserRole();
    
    var beat = await _beatRepository.GetByIdAsync(beatId);

    if (beat == null)
        throw new NotFoundException($"Beat with id {beatId} not found");
    
    if (beat.UserId != currentUserId && currentUserRole != "Admin")
        throw new UnauthorizedAccessException("You can only create licenses for your own beats");

    var existingLicense = await _licenseRepository.GetByIdAsync(beatId);
    
    if (existingLicense != null)
        throw new InvalidOperationException($"License of type {request.Type} already exists for this beat");

    var license = License.Create(request.Name, request.Price, request.Type, beat);

    await _licenseRepository.CreateAsync(license);

    Log.Information("Created new license {LicenseId} (Type: {Type}, Price: {Price}) for beat {BeatId}", 
        license.Id, license.Type, license.Price, beatId);
}

  public async Task UpdateLicense(Guid licenseId, LicenseRequest request)
{
    if (request == null)
        throw new ArgumentNullException(nameof(request));
    
    var currentUserId = _currentUserService.GetUserId();
    var currentUserRole = _currentUserService.GetUserRole();

    Log.Information("User {UserId} attempting to update license {LicenseId}", currentUserId, licenseId);

    var license = await _licenseRepository.GetByIdWithDetailsAsync(licenseId);

    if (license == null)
        throw new NotFoundException($"License with id {licenseId} not found");
    
    if (license.Beat == null)
        throw new InvalidOperationException($"Beat not found for license {licenseId}");

    if (license.Beat.UserId != currentUserId && currentUserRole != "Admin")
    {
        Log.Warning("User {UserId} does not have permission to update license {LicenseId}", 
            currentUserId, licenseId);
        throw new UnauthorizedAccessException("You can only update licenses for your own beats");
    }

    try
    {
        if (request.Type != license.Type)
            license.UpdateType(request.Type);
        
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != license.Name)
            license.UpdateName(request.Name);
        
        if (request.Price != license.Price)
            license.UpdatePrice(request.Price);
        
        await _licenseRepository.UpdateAsync(license);
        
        Log.Information("License {LicenseId} updated successfully by user {UserId}", 
            licenseId, currentUserId);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error updating license {LicenseId}", licenseId);
        throw;
    }
}

    public async Task<bool> DeleteLicense(Guid licenseId)
    {
        var currentUserId = _currentUserService.GetUserId();

        Log.Information("User {UserId} attempting to delete license {LicenseId}", currentUserId, licenseId);

        var license = await _licenseRepository.GetByIdWithDetailsAsync(licenseId);

        if (license == null)
            throw new NotFoundException($"License with id {licenseId} not found");

        if (license.Beat == null || license.Beat.UserId != currentUserId)
            throw new UnauthorizedAccessException("You can only delete licenses for your own beats");

        var result = await _licenseRepository.DeleteAsync(licenseId);

        Log.Information("License {LicenseId} deleted: {Result}", licenseId, result);

        return result;
    }
}