"use client";

import { useEffect, useState } from "react";
import {
  fetchDepartments,
  fetchOrgUnits,
  type DepartmentDto,
  type OrgUnitDto,
} from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";

export default function OrgPage() {
  const { can } = usePermissions();
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [depts, setDepts] = useState<DepartmentDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  const canRead = can("sys.user.read");

  useEffect(() => {
    if (!canRead) return;
    Promise.all([fetchOrgUnits(), fetchDepartments()])
      .then(([o, d]) => {
        setOrgs(o);
        setDepts(d);
      })
      .catch(() => setError("Không tải được tổ chức / phòng ban."));
  }, [canRead]);

  if (!can("sys.user.read")) {
    return <p className="text-body text-destructive">Không có quyền sys.user.read</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Tổ chức</h1>
        <p className="mt-1 text-body text-muted-foreground">Org unit · Department (Day-1 xem)</p>
      </div>
      {error && <p className="text-body text-destructive">{error}</p>}

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Đơn vị</h2>
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-semibold">Mã</th>
                <th className="px-4 py-2.5 font-semibold">Tên</th>
                <th className="px-4 py-2.5 font-semibold">Loại</th>
                <th className="px-4 py-2.5 font-semibold">Active</th>
              </tr>
            </thead>
            <tbody>
              {orgs.map((o) => (
                <tr key={o.id} className="border-t border-border">
                  <td className="px-4 py-2.5 font-mono text-meta font-semibold text-brand-strong">
                    {o.code}
                  </td>
                  <td className="px-4 py-2.5 font-medium">{o.name}</td>
                  <td className="px-4 py-2.5">{o.unitType}</td>
                  <td className="px-4 py-2.5">{o.isActive ? "Yes" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="space-y-2">
        <h2 className="text-lead font-bold">Phòng ban</h2>
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-semibold">Mã</th>
                <th className="px-4 py-2.5 font-semibold">Tên</th>
                <th className="px-4 py-2.5 font-semibold">Active</th>
              </tr>
            </thead>
            <tbody>
              {depts.map((d) => (
                <tr key={d.id} className="border-t border-border">
                  <td className="px-4 py-2.5 font-mono text-meta font-semibold text-brand-strong">
                    {d.code}
                  </td>
                  <td className="px-4 py-2.5 font-medium">{d.name}</td>
                  <td className="px-4 py-2.5">{d.isActive ? "Yes" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
