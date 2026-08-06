/** Pure helpers — đẩy bút toán khấu hao AST → FIN JE (UC_AST_012). */

type DepreciationRunLike = {
  status: string;
  lineCount: number;
  totalAmount: number;
  finJournalId?: string | null;
};

/** Chỉ đẩy FIN khi kỳ chưa Pushed và có dữ liệu KH (khớp validate BE). */
export function canPushDepreciationFin(run: DepreciationRunLike): boolean {
  if (run.status === "Pushed" && run.finJournalId) return false;
  return run.lineCount > 0 && run.totalAmount > 0;
}

/** Mã JE FIN theo convention BE `JE-AST-{runCode}`. */
export function buildDepreciationJeCode(runCode: string): string {
  return `JE-AST-${runCode}`;
}

/** Nhãn trạng thái đẩy FIN của kỳ KH. */
export function depreciationFinLabel(run: DepreciationRunLike): string {
  if (run.finJournalId) return "Đã tạo JE FIN (Nợ CP KH / Có KH lũy kế)";
  if (run.status === "Pushed") return "Đã đánh dấu đẩy (kỳ cũ, chưa có JE)";
  return "Chưa đẩy FIN";
}
