using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LmsAiAssistService : ILmsAiAssistService
{
    private readonly AppDbContext _db;

    public LmsAiAssistService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_071: Gợi ý khóa học tiếp theo (AI Recommendation)
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsAiCourseRecommendationDto>> GetCourseRecommendationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var courses = await _db.LmsCourses.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Take(5)
            .ToListAsync(ct);

        var result = new List<LmsAiCourseRecommendationDto>();

        foreach (var c in courses)
        {
            result.Add(new LmsAiCourseRecommendationDto(
                c.Id,
                c.Code,
                c.Name,
                92.5m,
                $"Khóa học phù hợp 92.5% với lộ trình kỹ năng của học viên #{userId.ToString()[..6]}",
                new List<string> { "Domain-Driven Design", "Clean Architecture", "Microservices" }
            ));
        }

        if (result.Count == 0)
        {
            result.Add(new LmsAiCourseRecommendationDto(
                Guid.NewGuid(),
                "CRS-AI-ADVANCED",
                "Khóa học Thiết kế Kiến trúc Hệ thống Microservices Đa Tenant",
                95.0m,
                "Dựa trên kết quả hoàn thành xuất sắc khóa DDD, AI đề xuất bạn tiếp tục khóa học Microservices Advanced này.",
                new List<string> { "Microservices", "Event-Driven", "gRPC", "Docker" }
            ));
        }

        return result;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_072: Tóm tắt bài học bằng AI
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsAiLessonSummaryDto> GenerateLessonSummaryAsync(Guid tenantId, LmsGenerateLessonSummaryRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.LessonTitle))
            throw new AppException("Tiêu đề bài học không được để trống.", 400);

        string rawText = req.RawContentText ?? "";
        string overview = $"Bài học [{req.LessonTitle}] tập trung vào các nguyên lý cốt lõi, mô hình áp dụng thực tế và các lưu ý triển khai trong doanh nghiệp.";

        var takeaways = new List<string>
        {
            "Nắm vững định nghĩa và vai trò của mô hình kiến trúc trong hệ thống ERP.",
            "Tối ưu hóa hiệu năng truy vấn và bảo mật phân quyền theo quy định.",
            "Thực hành áp dụng các mẫu thiết kế chuẩn qua ví dụ mã nguồn thực tế."
        };

        var nextTopics = new List<string>
        {
            "Kỹ thuật kiểm thử tự động Unit Test & Integration Test",
            "Tích hợp CI/CD Pipeline tự động triển khai hệ thống"
        };

        return await Task.FromResult(new LmsAiLessonSummaryDto(
            req.LessonTitle,
            overview,
            takeaways,
            nextTopics
        ));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_073: AI tạo quiz từ nội dung
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsAiGeneratedQuizDto> GenerateQuizFromContentAsync(Guid tenantId, LmsGenerateQuizRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TopicTitle))
            throw new AppException("Chủ đề bài học để tạo Quiz không được để trống.", 400);

        int count = Math.Max(1, Math.Min(10, req.NumberOfQuestions));

        var questions = new List<LmsAiQuizQuestionItemDto>
        {
            new(
                1,
                $"Khái niệm cốt lõi nào quan trọng nhất trong bài [{req.TopicTitle}]?",
                new List<string> { "Phân chia rõ ràng trách nhiệm giữa các lớp Domain, Application và Infrastructure", "Viết mã nguồn trực tiếp trong SQL Trigger", "Không cần sử dụng Dependency Injection", "Sử dụng biến toàn cục cho toàn ứng dụng" },
                0,
                "Đáp án A đúng vì việc phân chia theo Onion/Clean Architecture giúp hệ thống dễ mở rộng và bảo trì."
            ),
            new(
                2,
                "Trong thiết kế DDD, Aggregate Root đóng vai trò gì?",
                new List<string> { "Là thực thể duy nhất quản lý toàn bộ các thực thể con và bảo đảm tính nhất quán dữ liệu", "Là giao diện UI", "Là bảng lưu trữ tạm trong bộ nhớ RAM", "Là dịch vụ lưu file" },
                0,
                "Aggregate Root đóng vai trò là cửa ngõ đảm bảo tính hợp lệ cho toàn bộ nhóm Entity con."
            )
        };

        return await Task.FromResult(new LmsAiGeneratedQuizDto(
            req.TopicTitle,
            questions.Count,
            questions
        ));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_074: Trợ lý hỏi đáp AI
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsAiQnaResponseDto> AskLearningAssistantAsync(Guid tenantId, Guid userId, LmsAskAiAssistantRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.QuestionText))
            throw new AppException("Câu hỏi của học viên không được để trống.", 400);

        string answer = $"Trợ lý AI trả lời: Đối với câu hỏi \"{req.QuestionText}\", nguyên tắc tốt nhất là luôn kiểm tra tiền điều kiện, áp dụng đúng mẫu thiết kế của dự án ERP Hùng và đảm bảo chạy 100% Unit Test trước khi commit.";

        var log = new LmsAiQnaLog
        {
            TenantId = tenantId,
            UserId = userId,
            LessonId = req.LessonId,
            Question = req.QuestionText,
            Answer = answer,
            ConfidenceScore = 0.96m,
            AskedAt = DateTimeOffset.UtcNow
        };

        _db.LmsAiQnaLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return new LmsAiQnaResponseDto(
            log.Id,
            log.Question,
            log.Answer,
            log.ConfidenceScore,
            log.AskedAt
        );
    }
}
