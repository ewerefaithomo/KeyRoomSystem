using Microsoft.EntityFrameworkCore;
using KeyRoomSystem.Models;

namespace KeyRoomSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<User> Users { get; set; }

        public DbSet<KeyItem> KeyItems { get; set; }

        public DbSet<Staff> Staffs { get; set; }

        public DbSet<KeyTransaction> KeyTransactions { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =====================================================
            // USERS
            // =====================================================

            modelBuilder.Entity<User>()
                .ToTable("Users");


            // =====================================================
            // KEY ITEMS
            // =====================================================

            modelBuilder.Entity<KeyItem>()
                .ToTable("KeyItems");

            modelBuilder.Entity<KeyItem>()
                .HasKey(k => k.Id);


            // =====================================================
            // STAFF
            // =====================================================

            modelBuilder.Entity<Staff>()
                .ToTable("Staffs");

            modelBuilder.Entity<Staff>()
                .HasKey(s => s.StaffNumber);

            modelBuilder.Entity<Staff>()
                .Property(s => s.StaffNumber)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Staff>()
                .Property(s => s.FullName)
                .HasMaxLength(200)
                .IsRequired(false);

            modelBuilder.Entity<Staff>()
                .Property(s => s.Department)
                .HasMaxLength(150)
                .IsRequired(false);

            modelBuilder.Entity<Staff>()
                .Property(s => s.PhoneNumber)
                .HasMaxLength(30)
                .IsRequired(false);


            // =====================================================
            // KEY TRANSACTIONS
            // =====================================================

            modelBuilder.Entity<KeyTransaction>()
                .ToTable("KeyTransactions");

            modelBuilder.Entity<KeyTransaction>()
                .HasKey(t => t.Id);


            // KeyTransactions -> KeyItems

            modelBuilder.Entity<KeyTransaction>()
                .HasOne(t => t.KeyItem)
                .WithMany()
                .HasForeignKey(t => t.KeyId)
                .HasPrincipalKey(k => k.Id)
                .OnDelete(DeleteBehavior.Restrict);


            // KeyTransactions -> Staffs

            modelBuilder.Entity<KeyTransaction>()
                .HasOne(t => t.Staff)
                .WithMany()
                .HasForeignKey(t => t.StaffNumber)
                .HasPrincipalKey(s => s.StaffNumber)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}