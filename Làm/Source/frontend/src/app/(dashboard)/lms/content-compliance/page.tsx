'use client';

import React, { useState } from 'react';
import {
  evaluateVideoDownloadPermission,
  calculateSurveyScore,
  evaluateShiftTrainingGate,
} from '@/shared/api/lms-content-compliance-helpers';

export default function LmsContentCompliancePage() {
  const [activeTab, setActiveTab] = useState<'protection' | 'comprehension' | 'compliance' | 'gate'>('protection');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: VIDEO DOWNLOAD PROTECTION (UC_LMS_055)
  // ────────────────────────────────────────────────────────────────────────────
  const [videoConfigs, setVideoConfigs] = useState([
    { lessonId: 'les-1', title: 'Bài 01: Quy định An toàn Lao động Nhà máy (Video HD)', isDownloadBlocked: true, watermarkEnabled: true, watermarkText: 'PROTECTED - EMP164', signedUrlExpiryMinutes: 120 },
    { lessonId: 'les-2', title: 'Bài 02: Hướng dẫn Vận hành Máy phay CNC', isDownloadBlocked: true, watermarkEnabled: true, watermarkText: 'CONFIDENTIAL LMS', signedUrlExpiryMinutes: 60 },
  ]);

  const [testUserRole, setTestUserRole] = useState<'Learner' | 'Instructor' | 'Admin'>('Learner');
  const [testUserName, setTestUserName] = useState('Trần Văn B (EMP042)');
  const [playbackResult, setPlaybackResult] = useState<any>(null);

  const handleTestPlayback = (lessonId: string) => {
    const config = videoConfigs.find((v) => v.lessonId === lessonId);
    if (!config) return;

    const evalRes = evaluateVideoDownloadPermission(config.isDownloadBlocked, testUserRole, testUserName);
    const token = btoa(`${lessonId}:${Date.now()}`);

    setPlaybackResult({
      lessonTitle: config.title,
      streamUrl: `https://stream.erp-hung.vn/lms/video/${lessonId}?token=${token.substring(0, 16)}&nodownload=${evalRes.canDownload ? 0 : 1}`,
      canDownload: evalRes.canDownload,
      watermarkText: evalRes.watermarkText || config.watermarkText,
      reason: evalRes.reason,
    });

    if (evalRes.canDownload) {
      showToast(`Cho phép phát & tải video cho vai trò [${testUserRole}]`, 'success');
    } else {
      showToast(`Chế độ bảo vệ: Chặn tải video trực tiếp cho Học viên`, 'warning');
    }
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: COMPREHENSION SURVEY (UC_LMS_056)
  // ────────────────────────────────────────────────────────────────────────────
  const [comprehensionSurveys, setComprehensionSurveys] = useState([
    { id: 'sur-1', title: 'Khảo sát hiểu bài: Kiến thức Bounded Context DDD', course: 'Khóa LMS Architecture', targetPassingScore: 70, status: 'Active', totalResponses: 15 },
    { id: 'sur-2', title: 'Khảo sát hiểu bài: Nguyên tắc Vận hành Kho thông minh', course: 'Khóa Đào tạo NV Kho', targetPassingScore: 80, status: 'Active', totalResponses: 28 },
  ]);

  const [surveyAnswers, setSurveyAnswers] = useState({ q1: 10, q2: 8, q3: 9 });
  const [surveyResult, setSurveyResult] = useState<any>(null);

  const handleTestComprehensionSurvey = (e: React.FormEvent) => {
    e.preventDefault();
    const evalScore = calculateSurveyScore(surveyAnswers, 3, 70);

    setSurveyResult({
      score: evalScore.scorePercentage,
      isPass: evalScore.isPass,
      badge: evalScore.gradeBadge,
    });

    if (evalScore.isPass) {
      showToast(`Chúc mừng! Bạn đã hoàn thành khảo sát hiểu bài (${evalScore.scorePercentage}%)`, 'success');
    } else {
      showToast(`Chưa đạt tiêu chuẩn hiểu bài (${evalScore.scorePercentage}% < 70%). Cần ôn tập lại.`, 'error');
    }
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: COMPLIANCE SURVEY (UC_LMS_057)
  // ────────────────────────────────────────────────────────────────────────────
  const [complianceSurveys, setComplianceSurveys] = useState([
    { id: 'cs-1', code: 'CS-SAFETY-2026', title: 'Khảo sát Tuân thủ An toàn Lao động & PCCC Q3/2026', department: 'Xưởng Sản xuất 1', isMandatory: true, isSigned: true },
    { id: 'cs-2', code: 'CS-SEC-2026', title: 'Khảo sát Tuân thủ Bảo mật Dữ liệu & CNTT', department: 'Khối Văn phòng', isMandatory: true, isSigned: false },
  ]);

  const handleToggleSignCompliance = (surveyId: string) => {
    setComplianceSurveys((prev) =>
      prev.map((s) => (s.id === surveyId ? { ...s, isSigned: !s.isSigned } : s))
    );
    showToast('Đã cập nhật trạng thái xác nhận tuân thủ quy định!', 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: SHIFT TRAINING GATE (UC_LMS_059)
  // ────────────────────────────────────────────────────────────────────────────
  const [shiftGates, setShiftGates] = useState([
    { id: 'gate-1', employee: 'Nguyễn Văn C (EMP088)', shift: 'Ca Sáng (06:00 - 14:00)', shiftDate: '13/08/2026', course: 'An toàn Lao động Trước Ca', isCompleted: true, status: 'Passed' },
    { id: 'gate-2', employee: 'Phạm Thị D (EMP102)', shift: 'Ca Sáng (06:00 - 14:00)', shiftDate: '13/08/2026', course: 'An toàn Lao động Trước Ca', isCompleted: false, status: 'Blocked' },
  ]);

  const [testGate, setTestGate] = useState({
    employeeName: 'Lê Văn E (EMP105)',
    shiftName: 'Ca Chiều (14:00 - 22:00)',
    shiftStartTime: '2026-08-13T14:00:00Z',
    isMandatoryCompleted: false,
  });

  const [gateEvaluation, setGateEvaluation] = useState<any>(null);

  const handleEvaluateGate = (e: React.FormEvent) => {
    e.preventDefault();
    const result = evaluateShiftTrainingGate(testGate.isMandatoryCompleted, testGate.shiftStartTime);

    setGateEvaluation({
      canEnterWorkShift: result.canEnterWorkShift,
      gateStatus: result.gateStatus,
      message: result.message,
    });

    if (result.canEnterWorkShift) {
      showToast(result.message, 'success');
    } else {
      showToast(result.message, 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Toast */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white font-medium text-sm transition-all ${
            toast.type === 'success' ? 'bg-emerald-600' : toast.type === 'error' ? 'bg-rose-600' : 'bg-amber-600'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Header */}
      <div className="bg-brand p-5 rounded-xl text-brand-foreground shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-blue-500/30 text-blue-200 text-xs px-3 py-1 rounded-full font-semibold border border-blue-400/30">
              LMS - ĐÀO TẠO & QUẢN TRỊ BẢO MẬT
            </span>
            <h1 className="text-2xl font-bold mt-2">Video Protection, Surveys & Shift Training Gate</h1>
            <p className="text-blue-200 text-sm mt-1">
              Chống tải video, khảo sát hiểu bài, khảo sát tuân thủ & chốt cổng đào tạo trước ca
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-foreground/15 text-brand-foreground border border-brand-foreground/25">
              ● Tiến độ 100% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-2 mt-6 border-t border-brand-foreground/15 pt-4">
          <button
            onClick={() => setActiveTab('protection')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'protection' ? 'bg-surface text-blue-950 shadow-md' : 'text-blue-200 hover:bg-surface/10'
            }`}
          >
            🔒 UC_LMS_055: Chặn tải Video
          </button>
          <button
            onClick={() => setActiveTab('comprehension')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'comprehension' ? 'bg-surface text-blue-950 shadow-md' : 'text-blue-200 hover:bg-surface/10'
            }`}
          >
            📝 UC_LMS_056: Khảo sát hiểu bài
          </button>
          <button
            onClick={() => setActiveTab('compliance')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'compliance' ? 'bg-surface text-blue-950 shadow-md' : 'text-blue-200 hover:bg-surface/10'
            }`}
          >
            🛡️ UC_LMS_057: Khảo sát tuân thủ
          </button>
          <button
            onClick={() => setActiveTab('gate')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'gate' ? 'bg-surface text-blue-950 shadow-md' : 'text-blue-200 hover:bg-surface/10'
            }`}
          >
            🚪 UC_LMS_059: Hoàn thành trước ca
          </button>
        </div>
      </div>

      {/* TAB 1: VIDEO PROTECTION */}
      {activeTab === 'protection' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* List */}
            <div className="lg:col-span-2 rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4 flex items-center gap-2">
                <span>📹</span> Danh sách Video Bài học được Cấu hình Bảo vệ
              </h2>
              <div className="space-y-3">
                {videoConfigs.map((vid) => (
                  <div key={vid.lessonId} className="p-4 rounded-lg border border-border hover:border-blue-400 bg-slate-50 transition-all flex justify-between items-center">
                    <div>
                      <h3 className="font-semibold text-slate-900">{vid.title}</h3>
                      <div className="flex gap-2 mt-2 text-xs">
                        <span className="px-2 py-0.5 rounded bg-rose-100 text-rose-700 font-medium">
                          {vid.isDownloadBlocked ? '🔒 Chặn tải trực tiếp' : '🔓 Cho phép tải'}
                        </span>
                        <span className="px-2 py-0.5 rounded bg-blue-100 text-blue-700 font-medium">
                          💧 Watermark: {vid.watermarkText}
                        </span>
                        <span className="px-2 py-0.5 rounded bg-amber-100 text-amber-700 font-medium">
                          ⏱️ Hạn token: {vid.signedUrlExpiryMinutes} phút
                        </span>
                      </div>
                    </div>
                    <button
                      onClick={() => handleTestPlayback(vid.lessonId)}
                      className="px-3 py-1.5 text-xs font-semibold bg-blue-600 text-white rounded-md hover:bg-blue-700 shadow-sm"
                    >
                      Thử nghiệm Phát Video
                    </button>
                  </div>
                ))}
              </div>
            </div>

            {/* Test Console */}
            <div className="rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">⚙️ Giả lập Trình phát Video DRM</h2>
              <div className="space-y-4 text-sm">
                <div>
                  <label className="block text-foreground font-medium mb-1">Vai trò người xem:</label>
                  <select
                    value={testUserRole}
                    onChange={(e: any) => setTestUserRole(e.target.value)}
                    className="w-full border border-border rounded-lg p-2 bg-surface"
                  >
                    <option value="Learner">Học viên (Learner)</option>
                    <option value="Instructor">Giảng viên (Instructor)</option>
                    <option value="Admin">Quản trị viên (Admin)</option>
                  </select>
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Tên người dùng:</label>
                  <input
                    type="text"
                    value={testUserName}
                    onChange={(e) => setTestUserName(e.target.value)}
                    className="w-full border border-border rounded-lg p-2"
                  />
                </div>

                {playbackResult && (
                  <div className="mt-4 p-4 rounded-lg bg-slate-900 text-slate-100 space-y-2 text-xs font-mono">
                    <p className="text-blue-400 font-bold">🎬 {playbackResult.lessonTitle}</p>
                    <p className="truncate text-emerald-400">Stream URL: {playbackResult.streamUrl}</p>
                    <p className={playbackResult.canDownload ? 'text-emerald-300' : 'text-rose-400 font-bold'}>
                      Tải về: {playbackResult.canDownload ? '✅ ĐƯỢC PHÉP' : '🚫 BỊ CHẶN (DIS-ALLOW)'}
                    </p>
                    {!playbackResult.canDownload && (
                      <div className="p-2 bg-black/50 border border-slate-700 rounded text-amber-300">
                        💧 Watermark overlay active: <br />
                        <span className="font-bold text-amber-200">"{playbackResult.watermarkText}"</span>
                      </div>
                    )}
                    <p className="text-slate-400 italic mt-2">{playbackResult.reason}</p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: COMPREHENSION SURVEY */}
      {activeTab === 'comprehension' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">📋 Khảo sát Hiểu bài Sau Khóa học (UC_LMS_056)</h2>
              <div className="divide-y divide-slate-100">
                {comprehensionSurveys.map((sur) => (
                  <div key={sur.id} className="py-3 flex justify-between items-center">
                    <div>
                      <h3 className="font-semibold text-slate-900">{sur.title}</h3>
                      <p className="text-xs text-muted-foreground mt-0.5">Khóa: {sur.course} • Điểm đạt: {sur.targetPassingScore}%</p>
                    </div>
                    <div className="text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800">
                        {sur.totalResponses} Lượt trả lời
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">✍️ Thử nghiệm Nộp bài Khảo sát</h2>
              <form onSubmit={handleTestComprehensionSurvey} className="space-y-4 text-sm">
                <div>
                  <label className="block text-foreground font-medium mb-1">Câu 1: Điểm hiểu khái niệm (0-10)</label>
                  <input
                    type="number"
                    min="0"
                    max="10"
                    value={surveyAnswers.q1}
                    onChange={(e) => setSurveyAnswers({ ...surveyAnswers, q1: Number(e.target.value) })}
                    className="w-full border border-border rounded-lg p-2"
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Câu 2: Điểm ứng dụng thực tế (0-10)</label>
                  <input
                    type="number"
                    min="0"
                    max="10"
                    value={surveyAnswers.q2}
                    onChange={(e) => setSurveyAnswers({ ...surveyAnswers, q2: Number(e.target.value) })}
                    className="w-full border border-border rounded-lg p-2"
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Câu 3: Điểm tự tin vận hành (0-10)</label>
                  <input
                    type="number"
                    min="0"
                    max="10"
                    value={surveyAnswers.q3}
                    onChange={(e) => setSurveyAnswers({ ...surveyAnswers, q3: Number(e.target.value) })}
                    className="w-full border border-border rounded-lg p-2"
                  />
                </div>

                <button type="submit" className="w-full py-2 bg-brand text-white rounded-lg font-semibold hover:bg-brand-hover">
                  Tính điểm & Kiểm tra
                </button>

                {surveyResult && (
                  <div className={`p-4 rounded-lg text-center ${surveyResult.isPass ? 'bg-emerald-50 text-emerald-900 border border-emerald-200' : 'bg-rose-50 text-rose-900 border border-rose-200'}`}>
                    <div className="text-3xl font-extrabold">{surveyResult.score}%</div>
                    <div className="font-semibold text-sm mt-1">{surveyResult.badge}</div>
                  </div>
                )}
              </form>
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: COMPLIANCE SURVEY */}
      {activeTab === 'compliance' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5">
          <h2 className="text-lg font-bold text-foreground mb-4">🛡️ Khảo sát Tuân thủ Quy định & An toàn (UC_LMS_057)</h2>
          <div className="space-y-4">
            {complianceSurveys.map((cs) => (
              <div key={cs.id} className="p-4 rounded-xl border border-border bg-slate-50 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-slate-200 text-foreground">{cs.code}</span>
                    <h3 className="font-bold text-slate-900">{cs.title}</h3>
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">Phạm vi: {cs.department} • Bắt buộc: {cs.isMandatory ? 'Có' : 'Không'}</p>
                </div>
                <button
                  onClick={() => handleToggleSignCompliance(cs.id)}
                  className={`px-4 py-2 text-xs font-bold rounded-lg transition-all ${
                    cs.isSigned ? 'bg-emerald-600 text-white hover:bg-emerald-700' : 'bg-amber-500 text-white hover:bg-amber-600'
                  }`}
                >
                  {cs.isSigned ? '✓ Đã ký Xác nhận' : '✍️ Chờ ký Xác nhận'}
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 4: SHIFT TRAINING GATE */}
      {activeTab === 'gate' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">🚪 Trạng thái Cổng Đào tạo Trước Ca (UC_LMS_059)</h2>
              <div className="space-y-3">
                {shiftGates.map((g) => (
                  <div key={g.id} className="p-4 rounded-lg border border-border bg-slate-50 flex justify-between items-center">
                    <div>
                      <h3 className="font-bold text-slate-900">{g.employee}</h3>
                      <p className="text-xs text-muted-foreground mt-1">
                        Ca: {g.shift} ({g.shiftDate}) • Khóa: {g.course}
                      </p>
                    </div>
                    <div>
                      <span
                        className={`px-3 py-1 text-xs font-bold rounded-full ${
                          g.status === 'Passed' ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'
                        }`}
                      >
                        {g.status === 'Passed' ? '🔓 ĐƯỢC VÀO CA' : '⛔ CHẶN VÀO CA'}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">⚡ Đánh giá Cổng Vào Ca</h2>
              <form onSubmit={handleEvaluateGate} className="space-y-4 text-sm">
                <div>
                  <label className="block text-foreground font-medium mb-1">Nhân viên:</label>
                  <input type="text" value={testGate.employeeName} readOnly className="w-full border border-border bg-slate-100 rounded-lg p-2" />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Ca làm việc:</label>
                  <input type="text" value={testGate.shiftName} readOnly className="w-full border border-border bg-slate-100 rounded-lg p-2" />
                </div>
                <div className="flex items-center gap-2 pt-2">
                  <input
                    type="checkbox"
                    id="chkCompleted"
                    checked={testGate.isMandatoryCompleted}
                    onChange={(e) => setTestGate({ ...testGate, isMandatoryCompleted: e.target.checked })}
                    className="w-4 h-4 text-blue-600 rounded"
                  />
                  <label htmlFor="chkCompleted" className="text-foreground font-medium">
                    Đã hoàn thành khóa học bắt buộc trước ca
                  </label>
                </div>

                <button type="submit" className="w-full py-2 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700">
                  Kiểm tra Điều kiện Vào ca
                </button>

                {gateEvaluation && (
                  <div className={`p-4 rounded-lg font-semibold text-xs ${gateEvaluation.canEnterWorkShift ? 'bg-emerald-100 text-emerald-900' : 'bg-rose-100 text-rose-900'}`}>
                    {gateEvaluation.message}
                  </div>
                )}
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
