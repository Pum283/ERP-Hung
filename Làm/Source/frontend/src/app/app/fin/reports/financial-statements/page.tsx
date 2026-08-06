"use client";

import { useEffect, useState } from "react";
import { AppShell } from "@/components/shell/AppShell";
import {
  fetchFinTrialBalance,
  fetchFinBalanceSheet,
  fetchFinProfitLoss,
  fetchFinCashFlow,
  fetchFinArApReconciliation,
  runFinClosingTransfer,
  fetchFinPeriods,
  FinTrialBalanceRowDto,
  FinBalanceSheetRowDto,
  FinProfitLossRowDto,
  FinCashFlowRowDto,
  FinArApReconciliationRowDto,
  FinPeriodDto,
} from "@/shared/api/fin-api";

export default function FinancialStatementsPage() {
  const [tab, setTab] = useState<"tb" | "pl" | "bs" | "cf" | "rec">("tb");
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [periodId, setPeriodId] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState<{ text: string; error?: boolean } | null>(null);

  // Report data
  const [tbData, setTbData] = useState<FinTrialBalanceRowDto[]>([]);
  const [bsData, setBsData] = useState<FinBalanceSheetRowDto[]>([]);
  const [plData, setPlData] = useState<FinProfitLossRowDto[]>([]);
  const [cfData, setCfData] = useState<FinCashFlowRowDto[]>([]);
  const [recType, setRecType] = useState<"AR" | "AP">("AR");
  const [recData, setRecData] = useState<FinArApReconciliationRowDto[]>([]);

  useEffect(() => {
    fetchFinPeriods().then((list) => {
      setPeriods(list);
      if (list.length > 0) setPeriodId(list[0].id);
    });
  }, []);

  useEffect(() => {
    loadTab();
  }, [tab, periodId, recType]);

  async function loadTab() {
    setLoading(true);
    setMsg(null);
    try {
      if (tab === "tb") setTbData(await fetchFinTrialBalance(periodId || undefined));
      else if (tab === "pl") setPlData(await fetchFinProfitLoss(periodId || undefined));
      else if (tab === "bs") setBsData(await fetchFinBalanceSheet(periodId || undefined));
      else if (tab === "cf") setCfData(await fetchFinCashFlow(periodId || undefined));
      else if (tab === "rec") setRecData(await fetchFinArApReconciliation(recType));
    } catch (e: any) {
      setMsg({ text: e.message || "Lỗi tải báo cáo tài chính", error: true });
    } finally {
      setLoading(false);
    }
  }

  async function handleRunClosing() {
    if (!periodId) return alert("Vui lòng chọn kỳ kế toán!");
    if (!confirm("Xác nhận chạy bút toán kết chuyển doanh thu / chi phí xác định KQKD cho kỳ này?")) return;
    setLoading(true);
    try {
      const res = await runFinClosingTransfer(periodId);
      setMsg({ text: `Đã kết chuyển thành công! Mã BT: ${res.code}` });
      loadTab();
    } catch (e: any) {
      setMsg({ text: e.message || "Không thể thực hiện kết chuyển", error: true });
    } finally {
      setLoading(false);
    }
  }

  return (
    <AppShell title="Báo cáo tài chính & Kết chuyển cuối kỳ" moduleCode="FIN">
      <div className="p-6 max-w-7xl mx-auto space-y-6">
        {/* Header Controls */}
        <div className="flex flex-wrap items-center justify-between gap-4 bg-white p-4 rounded-xl shadow-sm border border-slate-200">
          <div className="flex items-center gap-3">
            <label className="text-sm font-semibold text-slate-700">Kỳ kế toán:</label>
            <select
              value={periodId}
              onChange={(e) => setPeriodId(e.target.value)}
              className="px-3 py-1.5 text-sm rounded-lg border border-slate-300 focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Tất cả các kỳ --</option>
              {periods.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} ({p.code}) - [{p.status}]
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={handleRunClosing}
              disabled={loading || !periodId}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-medium text-sm rounded-lg shadow-sm disabled:opacity-50"
            >
              ⚡ Kết chuyển KQKD cuối kỳ
            </button>
          </div>
        </div>

        {msg && (
          <div
            className={`p-4 rounded-xl text-sm font-medium ${
              msg.error ? "bg-rose-50 text-rose-700 border border-rose-200" : "bg-emerald-50 text-emerald-700 border border-emerald-200"
            }`}
          >
            {msg.text}
          </div>
        )}

        {/* Tab Navigation */}
        <div className="border-b border-slate-200 flex gap-2">
          {[
            { id: "tb", label: "Bảng cân đối phát sinh" },
            { id: "pl", label: "P&L Kết quả kinh doanh" },
            { id: "bs", label: "Bảng cân đối kế toán" },
            { id: "cf", label: "Lưu chuyển tiền tệ" },
            { id: "rec", label: "Đối chiếu nợ AR/AP" },
          ].map((t) => (
            <button
              key={t.id}
              onClick={() => setTab(t.id as any)}
              className={`px-4 py-2.5 text-sm font-medium rounded-t-lg transition-colors ${
                tab === t.id
                  ? "bg-blue-600 text-white shadow-sm"
                  : "text-slate-600 hover:bg-slate-100"
              }`}
            >
              {t.label}
            </button>
          ))}
        </div>

        {/* Content Table */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden p-4">
          {loading ? (
            <div className="py-12 text-center text-slate-500 font-medium">Đang tính toán dữ liệu báo cáo...</div>
          ) : (
            <>
              {tab === "tb" && (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm text-left">
                    <thead className="bg-slate-50 text-slate-700 uppercase font-semibold border-b">
                      <tr>
                        <th className="p-3">Mã TK</th>
                        <th className="p-3">Tên tài khoản</th>
                        <th className="p-3">Loại</th>
                        <th className="p-3 text-right">Phát sinh Nợ</th>
                        <th className="p-3 text-right">Phát sinh Có</th>
                        <th className="p-3 text-right">Dư cuối Nợ</th>
                        <th className="p-3 text-right">Dư cuối Có</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {tbData.length === 0 ? (
                        <tr><td colSpan={7} className="p-4 text-center text-slate-400">Không có phát sinh.</td></tr>
                      ) : (
                        tbData.map((row) => (
                          <tr key={row.accountId} className="hover:bg-slate-50">
                            <td className="p-3 font-mono font-bold text-blue-600">{row.accountCode}</td>
                            <td className="p-3 font-medium text-slate-800">{row.accountName}</td>
                            <td className="p-3 text-xs uppercase px-2 py-0.5 rounded bg-slate-100 w-fit">{row.accountType}</td>
                            <td className="p-3 text-right font-mono">{row.periodDebit.toLocaleString()}</td>
                            <td className="p-3 text-right font-mono">{row.periodCredit.toLocaleString()}</td>
                            <td className="p-3 text-right font-mono text-emerald-600">{row.closingDebit.toLocaleString()}</td>
                            <td className="p-3 text-right font-mono text-indigo-600">{row.closingCredit.toLocaleString()}</td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              )}

              {tab === "pl" && (
                <div className="space-y-4">
                  <h3 className="font-bold text-slate-800 text-lg">Báo cáo Kết quả Kinh doanh (Profit & Loss)</h3>
                  <table className="w-full text-sm text-left border">
                    <thead className="bg-slate-100 font-semibold border-b">
                      <tr>
                        <th className="p-3">Mã chỉ tiêu</th>
                        <th className="p-3">Chỉ tiêu kinh doanh</th>
                        <th className="p-3 text-right">Số tiền (VNĐ)</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {plData.map((row) => (
                        <tr key={row.itemCode} className={row.itemCode === "NET" ? "bg-emerald-50 font-bold" : ""}>
                          <td className="p-3 font-mono text-slate-600">{row.itemCode}</td>
                          <td className="p-3">{row.itemName}</td>
                          <td className={`p-3 text-right font-mono text-base ${row.currentPeriodAmount >= 0 ? "text-emerald-700" : "text-rose-600"}`}>
                            {row.currentPeriodAmount.toLocaleString()} đ
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {tab === "bs" && (
                <div className="space-y-4">
                  <h3 className="font-bold text-slate-800 text-lg">Bảng cân đối kế toán (Balance Sheet)</h3>
                  <table className="w-full text-sm text-left border">
                    <thead className="bg-slate-100 font-semibold border-b">
                      <tr>
                        <th className="p-3">Phân loại</th>
                        <th className="p-3">Mã TK</th>
                        <th className="p-3">Tên tài khoản</th>
                        <th className="p-3 text-right">Giá trị còn lại (VNĐ)</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {bsData.map((row, idx) => (
                        <tr key={idx} className="hover:bg-slate-50">
                          <td className="p-3 font-semibold text-blue-700">{row.category}</td>
                          <td className="p-3 font-mono font-bold text-slate-700">{row.accountCode}</td>
                          <td className="p-3">{row.accountName}</td>
                          <td className="p-3 text-right font-mono font-semibold">{row.amount.toLocaleString()} đ</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {tab === "cf" && (
                <div className="space-y-4">
                  <h3 className="font-bold text-slate-800 text-lg">Báo cáo lưu chuyển tiền tệ (Cash Flow Statement)</h3>
                  <table className="w-full text-sm text-left border">
                    <thead className="bg-slate-100 font-semibold border-b">
                      <tr>
                        <th className="p-3">Hoạt động</th>
                        <th className="p-3">Chỉ tiêu</th>
                        <th className="p-3 text-right">Dòng tiền net (VNĐ)</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {cfData.map((row, idx) => (
                        <tr key={idx} className="hover:bg-slate-50">
                          <td className="p-3 font-semibold text-indigo-700">{row.activityType}</td>
                          <td className="p-3">{row.itemName}</td>
                          <td className="p-3 text-right font-mono font-bold text-emerald-600">{row.amount.toLocaleString()} đ</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {tab === "rec" && (
                <div className="space-y-4">
                  <div className="flex items-center gap-4">
                    <span className="font-semibold text-slate-700 text-sm">Loại đối chiếu:</span>
                    <button
                      onClick={() => setRecType("AR")}
                      className={`px-3 py-1 text-xs font-bold rounded ${recType === "AR" ? "bg-blue-600 text-white" : "bg-slate-100 text-slate-600"}`}
                    >
                      Phải thu khách hàng (AR - 131)
                    </button>
                    <button
                      onClick={() => setRecType("AP")}
                      className={`px-3 py-1 text-xs font-bold rounded ${recType === "AP" ? "bg-blue-600 text-white" : "bg-slate-100 text-slate-600"}`}
                    >
                      Phải trả nhà cung cấp (AP - 331)
                    </button>
                  </div>

                  <table className="w-full text-sm text-left border">
                    <thead className="bg-slate-100 font-semibold border-b">
                      <tr>
                        <th className="p-3">Mã đối tác</th>
                        <th className="p-3 text-right">Sổ chi tiết (Subledger)</th>
                        <th className="p-3 text-right">Sổ cái GL (131/331)</th>
                        <th className="p-3 text-right">Chênh lệch</th>
                        <th className="p-3 text-center">Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {recData.map((row) => (
                        <tr key={row.partnerCode} className="hover:bg-slate-50">
                          <td className="p-3 font-mono font-bold text-slate-800">{row.partnerCode}</td>
                          <td className="p-3 text-right font-mono">{row.subledgerBalance.toLocaleString()}</td>
                          <td className="p-3 text-right font-mono">{row.generalLedgerBalance.toLocaleString()}</td>
                          <td className={`p-3 text-right font-mono font-bold ${row.variance === 0 ? "text-slate-600" : "text-rose-600"}`}>
                            {row.variance.toLocaleString()}
                          </td>
                          <td className="p-3 text-center">
                            <span
                              className={`px-2 py-0.5 text-xs font-bold rounded ${
                                row.isReconciled ? "bg-emerald-100 text-emerald-800" : "bg-rose-100 text-rose-800"
                              }`}
                            >
                              {row.isReconciled ? " Khớp" : " Lệch"}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </AppShell>
  );
}
