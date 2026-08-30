using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Quizora.Domain.Entities;

namespace Quizora.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<TestInvitation> TestInvitations => Set<TestInvitation>();
    public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<PracticeCategory> PracticeCategories => Set<PracticeCategory>();
    public DbSet<PracticeQuestion> PracticeQuestions => Set<PracticeQuestion>();
    public DbSet<PracticeOption> PracticeOptions => Set<PracticeOption>();
    public DbSet<PracticeAttempt> PracticeAttempts => Set<PracticeAttempt>();
    public DbSet<InterviewTopic> InterviewTopics => Set<InterviewTopic>();
    public DbSet<InterviewQA> InterviewQAs => Set<InterviewQA>();
    public DbSet<MockTest> MockTests => Set<MockTest>();
    public DbSet<MockTestQuestion> MockTestQuestions => Set<MockTestQuestion>();
    public DbSet<MockAttempt> MockAttempts => Set<MockAttempt>();
    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();
    public DbSet<QuestionBankOption> QuestionBankOptions => Set<QuestionBankOption>();
    public DbSet<InvitationQuestion> InvitationQuestions => Set<InvitationQuestion>();
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<CodingProblem> CodingProblems => Set<CodingProblem>();
    public DbSet<CodingTestCase> CodingTestCases => Set<CodingTestCase>();
    public DbSet<CodingSubmission> CodingSubmissions => Set<CodingSubmission>();
    public DbSet<TestCodingProblem> TestCodingProblems => Set<TestCodingProblem>();
    public DbSet<ContestRegistration> ContestRegistrations => Set<ContestRegistration>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(150);
            entity.Property(u => u.FullName).HasMaxLength(100);
        });

        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Company)
                  .HasForeignKey<Company>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Candidate
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Candidate)
                  .HasForeignKey<Candidate>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Test
        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasOne(t => t.Company)
                  .WithMany(c => c.Tests)
                  .HasForeignKey(t => t.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Question (company-created test questions)
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasOne(q => q.Test)
                  .WithMany(t => t.Questions)
                  .HasForeignKey(q => q.TestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Option
        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasOne(o => o.Question)
                  .WithMany(q => q.Options)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TestInvitation
        modelBuilder.Entity<TestInvitation>(entity =>
        {
            entity.HasOne(i => i.Test)
                  .WithMany(t => t.Invitations)
                  .HasForeignKey(i => i.TestId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(i => i.Candidate)
                  .WithMany(c => c.Invitations)
                  .HasForeignKey(i => i.CandidateId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(i => new { i.TestId, i.CandidateId }).IsUnique();
        });

        // TestAttempt
        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasOne(a => a.Invitation)
                  .WithOne(i => i.Attempt)
                  .HasForeignKey<TestAttempt>(a => a.InvitationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Answer — QuestionId / SelectedOptionId are QuestionBank IDs, NOT FK to Questions/Options
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasOne(a => a.Attempt)
                  .WithMany(t => t.Answers)
                  .HasForeignKey(a => a.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(a => a.QuestionId).IsRequired();
            entity.Property(a => a.SelectedOptionId).IsRequired(false);
            entity.Property(a => a.AnswerText).HasMaxLength(8000);
        });

        // Remove convention FKs Answer → Question / Option (if EF added them)
        var answerEntity = modelBuilder.Model.FindEntityType(typeof(Answer));
        if (answerEntity != null)
        {
            foreach (var fk in answerEntity.GetForeignKeys().ToList())
            {
                var principal = fk.PrincipalEntityType.ClrType;
                if (principal == typeof(Question) || principal == typeof(Option))
                    ((IMutableEntityType)answerEntity).RemoveForeignKey(fk);
            }
        }

        // ==================== Practice ====================
        modelBuilder.Entity<PracticeCategory>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });
        modelBuilder.Entity<PracticeQuestion>(entity =>
        {
            entity.HasOne(q => q.Category)
                  .WithMany(c => c.Questions)
                  .HasForeignKey(q => q.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(q => q.Text).IsRequired();
        });
        modelBuilder.Entity<PracticeOption>(entity =>
        {
            entity.HasOne(o => o.Question)
                  .WithMany(q => q.Options)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(o => o.Text).IsRequired();
        });
        modelBuilder.Entity<PracticeAttempt>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Category)
                  .WithMany()
                  .HasForeignKey(a => a.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==================== Interview ====================
        modelBuilder.Entity<InterviewTopic>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(150).IsRequired();
        });
        modelBuilder.Entity<InterviewQA>(entity =>
        {
            entity.HasOne(qa => qa.Topic)
                  .WithMany(t => t.QAs)
                  .HasForeignKey(qa => qa.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(qa => qa.Question).IsRequired();
            entity.Property(qa => qa.Answer).IsRequired();
        });

        // ==================== Mock Test ====================
        modelBuilder.Entity<MockTest>(entity =>
        {
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
        });
        modelBuilder.Entity<MockTestQuestion>(entity =>
        {
            entity.HasOne(mq => mq.MockTest)
                  .WithMany(m => m.Questions)
                  .HasForeignKey(mq => mq.MockTestId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(mq => mq.PracticeQuestion)
                  .WithMany()
                  .HasForeignKey(mq => mq.PracticeQuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MockAttempt>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.MockTest)
                  .WithMany(m => m.Attempts)
                  .HasForeignKey(a => a.MockTestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== Question Bank ====================
        modelBuilder.Entity<QuestionBank>(entity =>
        {
            entity.Property(q => q.Text).IsRequired();
            entity.Property(q => q.Category).HasMaxLength(100);
            entity.Property(q => q.Difficulty).HasMaxLength(50);
        });
        modelBuilder.Entity<QuestionBankOption>(entity =>
        {
            entity.HasOne(o => o.QuestionBank)
                  .WithMany(q => q.Options)
                  .HasForeignKey(o => o.QuestionBankId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<InvitationQuestion>(entity =>
        {
            entity.HasOne(iq => iq.Invitation)
                  .WithMany()
                  .HasForeignKey(iq => iq.InvitationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(iq => iq.QuestionBank)
                  .WithMany()
                  .HasForeignKey(iq => iq.QuestionBankId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==================== Notification ====================
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Message).HasMaxLength(2000);
            e.Property(x => x.Type).HasMaxLength(50);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== Coding ====================
        modelBuilder.Entity<CodingProblem>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Difficulty).HasMaxLength(20);
            e.HasMany(x => x.TestCases)
                .WithOne(x => x.Problem)
                .HasForeignKey(x => x.CodingProblemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CodingTestCase>(e =>
        {
            e.Property(x => x.Input).HasMaxLength(8000);
            e.Property(x => x.ExpectedOutput).HasMaxLength(8000);
        });
        modelBuilder.Entity<TestCodingProblem>(e =>
        {
            e.HasIndex(x => new { x.TestId, x.CodingProblemId }).IsUnique();
            e.HasOne(x => x.Test)
             .WithMany(t => t.CodingProblems)
             .HasForeignKey(x => x.TestId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Problem)
             .WithMany()
             .HasForeignKey(x => x.CodingProblemId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CodingSubmission>(e =>
        {
            e.Property(x => x.Language).HasMaxLength(10);
            e.Property(x => x.Verdict).HasMaxLength(40);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Problem).WithMany().HasForeignKey(x => x.CodingProblemId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.UserId, x.CodingProblemId });
        });
        modelBuilder.Entity<ContestRegistration>(e =>
        {
            e.HasIndex(x => new { x.ContestId, x.UserId }).IsUnique();
            e.HasOne(x => x.Contest).WithMany().HasForeignKey(x => x.ContestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}