using FluentValidation;

namespace FollowerService.Features.Follows.Commands.FollowArtist;

public class FollowArtistCommandValidator : AbstractValidator<FollowArtistCommand>
{
    public FollowArtistCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.ArtistId).NotEmpty().WithMessage("ArtistId is required.");
    }
}
