using FollowerService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FollowerService.Features.Follows.Commands.UnfollowArtist;

public class UnfollowArtistCommandHandler : IRequestHandler<UnfollowArtistCommand, bool>
{
    private readonly FollowerDbContext _context;

    public UnfollowArtistCommandHandler(FollowerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UnfollowArtistCommand request, CancellationToken cancellationToken)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.Id == request.FollowId, cancellationToken);

        if (follow == null)
        {
            return false;
        }

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
