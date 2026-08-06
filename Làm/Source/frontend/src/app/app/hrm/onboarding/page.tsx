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
      setError("Không tải được onboarding.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onSaveSettings(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      setSettings(await upsertOnboardingSettings(settings));
      setOk("Đã lưu cấu hình.");
    } catch {
      setError("Lưu cấu hình thất bại.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Onboarding</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Cấu hình · tạo NV từ UV · checklist · thử việc · chuyển chính thức
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <section className="grid gap-4 lg:grid-cols-2">
        <form
          onSubmit={(e) => void onSaveSettings(e)}
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
        >
          <h2 className="text-lead font-bold">Cấu hình thời hạn</h2>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Onboarding (ngày)</span>
            <input
              type="number"
              min={1}
              max={365}
              value={settings.onboardingDays}
              disabled={!canManage}
              onChange={(e) =>
                setSettings((s) => ({ ...s, onboardingDays: Number(e.target.value) }))
              }
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            />
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Thử việc (ngày)</span>
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
              Lưu
            </button>
          )}
        </form>

        {canManage && (
          <div className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Tạo NV từ ứng viên Accepted</h2>
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
            <button
              type="button"
              className={btn.primary}
              disabled={!hireable.length}
              onClick={() => {
                if (!candidateId) return;
                void hireFromCandidate(candidateId)
                  .then(() => {
                    setOk("Đã tạo hồ sơ NV + onboarding.");
                    return load();
                  })
                  .catch(() => setError("Thuê UV thất bại."));
              }}
            >
              Tạo hồ sơ NV (Probation)
            </button>
            {!hireable.length && (
              <p className="text-meta text-muted-foreground">
                Cần UV ở trạng thái Accepted (chưa convert).
              </p>
            )}
          </div>
        )}
      </section>

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Cảnh báo hết hạn thử việc (14 ngày)</h2>
        <div className="overflow-hidden rounded-xl border border-border bg-surface">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Mã NV</th>
                <th className="px-3 py-2">Họ tên</th>
                <th className="px-3 py-2">Hết TV</th>
                <th className="px-3 py-2">Còn</th>
              </tr>
            </thead>
            <tbody>
              {expiring.map((x) => (
                <tr key={x.onboardingCaseId} className="border-t border-border">
                  <td className="px-3 py-2 font-medium">{x.employeeCode}</td>
                  <td className="px-3 py-2">{x.fullName}</td>
                  <td className="px-3 py-2">{x.trialEndDate}</td>
                  <td className="px-3 py-2 text-destructive font-semibold">{x.daysLeft} ngày</td>
                </tr>
              ))}
              {!loading && expiring.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-4 text-center text-muted-foreground">
                    Không có cảnh báo.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Hồ sơ onboarding</h2>
        {loading ? (
          <p className="text-muted-foreground">Đang tải…</p>
        ) : (
          <div className="space-y-3">
            {cases.map((c) => (
              <article
                key={c.id}
                className="rounded-xl border border-border bg-surface p-4 shadow-sm"
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <h3 className="font-bold text-foreground">
                      {c.employeeCode} · {c.employeeName}
                    </h3>
                    <p className="text-meta text-muted-foreground">
                      Status NV: {c.employeeStatus} · Case: {c.status}
                      {c.candidateName ? ` · từ UV ${c.candidateName}` : ""}
                    </p>
                    <p className="text-meta text-muted-foreground">
                      OB đến {c.onboardingDueDate} · TV đến {c.trialEndDate}
                      {c.mentorName ? ` · Mentor: ${c.mentorName}` : ""}
                    </p>
                  </div>
                  {canManage && c.status !== "Converted" && (
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        className={btn.secondary}
                        onClick={() => {
                          const mentor = employees.find((e) => e.id !== c.employeeId);
                          if (!mentor) {
                            setError("Chưa có NV khác để gán mentor.");
                            return;
                          }
                          const pick = window.prompt(
                            "EmployeeId mentor (để trống = NV đầu tiên khác)",
                            mentor.id,
                          );
                          if (!pick) return;
                          void assignOnboardingMentor(c.id, pick)
                            .then(load)
                            .catch(() => setError("Gán mentor thất bại."));
                        }}
                      >
                        Gán mentor
                      </button>
                      <button
                        type="button"
                        className={btn.secondary}
                        onClick={() => {
                          const score = window.prompt("Điểm TV (0–100)", String(c.trialScore ?? 70));
                          if (score == null) return;
                          const comment = window.prompt("Nhận xét", c.trialComment ?? "") ?? "";
                          void evaluateOnboardingTrial(c.id, Number(score), comment)
                            .then(() => {
                              setOk("Đã lưu đánh giá TV.");
                              return load();
                            })
                            .catch(() => setError("Đánh giá thất bại."));
                        }}
                      >
                        Đánh giá TV
                      </button>
                      <button
                        type="button"
                        className={btn.primary}
                        onClick={() => {
                          void convertOnboardingOfficial(c.id)
                            .then(() => {
                              setOk("Đã chuyển chính thức.");
                              return load();
                            })
                            .catch(() => setError("Cần đánh giá TV trước khi convert."));
                        }}
                      >
                        Chuyển chính thức
                      </button>
                    </div>
                  )}
                </div>

                <div className="mt-3 grid gap-3 md:grid-cols-2">
                  <div>
                    <p className="mb-1 text-meta font-semibold">Checklist</p>
                    <ul className="space-y-1">
                      {c.checklist.map((item) => (
                        <li key={item.key} className="flex items-center gap-2 text-body">
                          <input
                            type="checkbox"
                            checked={item.done}
                            disabled={!canManage || c.status === "Converted"}
                            onChange={(e) => {
                              const next = c.checklist.map((x) =>
                                x.key === item.key ? { ...x, done: e.target.checked } : x,
                              );
                              void updateOnboardingChecklist(c.id, next)
                                .then(load)
                                .catch(() => setError("Cập nhật checklist thất bại."));
                            }}
                          />
                          <span className={item.done ? "line-through opacity-70" : ""}>
                            {item.label}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  <div>
                    <p className="mb-1 text-meta font-semibold">Giấy tờ / chứng chỉ</p>
                    <ul className="mb-2 space-y-1 text-meta">
                      {c.documents.map((d) => (
                        <li key={d.id}>
                          <a
                            className="underline"
                            href={`${apiBase}/api/sys/files/${encodeURIComponent(d.storageKey)}`}
                            target="_blank"
                            rel="noreferrer"
                          >
                            {d.title}
                          </a>
                        </li>
                      ))}
                      {c.documents.length === 0 && (
                        <li className="text-muted-foreground">Chưa có file.</li>
                      )}
                    </ul>
                    {canManage && c.status !== "Converted" && (
                      <input
                        type="file"
                        className="text-meta"
                        onChange={(e) => {
                          const f = e.target.files?.[0];
                          if (!f) return;
                          void uploadHrmFile(f)
                            .then((u) => addOnboardingDocument(c.id, f.name, u.storageKey))
                            .then(load)
                            .catch(() => setError("Upload giấy tờ thất bại."));
                          e.target.value = "";
                        }}
                      />
                    )}
                    {c.trialScore != null && (
                      <p className="mt-2 text-meta">
                        Điểm TV: <strong>{c.trialScore}</strong>
                        {c.trialComment ? ` — ${c.trialComment}` : ""}
                      </p>
                    )}
                  </div>
                </div>
              </article>
            ))}
            {cases.length === 0 && (
              <p className="text-muted-foreground">Chưa có hồ sơ onboarding.</p>
            )}
          </div>
        )}
      </section>
    </div>
  );
}
