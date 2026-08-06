"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import {
  addLmsClassSession,
  assignLmsMentor,
  closeLmsClass,
  enrollLmsClass,
  fetchLmsClassDetail,
  fetchLmsClasses,
  fetchLmsMentors,
  recordLmsAttendance,
  upsertLmsClass,
  type LmsClassDetailDto,
  type LmsMentorAssignmentDto,
  type LmsTrainingClassDto,
} from "@/shared/api/lms-api";
import { fetchEmployees, type EmployeeDto } from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

const STATUS_TONE: Record<string, "muted" | "brand" | "success" | "warning" | "danger"> = {
  Draft: "muted",
  Open: "brand",
  InProgress: "warning",
  Closed: "success",
};

function today() {
  return new Date().toISOString().slice(0, 10);
}

function fmtTime(t: string) {
  return t.length >= 5 ? t.slice(0, 5) : t;
}

export default function LmsClassesPage() {
  const { can } = usePermissions();
  const canRead = can("lms.class.read");
  const canManage = can("lms.class.manage");

  const [classes, setClasses] = useState<LmsTrainingClassDto[]>([]);
  const [selectedId, setSelectedId] = useState<string>("");
  const [detail, setDetail] = useState<LmsClassDetailDto | null>(null);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [mentors, setMentors] = useState<LmsMentorAssignmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("LOP-001");
  const [name, setName] = useState("");
  const [courseTitle, setCourseTitle] = useState("");
  const [instructorName, setInstructorName] = useState("");
  const [location, setLocation] = useState("");
  const [startDate, setStartDate] = useState(today);
  const [endDate, setEndDate] = useState(today);
  const [classStatus, setClassStatus] = useState("Draft");

  const [sessionDate, setSessionDate] = useState(today);
  const [sessionTopic, setSessionTopic] = useState("");
  const [sessionStart, setSessionStart] = useState("08:00");
  const [sessionEnd, setSessionEnd] = useState("12:00");

  const [enrollEmployeeId, setEnrollEmployeeId] = useState("");
  const [closeNote, setCloseNote] = useState("");

  const [menteeId, setMenteeId] = useState("");
  const [mentorId, setMentorId] = useState("");
  const [mentorNote, setMentorNote] = useState("");

  const selected = useMemo(
    () => classes.find((c) => c.id === selectedId) ?? null,
    [classes, selectedId],
  );

  const attendanceMap = useMemo(() => {
    const m = new Map<string, boolean>();
    detail?.attendance.forEach((a) => {
      m.set(`${a.sessionId}:${a.enrollmentId}`, a.present);
    });
    return m;
  }, [detail]);

  const loadList = useCallback(async () => {
    const [list, emps, m] = await Promise.all([
      fetchLmsClasses(),
      fetchEmployees(),
      fetchLmsMentors(),
    ]);
    setClasses(list);
    setEmployees(emps);
    setMentors(m);
    if (!selectedId && list[0]) setSelectedId(list[0].id);
    if (!enrollEmployeeId && emps[0]) setEnrollEmployeeId(emps[0].id);
    if (!menteeId && emps[0]) setMenteeId(emps[0].id);
    if (!mentorId && emps[1]) setMentorId(emps[1]?.id ?? emps[0].id);
  }, [selectedId, enrollEmployeeId, menteeId, mentorId]);

  const loadDetail = useCallback(async (id: string) => {
    if (!id) {
      setDetail(null);
      return;
    }
    const d = await fetchLmsClassDetail(id);
    setDetail(d);
  }, []);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      await loadList();
    } catch {
      setError("Không tải được danh sách lớp.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  useEffect(() => {
    if (!canRead || !selectedId) return;
    void loadDetail(selectedId).catch(() => setError("Không tải chi tiết lớp."));
  }, [canRead, selectedId, loadDetail]);

  async function onCreateClass(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      const created = await upsertLmsClass({
        code,
        name,
        courseTitle,
        instructorName: instructorName.trim() || undefined,
        location: location.trim() || undefined,
        startDate,
        endDate,
        status: classStatus,
      });
      setOk("Đã tạo lớp đào tạo.");
      setSelectedId(created.id);
      await loadList();
      await loadDetail(created.id);
    } catch {
      setError("Tạo lớp thất bại (kiểm tra mã trùng).");
    }
  }

  async function onAddSession(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !selectedId) return;
    setError(null);
    setOk(null);
    try {
      await addLmsClassSession(selectedId, {
        sessionDate,
        topic: sessionTopic,
        startTime: sessionStart.length === 5 ? `${sessionStart}:00` : sessionStart,
        endTime: sessionEnd.length === 5 ? `${sessionEnd}:00` : sessionEnd,
      });
      setOk("Đã thêm buổi học.");
      setSessionTopic("");
      await loadList();
      await loadDetail(selectedId);
    } catch {
      setError("Thêm buổi học thất bại.");
    }
  }

  async function onEnroll(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !selectedId) return;
    setError(null);
    setOk(null);
    try {
      await enrollLmsClass(selectedId, enrollEmployeeId);
      setOk("Đã ghi danh học viên.");
      await loadList();
      await loadDetail(selectedId);
    } catch {
      setError("Ghi danh thất bại (có thể đã ghi danh).");
    }
  }

  async function onToggleAttendance(sessionId: string, enrollmentId: string, present: boolean) {
    if (!canManage) return;
    setError(null);
    try {
      await recordLmsAttendance(sessionId, { enrollmentId, present });
      await loadDetail(selectedId);
    } catch {
      setError("Cập nhật điểm danh thất bại.");
    }
  }

  async function onCloseClass() {
    if (!canManage || !selectedId) return;
    if (!window.confirm("Đóng lớp và tổng kết? Học viên đang học sẽ chuyển Completed.")) return;
    setError(null);
    setOk(null);
    try {
      await closeLmsClass(selectedId, closeNote.trim() || undefined);
      setOk("Đã đóng lớp.");
      await loadList();
      await loadDetail(selectedId);
    } catch {
      setError("Đóng lớp thất bại.");
    }
  }

  async function onAssignMentor(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      await assignLmsMentor({
        menteeEmployeeId: menteeId,
        mentorEmployeeId: mentorId,
        note: mentorNote.trim() || undefined,
      });
      setOk("Đã gán mentor.");
      setMentorNote("");
      setMentors(await fetchLmsMentors());
    } catch {
      setError("Gán mentor thất bại.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền lms.class.read</p>;
  }

  const isClosed = selected?.status === "Closed";

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Lớp đào tạo offline</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Mở lớp · lịch buổi học · ghi danh · điểm danh · đóng lớp · gán mentor
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <div className="grid gap-6 xl:grid-cols-[320px_1fr]">
        <aside className="space-y-4">
          <section className={panel}>
            <h2 className="text-lead font-bold">Danh sách lớp</h2>
            {loading ? (
              <p className="mt-2 text-body text-muted-foreground">Đang tải…</p>
            ) : classes.length === 0 ? (
              <p className="mt-2 text-body text-muted-foreground">Chưa có lớp nào.</p>
            ) : (
              <ul className="mt-3 space-y-1">
                {classes.map((c) => (
                  <li key={c.id}>
                    <button
                      type="button"
                      className={`w-full rounded-lg px-3 py-2 text-left text-body transition-colors ${
                        c.id === selectedId
                          ? "bg-brand-muted font-semibold text-brand-strong"
                          : "hover:bg-muted"
                      }`}
                      onClick={() => setSelectedId(c.id)}
                    >
                      <span className="block">{c.code} — {c.name}</span>
                      <span className="mt-0.5 flex items-center gap-2 text-meta text-muted-foreground">
                        <span className={statusPill(STATUS_TONE[c.status] ?? "muted")}>{c.status}</span>
                        {c.sessionCount} buổi · {c.enrollmentCount} HV
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </section>

          {canManage && (
            <form onSubmit={(e) => void onCreateClass(e)} className={`${panel} space-y-3`}>
              <h2 className="text-lead font-bold">Mở lớp mới</h2>
              <label className="block">
                <span className={field.label}>Mã lớp</span>
                <input className={field.input} value={code} onChange={(e) => setCode(e.target.value)} required />
              </label>
              <label className="block">
                <span className={field.label}>Tên lớp</span>
                <input className={field.input} value={name} onChange={(e) => setName(e.target.value)} required />
              </label>
              <label className="block">
                <span className={field.label}>Khóa / chương trình</span>
                <input
                  className={field.input}
                  value={courseTitle}
                  onChange={(e) => setCourseTitle(e.target.value)}
                  required
                />
              </label>
              <label className="block">
                <span className={field.label}>Giảng viên</span>
                <input
                  className={field.input}
                  value={instructorName}
                  onChange={(e) => setInstructorName(e.target.value)}
                />
              </label>
              <label className="block">
                <span className={field.label}>Địa điểm</span>
                <input className={field.input} value={location} onChange={(e) => setLocation(e.target.value)} />
              </label>
              <div className="grid grid-cols-2 gap-2">
                <label className="block">
                  <span className={field.label}>Từ ngày</span>
                  <input
                    type="date"
                    className={field.input}
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    required
                  />
                </label>
                <label className="block">
                  <span className={field.label}>Đến ngày</span>
                  <input
                    type="date"
                    className={field.input}
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    required
                  />
                </label>
              </div>
              <label className="block">
                <span className={field.label}>Trạng thái</span>
                <select
                  className={field.select}
                  value={classStatus}
                  onChange={(e) => setClassStatus(e.target.value)}
                >
                  <option value="Draft">Draft</option>
                  <option value="Open">Open</option>
                </select>
              </label>
              <button type="submit" className={btn.primary}>
                Tạo lớp
              </button>
            </form>
          )}
        </aside>

        <main className="space-y-4">
          {!selected ? (
            <p className={panel}>Chọn hoặc tạo lớp để xem chi tiết.</p>
          ) : (
            <>
              <section className={panel}>
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h2 className="text-lead font-bold">
                      {selected.code} — {selected.name}
                    </h2>
                    <p className="mt-1 text-body text-muted-foreground">{selected.courseTitle}</p>
                    <p className="mt-1 text-meta text-muted-foreground">
                      {selected.instructorName && <>GV: {selected.instructorName} · </>}
                      {selected.location && <>Địa điểm: {selected.location} · </>}
                      {selected.startDate} → {selected.endDate}
                    </p>
                    <span className={`mt-2 inline-block ${statusPill(STATUS_TONE[selected.status] ?? "muted")}`}>
                      {selected.status}
                    </span>
                    {selected.summaryNote && (
                      <p className="mt-2 text-body text-muted-foreground">Tổng kết: {selected.summaryNote}</p>
                    )}
                  </div>
                  {canManage && !isClosed && (
                    <div className="space-y-2">
                      <label className="block">
                        <span className={field.label}>Ghi chú đóng lớp</span>
                        <textarea
                          className={field.textarea}
                          value={closeNote}
                          onChange={(e) => setCloseNote(e.target.value)}
                          placeholder="Tổng kết khóa học…"
                        />
                      </label>
                      <button type="button" className={btn.danger} onClick={() => void onCloseClass()}>
                        Đóng lớp & tổng kết
                      </button>
                    </div>
                  )}
                </div>
              </section>

              <div className="grid gap-4 lg:grid-cols-2">
                <section className={panel}>
                  <h3 className="text-lead font-bold">Buổi học ({detail?.sessions.length ?? 0})</h3>
                  {canManage && !isClosed && (
                    <form onSubmit={(e) => void onAddSession(e)} className="mt-3 space-y-2 border-b border-border pb-3">
                      <label className="block">
                        <span className={field.label}>Ngày</span>
                        <input
                          type="date"
                          className={field.input}
                          value={sessionDate}
                          onChange={(e) => setSessionDate(e.target.value)}
                          required
                        />
                      </label>
                      <label className="block">
                        <span className={field.label}>Chủ đề</span>
                        <input
                          className={field.input}
                          value={sessionTopic}
                          onChange={(e) => setSessionTopic(e.target.value)}
                          required
                        />
                      </label>
                      <div className="grid grid-cols-2 gap-2">
                        <label className="block">
                          <span className={field.label}>Bắt đầu</span>
                          <input
                            type="time"
                            className={field.input}
                            value={sessionStart}
                            onChange={(e) => setSessionStart(e.target.value)}
                            required
                          />
                        </label>
                        <label className="block">
                          <span className={field.label}>Kết thúc</span>
                          <input
                            type="time"
                            className={field.input}
                            value={sessionEnd}
                            onChange={(e) => setSessionEnd(e.target.value)}
                            required
                          />
                        </label>
                      </div>
                      <button type="submit" className={btn.secondary}>
                        Thêm buổi
                      </button>
                    </form>
                  )}
                  <ul className="mt-3 space-y-2">
                    {(detail?.sessions ?? []).map((s) => (
                      <li key={s.id} className="rounded-lg border border-border px-3 py-2 text-body">
                        <span className="font-semibold">{s.sessionDate}</span> · {s.topic}
                        <span className="ml-2 text-meta text-muted-foreground">
                          {fmtTime(s.startTime)}–{fmtTime(s.endTime)}
                        </span>
                      </li>
                    ))}
                  </ul>
                </section>

                <section className={panel}>
                  <h3 className="text-lead font-bold">Học viên ({detail?.enrollments.length ?? 0})</h3>
                  {canManage && !isClosed && (
                    <form onSubmit={(e) => void onEnroll(e)} className="mt-3 flex gap-2">
                      <select
                        className={`${field.select} flex-1`}
                        value={enrollEmployeeId}
                        onChange={(e) => setEnrollEmployeeId(e.target.value)}
                        required
                      >
                        {employees.map((e) => (
                          <option key={e.id} value={e.id}>
                            {e.employeeCode} — {e.fullName}
                          </option>
                        ))}
                      </select>
                      <button type="submit" className={btn.secondary}>
                        Ghi danh
                      </button>
                    </form>
                  )}
                  <ul className="mt-3 space-y-1">
                    {(detail?.enrollments ?? []).map((e) => (
                      <li key={e.id} className="flex items-center justify-between text-body">
                        <span>
                          {e.employeeCode} — {e.employeeName}
                        </span>
                        <span className={statusPill(e.status === "Completed" ? "success" : "brand")}>
                          {e.status}
                        </span>
                      </li>
                    ))}
                  </ul>
                </section>
              </div>

              {(detail?.sessions.length ?? 0) > 0 && (detail?.enrollments.length ?? 0) > 0 && (
                <section className={panel}>
                  <h3 className="text-lead font-bold">Điểm danh</h3>
                  <div className={tableWrap}>
                    <table className="min-w-full text-body">
                      <thead className="border-b border-border bg-muted/40">
                        <tr>
                          <th className={th}>Học viên</th>
                          {(detail?.sessions ?? []).map((s) => (
                            <th key={s.id} className={th}>
                              {s.sessionDate}
                              <br />
                              <span className="font-normal">{s.topic.slice(0, 20)}</span>
                            </th>
                          ))}
                        </tr>
                      </thead>
                      <tbody>
                        {(detail?.enrollments ?? []).map((e) => (
                          <tr key={e.id} className="border-b border-border/60">
                            <td className={td}>
                              {e.employeeCode}
                              <br />
                              <span className="text-meta text-muted-foreground">{e.employeeName}</span>
                            </td>
                            {(detail?.sessions ?? []).map((s) => {
                              const key = `${s.id}:${e.id}`;
                              const present = attendanceMap.get(key) ?? false;
                              return (
                                <td key={s.id} className={`${td} text-center`}>
                                  {canManage && !isClosed ? (
                                    <input
                                      type="checkbox"
                                      className={field.check}
                                      checked={present}
                                      onChange={(ev) =>
                                        void onToggleAttendance(s.id, e.id, ev.target.checked)
                                      }
                                    />
                                  ) : present ? (
                                    <span className={statusPill("success")}>Có</span>
                                  ) : (
                                    <span className={statusPill("muted")}>—</span>
                                  )}
                                </td>
                              );
                            })}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </section>
              )}
            </>
          )}

          <section className={panel}>
            <h3 className="text-lead font-bold">Gán mentor</h3>
            {canManage && (
              <form onSubmit={(e) => void onAssignMentor(e)} className="mt-3 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
                <label className="block">
                  <span className={field.label}>Học viên (mentee)</span>
                  <select className={field.select} value={menteeId} onChange={(e) => setMenteeId(e.target.value)}>
                    {employees.map((e) => (
                      <option key={e.id} value={e.id}>
                        {e.employeeCode} — {e.fullName}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="block">
                  <span className={field.label}>Mentor</span>
                  <select className={field.select} value={mentorId} onChange={(e) => setMentorId(e.target.value)}>
                    {employees.map((e) => (
                      <option key={e.id} value={e.id}>
                        {e.employeeCode} — {e.fullName}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="block sm:col-span-2">
                  <span className={field.label}>Ghi chú</span>
                  <input
                    className={field.input}
                    value={mentorNote}
                    onChange={(e) => setMentorNote(e.target.value)}
                  />
                </label>
                <button type="submit" className={`${btn.primary} sm:col-span-2 lg:col-span-4 lg:w-fit`}>
                  Gán mentor
                </button>
              </form>
            )}
            <div className={`${tableWrap} mt-4`}>
              <table className="min-w-full">
                <thead className="border-b border-border bg-muted/40">
                  <tr>
                    <th className={th}>Mentee</th>
                    <th className={th}>Mentor</th>
                    <th className={th}>Ghi chú</th>
                  </tr>
                </thead>
                <tbody>
                  {mentors.length === 0 ? (
                    <tr>
                      <td colSpan={3} className={`${td} text-muted-foreground`}>
                        Chưa có gán mentor.
                      </td>
                    </tr>
                  ) : (
                    mentors.map((m) => (
                      <tr key={m.id} className="border-b border-border/60">
                        <td className={td}>
                          {m.menteeCode} — {m.menteeName}
                        </td>
                        <td className={td}>
                          {m.mentorCode} — {m.mentorName}
                        </td>
                        <td className={td}>{m.note ?? "—"}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </section>
        </main>
      </div>
    </div>
  );
}
