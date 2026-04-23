using Bitstore.DTO.Beat;

namespace Bitstore.Application.Abstractions;

public interface IBeatService
{
    Task<List<BeatResponse>> GetBeats();
    Task<List<BeatResponse>> GetBeatsByUser(Guid userId);
    Task CreateBeat(BeatRequest request);
    Task UpdateBeat(Guid beatId, UpdateBeatRequest request);
    Task<bool> DeleteBeat(Guid beatId);
}