"use client";

import React, { useEffect, useState } from "react";
import {
  fetchIpRules,
  upsertIpRule,
  deleteIpRule,
  checkIpRule,
  type SysIpRuleDto,
} from "@/shared/api/sys-api";
import { validateIpRuleForm } from "@/shared/api/sys-notif-scan-export-ip-helpers";
import { RefreshCw, Shield } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function IpRulesPage() {
  const [rows, setRows] = useState<SysIpRuleDto[]>([]);
  const [ip, setIp] = useState("10.0.0.0/8");
  const [ruleType, setRuleType] = useState("Allow");
  const [desc, setDesc] = useState("");
  const [checkIp, setCheckIp] = useState("10.1.2.3");
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      setRows(await fetchIpRules());
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function onSave(e: React.FormEvent) {
    e.preventDefault();
    const v = validateIpRuleForm({ ipAddressOrCidr: ip, ruleType });
    if (!v.isValid) {
      setError(v.error ?? "Form không hợp lệ.");
      return;
    }
    try {
      setError(null);
      await upsertIpRule({ ipAddressOrCidr: ip, ruleType, description: desc, isActive: true });
      setMsg("Đã lưu IP rule.");
      setDesc("");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onDelete(id: string) {
    try {
      await deleteIpRule(id);
      setMsg("Đã xóa rule.");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onCheck() {
    try {
      const r = await checkIpRule(checkIp.trim());
      setMsg(r.allowed ? `Cho phép (${r.reason})` : `Từ chối (${r.reason})`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="max-w-4xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground flex items-center gap-2">
            <Shield className="w-6 h-6 text-brand" /> IP allow/deny (UC_SYS_082)
          </h1>
          <p className="text-body text-muted-foreground mt-1">
            Deny thắng Allow. Có Allow → allowlist. Áp dụng khi đăng nhập.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <form onSubmit={(e) => void onSave(e)} className={`${panel} space-y-3`}>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <input className={field} value={ip} onChange={(e) => setIp(e.target.value)} placeholder="IP/CIDR" />
          <select className={field} value={ruleType} onChange={(e) => setRuleType(e.target.value)}>
            <option value="Allow">Allow</option>
            <option value="Deny">Deny</option>
          </select>
          <input className={field} value={desc} onChange={(e) => setDesc(e.target.value)} placeholder="Mô tả" />
        </div>
        <button type="submit" className={btn.primary}>Thêm rule</button>
      </form>

      <div className={`${panel} flex flex-wrap gap-2 items-end`}>
        <label className="block space-y-1 flex-1">
          <span className="text-xs text-muted-foreground">Kiểm tra IP</span>
          <input className={field} value={checkIp} onChange={(e) => setCheckIp(e.target.value)} />
        </label>
        <button type="button" className={btn.soft} onClick={() => void onCheck()}>Check</button>
      </div>

      <div className="bg-surface shadow rounded-xl border divide-y">
        {loading ? (
          <div className="p-4 text-sm text-muted-foreground">Đang tải…</div>
        ) : rows.length === 0 ? (
          <div className="p-4 text-sm text-muted-foreground">Chưa có rule — mọi IP được phép.</div>
        ) : (
          rows.map((r) => (
            <div key={r.id} className="p-4 flex items-center justify-between gap-3 text-sm">
              <div>
                <div className="font-mono">{r.ipAddressOrCidr} · {r.ruleType}</div>
                <div className="text-muted-foreground">{r.description || "—"} {r.isActive ? "" : "(inactive)"}</div>
              </div>
              <button type="button" className={btn.soft} onClick={() => void onDelete(r.id)}>Xóa</button>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
