using Erp.Domain.Entities.Lms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Lms;

public sealed class LmsTrainingClassConfig : IEntityTypeConfiguration<LmsTrainingClass>
{
    public void Configure(EntityTypeBuilder<LmsTrainingClass> b)
    {
        b.ToTable("training_class", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CourseTitle).HasMaxLength(200).IsRequired();
        b.Property(x => x.InstructorName).HasMaxLength(200);
        b.HasIndex(x => new { x.TenantId, x.InstructorId });
        b.Property(x => x.Location).HasMaxLength(300);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.SummaryNote).HasMaxLength(2000);
    }
}

public sealed class LmsClassSessionConfig : IEntityTypeConfiguration<LmsClassSession>
{
    public void Configure(EntityTypeBuilder<LmsClassSession> b)
    {
        b.ToTable("class_session", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ClassId, x.SortOrder });
        b.Property(x => x.Topic).HasMaxLength(300).IsRequired();
    }
}

public sealed class LmsClassEnrollmentConfig : IEntityTypeConfiguration<LmsClassEnrollment>
{
    public void Configure(EntityTypeBuilder<LmsClassEnrollment> b)
    {
        b.ToTable("class_enrollment", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ClassId, x.EmployeeId }).IsUnique();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LmsSessionAttendanceConfig : IEntityTypeConfiguration<LmsSessionAttendance>
{
    public void Configure(EntityTypeBuilder<LmsSessionAttendance> b)
    {
        b.ToTable("session_attendance", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.SessionId, x.EnrollmentId }).IsUnique();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class LmsMentorAssignmentConfig : IEntityTypeConfiguration<LmsMentorAssignment>
{
    public void Configure(EntityTypeBuilder<LmsMentorAssignment> b)
    {
        b.ToTable("mentor_assignment", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.MenteeEmployeeId, x.MentorEmployeeId });
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class LmsProgramConfig : IEntityTypeConfiguration<LmsProgram>
{
    public void Configure(EntityTypeBuilder<LmsProgram> b)
    {
        b.ToTable("program", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LmsCourseConfig : IEntityTypeConfiguration<LmsCourse>
{
    public void Configure(EntityTypeBuilder<LmsCourse> b)
    {
        b.ToTable("course", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(2000);
        b.Property(x => x.DeliveryMode).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.CoverUrl).HasMaxLength(500);
    }
}

public sealed class LmsChapterConfig : IEntityTypeConfiguration<LmsChapter>
{
    public void Configure(EntityTypeBuilder<LmsChapter> b)
    {
        b.ToTable("chapter", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CourseId, x.SortOrder });
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
    }
}

public sealed class LmsLessonConfig : IEntityTypeConfiguration<LmsLesson>
{
    public void Configure(EntityTypeBuilder<LmsLesson> b)
    {
        b.ToTable("lesson", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ChapterId, x.SortOrder });
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.LessonType).HasMaxLength(30).IsRequired();
        b.Property(x => x.ContentUrl).HasMaxLength(500);
        b.Property(x => x.Body).HasMaxLength(8000);
    }
}

public sealed class LmsOnlineEnrollmentConfig : IEntityTypeConfiguration<LmsOnlineEnrollment>
{
    public void Configure(EntityTypeBuilder<LmsOnlineEnrollment> b)
    {
        b.ToTable("online_enrollment", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CourseId, x.UserId }).IsUnique();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.PaidAmount).HasPrecision(18, 2);
    }
}

public sealed class LmsLessonProgressConfig : IEntityTypeConfiguration<LmsLessonProgress>
{
    public void Configure(EntityTypeBuilder<LmsLessonProgress> b)
    {
        b.ToTable("lesson_progress", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EnrollmentId, x.LessonId }).IsUnique();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LmsQuestionConfig : IEntityTypeConfiguration<LmsQuestion>
{
    public void Configure(EntityTypeBuilder<LmsQuestion> b)
    {
        b.ToTable("question", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Stem).HasMaxLength(2000).IsRequired();
        b.Property(x => x.QuestionType).HasMaxLength(30).IsRequired();
        b.Property(x => x.OptionsJson).HasMaxLength(8000).IsRequired();
        b.Property(x => x.CorrectKeysJson).HasMaxLength(500).IsRequired();
        b.Property(x => x.Points).HasPrecision(18, 2);
        b.Property(x => x.Tag).HasMaxLength(100);
    }
}

public sealed class LmsExamConfig : IEntityTypeConfiguration<LmsExam>
{
    public void Configure(EntityTypeBuilder<LmsExam> b)
    {
        b.ToTable("exam", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ExamType).HasMaxLength(30).IsRequired();
        b.Property(x => x.PassScore).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LmsExamQuestionConfig : IEntityTypeConfiguration<LmsExamQuestion>
{
    public void Configure(EntityTypeBuilder<LmsExamQuestion> b)
    {
        b.ToTable("exam_question", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ExamId, x.QuestionId }).IsUnique();
        b.Property(x => x.PointsOverride).HasPrecision(18, 2);
    }
}

public sealed class LmsExamAttemptConfig : IEntityTypeConfiguration<LmsExamAttempt>
{
    public void Configure(EntityTypeBuilder<LmsExamAttempt> b)
    {
        b.ToTable("exam_attempt", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ExamId, x.UserId, x.AttemptNo }).IsUnique();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.AnswersJson).HasMaxLength(8000).IsRequired();
        b.Property(x => x.Score).HasPrecision(18, 2);
        b.Property(x => x.MaxScore).HasPrecision(18, 2);
    }
}

public sealed class LmsCertificateConfig : IEntityTypeConfiguration<LmsCertificate>
{
    public void Configure(EntityTypeBuilder<LmsCertificate> b)
    {
        b.ToTable("certificate", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.CourseId, x.UserId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ScoreAtIssue).HasPrecision(18, 2);
    }
}

public sealed class LmsInstructorConfig : IEntityTypeConfiguration<LmsInstructor>
{
    public void Configure(EntityTypeBuilder<LmsInstructor> b)
    {
        b.ToTable("instructor", "lms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.EmployeeId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Title).HasMaxLength(120);
        b.Property(x => x.Specialty).HasMaxLength(200);
        b.Property(x => x.Bio).HasMaxLength(2000);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}
