using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/lms/ai-assist")]
public sealed class LmsAiAssistController : ControllerBase
{
    private readonly ILmsAiAssistService _svc;

    public LmsAiAssistController(ILmsAiAssistService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_071: Gợi ý khóa học tiếp theo (AI Recommendation)
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/ai/recommendations")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsAiCourseRecommendationDto>>>> GetCourseRecommendations(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsAiCourseRecommendationDto>>.Ok(await _svc.GetCourseRecommendationsAsync(TenantId, UserId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_072: Tóm tắt bài học bằng AI
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/ai/summarize-lesson")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsAiLessonSummaryDto>>> GenerateLessonSummary([FromBody] LmsGenerateLessonSummaryRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsAiLessonSummaryDto>.Ok(await _svc.GenerateLessonSummaryAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_073: AI tạo quiz từ nội dung
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/ai/generate-quiz")]
    [AuthorizePermission("lms.exam.write")]
    public async Task<ActionResult<ApiResponse<LmsAiGeneratedQuizDto>>> GenerateQuizFromContent([FromBody] LmsGenerateQuizRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsAiGeneratedQuizDto>.Ok(await _svc.GenerateQuizFromContentAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_074: Trợ lý hỏi đáp AI
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/ai/ask-assistant")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsAiQnaResponseDto>>> AskLearningAssistant([FromBody] LmsAskAiAssistantRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsAiQnaResponseDto>.Ok(await _svc.AskLearningAssistantAsync(TenantId, UserId, req, ct)));
}
