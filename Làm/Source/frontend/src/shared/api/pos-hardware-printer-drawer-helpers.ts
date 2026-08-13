export function validatePrinterConfigForm(printerName: string, ipOrPort: string): { isValid: boolean; error?: string } {
  if (!printerName || !printerName.trim()) {
    return { isValid: false, error: 'Tên máy in không được để trống.' };
  }
  if (!ipOrPort || !ipOrPort.trim()) {
    return { isValid: false, error: 'Địa chỉ IP hoặc cổng USB/Serial không được để trống.' };
  }
  return { isValid: true };
}

export function validateCashDrawerConfigForm(drawerName: string, commandHex: string): { isValid: boolean; error?: string } {
  if (!drawerName || !drawerName.trim()) {
    return { isValid: false, error: 'Tên ngăn kéo tiền không được để trống.' };
  }
  if (!commandHex || !commandHex.trim()) {
    return { isValid: false, error: 'Lệnh kích mở hex (Pulse Command) không được để trống.' };
  }
  return { isValid: true };
}
