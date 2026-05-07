using Microsoft.EntityFrameworkCore;
using QuizForge.Models;

namespace QuizForge.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<User>();
        user.ToTable("users");

        user.HasKey(x => x.Id);
        user.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        user.Property(x => x.GoogleId)
            .HasColumnName("google_id")
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("character varying(255)");

        user.Property(x => x.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("character varying(255)");
    
        user.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnType("character varying(150)");

        user.Property(x => x.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(255)
            .HasColumnType("character varying(255)");

        user.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        user.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("ux_users_email");
        
        user.HasIndex(x => x.GoogleId)
            .IsUnique()
            .HasDatabaseName("ux_users_google_id");

        var refreshToken = modelBuilder.Entity<RefreshToken>();
        refreshToken.ToTable("refresh_tokens");

        refreshToken.HasKey(x => x.Token);
        refreshToken.Property(x => x.Token)
            .HasColumnName("token")
            .ValueGeneratedNever();
        
        refreshToken.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        refreshToken.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        
        refreshToken.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
