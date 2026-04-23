using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Bitstore.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bitstore.DataAccess.Repositories;

public class BeatRepository(BitstoreDbContext context): IBeatRepository
{
    private readonly BitstoreDbContext _context = context;
   
    
    public async Task<List<Beat>> GetAll()
    {
        var beatEntities = await _context.Beats
            .AsNoTracking()
            .ToListAsync();
        
        if (beatEntities == null)
            throw new NotFoundException($"Beats not found");
        
        var beats = beatEntities
            .Select(b => Beat.FromEntity(
                b.Id,
                b.Title,
                b.Price,
                b.AudioUrl,
                b.IsPublished,
                b.Description,
                b.CoverUrl,
                b.UserId,
                b.CreatedAt))
            .ToList();
        return beats;
    }

    public async Task Create(Beat beat)
    {
        if (beat == null) 
            throw new ArgumentNullException($"Beat {beat.Id} not found");
        
        var existingBeat = await _context.Beats
            .AnyAsync(b => b.Id == beat.Id);
        
        if (existingBeat)
            throw new AlreadyExistException($"Beat {beat.Id} already exists");
        
        var beatEntity = new BeatEntity()
        {
            Id = beat.Id,
            Title = beat.Title,
            Price = beat.Price,
            Description = beat.Description,
            AudioUrl = beat.AudioUrl,
            CoverUrl = beat.CoverUrl,
            IsPublished = beat.IsPublished,
            CreatedAt = beat.CreatedAt,
            UserId = beat.UserId
        };
        
        await _context.Beats.AddAsync(beatEntity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Delete(Guid beatId)
    {
        if (beatId == Guid.Empty)
            throw new ArgumentException("BeatId cannot be empty", nameof(beatId));
        
        var beatExists = await _context.Beats
            .AnyAsync(b => b.Id == beatId);
    
        if (!beatExists)
            return false;
  
        var deletedCount = await _context.Beats
            .Where(b => b.Id == beatId)
            .ExecuteDeleteAsync();
    
        return deletedCount > 0;
    }

    public async Task Update(Beat beat)
    {
        if (beat == null)
            throw new ArgumentNullException(nameof(beat));

        if (beat.Id == Guid.Empty)
            throw new ArgumentException("BeatId cannot be empty", nameof(beat.Id));
        
        var beatExists = await _context.Beats
            .AnyAsync(b => b.Id == beat.Id);
    
        if (!beatExists)
            throw new  NotFoundException($"Beat {beat.Id} not found");
        
        await _context.Beats
            .Where(b => b.Id == beat.Id)
            .ExecuteUpdateAsync(b =>
            {
                b.SetProperty(x => x.Title, beat.Title);
                b.SetProperty(x => x.Price, beat.Price);
                b.SetProperty(x => x.AudioUrl, beat.AudioUrl);
                b.SetProperty(x => x.IsPublished, beat.IsPublished);
                b.SetProperty(x => x.Description, beat.Description);
                b.SetProperty(x => x.CoverUrl, beat.CoverUrl);
                
            });
    }

    public async Task<Beat> GetById(Guid beatId)
    {
        var beatEntity = await _context.Beats
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == beatId);
        
        if (beatEntity == null)
            throw new NotFoundException($"Beat {beatId} not found");

        var beat = Beat.FromEntity(beatEntity.Id, beatEntity.Title, beatEntity.Price, beatEntity.AudioUrl,
            beatEntity.IsPublished, beatEntity.Description, beatEntity.CoverUrl, beatEntity.UserId, beatEntity.CreatedAt);
        
        return beat;
    }

    public Task<Beat> GetByIdWithDetails(Guid beatId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Beat>> GetByUserId(Guid userId)
    {
        var beatEntities = await _context.Beats
            .Where(b => b.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
        
        if (!beatEntities.Any())
            return new List<Beat>();
        
        var beats = beatEntities
            .Select(b => Beat.FromEntity(
                b.Id,
                b.Title,
                b.Price,
                b.AudioUrl,
                b.IsPublished,
                b.Description,
                b.CoverUrl,
                b.UserId,
                b.CreatedAt))
            .ToList();

        return beats;
    }
}