import type { LucideIcon } from "lucide-react";
import {
  Boxes,
  Briefcase,
  Building2,
  Calculator,
  Factory,
  GraduationCap,
  Inbox,
  LayoutDashboard,
  Package,
  ShoppingCart,
  Truck,
  Users,
  Wrench,
  BarChart3,
  Globe,
} from "lucide-react";

export type ModuleMeta = {
  code: string;
  name: string;
  shortName: string;
  homePath: string;
  Icon: LucideIcon;
};

/** Catalog hiển thị module (chỉ những module user có quyền mới hiện ở switcher). */
export const MODULE_META: Record<string, ModuleMeta> = {
  SYS: {
    code: "SYS",
    name: "Hệ thống",
    shortName: "SYS",
    homePath: "/app/sys/users",
    Icon: Building2,
  },
  HRM: {
    code: "HRM",
    name: "Nhân sự",
    shortName: "HRM",
    homePath: "/app/hrm/employees",
    Icon: Users,
  },
  WF: {
    code: "WF",
    name: "Phê duyệt & công việc",
    shortName: "WF",
    homePath: "/app/wf/tasks",
    Icon: Inbox,
  },
  LMS: {
    code: "LMS",
    name: "Đào tạo",
    shortName: "LMS",
    homePath: "/app",
    Icon: GraduationCap,
  },
  CRM: {
    code: "CRM",
    name: "CRM & Bán hàng",
    shortName: "CRM",
    homePath: "/app",
    Icon: Briefcase,
  },
  POS: {
    code: "POS",
    name: "POS bán lẻ",
    shortName: "POS",
    homePath: "/app",
    Icon: ShoppingCart,
  },
  PUR: {
    code: "PUR",
    name: "Mua hàng",
    shortName: "PUR",
    homePath: "/app",
    Icon: Package,
  },
  INV: {
    code: "INV",
    name: "Kho & tồn kho",
    shortName: "INV",
    homePath: "/app",
    Icon: Boxes,
  },
  LOG: {
    code: "LOG",
    name: "Giao vận",
    shortName: "LOG",
    homePath: "/app",
    Icon: Truck,
  },
  MFG: {
    code: "MFG",
    name: "Sản xuất",
    shortName: "MFG",
    homePath: "/app",
    Icon: Factory,
  },
  FSM: {
    code: "FSM",
    name: "Dịch vụ kỹ thuật",
    shortName: "FSM",
    homePath: "/app",
    Icon: Wrench,
  },
  PJM: {
    code: "PJM",
    name: "Dự án",
    shortName: "PJM",
    homePath: "/app",
    Icon: LayoutDashboard,
  },
  FIN: {
    code: "FIN",
    name: "Tài chính",
    shortName: "FIN",
    homePath: "/app",
    Icon: Calculator,
  },
  AST: {
    code: "AST",
    name: "Tài sản",
    shortName: "AST",
    homePath: "/app",
    Icon: Package,
  },
  BI: {
    code: "BI",
    name: "Báo cáo & BI",
    shortName: "BI",
    homePath: "/app",
    Icon: BarChart3,
  },
  PRT: {
    code: "PRT",
    name: "Cổng khách hàng",
    shortName: "PRT",
    homePath: "/app",
    Icon: Globe,
  },
};

export function getModuleMeta(code: string): ModuleMeta {
  const c = code.toUpperCase();
  return (
    MODULE_META[c] ?? {
      code: c,
      name: c,
      shortName: c,
      homePath: "/app",
      Icon: LayoutDashboard,
    }
  );
}

/** Suy module từ pathname `/app/{mod}/...` */
export function moduleFromPath(pathname: string): string | null {
  const m = pathname.match(/^\/app\/([a-z0-9]+)/i);
  if (!m) return null;
  const seg = m[1].toUpperCase();
  if (seg === "APP") return null;
  return MODULE_META[seg] ? seg : seg;
}
