"use client";

import { useEffect, useState } from "react";
import { api } from "@/shared/api/client";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";
import { cn } from "@/shared/lib/cn";

type Cat = { id: string; code: string; name: string; isActive: boolean };
type Item = { id: string; categoryId: string; code: string; name: string; sortOrder: number; isActive: boolean };

export default function LookupsPage() {
  const { can } = usePermissions();
  const canManage = can("sys.license.manage");
  const [cats, setCats] = useState<Cat[]>([]);
  const [active, setActive] = useState<string | null>(null);
  const [items, setItems] = useState<Item[]>([]);
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [itemCode, setItemCode] = useState("");
  const [itemName, setItemName] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function loadCats() {
    const { data } = await api.get<{ data: Cat[] }>("/api/sys/lookups/categories");
    setCats(data.data);
  }

  async function loadItems(categoryId: string) {
    const { data } = await api.get<{ data: Item[] }>(`/api/sys/lookups/categories/${categoryId}/items`);
    setItems(data.data);
  }

  useEffect(() => {
    void loadCats();
  }, []);

  useEffect(() => {
    if (!active) return;
    void loadItems(active);
  }, [active]);

  const activeCat = cats.find((c) => c.id === active);

  if (!can("sys.user.read") && !canManage) {
    return <p className="text-body text-destructive">Không có quyền</p>;
  }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Danh mục dùng chung</h1>
        <p className="mt-1 text-body text-muted-foreground">Nhóm lookup · mục con theo category</p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}

      <div className="grid gap-4 lg:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 font-display text-lead font-bold">Nhóm</h2>
          {canManage && (
            <div className="mb-3 flex flex-wrap gap-2">
              <input
                placeholder="Mã"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                className={cn(field.input, "min-w-[100px] flex-1")}
              />
              <input
                placeholder="Tên"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className={cn(field.input, "min-w-[120px] flex-1")}
              />
              <button
                type="button"
                className={btn.primary}
                onClick={() => {
                  setError(null);
                  void api
                    .post("/api/sys/lookups/categories", {
                      id: "00000000-0000-0000-0000-000000000000",
                      code,
                      name,
                      isActive: true,
                    })
                    .then(() => {
                      setCode("");
                      setName("");
                      return loadCats();
                    })
                    .catch(() => setError("Thêm nhóm thất bại."));
                }}
              >
                Thêm nhóm
              </button>
            </div>
          )}
          {cats.length === 0 ? (
            <p className="py-6 text-center text-muted-foreground">Chưa có nhóm.</p>
          ) : (
            <ul className="space-y-1">
              {cats.map((c) => (
                <li key={c.id}>
                  <button
                    type="button"
                    className={cn(
                      "flex w-full items-center justify-between rounded-md px-3 py-2 text-left text-body transition-colors",
                      active === c.id
                        ? "bg-brand-muted font-semibold text-brand-strong"
                        : "hover:bg-muted"
                    )}
                    onClick={() => setActive(c.id)}
                  >
                    <span>
                      <span className="font-mono text-brand-strong">{c.code}</span>
                      <span className="text-muted-foreground"> — {c.name}</span>
                    </span>
                    <span className={statusPill(c.isActive ? "success" : "muted")}>
                      {c.isActive ? "Active" : "Off"}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 font-display text-lead font-bold">
            Mục{activeCat ? ` · ${activeCat.code}` : ""}
          </h2>
          {!active ? (
            <p className="py-8 text-center text-muted-foreground">Chọn một nhóm bên trái.</p>
          ) : (
            <>
              {canManage && (
                <div className="mb-3 flex flex-wrap gap-2">
                  <input
                    placeholder="Mã mục"
                    value={itemCode}
                    onChange={(e) => setItemCode(e.target.value)}
                    className={cn(field.input, "min-w-[100px] flex-1")}
                  />
                  <input
                    placeholder="Tên mục"
                    value={itemName}
                    onChange={(e) => setItemName(e.target.value)}
                    className={cn(field.input, "min-w-[120px] flex-1")}
                  />
                  <button
                    type="button"
                    className={btn.primary}
                    onClick={() => {
                      if (!itemCode.trim() || !itemName.trim()) return;
                      setError(null);
                      void api
                        .post("/api/sys/lookups/items", {
                          id: "00000000-0000-0000-0000-000000000000",
                          categoryId: active,
                          code: itemCode,
                          name: itemName,
                          sortOrder: items.length + 1,
                          isActive: true,
                        })
                        .then(() => {
                          setItemCode("");
                          setItemName("");
                          return loadItems(active);
                        })
                        .catch(() => setError("Thêm mục thất bại."));
                    }}
                  >
                    Thêm mục
                  </button>
                </div>
              )}
              <div className={tableWrap}>
                <table className="w-full text-body">
                  <thead className="border-b border-border bg-muted">
                    <tr>
                      <th className={th}>Mã</th>
                      <th className={th}>Tên</th>
                      <th className={th}>TT</th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.length === 0 ? (
                      <tr>
                        <td colSpan={3} className="px-3 py-6 text-center text-muted-foreground">
                          Nhóm trống.
                        </td>
                      </tr>
                    ) : (
                      items.map((i) => (
                        <tr key={i.id} className="border-t border-border">
                          <td className={cn(td, "font-mono text-brand-strong")}>{i.code}</td>
                          <td className={td}>{i.name}</td>
                          <td className={td}>
                            <span className={statusPill(i.isActive ? "success" : "muted")}>
                              {i.isActive ? "Active" : "Off"}
                            </span>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </section>
      </div>
    </div>
  );
}
