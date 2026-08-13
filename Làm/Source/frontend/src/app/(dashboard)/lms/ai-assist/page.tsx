'use client';

import React, { useState } from 'react';
import {
  calculateAiMatchScore,
  formatAiSummaryBullets,
  validateAiQuizStructure,
} from '@/shared/api/lms-ai-assist-helpers';

export default function LmsAiAssistPage() {
  const [activeTab, setActiveTab] = useState<'recommend' | 'summary' | 'quiz' | 'qna'>('recommend');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: AI COURSE RECOMMENDATIONS (UC_LMS_071)
  // ────────────────────────────────────────────────────────────────────────────
  const [userSkills] = useState(['C#', 'SQL', 'DDD', 'Clean Architecture']);
  const [aiRecommendations] = useState([
    {
      id: 'rec-1',
      code: 'CRS-MICROSERVICES',
      title: 'Khóa học Thiết kế Kiến trúc Hệ thống Microservices Đa Tenant',
      reqSkills: ['C#', 'DDD', 'Microservices', 'Event-Driven'],
      reason: 'AI phát hiện bạn đã hoàn thành xuất sắc khóa DDD. Đây là bước đệm tiếp theo hoàn hảo.',
    },
    {
      id: 'rec-2',
      code: 'CRS-PERF-TUNING',
      title: 'Khóa Tối ưu Hiệu năng SQL Server & C# Memory Profiling',
      reqSkills: ['C#', 'SQL', 'Performance'],
      reason: 'Khóa học bổ trợ trực tiếp kỹ năng làm việc với lượng lớn dữ liệu phát sinh trong ERP.',
    },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: AI LESSON SUMMARIZATION (UC_LMS_072)
  // ────────────────────────────────────────────────────────────────────────────
  const [summaryInput, setSummaryInput] = useState({
    title: 'Bài 01: Kiến trúc Onion & Clean Architecture trong C#',
    rawText: 'Mô hình Clean Architecture chia ứng dụng thành 4 tầng độc lập: Domain, Application, Infrastructure và Api. Tầng Domain là trung tâm không phụ thuộc vào bất kỳ thư viện bên ngoài nào. Tầng Application chứa các DTOs, Use Cases và Interfaces. Tầng Infrastructure triển khai DbContext và Repository. Tầng API định nghĩa các Controller và DTOs.',
  });
  const [aiSummaryResult, setAiSummaryResult] = useState<any>(null);

  const handleGenerateSummary = (e: React.FormEvent) => {
    e.preventDefault();
    if (!summaryInput.title) {
      showToast('Vui lòng nhập tiêu đề bài học.', 'error');
      return;
    }

    const bullets = formatAiSummaryBullets(summaryInput.rawText);

    const result = {
      title: summaryInput.title,
      overview: `AI đã tổng hợp tự động bài giảng [${summaryInput.title}].`,
      takeaways: bullets.length > 0 ? bullets : [
        'Nắm vững cách phân chia 4 tầng độc lập trong Clean Architecture.',
        'Đảm bảo tầng Domain không phụ thuộc vào thư viện bên ngoài.',
        'Sử dụng Dependency Injection để nối tầng Infrastructure với Application.'
      ],
      nextTopics: ['Viết Unit Tests cho Services', 'Triển khai Repository Pattern với EF Core'],
    };

    setAiSummaryResult(result);
    showToast('AI đã tóm tắt bài học thành công!', 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: AI QUIZ GENERATION (UC_LMS_073)
  // ────────────────────────────────────────────────────────────────────────────
  const [quizInput, setQuizInput] = useState({
    topic: 'Nguyên lý Aggregate Root trong DDD',
    questionCount: 2,
  });
  const [generatedQuiz, setGeneratedQuiz] = useState<any>(null);

  const handleGenerateQuiz = (e: React.FormEvent) => {
    e.preventDefault();
    const mockQuestions = [
      {
        questionText: `Khái niệm cốt lõi nào quan trọng nhất trong bài [${quizInput.topic}]?`,
        options: [
          'Phân chia rõ ràng trách nhiệm giữa các lớp Domain và Infrastructure',
          'Viết trực tiếp truy vấn SQL trong Controller',
          'Không sử dụng Dependency Injection',
          'Lưu trữ tất cả biến dạng Global'
        ],
        correctOptionIndex: 0,
      },
      {
        questionText: 'Trong thiết kế DDD, Aggregate Root đóng vai trò gì?',
        options: [
          'Là thực thể duy nhất quản lý toàn bộ các thực thể con và bảo đảm tính nhất quán dữ liệu',
          'Là giao diện UI của ứng dụng',
          'Là dịch vụ gửi Email thông báo',
          'Là bảng dữ liệu trong bộ nhớ RAM'
        ],
        correctOptionIndex: 0,
      },
    ];

    const val = validateAiQuizStructure(mockQuestions);
    if (!val.isValid) {
      showToast(val.errorMessage || 'Lỗi cấu trúc quiz sinh bởi AI', 'error');
      return;
    }

    setGeneratedQuiz({
      topic: quizInput.topic,
      questions: mockQuestions,
    });
    showToast(`AI đã tự động sinh ${mockQuestions.length} câu hỏi Quiz trắc nghiệm thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: AI LEARNING QNA CHATBOT (UC_LMS_074)
  // ────────────────────────────────────────────────────────────────────────────
  const [chatInput, setChatInput] = useState('');
  const [chatMessages, setChatMessages] = useState([
    { sender: 'ai', text: 'Xin chào! Tôi là Trợ lý Học tập AI của ERP Hùng. Bạn có câu hỏi nào về bài học hôm nay không?' },
  ]);

  const handleSendChat = (e: React.FormEvent) => {
    e.preventDefault();
    if (!chatInput.trim()) return;

    const userMsg = chatInput.trim();
    setChatMessages((prev) => [...prev, { sender: 'user', text: userMsg }]);
    setChatInput('');

    setTimeout(() => {
      const aiReply = `Trợ lý AI trả lời: Đối với câu hỏi "${userMsg}", quy tắc dự án ERP Hùng yêu cầu luôn kiểm tra tiền điều kiện, áp dụng Clean Architecture và viết unit test phủ 100% logic trước khi gửi báo cáo!`;
      setChatMessages((prev) => [...prev, { sender: 'ai', text: aiReply }]);
    }, 600);
  };

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
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
      <div className="bg-gradient-to-r from-purple-900 via-indigo-900 to-cyan-900 p-6 rounded-2xl text-white shadow-xl">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-cyan-500/30 text-cyan-200 text-xs px-3 py-1 rounded-full font-semibold border border-cyan-400/30">
              LMS - AI HỖ TRỢ HỌC TẬP THÔNG MINH
            </span>
            <h1 className="text-2xl font-bold mt-2">Bước 167: AI Learning Assistant Suite</h1>
            <p className="text-cyan-100 text-sm mt-1">
              Gợi ý khóa học AI, Tóm tắt bài học tự động, Tạo Quiz trắc nghiệm từ tài liệu & Trợ lý hỏi đáp AI
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-2 mt-6 border-t border-white/10 pt-4">
          <button
            onClick={() => setActiveTab('recommend')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'recommend' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🎯 UC_LMS_071: Gợi ý khóa học AI
          </button>
          <button
            onClick={() => setActiveTab('summary')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'summary' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📄 UC_LMS_072: AI Tóm tắt bài học
          </button>
          <button
            onClick={() => setActiveTab('quiz')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'quiz' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⚡ UC_LMS_073: AI Tạo Quiz
          </button>
          <button
            onClick={() => setActiveTab('qna')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'qna' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🤖 UC_LMS_074: Trợ lý Hỏi đáp AI
          </button>
        </div>
      </div>

      {/* TAB 1: AI RECOMMENDATIONS */}
      {activeTab === 'recommend' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-slate-800">🎯 Gợi Ý Khóa Học Tiếp Theo Bằng AI (UC_LMS_071)</h2>
              <p className="text-xs text-slate-500 mt-1">
                Kỹ năng hiện tại của học viên: {userSkills.map((s) => `[${s}]`).join(' ')}
              </p>
            </div>
            <span className="px-3 py-1 text-xs font-bold rounded-full bg-purple-100 text-purple-800">
              🤖 AI Engine Personalization Active
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {aiRecommendations.map((rec) => {
              const matchScore = calculateAiMatchScore(userSkills, rec.reqSkills);
              return (
                <div key={rec.id} className="p-5 rounded-xl border border-purple-200 bg-purple-50/40 space-y-4">
                  <div className="flex justify-between items-start">
                    <div>
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-purple-200 text-purple-900">{rec.code}</span>
                      <h3 className="font-bold text-slate-900 mt-1 text-base">{rec.title}</h3>
                    </div>
                    <span className="px-3 py-1 text-xs font-extrabold rounded-full bg-emerald-600 text-white">
                      Độ phù hợp: {matchScore}%
                    </span>
                  </div>

                  <div className="bg-white p-3 rounded-lg border border-slate-200 text-xs space-y-1">
                    <p className="font-semibold text-purple-900">Lý do AI đề xuất:</p>
                    <p className="text-slate-600 italic">{rec.reason}</p>
                  </div>

                  <div className="flex items-center gap-1.5 flex-wrap text-xs">
                    <span className="text-slate-500 font-medium">Kỹ năng yêu cầu:</span>
                    {rec.reqSkills.map((sk) => (
                      <span key={sk} className="px-2 py-0.5 text-xs rounded-full bg-slate-200 text-slate-700">
                        {sk}
                      </span>
                    ))}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 2: AI LESSON SUMMARIZATION */}
      {activeTab === 'summary' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
              <h2 className="text-lg font-bold text-slate-800 mb-4">📄 Tóm Tắt Bài Học Bằng AI (UC_LMS_072)</h2>
              <form onSubmit={handleGenerateSummary} className="space-y-4 text-sm">
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Tiêu đề bài học:</label>
                  <input
                    type="text"
                    value={summaryInput.title}
                    onChange={(e) => setSummaryInput({ ...summaryInput, title: e.target.value })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                  />
                </div>
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Nội dung bài học thô (Text / Transcript):</label>
                  <textarea
                    value={summaryInput.rawText}
                    onChange={(e) => setSummaryInput({ ...summaryInput, rawText: e.target.value })}
                    className="w-full border border-slate-300 rounded-lg p-2 font-mono text-xs"
                    rows={6}
                  />
                </div>

                <button type="submit" className="w-full py-2.5 bg-indigo-600 text-white rounded-lg font-semibold hover:bg-indigo-700">
                  ✨ Kích hoạt AI Tóm Tắt Ý Chính
                </button>
              </form>
            </div>

            <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
              <h2 className="text-lg font-bold text-slate-800 mb-4">✨ Kết quả Tóm tắt AI</h2>
              {aiSummaryResult ? (
                <div className="p-4 rounded-xl border border-indigo-200 bg-indigo-50/50 space-y-4 text-xs">
                  <h3 className="font-bold text-indigo-950 text-sm">{aiSummaryResult.title}</h3>
                  <p className="text-slate-700">{aiSummaryResult.overview}</p>

                  <div className="bg-white p-3 rounded-lg border border-slate-200 space-y-1">
                    <p className="font-bold text-slate-800">📌 Các điểm mấu chốt (Key Takeaways):</p>
                    <ul className="list-disc list-inside text-slate-600 space-y-1">
                      {aiSummaryResult.takeaways.map((t: string, idx: number) => (
                        <li key={idx}>{t}</li>
                      ))}
                    </ul>
                  </div>

                  <div className="bg-white p-3 rounded-lg border border-slate-200 space-y-1">
                    <p className="font-bold text-slate-800">💡 Gợi ý chủ đề học tiếp theo:</p>
                    <ul className="list-disc list-inside text-slate-600 space-y-1">
                      {aiSummaryResult.nextTopics.map((nt: string, idx: number) => (
                        <li key={idx}>{nt}</li>
                      ))}
                    </ul>
                  </div>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-400 text-sm">
                  Chưa thực hiện tóm tắt. Nhập nội dung bài học ở bên trái và bấm kích hoạt.
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: AI QUIZ GENERATION */}
      {activeTab === 'quiz' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
              <h2 className="text-lg font-bold text-slate-800 mb-4">⚡ AI Tạo Quiz Trắc Nghiệm Tự Động (UC_LMS_073)</h2>
              <form onSubmit={handleGenerateQuiz} className="space-y-4 text-sm">
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Chủ đề bài học:</label>
                  <input
                    type="text"
                    value={quizInput.topic}
                    onChange={(e) => setQuizInput({ ...quizInput, topic: e.target.value })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                  />
                </div>
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Số lượng câu hỏi sinh tự động:</label>
                  <input
                    type="number"
                    value={quizInput.questionCount}
                    onChange={(e) => setQuizInput({ ...quizInput, questionCount: Number(e.target.value) })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                    min={1}
                    max={10}
                  />
                </div>

                <button type="submit" className="w-full py-2.5 bg-purple-600 text-white rounded-lg font-semibold hover:bg-purple-700">
                  🎲 AI Sinh Đề Thi Trắc Nghiệm
                </button>
              </form>
            </div>

            <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5">
              <h2 className="text-lg font-bold text-slate-800 mb-4">📋 Đề Thi Trắc Nghiệm AI Vừa Sinh</h2>
              {generatedQuiz ? (
                <div className="space-y-4 text-xs">
                  <span className="px-3 py-1 text-xs font-bold rounded-full bg-purple-100 text-purple-800">
                    Chủ đề: {generatedQuiz.topic} ({generatedQuiz.questions.length} câu hỏi)
                  </span>

                  <div className="space-y-3">
                    {generatedQuiz.questions.map((q: any, idx: number) => (
                      <div key={idx} className="p-4 rounded-xl border border-slate-200 bg-slate-50 space-y-2">
                        <p className="font-bold text-slate-900 text-sm">
                          Câu {idx + 1}: {q.questionText}
                        </p>
                        <div className="space-y-1 pl-2">
                          {q.options.map((opt: string, optIdx: number) => (
                            <div
                              key={optIdx}
                              className={`p-2 rounded border text-slate-700 ${
                                optIdx === q.correctOptionIndex
                                  ? 'bg-emerald-100 border-emerald-300 font-semibold text-emerald-900'
                                  : 'bg-white border-slate-200'
                              }`}
                            >
                              {String.fromCharCode(65 + optIdx)}. {opt} {optIdx === q.correctOptionIndex && '✓ (Đáp án đúng)'}
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-400 text-sm">
                  Chưa sinh đề thi. Hãy chọn chủ đề và bấm nút ở bên trái.
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* TAB 4: AI LEARNING QNA CHATBOT */}
      {activeTab === 'qna' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🤖 Trợ Lý Học Tập Hỏi Đáp AI 24/7 (UC_LMS_074)</h2>

          <div className="h-80 border border-slate-200 rounded-xl p-4 overflow-y-auto space-y-3 bg-slate-50">
            {chatMessages.map((msg, idx) => (
              <div key={idx} className={`flex ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}>
                <div
                  className={`max-w-xl p-3 rounded-2xl text-xs font-medium ${
                    msg.sender === 'user' ? 'bg-indigo-600 text-white rounded-br-none' : 'bg-white border border-slate-200 text-slate-800 rounded-bl-none shadow-sm'
                  }`}
                >
                  {msg.text}
                </div>
              </div>
            ))}
          </div>

          <form onSubmit={handleSendChat} className="flex gap-2">
            <input
              type="text"
              value={chatInput}
              onChange={(e) => setChatInput(e.target.value)}
              placeholder="Đặt câu hỏi về bài học cho Trợ lý AI..."
              className="flex-1 border border-slate-300 rounded-lg text-xs px-3 py-2"
            />
            <button type="submit" className="px-5 py-2 bg-cyan-600 text-white rounded-lg font-semibold text-xs hover:bg-cyan-700">
              Gửi câu hỏi
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
