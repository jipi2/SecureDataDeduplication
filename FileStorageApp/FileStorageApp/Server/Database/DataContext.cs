using FileStorageApp.Server.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FileStorageApp.Server.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Entity.User> Users { get; set; }
        public DbSet<Entity.Role> Roles { get; set; }
        public DbSet<Entity.FileMetadata> FilesMetadata { get; set; }
        public DbSet<Entity.Resp> Resps { get; set; }
        public DbSet<Entity.UserFile> UserFiles { get; set; }
        public DbSet<Entity.FileTransfer> FileTransfers { get; set; }
        public DbSet<Entity.FSRC> FSRCs { get; set; }
        public DbSet<Entity.Label> Labels { get; set; }
        public DbSet<Entity.FileFolder> FileFolders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(x => x.Roles)
                .WithMany(y => y.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));

            modelBuilder.Entity<Label>()
                .HasMany(e => e.UserFiles)
                .WithMany(e => e.Labels);

            modelBuilder.Entity<FileFolder>()
                .HasOne(ff => ff.User)
                .WithMany() // Assuming User does not have a navigation property pointing back to FileFolder
                .HasForeignKey(ff => ff.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Specify ON DELETE RESTRICT

            modelBuilder.Entity<FileFolder>()
               .HasOne(f => f.ParentFolder)
               .WithMany(f => f.ChildFileFolders)
               .HasForeignKey(f => f.ParentId)
               .OnDelete(DeleteBehavior.NoAction); // Specify ON DELETE NO ACTION
        }
    }
}
