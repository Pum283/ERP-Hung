"use client";

import { useEffect, useRef, useState } from "react";
import { api } from "@/shared/api/client";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { useAuthStore } from "@/shared/auth/auth-store";
import { btn } from "@/shared/ui/btn";

type TenantDto = {
  id: string;
  code: string;
  name: string;
  status: string;
  timezone: string;
  defaultLocale: string;
  defaultCurrency: string;
  logoUrl?: string | null;
};

const LOGO_SPEC = {
  formats: "PNG · JPEG · WebP · SVG",
  maxMb: 2,
  recommend: "512×512 px (vuông)",
  min: "≥ 128 px",
  maxDim: "≤ 2048 px",
};

export default function TenantPage() {
  const { can } = usePermissions();
  const patchTenantBrand = useAuthStore((s) => s.patchTenantBrand);
  const canManage = can("sys.license.manage");
  const [form, setForm] = useState<TenantDto | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [err, setErr] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  async function load() {
    const { data } = await api.get<{ data: TenantDto }>("/api/sys/tenant");
    setForm(data.data);
  }

  useEffect(() => {
    void load().catch(() => setErr("Không tải được thông tin công ty."));
  }, []);

  async function onPickLogo(file: File | null) {
    if (!file || !canManage) return;
    if (file.size > LOGO_SPEC.maxMb * 1024 * 1024) {
      setErr(`Logo tối đa ${LOGO_SPEC.maxMb} MB.`);
      return;
    }
    setUploading(true);
    setErr(null);
    setOk(null);
    try {
      const fd = new FormData();
      fd.append("file", file);
      const { data } = await api.post<{ data: TenantDto }>("/api/sys/tenant/logo", fd, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      setForm(data.data);
      setOk("Đã cập nhật logo.");
      patchTenantBrand(data.data.logoUrl, data.data.name);
    } catch {
      setErr("Upload logo thất bại. Kiểm tra định dạng / Cloudinary.");
    } finally {
      setUploading(false);
      if (inputRef.current) inputRef.current.value = "";
    }
  }

  if (!form) return <p className="text-body text-muted-foreground">Đang tải…</p>;

  return (
    <div className="mx-auto max-w-2xl space-y-5">
      <div>
        <h1 className="font-display text-title font-bold">Công ty / Tenant</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Thông tin công ty · logo thương hiệu (Cloudinary)
        </p>
      </div>
      {err && <p className="text-body text-destructive">{err}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <section className="rounded-xl border border-border bg-surface p-4">
        <h2 className="font-display text-lead font-bold">Logo</h2>
        <p className="mt-1 text-meta text-muted-foreground">
          {LOGO_SPEC.formats} · tối đa {LOGO_SPEC.maxMb} MB · khuyến nghị {LOGO_SPEC.recommend} · {LOGO_SPEC.min} ·{" "}
          {LOGO_SPEC.maxDim}. Nền trong suốt (PNG) hiển thị đẹp trên sidebar.
        </p>
        <div className="mt-4 flex flex-wrap items-center gap-4">
          <div className="flex h-20 w-20 items-center justify-center overflow-hidden rounded-xl border border-border bg-muted/40">
            {form.logoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={form.logoUrl} alt="Logo" className="h-full w-full object-contain p-1.5" />
            ) : (
              <span className="flex h-12 w-12 items-center justify-center rounded-lg bg-brand text-lg font-bold text-brand-foreground">
                P
              </span>
            )}
          </div>
          <div className="flex flex-wrap gap-2">
            <input
              ref={inputRef}
              type="file"
              accept="image/png,image/jpeg,image/webp,image/svg+xml,.png,.jpg,.jpeg,.webp,.svg"
              className="hidden"
              disabled={!canManage || uploading}
              onChange={(e) => void onPickLogo(e.target.files?.[0] ?? null)}
            />
            {canManage && (
              <>
                <button
                  type="button"
                  className={btn.primary}
                  disabled={uploading}
                  onClick={() => inputRef.current?.click()}
                >
                  {uploading ? "Đang tải…" : form.logoUrl ? "Đổi logo" : "Upload logo"}
                </button>
                {form.logoUrl && (
                  <button
                    type="button"
                    className={btn.ghost}
                    disabled={uploading}
                    onClick={() => {
                      void (async () => {
                        try {
                          setErr(null);
                          const { data } = await api.delete<{ data: TenantDto }>("/api/sys/tenant/logo");
                          setForm(data.data);
                          setOk("Đã gỡ logo.");
                          patchTenantBrand(null);
                        } catch {
                          setErr("Không gỡ được logo.");
                        }
                      })();
                    }}
                  >
                    Gỡ logo
                  </button>
                )}
              </>
            )}
          </div>
        </div>
      </section>

      <div className="space-y-3 rounded-xl border border-border bg-surface p-4">
        <label className="block text-meta">
          Mã
          <input disabled value={form.code} className="mt-1 w-full rounded-lg border border-border bg-muted/40 px-3 py-2" />
        </label>
        <label className="block text-meta">
          Tên
          <input
            value={form.name}
            disabled={!canManage}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="mt-1 w-full rounded-lg border border-border px-3 py-2"
          />
        </label>
        <label className="block text-meta">
          Múi giờ
          <input
            value={form.timezone}
            disabled={!canManage}
            onChange={(e) => setForm({ ...form, timezone: e.target.value })}
            className="mt-1 w-full rounded-lg border border-border px-3 py-2"
          />
        </label>
        <label className="block text-meta">
          Ngôn ngữ
          <input
            value={form.defaultLocale}
            disabled={!canManage}
            onChange={(e) => setForm({ ...form, defaultLocale: e.target.value })}
            className="mt-1 w-full rounded-lg border border-border px-3 py-2"
          />
        </label>
        <label className="block text-meta">
          Tiền tệ
          <input
            value={form.defaultCurrency}
            disabled={!canManage}
            onChange={(e) => setForm({ ...form, defaultCurrency: e.target.value })}
            className="mt-1 w-full rounded-lg border border-border px-3 py-2"
          />
        </label>
        {canManage && (
          <button
            type="button"
            className={btn.primary}
            onClick={() => {
              void (async () => {
                try {
                  setErr(null);
                  await api.put("/api/sys/tenant", {
                    name: form.name,
                    status: form.status,
                    timezone: form.timezone,
                    defaultLocale: form.defaultLocale,
                    defaultCurrency: form.defaultCurrency,
                  });
                  setOk("Đã lưu.");
                } catch {
                  setErr("Không lưu được.");
                }
              })();
            }}
          >
            Lưu
          </button>
        )}
      </div>
    </div>
  );
}
