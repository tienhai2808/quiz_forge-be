using Microsoft.EntityFrameworkCore;
using QuizForge.Models;

namespace QuizForge.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Extraction> Extractions => Set<Extraction>();

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

        refreshToken.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_refresh_tokens_user_id");

        var document = modelBuilder.Entity<Document>();
        document.ToTable("documents", table =>
        {
            table.HasCheckConstraint(
                "ck_documents_source_type",
                "source_type IN ('docx', 'pdf_text', 'pdf_ocr', 'image')"
            );
            table.HasCheckConstraint(
                "ck_documents_status",
                "status IN ('uploaded', 'processing', 'parsed', 'failed')"
            );
        });

        document.HasKey(x => x.Id);
        document.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        document.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        document.Property(x => x.SourceType)
            .HasColumnName("source_type")
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnType("character varying(32)");

        document.Property(x => x.FileKey)
            .HasColumnName("file_key")
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("character varying(255)");

        document.Property(x => x.FileHashSha256)
            .HasColumnName("file_hash_sha256")
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("character varying(64)");

        document.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnType("character varying(32)");

        document.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        document.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .IsRequired()
            .HasDefaultValue(false);

        document.HasOne(x => x.User)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        document.HasOne(x => x.Extraction)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.FileHashSha256)
            .HasPrincipalKey(x => x.HashSha256)
            .OnDelete(DeleteBehavior.Cascade);

        document.HasIndex(x => x.FileHashSha256)
            .HasDatabaseName("ix_documents_file_hash_sha256");

        var extraction = modelBuilder.Entity<Extraction>();
        extraction.ToTable("extractions");

        extraction.HasKey(x => x.HashSha256);

        extraction.Property(x => x.HashSha256)
            .HasColumnName("hash_sha256")
            .ValueGeneratedNever()
            .HasMaxLength(64)
            .HasColumnType("character varying(64)");

        extraction.Property(x => x.Version)
            .HasColumnName("version")
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnType("character varying(32)");

        extraction.Property(x => x.RawJson)
            .HasColumnName("raw_json")
            .IsRequired()
            .HasColumnType("text");
    }
}
