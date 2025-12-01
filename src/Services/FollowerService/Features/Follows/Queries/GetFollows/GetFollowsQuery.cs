using FollowerService.Features.Follows.Dtos;
using MediatR;

namespace FollowerService.Features.Follows.Queries.GetFollows;

public record GetFollowsQuery(string UserId) : IRequest<List<FollowDto>>;
