export interface SlaStatusResult {
  isBreached: boolean;
  statusText: 'Phản hồi tốt' | 'Cảnh báo sắp trễ' | 'Vi phạm SLA';
  badgeClass: string;
}

export function evaluateSlaStatus(maxMinutes: number, actualMinutes: number): SlaStatusResult {
  const max = Math.max(1, maxMinutes);
  const actual = Math.max(0, actualMinutes);

  if (actual > max) {
    return { isBreached: true, statusText: 'Vi phạm SLA', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300' };
  }
  if (actual >= max - 1) {
    return { isBreached: false, statusText: 'Cảnh báo sắp trễ', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
  return { isBreached: false, statusText: 'Phản hồi tốt', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
}

export function validateRoutingRule(ruleName: string, strategy: string): { isValid: boolean; error?: string } {
  if (!ruleName || !ruleName.trim()) {
    return { isValid: false, error: 'Tên quy tắc phân phối không được để trống.' };
  }
  const validStrategies = ['RoundRobin', 'LoadBalance', 'SkillBased'];
  if (!validStrategies.includes(strategy)) {
    return { isValid: false, error: 'Chiến lược phân phối không hợp lệ.' };
  }
  return { isValid: true };
}

export function parseBotFlowSteps(flowJson: string): Array<{ step: number; action: string; text?: string }> {
  try {
    const parsed = JSON.parse(flowJson);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}
