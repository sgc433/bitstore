namespace Bitstore.DTO.Beat;

public record UpdateBeatRequest(
    string Title,
    decimal Price,
    string AudioUrl,
    string? Description,
    string? CoverUrl);