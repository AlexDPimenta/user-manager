using Microsoft.EntityFrameworkCore;
using UserManager.WebApi.Models;

namespace UserManager.WebApi.Data;

public class UserManagerDbContext : DbContext
{
    public UserManagerDbContext(DbContextOptions<UserManagerDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}

