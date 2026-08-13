using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LmsAiAssistPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsAiAssistService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();

    public LmsAiAssistPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("lms-ai-assist-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T167", Name = "Tenant 167 AI" });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _courseId,
            TenantId = _tenant,
            Code = "CRS-AI-01",
            Name = "Khóa Đào tạo Ứng dụng AI trong Đột phá Năng suất",
            Price = 2000000m
        });

        _db.SaveChanges();

        _svc = new LmsAiAssistService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_071: Gợi ý khóa học tiếp theo (AI Recommendation)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC071_GetCourseRecommendations_ReturnsAiRecommendations()
    {
        var list = await _svc.GetCourseRecommendationsAsync(_tenant, _userId);
        Assert.NotEmpty(list);
        Assert.Contains(list, r => r.MatchPercentage >= 80m);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_072: Tóm tắt bài học bằng AI
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC072_GenerateLessonSummary_ReturnsFormattedSummary()
    {
        var req = new LmsGenerateLessonSummaryRequest(
            _courseId,
            "Bài 01: Giới thiệu Nguyên lý Clean Architecture & DDD",
            "Mã nguồn được cấu hình theo mô hình 4 tầng độc lập..."
        );

        var res = await _svc.GenerateLessonSummaryAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Bài 01: Giới thiệu Nguyên lý Clean Architecture & DDD", res.LessonTitle);
        Assert.NotEmpty(res.KeyTakeaways);
        Assert.NotEmpty(res.SuggestedNextTopics);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_073: AI tạo quiz từ nội dung
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC073_GenerateQuizFromContent_GeneratesValidQuizQuestions()
    {
        var req = new LmsGenerateQuizRequest(
            _courseId,
            "Nguyên lý Aggregate Root trong DDD",
            "Nội dung bài giảng hướng dẫn cách đảm bảo tính toàn vẹn dữ liệu...",
            2
        );

        var quiz = await _svc.GenerateQuizFromContentAsync(_tenant, req);

        Assert.NotNull(quiz);
        Assert.Equal("Nguyên lý Aggregate Root trong DDD", quiz.TopicTitle);
        Assert.NotEmpty(quiz.Questions);
        Assert.Equal(4, quiz.Questions[0].Options.Count);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_074: Trợ lý hỏi đáp AI
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC074_AskLearningAssistant_ReturnsAiAnswerAndLogs()
    {
        var req = new LmsAskAiAssistantRequest(
            Guid.NewGuid(),
            "Làm thế nào để áp dụng đúng quy chuẩn Unit Test trong ERP Hùng?"
        );

        var res = await _svc.AskLearningAssistantAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal(req.QuestionText, res.QuestionText);
        Assert.Contains("Trợ lý AI trả lời", res.AnswerText);
        Assert.True(res.ConfidenceScore >= 0.9m);

        var loggedInDb = await _db.LmsAiQnaLogs.AnyAsync(l => l.TenantId == _tenant && l.Id == res.LogId);
        Assert.True(loggedInDb);
    }
}
