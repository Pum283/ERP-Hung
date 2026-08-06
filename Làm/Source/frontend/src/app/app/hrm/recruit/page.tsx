"use client";

import { FormEvent, Fragment, useEffect, useState } from "react";
import {
  closeRecruitmentRequest,
  createRecruitmentRequest,
  fetchJobTitles,
  fetchRecruitmentRequests,
  submitRecruitmentRequest,
  type RecruitmentRequestDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

export default function RecruitDemandPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.recruit.read");
  const canManage = can("hrm.recruit.manage");

  const [titles, setTitles] = useState<{ id: string; code: string; name: string }[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [rows, setRows] = useState<RecruitmentRequestDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  const [jobTitleId, setJobTitleId] = useState("");
  const [orgUnitId, setOrgUnitId] = useState("");
  const [headcount, setHeadcount] = useState("1");
  const [reason, setReason] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [t, o, r] = await Promise.all([
        fetchJobTitles(),
        fetchOrgUnits(),
        fetchRecruitmentRequests(),
      ]);
      setTitles(t);
      setOrgs(o);
      setRows(r);
      if (!jobTitleId && t[0]) setJobTitleId(t[0].id);
      if (!orgUnitId && o[0]) setOrgUnitId(o[0].id);
    } catch {
      setError("Không tải được nhu cầu tuyển.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onCreate(e: FormEvent, submit: boolean) {
    e.preventDefault();
    if (!canManage) return;
    setSubmitting(true);
    setError(null);
    setOk(null);
    try {
      await createRecruitmentRequest({
        jobTitleId,
        orgUnitId,
        headcount: Number(headcount),
        reason,
        submit,
      });
      setOk(submit ? "Đã gửi duyệt." : "Đã lưu nháp.");
      setReason("");
      await load();
    } catch {
      setError("Không tạo/gửi được phiếu. Kiểm tra vị trí, đơn vị và lý do.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.recruit.read</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Nhu cầu tuyển dụng</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Phiếu đề xuất · gửi duyệt WF · lịch sử duyệt
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      {canManage && (
        <section className="space-y-3">
          <h2 className="text-lead font-bold text-foreground">Tạo phiếu đề xuất</h2>
          <form className="grid max-w-2xl gap-3 rounded-xl border border-border bg-surface p-4 shadow-sm sm:grid-cols-2">
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Vị trí cần tuyển</span>
              <select
                value={jobTitleId}
                onChange={(e) => setJobTitleId(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              >
                {titles.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Số lượng</span>
              <input
                type="number"
                min={1}
                max={999}
                value={headcount}
                onChange={(e) => setHeadcount(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              />
            </label>
            <label className="space-y-1 text-body sm:col-span-2">
              <span className="text-muted-foreground">Đơn vị</span>
              <select
                value={orgUnitId}
                onChange={(e) => setOrgUnitId(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              >
                {orgs.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="space-y-1 text-body sm:col-span-2">
              <span className="text-muted-foreground">Lý do tuyển dụng</span>
              <input
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                placeholder="Thay thế nghỉ việc / mở rộng team…"
                required
              />
            </label>
            <div className="flex gap-2 sm:col-span-2">
              <button
                type="button"
                disabled={submitting}
                onClick={(e) => void onCreate(e, false)}
                className={btn.secondary}
              >
                Lưu nháp
              </button>
              <button
                type="button"
                disabled={submitting}
                onClick={(e) => void onCreate(e, true)}
                className={btn.primary}
              >
                Gửi duyệt
              </button>
            </div>
          </form>
        </section>
      )}

      <section className="space-y-2">
        <h2 className="text-lead font-bold text-foreground">Danh sách phiếu</h2>
        {loading ? (
          <p className="text-body text-muted-foreground">Đang tải…</p>
        ) : (
          <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
            <table className="w-full text-body">
              <thead className="border-b border-border bg-muted text-left text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-semibold">Số phiếu</th>
                  <th className="px-4 py-2.5 font-semibold">Vị trí</th>
                  <th className="px-4 py-2.5 font-semibold">SL</th>
                  <th className="px-4 py-2.5 font-semibold">Đơn vị</th>
                  <th className="px-4 py-2.5 font-semibold">Người đề xuất</th>
                  <th className="px-4 py-2.5 font-semibold">Status</th>
                  <th className="px-4 py-2.5 font-semibold" />
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => (
                  <Fragment key={r.id}>
                    <tr className="border-t border-border">
                      <td className="px-4 py-2.5 font-medium">{r.docNo}</td>
                      <td className="px-4 py-2.5">{r.jobTitleName}</td>
                      <td className="px-4 py-2.5">{r.headcount}</td>
                      <td className="px-4 py-2.5">{r.orgUnitName}</td>
                      <td className="px-4 py-2.5">{r.requesterName}</td>
                      <td className="px-4 py-2.5">
                        <span className="inline-flex rounded-full bg-brand-muted px-2 py-0.5 text-meta font-semibold text-brand-strong">
                          {r.status}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <div className="flex flex-wrap justify-end gap-2">
                          <button
                            type="button"
                            className="text-meta font-semibold text-brand-strong underline-offset-2 hover:underline"
                            onClick={() => setExpandedId((id) => (id === r.id ? null : r.id))}
                          >
                            Lịch sử
                          </button>
                          {canManage && r.status === "Draft" && (
                            <button
                              type="button"
                              className="text-meta font-semibold text-brand-strong underline-offset-2 hover:underline"
                              onClick={() => {
                                void submitRecruitmentRequest(r.id)
                                  .then(() => load())
                                  .catch(() => setError("Gửi duyệt thất bại."));
                              }}
                            >
                              Gửi duyệt
                            </button>
                          )}
                          {canManage &&
                            (r.status === "Draft" ||
                              r.status === "Rejected" ||
                              r.status === "Approved") && (
                              <button
                                type="button"
                                className="text-meta font-semibold text-destructive underline-offset-2 hover:underline"
                                onClick={() => {
                                  void closeRecruitmentRequest(r.id)
                                    .then(() => load())
                                    .catch(() => setError("Đóng/hủy thất bại."));
                                }}
                              >
                                {r.status === "Draft" ? "Hủy" : "Đóng"}
                              </button>
                            )}
                        </div>
                      </td>
                    </tr>
                    {expandedId === r.id && (
                      <tr className="border-t border-border bg-muted/40">
                        <td colSpan={7} className="px-4 py-3 text-meta">
                          <p className="mb-1 font-semibold text-foreground">Lý do: {r.reason}</p>
                          {r.approvalHistory.length === 0 ? (
                            <p className="text-muted-foreground">Chưa có bước duyệt.</p>
                          ) : (
                            <ul className="space-y-1">
                              {r.approvalHistory.map((h) => (
                                <li key={h.actionId}>
                                  {new Date(h.at).toLocaleString("vi-VN")} · {h.actorName} ·{" "}
                                  <strong>{h.action}</strong>
                                  {h.comment ? ` — ${h.comment}` : ""}
                                </li>
                              ))}
                            </ul>
                          )}
                        </td>
                      </tr>
                    )}
                  </Fragment>
                ))}
                {rows.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-6 text-center text-muted-foreground">
                      Chưa có phiếu đề xuất.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
