using FollowerService.Features.Follows.Dtos;
using FollowerService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FollowerService.Features.Follows.Queries.GetFollows;

public class GetFollowsQueryHandler : IRequestHandler<GetFollowsQuery, List<FollowDto>>
{
    private readonly FollowerDbContext _context;

    public GetFollowsQueryHandler(FollowerDbContext context)
    {
        _context = context;
    }

    public async Task<List<FollowDto>> Handle(GetFollowsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Follows
            .Where(f => f.UserId == request.UserId)
            .Select(f => new FollowDto(f.ArtistId, f.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
