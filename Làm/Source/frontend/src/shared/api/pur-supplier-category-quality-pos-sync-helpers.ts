export function calculateWeightedQualityScore(
  onTime: number,
  quality: number,
  price: number
): { overallScore: number; grade: 'A' | 'B' | 'C' | 'D' } {
  const overall = Math.round(((onTime + quality + price) / 3.0) * 10) / 10;
  let grade: 'A' | 'B' | 'C' | 'D' = 'D';
  if (overall >= 90) grade = 'A';
  else if (overall >= 75) grade = 'B';
  else if (overall >= 60) grade = 'C';

  return { overallScore: overall, grade };
}

export function validateOrderMoqCompliance(orderQty: number, moq: number): { isCompliant: boolean; deficit: number } {
  if (orderQty >= moq) {
    return { isCompliant: true, deficit: 0 };
  }
  return { isCompliant: false, deficit: moq - orderQty };
}
