"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchEmployees, type EmployeeDto } from "@/shared/api/hrm-api";
import {
  grantLmsInstructorRole,
  fetchLmsInstructors,
  setLmsInstructorStatus,
  upsertLmsInstructor,
  type LmsInstructorDto,
} from "@/shared/api/lms-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function LmsInstructorsPage() {
  const { can } = usePermissions();
  const canRead = can("lms.instructor.read");
  const canManage = can("lms.instructor.manage");

  const [rows, setRows] = useState<LmsInstructorDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [editId, setEditId] = useState<string | null>(null);
  const [code, setCode] = useState("GV-001");
  const [displayName, setDisplayName] = useState("");
  const [employeeId, setEmployeeId] = useState("");
  const [title, setTitle] = useState("");
  const [specialty, setSpecialty] = useState("");
  const [bio, setBio] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [grantRole, setGrantRole] = useState(true);

  const load = useCallback(async () => {
    const [list, emps] = await Promise.all([
      fetchLmsInstructors(),
      fetchEmployees().catch(() => [] as EmployeeDto[]),
    ]);
    setRows(list);
    setEmployees(emps);
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function pickEdit(r: LmsInstructorDto) {
    setEditId(r.id);
    setCode(r.code);
    setDisplayName(r.displayName);
    setEmployeeId(r.employeeId ?? "");
    setTitle(r.title ?? "");
    setSpecialty(r.specialty ?? "");
    setBio(r.bio ?? "");
    setEmail(r.email ?? "");
    setPhone(r.phone ?? "");
    setGrantRole(false);
  }

  function resetForm() {
    setEditId(null);
    setCode(`GV-${String(rows.length + 1).padStart(3, "0")}`);
    setDisplayName("");
    setEmployeeId("");
    setTitle("");
    setSpecialty("");
    setBio("");
    setEmail("");
    setPhone("");
    setGrantRole(true);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    try {
      setError(null);
      await upsertLmsInstructor({
        id: editId ?? undefined,
        code,
        displayName,
        employeeId: employeeId || null,
        title: title || undefined,
        specialty: specialty || undefined,
        bio: bio || undefined,
        email: email || undefined,
        phone: phone || undefined,
        grantInstructorRole: grantRole,
      });
      setOk(editId ? "Đã cập nhật giảng viên." : "Đã tạo giảng viên.");
      resetForm();
      await load();
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem giảng viên.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Giảng viên LMS</h1>
        <p className="text-sm text-[var(--muted)]">UC_LMS_049–050 · hồ sơ · gán role LMS_INSTRUCTOR.</p>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-800">{ok}</div>}

      {canManage && (
        <form className={`${panel} grid gap-3 md:grid-cols-3`} onSubmit={(e) => void onSubmit(e)}>
          <label className={field.label}>Mã<input className={field.input} value={code} onChange={(e) => setCode(e.target.value)} required /></label>
          <label className={field.label}>Họ tên<input className={field.input} value={displayName} onChange={(e) => setDisplayName(e.target.value)} required /></label>
          <label className={field.label}>
            Nhân viên HRM
            <select className={field.input} value={employeeId} onChange={(e) => {
              const id = e.target.value;
              setEmployeeId(id);
              const emp = employees.find((x) => x.id === id);
              if (emp) {
                if (!displayName) setDisplayName(emp.fullName);
                if (!email) setEmail(emp.email ?? "");
                if (!phone) setPhone(emp.phone ?? "");
              }
            }}>
              <option value="">—</option>
              {employees.map((emp) => <option key={emp.id} value={emp.id}>{emp.employeeCode} · {emp.fullName}</option>)}
            </select>
          </label>
          <label className={field.label}>Chức danh<input className={field.input} value={title} onChange={(e) => setTitle(e.target.value)} /></label>
          <label className={field.label}>Chuyên môn<input className={field.input} value={specialty} onChange={(e) => setSpecialty(e.target.value)} /></label>
          <label className={field.label}>Email<input className={field.input} value={email} onChange={(e) => setEmail(e.target.value)} /></label>
          <label className={field.label}>Điện thoại<input className={field.input} value={phone} onChange={(e) => setPhone(e.target.value)} /></label>
          <label className={`${field.label} md:col-span-2`}>Bio<textarea className={field.input} rows={2} value={bio} onChange={(e) => setBio(e.target.value)} /></label>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={grantRole} onChange={(e) => setGrantRole(e.target.checked)} />
            Gán role LMS_INSTRUCTOR (cần User liên kết NV)
          </label>
          <div className="flex gap-2 md:col-span-3">
            <button type="submit" className={btn.primary}>{editId ? "Cập nhật" : "Tạo mới"}</button>
            {editId && <button type="button" className={btn.ghost} onClick={resetForm}>Hủy</button>}
          </div>
        </form>
      )}

      <div className={tableWrap}>
        <table className="min-w-full text-sm">
          <thead>
            <tr>
              <th className={th}>Mã</th><th className={th}>Tên</th><th className={th}>NV</th>
              <th className={th}>Chuyên môn</th><th className={th}>Lớp</th><th className={th}>Role</th>
              <th className={th}>TT</th><th className={th} />
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.id}>
                <td className={td}>{r.code}</td>
                <td className={td}>{r.displayName}{r.title ? ` · ${r.title}` : ""}</td>
                <td className={td}>{r.employeeCode ? `${r.employeeCode} · ${r.employeeName}` : "—"}</td>
                <td className={td}>{r.specialty ?? "—"}</td>
                <td className={td}>{r.classCount}</td>
                <td className={td}>{r.roleGranted ? "Đã gán" : "—"}</td>
                <td className={td}><span className={statusPill(r.status === "Active" ? "success" : "muted")}>{r.status}</span></td>
                <td className={td}>
                  {canManage && (
                    <div className="flex flex-wrap gap-1">
                      <button type="button" className={btn.ghost} onClick={() => pickEdit(r)}>Sửa</button>
                      {!r.roleGranted && (
                        <button type="button" className={btn.soft} onClick={() => void grantLmsInstructorRole(r.id).then(load).catch((e: Error) => setError(e.message))}>
                          Gán role
                        </button>
                      )}
                      <button type="button" className={btn.ghost} onClick={() => void setLmsInstructorStatus(r.id, r.status === "Active" ? "Inactive" : "Active").then(load).catch((e: Error) => setError(e.message))}>
                        {r.status === "Active" ? "Ngưng" : "Active"}
                      </button>
                    </div>
                  )}
                </td>
              </tr>
            ))}
            {!loading && rows.length === 0 && <tr><td className={td} colSpan={8}>Chưa có giảng viên.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  );
}
