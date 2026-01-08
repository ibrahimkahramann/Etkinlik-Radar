namespace FollowerService.Features.Follows.Dtos;

public record FollowDto(int Id, string UserId, string ArtistId, DateTime CreatedAt);

