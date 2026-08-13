export interface PriorityTierResult {
  priorityTier: 'Hot' | 'Warm' | 'Cold';
  badgeColorClass: string;
}

export function evaluateLeadPriorityTier(score: number): PriorityTierResult {
  const safeScore = Math.min(100, Math.max(0, score));

  if (safeScore >= 80) {
    return { priorityTier: 'Hot', badgeColorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
  }
  if (safeScore >= 50) {
    return { priorityTier: 'Warm', badgeColorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
  return { priorityTier: 'Cold', badgeColorClass: 'bg-blue-100 text-blue-800 border-blue-300' };
}

export function generateClonedCampaignName(originalName: string): string {
  if (!originalName || !originalName.trim()) return 'Campaign Mới (Bản sao)';
  const trimmed = originalName.trim();
  if (trimmed.endsWith('(Bản sao)')) return `${trimmed} 2`;
  return `${trimmed} (Bản sao)`;
}

export function filterConversationsByChannel<T extends { channel: string; customerName: string }>(
  conversations: T[],
  channelFilter: string
): T[] {
  if (!channelFilter || channelFilter.trim() === '' || channelFilter.toLowerCase() === 'all') {
    return conversations;
  }

  const term = channelFilter.trim().toLowerCase();
  return conversations.filter((c) => c.channel.toLowerCase() === term);
}
