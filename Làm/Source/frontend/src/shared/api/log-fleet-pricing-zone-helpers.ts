export function formatVehiclePayload(kg: number): string {
  if (kg >= 1000) {
    return `${(kg / 1000).toFixed(1)} Tấn`;
  }
  return `${kg} Kg`;
}

export function formatEstimatedTransitTime(hours: number): string {
  return `${hours} Giờ`;
}
