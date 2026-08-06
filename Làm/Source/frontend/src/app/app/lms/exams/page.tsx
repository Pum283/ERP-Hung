"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  addQuestionToLmsExam,
  fetchLmsCourseDetail,
  fetchLmsCourses,
  fetchLmsExamDetail,
  fetchLmsExams,
  fetchLmsQuestions,
  publishLmsExam,
  upsertLmsExam,
  upsertLmsQuestion,
  type LmsCourseDto,
  type LmsExamDetailDto,
  type LmsExamDto,
  type LmsQuestionDto,
} from "@/shared/api/lms-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

const EXAM_TONE: Record<string, "muted" | "brand" | "success" | "warning"> = {
  Draft: "muted",
  Published: "success",
  Archived: "warning",
};

export default function LmsExamsPage() {
  const { can } = usePermissions();
  const canRead = can("lms.exam.read");
  const canManage = can("lms.exam.manage");

  const [questions, setQuestions] = useState<LmsQuestionDto[]>([]);
  const [exams, setExams] = useState<LmsExamDto[]>([]);
  const [courses, setCourses] = useState<LmsCourseDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<LmsExamDetailDto | null>(null);
  const [chapters, setChapters] = useState<{ id: string; title: string }[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [qCode, setQCode] = useState("CH-001");
  const [qStem, setQStem] = useState("");
  const [qType, setQType] = useState("SingleChoice");
  const [optA, setOptA] = useState("");
  const [optB, setOptB] = useState("");
  const [optC, setOptC] = useState("");
  const [optD, setOptD] = useState("");
  const [correct, setCorrect] = useState("A");
  const [qPoints, setQPoints] = useState("1");

  const [eCode, setECode] = useState("DE-001");
  const [eName, setEName] = useState("");
  const [eType, setEType] = useState("Final");
  const [eCourseId, setECourseId] = useState("");
  const [eChapterId, setEChapterId] = useState("");
  const [passScore, setPassScore] = useState("70");
  const [maxAttempts, setMaxAttempts] = useState("3");
  const [addQuestionId, setAddQuestionId] = useState("");

  const load = useCallback(async () => {
    const [q, e, c] = await Promise.all([
      fetchLmsQuestions(),
      fetchLmsExams(),
      fetchLmsCourses().catch(() => [] as LmsCourseDto[]),
    ]);
    setQuestions(q);
    setExams(e);
    setCourses(c);
    if (!selectedId && e[0]) setSelectedId(e[0].id);
    if (!eCourseId && c[0]) setECourseId(c[0].id);
    if (!addQuestionId && q[0]) setAddQuestionId(q[0].id);
  }, [selectedId, eCourseId, addQuestionId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    load()
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchLmsExamDetail(selectedId)
      .then(setDetail)
      .catch((err: Error) => setError(err.message));
  }, [selectedId, canRead]);

  useEffect(() => {
    if (!eCourseId) {
      setChapters([]);
      return;
    }
    fetchLmsCourseDetail(eCourseId)
      .then((d) => setChapters(d.chapters.map((ch) => ({ id: ch.id, title: ch.title }))))
      .catch(() => setChapters([]));
  }, [eCourseId]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onSaveQuestion(e: FormEvent) {
    e.preventDefault();
    try {
      const options = [
        { key: "A", text: optA },
        { key: "B", text: optB },
      ];
      if (qType === "SingleChoice") {
        if (optC.trim()) options.push({ key: "C", text: optC });
        if (optD.trim()) options.push({ key: "D", text: optD });
      }
      await upsertLmsQuestion({
        code: qCode,
        stem: qStem,
        questionType: qType,
        options,
        correctKeys: [correct],
        points: Number(qPoints) || 1,
      });
      setQStem("");
      setOptA("");
      setOptB("");
      setOptC("");
      setOptD("");
      await load();
      flash("Đã lưu câu hỏi.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onSaveExam(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertLmsExam({
        code: eCode,
        name: eName,
        examType: eType,
        courseId: eCourseId || null,
        chapterId: eType === "ChapterQuiz" ? eChapterId || null : null,
        passScore: Number(passScore) || 70,
        maxAttempts: Number(maxAttempts) || 3,
        status: "Draft",
      });
      setEName("");
      await load();
      setSelectedId(saved.id);
      flash("Đã tạo đề thi.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAddQuestion(e: FormEvent) {
    e.preventDefault();
    if (!selectedId || !addQuestionId) return;
    try {
      await addQuestionToLmsExam(selectedId, addQuestionId);
      setDetail(await fetchLmsExamDetail(selectedId));
      await load();
      flash("Đã thêm câu vào đề.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function setStatus(status: string) {
    if (!selectedId) return;
    try {
      await publishLmsExam(selectedId, status);
      await load();
      setDetail(await fetchLmsExamDetail(selectedId));
      flash(`Trạng thái → ${status}`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem đề thi / NHCH.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Đề thi & ngân hàng câu hỏi</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          NHCH · đề cố định · điểm đạt / số lần thi (UC_LMS_010, 012, 014)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Ngân hàng câu hỏi</h2>
          {canManage && (
            <form onSubmit={onSaveQuestion} className="mb-4 grid gap-2 sm:grid-cols-2">
              <input className={field} placeholder="Mã" value={qCode} onChange={(e) => setQCode(e.target.value)} required />
              <select className={field} value={qType} onChange={(e) => setQType(e.target.value)}>
                <option value="SingleChoice">SingleChoice</option>
                <option value="TrueFalse">TrueFalse</option>
              </select>
              <input className={`${field} sm:col-span-2`} placeholder="Câu hỏi" value={qStem} onChange={(e) => setQStem(e.target.value)} required />
              <input className={field} placeholder="A" value={optA} onChange={(e) => setOptA(e.target.value)} required />
              <input className={field} placeholder="B" value={optB} onChange={(e) => setOptB(e.target.value)} required />
              {qType === "SingleChoice" && (
                <>
                  <input className={field} placeholder="C (tuỳ chọn)" value={optC} onChange={(e) => setOptC(e.target.value)} />
                  <input className={field} placeholder="D (tuỳ chọn)" value={optD} onChange={(e) => setOptD(e.target.value)} />
                </>
              )}
              <select className={field} value={correct} onChange={(e) => setCorrect(e.target.value)}>
                <option value="A">Đúng: A</option>
                <option value="B">Đúng: B</option>
                {qType === "SingleChoice" && optC && <option value="C">Đúng: C</option>}
                {qType === "SingleChoice" && optD && <option value="D">Đúng: D</option>}
              </select>
              <input className={field} type="number" min={0.5} step={0.5} value={qPoints} onChange={(e) => setQPoints(e.target.value)} />
              <button type="submit" className={`${btn.primary} sm:col-span-2`}>Lưu câu hỏi</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Câu hỏi</th>
                  <th className={th}>Loại</th>
                  <th className={th}>Đúng</th>
                </tr>
              </thead>
              <tbody>
                {questions.map((q) => (
                  <tr key={q.id}>
                    <td className={td}>{q.code}</td>
                    <td className={td}>{q.stem}</td>
                    <td className={td}>{q.questionType}</td>
                    <td className={td}>{q.correctKeys.join(",")}</td>
                  </tr>
                ))}
                {questions.length === 0 && (
                  <tr><td className={td} colSpan={4}>Chưa có câu hỏi.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo đề thi</h2>
              <form onSubmit={onSaveExam} className="grid gap-2 sm:grid-cols-2">
                <input className={field} placeholder="Mã đề" value={eCode} onChange={(e) => setECode(e.target.value)} required />
                <input className={field} placeholder="Tên đề" value={eName} onChange={(e) => setEName(e.target.value)} required />
                <select className={field} value={eType} onChange={(e) => setEType(e.target.value)}>
                  <option value="Final">Final (cuối khóa)</option>
                  <option value="ChapterQuiz">ChapterQuiz</option>
                </select>
                <select className={field} value={eCourseId} onChange={(e) => setECourseId(e.target.value)}>
                  <option value="">— Khóa học —</option>
                  {courses.map((c) => (
                    <option key={c.id} value={c.id}>{c.code} · {c.name}</option>
                  ))}
                </select>
                {eType === "ChapterQuiz" && (
                  <select className={`${field} sm:col-span-2`} value={eChapterId} onChange={(e) => setEChapterId(e.target.value)}>
                    <option value="">— Chương —</option>
                    {chapters.map((ch) => (
                      <option key={ch.id} value={ch.id}>{ch.title}</option>
                    ))}
                  </select>
                )}
                <input className={field} type="number" min={0} max={100} value={passScore} onChange={(e) => setPassScore(e.target.value)} title="Điểm đạt %" />
                <input className={field} type="number" min={1} value={maxAttempts} onChange={(e) => setMaxAttempts(e.target.value)} title="Số lần thi" />
                <button type="submit" className={`${btn.primary} sm:col-span-2`}>Tạo đề</button>
              </form>
            </section>
          )}

          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Danh sách đề</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Tên</th>
                    <th className={th}>Loại</th>
                    <th className={th}>TT</th>
                    <th className={th}>Câu</th>
                  </tr>
                </thead>
                <tbody>
                  {exams.map((ex) => (
                    <tr
                      key={ex.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === ex.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setSelectedId(ex.id)}
                    >
                      <td className={td}>{ex.code}</td>
                      <td className={td}>{ex.name}</td>
                      <td className={td}>{ex.examType}</td>
                      <td className={td}>
                        <span className={statusPill(EXAM_TONE[ex.status] ?? "muted")}>{ex.status}</span>
                      </td>
                      <td className={td}>{ex.questionCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          {detail && (
            <section className={panel}>
              <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
                <div>
                  <h2 className="text-sm font-semibold">{detail.exam.name}</h2>
                  <p className="text-xs text-[var(--muted)]">
                    Đạt {detail.exam.passScore}% · tối đa {detail.exam.maxAttempts} lần · {detail.exam.courseName ?? "chưa gắn khóa"}
                  </p>
                </div>
                {canManage && (
                  <div className="flex gap-2">
                    <button type="button" className={btn.primary} onClick={() => setStatus("Published")}>Xuất bản</button>
                    <button type="button" className={btn.ghost} onClick={() => setStatus("Draft")}>Draft</button>
                  </div>
                )}
              </div>
              <ul className="mb-3 space-y-1 text-sm">
                {detail.questions.map((q) => (
                  <li key={q.id}>{q.sortOrder}. [{q.questionCode}] {q.stem} ({q.points}đ)</li>
                ))}
                {detail.questions.length === 0 && (
                  <li className="text-[var(--muted)]">Chưa có câu trong đề.</li>
                )}
              </ul>
              {canManage && (
                <form onSubmit={onAddQuestion} className="flex flex-wrap gap-2 border-t border-[var(--border)] pt-3">
                  <select className={`${field} min-w-[220px] flex-1`} value={addQuestionId} onChange={(e) => setAddQuestionId(e.target.value)}>
                    {questions.map((q) => (
                      <option key={q.id} value={q.id}>{q.code} · {q.stem.slice(0, 40)}</option>
                    ))}
                  </select>
                  <button type="submit" className={btn.primary}>Thêm câu vào đề</button>
                </form>
              )}
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
