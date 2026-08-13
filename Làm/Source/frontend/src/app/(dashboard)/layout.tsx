import { AppShell } from "@/components/shell/AppShell";

/** Các trang Cap trong (dashboard) dùng chung shell + theme brand như /app/*. */
export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return <AppShell>{children}</AppShell>;
}
