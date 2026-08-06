"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { fetchMyMenu, type MenuItemDto } from "@/shared/api/sys-api";
import { useAuthStore } from "@/shared/auth/auth-store";
import { useActiveModuleStore } from "@/shared/modules/active-module-store";
import { getModuleMeta } from "@/shared/modules/module-meta";

function modulesFromMenu(menu: MenuItemDto[], enabled: string[]): string[] {
  const fromMenu = [...new Set(menu.map((m) => m.moduleCode.toUpperCase()))];
  const enabledSet = new Set(enabled.map((m) => m.toUpperCase()));
  const order = ["SYS", "HRM", "WF", "LMS", "AST", "FIN", "CRM", "INV"];
  const rest = fromMenu.filter((c) => enabledSet.has(c) || fromMenu.includes(c));
  return [
    ...order.filter((c) => rest.includes(c)),
    ...rest.filter((c) => !order.includes(c)).sort(),
  ];
}

export default function AppHomePage() {
  const session = useAuthStore();
  const router = useRouter();
  const setActiveModule = useActiveModuleStore((s) => s.setActiveModule);
  const [menu, setMenu] = useState<MenuItemDto[]>([]);

  useEffect(() => {
    void fetchMyMenu()
      .then(setMenu)
      .catch(() => setMenu([]));
  }, []);

  const modules = useMemo(
    () => modulesFromMenu(menu, session.enabledModules),
    [menu, session.enabledModules]
  );

  function openModule(code: string) {
    setActiveModule(code);
    const first = menu.find((m) => m.moduleCode.toUpperCase() === code && m.routePath);
    router.push(first?.routePath || getModuleMeta(code).homePath);
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Chọn module</h1>
        <p className="mt-1 text-lead text-muted-foreground">
          Xin chào{" "}
          <strong className="text-foreground">{session.displayName ?? session.username}</strong>
          {" · "}chỉ hiện module bạn có quyền truy cập
        </p>
      </div>

      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {modules.map((code) => {
          const meta = getModuleMeta(code);
          const count = menu.filter((m) => m.moduleCode.toUpperCase() === code).length;
          return (
            <button
              key={code}
              type="button"
              onClick={() => openModule(code)}
              className="group flex flex-col items-start gap-3 rounded-xl border border-border bg-surface p-4 text-left shadow-sm transition-colors hover:border-brand hover:bg-brand-muted/40"
            >
              <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-brand-muted text-brand-strong group-hover:bg-brand group-hover:text-brand-foreground">
                <meta.Icon className="h-5 w-5" />
              </span>
              <span>
                <span className="block text-[15px] font-bold text-foreground">{meta.name}</span>
                <span className="mt-0.5 block text-meta text-muted-foreground">
                  {meta.shortName} · {count} mục menu
                </span>
              </span>
            </button>
          );
        })}
      </div>

      {modules.length === 0 && (
        <p className="text-body text-muted-foreground">
          Tài khoản chưa được gán menu / module. Liên hệ quản trị viên.
        </p>
      )}
    </div>
  );
}
