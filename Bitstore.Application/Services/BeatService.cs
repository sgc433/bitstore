using Bitstore.Application.Abstractions;
using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Bitstore.DTO.Beat;
using Serilog;

namespace Bitstore.Application.Services;

public class BeatService(
    IBeatRepository beatRepository,
    IUserRepository userRepository, 
    ICurrentUserService currentUserService) 
    : IBeatService
{
    private readonly IBeatRepository _beatRepository = beatRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<List<BeatResponse>> GetBeats()
    {
        Log.Information("Getting all beats");
        
        var beats = await _beatRepository.GetAllAsync();
        
        var response = beats.Select(b => 
            new BeatResponse(b.Title, b.Price, b.AudioUrl,
                b.Description, b.CoverUrl)).ToList();
        
        Log.Information("Returned {Count} beats", response.Count);
        
        return response;
    }

    public async Task<List<BeatResponse>> GetBeatsByUser(Guid userId)
    {
        var currentUserId = _currentUserService.GetUserId();
        var currentUserRole = _currentUserService.GetUserRole();
        
        if (currentUserRole != "Admin" && currentUserId != userId)
        {
            Log.Warning("User {CurrentUserId} attempted to access beats of user {TargetUserId}", 
                currentUserId, userId);
            throw new UnauthorizedAccessException("You can only view your own beats");
        }
        
        Log.Information("Getting beats for user {UserId}", userId);
        
        var beats = await _beatRepository.GetByUserIdAsync(userId);
        
        var response = beats.Select(b => 
            new BeatResponse(b.Title, b.Price, b.AudioUrl,
                b.Description, b.CoverUrl)).ToList();
        
        Log.Information("Returned {Count} beats", response.Count);
        
        return response;
    }

    public async Task CreateBeat(BeatRequest request)
    {   
        var currentUserId = _currentUserService.GetUserId();
        
        Log.Information("User {UserId} is creating a new beat", currentUserId);
        
        var user = await _userRepository.GetByIdAsync(currentUserId);

        var beat = Beat.Create(
            request.Title,
            request.Price,
            request.AudioUrl,
            request.IsPublished,
            request.Description,
            request.CoverUrl,
            user);
        
        await _beatRepository.CreateAsync(beat);
        
        Log.Information("Created new beat {BeatId} by {UserId}", beat.Id, user.Id);
    }
    
    public async Task UpdateBeat(Guid beatId, UpdateBeatRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();

        Log.Information("User {UserId} attempting to update beat {BeatId}", currentUserId, beatId);

        var beat = await _beatRepository.GetByIdAsync(beatId);
        
        if (beat == null)
            throw new NotFoundException($"Beat with id {beatId} not found");
        
        if (beat.UserId != currentUserId)
            throw new UnauthorizedAccessException("You can only edit your own beats");

        beat.Update(
            request.Title,
            request.Price,
            request.AudioUrl,
            beat.IsPublished,
            request.Description,
            request.CoverUrl);
            

        await _beatRepository.UpdateAsync(beat);

        Log.Information("Beat {BeatId} updated successfully by user {UserId}", beatId, currentUserId);
    }

    public async Task<bool> DeleteBeat(Guid beatId)
    {
        if (currentUserService.GetUserRole() != "Admin")
            throw new UnauthorizedAccessException("Only admin users can do this");
        
        var result = await _beatRepository.DeleteAsync(beatId);
        
        Log.Information("The result of deleting beat with id {beatId} is {result}", beatId, result);
        
        return result;
    }

    public async Task<BeatResponse> GetBeatById(Guid beatId)
    {
        Log.Information("Getting beat by id {BeatId}", beatId);

        var beat = await _beatRepository.GetByIdAsync(beatId);
        
        if (beat == null)
            throw new NotFoundException($"Beat with id {beatId} not found");
        
        var response = new BeatResponse(beat.Title, beat.Price, beat.AudioUrl,
            beat.Description, beat.CoverUrl);
        
        Log.Information("Returned beat {BeatId}", beatId);
        
        return response;
    }
}