"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import {
  ArrowLeftRight,
  Award,
  Banknote,
  Bell,
  Briefcase,
  Building2,
  CalendarDays,
  Check,
  ChevronDown,
  ClipboardList,
  Clock,
  Contact,
  DoorOpen,
  Factory,
  FileSignature,
  FileText,
  Fingerprint,
  FolderKanban,
  BookOpen,
  Boxes,
  ClipboardCheck,
  GraduationCap,
  HandCoins,
  Headset,
  Package,
  Percent,
  ShoppingCart,
  Store,
  Tag,
  TrendingUp,
  Truck,
  Warehouse,
  Wrench,
  Inbox,
  KeyRound,
  Landmark,
  Layers,
  LayoutDashboard,
  LogOut,
  Map,
  MessageSquare,
  PlayCircle,
  Receipt,
  Search,
  Settings,
  Shield,
  UserCheck,
  UserPlus,
  UserX,
  Users,
  Wallet,
  UsersRound,
  type LucideIcon,
} from "lucide-react";
import { changePassword, fetchMe, logoutApi } from "@/shared/auth/auth-api";
import { useAuthStore } from "@/shared/auth/auth-store";
import { MessengerDock } from "@/components/msg/MessengerDock";
import { MessengerDropdown } from "@/components/msg/MessengerDropdown";
import { NotificationDropdown } from "@/components/notify/NotificationDropdown";
import { fetchUnreadCount } from "@/shared/api/msg-api";
import { fetchNotificationUnreadCount } from "@/shared/api/notify-api";
import { fetchMyMenu, type MenuItemDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { useMessengerStore } from "@/shared/msg/messenger-store";
import { useActiveModuleStore } from "@/shared/modules/active-module-store";
import { getModuleMeta, moduleFromPath } from "@/shared/modules/module-meta";
import { subscribeConversationUpdated, subscribeMsgReceived } from "@/shared/realtime/msg-hub";
import { cn } from "@/shared/lib/cn";

/** Icon theo mã menu — ưu tiên hơn title (tránh “Nghỉ phép” / “Nghỉ việc” trùng). */
const MENU_ICON_BY_CODE: Record<string, LucideIcon> = {
  HRM_DASHBOARD: LayoutDashboard,
  HRM_EMP: Users,
  HRM_LEAVE: CalendarDays,
  HRM_CONTRACT: FileSignature,
  HRM_RECRUIT: Briefcase,
  HRM_CANDIDATES: Contact,
  HRM_ONBOARD: UserCheck,
  HRM_HEADCOUNT: UsersRound,
  HRM_SHIFTS: Clock,
  HRM_TRANSFERS: ArrowLeftRight,
  HRM_ATTENDANCE: Fingerprint,
  HRM_PAYROLL: Banknote,
  HRM_REWARDS: Award,
  HRM_OFFBOARD: DoorOpen,
  LMS_COURSES: BookOpen,
  LMS_EXAMS: ClipboardCheck,
  LMS_CATALOG: PlayCircle,
  LMS_CLASSES: GraduationCap,
  LMS_CERTS: Award,
  LMS_INSTRUCTORS: UserCheck,
  LMS_REPORTS: FileText,
  CRM_HOME: UserPlus,
  CRM_CUSTOMERS: Contact,
  CRM_LEADS: UserPlus,
  CRM_OPPORTUNITIES: Briefcase,
  CRM_QUOTES: FileText,
  CRM_ORDERS: ShoppingCart,
  POS_HOME: ShoppingCart,
  POS_STORES: Store,
  POS_CATALOG: Package,
  POS_SHIFTS: Banknote,
  POS_SELL: ShoppingCart,
  POS_PROMOS: Tag,
  POS_REPORTS: FileText,
  PUR_HOME: Truck,
  PUR_VENDORS: Truck,
  PUR_ORDERS: ShoppingCart,
  PUR_RECEIPTS: ClipboardList,
  PUR_INVOICES: FileText,
  PUR_REPORTS: FileText,
  INV_HOME: Boxes,
  INV_ITEMS: Boxes,
  INV_WAREHOUSES: Warehouse,
  INV_STOCK: Boxes,
  INV_TRANSFERS: ArrowLeftRight,
  INV_STOCKTAKES: ClipboardCheck,
  INV_REPORTS: FileText,
  LOG_HOME: Truck,
  LOG_CARRIERS: Truck,
  LOG_DELIVERIES: Map,
  LOG_COD: Banknote,
  LOG_RETURNS: Package,
  MFG_HOME: Factory,
  MFG_CATALOG: Factory,
  MFG_ORDERS: ClipboardList,
  MFG_REPORTS: FileText,
  FSM_HOME: Headset,
  FSM_CATALOG: Wrench,
  FSM_TICKETS: Headset,
  FSM_PARTS: Package,
  FSM_REPORTS: FileText,
  PJM_HOME: Briefcase,
  PJM_CATALOG: FolderKanban,
  PJM_PROJECTS: Briefcase,
  PJM_REPORTS: FileText,
  FIN_HOME: Banknote,
  FIN_CATALOG: BookOpen,
  FIN_JOURNALS: Banknote,
  FIN_CASH: Wallet,
  FIN_BANK: Landmark,
  FIN_AP: Receipt,
  FIN_AR: HandCoins,
  FIN_TAX: Percent,
  FIN_REVENUE: TrendingUp,
  AST_HOME: Building2,
  AST_CATALOG: Layers,
  AST_ASSETS: Building2,
  AST_MOVEMENTS: ArrowLeftRight,
  AST_STOCKTAKES: ClipboardCheck,
  AST_REPORTS: FileText,
  BI_HOME: FileText,
  BI_CATALOG: LayoutDashboard,
  BI_REPORTS: FileText,
  BI_KPI: LayoutDashboard,
  PRT_HOME: Store,
  PRT_ACCOUNTS: UserCheck,
  PRT_PORTAL: Store,
  PRT_PACKAGE: Layers,
  SYS_USERS: Users,
  SYS_ROLES: Shield,
  SYS_PERMISSIONS: KeyRound,
  SYS_ORG: Building2,
  SYS_MSG: MessageSquare,
  SYS_TENANT: Building2,
  SYS_LOOKUPS: Layers,
  SYS_AUDIT_LOGIN: Shield,
  WF_TASKS: Inbox,
  WF_WORK: ClipboardList,
  WF_DELEGATION: UserPlus,
  WF_DASHBOARD: LayoutDashboard,
};

const MENU_ICON_BY_KEY: Record<string, LucideIcon> = {
  users: Users,
  "user-plus": UserPlus,
  "user-check": UserCheck,
  "user-x": UserX,
  "users-round": UsersRound,
  shield: Shield,
  building: Building2,
  inbox: Inbox,
  calendar: CalendarDays,
  "calendar-days": CalendarDays,
  file: FileText,
  "file-text": FileText,
  "file-signature": FileSignature,
  message: MessageSquare,
  layers: Layers,
  chart: LayoutDashboard,
  "layout-dashboard": LayoutDashboard,
  briefcase: Briefcase,
  contact: Contact,
  clock: Clock,
  "arrow-left-right": ArrowLeftRight,
  fingerprint: Fingerprint,
  banknote: Banknote,
  wallet: Wallet,
  landmark: Landmark,
  receipt: Receipt,
  "hand-coins": HandCoins,
  percent: Percent,
  "trending-up": TrendingUp,
  award: Award,
  "door-open": DoorOpen,
  "graduation-cap": GraduationCap,
  "book-open": BookOpen,
  "play-circle": PlayCircle,
  "clipboard-check": ClipboardCheck,
  store: Store,
  package: Package,
  boxes: Boxes,
  warehouse: Warehouse,
  truck: Truck,
  map: Map,
  factory: Factory,
  wrench: Wrench,
  headset: Headset,
  "folder-kanban": FolderKanban,
  "shopping-cart": ShoppingCart,
  tag: Tag,
  key: KeyRound,
  clipboard: ClipboardList,
  "clipboard-list": ClipboardList,
};

function menuIcon(code: string, title: string, icon?: string | null): LucideIcon {
  const byCode = MENU_ICON_BY_CODE[code.toUpperCase()];
  if (byCode) return byCode;

  const key = (icon ?? "").trim().toLowerCase();
  if (key && MENU_ICON_BY_KEY[key]) return MENU_ICON_BY_KEY[key];

  const c = `${code} ${title}`.toUpperCase();
  if (c.includes("USER") || c.includes("EMP") || c.includes("HỒ SƠ")) return Users;
  if (c.includes("ROLE") || c.includes("VAI") || c.includes("PERMISSION") || c.includes("QUYỀN")) return Shield;
  if (c.includes("ORG") || c.includes("TỔ CHỨC")) return Building2;
  if (c.includes("WF") || c.includes("TASK") || c.includes("PHÊ DUYỆT")) return Inbox;
  if (c.includes("LEAVE") || c.includes("PHÉP")) return CalendarDays;
  if (c.includes("OFFBOARD") || c.includes("NGHỈ VIỆC")) return DoorOpen;
  if (c.includes("CONTRACT") || c.includes("HỢP ĐỒNG")) return FileSignature;
  if (c.includes("PAYROLL") || c.includes("LƯƠNG")) return Banknote;
  if (c.includes("ATTEND") || c.includes("CHẤM")) return Fingerprint;
  if (c.includes("SHIFT") || c.includes("CA ")) return Clock;
  if (c.includes("TRANSFER") || c.includes("ĐIỀU ĐỘNG")) return ArrowLeftRight;
  if (c.includes("REWARD") || c.includes("KHEN")) return Award;
  if (c.includes("RECRUIT") || c.includes("TUYỂN")) return Briefcase;
  if (c.includes("CANDIDATE") || c.includes("ỨNG VIÊN")) return Contact;
  if (c.includes("ONBOARD")) return UserCheck;
  if (c.includes("HEADCOUNT") || c.includes("ĐỊNH BIÊN")) return UsersRound;
  if (c.includes("MSG") || c.includes("TIN NHẮN") || c.includes("MESSAGE")) return MessageSquare;
  return LayoutDashboard;
}

/** Module user được vào = có ít nhất 1 menu item (đã lọc license + permission phía API). */
function accessibleModuleCodes(menu: MenuItemDto[], enabledModules: string[]): string[] {
  const fromMenu = [...new Set(menu.map((m) => m.moduleCode.toUpperCase()))];
  const enabled = new Set(enabledModules.map((m) => m.toUpperCase()));
  // Ưu tiên thứ tự ổn định: SYS → HRM → WF → còn lại
  const order = ["SYS", "HRM", "WF", "LMS", "AST", "FIN", "CRM", "INV"];
  const rest = fromMenu.filter((c) => enabled.has(c) || fromMenu.includes(c));
  const ranked = [
    ...order.filter((c) => rest.includes(c)),
    ...rest.filter((c) => !order.includes(c)).sort(),
  ];
  return ranked;
}

function firstPathOfModule(menu: MenuItemDto[], moduleCode: string): string {
  const item = menu.find((m) => m.moduleCode.toUpperCase() === moduleCode && m.routePath);
  if (item?.routePath) return item.routePath;
  return getModuleMeta(moduleCode).homePath;
}

export function AppShell({ children }: { children: React.ReactNode }) {
  const session = useAuthStore();
  const { can } = usePermissions();
  const pathname = usePathname();
  const router = useRouter();
  const { activeModule, setActiveModule, hydrate } = useActiveModuleStore();

  const [menu, setMenu] = useState<MenuItemDto[]>([]);
  const [ready, setReady] = useState(false);
  const [search, setSearch] = useState("");
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [moduleOpen, setModuleOpen] = useState(false);
  const [unreadMsg, setUnreadMsg] = useState(0);
  const [unreadNotif, setUnreadNotif] = useState(0);
  const [notifOpen, setNotifOpen] = useState(false);
  const [pwdOpen, setPwdOpen] = useState(false);
  const [pwdCur, setPwdCur] = useState("");
  const [pwdNew, setPwdNew] = useState("");
  const [pwdErr, setPwdErr] = useState<string | null>(null);
  const canMsg = can("sys.msg.read");
  const dropdownOpen = useMessengerStore((s) => s.dropdownOpen);
  const setDropdownOpen = useMessengerStore((s) => s.setDropdownOpen);
  const toggleDropdown = useMessengerStore((s) => s.toggleDropdown);

  useEffect(() => {
    hydrate();
  }, [hydrate]);

  useEffect(() => {
    if (!canMsg || !ready) return;
    let cancelled = false;
    const refresh = () => {
      void fetchUnreadCount()
        .then((n) => {
          if (!cancelled) setUnreadMsg(n);
        })
        .catch(() => {});
    };
    refresh();
    const unsub1 = subscribeMsgReceived(() => refresh());
    const unsub2 = subscribeConversationUpdated(() => refresh());
    return () => {
      cancelled = true;
      unsub1();
      unsub2();
    };
  }, [canMsg, ready, pathname]);

  useEffect(() => {
    if (!ready) return;
    let cancelled = false;
    void fetchNotificationUnreadCount()
      .then((n) => {
        if (!cancelled) setUnreadNotif(n);
      })
      .catch(() => {});
    return () => {
      cancelled = true;
    };
  }, [ready, pathname]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      const token = localStorage.getItem("access_token");
      if (!token) {
        router.replace("/login");
        return;
      }
      try {
        if (!session.userId) {
          const me = await fetchMe();
          session.setSession(me, token);
        }
        const items = await fetchMyMenu();
        if (!cancelled) setMenu(items);
      } catch {
        session.clear();
        router.replace("/login");
        return;
      } finally {
        if (!cancelled) setReady(true);
      }
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const modules = useMemo(
    () => accessibleModuleCodes(menu, session.enabledModules),
    [menu, session.enabledModules]
  );

  // Đồng bộ module đang chọn với URL + danh sách được phép
  useEffect(() => {
    if (!ready || modules.length === 0) return;

    const fromPath = moduleFromPath(pathname);
    if (fromPath && modules.includes(fromPath)) {
      if (activeModule !== fromPath) setActiveModule(fromPath);
      return;
    }

    if (activeModule && modules.includes(activeModule)) return;

    const fallback = modules[0];
    setActiveModule(fallback);
  }, [ready, pathname, modules, activeModule, setActiveModule]);

  if (!ready) {
    return (
      <div className="flex h-screen w-screen items-center justify-center bg-background">
        <div className="flex items-center gap-3 text-muted-foreground">
          <div className="h-5 w-5 animate-spin rounded-full border-2 border-brand border-t-transparent" />
          <span className="text-body font-medium">Đang tải Pum&apos;s ERP…</span>
        </div>
      </div>
    );
  }

  const currentCode = activeModule && modules.includes(activeModule) ? activeModule : modules[0] ?? "SYS";
  const currentMeta = getModuleMeta(currentCode);

  const moduleRoot = `/app/${currentCode.toLowerCase()}`;
  const moduleMenu = menu.filter((m) => {
    if (m.moduleCode.toUpperCase() !== currentCode) return false;
    // Ẩn mục “root module” kiểu /app/sys (chỉ là redirect, không phải chức năng)
    const route = (m.routePath || "").replace(/\/$/, "");
    if (!route || route === moduleRoot || route === "/app") return false;
    if (m.permissionCode && !(can(m.permissionCode) || session.bypassDataScope)) return false;
    if (!search.trim()) return true;
    const q = search.trim().toLowerCase();
    return m.title.toLowerCase().includes(q) || m.code.toLowerCase().includes(q);
  });
  const moduleHrefs = moduleMenu.map((m) => m.routePath || "").filter(Boolean);

  const displayName = session.displayName ?? session.username ?? "User";
  const initials = displayName
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);

  /** Active = khớp dài nhất trong menu module (tránh /app/sys và /app/sys/users cùng sáng). */
  function isNavActive(href: string, candidates: string[]) {
    const paths = candidates.filter(Boolean).sort((a, b) => b.length - a.length);
    const best = paths.find((p) => pathname === p || pathname.startsWith(`${p}/`));
    return best === href;
  }

  function switchModule(code: string) {
    setActiveModule(code);
    setModuleOpen(false);
    setSearch("");
    const path = firstPathOfModule(menu, code);
    router.push(path);
  }

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      <aside
        className="fixed top-0 left-0 z-50 flex h-screen shrink-0 flex-col overflow-hidden border-r border-sidebar-border bg-sidebar text-sidebar-foreground shadow-sm select-none"
        style={{ width: "var(--sidebar-width)" }}
        aria-label="Navigation"
      >
        {/* Brand */}
        <Link
          href="/app"
          className="flex h-header w-full shrink-0 items-center gap-2 border-b border-sidebar-border px-3 hover:bg-sidebar-hover"
          title="Về trang chọn module"
        >
          {session.tenantLogoUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={session.tenantLogoUrl}
              alt=""
              className="h-8 w-8 shrink-0 rounded-lg object-contain bg-muted/40"
            />
          ) : (
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-brand text-sm font-bold text-brand-foreground shadow-sm">
              P
            </span>
          )}
          <div className="flex min-w-0 flex-1 flex-col items-start">
            <span className="w-full truncate text-left text-[13px] font-bold text-foreground">
              {session.tenantName || "Pum's ERP"}
            </span>
            <span className="w-full truncate text-left text-[10px] font-medium text-muted-foreground">
              Hệ thống quản trị
            </span>
          </div>
        </Link>

        {/* Module switcher */}
        <div className="relative shrink-0 border-b border-sidebar-border p-2">
          <button
            type="button"
            onClick={() => setModuleOpen((o) => !o)}
            className="flex h-11 w-full items-center gap-2 rounded-lg border border-border bg-muted/60 px-2.5 text-left transition-colors hover:bg-sidebar-hover"
            aria-expanded={moduleOpen}
            aria-haspopup="listbox"
          >
            <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-brand-muted text-brand-strong">
              <currentMeta.Icon className="h-3.5 w-3.5" />
            </span>
            <span className="min-w-0 flex-1">
              <span className="block truncate text-[11px] font-semibold text-muted-foreground">
                Module
              </span>
              <span className="block truncate text-[13px] font-bold text-foreground">
                {currentMeta.name}
              </span>
            </span>
            <Layers className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
            <ChevronDown
              className={cn(
                "h-3.5 w-3.5 shrink-0 text-muted-foreground transition-transform",
                moduleOpen && "rotate-180"
              )}
            />
          </button>

          {moduleOpen && (
            <>
              <button
                type="button"
                className="fixed inset-0 z-40 cursor-default"
                aria-label="Đóng chọn module"
                onClick={() => setModuleOpen(false)}
              />
              <div
                role="listbox"
                className="absolute top-[calc(100%-4px)] right-2 left-2 z-50 max-h-72 overflow-y-auto rounded-xl border border-border bg-surface py-1 shadow-md"
              >
                <p className="px-3 py-1.5 text-[10px] font-semibold tracking-wide text-muted-foreground uppercase">
                  Module bạn có quyền
                </p>
                {modules.map((code) => {
                  const meta = getModuleMeta(code);
                  const selected = code === currentCode;
                  return (
                    <button
                      key={code}
                      type="button"
                      role="option"
                      aria-selected={selected}
                      onClick={() => switchModule(code)}
                      className={cn(
                        "flex w-full items-center gap-2 px-2.5 py-2 text-left transition-colors",
                        selected ? "bg-brand-muted" : "hover:bg-muted"
                      )}
                    >
                      <span
                        className={cn(
                          "flex h-7 w-7 shrink-0 items-center justify-center rounded-md",
                          selected
                            ? "bg-brand text-brand-foreground"
                            : "bg-muted text-muted-foreground"
                        )}
                      >
                        <meta.Icon className="h-3.5 w-3.5" />
                      </span>
                      <span className="min-w-0 flex-1">
                        <span className="block truncate text-[12px] font-bold text-foreground">
                          {meta.name}
                        </span>
                        <span className="block truncate text-[10px] text-muted-foreground">
                          {meta.shortName}
                        </span>
                      </span>
                      {selected && <Check className="h-3.5 w-3.5 shrink-0 text-brand-strong" />}
                    </button>
                  );
                })}
                {modules.length === 0 && (
                  <p className="px-3 py-3 text-[12px] text-muted-foreground">
                    Không có module khả dụng.
                  </p>
                )}
              </div>
            </>
          )}
        </div>

        <div className="shrink-0 p-2">
          <div className="flex h-[30px] items-center gap-2 rounded-lg border border-border bg-muted px-2.5 text-muted-foreground">
            <Search className="h-3.5 w-3.5 shrink-0" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={`Tìm trong ${currentMeta.shortName}…`}
              className="w-full border-0 bg-transparent text-xs text-foreground outline-none placeholder:text-muted-foreground"
            />
          </div>
        </div>

        <div className="mt-1 min-h-0 flex-1 overflow-x-hidden overflow-y-auto px-1.5">
          <nav className="flex flex-col gap-0.5 pb-2">
            <p className="px-2 py-1 text-[10px] font-semibold tracking-wide text-sidebar-muted uppercase">
              Menu · {currentMeta.shortName}
            </p>

            {moduleMenu.map((m) => {
              const href = m.routePath || "/app";
              const active = isNavActive(href, moduleHrefs);
              const Icon = menuIcon(m.code, m.title, m.icon);
              return (
                <Link
                  key={m.id}
                  href={href}
                  className={cn(
                    "flex h-8 w-full items-center gap-2 rounded-md border-l-2 px-2 text-[12px] font-medium transition-colors",
                    active
                      ? "border-brand bg-sidebar-active-bg font-bold text-sidebar-active-fg"
                      : "border-transparent text-sidebar-foreground hover:bg-sidebar-hover hover:text-foreground"
                  )}
                >
                  <Icon
                    className={cn(
                      "h-3.5 w-3.5 shrink-0",
                      active ? "text-brand" : "text-muted-foreground"
                    )}
                  />
                  <span className="flex-1 truncate text-left">{m.title}</span>
                </Link>
              );
            })}

            {moduleMenu.length === 0 && (
              <p className="px-2 py-3 text-[12px] text-muted-foreground">
                Không có menu trong module này.
              </p>
            )}
          </nav>
        </div>

        <div className="relative shrink-0 border-t border-sidebar-border bg-muted/50 p-1.5">
          <button
            type="button"
            onClick={() => setUserMenuOpen((o) => !o)}
            className="mx-0.5 mb-0.5 flex h-11 w-[calc(100%-4px)] cursor-pointer items-center gap-2.5 rounded-lg px-2.5 text-left transition-colors hover:bg-sidebar-hover"
          >
            <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-brand text-[10px] font-bold text-brand-foreground shadow-sm">
              {initials}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-[12px] font-bold leading-tight text-foreground">
                {displayName}
              </p>
              <p className="truncate text-[10px] font-medium leading-tight text-muted-foreground">
                {session.roles[0] ?? "—"}
              </p>
            </div>
            <ChevronDown className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
          </button>

          {userMenuOpen && (
            <div className="absolute bottom-[3.25rem] left-2 right-2 z-50 overflow-hidden rounded-xl border border-border bg-surface shadow-md">
              <div className="border-b border-border px-3 py-2">
                <p className="text-[12px] font-bold text-foreground">{displayName}</p>
                <p className="text-[10px] text-muted-foreground">
                  Module: {modules.join(", ") || "—"}
                </p>
              </div>
              <button
                type="button"
                className="flex w-full items-center gap-2 px-3 py-2 text-[12px] text-foreground hover:bg-muted"
                onClick={() => {
                  setUserMenuOpen(false);
                  setPwdOpen(true);
                  setPwdErr(null);
                }}
              >
                <Settings className="h-3.5 w-3.5" />
                Đổi mật khẩu
              </button>
              <button
                type="button"
                className="flex w-full items-center gap-2 px-3 py-2 text-[12px] font-semibold text-destructive hover:bg-destructive/5"
                onClick={() => {
                  void (async () => {
                    await logoutApi();
                    session.clear();
                    router.replace("/login");
                  })();
                }}
              >
                <LogOut className="h-3.5 w-3.5" />
                Đăng xuất
              </button>
            </div>
          )}
        </div>
      </aside>

      {pwdOpen && (
        <div className="fixed inset-0 z-[90] flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-sm space-y-3 rounded-xl border border-border bg-surface p-5 shadow-lg">
            <h2 className="text-body font-bold">Đổi mật khẩu</h2>
            {pwdErr && <p className="text-meta text-destructive">{pwdErr}</p>}
            <input
              type="password"
              placeholder="Mật khẩu hiện tại"
              value={pwdCur}
              onChange={(e) => setPwdCur(e.target.value)}
              className="w-full rounded-lg border border-border px-3 py-2 text-body"
            />
            <input
              type="password"
              placeholder="Mật khẩu mới"
              value={pwdNew}
              onChange={(e) => setPwdNew(e.target.value)}
              className="w-full rounded-lg border border-border px-3 py-2 text-body"
            />
            <div className="flex justify-end gap-2">
              <button type="button" className="rounded-lg px-3 py-1.5 text-body" onClick={() => setPwdOpen(false)}>
                Hủy
              </button>
              <button
                type="button"
                className="rounded-lg bg-brand px-3 py-1.5 text-body font-semibold text-white"
                onClick={() => {
                  void (async () => {
                    try {
                      await changePassword(pwdCur, pwdNew);
                      setPwdOpen(false);
                      setPwdCur("");
                      setPwdNew("");
                    } catch {
                      setPwdErr("Không đổi được (sai MK hoặc policy).");
                    }
                  })();
                }}
              >
                Lưu
              </button>
            </div>
          </div>
        </div>
      )}

      <div
        className="flex min-h-screen flex-1 flex-col"
        style={{ paddingLeft: "var(--sidebar-width)" }}
      >
        <header className="sticky top-0 z-40 flex h-header w-full shrink-0 items-center justify-between border-b border-header-border bg-header px-6 backdrop-blur-md">
          <div className="flex items-center gap-3">
            <span className="inline-flex items-center gap-2 text-lead font-bold tracking-tight">
              {session.tenantLogoUrl ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={session.tenantLogoUrl}
                  alt=""
                  className="h-7 w-7 rounded-md object-contain"
                />
              ) : null}
              <span>
                <span className="text-brand">{(session.tenantName || "Pum's ERP").split(" ")[0]}</span>
                {(session.tenantName || "Pum's ERP").includes(" ") ? (
                  <span className="text-foreground">
                    {" "}
                    {(session.tenantName || "Pum's ERP").split(" ").slice(1).join(" ")}
                  </span>
                ) : null}
              </span>
            </span>
            <span className="border-l border-border pl-3 text-meta font-medium text-muted-foreground">
              <span className="font-semibold text-foreground">{currentMeta.name}</span>
              <span className="mx-1.5 text-border-strong">·</span>
              {currentMeta.shortName}
            </span>
          </div>
          <div className="flex items-center gap-2 text-meta text-muted-foreground sm:gap-3">
            {canMsg && (
              <div className="relative">
                <button
                  type="button"
                  onClick={() => {
                    setNotifOpen(false);
                    toggleDropdown();
                  }}
                  className={cn(
                    "relative inline-flex items-center gap-1.5 rounded-lg px-2 py-1.5 text-foreground transition hover:bg-muted",
                    dropdownOpen && "bg-muted",
                  )}
                  title="Tin nhắn"
                  aria-expanded={dropdownOpen}
                  aria-haspopup="dialog"
                >
                  <MessageSquare className="h-4 w-4" />
                  <span className="hidden sm:inline">Tin nhắn</span>
                  {unreadMsg > 0 && (
                    <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white">
                      {unreadMsg > 99 ? "99+" : unreadMsg}
                    </span>
                  )}
                </button>
                {dropdownOpen && (
                  <>
                    <button
                      type="button"
                      className="fixed inset-0 z-40 cursor-default bg-transparent"
                      aria-label="Đóng panel tin nhắn"
                      onClick={() => setDropdownOpen(false)}
                    />
                    <MessengerDropdown onClose={() => setDropdownOpen(false)} />
                  </>
                )}
              </div>
            )}

            <div className="relative">
              <button
                type="button"
                onClick={() => {
                  setDropdownOpen(false);
                  setNotifOpen((o) => !o);
                }}
                className={cn(
                  "relative inline-flex items-center gap-1.5 rounded-lg px-2 py-1.5 text-foreground transition hover:bg-muted",
                  notifOpen && "bg-muted",
                )}
                title="Thông báo"
                aria-expanded={notifOpen}
                aria-haspopup="dialog"
              >
                <Bell className="h-4 w-4" />
                <span className="hidden sm:inline">Thông báo</span>
                {unreadNotif > 0 && (
                  <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white">
                    {unreadNotif > 99 ? "99+" : unreadNotif}
                  </span>
                )}
              </button>
              {notifOpen && (
                <>
                  <button
                    type="button"
                    className="fixed inset-0 z-40 cursor-default bg-transparent"
                    aria-label="Đóng panel thông báo"
                    onClick={() => setNotifOpen(false)}
                  />
                  <NotificationDropdown
                    onClose={() => setNotifOpen(false)}
                    onUnreadChange={setUnreadNotif}
                  />
                </>
              )}
            </div>

            <span className="hidden md:inline">
              Scope:{" "}
              <span className="font-semibold text-foreground">{session.effectiveScopeType}</span>
            </span>
          </div>
        </header>
        <main className="flex-1 p-6">{children}</main>
      </div>

      {canMsg && <MessengerDock />}
    </div>
  );
}
