namespace Chirp.Core.Models;

public class Following
{
    public int FollowerId { get; set; }
    public Author Follower { get; set; } = null!;

    public int FollowedId { get; set; }
    public Author Followed { get; set; } = null!;
}
