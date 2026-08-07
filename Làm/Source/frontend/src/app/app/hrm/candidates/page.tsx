"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  addCandidateCareNote,
  closeJobPosting,
  createCandidate,
  createJobPosting,
  evaluateCandidate,
  fetchCandidates,
  fetchJobPostings,
  fetchRecruitChannelStats,
  fetchRecruitmentRequests,
  updateCandidatePipeline,
  uploadHrmFile,
  type CandidateDto,
  type JobPostingDto,
  type RecruitChannelStatDto,
  type RecruitmentRequestDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

const PIPELINE = ["New", "Screening", "Evaluating", "Accepted", "Rejected"] as const;
const CHANNELS = ["Internal", "Website", "Facebook", "LinkedIn", "Other"] as const;

export default function CandidatesPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.recruit.read");
  const canManage = can("hrm.recruit.manage");

  const [requests, setRequests] = useState<RecruitmentRequestDto[]>([]);
  const [postings, setPostings] = useState<JobPostingDto[]>([]);
  const [candidates, setCandidates] = useState<CandidateDto[]>([]);
  const [stats, setStats] = useState<RecruitChannelStatDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [reqId, setReqId] = useState("");
  const [postTitle, setPostTitle] = useState("");
  const [channel, setChannel] = useState<string>("Internal");

  const [postingId, setPostingId] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [cvKey, setCvKey] = useState<string | null>(null);

  const approved = useMemo(
    () => requests.filter((r) => r.status === "Approved"),
    [requests],
  );
  const openPosts = useMemo(() => postings.filter((p) => p.status === "Open"), [postings]);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [r, p, c, s, o] = await Promise.all([
        fetchRecruitmentRequests(),
        fetchJobPostings(),
        fetchCandidates(),
        fetchRecruitChannelStats(),
        fetchOrgUnits(),
      ]);
      setRequests(r);
      setPostings(p);
      setCandidates(c);
      setStats(s);
      setOrgs(o);
      if (!reqId && r.find((x) => x.status === "Approved"))
        setReqId(r.find((x) => x.status === "Approved")!.id);
      if (!postingId && p.find((x) => x.status === "Open"))
        setPostingId(p.find((x) => x.status === "Open")!.id);
    } catch {
      setError("Không tải được tin tuyển / ứng viên.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onCreatePost(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !reqId) return;
    try {
      await createJobPosting({
        recruitmentRequestId: reqId,
        title: postTitle || approved.find((a) => a.id === reqId)?.jobTitleName || "Tin tuyển",
        channel,
      });
      setOk("Đã tạo tin tuyển.");
      setPostTitle("");
      await load();
    } catch {
      setError("Tạo tin thất bại (cần phiếu Approved).");
    }
  }

  async function onCreateCandidate(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !postingId) return;
    try {
      await createCandidate({
        jobPostingId: postingId,
        fullName,
        email: email || null,
        phone: phone || null,
        cvStorageKey: cvKey,
      });
      setOk("Đã thêm ứng viên.");
      setFullName("");
      setEmail("");
      setPhone("");
      setCvKey(null);
      await load();
    } catch {
      setError("Thêm ứng viên thất bại.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.recruit.read</p>;
  }

  const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:1111";

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Tin tuyển & ứng viên</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Tạo tin từ phiếu đã duyệt · pipeline UV · CV · đánh giá · kênh
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      {canManage && (
        <section className="grid gap-4 lg:grid-cols-2">
          <form
            onSubmit={(e) => void onCreatePost(e)}
            className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
          >
            <h2 className="text-lead font-bold">Tạo tin tuyển</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Phiếu đã duyệt</span>
              <select
                value={reqId}
                onChange={(e) => setReqId(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              >
                {approved.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.docNo} · {r.jobTitleName} ×{r.headcount}
                  </option>
                ))}
              </select>
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Tiêu đề</span>
              <input
                value={postTitle}
                onChange={(e) => setPostTitle(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                placeholder="Tuyển Senior Dev…"
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Kênh</span>
              <select
                value={channel}
                onChange={(e) => setChannel(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              >
                {CHANNELS.map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </label>
            <button type="submit" className={btn.primary} disabled={!approved.length}>
              Tạo tin
            </button>
            {!approved.length && (
              <p className="text-meta text-muted-foreground">Chưa có phiếu Approved.</p>
            )}
          </form>

          <form
            onSubmit={(e) => void onCreateCandidate(e)}
            className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
          >
            <h2 className="text-lead font-bold">Nhập ứng viên</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Tin tuyển mở</span>
              <select
                value={postingId}
                onChange={(e) => setPostingId(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              >
                {openPosts.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.title} · {p.channel}
                  </option>
                ))}
              </select>
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Họ tên</span>
              <input
                required
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              />
            </label>
            <div className="grid gap-2 sm:grid-cols-2">
              <input
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Email"
                className="h-9 rounded-md border border-border bg-background px-2 text-body"
              />
              <input
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                placeholder="SĐT"
                className="h-9 rounded-md border border-border bg-background px-2 text-body"
              />
            </div>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">CV (upload)</span>
              <input
                type="file"
                className="block w-full text-meta"
                onChange={(e) => {
                  const f = e.target.files?.[0];
                  if (!f) return;
                  void uploadHrmFile(f)
                    .then((x) => {
                      setCvKey(x.storageKey);
                      setOk(`Đã upload CV: ${x.fileName ?? x.storageKey}`);
                    })
                    .catch(() => setError("Upload CV thất bại."));
                }}
              />
              {cvKey && <span className="text-meta text-brand-strong">Đã gắn CV</span>}
            </label>
            <button type="submit" className={btn.primary} disabled={!openPosts.length}>
              Thêm UV
            </button>
          </form>
        </section>
      )}

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Tin tuyển</h2>
        {loading ? (
          <p className="text-muted-foreground">Đang tải…</p>
        ) : (
          <div className="overflow-hidden rounded-xl border border-border bg-surface">
            <table className="w-full text-body">
              <thead className="border-b border-border bg-muted text-left text-muted-foreground">
                <tr>
                  <th className="px-3 py-2">Tiêu đề</th>
                  <th className="px-3 py-2">Phiếu</th>
                  <th className="px-3 py-2">Kênh</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2" />
                </tr>
              </thead>
              <tbody>
                {postings.map((p) => (
                  <tr key={p.id} className="border-t border-border">
                    <td className="px-3 py-2 font-medium">{p.title}</td>
                    <td className="px-3 py-2 text-meta">
                      {p.requestDocNo} · {p.jobTitleName}
                    </td>
                    <td className="px-3 py-2">{p.channel}</td>
                    <td className="px-3 py-2">{p.status}</td>
                    <td className="px-3 py-2 text-right">
                      {canManage && p.status === "Open" && (
                        <button
                          type="button"
                          className="text-meta text-destructive underline-offset-2 hover:underline"
                          onClick={() => void closeJobPosting(p.id).then(load)}
                        >
                          Đóng
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
                {postings.length === 0 && (
                  <tr>
                    <td colSpan={5} className="px-3 py-4 text-center text-muted-foreground">
                      Chưa có tin.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Pipeline ứng viên</h2>
        <div className="overflow-x-auto rounded-xl border border-border bg-surface">
          <table className="w-full min-w-[720px] text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Họ tên</th>
                <th className="px-3 py-2">Tin</th>
                <th className="px-3 py-2">Pipeline</th>
                <th className="px-3 py-2">Đánh giá</th>
                <th className="px-3 py-2">CV / chăm sóc</th>
              </tr>
            </thead>
            <tbody>
              {candidates.map((c) => (
                <tr key={c.id} className="border-t border-border align-top">
                  <td className="px-3 py-2">
                    <div className="font-medium">{c.fullName}</div>
                    <div className="text-meta text-muted-foreground">
                      {[c.email, c.phone].filter(Boolean).join(" · ")}
                    </div>
                  </td>
                  <td className="px-3 py-2 text-meta">{c.jobPostingTitle}</td>
                  <td className="px-3 py-2">
                    {canManage ? (
                      <select
                        value={c.pipelineStatus}
                        className="h-8 rounded-md border border-border bg-background px-1 text-meta"
                        onChange={(e) => {
                          void updateCandidatePipeline(c.id, e.target.value)
                            .then(load)
                            .catch(() => setError("Cập nhật pipeline thất bại."));
                        }}
                      >
                        {PIPELINE.map((s) => (
                          <option key={s} value={s}>
                            {s}
                          </option>
                        ))}
                      </select>
                    ) : (
                      c.pipelineStatus
                    )}
                  </td>
                  <td className="px-3 py-2 text-meta">
                    <div>
                      {c.evalOrgUnitName ?? "—"} · {c.evalScore ?? "—"}đ
                    </div>
                    {c.evalComment && <div className="text-muted-foreground">{c.evalComment}</div>}
                    {canManage && (
                      <button
                        type="button"
                        className="mt-1 text-brand-strong underline-offset-2 hover:underline"
                        onClick={() => {
                          const ou = orgs[0]?.id;
                          const score = window.prompt("Điểm (0–100)", String(c.evalScore ?? 70));
                          const comment = window.prompt("Nhận xét", c.evalComment ?? "") ?? "";
                          if (score == null) return;
                          void evaluateCandidate(c.id, {
                            evalOrgUnitId: ou ?? null,
                            evalScore: Number(score),
                            evalComment: comment,
                          })
                            .then(load)
                            .catch(() => setError("Lưu đánh giá thất bại."));
                        }}
                      >
                        Đánh giá / chuyển ĐV
                      </button>
                    )}
                  </td>
                  <td className="px-3 py-2 text-meta">
                    {c.cvStorageKey ? (
                      <a
                        className="underline"
                        href={`${apiBase}/api/sys/files/${encodeURIComponent(c.cvStorageKey)}`}
                        target="_blank"
                        rel="noreferrer"
                      >
                        Tải CV
                      </a>
                    ) : (
                      "—"
                    )}
                    {c.careNotes && (
                      <pre className="mt-1 max-h-20 overflow-auto whitespace-pre-wrap text-[11px] text-muted-foreground">
                        {c.careNotes}
                      </pre>
                    )}
                    {canManage && (
                      <button
                        type="button"
                        className="mt-1 block text-brand-strong underline-offset-2 hover:underline"
                        onClick={() => {
                          const note = window.prompt("Ghi chú chăm sóc");
                          if (!note) return;
                          void addCandidateCareNote(c.id, note)
                            .then(load)
                            .catch(() => setError("Ghi chú thất bại."));
                        }}
                      >
                        + Ghi chú
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {candidates.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-3 py-4 text-center text-muted-foreground">
                    Chưa có ứng viên.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Hiệu quả kênh</h2>
        <div className="overflow-hidden rounded-xl border border-border bg-surface">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Kênh</th>
                <th className="px-3 py-2">Số tin</th>
                <th className="px-3 py-2">Số UV</th>
              </tr>
            </thead>
            <tbody>
              {stats.map((s) => (
                <tr key={s.channel} className="border-t border-border">
                  <td className="px-3 py-2 font-medium">{s.channel}</td>
                  <td className="px-3 py-2">{s.postingCount}</td>
                  <td className="px-3 py-2">{s.candidateCount}</td>
                </tr>
              ))}
              {stats.length === 0 && (
                <tr>
                  <td colSpan={3} className="px-3 py-4 text-center text-muted-foreground">
                    Chưa có dữ liệu kênh.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
