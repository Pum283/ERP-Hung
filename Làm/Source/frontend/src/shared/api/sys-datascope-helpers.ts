export interface OrgUnitNode {
  id: string;
  code: string;
  name: string;
  parentId: string | null;
  unitType: string;
  isActive: boolean;
  path: string;
}

export interface DepartmentNode {
  id: string;
  code: string;
  name: string;
  parentId: string | null;
  path: string;
  isActive: boolean;
}

export function filterAccessibleDepartments(
  departments: DepartmentNode[],
  accessibleDepartmentIds: string[] | null,
  bypassDataScope: boolean = false
): DepartmentNode[] {
  if (bypassDataScope || !accessibleDepartmentIds) return departments;
  const idSet = new Set(accessibleDepartmentIds);
  return departments.filter(d => idSet.has(d.id));
}

export function validateOrgUnitForm(data: { code: string; name: string; parentId?: string | null; id?: string | null }): { valid: boolean; error?: string } {
  if (!data.code || !data.code.trim()) {
    return { valid: false, error: 'Mã chi nhánh không được để trống.' };
  }
  if (!data.name || !data.name.trim()) {
    return { valid: false, error: 'Tên chi nhánh không được để trống.' };
  }
  if (data.id && data.parentId && data.id === data.parentId) {
    return { valid: false, error: 'Chi nhánh không thể làm đơn vị cấp trên của chính nó.' };
  }
  return { valid: true };
}

export function buildOrgUnitTree(units: OrgUnitNode[]): (OrgUnitNode & { children: OrgUnitNode[] })[] {
  const map = new Map<string, OrgUnitNode & { children: OrgUnitNode[] }>();
  const roots: (OrgUnitNode & { children: OrgUnitNode[] })[] = [];

  for (const u of units) {
    map.set(u.id, { ...u, children: [] });
  }

  for (const u of units) {
    const node = map.get(u.id)!;
    if (u.parentId && map.has(u.parentId)) {
      map.get(u.parentId)!.children.push(node);
    } else {
      roots.push(node);
    }
  }

  return roots;
}
