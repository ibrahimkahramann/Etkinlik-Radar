using MediatR;

namespace FollowerService.Features.Follows.Commands.UnfollowArtist;

public record UnfollowArtistCommand(int FollowId) : IRequest<bool>;
