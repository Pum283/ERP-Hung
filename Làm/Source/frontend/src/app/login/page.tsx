"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { Eye, EyeOff, Loader2 } from "lucide-react";
import { fetchMe, login } from "@/shared/auth/auth-api";
import { useAuthStore } from "@/shared/auth/auth-store";

export default function LoginPage() {
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("!Abc123");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const session = useAuthStore();
  const router = useRouter();

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const tokenRes = await login(username, password);
      const me = await fetchMe();
      session.setSession(me, tokenRes.accessToken);
      router.replace("/app");
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "Đăng nhập thất bại. Kiểm tra API và SQL Server.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      {/* Left — brand panel (Digione 2-col, palette sky) */}
      <div className="relative hidden flex-col justify-between bg-login-panel p-12 text-login-panel-fg lg:flex overflow-hidden">
        <div
          className="pointer-events-none absolute inset-0 opacity-[0.07]"
          style={{
            backgroundImage:
              "linear-gradient(rgba(255,255,255,0.45) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.45) 1px, transparent 1px)",
            backgroundSize: "40px 40px",
          }}
        />
        <div
          className="pointer-events-none absolute -right-16 -top-16 h-72 w-72 rounded-full blur-3xl"
          style={{ background: "radial-gradient(circle, rgba(14,165,233,0.55), transparent 70%)" }}
        />

        <div className="relative flex items-center gap-3">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-brand text-sm font-bold text-brand-foreground shadow-sm">
            P
          </span>
          <span className="text-lead font-bold tracking-tight">
            Pum&apos;s <span className="text-accent">ERP</span>
          </span>
        </div>

        <div className="relative space-y-3 max-w-md">
          <h1 className="font-display text-display font-bold leading-tight text-login-panel-fg">
            Hệ điều hành
            <br />
            doanh nghiệp <span className="text-accent">linh hoạt</span>
          </h1>
          <p className="text-lead text-login-panel-muted">
            Một shell · module theo license · phân quyền Role / Permission / Department / JobLevel.
          </p>
        </div>

        <p className="relative text-meta text-login-panel-muted">
          © {new Date().getFullYear()} Pum&apos;s ERP
        </p>
      </div>

      {/* Right — form trên nền trắng */}
      <div className="flex items-center justify-center bg-background p-8">
        <div className="w-full max-w-md space-y-6">
          <div className="flex items-center gap-2 lg:hidden">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand text-sm font-bold text-brand-foreground">
              P
            </span>
            <span className="font-bold text-foreground">Pum&apos;s ERP</span>
          </div>

          <div className="rounded-xl border border-border bg-surface p-6 shadow-sm">
            <div className="mb-5 space-y-1">
              <h2 className="font-display text-title font-bold text-foreground">Đăng nhập</h2>
              <p className="text-body text-muted-foreground">Nhập tài khoản để tiếp tục</p>
            </div>

            <form onSubmit={onSubmit} className="space-y-5">
              <label className="block space-y-1.5">
                <span className="text-body font-medium text-foreground">Tên đăng nhập</span>
                <input
                  className="h-10 w-full rounded-md border border-border bg-input px-3 text-body text-foreground outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/25"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  autoComplete="username"
                />
              </label>

              <label className="block space-y-1.5">
                <span className="text-body font-medium text-foreground">Mật khẩu</span>
                <div className="relative">
                  <input
                    type={showPassword ? "text" : "password"}
                    className="h-10 w-full rounded-md border border-border bg-input px-3 pr-10 text-body text-foreground outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/25"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    className="absolute top-1/2 right-1 flex h-8 w-8 -translate-y-1/2 items-center justify-center text-muted-foreground hover:text-foreground"
                    onClick={() => setShowPassword((v) => !v)}
                    aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                  >
                    {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                  </button>
                </div>
              </label>

              {error && <p className="text-body text-destructive">{error}</p>}

              <button
                type="submit"
                disabled={loading}
                className="flex h-10 w-full items-center justify-center gap-2 rounded-md bg-brand text-body font-semibold text-brand-foreground shadow-sm transition hover:bg-brand-hover disabled:opacity-60"
              >
                {loading && <Loader2 size={16} className="animate-spin" />}
                {loading ? "Đang đăng nhập…" : "Đăng nhập"}
              </button>

              <p className="text-meta text-muted-foreground">Seed: admin / !Abc123</p>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
