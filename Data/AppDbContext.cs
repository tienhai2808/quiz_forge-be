using Microsoft.EntityFrameworkCore;
using QuizForge.Models;

namespace QuizForge.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ExtractionCache> ExtractionCaches => Set<ExtractionCache>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<Rating> Ratings => Set<Rating>();

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
        document.ToTable("documents");

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
            .HasConversion<byte>()
            .HasColumnType("smallint");

        document.Property(x => x.FilePath)
            .HasColumnName("file_path")
            .IsRequired()
            .HasMaxLength(1024)
            .HasColumnType("character varying(1024)");

        document.Property(x => x.FileHashSha256)
            .HasColumnName("file_hash_sha256")
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("character varying(64)");

        document.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<byte>()
            .HasColumnType("smallint");

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

        document.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_documents_user_id");

        document.HasIndex(x => x.FileHashSha256)
            .HasDatabaseName("ix_documents_file_hash_sha256");

        var extractionCache = modelBuilder.Entity<ExtractionCache>();
        extractionCache.ToTable("extraction_caches");

        extractionCache.HasKey(x => new { x.FileHashSha256, x.ParserMode, x.ExtractorVersion });

        extractionCache.Property(x => x.FileHashSha256)
            .HasColumnName("file_hash_sha256")
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("character varying(64)");

        extractionCache.Property(x => x.ParserMode)
            .HasColumnName("parser_mode")
            .IsRequired()
            .HasConversion<byte>()
            .HasColumnType("smallint");

        extractionCache.Property(x => x.ExtractorVersion)
            .HasColumnName("extractor_version")
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnType("character varying(32)");

        extractionCache.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .IsRequired();

        extractionCache.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        extractionCache.Property(x => x.LastHitAt)
            .HasColumnName("last_hit_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        extractionCache.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        extractionCache.HasIndex(x => x.DocumentId)
            .HasDatabaseName("ix_extraction_caches_document_id");

        var quiz = modelBuilder.Entity<Quiz>();
        quiz.ToTable("quizzes");

        quiz.HasKey(x => x.Id);
        quiz.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        quiz.Property(x => x.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("character varying(255)");

        quiz.Property(x => x.Instructions)
            .HasColumnName("instructions")
            .HasColumnType("text");

        quiz.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .IsRequired()
            .HasDefaultValue(false);

        quiz.Property(x => x.TotalQuestions)
            .HasColumnName("total_questions")
            .IsRequired()
            .HasDefaultValue(0);

        quiz.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        quiz.HasMany(x => x.Documents)
            .WithMany(x => x.Quizzes)
            .UsingEntity<Dictionary<string, object>>(
                "quiz_documents",
                right => right
                    .HasOne<Document>()
                    .WithMany()
                    .HasForeignKey("document_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Quiz>()
                    .WithMany()
                    .HasForeignKey("quiz_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("quiz_documents");
                    join.HasKey("quiz_id", "document_id");
                    join.IndexerProperty<long>("quiz_id").HasColumnName("quiz_id");
                    join.IndexerProperty<long>("document_id").HasColumnName("document_id");
                    join.HasIndex("document_id").HasDatabaseName("ix_quiz_documents_document_id");
                });

        var question = modelBuilder.Entity<Question>();
        question.ToTable("questions");

        question.HasKey(x => x.Id);
        question.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        question.Property(x => x.QuizId)
            .HasColumnName("quiz_id")
            .IsRequired();

        question.Property(x => x.OrderNo)
            .HasColumnName("order_no")
            .IsRequired();

        question.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired()
            .HasColumnType("text");

        question.Property(x => x.QuestionType)
            .HasColumnName("question_type")
            .IsRequired()
            .HasConversion<byte>()
            .HasColumnType("smallint");

        question.Property(x => x.CorrectAnswer)
            .HasColumnName("correct_answer")
            .IsRequired()
            .HasColumnType("text");

        question.HasOne(x => x.Quiz)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        question.HasIndex(x => new { x.QuizId, x.OrderNo })
            .IsUnique()
            .HasDatabaseName("ux_questions_quiz_id_order_no");

        var questionOption = modelBuilder.Entity<QuestionOption>();
        questionOption.ToTable("question_options");

        questionOption.HasKey(x => x.Id);
        questionOption.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        questionOption.Property(x => x.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        questionOption.Property(x => x.OptionKey)
            .HasColumnName("option_key")
            .IsRequired()
            .HasMaxLength(16)
            .HasColumnType("character varying(16)");

        questionOption.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired()
            .HasColumnType("text");

        questionOption.Property(x => x.OrderNo)
            .HasColumnName("order_no")
            .IsRequired();

        questionOption.HasOne(x => x.Question)
            .WithMany(x => x.QuestionOptions)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        questionOption.HasIndex(x => new { x.QuestionId, x.OrderNo })
            .IsUnique()
            .HasDatabaseName("ux_question_options_question_id_order_no");

        questionOption.HasIndex(x => new { x.QuestionId, x.OptionKey })
            .IsUnique()
            .HasDatabaseName("ux_question_options_question_id_option_key");

        var quizAttempt = modelBuilder.Entity<QuizAttempt>();
        quizAttempt.ToTable("quiz_attempts");

        quizAttempt.HasKey(x => x.Id);
        quizAttempt.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        quizAttempt.Property(x => x.QuizId)
            .HasColumnName("quiz_id")
            .IsRequired();

        quizAttempt.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        quizAttempt.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        quizAttempt.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamp with time zone");

        quizAttempt.Property(x => x.TotalCorrectAnswers)
            .HasColumnName("total_correct_answers")
            .IsRequired()
            .HasDefaultValue(0);

        quizAttempt.Property(x => x.TotalQuestions)
            .HasColumnName("total_questions")
            .IsRequired()
            .HasDefaultValue(0);

        quizAttempt.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<byte>()
            .HasColumnType("smallint");

        quizAttempt.HasOne(x => x.Quiz)
            .WithMany(x => x.QuizAttempts)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        quizAttempt.HasOne(x => x.User)
            .WithMany(x => x.QuizAttempts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        quizAttempt.HasIndex(x => x.QuizId)
            .HasDatabaseName("ix_quiz_attempts_quiz_id");

        quizAttempt.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_quiz_attempts_user_id");

        var attemptAnswer = modelBuilder.Entity<AttemptAnswer>();
        attemptAnswer.ToTable("attempt_answers");

        attemptAnswer.HasKey(x => new { x.AttemptId, x.QuestionId });

        attemptAnswer.Property(x => x.AttemptId)
            .HasColumnName("attempt_id")
            .IsRequired();

        attemptAnswer.Property(x => x.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        attemptAnswer.Property(x => x.QuestionOptionId)
            .HasColumnName("question_option_id");

        attemptAnswer.Property(x => x.TextAnswer)
            .HasColumnName("text_answer")
            .HasColumnType("text");

        attemptAnswer.Property(x => x.IsCorrect)
            .HasColumnName("is_correct")
            .IsRequired();

        attemptAnswer.HasOne(x => x.QuizAttempt)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        attemptAnswer.HasOne(x => x.Question)
            .WithMany(x => x.AttemptAnswers)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        attemptAnswer.HasOne(x => x.QuestionOption)
            .WithMany(x => x.AttemptAnswers)
            .HasForeignKey(x => x.QuestionOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        attemptAnswer.HasIndex(x => x.QuestionOptionId)
            .HasDatabaseName("ix_attempt_answers_question_option_id");

        var rating = modelBuilder.Entity<Rating>();
        rating.ToTable("ratings");

        rating.HasKey(x => new { x.QuizId, x.UserId });

        rating.Property(x => x.QuizId)
            .HasColumnName("quiz_id")
            .IsRequired();

        rating.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        rating.Property(x => x.Stars)
            .HasColumnName("stars")
            .IsRequired()
            .HasConversion<byte>()
            .HasColumnType("smallint");

        rating.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasMaxLength(1000)
            .HasColumnType("character varying(1000)");

        rating.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        rating.HasOne(x => x.Quiz)
            .WithMany(x => x.Ratings)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        rating.HasOne(x => x.User)
            .WithMany(x => x.Ratings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        rating.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_ratings_user_id");
    }
}
