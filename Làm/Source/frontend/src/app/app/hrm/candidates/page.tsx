"use client";

import { FormEvent, useEffect, useMemo, useRef, useState } from "react";
import {
  addCandidateCareNote,
  assignCandidateEvalOrg,
  closeJobPosting,
  createCandidate,
  createJobPosting,
  decideCandidate,
  fetchCandidateCareNotes,
  fetchCandidates,
  fetchJobPostings,
  fetchRecruitChannelReport,
  fetchRecruitChannelStats,
  fetchRecruitmentRequests,
  screenCandidate,
  submitCandidateEvaluation,
  updateCandidatePipeline,
  uploadHrmFile,
  type CandidateDto,
  type CareNoteItemDto,
  type JobPostingDto,
  type RecruitChannelReportDto,
  type RecruitChannelStatDto,
  type RecruitmentRequestDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import {
  canScreen,
  validateCandidateForm,
  validateCvFile,
  validateScreenForm,
  RECRUIT_CHANNELS,
  type RecruitChannel,
} from "@/shared/api/hrm-recruit-candidate-screening-helpers";
import {
  validateAssignEvalOrgForm,
  validateCandidateDecisionForm,
  validateEvaluationForm,
  isValidPipelineTransition,
  EVAL_RESULT_OPTIONS,
  type EvalResultOption,
} from "@/shared/api/hrm-recruit-evaluation-decide-helpers";

const PIPELINE = ["New", "Screening", "Evaluating", "Accepted", "Rejected"] as const;

const STATUS_BADGE: Record<string, string> = {
  New: "bg-slate-100 text-slate-700",
  Screening: "bg-blue-100 text-blue-700",
  Evaluating: "bg-yellow-100 text-yellow-700",
  Accepted: "bg-green-100 text-green-700",
  Rejected: "bg-red-100 text-red-700",
};

const EVAL_RESULT_BADGE: Record<string, string> = {
  Pass: "bg-green-100 text-green-800 border-green-300",
  Fail: "bg-red-100 text-red-800 border-red-300",
  Hold: "bg-yellow-100 text-yellow-800 border-yellow-300",
};

// ─── Modal Types ──────────────────────────────────────────────────────────────

type ModalState =
  | { type: "screen"; candidateId: string; candidateName: string }
  | { type: "assignEvalOrg"; candidateId: string; candidateName: string; currentOrgId?: string | null }
  | { type: "evaluate"; candidateId: string; candidateName: string; currentOrgId?: string | null; score?: number | null; comment?: string | null; result?: string | null }
  | { type: "decide"; candidateId: string; candidateName: string; initialAction?: "Accept" | "Reject" }
  | { type: "careNotesHistory"; candidateId: string; candidateName: string }
  | null;

// ─── UC_HRM_059: Screen Modal Panel ──────────────────────────────────────────

function ScreenModalPanel({
  modal,
  onClose,
  onConfirm,
}: {
  modal: Extract<NonNullable<ModalState>, { type: "screen" }>;
  onClose: () => void;
  onConfirm: (action: "Screen" | "ScreenReject", note: string) => Promise<void>;
}) {
  const [action, setAction] = useState<"Screen" | "ScreenReject">("Screen");
  const [note, setNote] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const v = validateScreenForm({ action, screeningNote: note });
    if (!v.valid) { setErr(v.error ?? "Lỗi không xác định."); return; }
    setSubmitting(true);
    setErr(null);
    try {
      await onConfirm(action, note.trim());
      onClose();
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Thao tác thất bại.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-6 shadow-xl">
        <h3 className="text-lead font-bold text-foreground">Sơ loại ứng viên</h3>
        <p className="mt-1 text-meta text-muted-foreground">{modal.candidateName}</p>
        <form onSubmit={(e) => void handleSubmit(e)} className="mt-4 space-y-4">
          <div className="flex gap-3">
            {(["Screen", "ScreenReject"] as const).map((a) => (
              <button
                key={a}
                type="button"
                onClick={() => setAction(a)}
                className={`flex-1 rounded-lg border py-2 text-body font-medium transition-colors ${
                  action === a
                    ? a === "Screen"
                      ? "border-blue-500 bg-blue-50 text-blue-700"
                      : "border-red-500 bg-red-50 text-red-700"
                    : "border-border bg-background text-muted-foreground hover:bg-muted"
                }`}
              >
                {a === "Screen" ? "✅ Tiếp tục" : "❌ Từ chối"}
              </button>
            ))}
          </div>
          <label className="block space-y-1">
            <span className="text-body text-muted-foreground">
              {action === "Screen" ? "Ghi chú sơ loại *" : "Lý do từ chối *"}
            </span>
            <textarea
              value={note}
              onChange={(e) => setNote(e.target.value)}
              rows={3}
              maxLength={500}
              className="w-full rounded-lg border border-border bg-background p-2 text-body focus:outline-none focus:ring-2 focus:ring-brand-strong"
              placeholder={action === "Screen" ? "CV phù hợp, mời vòng phỏng vấn sơ bộ…" : "Kinh nghiệm chưa đủ yêu cầu…"}
            />
            <span className="block text-right text-meta text-muted-foreground">{note.length}/500</span>
          </label>
          {err && <p className="text-body text-destructive">{err}</p>}
          <div className="flex gap-2 pt-2">
            <button type="button" onClick={onClose} className={btn.secondary + " flex-1"}>Hủy</button>
            <button
              type="submit"
              disabled={submitting}
              className={`flex-1 ${action === "Screen" ? btn.primary : "rounded-lg bg-red-600 px-4 py-2 text-body font-medium text-white hover:bg-red-700 disabled:opacity-60"}`}
            >
              {submitting ? "Đang lưu…" : action === "Screen" ? "Xác nhận tiếp tục" : "Xác nhận từ chối"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── UC_HRM_060: Assign Eval Org Modal Panel ──────────────────────────────────

function AssignEvalOrgModalPanel({
  modal,
  orgs,
  onClose,
  onConfirm,
}: {
  modal: Extract<NonNullable<ModalState>, { type: "assignEvalOrg" }>;
  orgs: OrgUnitDto[];
  onClose: () => void;
  onConfirm: (evalOrgUnitId: string) => Promise<void>;
}) {
  const [orgId, setOrgId] = useState(modal.currentOrgId ?? orgs[0]?.id ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const v = validateAssignEvalOrgForm({ evalOrgUnitId: orgId });
    if (!v.valid) { setErr(v.error ?? "Vui lòng chọn đơn vị."); return; }
    setSubmitting(true);
    setErr(null);
    try {
      await onConfirm(orgId);
      onClose();
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Thao tác thất bại.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-6 shadow-xl">
        <h3 className="text-lead font-bold text-foreground">Phân công đơn vị đánh giá</h3>
        <p className="mt-1 text-meta text-muted-foreground">{modal.candidateName}</p>
        <form onSubmit={(e) => void handleSubmit(e)} className="mt-4 space-y-4">
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Đơn vị / Phòng ban đánh giá *</span>
            <select
              value={orgId}
              onChange={(e) => setOrgId(e.target.value)}
              className="h-10 w-full rounded-lg border border-border bg-background px-3 text-body"
            >
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>{o.name} ({o.code})</option>
              ))}
            </select>
          </label>
          {err && <p className="text-body text-destructive">{err}</p>}
          <div className="flex gap-2 pt-2">
            <button type="button" onClick={onClose} className={btn.secondary + " flex-1"}>Hủy</button>
            <button type="submit" disabled={submitting} className={btn.primary + " flex-1"}>
              {submitting ? "Đang lưu…" : "Xác nhận phân công"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── UC_HRM_061: Evaluation Modal Panel ──────────────────────────────────────

function EvaluationModalPanel({
  modal,
  orgs,
  onClose,
  onConfirm,
}: {
  modal: Extract<NonNullable<ModalState>, { type: "evaluate" }>;
  orgs: OrgUnitDto[];
  onClose: () => void;
  onConfirm: (body: { evalOrgUnitId?: string | null; evalScore: number; evalResult: string; evalComment?: string | null }) => Promise<void>;
}) {
  const [orgId, setOrgId] = useState<string>(modal.currentOrgId ?? orgs[0]?.id ?? "");
  const [score, setScore] = useState<number>(modal.score ?? 75);
  const [result, setResult] = useState<EvalResultOption>((modal.result as EvalResultOption) ?? "Pass");
  const [comment, setComment] = useState<string>(modal.comment ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const v = validateEvaluationForm({ evalOrgUnitId: orgId, evalScore: score, evalResult: result, evalComment: comment });
    if (!v.valid) { setErr(v.error ?? "Dữ liệu không hợp lệ."); return; }
    setSubmitting(true);
    setErr(null);
    try {
      await onConfirm({ evalOrgUnitId: orgId || null, evalScore: score, evalResult: result, evalComment: comment.trim() || null });
      onClose();
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Đánh giá thất bại.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-2xl border border-border bg-surface p-6 shadow-xl">
        <h3 className="text-lead font-bold text-foreground">Form đánh giá ứng viên</h3>
        <p className="mt-1 text-meta text-muted-foreground">{modal.candidateName}</p>
        <form onSubmit={(e) => void handleSubmit(e)} className="mt-4 space-y-4">
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Đơn vị đánh giá</span>
            <select
              value={orgId}
              onChange={(e) => setOrgId(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            >
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>
          </label>
          <div className="grid gap-3 sm:grid-cols-2">
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Điểm số (0 – 100) *</span>
              <input
                type="number"
                min={0}
                max={100}
                value={score}
                onChange={(e) => setScore(Number(e.target.value))}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Kết quả đề xuất *</span>
              <select
                value={result}
                onChange={(e) => setResult(e.target.value as EvalResultOption)}
                className="h-9 w-full rounded-md border border-border bg-background px-2 font-medium"
              >
                {EVAL_RESULT_OPTIONS.map((r) => (
                  <option key={r} value={r}>{r === "Pass" ? "✅ Pass (Đạt)" : r === "Fail" ? "❌ Fail (Không đạt)" : "⏸️ Hold (Cân nhắc)"}</option>
                ))}
              </select>
            </label>
          </div>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Nhận xét chi tiết</span>
            <textarea
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              rows={3}
              maxLength={1000}
              className="w-full rounded-lg border border-border bg-background p-2 text-body focus:outline-none focus:ring-2 focus:ring-brand-strong"
              placeholder="Nhận xét về chuyên môn, thái độ, sự phù hợp với văn hóa công ty…"
            />
            <span className="block text-right text-meta text-muted-foreground">{comment.length}/1000</span>
          </label>
          {err && <p className="text-body text-destructive">{err}</p>}
          <div className="flex gap-2 pt-2">
            <button type="button" onClick={onClose} className={btn.secondary + " flex-1"}>Hủy</button>
            <button type="submit" disabled={submitting} className={btn.primary + " flex-1"}>
              {submitting ? "Đang lưu…" : "Lưu đánh giá"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── UC_HRM_062: Decision Modal Panel ────────────────────────────────────────

function DecisionModalPanel({
  modal,
  onClose,
  onConfirm,
}: {
  modal: Extract<NonNullable<ModalState>, { type: "decide" }>;
  onClose: () => void;
  onConfirm: (action: "Accept" | "Reject", note: string) => Promise<void>;
}) {
  const [action, setAction] = useState<"Accept" | "Reject">(modal.initialAction ?? "Accept");
  const [note, setNote] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const v = validateCandidateDecisionForm({ action, decisionNote: note });
    if (!v.valid) { setErr(v.error ?? "Dữ liệu không hợp lệ."); return; }
    setSubmitting(true);
    setErr(null);
    try {
      await onConfirm(action, note.trim());
      onClose();
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Quyết định thất bại.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-6 shadow-xl">
        <h3 className="text-lead font-bold text-foreground">Quyết định tuyển dụng</h3>
        <p className="mt-1 text-meta text-muted-foreground">{modal.candidateName}</p>
        <form onSubmit={(e) => void handleSubmit(e)} className="mt-4 space-y-4">
          <div className="flex gap-3">
            <button
              type="button"
              onClick={() => setAction("Accept")}
              className={`flex-1 rounded-lg border py-2.5 text-body font-bold transition-colors ${
                action === "Accept"
                  ? "border-green-600 bg-green-50 text-green-700"
                  : "border-border bg-background text-muted-foreground hover:bg-muted"
              }`}
            >
              🎉 Chấp nhận (Accept)
            </button>
            <button
              type="button"
              onClick={() => setAction("Reject")}
              className={`flex-1 rounded-lg border py-2.5 text-body font-bold transition-colors ${
                action === "Reject"
                  ? "border-red-600 bg-red-50 text-red-700"
                  : "border-border bg-background text-muted-foreground hover:bg-muted"
              }`}
            >
              ❌ Từ chối (Reject)
            </button>
          </div>
          <label className="block space-y-1">
            <span className="text-body text-muted-foreground">
              {action === "Accept" ? "Ghi chú nhận việc / Thư mời làm việc (Offer) *" : "Lý do từ chối tuyển dụng *"}
            </span>
            <textarea
              value={note}
              onChange={(e) => setNote(e.target.value)}
              rows={3}
              maxLength={1000}
              className="w-full rounded-lg border border-border bg-background p-2 text-body focus:outline-none focus:ring-2 focus:ring-brand-strong"
              placeholder={action === "Accept" ? "Đồng ý tuyển dụng vị trí Senior, mức lương Gross 35M, ngày nhận việc 01/09..." : "Kỳ vọng lương vượt ngân sách..."}
            />
            <span className="block text-right text-meta text-muted-foreground">{note.length}/1000</span>
          </label>
          {err && <p className="text-body text-destructive">{err}</p>}
          <div className="flex gap-2 pt-2">
            <button type="button" onClick={onClose} className={btn.secondary + " flex-1"}>Hủy</button>
            <button
              type="submit"
              disabled={submitting}
              className={`flex-1 ${action === "Accept" ? "rounded-lg bg-green-600 px-4 py-2 text-body font-medium text-white hover:bg-green-700 disabled:opacity-60" : "rounded-lg bg-red-600 px-4 py-2 text-body font-medium text-white hover:bg-red-700 disabled:opacity-60"}`}
            >
              {submitting ? "Đang lưu…" : action === "Accept" ? "Xác nhận Chấp nhận" : "Xác nhận Từ chối"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── UC_HRM_064: Care Notes History Modal Panel ───────────────────────────────

function CareNotesHistoryModalPanel({
  modal,
  onClose,
}: {
  modal: Extract<NonNullable<ModalState>, { type: "careNotesHistory" }>;
  onClose: () => void;
}) {
  const [items, setItems] = useState<CareNoteItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    void fetchCandidateCareNotes(modal.candidateId)
      .then(setItems)
      .catch(() => setErr("Không tải được lịch sử chăm sóc."))
      .finally(() => setLoading(false));
  }, [modal.candidateId]);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-2xl border border-border bg-surface p-6 shadow-xl">
        <h3 className="text-lead font-bold text-foreground">Lịch sử chăm sóc ứng viên</h3>
        <p className="mt-1 text-meta text-muted-foreground">{modal.candidateName}</p>

        <div className="mt-4 max-h-80 overflow-y-auto space-y-3 pr-1">
          {loading && <p className="text-meta text-muted-foreground">Đang tải lịch sử…</p>}
          {err && <p className="text-body text-destructive">{err}</p>}
          {!loading && !err && items.length === 0 && (
            <p className="text-meta text-muted-foreground text-center py-4">Chưa có ghi chú chăm sóc nào.</p>
          )}
          {items.map((item, idx) => (
            <div key={idx} className="rounded-lg border border-border bg-background p-3 text-body space-y-1">
              <div className="text-meta font-medium text-brand-strong">
                📅 {new Date(item.at).toLocaleString("vi-VN")}
              </div>
              <p className="text-foreground whitespace-pre-wrap">{item.note}</p>
            </div>
          ))}
        </div>

        <div className="mt-6 text-right">
          <button type="button" onClick={onClose} className={btn.secondary}>
            Đóng
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function CandidatesPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.recruit.read");
  const canManage = can("hrm.recruit.manage");

  const [requests, setRequests] = useState<RecruitmentRequestDto[]>([]);
  const [postings, setPostings] = useState<JobPostingDto[]>([]);
  const [candidates, setCandidates] = useState<CandidateDto[]>([]);
  const [reports, setReports] = useState<RecruitChannelReportDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [modalState, setModalState] = useState<ModalState>(null);

  // Form states
  const [reqId, setReqId] = useState("");
  const [postTitle, setPostTitle] = useState("");
  const [channel, setChannel] = useState<RecruitChannel>("LinkedIn");
  const [postingId, setPostingId] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [cvKey, setCvKey] = useState<string | null>(null);
  const [cvFileName, setCvFileName] = useState<string | null>(null);
  const [cvUploading, setCvUploading] = useState(false);
  const [filterStatus, setFilterStatus] = useState<string>("all");
  const [filterPostingId, setFilterPostingId] = useState<string>("all");

  const [actionLoadingId, setActionLoadingId] = useState<string | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const approved = useMemo(() => requests.filter((r) => r.status === "Approved"), [requests]);
  const openPosts = useMemo(() => postings.filter((p) => p.status === "Open"), [postings]);

  const filteredCandidates = useMemo(() => {
    return candidates.filter((c) => {
      if (filterStatus !== "all" && c.pipelineStatus !== filterStatus) return false;
      if (filterPostingId !== "all" && c.jobPostingId !== filterPostingId) return false;
      return true;
    });
  }, [candidates, filterStatus, filterPostingId]);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [r, p, c, rpt, o] = await Promise.all([
        fetchRecruitmentRequests(),
        fetchJobPostings(),
        fetchCandidates(),
        fetchRecruitChannelReport(),
        fetchOrgUnits(),
      ]);
      setRequests(r);
      setPostings(p);
      setCandidates(c);
      setReports(rpt);
      setOrgs(o);
      if (!reqId && r.find((x) => x.status === "Approved")) setReqId(r.find((x) => x.status === "Approved")!.id);
      if (!postingId && p.find((x) => x.status === "Open")) setPostingId(p.find((x) => x.status === "Open")!.id);
    } catch {
      setError("Không tải được tin tuyển / ứng viên / báo cáo.");
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

  async function onCreatePost(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !reqId) return;
    setError(null);
    try {
      await createJobPosting({
        recruitmentRequestId: reqId,
        title: postTitle || approved.find((a) => a.id === reqId)?.jobTitleName || "Tin tuyển",
        channel,
      });
      setOk("✅ Đã tạo tin tuyển.");
      setPostTitle("");
      await load();
    } catch {
      setError("Tạo tin thất bại (cần phiếu Approved).");
    }
  }

  async function onCreateCandidate(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !postingId) return;
    const v = validateCandidateForm({
      jobPostingId: postingId,
      fullName,
      email: email || undefined,
      phone: phone || undefined,
      cvStorageKey: cvKey,
    });
    if (!v.valid) { setError(v.error ?? "Lỗi validation."); return; }
    setError(null);
    try {
      await createCandidate({
        jobPostingId: postingId,
        fullName: fullName.trim(),
        email: email.trim() || null,
        phone: phone.trim() || null,
        cvStorageKey: cvKey,
      });
      setOk("✅ Đã thêm ứng viên.");
      setFullName("");
      setEmail("");
      setPhone("");
      setCvKey(null);
      setCvFileName(null);
      if (fileRef.current) fileRef.current.value = "";
      await load();
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Thêm ứng viên thất bại.");
    }
  }

  async function onScreenConfirm(candidateId: string, action: "Screen" | "ScreenReject", note: string) {
    await screenCandidate(candidateId, { action, screeningNote: note });
    setOk(action === "Screen" ? "✅ Ứng viên chuyển sang Screening." : "✅ Đã từ chối sơ loại ứng viên.");
    await load();
  }

  async function onAssignEvalOrgConfirm(candidateId: string, evalOrgUnitId: string) {
    await assignCandidateEvalOrg(candidateId, evalOrgUnitId);
    setOk("✅ Đã phân công đơn vị đánh giá.");
    await load();
  }

  async function onEvaluationConfirm(candidateId: string, body: { evalOrgUnitId?: string | null; evalScore: number; evalResult: string; evalComment?: string | null }) {
    await submitCandidateEvaluation(candidateId, body);
    setOk("✅ Đã lưu kết quả đánh giá ứng viên.");
    await load();
  }

  async function onDecideConfirm(candidateId: string, action: "Accept" | "Reject", note: string) {
    await decideCandidate(candidateId, { action, decisionNote: note });
    setOk(action === "Accept" ? "🎉 Đã chấp nhận ứng viên (Accepted)." : "✅ Đã từ chối ứng viên (Rejected).");
    await load();
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.recruit.read</p>;
  }

  const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:1111";

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Tin tuyển & ứng viên</h1>
        <p className="mt-1 text-body text-muted-foreground">
          UC_HRM_055–065 · Đánh giá · Phân công · Quyết định Accept/Reject · Pipeline · Báo cáo Kênh tuyển
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

      {/* Modals */}
      {modalState?.type === "screen" && (
        <ScreenModalPanel
          modal={modalState}
          onClose={() => setModalState(null)}
          onConfirm={(action, note) => onScreenConfirm(modalState.candidateId, action, note)}
        />
      )}
      {modalState?.type === "assignEvalOrg" && (
        <AssignEvalOrgModalPanel
          modal={modalState}
          orgs={orgs}
          onClose={() => setModalState(null)}
          onConfirm={(evalOrgUnitId) => onAssignEvalOrgConfirm(modalState.candidateId, evalOrgUnitId)}
        />
      )}
      {modalState?.type === "evaluate" && (
        <EvaluationModalPanel
          modal={modalState}
          orgs={orgs}
          onClose={() => setModalState(null)}
          onConfirm={(body) => onEvaluationConfirm(modalState.candidateId, body)}
        />
      )}
      {modalState?.type === "decide" && (
        <DecisionModalPanel
          modal={modalState}
          onClose={() => setModalState(null)}
          onConfirm={(action, note) => onDecideConfirm(modalState.candidateId, action, note)}
        />
      )}
      {modalState?.type === "careNotesHistory" && (
        <CareNotesHistoryModalPanel
          modal={modalState}
          onClose={() => setModalState(null)}
        />
      )}

      {/* Forms — Tạo tin & nhập UV */}
      {canManage && (
        <section className="grid gap-4 lg:grid-cols-2">
          {/* Form tạo tin tuyển */}
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
              <span className="text-muted-foreground">Tiêu đề tin</span>
              <input
                value={postTitle}
                onChange={(e) => setPostTitle(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                placeholder="Tuyển Senior Dev…"
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Kênh đăng</span>
              <select
                value={channel}
                onChange={(e) => setChannel(e.target.value as RecruitChannel)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
              >
                {RECRUIT_CHANNELS.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </label>
            <button type="submit" className={btn.primary} disabled={!approved.length}>
              Tạo tin
            </button>
            {!approved.length && <p className="text-meta text-muted-foreground">Chưa có phiếu Approved.</p>}
          </form>

          {/* Form nhập ứng viên */}
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
                  <option key={p.id} value={p.id}>{p.title} · {p.channel}</option>
                ))}
              </select>
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Họ tên *</span>
              <input
                required
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                placeholder="Nguyễn Văn A"
              />
            </label>
            <div className="grid gap-2 sm:grid-cols-2">
              <div className="space-y-1">
                <label className="text-body text-muted-foreground">Email</label>
                <input
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  type="email"
                  placeholder="email@example.com"
                  className="h-9 w-full rounded-md border border-border bg-background px-2 text-body"
                />
              </div>
              <div className="space-y-1">
                <label className="text-body text-muted-foreground">SĐT</label>
                <input
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  placeholder="0901234567"
                  className="h-9 w-full rounded-md border border-border bg-background px-2 text-body"
                />
              </div>
            </div>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">CV (PDF / DOC / DOCX · tối đa 10MB)</span>
              <input
                ref={fileRef}
                type="file"
                accept=".pdf,.doc,.docx,application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                className="block w-full text-meta"
                onChange={(e) => {
                  const f = e.target.files?.[0];
                  if (!f) return;
                  const v = validateCvFile(f);
                  if (!v.valid) { setError(v.error ?? "File không hợp lệ."); return; }
                  setCvUploading(true);
                  void uploadHrmFile(f)
                    .then((x) => {
                      setCvKey(x.storageKey);
                      setCvFileName(f.name);
                      setOk(`✅ Upload CV: ${f.name}`);
                    })
                    .catch(() => setError("Upload CV thất bại."))
                    .finally(() => setCvUploading(false));
                }}
              />
              {cvUploading && <span className="text-meta text-muted-foreground animate-pulse">Đang upload…</span>}
              {cvKey && !cvUploading && <span className="text-meta text-green-600">✅ {cvFileName ?? cvKey}</span>}
            </label>
            <button type="submit" className={btn.primary} disabled={!openPosts.length}>
              Thêm ứng viên
            </button>
          </form>
        </section>
      )}

      {/* Bảng tin tuyển */}
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
                  <th className="px-3 py-2">Trạng thái</th>
                  <th className="px-3 py-2" />
                </tr>
              </thead>
              <tbody>
                {postings.map((p) => (
                  <tr key={p.id} className="border-t border-border hover:bg-muted/30 transition-colors">
                    <td className="px-3 py-2 font-medium">{p.title}</td>
                    <td className="px-3 py-2 text-meta">{p.requestDocNo} · {p.jobTitleName}</td>
                    <td className="px-3 py-2">
                      <span className="rounded-full bg-blue-50 px-2 py-0.5 text-meta text-blue-700">{p.channel}</span>
                    </td>
                    <td className="px-3 py-2">
                      <span className={`rounded-full px-2 py-0.5 text-meta ${p.status === "Open" ? "bg-green-50 text-green-700" : "bg-slate-100 text-slate-500"}`}>
                        {p.status}
                      </span>
                    </td>
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
                    <td colSpan={5} className="px-3 py-4 text-center text-muted-foreground">Chưa có tin tuyển.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* Bảng pipeline ứng viên */}
      <section className="space-y-3">
        <div className="flex flex-wrap items-center gap-3">
          <h2 className="text-lead font-bold">Pipeline ứng viên</h2>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="h-8 rounded-md border border-border bg-background px-2 text-meta"
          >
            <option value="all">Tất cả trạng thái</option>
            {PIPELINE.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <select
            value={filterPostingId}
            onChange={(e) => setFilterPostingId(e.target.value)}
            className="h-8 max-w-xs rounded-md border border-border bg-background px-2 text-meta"
          >
            <option value="all">Tất cả tin tuyển</option>
            {postings.map((p) => (
              <option key={p.id} value={p.id}>{p.title}</option>
            ))}
          </select>
          <span className="ml-auto text-meta text-muted-foreground">
            {filteredCandidates.length} ứng viên
          </span>
        </div>

        <div className="overflow-x-auto rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full min-w-[950px] text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Họ tên & liên hệ</th>
                <th className="px-3 py-2">Tin tuyển</th>
                <th className="px-3 py-2">Pipeline (UC_HRM_063)</th>
                <th className="px-3 py-2">Đơn vị ĐG (UC_HRM_060)</th>
                <th className="px-3 py-2">Đánh giá (UC_HRM_061)</th>
                <th className="px-3 py-2">Quyết định (UC_HRM_062)</th>
                <th className="px-3 py-2">CV & Chăm sóc (UC_HRM_064)</th>
              </tr>
            </thead>
            <tbody>
              {filteredCandidates.map((c) => (
                <tr key={c.id} className="border-t border-border align-top hover:bg-muted/20 transition-colors">
                  <td className="px-3 py-2">
                    <div className="font-medium">{c.fullName}</div>
                    <div className="text-meta text-muted-foreground">
                      {[c.email, c.phone].filter(Boolean).join(" · ") || "Chưa có liên hệ"}
                    </div>
                  </td>

                  <td className="px-3 py-2 text-meta text-muted-foreground">{c.jobPostingTitle}</td>

                  <td className="px-3 py-2">
                    {canManage ? (
                      <div className="space-y-1">
                        <select
                          value={c.pipelineStatus}
                          disabled={actionLoadingId === c.id || c.pipelineStatus === "Accepted" || c.pipelineStatus === "Rejected"}
                          className="h-8 rounded-md border border-border bg-background px-1 text-meta font-medium"
                          onChange={(e) => {
                            const next = e.target.value;
                            if (!isValidPipelineTransition(c.pipelineStatus, next)) {
                              setError(`Không thể chuyển trực tiếp từ ${c.pipelineStatus} sang ${next}.`);
                              return;
                            }
                            setActionLoadingId(c.id);
                            void updateCandidatePipeline(c.id, next)
                              .then(load)
                              .catch((err: unknown) => setError(err instanceof Error ? err.message : "Cập nhật pipeline thất bại."))
                              .finally(() => setActionLoadingId(null));
                          }}
                        >
                          {PIPELINE.map((s) => (
                            <option key={s} value={s} disabled={!isValidPipelineTransition(c.pipelineStatus, s)}>
                              {s}
                            </option>
                          ))}
                        </select>
                        {canScreen(c.pipelineStatus) && (
                          <button
                            type="button"
                            className="block text-meta text-blue-600 hover:underline"
                            onClick={() => setModalState({ type: "screen", candidateId: c.id, candidateName: c.fullName })}
                          >
                            🔍 Sơ loại
                          </button>
                        )}
                      </div>
                    ) : (
                      <span className={`rounded-full px-2 py-0.5 text-meta font-medium ${STATUS_BADGE[c.pipelineStatus] ?? "bg-slate-100 text-slate-600"}`}>
                        {c.pipelineStatus}
                      </span>
                    )}
                  </td>

                  <td className="px-3 py-2 text-meta">
                    <div className="font-medium text-foreground">{c.evalOrgUnitName ?? "Chưa phân công"}</div>
                    {canManage && c.pipelineStatus !== "Accepted" && c.pipelineStatus !== "Rejected" && (
                      <button
                        type="button"
                        className="mt-1 text-brand-strong underline-offset-2 hover:underline text-meta"
                        onClick={() =>
                          setModalState({
                            type: "assignEvalOrg",
                            candidateId: c.id,
                            candidateName: c.fullName,
                            currentOrgId: c.evalOrgUnitId,
                          })
                        }
                      >
                        Gán đơn vị
                      </button>
                    )}
                  </td>

                  <td className="px-3 py-2 text-meta">
                    {c.evalScore != null ? (
                      <div className="space-y-0.5">
                        <div className="flex items-center gap-1.5 font-bold">
                          <span>{c.evalScore} điểm</span>
                          {c.evalResult && (
                            <span className={`rounded border px-1.5 py-0.2 text-[10px] ${EVAL_RESULT_BADGE[c.evalResult] ?? "bg-slate-100 text-slate-700"}`}>
                              {c.evalResult}
                            </span>
                          )}
                        </div>
                        {c.evalComment && (
                          <div className="max-w-[200px] truncate text-muted-foreground" title={c.evalComment}>
                            {c.evalComment}
                          </div>
                        )}
                      </div>
                    ) : (
                      <span className="text-muted-foreground">—</span>
                    )}
                    {canManage && c.pipelineStatus !== "Accepted" && c.pipelineStatus !== "Rejected" && (
                      <button
                        type="button"
                        className="mt-1 block text-brand-strong underline-offset-2 hover:underline text-meta"
                        onClick={() =>
                          setModalState({
                            type: "evaluate",
                            candidateId: c.id,
                            candidateName: c.fullName,
                            currentOrgId: c.evalOrgUnitId,
                            score: c.evalScore,
                            comment: c.evalComment,
                            result: c.evalResult,
                          })
                        }
                      >
                        📝 Đánh giá chi tiết
                      </button>
                    )}
                  </td>

                  <td className="px-3 py-2 text-meta">
                    {c.decisionNote ? (
                      <div className="space-y-0.5">
                        <span className={`font-bold ${c.pipelineStatus === "Accepted" ? "text-green-700" : "text-red-700"}`}>
                          {c.pipelineStatus === "Accepted" ? "🎉 Chấp nhận" : "❌ Từ chối"}
                        </span>
                        <div className="max-w-[200px] truncate text-muted-foreground" title={c.decisionNote}>
                          {c.decisionNote}
                        </div>
                      </div>
                    ) : (
                      <span className="text-muted-foreground">—</span>
                    )}
                    {canManage && c.pipelineStatus !== "Accepted" && (
                      <div className="mt-1 flex gap-2 text-meta">
                        <button
                          type="button"
                          className="font-medium text-green-700 hover:underline"
                          onClick={() =>
                            setModalState({
                              type: "decide",
                              candidateId: c.id,
                              candidateName: c.fullName,
                              initialAction: "Accept",
                            })
                          }
                        >
                          Chấp nhận
                        </button>
                        <span>·</span>
                        <button
                          type="button"
                          className="font-medium text-red-600 hover:underline"
                          onClick={() =>
                            setModalState({
                              type: "decide",
                              candidateId: c.id,
                              candidateName: c.fullName,
                              initialAction: "Reject",
                            })
                          }
                        >
                          Từ chối
                        </button>
                      </div>
                    )}
                  </td>

                  {/* UC_HRM_064: CV & Lịch sử chăm sóc */}
                  <td className="px-3 py-2 text-meta">
                    {c.cvStorageKey ? (
                      <a
                        className="text-brand-strong underline-offset-2 hover:underline font-medium"
                        href={`${apiBase}/api/sys/files/${encodeURIComponent(c.cvStorageKey)}`}
                        target="_blank"
                        rel="noreferrer"
                      >
                        📄 Tải CV
                      </a>
                    ) : (
                      <span className="text-muted-foreground">—</span>
                    )}
                    {c.careNotes && (
                      <div className="mt-1">
                        <button
                          type="button"
                          className="text-meta text-blue-600 underline hover:text-blue-800"
                          onClick={() => setModalState({ type: "careNotesHistory", candidateId: c.id, candidateName: c.fullName })}
                        >
                          📋 Xem lịch sử chăm sóc
                        </button>
                      </div>
                    )}
                    {canManage && (
                      <button
                        type="button"
                        disabled={actionLoadingId === c.id}
                        className="mt-1 block text-meta text-brand-strong underline-offset-2 hover:underline disabled:opacity-50"
                        onClick={() => {
                          const note = window.prompt("Ghi chú chăm sóc (tối đa 1000 ký tự)");
                          if (!note?.trim()) return;
                          setActionLoadingId(c.id);
                          void addCandidateCareNote(c.id, note.trim())
                            .then(load)
                            .catch(() => setError("Ghi chú thất bại."))
                            .finally(() => setActionLoadingId(null));
                        }}
                      >
                        + Thêm ghi chú
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {filteredCandidates.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-3 py-4 text-center text-muted-foreground">
                    Chưa có ứng viên{filterStatus !== "all" ? ` ở trạng thái "${filterStatus}"` : ""}.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      {/* UC_HRM_065: Báo cáo hiệu quả kênh tuyển dụng với đầy đủ Funnel */}
      <section className="space-y-3">
        <h2 className="text-lead font-bold">Báo cáo hiệu quả kênh tuyển dụng (UC_HRM_065)</h2>
        <div className="overflow-x-auto rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full min-w-[700px] text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5">Kênh tuyển</th>
                <th className="px-4 py-2.5 text-center">Số tin</th>
                <th className="px-4 py-2.5 text-center">Tổng Ứng viên</th>
                <th className="px-4 py-2.5 text-center">Sơ loại (Screening)</th>
                <th className="px-4 py-2.5 text-center">Đánh giá (Evaluating)</th>
                <th className="px-4 py-2.5 text-center text-green-700">Trúng tuyển (Accepted)</th>
                <th className="px-4 py-2.5 text-center text-red-700">Từ chối (Rejected)</th>
                <th className="px-4 py-2.5 text-right">Tỷ lệ chuyển đổi</th>
              </tr>
            </thead>
            <tbody>
              {reports.map((r) => (
                <tr key={r.channel} className="border-t border-border hover:bg-muted/20 transition-colors">
                  <td className="px-4 py-2.5 font-bold text-foreground">{r.channel}</td>
                  <td className="px-4 py-2.5 text-center font-medium">{r.postingCount}</td>
                  <td className="px-4 py-2.5 text-center font-bold text-brand-strong">{r.candidateCount}</td>
                  <td className="px-4 py-2.5 text-center text-blue-700 font-medium">{r.screeningCount}</td>
                  <td className="px-4 py-2.5 text-center text-yellow-700 font-medium">{r.evaluatingCount}</td>
                  <td className="px-4 py-2.5 text-center text-green-700 font-bold">{r.acceptedCount}</td>
                  <td className="px-4 py-2.5 text-center text-red-600 font-medium">{r.rejectedCount}</td>
                  <td className="px-4 py-2.5 text-right font-bold">
                    <span className={`inline-block rounded-full px-2.5 py-0.5 text-meta ${r.conversionRatePct >= 30 ? "bg-green-100 text-green-800" : r.conversionRatePct >= 10 ? "bg-yellow-100 text-yellow-800" : "bg-slate-100 text-slate-700"}`}>
                      {r.conversionRatePct}%
                    </span>
                  </td>
                </tr>
              ))}
              {reports.length === 0 && (
                <tr>
                  <td colSpan={8} className="px-4 py-4 text-center text-muted-foreground">Chưa có dữ liệu báo cáo kênh.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
