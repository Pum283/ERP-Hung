"use client";

import { useParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { api } from "@/shared/api/client";
import { btn } from "@/shared/ui/btn";

type Master = {
  id: string;
  moduleCode: string;
  recordType: string;
  code: string;
  name: string;
  status: string;
};
type Doc = {
  id: string;
  docType: string;
  docNo: string;
  title: string;
  status: string;
  createdAt: string;
};

const DEFAULT_TYPES: Record<string, { master: string; doc: string }> = {
  lms: { master: "Course", doc: "Enrollment" },
  crm: { master: "Customer", doc: "Lead" },
  pos: { master: "Product", doc: "PosSale" },
  pur: { master: "Vendor", doc: "PurchaseRequest" },
  inv: { master: "Sku", doc: "StockIn" },
  log: { master: "Carrier", doc: "DeliveryOrder" },
  mfg: { master: "Bom", doc: "ProductionOrder" },
  fsm: { master: "ServiceType", doc: "ServiceTicket" },
  pjm: { master: "ProjectType", doc: "Project" },
  fin: { master: "Account", doc: "JournalEntry" },
  ast: { master: "AssetGroup", doc: "FixedAsset" },
  bi: { master: "Dataset", doc: "ReportRun" },
  prt: { master: "PortalUser", doc: "PortalTicket" },
  hrm: { master: "OrgBlock", doc: "RecruitmentRequest" },
  wf: { master: "WorkType", doc: "Ticket" },
};

export default function ModuleDay1Page() {
  const params = useParams<{ module: string }>();
  const mod = (params.module || "").toLowerCase();
  const cfg = DEFAULT_TYPES[mod] ?? { master: "Master", doc: "Document" };
  const apiMod = mod.toUpperCase();

  const [masters, setMasters] = useState<Master[]>([]);
  const [docs, setDocs] = useState<Doc[]>([]);
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [title, setTitle] = useState("");
  const [err, setErr] = useState<string | null>(null);

  const load = useCallback(async () => {
    setErr(null);
    try {
      const [m, d] = await Promise.all([
        api.get<{ data: Master[] }>(`/api/${apiMod}/masters`),
        api.get<{ data: Doc[] }>(`/api/${apiMod}/documents`),
      ]);
      setMasters(m.data.data);
      setDocs(d.data.data);
    } catch {
      setErr("Không tải được (module chưa license hoặc API lỗi).");
    }
  }, [apiMod]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <div className="space-y-4">
      <div>
        <h1 className="font-display text-title font-bold uppercase">{apiMod}</h1>
        <p className="text-body text-muted-foreground">
          Day-1 cấp 1 · masters `{cfg.master}` · chứng từ `{cfg.doc}`
        </p>
      </div>
      {err && <p className="text-destructive">{err}</p>}

      <div className="grid gap-4 md:grid-cols-2">
        <section className="rounded-xl border border-border bg-surface p-4">
          <h2 className="mb-2 font-semibold">Danh mục / Master</h2>
          <div className="mb-3 flex gap-2">
            <input className="flex-1 rounded border px-2 py-1" placeholder="code" value={code} onChange={(e) => setCode(e.target.value)} />
            <input className="flex-1 rounded border px-2 py-1" placeholder="name" value={name} onChange={(e) => setName(e.target.value)} />
            <button
              type="button"
              className={btn.primary}
              onClick={() => {
                void api
                  .post(`/api/${apiMod}/masters`, {
                    recordType: cfg.master,
                    code,
                    name,
                    status: "Active",
                  })
                  .then(() => {
                    setCode("");
                    setName("");
                    return load();
                  });
              }}
            >
              Thêm
            </button>
          </div>
          <ul className="max-h-80 space-y-1 overflow-y-auto text-body">
            {masters.map((m) => (
              <li key={m.id} className="rounded px-2 py-1 hover:bg-muted">
                <span className="text-meta text-muted-foreground">{m.recordType}</span> {m.code} — {m.name}
              </li>
            ))}
          </ul>
        </section>

        <section className="rounded-xl border border-border bg-surface p-4">
          <h2 className="mb-2 font-semibold">Chứng từ / vận hành</h2>
          <div className="mb-3 flex gap-2">
            <input className="flex-1 rounded border px-2 py-1" placeholder="Tiêu đề" value={title} onChange={(e) => setTitle(e.target.value)} />
            <button
              type="button"
              className={btn.primary}
              onClick={() => {
                void api
                  .post(`/api/${apiMod}/documents`, {
                    docType: cfg.doc,
                    title,
                    status: "Draft",
                  })
                  .then(() => {
                    setTitle("");
                    return load();
                  });
              }}
            >
              Tạo
            </button>
          </div>
          <ul className="max-h-80 space-y-1 overflow-y-auto text-body">
            {docs.map((d) => (
              <li key={d.id} className="flex items-center justify-between rounded px-2 py-1 hover:bg-muted">
                <span>
                  {d.docNo} · {d.title}{" "}
                  <span className="text-meta text-muted-foreground">{d.status}</span>
                </span>
                {d.status === "Draft" && (
                  <button
                    type="button"
                    className={btn.soft}
                    onClick={() => {
                      void api.post(`/api/${apiMod}/documents/${d.id}/transition`, { status: "Submitted" }).then(load);
                    }}
                  >
                    Submit
                  </button>
                )}
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
