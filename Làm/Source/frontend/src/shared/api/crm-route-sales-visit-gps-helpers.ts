export function calculateGpsDistanceKm(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const R = 6371; // Radius of Earth in km
  const dLat = ((lat2 - lat1) * Math.PI) / 180;
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos((lat1 * Math.PI) / 180) * Math.cos((lat2 * Math.PI) / 180) * Math.sin(dLon / 2) * Math.sin(dLon / 2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  const distance = R * c;
  return Math.round(distance * 100) / 100;
}

export function formatVisitFrequencyLabel(freq: string): string {
  switch (freq?.toLowerCase()) {
    case 'weekly':
      return 'Hàng tuần (Weekly - 1 lần/tuần)';
    case 'biweekly':
      return '2 tuần / lần (Bi-weekly)';
    case 'monthly':
      return 'Hàng tháng (Monthly - 1 lần/tháng)';
    default:
      return 'Hàng tuần (Weekly)';
  }
}

export function validateGpsCoordinates(lat: number, lng: number): { isValid: boolean; error?: string } {
  if (isNaN(lat) || lat < -90 || lat > 90) {
    return { isValid: false, error: 'Vĩ độ GPS (Latitude) không hợp lệ.' };
  }
  if (isNaN(lng) || lng < -180 || lng > 180) {
    return { isValid: false, error: 'Kinh độ GPS (Longitude) không hợp lệ.' };
  }
  return { isValid: true };
}
