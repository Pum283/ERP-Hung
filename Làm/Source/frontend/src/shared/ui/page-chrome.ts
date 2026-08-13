/** Class tab/page chrome thống nhất — dùng màu brand từ Branding. */
export const pageTitle = "font-display text-title font-bold text-foreground";
export const pageDesc = "mt-1 text-body text-muted-foreground";

export const tabBar = "flex flex-wrap gap-1 border-b border-border pb-0";

export function tabBtn(active: boolean): string {
  return [
    "inline-flex h-9 items-center rounded-t-md px-3 text-body font-semibold transition-colors",
    active
      ? "border border-b-0 border-border bg-surface text-brand-strong"
      : "text-muted-foreground hover:bg-muted hover:text-foreground",
  ].join(" ");
}
