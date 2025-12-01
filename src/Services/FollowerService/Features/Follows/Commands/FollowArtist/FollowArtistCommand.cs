using MediatR;

namespace FollowerService.Features.Follows.Commands.FollowArtist;

public record FollowArtistCommand(string UserId, string ArtistId) : IRequest<bool>;
