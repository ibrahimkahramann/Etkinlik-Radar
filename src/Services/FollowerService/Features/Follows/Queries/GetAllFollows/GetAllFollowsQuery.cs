using FollowerService.Features.Follows.Dtos;
using MediatR;

namespace FollowerService.Features.Follows.Queries.GetAllFollows;

public record GetAllFollowsQuery() : IRequest<List<FollowDto>>;
