
namespace API.Entities
{
    public class UserLike  // this is going to act like a joined table
    {
        public AppUser SourceUser { get; set; }

        public int SourceUserId { get; set; }

        public AppUser TargetUser { get; set; }

        public int TargetUserId { get; set; }
    }
}