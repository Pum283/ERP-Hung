export function validateSplitBillSelection(selectedItemIds: string[]): { isValid: boolean; error?: string } {
  if (!selectedItemIds || selectedItemIds.length === 0) {
    return { isValid: false, error: 'Vui lòng chọn ít nhất 1 món để thực hiện tách hóa đơn.' };
  }
  return { isValid: true };
}

export function validateKitchenNoteLength(note: string, maxLength = 200): { isValid: boolean; error?: string } {
  if (note && note.length > maxLength) {
    return { isValid: false, error: `Ghi chú chế bếp không được vượt quá ${maxLength} ký tự.` };
  }
  return { isValid: true };
}
