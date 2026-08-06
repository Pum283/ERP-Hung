/** Class field chuẩn ERP (khớp Employees / Users). */
export const field = {
  label: "mb-1 block text-meta font-medium text-muted-foreground",
  input:
    "h-9 w-full rounded-md border border-border bg-background px-3 text-body text-foreground outline-none transition-colors placeholder:text-muted-foreground focus:border-brand focus:ring-1 focus:ring-brand/30 disabled:opacity-60",
  select:
    "h-9 w-full rounded-md border border-border bg-background px-2.5 text-body text-foreground outline-none transition-colors focus:border-brand focus:ring-1 focus:ring-brand/30 disabled:opacity-60",
  textarea:
    "min-h-[72px] w-full rounded-md border border-border bg-background px-3 py-2 text-body text-foreground outline-none transition-colors placeholder:text-muted-foreground focus:border-brand focus:ring-1 focus:ring-brand/30 disabled:opacity-60",
  check: "h-4 w-4 rounded border-border text-brand focus:ring-brand/30",
} as const;

export const panel =
  "rounded-xl border border-border bg-surface p-4 shadow-sm" as const;

export const tableWrap =
  "overflow-x-auto rounded-xl border border-border bg-surface shadow-sm" as const;

export const th =
  "px-3 py-2.5 text-left text-meta font-semibold text-muted-foreground" as const;

export const td = "px-3 py-2 text-body text-foreground align-middle" as const;

export function statusPill(tone: "brand" | "success" | "warning" | "danger" | "muted" = "muted") {
  const tones = {
    brand: "bg-brand-muted text-brand-strong",
    success: "bg-success/15 text-success",
    warning: "bg-warning/15 text-warning",
    danger: "bg-destructive/10 text-destructive",
    muted: "bg-muted text-muted-foreground",
  } as const;
  return `inline-flex items-center rounded-md px-2 py-0.5 text-meta font-semibold ${tones[tone]}`;
}
