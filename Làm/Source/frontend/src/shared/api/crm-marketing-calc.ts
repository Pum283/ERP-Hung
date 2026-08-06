/** Công thức chiết khấu CRM promo (FE mirror BE) — test độc lập, không phụ thuộc axios. */
export function calcPromoDiscount(input: {
  discountType: string;
  discountValue: number;
  maxDiscountAmount?: number | null;
  minOrderValue?: number | null;
  subTotal: number;
}): number {
  const { discountType, discountValue, maxDiscountAmount, minOrderValue, subTotal } = input;
  if (minOrderValue != null && subTotal < minOrderValue) return 0;
  let d =
    discountType === "Percentage"
      ? Math.round(((subTotal * discountValue) / 100) * 100) / 100
      : discountType === "FixedAmount"
        ? discountValue
        : 0;
  if (maxDiscountAmount != null && d > maxDiscountAmount) d = maxDiscountAmount;
  if (d > subTotal) d = subTotal;
  return d < 0 ? 0 : d;
}
