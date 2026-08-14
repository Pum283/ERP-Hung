export function validateEan13BarcodeFormat(barcode: string): boolean {
  if (!barcode) return false;
  const cleaned = barcode.trim();
  return /^\d{13}$/.test(cleaned);
}

export function parseQrPayload(qrPayload: string): { isProductQr: boolean; productId: string; productCode: string; barcode: string } {
  if (!qrPayload || !qrPayload.startsWith('ERP-PROD|')) {
    return { isProductQr: false, productId: '', productCode: '', barcode: '' };
  }

  const parts = qrPayload.split('|');
  const productId = parts[1] || '';
  const productCode = parts[2] || '';
  const barcodePart = parts[3] || '';
  const barcode = barcodePart.replace('BC:', '');

  return {
    isProductQr: true,
    productId,
    productCode,
    barcode,
  };
}
