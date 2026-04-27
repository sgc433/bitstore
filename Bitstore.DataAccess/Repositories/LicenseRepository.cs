using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Bitstore.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bitstore.DataAccess.Repositories;

public class LicenseRepository(BitstoreDbContext context) : ILicenseRepository
{
    private readonly BitstoreDbContext _context = context;

    public async Task<List<License>> GetByBeatIdAsync(Guid beatId)
{
    if (beatId == Guid.Empty)
        throw new ArgumentException("BeatId cannot be empty", nameof(beatId));
    
    Log.Information("Getting licenses for beat {BeatId}", beatId);

    try
    {
        var licenseEntities = await _context.Licenses
            .AsNoTracking()
            .Include(l => l.Beat)
            .Where(l => l.BeatId == beatId)
            .OrderBy(l => l.Type)
            .ToListAsync();

        if (!licenseEntities.Any())
        {
            Log.Information("No licenses found for beat {BeatId}", beatId);
            return new List<License>();
        }

        var licenses = licenseEntities.Select(licenseEntity =>
        {
            if (licenseEntity.Beat == null)
                throw new InvalidOperationException($"Beat not found for license {licenseEntity.Id}");
            
            var beat = Beat.FromEntity(
                licenseEntity.Beat.Id,
                licenseEntity.Beat.Title,
                licenseEntity.Beat.Price,
                licenseEntity.Beat.AudioUrl,
                licenseEntity.Beat.IsPublished,
                licenseEntity.Beat.Description,
                licenseEntity.Beat.CoverUrl,
                licenseEntity.Beat.UserId,
                licenseEntity.Beat.CreatedAt
            );
            
            return License.FromEntity(
                licenseEntity.Id,
                licenseEntity.Type,
                licenseEntity.Price,
                licenseEntity.Name,
                licenseEntity.BeatId,
                beat
            );
        }).ToList();

        Log.Information("Returned {Count} licenses for beat {BeatId}", licenses.Count, beatId);
        return licenses;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting licenses for beat {BeatId}", beatId);
        throw;
    }
}

    public async Task<License> GetByIdAsync(Guid licenseId)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
    
        Log.Information("Getting license by id {LicenseId}", licenseId);

        try
        {
            var licenseEntity = await _context.Licenses
                .AsNoTracking()
                .Include(l => l.Beat)
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (licenseEntity == null)
                throw new NotFoundException($"License with id {licenseId} not found");
            
            if (licenseEntity.Beat == null)
                throw new InvalidOperationException($"Beat not found for license {licenseId}");

            var beat = Beat.FromEntity(
                licenseEntity.Beat.Id,
                licenseEntity.Beat.Title,
                licenseEntity.Beat.Price,
                licenseEntity.Beat.AudioUrl,
                licenseEntity.Beat.IsPublished,
                licenseEntity.Beat.Description,
                licenseEntity.Beat.CoverUrl,
                licenseEntity.Beat.UserId,
                licenseEntity.Beat.CreatedAt
            );

            var license = License.FromEntity(
                licenseEntity.Id,
                licenseEntity.Type,
                licenseEntity.Price,
                licenseEntity.Name,
                licenseEntity.BeatId,
                beat
            );

            Log.Information("License {LicenseId} retrieved successfully", licenseId);
            return license;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            Log.Error(ex, "Error getting license {LicenseId}", licenseId);
            throw;
        }
    }

   public async Task<License> GetByIdWithDetailsAsync(Guid licenseId)
{
    if (licenseId == Guid.Empty)
        throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
    
    Log.Information("Getting license by id with details {LicenseId}", licenseId);

    try
    {
        var licenseEntity = await _context.Licenses
            .AsNoTracking()
            .Include(l => l.Beat)
            .Include(l => l.OrderItems)
            .FirstOrDefaultAsync(l => l.Id == licenseId);

        if (licenseEntity == null)
            throw new NotFoundException($"License with id {licenseId} not found");
        
        if (licenseEntity.Beat == null)
            throw new NotFoundException($"Beat not found for license {licenseId}");
        
        var beat = Beat.FromEntity(
            licenseEntity.Beat.Id,
            licenseEntity.Beat.Title,
            licenseEntity.Beat.Price,
            licenseEntity.Beat.AudioUrl,
            licenseEntity.Beat.IsPublished,
            licenseEntity.Beat.Description,
            licenseEntity.Beat.CoverUrl,
            licenseEntity.Beat.UserId,
            licenseEntity.Beat.CreatedAt
        );
        
        var orderItems = licenseEntity.OrderItems?
            .Select(item => OrderItem.FromEntity(
                item.Id,
                item.Price,
                item.OrderId,
                item.BeatId,
                item.LicenseId,
                item.SellerId
            ))
            .ToList() ?? new List<OrderItem>();
        
        var license = License.FromEntity(
            licenseEntity.Id,
            licenseEntity.Type,
            licenseEntity.Price,
            licenseEntity.Name,
            licenseEntity.BeatId,
            beat, 
            orderItems
        );

        Log.Information("License {LicenseId} retrieved successfully", licenseId);
        return license;
    }
    catch (Exception ex) when (ex is not NotFoundException)
    {
        Log.Error(ex, "Error getting license {LicenseId}", licenseId);
        throw;
    }
}

    public async Task CreateAsync(License license)
    {
        if (license == null)
            throw new ArgumentNullException(nameof(license));

        var existingLicense = await _context.Licenses
            .AnyAsync(l => l.Id == license.Id);
        
        if (existingLicense)
            throw new AlreadyExistException($"License {license.Id} already exists");
        
        Log.Information("Creating license for beat {BeatId}", license.BeatId);

        var licenseEntity = new LicenseEntity
        {
            Id = license.Id,
            Type = license.Type,
            Name = license.Name,
            Price = license.Price,
            BeatId = license.BeatId
        };

        await _context.Licenses.AddAsync(licenseEntity);
        await _context.SaveChangesAsync();

        Log.Information("Created license {LicenseId} for beat {BeatId}", license.Id, license.BeatId);
    }

    public async Task UpdateAsync(License license)
    {
        if (license == null)
            throw new ArgumentNullException(nameof(license));

        if (license.Id == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(license.Id));

        Log.Information("Updating license {LicenseId}", license.Id);

        var licenseExists = await _context.Licenses
            .AnyAsync(l => l.Id == license.Id);

        if (!licenseExists)
            throw new NotFoundException($"License {license.Id} not found");

        await _context.Licenses
            .Where(l => l.Id == license.Id)
            .ExecuteUpdateAsync(l => l
                .SetProperty(x => x.Type, license.Type)
                .SetProperty(x => x.Name, license.Name)
                .SetProperty(x => x.Price, license.Price));

        Log.Information("Updated license {LicenseId}", license.Id);
    }

    public async Task<bool> DeleteAsync(Guid licenseId)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));

        Log.Information("Deleting license {LicenseId}", licenseId);

        var licenseExists = await _context.Licenses
            .AnyAsync(l => l.Id == licenseId);

        if (!licenseExists)
            return false;

        var deletedCount = await _context.Licenses
            .Where(l => l.Id == licenseId)
            .ExecuteDeleteAsync();

        Log.Information("Deleted license {LicenseId}: {Result}", licenseId, deletedCount > 0);

        return deletedCount > 0;
    }
}