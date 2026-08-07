/** Pure helpers — FIN JE always-on post (UC_FIN_019/025/030/039). */

export function finJePostedLabel(finJournalCode?: string | null): string {
  return finJournalCode
    ? `Đã ghi sổ + JE ${finJournalCode}`
    : "Đã ghi sổ (chưa có mã JE)";
}

export function canShowFinJe(finJournalId?: string | null): boolean {
  return !!finJournalId;
}

/** Expected COA prefixes for auto-resolve (mirror BE). */
export function expectedCounterPrefixes(kind: "cash-receipt" | "cash-payment" | "bank-credit" | "bank-debit" | "ar" | "ap"): string[] {
  switch (kind) {
    case "cash-receipt":
    case "bank-credit":
      return ["131", "511"];
    case "cash-payment":
    case "bank-debit":
      return ["331", "642", "156"];
    case "ar":
      return ["131", "511"];
    case "ap":
      return ["156", "331"];
  }
}
