"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import {
  fetchLmsCourseDetail,
  fetchLmsCourses,
  fetchLmsPrograms,
  publishLmsCourse,
  upsertLmsChapter,
  upsertLmsCourse,
  upsertLmsLesson,
  upsertLmsProgram,
  type LmsCourseDetailDto,
  type LmsCourseDto,
  type LmsProgramDto,
} from "@/shared/api/lms-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

const COURSE_TONE: Record<string, "muted" | "brand" | "success" | "warning" | "danger"> = {
  Draft: "muted",
  Published: "success",
  Hidden: "warning",
};

export default function LmsCoursesPage() {
  const { can } = usePermissions();
  const canRead = can("lms.course.read");
  const canManage = can("lms.course.manage");

  const [programs, setPrograms] = useState<LmsProgramDto[]>([]);
  const [courses, setCourses] = useState<LmsCourseDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<LmsCourseDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [progCode, setProgCode] = useState("CTDT-001");
  const [progName, setProgName] = useState("");
  const [progDesc, setProgDesc] = useState("");

  const [code, setCode] = useState("KH-001");
  const [name, setName] = useState("");
  const [summary, setSummary] = useState("");
  const [programId, setProgramId] = useState("");
  const [deliveryMode, setDeliveryMode] = useState("Online");
  const [price, setPrice] = useState("0");
  const [editingId, setEditingId] = useState<string | undefined>();

  const [chapterTitle, setChapterTitle] = useState("");
  const [lessonChapterId, setLessonChapterId] = useState("");
  const [lessonTitle, setLessonTitle] = useState("");
  const [lessonType, setLessonType] = useState("Text");
  const [lessonUrl, setLessonUrl] = useState("");
  const [lessonBody, setLessonBody] = useState("");

  const selected = useMemo(
    () => courses.find((c) => c.id === selectedId) ?? null,
    [courses, selectedId],
  );

  const loadList = useCallback(async () => {
    const [p, c] = await Promise.all([fetchLmsPrograms(), fetchLmsCourses()]);
    setPrograms(p);
    setCourses(c);
    if (!selectedId && c[0]) setSelectedId(c[0].id);
    if (!programId && p[0]) setProgramId(p[0].id);
  }, [selectedId, programId]);

  const loadDetail = useCallback(async (id: string) => {
    if (!id) {
      setDetail(null);
      return;
    }
    const d = await fetchLmsCourseDetail(id);
    setDetail(d);
    if (!lessonChapterId && d.chapters[0]) setLessonChapterId(d.chapters[0].id);
  }, [lessonChapterId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    loadList()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, loadList]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    loadDetail(selectedId).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead, loadDetail]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onSaveProgram(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertLmsProgram({ code: progCode, name: progName, description: progDesc });
      setProgName("");
      setProgDesc("");
      await loadList();
      flash("Đã lưu chương trình.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onSaveCourse(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertLmsCourse({
        id: editingId,
        programId: programId || null,
        code,
        name,
        summary,
        deliveryMode,
        price: Number(price) || 0,
        currency: "VND",
        status: editingId ? undefined : "Draft",
      });
      setEditingId(undefined);
      setName("");
      setSummary("");
      await loadList();
      setSelectedId(saved.id);
      flash(editingId ? "Đã cập nhật khóa." : "Đã tạo khóa.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  function startEdit(c: LmsCourseDto) {
    setEditingId(c.id);
    setCode(c.code);
    setName(c.name);
    setSummary(c.summary ?? "");
    setProgramId(c.programId ?? "");
    setDeliveryMode(c.deliveryMode);
    setPrice(String(c.price));
    setSelectedId(c.id);
  }

  async function onAddChapter(e: FormEvent) {
    e.preventDefault();
    if (!selectedId) return;
    try {
      await upsertLmsChapter(selectedId, { title: chapterTitle });
      setChapterTitle("");
      await loadDetail(selectedId);
      await loadList();
      flash("Đã thêm chương.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAddLesson(e: FormEvent) {
    e.preventDefault();
    if (!lessonChapterId) return;
    try {
      await upsertLmsLesson(lessonChapterId, {
        title: lessonTitle,
        lessonType,
        contentUrl: lessonUrl || undefined,
        body: lessonBody || undefined,
      });
      setLessonTitle("");
      setLessonUrl("");
      setLessonBody("");
      if (selectedId) {
        await loadDetail(selectedId);
        await loadList();
      }
      flash("Đã thêm bài học.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function setStatus(status: string) {
    if (!selectedId) return;
    try {
      await publishLmsCourse(selectedId, status);
      await loadList();
      await loadDetail(selectedId);
      flash(status === "Published" ? "Đã xuất bản." : `Trạng thái → ${status}`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem khóa học.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Khóa học (catalog)</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          CTĐT · khóa · chương/bài · xuất bản (UC_LMS_001–006, 009)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách khóa</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Hình thức</th>
                  <th className={th}>TT</th>
                  <th className={th}>Bài</th>
                </tr>
              </thead>
              <tbody>
                {courses.map((c) => (
                  <tr
                    key={c.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === c.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(c.id)}
                  >
                    <td className={td}>{c.code}</td>
                    <td className={td}>{c.name}</td>
                    <td className={td}>{c.deliveryMode}</td>
                    <td className={td}>
                      <span className={statusPill(COURSE_TONE[c.status] ?? "muted")}>{c.status}</span>
                    </td>
                    <td className={td}>{c.lessonCount}</td>
                  </tr>
                ))}
                {courses.length === 0 && (
                  <tr>
                    <td className={td} colSpan={5}>
                      Chưa có khóa — tạo bên phải.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <>
              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Chương trình đào tạo</h2>
                <form onSubmit={onSaveProgram} className="grid gap-2 sm:grid-cols-2">
                  <input className={field} placeholder="Mã" value={progCode} onChange={(e) => setProgCode(e.target.value)} required />
                  <input className={field} placeholder="Tên CTĐT" value={progName} onChange={(e) => setProgName(e.target.value)} required />
                  <input className={`${field} sm:col-span-2`} placeholder="Mô tả" value={progDesc} onChange={(e) => setProgDesc(e.target.value)} />
                  <button type="submit" className={btn.primary}>Lưu CTĐT</button>
                </form>
                {programs.length > 0 && (
                  <ul className="mt-3 space-y-1 text-xs text-[var(--muted)]">
                    {programs.map((p) => (
                      <li key={p.id}>{p.code} · {p.name}</li>
                    ))}
                  </ul>
                )}
              </section>

              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">{editingId ? "Sửa khóa học" : "Tạo khóa học"}</h2>
                <form onSubmit={onSaveCourse} className="grid gap-2 sm:grid-cols-2">
                  <input className={field} placeholder="Mã" value={code} onChange={(e) => setCode(e.target.value)} required />
                  <input className={field} placeholder="Tên khóa" value={name} onChange={(e) => setName(e.target.value)} required />
                  <select className={field} value={programId} onChange={(e) => setProgramId(e.target.value)}>
                    <option value="">— Không gắn CTĐT —</option>
                    {programs.map((p) => (
                      <option key={p.id} value={p.id}>{p.name}</option>
                    ))}
                  </select>
                  <select className={field} value={deliveryMode} onChange={(e) => setDeliveryMode(e.target.value)}>
                    <option value="Online">Online</option>
                    <option value="Offline">Offline</option>
                    <option value="Blended">Blended</option>
                  </select>
                  <input className={field} type="number" min={0} placeholder="Giá" value={price} onChange={(e) => setPrice(e.target.value)} />
                  <input className={`${field} sm:col-span-2`} placeholder="Tóm tắt" value={summary} onChange={(e) => setSummary(e.target.value)} />
                  <div className="flex gap-2 sm:col-span-2">
                    <button type="submit" className={btn.primary}>{editingId ? "Cập nhật" : "Tạo khóa"}</button>
                    {editingId && (
                      <button type="button" className={btn.ghost} onClick={() => setEditingId(undefined)}>Hủy sửa</button>
                    )}
                    {selected && (
                      <button type="button" className={btn.ghost} onClick={() => startEdit(selected)}>Sửa khóa đang chọn</button>
                    )}
                  </div>
                </form>
              </section>
            </>
          )}

          {selected && detail && (
            <section className={panel}>
              <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
                <div>
                  <h2 className="text-sm font-semibold">{detail.course.name}</h2>
                  <p className="text-xs text-[var(--muted)]">
                    {detail.course.code} · {detail.course.deliveryMode} · {detail.course.chapterCount} chương / {detail.course.lessonCount} bài
                  </p>
                </div>
                {canManage && (
                  <div className="flex flex-wrap gap-2">
                    <button type="button" className={btn.primary} onClick={() => setStatus("Published")}>Xuất bản</button>
                    <button type="button" className={btn.ghost} onClick={() => setStatus("Hidden")}>Ẩn</button>
                    <button type="button" className={btn.ghost} onClick={() => setStatus("Draft")}>Về Draft</button>
                  </div>
                )}
              </div>

              <div className="space-y-3">
                {detail.chapters.map((ch) => (
                  <div key={ch.id} className="rounded-md border border-[var(--border)] p-3">
                    <div className="mb-2 text-sm font-medium">{ch.sortOrder}. {ch.title}</div>
                    <ul className="space-y-1 text-sm text-[var(--muted)]">
                      {detail.lessons.filter((l) => l.chapterId === ch.id).map((l) => (
                        <li key={l.id}>
                          [{l.lessonType}] {l.title}
                          {l.contentUrl ? ` · ${l.contentUrl}` : ""}
                        </li>
                      ))}
                      {detail.lessons.filter((l) => l.chapterId === ch.id).length === 0 && (
                        <li>Chưa có bài.</li>
                      )}
                    </ul>
                  </div>
                ))}
                {detail.chapters.length === 0 && (
                  <p className="text-sm text-[var(--muted)]">Chưa có chương — thêm bên dưới.</p>
                )}
              </div>

              {canManage && (
                <div className="mt-4 grid gap-3 border-t border-[var(--border)] pt-4 md:grid-cols-2">
                  <form onSubmit={onAddChapter} className="space-y-2">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--muted)]">Thêm chương</div>
                    <input className={field} placeholder="Tiêu đề chương" value={chapterTitle} onChange={(e) => setChapterTitle(e.target.value)} required />
                    <button type="submit" className={btn.primary}>Thêm chương</button>
                  </form>
                  <form onSubmit={onAddLesson} className="space-y-2">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--muted)]">Thêm bài học</div>
                    <select className={field} value={lessonChapterId} onChange={(e) => setLessonChapterId(e.target.value)} required>
                      <option value="">— Chọn chương —</option>
                      {detail.chapters.map((ch) => (
                        <option key={ch.id} value={ch.id}>{ch.title}</option>
                      ))}
                    </select>
                    <input className={field} placeholder="Tiêu đề bài" value={lessonTitle} onChange={(e) => setLessonTitle(e.target.value)} required />
                    <select className={field} value={lessonType} onChange={(e) => setLessonType(e.target.value)}>
                      <option value="Text">Text</option>
                      <option value="Video">Video</option>
                      <option value="Document">Document</option>
                    </select>
                    {(lessonType === "Video" || lessonType === "Document") && (
                      <input className={field} placeholder="URL video / PDF" value={lessonUrl} onChange={(e) => setLessonUrl(e.target.value)} required />
                    )}
                    {lessonType === "Text" && (
                      <textarea className={field} rows={3} placeholder="Nội dung bài" value={lessonBody} onChange={(e) => setLessonBody(e.target.value)} />
                    )}
                    <button type="submit" className={btn.primary} disabled={!lessonChapterId}>Thêm bài</button>
                  </form>
                </div>
              )}
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
