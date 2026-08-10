"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPrtAccounts,
  fetchPrtTickets,
  upsertPrtTicket,
  type PrtAccountDto,
  type PrtTicketDto,
} from "@/shared/api/prt-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PrtTicketsPage() {
  const { can } = usePermissions();
  const canRead = can("prt.portal.read");
  const canManage = can("prt.portal.manage");

  const [accounts, setAccounts] = useState<PrtAccountDto[]>([]);
  const [accountId, setAccountId] = useState("");
  const [tickets, setTickets] = useState<PrtTicketDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  // Ticket Form States
  const [subject, setSubject] = useState("Hỗ trợ kỹ thuật / giao nhận");
  const [priority, setPriority] = useState("Medium");
  const [description, setDescription] = useState("Khách hàng gửi yêu cầu hỗ trợ đơn hàng.");

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const accs = await fetchPrtAccounts().catch(() => [] as PrtAccountDto[]);
      setAccounts(accs);
      const aid = accountId || accs[0]?.id || "";
      if (!accountId && aid) setAccountId(aid);
      if (!aid) {
        setTickets([]);
        return;
      }
      const tList = await fetchPrtTickets(aid);
      setTickets(tList);
    } catch (e) {
      setError((e as Error).message);
    }
  }, [accountId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 3000);
  }

  async function handleCreateTicket(e: FormEvent) {
    e.preventDefault();
    if (!accountId) return;
    try {
      await upsertPrtTicket({
        accountId,
        subject,
        content: description,
        priority,
      });
      await loadData();
      flash("Đã gửi Yêu cầu hỗ trợ (Ticket) thành công!");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem Ticket Hỗ trợ Portal.</div>;
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Trung Tâm Ticket Hỗ Trợ Khách Hàng (UC_PRT_019–020)</h1>
          <p className="mt-1 text-sm text-slate-500">
            Tạo yêu cầu phản hồi, phản ánh sự cố đơn hàng/hóa đơn và theo dõi trạng thái xử lý từ phía bộ phận CSKH.
          </p>
        </div>
        <select
          className={`${field} min-w-[280px] font-semibold text-slate-900`}
          value={accountId}
          onChange={(e) => setAccountId(e.target.value)}
        >
          <option value="">— Chọn tài khoản khách hàng —</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.displayName || a.email} ({a.customerCode ?? "Không mã"})
            </option>
          ))}
        </select>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}
      {ok && <div className="rounded-lg bg-emerald-50 p-4 text-sm font-medium text-emerald-800 border border-emerald-200">{ok}</div>}

      <div className="grid gap-6 xl:grid-cols-3">
        {/* Ticket List */}
        <section className={`${panel} xl:col-span-2 space-y-4`}>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-slate-900">Danh Sách Ticket Yêu Cầu Hỗ Trợ</h2>
            <span className="text-xs font-semibold text-slate-500">{tickets.length} yêu cầu</span>
          </div>

          {loading ? (
            <div className="p-6 text-center text-sm text-slate-500">Đang tải ticket...</div>
          ) : tickets.length === 0 ? (
            <div className="p-6 text-center text-sm text-slate-500">Chưa có ticket hỗ trợ nào.</div>
          ) : (
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã Ticket</th>
                    <th className={th}>Chủ Đề</th>
                    <th className={th}>Độ Ưu Tiên</th>
                    <th className={th}>Trạng Thái Processing</th>
                  </tr>
                </thead>
                <tbody>
                  {tickets.map((t) => (
                    <tr key={t.id} className="hover:bg-slate-50">
                      <td className={`${td} font-bold text-slate-900`}>{t.code}</td>
                      <td className={td}>
                        <div className="font-semibold text-slate-900">{t.subject}</div>
                        <div className="text-xs text-slate-500 truncate max-w-xs">{t.content || "Không có ghi chú"}</div>
                      </td>
                      <td className={td}>
                        <span className={`rounded px-2 py-0.5 text-xs font-bold ${
                          t.priority === "High" || t.priority === "Urgent"
                            ? "bg-red-100 text-red-700"
                            : "bg-blue-100 text-blue-700"
                        }`}>
                          {t.priority || "Normal"}
                        </span>
                      </td>
                      <td className={td}>
                        <span className={statusPill(t.status === "Closed" ? "success" : "brand")}>{t.status}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {/* Create Ticket Form */}
        {canManage && accountId && (
          <section className={panel}>
            <h2 className="text-base font-bold text-slate-900 border-b border-slate-100 pb-2 mb-4">🎫 Gửi Ticket Hỗ Trợ Mới</h2>
            <form onSubmit={handleCreateTicket} className="space-y-4">
              <div>
                <label className="text-xs font-semibold text-slate-700">Tiêu đề yêu cầu (*)</label>
                <input className={field} value={subject} onChange={(e) => setSubject(e.target.value)} required />
              </div>
              <div>
                <label className="text-xs font-semibold text-slate-700">Mức độ ưu tiên</label>
                <select className={field} value={priority} onChange={(e) => setPriority(e.target.value)}>
                  <option value="Low">Thấp (Low)</option>
                  <option value="Medium">Trung bình (Medium)</option>
                  <option value="High">Cao (High)</option>
                  <option value="Urgent">Khẩn cấp (Urgent)</option>
                </select>
              </div>
              <div>
                <label className="text-xs font-semibold text-slate-700">Nội dung chi tiết sự cố</label>
                <textarea
                  className={`${field} h-28 w-full p-2.5`}
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Mô tả sự cố hoặc yêu cầu hỗ trợ..."
                  required
                />
              </div>
              <button type="submit" className={`${btn.primary} w-full justify-center`}>
                📩 Gửi Yêu Cầu Hỗ Trợ
              </button>
            </form>
          </section>
        )}
      </div>
    </div>
  );
}
