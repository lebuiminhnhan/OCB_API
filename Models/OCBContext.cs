using Microsoft.EntityFrameworkCore;

namespace OCB_API.Models
{
    public class OCBContext : DbContext
    {
        public OCBContext(DbContextOptions options)
            : base(options)
        {
        }
        public DbSet<User> UserTable { get; set; }
        public DbSet<Gift> GiftTable { get; set; }
        public DbSet<UserGift> UserGiftTable { get; set; }
        public DbSet<UserLogin> UserLoginTable { get; set; }
        public DbSet<InfoContact> InfoContactTable { get; set; }
        public DbSet<InfoRegister> InfoRegisterTable { get; set; }

    }
}
