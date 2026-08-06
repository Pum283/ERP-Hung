"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchLmsClasses, fetchLmsCourses, type LmsCourseDto, type LmsTrainingClassDto } from "@/shared/api/lms-api";
import {
  downloadLmsReportCsv,
  fetchLmsCompletionByOrg,
  fetchLmsDashboard,
  fetchLmsInstructors,
  fetchLmsLearners,
  type LmsCompletionByOrgRowDto,
  type LmsDashboardDto,
  type LmsInstructorDto,
  type LmsLearnerRowDto,
} from "@/shared/api/lms-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

type Tab = "dashboard" | "by-org" | "learners";

export default function LmsReportsPage() {
  const { can } = usePermissions();
  const canRead = can("lms.report.read");

  const [tab, setTab] = useState<Tab>("dashboard");
  const [dashboard, setDashboard] = useState<LmsDashboardDto | null>(null);
  const [orgRows, setOrgRows] = useState<LmsCompletionByOrgRowDto[]>([]);
  const [learners, setLearners] = useState<LmsLearnerRowDto[]>([]);
  const [classes, setClasses] = useState<LmsTrainingClassDto[]>([]);
  const [courses, setCourses] = useState<LmsCourseDto[]>([]);
  const [instructors, setInstructors] = useState<LmsInstructorDto[]>([]);
  const [classId, setClassId] = useState("");
  const [courseId, setCourseId] = useState("");
  const [instructorId, setInstructorId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (tab === "dashboard") setDashboard(await fetchLmsDashboard());
    else if (tab === "by-org") setOrgRows(await fetchLmsCompletionByOrg());
    else setLearners(await fetchLmsLearners({
      ...(classId ? { classId } : {}),
      ...(courseId ? { courseId } : {}),
      ...(instructorId ? { instructorId } : {}),
    }));
  }, [tab, classId, courseId, instructorId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    Promise.all([
      fetchLmsClasses().catch(() => [] as LmsTrainingClassDto[]),
      fetchLmsCourses().catch(() => [] as LmsCourseDto[]),
      fetchLmsInstructors().catch(() => [] as LmsInstructorDto[]),
      load(),
    ])
      .then(([c, coursesList, instr]) => {
        setClasses(c);
        setCourses(coursesList);
        setInstructors(instr);
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      await downloadLmsReportCsv({
        report: tab === "by-org" ? "by-org" : tab === "learners" ? "learners" : "dashboard",
        ...(classId ? { classId } : {}),
        ...(courseId ? { courseId } : {}),
        ...(instructorId ? { instructorId } : {}),
      });
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo đào tạo.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo đào tạo</h1>
          <p className="text-sm text-[var(--muted)]">UC_LMS_051 · 065 · 066 · 070 · dashboard · theo đơn vị · học viên · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["dashboard", "Dashboard"],
            ["by-org", "Theo đơn vị"],
            ["learners", "Học viên"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
          <button type="button" className={btn.soft} onClick={() => void exportCsv()}>Xuất CSV</button>
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}

      {tab === "learners" && (
        <div className={`${panel} flex flex-wrap gap-3`}>
          <label className={field.label}>
            Lớp
            <select className={field.input} value={classId} onChange={(e) => setClassId(e.target.value)}>
              <option value="">Tất cả offline</option>
              {classes.map((c) => <option key={c.id} value={c.id}>{c.code} · {c.name}</option>)}
            </select>
          </label>
          <label className={field.label}>
            Khóa online
            <select className={field.input} value={courseId} onChange={(e) => setCourseId(e.target.value)}>
              <option value="">Tất cả</option>
              {courses.map((c) => <option key={c.id} value={c.id}>{c.code} · {c.name}</option>)}
            </select>
          </label>
          <label className={field.label}>
            GV
            <select className={field.input} value={instructorId} onChange={(e) => setInstructorId(e.target.value)}>
              <option value="">Tất cả</option>
              {instructors.map((i) => <option key={i.id} value={i.id}>{i.code} · {i.displayName}</option>)}
            </select>
          </label>
          <button type="button" className={btn.primary} disabled={loading} onClick={() => void load().catch((e: Error) => setError(e.message))}>
            Làm mới
          </button>
        </div>
      )}

      {tab === "dashboard" && dashboard && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {[
            ["Khóa học", String(dashboard.courseCount)],
            ["Đã publish", String(dashboard.publishedCourseCount)],
            ["Lớp mở", String(dashboard.openClassCount)],
            ["Lớp đóng", String(dashboard.closedClassCount)],
            ["Ghi danh offline", `${dashboard.offlineCompletedCount}/${dashboard.offlineEnrollmentCount}`],
            ["Ghi danh online", `${dashboard.onlineCompletedCount}/${dashboard.onlineEnrollmentCount}`],
            ["Chứng chỉ", String(dashboard.activeCertificateCount)],
            ["Giảng viên", String(dashboard.instructorCount)],
            ["Tiến độ online TB %", String(dashboard.avgOnlineProgressPercent)],
            ["Tỷ lệ đạt thi %", String(dashboard.examPassRatePercent)],
          ].map(([label, val]) => (
            <div key={label} className={panel}>
              <div className="text-xs text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </div>
      )}

      <div className={tableWrap}>
        {tab === "by-org" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Đơn vị</th><th className={th}>Offline</th><th className={th}>Online</th>
                <th className={th}>Hoàn thành %</th>
              </tr>
            </thead>
            <tbody>
              {orgRows.map((r) => (
                <tr key={r.orgUnitId ?? "none"}>
                  <td className={td}>{r.orgUnitCode} · {r.orgUnitName}</td>
                  <td className={td}>{r.offlineCompleted}/{r.offlineTotal}</td>
                  <td className={td}>{r.onlineCompleted}/{r.onlineTotal}</td>
                  <td className={td}>{r.completionRatePercent}%</td>
                </tr>
              ))}
              {!loading && orgRows.length === 0 && <tr><td className={td} colSpan={4}>Không có dữ liệu.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "learners" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Nguồn</th><th className={th}>Lớp / khóa</th><th className={th}>Học viên</th>
                <th className={th}>Đơn vị</th><th className={th}>TT</th><th className={th}>Tiến độ %</th>
              </tr>
            </thead>
            <tbody>
              {learners.map((r, i) => (
                <tr key={`${r.source}-${r.learnerCode}-${i}`}>
                  <td className={td}>{r.source}</td>
                  <td className={td}>{r.classCode ?? r.courseCode} · {r.className ?? r.courseName}</td>
                  <td className={td}>{r.learnerCode} · {r.learnerName}</td>
                  <td className={td}>{r.orgUnitName ?? "—"}</td>
                  <td className={td}>{r.status}</td>
                  <td className={td}>{r.progressPercent}{r.presentSessions != null ? ` (${r.presentSessions}/${r.totalSessions})` : ""}</td>
                </tr>
              ))}
              {!loading && learners.length === 0 && <tr><td className={td} colSpan={6}>Không có học viên.</td></tr>}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
