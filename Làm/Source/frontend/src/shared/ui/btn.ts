/** Class nút chuẩn ERP (khớp Users / HRM). */
export const btn = {
  primary:
    "inline-flex h-9 items-center justify-center rounded-md bg-brand px-3 text-body font-semibold text-brand-foreground transition-colors hover:bg-brand-hover disabled:opacity-60",
  secondary:
    "inline-flex h-9 items-center justify-center rounded-md border border-border bg-surface px-3 text-body font-medium text-foreground transition-colors hover:bg-muted disabled:opacity-60",
  soft:
    "inline-flex h-8 items-center justify-center gap-1.5 rounded-md border border-brand/25 bg-brand-muted px-2.5 text-meta font-semibold text-brand-strong transition-colors hover:border-brand hover:bg-brand hover:text-brand-foreground disabled:opacity-60",
  danger:
    "inline-flex h-8 items-center justify-center rounded-md border border-destructive/30 bg-destructive/10 px-2.5 text-meta font-semibold text-destructive transition-colors hover:bg-destructive hover:text-white disabled:opacity-60",
  ghost:
    "inline-flex h-8 items-center justify-center rounded-md px-2.5 text-meta font-semibold text-muted-foreground transition-colors hover:bg-muted hover:text-foreground disabled:opacity-60",
} as const;
