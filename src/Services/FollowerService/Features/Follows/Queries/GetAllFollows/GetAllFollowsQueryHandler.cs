using FollowerService.Features.Follows.Dtos;
using FollowerService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FollowerService.Features.Follows.Queries.GetAllFollows;

public class GetAllFollowsQueryHandler : IRequestHandler<GetAllFollowsQuery, List<FollowDto>>
{
    private readonly FollowerDbContext _context;

    public GetAllFollowsQueryHandler(FollowerDbContext context)
    {
        _context = context;
    }

    public async Task<List<FollowDto>> Handle(GetAllFollowsQuery request, CancellationToken cancellationToken)
    {
        var follows = await _context.Follows
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowDto(f.Id, f.UserId, f.ArtistId, f.CreatedAt))
            .ToListAsync(cancellationToken);

        return follows;
    }
}
