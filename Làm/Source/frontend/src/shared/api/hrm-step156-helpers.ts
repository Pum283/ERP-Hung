export interface FlatDepartment {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  orgUnitId: string;
  managerUserId?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface DepartmentTreeNode extends FlatDepartment {
  children: DepartmentTreeNode[];
}

export interface CostCenterItem {
  id: string;
  code: string;
  name: string;
  orgUnitId?: string | null;
  allocationPercentage: number;
  isActive: boolean;
}

export interface RelativeItem {
  id: string;
  employeeId: string;
  fullName: string;
  relationship: string;
  phone?: string | null;
  address?: string | null;
  isEmergencyContact: boolean;
  isTaxDependent: boolean;
  idNumber?: string | null;
}

/**
 * Xây dựng cấu trúc cây phòng ban từ danh sách phẳng
 */
export function buildDepartmentTree(items: FlatDepartment[]): DepartmentTreeNode[] {
  const map = new Map<string, DepartmentTreeNode>();
  const roots: DepartmentTreeNode[] = [];

  items.forEach((item) => {
    map.set(item.id, { ...item, children: [] });
  });

  items.forEach((item) => {
    const node = map.get(item.id)!;
    if (item.parentId && map.has(item.parentId)) {
      map.get(item.parentId)!.children.push(node);
    } else {
      roots.push(node);
    }
  });

  const sortNodes = (nodes: DepartmentTreeNode[]) => {
    nodes.sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name));
    nodes.forEach((n) => sortNodes(n.children));
  };

  sortNodes(roots);
  return roots;
}

/**
 * Kiểm tra và tính tổng % phân bổ trung tâm chi phí
 */
export function validateCostCenterAllocation(items: CostCenterItem[]): {
  totalPercentage: number;
  isValid: boolean;
  errorMessage?: string;
} {
  const activeItems = items.filter((i) => i.isActive);
  const total = activeItems.reduce((acc, curr) => acc + (Number(curr.allocationPercentage) || 0), 0);

  if (total > 100) {
    return {
      totalPercentage: total,
      isValid: false,
      errorMessage: `Tổng tỷ lệ phân bổ (${total}%) vượt quá 100%.`,
    };
  }

  return {
    totalPercentage: total,
    isValid: true,
  };
}

/**
 * Lọc danh sách liên hệ khẩn cấp
 */
export function filterEmergencyContacts(relatives: RelativeItem[]): RelativeItem[] {
  return relatives
    .filter((r) => r.isEmergencyContact)
    .sort((a, b) => a.fullName.localeCompare(b.fullName));
}

/**
 * Validate form tạo/sửa bộ phận
 */
export function validateDepartmentForm(input: {
  code: string;
  name: string;
  id?: string;
  parentId?: string | null;
}): { isValid: boolean; error?: string } {
  if (!input.code || !input.code.trim()) {
    return { isValid: false, error: 'Mã bộ phận không được để trống.' };
  }
  if (!input.name || !input.name.trim()) {
    return { isValid: false, error: 'Tên bộ phận không được để trống.' };
  }
  if (input.id && input.parentId && input.id === input.parentId) {
    return { isValid: false, error: 'Bộ phận cha không thể là chính nó.' };
  }
  return { isValid: true };
}

/**
 * Validate form tạo/sửa người thân
 */
export function validateRelativeForm(input: {
  employeeId: string;
  fullName: string;
  relationship: string;
  phone?: string | null;
}): { isValid: boolean; error?: string } {
  if (!input.employeeId) {
    return { isValid: false, error: 'Chưa chọn nhân sự.' };
  }
  if (!input.fullName || !input.fullName.trim()) {
    return { isValid: false, error: 'Họ tên người thân không được để trống.' };
  }
  const validRelationships = ['Spouse', 'Child', 'Parent', 'Sibling', 'Other'];
  if (!validRelationships.includes(input.relationship)) {
    return { isValid: false, error: 'Mối quan hệ không hợp lệ.' };
  }
  if (input.phone && !/^[0-9+\s\-()]{8,15}$/.test(input.phone.trim())) {
    return { isValid: false, error: 'Số điện thoại không đúng định dạng.' };
  }
  return { isValid: true };
}
