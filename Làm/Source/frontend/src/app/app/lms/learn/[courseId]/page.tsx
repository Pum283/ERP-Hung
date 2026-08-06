"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useCallback, useEffect, useMemo, useState } from "react";
import {
  completeLmsLesson,
  fetchLearnerExams,
  fetchLmsLearn,
  startLmsExam,
  submitLmsAttempt,
  type LmsAttemptDto,
  type LmsAttemptResultDto,
  type LmsLearnerExamDto,
  type LmsLearnCourseDto,
  type LmsLessonDto,
} from "@/shared/api/lms-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { panel, statusPill } from "@/shared/ui/field";

export default function LmsLearnPage() {
  const params = useParams<{ courseId: string }>();
  const courseId = params.courseId;
  const { can } = usePermissions();
  const canRead = can("lms.learn.read");

  const [data, setData] = useState<LmsLearnCourseDto | null>(null);
  const [exams, setExams] = useState<LmsLearnerExamDto[]>([]);
  const [activeId, setActiveId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [attempt, setAttempt] = useState<LmsAttemptDto | null>(null);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [result, setResult] = useState<LmsAttemptResultDto | null>(null);

  const load = useCallback(async () => {
    const [d, ex] = await Promise.all([
      fetchLmsLearn(courseId),
      fetchLearnerExams(courseId).catch(() => [] as LmsLearnerExamDto[]),
    ]);
    setData(d);
    setExams(ex);
    setActiveId((prev) => prev || d.resumeLessonId || d.lessons[0]?.id || "");
  }, [courseId]);

  useEffect(() => {
    if (!canRead || !courseId) {
      setLoading(false);
      return;
    }
    setLoading(true);
    load()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, courseId, load]);

  const completed = useMemo(() => {
    const s = new Set<string>();
    data?.progress.filter((p) => p.status === "Completed").forEach((p) => s.add(p.lessonId));
    return s;
  }, [data]);

  const active: LmsLessonDto | null = useMemo(
    () => data?.lessons.find((l) => l.id === activeId) ?? null,
    [data, activeId],
  );

  async function markDone() {
    if (!active) return;
    try {
      await completeLmsLesson(courseId, active.id);
      setOk("Đã đánh dấu hoàn thành bài.");
      setError(null);
      await load();
      const next = data?.lessons.find((l) => !completed.has(l.id) && l.id !== active.id);
      if (next) setActiveId(next.id);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function beginExam(examId: string) {
    try {
      const a = await startLmsExam(examId);
      setAttempt(a);
      setAnswers({});
      setResult(null);
      setOk(`Bắt đầu lần thi #${a.attemptNo}`);
      setError(null);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function submitExam() {
    if (!attempt) return;
    try {
      const r = await submitLmsAttempt(attempt.id, answers);
      setResult(r);
      setAttempt(null);
      setOk(r.passed ? "Đậu!" : "Chưa đạt điểm.");
      setError(null);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền học online.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <Link href="/app/lms/catalog" className="text-xs text-[var(--muted)] underline">
            ← Catalog
          </Link>
          <h1 className="mt-1 text-xl font-semibold tracking-tight">
            {data?.course.name ?? "Đang tải…"}
          </h1>
          {data && (
            <p className="mt-1 text-sm text-[var(--muted)]">
              Tiến độ {data.enrollment.progressPct}% · {data.enrollment.status}
            </p>
          )}
        </div>
        {data && (
          <span className={statusPill(data.enrollment.status === "Completed" ? "success" : "brand")}>
            {data.enrollment.progressPct}%
          </span>
        )}
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      {data && (
        <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
          <aside className={`${panel} space-y-3`}>
            {data.chapters.map((ch) => (
              <div key={ch.id}>
                <div className="mb-1 text-xs font-semibold uppercase tracking-wide text-[var(--muted)]">
                  {ch.title}
                </div>
                <ul className="space-y-1">
                  {data.lessons
                    .filter((l) => l.chapterId === ch.id)
                    .map((l) => {
                      const done = completed.has(l.id);
                      const activeRow = l.id === activeId && !attempt;
                      return (
                        <li key={l.id}>
                          <button
                            type="button"
                            onClick={() => {
                              setActiveId(l.id);
                              setAttempt(null);
                              setResult(null);
                            }}
                            className={`w-full rounded-md px-2 py-1.5 text-left text-sm ${
                              activeRow
                                ? "bg-[var(--brand)] text-white"
                                : "hover:bg-[var(--surface-2)]"
                            }`}
                          >
                            {done ? "✓ " : ""}
                            {l.title}
                          </button>
                        </li>
                      );
                    })}
                </ul>
              </div>
            ))}

            {exams.length > 0 && (
              <div className="border-t border-[var(--border)] pt-3">
                <div className="mb-1 text-xs font-semibold uppercase tracking-wide text-[var(--muted)]">
                  Thi & quiz
                </div>
                <ul className="space-y-2">
                  {exams.map((ex) => (
                    <li key={ex.id} className="rounded-md border border-[var(--border)] p-2 text-sm">
                      <div className="font-medium">{ex.name}</div>
                      <div className="text-xs text-[var(--muted)]">
                        {ex.examType} · đạt {ex.passScore}% · {ex.attemptsUsed}/{ex.maxAttempts} lần
                        {ex.lastScore != null && ` · lần trước ${ex.lastScore}%`}
                        {ex.lastPassed === true && " · Đậu"}
                      </div>
                      <button
                        type="button"
                        className={`${btn.ghost} mt-2 w-full`}
                        disabled={!ex.canStart}
                        onClick={() => beginExam(ex.id)}
                      >
                        {ex.canStart ? "Làm bài" : "Hết lượt / đang làm"}
                      </button>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </aside>

          <section className={panel}>
            {attempt?.questions && (
              <div className="space-y-4">
                <h2 className="text-lg font-semibold">Làm bài · lần #{attempt.attemptNo}</h2>
                {attempt.questions.map((q, idx) => (
                  <div key={q.questionId} className="rounded-md border border-[var(--border)] p-3">
                    <div className="mb-2 text-sm font-medium">
                      {idx + 1}. {q.stem} <span className="text-[var(--muted)]">({q.points}đ)</span>
                    </div>
                    <div className="space-y-1">
                      {q.options.map((o) => (
                        <label key={o.key} className="flex cursor-pointer items-center gap-2 text-sm">
                          <input
                            type="radio"
                            name={q.questionId}
                            checked={answers[q.questionId] === o.key}
                            onChange={() => setAnswers((prev) => ({ ...prev, [q.questionId]: o.key }))}
                          />
                          <span>{o.key}. {o.text}</span>
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
                <button type="button" className={btn.primary} onClick={submitExam}>
                  Nộp bài
                </button>
              </div>
            )}

            {!attempt && result && (
              <div className="space-y-4">
                <div>
                  <h2 className="text-lg font-semibold">Kết quả lần #{result.attemptNo}</h2>
                  <p className="text-sm text-[var(--muted)]">
                    {result.score}/{result.maxScore} điểm (
                    {result.maxScore ? Math.round((100 * result.score) / result.maxScore) : 0}
                    %) · cần {result.passScore}% ·{" "}
                    <span className={statusPill(result.passed ? "success" : "danger")}>
                      {result.passed ? "Đậu" : "Rớt"}
                    </span>
                  </p>
                </div>
                <ul className="space-y-2 text-sm">
                  {result.reviews.map((r) => (
                    <li key={r.questionId} className="rounded-md border border-[var(--border)] p-2">
                      <div className="font-medium">{r.stem}</div>
                      <div className="text-[var(--muted)]">
                        Bạn chọn: {r.yourKey ?? "—"} · Đáp án: {r.correctKeys.join(", ")} ·{" "}
                        {r.isCorrect ? "Đúng" : "Sai"} ({r.pointsEarned}/{r.points})
                      </div>
                    </li>
                  ))}
                </ul>
                {result.certificate && (
                  <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-800">
                    Đã cấp chứng chỉ <strong className="font-mono">{result.certificate.code}</strong>
                    {" · "}
                    <Link href="/app/lms/certificates" className="underline">Xem chứng chỉ</Link>
                  </div>
                )}
                {!result.certificate && result.passed && (
                  <p className="text-sm text-[var(--muted)]">
                    Đậu thi nhưng chưa đủ điều kiện cấp chứng chỉ (cần hoàn thành hết bài học).
                  </p>
                )}
              </div>
            )}

            {!attempt && !result && (
              <>
                {!active && <p className="text-sm text-[var(--muted)]">Chọn bài học bên trái.</p>}
                {active && (
                  <div className="space-y-4">
                    <div>
                      <div className="text-xs text-[var(--muted)]">{active.lessonType}</div>
                      <h2 className="text-lg font-semibold">{active.title}</h2>
                    </div>

                    {active.lessonType === "Video" && active.contentUrl && (
                      <div className="aspect-video overflow-hidden rounded-md bg-black">
                        <iframe
                          title={active.title}
                          src={active.contentUrl}
                          className="h-full w-full"
                          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                          allowFullScreen
                        />
                      </div>
                    )}

                    {active.lessonType === "Document" && active.contentUrl && (
                      <div className="space-y-2">
                        <a href={active.contentUrl} target="_blank" rel="noreferrer" className="text-sm underline">
                          Mở tài liệu / PDF
                        </a>
                        <iframe title={active.title} src={active.contentUrl} className="h-[480px] w-full rounded-md border border-[var(--border)]" />
                      </div>
                    )}

                    {active.lessonType === "Text" && (
                      <div className="whitespace-pre-wrap text-sm leading-relaxed">
                        {active.body || "Chưa có nội dung."}
                      </div>
                    )}

                    <div className="flex flex-wrap gap-2 border-t border-[var(--border)] pt-3">
                      {completed.has(active.id) ? (
                        <span className={statusPill("success")}>Đã hoàn thành</span>
                      ) : (
                        <button type="button" className={btn.primary} onClick={markDone}>
                          Đánh dấu hoàn thành
                        </button>
                      )}
                    </div>
                  </div>
                )}
              </>
            )}
          </section>
        </div>
      )}
    </div>
  );
}
