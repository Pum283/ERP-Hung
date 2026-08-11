"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  approveHeadcountPlan,
  fetchHeadcountCompare,
  fetchHeadcountPlans,
  fetchHeadcountShortages,
  rejectHeadcountPlan,
  submitHeadcountPlan,
  upsertHeadcountPlan,
  type HeadcountCompareRowDto,
  type HeadcountPlanDto,
} from "@/shared/api/hrm-api";
import {
  fetchDepartments,
  fetchOrgUnits,
  type DepartmentDto,
  type OrgUnitDto,
} from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

const SCOPE_LABEL: Record<string, string> = {
  OrgUnit: "Đơn vị",
  Department: "Bộ phận",
  Shift: "Ca",
};

import { validateHeadcountPlan } from "@/shared/api/hrm-step22-helpers";
import {
  validateDeptHeadcountPlan,
  validateShiftHeadcountPlan,
} from "@/shared/api/hrm-step23-helpers";

export default function HeadcountPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [plans, setPlans] = useState<HeadcountPlanDto[]>([]);
  const [compare, setCompare] = useState<HeadcountCompareRowDto[]>([]);
  const [shortages, setShortages] = useState<HeadcountCompareRowDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [depts, setDepts] = useState<DepartmentDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  const [scopeType, setScopeType] = useState("OrgUnit");
  const [orgUnitId, setOrgUnitId] = useState("");
  const [departmentId, setDepartmentId] = useState("");
  const [shiftCode, setShiftCode] = useState("");
  const [planned, setPlanned] = useState("1");
  const [effectiveFrom, setEffectiveFrom] = useState(() => new Date().toISOString().slice(0, 10));
  const [note, setNote] = useState("");

  const deptsForOrg = useMemo(
    () => depts.filter((d) => d.orgUnitId === orgUnitId),
    [depts, orgUnitId],
  );

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [p, c, s, o, d] = await Promise.all([
        fetchHeadcountPlans(),
        fetchHeadcountCompare(),
        fetchHeadcountShortages(),
        fetchOrgUnits(),
        fetchDepartments(),
      ]);
      setPlans(p);
      setCompare(c);
      setShortages(s);
      setOrgs(o);
      setDepts(d);
      if (!orgUnitId && o[0]) setOrgUnitId(o[0].id);
    } catch {
      setError("Không tải được định biên.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onSave(e: FormEvent, submit: boolean) {
    e.preventDefault();
    if (!canManage) return;

    const plannedNum = Number(planned);
    const vBase = validateHeadcountPlan({ scopeType, orgUnitId, plannedHeadcount: plannedNum });
    if (!vBase.valid) { setError(vBase.error ?? "Lỗi validation định biên."); return; }

    if (scopeType === "Shift") {
      const vShift = validateShiftHeadcountPlan(shiftCode, plannedNum);
      if (!vShift.valid) { setError(vShift.error ?? "Lỗi ca làm việc."); return; }
    }

    if (scopeType === "Department") {
      const vDept = validateDeptHeadcountPlan(departmentId, plannedNum);
      if (!vDept.valid) { setError(vDept.error ?? "Lỗi bộ phận."); return; }
    }

    setSubmitting(true);
    setError(null);
    setOk(null);
    try {
      await upsertHeadcountPlan({
        scopeType,
        orgUnitId,
        departmentId: scopeType === "Department" ? departmentId || null : null,
        shiftCode: scopeType === "Shift" ? shiftCode.trim() || null : null,
        plannedHeadcount: Number(planned),
        effectiveFrom,
        note: note.trim() || null,
        submit,
      });
      setOk(submit ? "✅ Đã gửi duyệt định biên." : "✅ Đã lưu nháp định biên.");
      setNote("");
      await load();
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Không lưu/gửi được định biên.");
    } finally {
      setSubmitting(false);
    }
  }

  async function act(id: string, kind: "submit" | "approve" | "reject") {
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      if (kind === "submit") await submitHeadcountPlan(id);
      else if (kind === "approve") await approveHeadcountPlan(id);
      else await rejectHeadcountPlan(id);
      setOk(kind === "approve" ? "Đã duyệt." : kind === "reject" ? "Đã từ chối." : "Đã gửi duyệt.");
      await load();
    } catch {
      setError("Thao tác thất bại.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Định biên</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Khai báo theo ĐV / bộ phận / ca · so sánh thực tế · cảnh báo thiếu · duyệt thay đổi
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      {shortages.length > 0 && (
        <section className="rounded-xl border border-destructive/40 bg-destructive/5 p-4">
          <h2 className="text-lead font-bold text-destructive">
            Cảnh báo thiếu người ({shortages.length})
          </h2>
          <ul className="mt-2 space-y-1 text-body">
            {shortages.map((r, i) => (
              <li key={`${r.orgUnitId}-${r.departmentId ?? ""}-${r.shiftCode ?? ""}-${i}`}>
                {r.orgUnitName}
                {r.departmentName ? ` · ${r.departmentName}` : ""}
                {r.shiftCode ? ` · ca ${r.shiftCode}` : ""}: kế hoạch {r.planned}, thực tế {r.actual}{" "}
                (thiếu {Math.abs(r.gap)})
              </li>
            ))}
          </ul>
        </section>
      )}

      <section className="grid gap-4 lg:grid-cols-2">
        <form
          onSubmit={(e) => void onSave(e, false)}
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
        >
          <h2 className="text-lead font-bold">Khai báo định biên</h2>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Phạm vi</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={scopeType}
              onChange={(e) => setScopeType(e.target.value)}
              disabled={!canManage}
            >
              <option value="OrgUnit">Theo đơn vị</option>
              <option value="Department">Theo bộ phận</option>
              <option value="Shift">Theo ca</option>
            </select>
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Đơn vị</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={orgUnitId}
              onChange={(e) => {
                setOrgUnitId(e.target.value);
                setDepartmentId("");
              }}
              disabled={!canManage}
              required
            >
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.code} — {o.name}
                </option>
              ))}
            </select>
          </label>
          {scopeType === "Department" && (
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Bộ phận</span>
              <select
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={departmentId}
                onChange={(e) => setDepartmentId(e.target.value)}
                disabled={!canManage}
                required
              >
                <option value="">— Chọn —</option>
                {deptsForOrg.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.code} — {d.name}
                  </option>
                ))}
              </select>
            </label>
          )}
          {scopeType === "Shift" && (
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Mã ca</span>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={shiftCode}
                onChange={(e) => setShiftCode(e.target.value)}
                placeholder="VD: CA1, CA2"
                disabled={!canManage}
                required
              />
            </label>
          )}
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Số định biên</span>
            <input
              type="number"
              min={0}
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={planned}
              onChange={(e) => setPlanned(e.target.value)}
              disabled={!canManage}
              required
            />
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Hiệu lực từ</span>
            <input
              type="date"
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={effectiveFrom}
              onChange={(e) => setEffectiveFrom(e.target.value)}
              disabled={!canManage}
              required
            />
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Ghi chú</span>
            <textarea
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              rows={2}
              value={note}
              onChange={(e) => setNote(e.target.value)}
              disabled={!canManage}
            />
          </label>
          {canManage && (
            <div className="flex flex-wrap gap-2">
              <button type="submit" className={btn.secondary} disabled={submitting}>
                Lưu nháp
              </button>
              <button
                type="button"
                className={btn.primary}
                disabled={submitting}
                onClick={(e) => void onSave(e as unknown as FormEvent, true)}
              >
                Lưu & gửi duyệt
              </button>
            </div>
          )}
        </form>

        <div className="rounded-xl border border-border bg-surface p-4 shadow-sm">
          <h2 className="text-lead font-bold">So sánh thực tế vs định biên</h2>
          {loading ? (
            <p className="mt-2 text-body text-muted-foreground">Đang tải…</p>
          ) : compare.length === 0 ? (
            <p className="mt-2 text-body text-muted-foreground">Chưa có kế hoạch Approved để so sánh.</p>
          ) : (
            <div className="mt-3 overflow-x-auto">
              <table className="w-full text-left text-body">
                <thead>
                  <tr className="border-b border-border text-muted-foreground">
                    <th className="py-2 pr-2">Phạm vi</th>
                    <th className="py-2 pr-2">Kế hoạch</th>
                    <th className="py-2 pr-2">Thực tế</th>
                    <th className="py-2">Gap</th>
                  </tr>
                </thead>
                <tbody>
                  {compare.map((r, i) => (
                    <tr
                      key={`${r.scopeType}-${r.orgUnitId}-${r.departmentId ?? ""}-${r.shiftCode ?? ""}-${i}`}
                      className="border-b border-border/60"
                    >
                      <td className="py-2 pr-2">
                        {SCOPE_LABEL[r.scopeType] ?? r.scopeType}: {r.orgUnitName}
                        {r.departmentName ? ` / ${r.departmentName}` : ""}
                        {r.shiftCode ? ` / ${r.shiftCode}` : ""}
                      </td>
                      <td className="py-2 pr-2">{r.planned}</td>
                      <td className="py-2 pr-2">{r.actual}</td>
                      <td className={`py-2 ${r.shortage ? "font-semibold text-destructive" : ""}`}>
                        {r.gap > 0 ? `+${r.gap}` : r.gap}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </section>

      <section className="rounded-xl border border-border bg-surface p-4 shadow-sm">
        <div className="flex items-center justify-between gap-2">
          <h2 className="text-lead font-bold">Danh sách kế hoạch</h2>
          <button type="button" className={btn.ghost} onClick={() => void load()}>
            Làm mới
          </button>
        </div>
        {loading ? (
          <p className="mt-2 text-body text-muted-foreground">Đang tải…</p>
        ) : plans.length === 0 ? (
          <p className="mt-2 text-body text-muted-foreground">Chưa có kế hoạch định biên.</p>
        ) : (
          <div className="mt-3 overflow-x-auto">
            <table className="w-full text-left text-body">
              <thead>
                <tr className="border-b border-border text-muted-foreground">
                  <th className="py-2 pr-2">Phạm vi</th>
                  <th className="py-2 pr-2">Số lượng</th>
                  <th className="py-2 pr-2">Hiệu lực</th>
                  <th className="py-2 pr-2">TT</th>
                  <th className="py-2">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {plans.map((p) => (
                  <tr key={p.id} className="border-b border-border/60 align-top">
                    <td className="py-2 pr-2">
                      {SCOPE_LABEL[p.scopeType] ?? p.scopeType}: {p.orgUnitName}
                      {p.departmentName ? ` / ${p.departmentName}` : ""}
                      {p.shiftCode ? ` / ${p.shiftCode}` : ""}
                    </td>
                    <td className="py-2 pr-2">{p.plannedHeadcount}</td>
                    <td className="py-2 pr-2">{p.effectiveFrom.slice(0, 10)}</td>
                    <td className="py-2 pr-2">{p.status}</td>
                    <td className="py-2">
                      {canManage && (
                        <div className="flex flex-wrap gap-1">
                          {p.status === "Draft" && (
                            <button
                              type="button"
                              className={btn.secondary}
                              onClick={() => void act(p.id, "submit")}
                            >
                              Gửi duyệt
                            </button>
                          )}
                          {p.status === "Pending" && (
                            <>
                              <button
                                type="button"
                                className={btn.primary}
                                onClick={() => void act(p.id, "approve")}
                              >
                                Duyệt
                              </button>
                              <button
                                type="button"
                                className={btn.ghost}
                                onClick={() => void act(p.id, "reject")}
                              >
                                Từ chối
                              </button>
                            </>
                          )}
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
