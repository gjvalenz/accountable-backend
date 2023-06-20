using Accountable.DataStructures.ResponseRequestData;
using Microsoft.EntityFrameworkCore;

namespace Accountable.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions options) : base(options)
        {

        }

        public bool IsAuthenticated(UserIDs ids)
        {
            var user = Users.Find(ids.UserID);
            var userAccount = UserAccounts.Find(ids.UserAccountID);
            if (userAccount == null || user == null)
                return false;
            return true;
        }



        public DbSet<User> Users { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<WeightEntry> WeightEntries { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

    }
}
