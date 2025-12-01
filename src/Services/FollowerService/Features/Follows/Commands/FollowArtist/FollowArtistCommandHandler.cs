using FollowerService.Entities;
using FollowerService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FollowerService.Features.Follows.Commands.FollowArtist;

public class FollowArtistCommandHandler : IRequestHandler<FollowArtistCommand, bool>
{
    private readonly FollowerDbContext _context;

    public FollowArtistCommandHandler(FollowerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(FollowArtistCommand request, CancellationToken cancellationToken)
    {
        var existingFollow = await _context.Follows
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.ArtistId == request.ArtistId, cancellationToken);

        if (existingFollow != null)
        {
            return false; // Already following
        }

        var follow = new Follow
        {
            UserId = request.UserId,
            ArtistId = request.ArtistId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
