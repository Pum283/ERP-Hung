"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  addOnboardingDocument,
  assignOnboardingMentor,
  convertOnboardingOfficial,
  evaluateOnboardingTrial,
  fetchCandidates,
  fetchEmployees,
  fetchOnboardingCases,
  fetchOnboardingSettings,
  fetchTrialExpiring,
  hireFromCandidate,
  updateOnboardingChecklist,
  uploadHrmFile,
  upsertOnboardingSettings,
  type CandidateDto,
  type EmployeeDto,
  type OnboardingCaseDto,
  type OnboardingSettingDto,
  type TrialExpiringDto,
} from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { validateOnboardingSettingsForm } from "@/shared/api/hrm-recruit-step20-helpers";
import {
  calculateChecklistProgress,
  validateHireRequest,
  validateMentorAssignment,
  validateOnboardingDocument,
} from "@/shared/api/hrm-recruit-step21-helpers";
import {
  validateConvertOfficial,
  validateTrialEvaluation,
} from "@/shared/api/hrm-step22-helpers";

type MentorModalState = {
  caseId: string;
  employeeName: string;
  employeeId: string;
  currentMentorId?: string | null;
} | null;

export default function OnboardingPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [settings, setSettings] = useState<OnboardingSettingDto>({ onboardingDays: 30, trialDays: 60 });
  const [cases, setCases] = useState<OnboardingCaseDto[]>([]);
  const [expiring, setExpiring] = useState<TrialExpiringDto[]>([]);
  const [accepted, setAccepted] = useState<CandidateDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [candidateId, setCandidateId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  // Mentor modal state
  const [mentorModal, setMentorModal] = useState<MentorModalState>(null);
  const [selectedMentorId, setSelectedMentorId] = useState<string>("");
  const [mentorSubmitting, setMentorSubmitting] = useState(false);

  const hireable = useMemo(
    () => accepted.filter((c) => c.pipelineStatus === "Accepted" && !c.convertedEmployeeId),
    [accepted],
  );

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [s, c, e, cand, emp] = await Promise.all([
        fetchOnboardingSettings(),
        fetchOnboardingCases(),
        fetchTrialExpiring(14),
        fetchCandidates(),
        fetchEmployees(),
      ]);
      setSettings(s);
      setCases(c);
      setExpiring(e);
      setAccepted(cand);
      setEmployees(emp);
      if (!candidateId && cand.find((x) => x.pipelineStatus === "Accepted" && !x.convertedEmployeeId)) {
        setCandidateId(cand.find((x) => x.pipelineStatus === "Accepted" && !x.convertedEmployeeId)!.id);
      }
    } catch {
      setError("Không tải được dữ liệu onboarding.");
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
    if (!ok) return;
    const t = setTimeout(() => setOk(null), 4000);
    return () => clearTimeout(t);
  }, [ok]);

  async function onSaveSettings(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    const v = validateOnboardingSettingsForm(settings);
    if (!v.valid) { setError(v.error ?? "Lỗi validation."); return; }
    setError(null);
    try {
      setSettings(await upsertOnboardingSettings(settings));
      setOk("✅ Đã lưu cấu hình thời hạn Onboarding & Thử việc.");
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Lưu cấu hình thất bại.");
    }
  }

  async function onHireCandidate(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    const v = validateHireRequest({ candidateId });
    if (!v.valid) { setError(v.error ?? "Vui lòng chọn ứng viên."); return; }
    setError(null);
    try {
      await hireFromCandidate(candidateId);
      setOk("🎉 Đã tạo hồ sơ NV mới (Probation) & Hồ sơ Onboarding.");
      await load();
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Tạo hồ sơ NV thất bại.");
    }
  }

  async function onAssignMentorSubmit(e: FormEvent) {
    e.preventDefault();
    if (!mentorModal) return;
    const v = validateMentorAssignment(selectedMentorId, mentorModal.employeeId);
    if (!v.valid) { setError(v.error ?? "Lỗi chọn người hướng dẫn."); return; }
    setMentorSubmitting(true);
    setError(null);
    try {
      await assignOnboardingMentor(mentorModal.caseId, selectedMentorId);
      setOk("✅ Đã gán người hướng dẫn onboarding.");
      setMentorModal(null);
      await load();
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Gán mentor thất bại.");
    } finally {
      setMentorSubmitting(false);
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:1111";

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Onboarding nhân viên mới</h1>
        <p className="mt-1 text-body text-muted-foreground">
          UC_HRM_066–074 · Tiếp nhận NV mới từ ứng viên Accepted · Gán Mentor · Checklist & Tiến độ · Thử việc & Chuyển chính thức
        </p>
      </div>

      {/* Toasts */}
      {error && (
        <div className="flex items-center gap-2 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-body text-destructive">
          <span>⚠️</span>
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="ml-auto text-meta opacity-70 hover:opacity-100">✕</button>
        </div>
      )}
      {ok && (
        <div className="flex items-center gap-2 rounded-xl border border-green-300 bg-green-50 px-4 py-3 text-body text-green-700">
          <span>{ok}</span>
          <button type="button" onClick={() => setOk(null)} className="ml-auto text-meta opacity-70 hover:opacity-100">✕</button>
        </div>
      )}

      {/* Mentor Assignment Modal */}
      {mentorModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-6 shadow-xl">
            <h3 className="text-lead font-bold text-foreground">Gán người hướng dẫn (Mentor)</h3>
            <p className="mt-1 text-meta text-muted-foreground">Nhân viên mới: {mentorModal.employeeName}</p>
            <form onSubmit={(e) => void onAssignMentorSubmit(e)} className="mt-4 space-y-4">
              <label className="block space-y-1 text-body">
                <span className="text-muted-foreground">Chọn người hướng dẫn *</span>
                <select
                  value={selectedMentorId}
                  onChange={(e) => setSelectedMentorId(e.target.value)}
                  className="h-10 w-full rounded-lg border border-border bg-background px-3 text-body"
                >
                  <option value="">-- Chọn nhân viên kinh nghiệm --</option>
                  {employees
                    .filter((e) => e.id !== mentorModal.employeeId)
                    .map((emp) => (
                      <option key={emp.id} value={emp.id}>
                        {emp.employeeCode} · {emp.fullName} ({emp.orgUnitName ?? "Đơn vị"})
                      </option>
                    ))}
                </select>
              </label>
              <div className="flex gap-2 pt-2">
                <button type="button" onClick={() => setMentorModal(null)} className={btn.secondary + " flex-1"}>Hủy</button>
                <button type="submit" disabled={mentorSubmitting || !selectedMentorId} className={btn.primary + " flex-1"}>
                  {mentorSubmitting ? "Đang lưu…" : "Xác nhận gán Mentor"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Top Section: Form Cấu hình & Form Tiếp nhận */}
      <section className="grid gap-4 lg:grid-cols-2">
        {/* Form Cấu hình thời hạn */}
        <form
          onSubmit={(e) => void onSaveSettings(e)}
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
        >
          <h2 className="text-lead font-bold">Cấu hình thời hạn (UC_HRM_066 & 067)</h2>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Onboarding (1–365 ngày)</span>
            <input
              type="number"
              min={1}
              max={365}
              value={settings.onboardingDays}
              disabled={!canManage}
              onChange={(e) => setSettings((s) => ({ ...s, onboardingDays: Number(e.target.value) }))}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            />
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Thử việc (1–365 ngày)</span>
            <input
              type="number"
              min={1}
              max={365}
              value={settings.trialDays}
              disabled={!canManage}
              onChange={(e) => setSettings((s) => ({ ...s, trialDays: Number(e.target.value) }))}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            />
          </label>
          {canManage && (
            <button type="submit" className={btn.primary}>
              Lưu cấu hình
            </button>
          )}
        </form>

        {/* Form Tiếp nhận nhân viên từ ứng viên Accepted */}
        {canManage && (
          <form
            onSubmit={(e) => void onHireCandidate(e)}
            className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
          >
            <h2 className="text-lead font-bold">Tiếp nhận NV mới từ UV Accepted (UC_HRM_068)</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Ứng viên trúng tuyển (Accepted)</span>
              <select
                value={candidateId}
                onChange={(e) => setCandidateId(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2 text-body"
              >
                {hireable.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.fullName} · {c.jobPostingTitle}
                  </option>
                ))}
              </select>
            </label>
            <button
              type="submit"
              className={btn.primary}
              disabled={!hireable.length}
            >
              🎉 Tạo hồ sơ NV (Probation) + Onboarding
            </button>
            {!hireable.length && (
              <p className="text-meta text-muted-foreground">
                Chưa có ứng viên ở trạng thái Accepted (chưa convert).
              </p>
            )}
          </form>
        )}
      </section>

      {/* Bảng cảnh báo thử việc sắp hết hạn */}
      <section className="space-y-2">
        <h2 className="text-lead font-bold">Cảnh báo sắp hết hạn thử việc (14 ngày)</h2>
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Mã NV</th>
                <th className="px-3 py-2">Họ tên</th>
                <th className="px-3 py-2">Hết hạn thử việc</th>
                <th className="px-3 py-2">Thời gian còn lại</th>
              </tr>
            </thead>
            <tbody>
              {expiring.map((x) => (
                <tr key={x.onboardingCaseId} className="border-t border-border hover:bg-muted/20 transition-colors">
                  <td className="px-3 py-2 font-medium">{x.employeeCode}</td>
                  <td className="px-3 py-2">{x.fullName}</td>
                  <td className="px-3 py-2">{x.trialEndDate}</td>
                  <td className="px-3 py-2 text-destructive font-bold">{x.daysLeft} ngày</td>
                </tr>
              ))}
              {!loading && expiring.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-4 text-center text-muted-foreground">
                    Không có cảnh báo thử việc hết hạn trong 14 ngày tới.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      {/* Danh sách Hồ sơ onboarding */}
      <section className="space-y-3">
        <h2 className="text-lead font-bold">Danh sách hồ sơ onboarding (UC_HRM_069, 070, 071)</h2>
        {loading ? (
          <p className="text-muted-foreground">Đang tải hồ sơ onboarding…</p>
        ) : (
          <div className="space-y-4">
            {cases.map((c) => {
              const progressPct = calculateChecklistProgress(c.checklist);
              return (
                <article
                  key={c.id}
                  className="rounded-2xl border border-border bg-surface p-5 shadow-sm space-y-4"
                >
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div className="flex items-center gap-2 font-bold text-foreground text-lead">
                        <span>{c.employeeCode} · {c.employeeName}</span>
                        <span className={`rounded-full px-2.5 py-0.5 text-meta ${c.status === "Converted" ? "bg-green-100 text-green-800" : "bg-blue-100 text-blue-800"}`}>
                          {c.status}
                        </span>
                      </div>
                      <p className="mt-1 text-meta text-muted-foreground">
                        Status NV: <strong className="text-foreground">{c.employeeStatus}</strong>
                        {c.candidateName ? ` · Nguồn UV: ${c.candidateName}` : ""}
                      </p>
                      <p className="text-meta text-muted-foreground">
                        Hạn Onboarding: <strong>{c.onboardingDueDate}</strong> · Hạn Thử việc: <strong>{c.trialEndDate}</strong>
                        {c.mentorName ? (
                          <span className="ml-2 rounded bg-purple-50 px-2 py-0.5 font-medium text-purple-700">
                            👤 Mentor: {c.mentorName}
                          </span>
                        ) : (
                          <span className="ml-2 text-yellow-700 italic">⚠️ Chưa có mentor</span>
                        )}
                      </p>
                    </div>

                    {/* Action buttons */}
                    {canManage && c.status !== "Converted" && (
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          className={btn.secondary}
                          onClick={() => {
                            const firstMentor = employees.find((e) => e.id !== c.employeeId);
                            setSelectedMentorId(c.mentorEmployeeId ?? firstMentor?.id ?? "");
                            setMentorModal({
                              caseId: c.id,
                              employeeName: c.employeeName,
                              employeeId: c.employeeId,
                              currentMentorId: c.mentorEmployeeId,
                            });
                          }}
                        >
                          👤 Gán Mentor (UC_HRM_069)
                        </button>
                        <button
                          type="button"
                          className={btn.secondary}
                          onClick={() => {
                            const scoreStr = window.prompt("Điểm Đánh giá Thử việc (0–100)", String(c.trialScore ?? 75));
                            if (scoreStr == null) return;
                            const scoreNum = Number(scoreStr);
                            const comment = window.prompt("Nhận xét đánh giá thử việc", c.trialComment ?? "") ?? "";

                            const v = validateTrialEvaluation(scoreNum, comment);
                            if (!v.valid) { setError(v.error ?? "Điểm đánh giá không hợp lệ."); return; }

                            void evaluateOnboardingTrial(c.id, scoreNum, comment)
                              .then(() => {
                                setOk("✅ Đã lưu kết quả đánh giá thử việc (UC_HRM_072).");
                                return load();
                              })
                              .catch((err: unknown) => setError(err instanceof Error ? err.message : "Đánh giá thất bại."));
                          }}
                        >
                          📝 Đánh giá TV (UC_HRM_072)
                        </button>
                        <button
                          type="button"
                          className={btn.primary}
                          onClick={() => {
                            const v = validateConvertOfficial(c.trialScore);
                            if (!v.valid) { setError(v.error ?? "Chưa đủ điều kiện chuyển chính thức."); return; }

                            void convertOnboardingOfficial(c.id)
                              .then(() => {
                                setOk("🎉 Đã chuyển nhân viên thành chính thức (Active)! (UC_HRM_073)");
                                return load();
                              })
                              .catch((err: unknown) => setError(err instanceof Error ? err.message : "Cần đánh giá TV trước khi convert."));
                          }}
                        >
                          🎉 Chuyển chính thức (UC_HRM_073)
                        </button>
                      </div>
                    )}
                  </div>

                  {/* Checklist & Documents grid */}
                  <div className="grid gap-4 md:grid-cols-2 pt-2 border-t border-border">
                    {/* UC_HRM_070: Interactive Checklist & Progress Bar */}
                    <div className="space-y-2">
                      <div className="flex items-center justify-between">
                        <span className="text-body font-bold text-foreground">Checklist tiếp nhận (UC_HRM_070)</span>
                        <span className={`text-meta font-bold ${progressPct === 100 ? "text-green-600" : "text-brand-strong"}`}>
                          {progressPct}% hoàn thành
                        </span>
                      </div>

                      {/* Progress Bar */}
                      <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
                        <div
                          className={`h-full transition-all duration-300 ${progressPct === 100 ? "bg-green-500" : "bg-brand-strong"}`}
                          style={{ width: `${progressPct}%` }}
                        />
                      </div>

                      <ul className="space-y-1.5 pt-1">
                        {c.checklist.map((item) => (
                          <li key={item.key} className="flex items-center gap-2.5 text-body">
                            <input
                              type="checkbox"
                              checked={item.done}
                              disabled={!canManage || c.status === "Converted"}
                              className="h-4 w-4 rounded border-border text-brand-strong focus:ring-brand-strong"
                              onChange={(e) => {
                                const next = c.checklist.map((x) =>
                                  x.key === item.key ? { ...x, done: e.target.checked } : x,
                                );
                                void updateOnboardingChecklist(c.id, next)
                                  .then(load)
                                  .catch(() => setError("Cập nhật checklist thất bại."));
                              }}
                            />
                            <span className={item.done ? "line-through opacity-60 text-muted-foreground" : "font-medium"}>
                              {item.label}
                            </span>
                          </li>
                        ))}
                      </ul>
                    </div>

                    {/* UC_HRM_071: Upload Giấy tờ / Chứng chỉ */}
                    <div className="space-y-2">
                      <span className="text-body font-bold text-foreground">Chứng chỉ / Giấy tờ tiếp nhận (UC_HRM_071)</span>
                      <ul className="space-y-1.5 text-meta">
                        {c.documents.map((d) => (
                          <li key={d.id} className="flex items-center gap-1.5">
                            <span>📄</span>
                            <a
                              className="font-medium text-brand-strong underline-offset-2 hover:underline"
                              href={`${apiBase}/api/sys/files/${encodeURIComponent(d.storageKey)}`}
                              target="_blank"
                              rel="noreferrer"
                            >
                              {d.title}
                            </a>
                          </li>
                        ))}
                        {c.documents.length === 0 && (
                          <li className="text-muted-foreground italic">Chưa có chứng chỉ / giấy tờ đính kèm.</li>
                        )}
                      </ul>

                      {canManage && c.status !== "Converted" && (
                        <div className="pt-2">
                          <label className="block space-y-1 text-meta">
                            <span className="text-muted-foreground font-medium">+ Thêm chứng chỉ / giấy tờ mới:</span>
                            <input
                              type="file"
                              accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                              className="block w-full text-meta"
                              onChange={(e) => {
                                const f = e.target.files?.[0];
                                if (!f) return;
                                const title = window.prompt("Tiêu đề giấy tờ / chứng chỉ (ví dụ: Bằng ĐH, Giấy khám sức khỏe...)", f.name);
                                if (!title?.trim()) return;

                                const v = validateOnboardingDocument(title.trim(), "pending");
                                if (!v.valid) { setError(v.error ?? "Tiêu đề không hợp lệ."); return; }

                                void uploadHrmFile(f)
                                  .then((u) => addOnboardingDocument(c.id, title.trim(), u.storageKey))
                                  .then(() => {
                                    setOk(`✅ Upload tài liệu "${title.trim()}" thành công.`);
                                    return load();
                                  })
                                  .catch(() => setError("Upload giấy tờ thất bại."));
                                e.target.value = "";
                              }}
                            />
                          </label>
                        </div>
                      )}

                      {c.trialScore != null && (
                        <div className="mt-3 rounded-lg border border-border bg-background p-2 text-meta space-y-0.5">
                          <div>
                            Kết quả thử việc: <strong className="text-brand-strong">{c.trialScore} / 100 điểm</strong>
                          </div>
                          {c.trialComment && <div className="text-muted-foreground">{c.trialComment}</div>}
                        </div>
                      )}
                    </div>
                  </div>
                </article>
              );
            })}
            {cases.length === 0 && (
              <p className="text-muted-foreground text-center py-6">Chưa có hồ sơ onboarding nào.</p>
            )}
          </div>
        )}
      </section>
    </div>
  );
}
