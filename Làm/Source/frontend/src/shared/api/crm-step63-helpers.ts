// crm-step63-helpers.ts
// Frontend helpers cho Bước 63:
//   UC_CRM_027 — Đồng bộ kênh khác (formatChannelTypeBadge)
//   UC_CRM_028 — Attribution nguồn khách (formatAttributionSummary)
//   UC_CRM_029 — Tính CPL / CAC / ROAS / ROI (formatFinancialMetrics)
//   UC_CRM_030 — Funnel marketing đến doanh thu (calculateMarketingFunnelRates)

export function formatChannelTypeBadge(channel?: string): { label: string; icon: string } {
  switch ((channel || '').trim().toLowerCase()) {
    case 'partner_api':
    case 'partner':
      return { label: '🤝 Đối tác / Partner API', icon: '🤝' };
    case 'event_workshop':
    case 'event':
      return { label: '🎤 Sự kiện / Workshop', icon: '🎪' };
    case 'referral':
      return { label: '👥 Giới thiệu (Referral)', icon: '💬' };
    case 'facebook':
    case 'zalo':
    case 'tiktok':
      return { label: '📲 Mạng xã hội', icon: '📲' };
    case 'google':
    case 'website':
      return { label: '🌐 Website / Search', icon: '🔍' };
    default:
      return { label: '📡 Kênh truyền thông khác', icon: '📡' };
  }
}

export function formatAttributionSummary(utmSource?: string, utmMedium?: string, utmCampaign?: string): string {
  const src = (utmSource || 'Trực tiếp').trim();
  const med = (utmMedium || 'none').trim();
  const camp = (utmCampaign || 'Không gắn chiến dịch').trim();

  return `🎯 Nguồn: ${src} | Kênh: ${med} | Chiến dịch: ${camp}`;
}

export function calculateMarketingFunnelRates(leads: number, customers: number, spent: number, revenue: number): {
  conversionRatePct: number;
  roas: number;
  cpl: number;
  cac: number;
  funnelStatus: string;
} {
  const conversionRatePct = leads > 0 ? Math.round((customers / leads) * 10000) / 100 : 0;
  const roas = spent > 0 ? Math.round((revenue / spent) * 100) / 100 : 0;
  const cpl = leads > 0 ? Math.round(spent / leads) : 0;
  const cac = customers > 0 ? Math.round(spent / customers) : 0;

  const funnelStatus = roas >= 3
    ? '🚀 Funnel hiệu quả cao (ROAS >= 3x)'
    : roas >= 1
    ? '✅ Funnel hòa vốn / Có lời nhẹ'
    : '⚠️ Funnel chưa tối ưu (ROAS < 1x)';

  return { conversionRatePct, roas, cpl, cac, funnelStatus };
}

export function formatFinancialMetrics(cpl: number, cac: number, roas: number, roiPct: number): string {
  const roiText = roiPct >= 0 ? `+${roiPct}%` : `${roiPct}%`;
  return `📊 CPL: ${cpl.toLocaleString('vi-VN')} VNĐ | CAC: ${cac.toLocaleString('vi-VN')} VNĐ | ROAS: ${roas}x | ROI: ${roiText}`;
}
