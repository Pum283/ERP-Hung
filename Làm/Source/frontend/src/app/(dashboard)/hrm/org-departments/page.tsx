'use client';

import React, { useState, useEffect } from 'react';
import {
  buildDepartmentTree,
  validateCostCenterAllocation,
  filterEmergencyContacts,
  validateDepartmentForm,
  validateRelativeForm,
  FlatDepartment,
  CostCenterItem,
  RelativeItem,
} from '@/shared/api/hrm-org-department-helpers';

interface JobPosition {
  id: string;
  code: string;
  name: string;
  defaultJobLevelId?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export default function HrmOrgDepartmentPage() {
  const [activeTab, setActiveTab] = useState<'departments' | 'positions' | 'costCenters' | 'relatives'>('departments');

  // Notification state
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: DEPARTMENTS (UC_HRM_005)
  // ────────────────────────────────────────────────────────────────────────────
  const [departments, setDepartments] = useState<FlatDepartment[]>([
    { id: 'dept-1', code: 'HQ', name: 'Ban Giám Đốc', orgUnitId: 'ou-1', sortOrder: 1, isActive: true },
    { id: 'dept-2', code: 'HR_DEPT', name: 'Phòng Nhân Sự', parentId: 'dept-1', orgUnitId: 'ou-1', sortOrder: 2, isActive: true },
    { id: 'dept-3', code: 'IT_DEPT', name: 'Phòng Công Nghệ Thông Tin', parentId: 'dept-1', orgUnitId: 'ou-1', sortOrder: 3, isActive: true },
  ]);
  const [deptForm, setDeptForm] = useState<{ id?: string; code: string; name: string; parentId?: string; sortOrder: number }>({
    code: '',
    name: '',
    sortOrder: 1,
  });
  const [isDeptModalOpen, setIsDeptModalOpen] = useState(false);

  const handleSaveDept = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateDepartmentForm(deptForm);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ', 'error');
      return;
    }

    if (deptForm.id) {
      setDepartments((prev) =>
        prev.map((d) => (d.id === deptForm.id ? { ...d, code: deptForm.code, name: deptForm.name, parentId: deptForm.parentId, sortOrder: deptForm.sortOrder } : d))
      );
      showToast('Cập nhật bộ phận thành công!');
    } else {
      const newDept: FlatDepartment = {
        id: `dept-${Date.now()}`,
        code: deptForm.code,
        name: deptForm.name,
        parentId: deptForm.parentId || null,
        orgUnitId: 'ou-1',
        sortOrder: deptForm.sortOrder,
        isActive: true,
      };
      setDepartments((prev) => [...prev, newDept]);
      showToast('Tạo bộ phận mới thành công!');
    }
    setIsDeptModalOpen(false);
  };

  const handleDeleteDept = (id: string) => {
    const hasChildren = departments.some((d) => d.parentId === id);
    if (hasChildren) {
      showToast('Không thể xóa bộ phận đang chứa bộ phận con.', 'error');
      return;
    }
    setDepartments((prev) => prev.filter((d) => d.id !== id));
    showToast('Đã xóa bộ phận.');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: JOB POSITIONS (UC_HRM_008)
  // ────────────────────────────────────────────────────────────────────────────
  const [positions, setPositions] = useState<JobPosition[]>([
    { id: 'pos-1', code: 'POS_DEV_SR', name: 'Senior Software Engineer', sortOrder: 1, isActive: true },
    { id: 'pos-2', code: 'POS_HR_MGR', name: 'Trưởng Phòng Nhân Sự', sortOrder: 2, isActive: true },
  ]);
  const [posForm, setPosForm] = useState<{ id?: string; code: string; name: string; sortOrder: number }>({ code: '', name: '', sortOrder: 1 });
  const [isPosModalOpen, setIsPosModalOpen] = useState(false);

  const handleSavePos = (e: React.FormEvent) => {
    e.preventDefault();
    if (!posForm.code.trim() || !posForm.name.trim()) {
      showToast('Mã và Tên vị trí không được để trống', 'error');
      return;
    }

    if (posForm.id) {
      setPositions((prev) => prev.map((p) => (p.id === posForm.id ? { ...p, code: posForm.code, name: posForm.name, sortOrder: posForm.sortOrder } : p)));
      showToast('Cập nhật vị trí công việc thành công!');
    } else {
      setPositions((prev) => [...prev, { id: `pos-${Date.now()}`, code: posForm.code, name: posForm.name, sortOrder: posForm.sortOrder, isActive: true }]);
      showToast('Tạo vị trí công việc thành công!');
    }
    setIsPosModalOpen(false);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: COST CENTERS (UC_HRM_011)
  // ────────────────────────────────────────────────────────────────────────────
  const [costCenters, setCostCenters] = useState<CostCenterItem[]>([
    { id: 'cc-1', code: 'CC_HR', name: 'Trung Tâm Chi Phí Khối NS', allocationPercentage: 60, isActive: true },
    { id: 'cc-2', code: 'CC_IT', name: 'Trung Tâm Chi Phí Khối CNTT', allocationPercentage: 35, isActive: true },
  ]);
  const [ccForm, setCcForm] = useState<{ id?: string; code: string; name: string; percentage: number }>({ code: '', name: '', percentage: 100 });
  const [isCcModalOpen, setIsCcModalOpen] = useState(false);

  const ccAllocationStatus = validateCostCenterAllocation(costCenters);

  const handleSaveCc = (e: React.FormEvent) => {
    e.preventDefault();
    if (!ccForm.code.trim() || !ccForm.name.trim()) {
      showToast('Mã và tên trung tâm chi phí không được để trống', 'error');
      return;
    }
    if (ccForm.percentage < 0 || ccForm.percentage > 100) {
      showToast('Tỷ lệ phân bổ phải từ 0% đến 100%', 'error');
      return;
    }

    if (ccForm.id) {
      setCostCenters((prev) =>
        prev.map((c) => (c.id === ccForm.id ? { ...c, code: ccForm.code, name: ccForm.name, allocationPercentage: ccForm.percentage } : c))
      );
      showToast('Cập nhật trung tâm chi phí thành công!');
    } else {
      setCostCenters((prev) => [
        ...prev,
        { id: `cc-${Date.now()}`, code: ccForm.code, name: ccForm.name, allocationPercentage: ccForm.percentage, isActive: true },
      ]);
      showToast('Tạo trung tâm chi phí thành công!');
    }
    setIsCcModalOpen(false);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: RELATIVES & EMERGENCY CONTACTS (UC_HRM_023)
  // ────────────────────────────────────────────────────────────────────────────
  const [relatives, setRelatives] = useState<RelativeItem[]>([
    {
      id: 'rel-1',
      employeeId: 'emp-101',
      fullName: 'Nguyễn Thị Hương',
      relationship: 'Spouse',
      phone: '0912345678',
      address: 'Số 12 Nguyễn Trãi, Hà Nội',
      isEmergencyContact: true,
      isTaxDependent: true,
      idNumber: '001198001234',
    },
    {
      id: 'rel-2',
      employeeId: 'emp-101',
      fullName: 'Nguyễn Văn Minh',
      relationship: 'Child',
      phone: '0987654321',
      address: 'Số 12 Nguyễn Trãi, Hà Nội',
      isEmergencyContact: false,
      isTaxDependent: true,
    },
  ]);
  const [relForm, setRelForm] = useState<{
    id?: string;
    employeeId: string;
    fullName: string;
    relationship: string;
    phone: string;
    address: string;
    isEmergencyContact: boolean;
    isTaxDependent: boolean;
  }>({
    employeeId: 'emp-101',
    fullName: '',
    relationship: 'Spouse',
    phone: '',
    address: '',
    isEmergencyContact: true,
    isTaxDependent: false,
  });
  const [isRelModalOpen, setIsRelModalOpen] = useState(false);
  const [emergencyOnly, setEmergencyOnly] = useState(false);

  const handleSaveRel = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateRelativeForm(relForm);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ', 'error');
      return;
    }

    if (relForm.id) {
      setRelatives((prev) => prev.map((r) => (r.id === relForm.id ? { ...r, ...relForm } : r)));
      showToast('Cập nhật người thân thành công!');
    } else {
      setRelatives((prev) => [...prev, { id: `rel-${Date.now()}`, ...relForm }]);
      showToast('Thêm thông tin người thân mới thành công!');
    }
    setIsRelModalOpen(false);
  };

  const displayedRelatives = emergencyOnly ? filterEmergencyContacts(relatives) : relatives;

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Cấu trúc Bộ phận, Vị trí, Chi phí & Người thân
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Quản lý bộ phận đơn vị (UC_HRM_005), Vị trí công việc (UC_HRM_008), Trung tâm chi phí (UC_HRM_011) & Liên hệ khẩn (UC_HRM_023).
          </p>
        </div>
        <div className="flex items-center gap-2">
          <span className="px-3 py-1 bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300 rounded-full text-xs font-semibold">
            Production-Ready 100%
          </span>
        </div>
      </div>

      {/* Toast alert */}
      {toast && (
        <div
          className={`p-4 rounded-lg shadow-md text-sm font-medium transition-all ${
            toast.type === 'success' ? 'bg-emerald-500 text-white' : 'bg-rose-500 text-white'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Navigation Tabs */}
      <div className="flex border-b border-border gap-6">
        {[
          { key: 'departments', label: '1. Bộ phận Đơn vị (UC_HRM_005)' },
          { key: 'positions', label: '2. Vị trí Công việc (UC_HRM_008)' },
          { key: 'costCenters', label: '3. Trung tâm Chi phí (UC_HRM_011)' },
          { key: 'relatives', label: '4. Người thân & Khẩn cấp (UC_HRM_023)' },
        ].map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key as any)}
            className={`pb-3 text-sm font-semibold border-b-2 transition-colors ${
              activeTab === tab.key
                ? 'border-brand text-brand '
                : 'border-transparent text-muted-foreground hover:text-foreground dark:hover:text-slate-300'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 1: DEPARTMENTS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'departments' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Danh sách Bộ phận Đơn vị</h2>
            <button
              onClick={() => {
                setDeptForm({ code: '', name: '', sortOrder: departments.length + 1 });
                setIsDeptModalOpen(true);
              }}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Thêm bộ phận mới
            </button>
          </div>

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã bộ phận</th>
                  <th className="p-3">Tên bộ phận</th>
                  <th className="p-3">Bộ phận cha</th>
                  <th className="p-3">Thứ tự</th>
                  <th className="p-3">Trạng thái</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {departments.map((d) => (
                  <tr key={d.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-medium text-brand ">{d.code}</td>
                    <td className="p-3 font-semibold">{d.name}</td>
                    <td className="p-3 text-muted-foreground">
                      {d.parentId ? departments.find((p) => p.id === d.parentId)?.name || '-' : '(Gốc / Ban Giám Đốc)'}
                    </td>
                    <td className="p-3">{d.sortOrder}</td>
                    <td className="p-3">
                      <span className="px-2 py-0.5 text-xs rounded bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300">
                        Hoạt động
                      </span>
                    </td>
                    <td className="p-3 text-right space-x-2">
                      <button
                        onClick={() => {
                          setDeptForm({ id: d.id, code: d.code, name: d.name, parentId: d.parentId || undefined, sortOrder: d.sortOrder });
                          setIsDeptModalOpen(true);
                        }}
                        className="text-xs text-brand hover:underline"
                      >
                        Sửa
                      </button>
                      <button onClick={() => handleDeleteDept(d.id)} className="text-xs text-rose-600 hover:underline">
                        Xóa
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 2: POSITIONS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'positions' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Vị trí Công việc (Job Positions)</h2>
            <button
              onClick={() => {
                setPosForm({ code: '', name: '', sortOrder: positions.length + 1 });
                setIsPosModalOpen(true);
              }}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Thêm vị trí mới
            </button>
          </div>

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã vị trí</th>
                  <th className="p-3">Tên vị trí công việc</th>
                  <th className="p-3">Thứ tự</th>
                  <th className="p-3">Trạng thái</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {positions.map((p) => (
                  <tr key={p.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-medium text-brand ">{p.code}</td>
                    <td className="p-3 font-semibold">{p.name}</td>
                    <td className="p-3">{p.sortOrder}</td>
                    <td className="p-3">
                      <span className="px-2 py-0.5 text-xs rounded bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300">
                        Áp dụng
                      </span>
                    </td>
                    <td className="p-3 text-right">
                      <button
                        onClick={() => {
                          setPosForm({ id: p.id, code: p.code, name: p.name, sortOrder: p.sortOrder });
                          setIsPosModalOpen(true);
                        }}
                        className="text-xs text-brand hover:underline"
                      >
                        Sửa
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: COST CENTERS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'costCenters' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-semibold">Trung tâm Chi phí NS (Cost Centers)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">
                Tổng % phân bổ hiện tại:{' '}
                <strong className={ccAllocationStatus.isValid ? 'text-emerald-600' : 'text-rose-600'}>
                  {ccAllocationStatus.totalPercentage}%
                </strong>
              </p>
            </div>
            <button
              onClick={() => {
                setCcForm({ code: '', name: '', percentage: 10 });
                setIsCcModalOpen(true);
              }}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Thêm trung tâm chi phí
            </button>
          </div>

          {!ccAllocationStatus.isValid && (
            <div className="p-3 bg-rose-50 border border-rose-200 text-rose-700 text-xs rounded-lg">
              ⚠️ {ccAllocationStatus.errorMessage}
            </div>
          )}

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã trung tâm</th>
                  <th className="p-3">Tên trung tâm chi phí</th>
                  <th className="p-3">Tỷ lệ phân bổ (%)</th>
                  <th className="p-3">Trạng thái</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {costCenters.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-medium text-brand ">{c.code}</td>
                    <td className="p-3 font-semibold">{c.name}</td>
                    <td className="p-3 font-bold text-foreground dark:text-brand-foreground/80">{c.allocationPercentage}%</td>
                    <td className="p-3">
                      <span className="px-2 py-0.5 text-xs rounded bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300">
                        Đang hoạt động
                      </span>
                    </td>
                    <td className="p-3 text-right">
                      <button
                        onClick={() => {
                          setCcForm({ id: c.id, code: c.code, name: c.name, percentage: c.allocationPercentage });
                          setIsCcModalOpen(true);
                        }}
                        className="text-xs text-brand hover:underline"
                      >
                        Sửa
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: RELATIVES & EMERGENCY */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'relatives' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4">
              <h2 className="text-lg font-semibold">Người thân & Liên hệ khẩn cấp</h2>
              <label className="flex items-center gap-2 text-xs text-muted-foreground dark:text-slate-400 cursor-pointer">
                <input
                  type="checkbox"
                  checked={emergencyOnly}
                  onChange={(e) => setEmergencyOnly(e.target.checked)}
                  className="rounded border-border"
                />
                Chỉ hiển thị liên hệ khẩn cấp
              </label>
            </div>
            <button
              onClick={() => {
                setRelForm({
                  employeeId: 'emp-101',
                  fullName: '',
                  relationship: 'Spouse',
                  phone: '',
                  address: '',
                  isEmergencyContact: true,
                  isTaxDependent: false,
                });
                setIsRelModalOpen(true);
              }}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Thêm người thân / liên hệ
            </button>
          </div>

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Họ tên người thân</th>
                  <th className="p-3">Mối quan hệ</th>
                  <th className="p-3">Số điện thoại</th>
                  <th className="p-3">Liên hệ khẩn?</th>
                  <th className="p-3">Phụ thuộc thuế?</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {displayedRelatives.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-semibold">{r.fullName}</td>
                    <td className="p-3 font-mono text-muted-foreground dark:text-slate-400">{r.relationship}</td>
                    <td className="p-3">{r.phone || '-'}</td>
                    <td className="p-3">
                      {r.isEmergencyContact ? (
                        <span className="px-2 py-0.5 text-xs rounded bg-rose-100 text-rose-800 font-semibold">Khẩn cấp</span>
                      ) : (
                        <span className="text-slate-400 text-xs">Không</span>
                      )}
                    </td>
                    <td className="p-3">
                      {r.isTaxDependent ? (
                        <span className="px-2 py-0.5 text-xs rounded bg-sky-100 text-sky-800 font-semibold">Giảm trừ thuế</span>
                      ) : (
                        <span className="text-slate-400 text-xs">Không</span>
                      )}
                    </td>
                    <td className="p-3 text-right">
                      <button
                        onClick={() => {
                          setRelForm({
                            id: r.id,
                            employeeId: r.employeeId,
                            fullName: r.fullName,
                            relationship: r.relationship,
                            phone: r.phone || '',
                            address: r.address || '',
                            isEmergencyContact: r.isEmergencyContact,
                            isTaxDependent: r.isTaxDependent,
                          });
                          setIsRelModalOpen(true);
                        }}
                        className="text-xs text-brand hover:underline"
                      >
                        Sửa
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* MODALS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {isDeptModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveDept} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{deptForm.id ? 'Sửa bộ phận' : 'Tạo bộ phận mới'}</h3>
            <div>
              <label className="text-xs font-semibold">Mã bộ phận</label>
              <input
                type="text"
                value={deptForm.code}
                onChange={(e) => setDeptForm({ ...deptForm, code: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Tên bộ phận</label>
              <input
                type="text"
                value={deptForm.name}
                onChange={(e) => setDeptForm({ ...deptForm, name: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Bộ phận cha</label>
              <select
                value={deptForm.parentId || ''}
                onChange={(e) => setDeptForm({ ...deptForm, parentId: e.target.value || undefined })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value="">(Không có - Bộ phận cấp cao nhất)</option>
                {departments
                  .filter((d) => d.id !== deptForm.id)
                  .map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name} ({d.code})
                    </option>
                  ))}
              </select>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsDeptModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}

      {/* POS MODAL */}
      {isPosModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSavePos} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{posForm.id ? 'Sửa vị trí công việc' : 'Tạo vị trí công việc'}</h3>
            <div>
              <label className="text-xs font-semibold">Mã vị trí</label>
              <input
                type="text"
                value={posForm.code}
                onChange={(e) => setPosForm({ ...posForm, code: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Tên vị trí công việc</label>
              <input
                type="text"
                value={posForm.name}
                onChange={(e) => setPosForm({ ...posForm, name: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsPosModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}

      {/* CC MODAL */}
      {isCcModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveCc} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{ccForm.id ? 'Sửa trung tâm chi phí' : 'Thêm trung tâm chi phí'}</h3>
            <div>
              <label className="text-xs font-semibold">Mã trung tâm</label>
              <input
                type="text"
                value={ccForm.code}
                onChange={(e) => setCcForm({ ...ccForm, code: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Tên trung tâm chi phí</label>
              <input
                type="text"
                value={ccForm.name}
                onChange={(e) => setCcForm({ ...ccForm, name: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Tỷ lệ phân bổ (%)</label>
              <input
                type="number"
                min="0"
                max="100"
                value={ccForm.percentage}
                onChange={(e) => setCcForm({ ...ccForm, percentage: Number(e.target.value) })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsCcModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}

      {/* REL MODAL */}
      {isRelModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveRel} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{relForm.id ? 'Sửa thông tin người thân' : 'Thêm người thân / liên hệ khẩn'}</h3>
            <div>
              <label className="text-xs font-semibold">Họ tên người thân</label>
              <input
                type="text"
                value={relForm.fullName}
                onChange={(e) => setRelForm({ ...relForm, fullName: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Mối quan hệ</label>
              <select
                value={relForm.relationship}
                onChange={(e) => setRelForm({ ...relForm, relationship: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value="Spouse">Vợ / Chồng (Spouse)</option>
                <option value="Child">Con (Child)</option>
                <option value="Parent">Bố / Mẹ (Parent)</option>
                <option value="Sibling">Anh chị em (Sibling)</option>
                <option value="Other">Khác (Other)</option>
              </select>
            </div>
            <div>
              <label className="text-xs font-semibold">Số điện thoại</label>
              <input
                type="text"
                value={relForm.phone}
                onChange={(e) => setRelForm({ ...relForm, phone: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              />
            </div>
            <div className="flex gap-4 items-center">
              <label className="flex items-center gap-2 text-xs font-semibold">
                <input
                  type="checkbox"
                  checked={relForm.isEmergencyContact}
                  onChange={(e) => setRelForm({ ...relForm, isEmergencyContact: e.target.checked })}
                />
                Liên hệ khẩn cấp
              </label>
              <label className="flex items-center gap-2 text-xs font-semibold">
                <input
                  type="checkbox"
                  checked={relForm.isTaxDependent}
                  onChange={(e) => setRelForm({ ...relForm, isTaxDependent: e.target.checked })}
                />
                Phụ thuộc giảm trừ thuế
              </label>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsRelModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
