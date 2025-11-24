using Cartesian.Services.Communities;
using Cartesian.Services.Content;
using Cartesian.Services.Events;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class CartesianUser : IdentityUser
{
    public Guid? AvatarId { get; set; }
    public Media? Avatar { get; set; }

    public List<Media> Media { get; set; } = [];
    public List<Membership> Memberships { get; set; } = [];
    public List<Event> SubscribedEvents { get; set; } = [];
    public List<Event> ParticipatedEvents { get; set; } = [];
    public List<Event> FavoritedEvents { get; set; } = [];

    public CartesianUserDto ToDto() => new(Id, UserName!, Avatar?.ToDto());
    public MyUserDto ToMyUserDto() => new(Id, UserName!, Email!, EmailConfirmed, Avatar?.ToDto());
}
