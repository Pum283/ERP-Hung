# DDD-01-v1.0 — Tổng quan & chuẩn thiết kế cơ sở dữ liệu

> **Database Architecture & Design Standards**
> *Database Design Document (DDD)* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-01-v1.0` |
| Tên | Tổng quan & chuẩn thiết kế cơ sở dữ liệu |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Thiết kế CSDL (Solution / DBA) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Đầu vào | SRS module v1.1 · INT v1.0 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ Database Design Document | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Thiết lập **kiến trúc dữ liệu thống nhất** cho ERP đa module: quy ước schema, multi-tenant, khóa, audit, soft-delete — làm chuẩn bắt buộc trước khi viết migration.

### 1.2. Phạm vi
- Mô hình logic theo schema module.
- Quy ước đặt tên, kiểu dữ liệu, ràng buộc chung.
- Hướng dẫn vật lý Phase 1 (chưa thay thế script SQL chi tiết từng môi trường).

### 1.3. Ngoài phạm vi
- Tối ưu query cụ thể từng báo cáo BI.
- Thiết kế kho dữ liệu analytic riêng (data warehouse) — thuộc BI nâng cao.

---

## 2. Kiến trúc CSDL Phase 1

```text
  ┌─────────────────────────────────────────────┐
  │         Database (1 cluster / tenant)        │
  │  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐   │
  │  │ sys │ │ hrm │ │ crm │ │ inv │ │ fin │…  │
  │  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘   │
  │         FK / Ref ID xuyên schema             │
  └─────────────────────────────────────────────┘
```

| Quyết định | Lựa chọn Phase 1 | Ghi chú |
|---|---|---|
| Phân tách module | **Schema** theo mã module | `sys`, `hrm`, `crm`… |
| Multi-tenant | Cột `tenant_id` trên hầu hết bảng | Có thể tách DB riêng tenant enterprise sau |
| Engine gợi ý | PostgreSQL 15+ *(hoặc SQL Server tương đương)* | Chốt kỹ thuật khi vào Source |
| Charset | UTF-8 | Tiếng Việt đầy đủ |
| Time | Lưu UTC; hiển thị theo TZ tenant | `timestamptz` |

### 2.1. Danh sách schema

| Schema | Module |
|---|---|
| `sys` | SYS |
| `hrm` | HRM |
| `lms` | LMS |
| `crm` | CRM |
| `pos` | POS |
| `pur` | PUR |
| `inv` | INV |
| `log` | LOG |
| `mfg` | MFG |
| `fsm` | FSM |
| `pjm` | PJM |
| `fin` | FIN |
| `ast` | AST |
| `wf` | WF |
| `bi` | BI (metadata; dataset có thể read-only) |
| `prt` | PRT |

---

## 3. Quy ước đặt tên

| Đối tượng | Quy ước | Ví dụ |
|---|---|---|
| Schema | `snake` ngắn = mã module | `hrm` |
| Bảng | `snake_case`, số ít hoặc danh từ nghiệp vụ | `employee`, `sales_order` |
| Cột | `snake_case` | `full_name`, `ordered_at` |
| PK | `{table}_id` hoặc `id` (UUID) | `employee_id` |
| FK | `{ref}_id` | `customer_id`, `tenant_id` |
| Unique | `uq_{table}_{cols}` | `uq_employee_code_tenant` |
| Index | `ix_{table}_{cols}` | `ix_so_tenant_status` |
| Check | `ck_{table}_{rule}` | `ck_qty_positive` |

### 3.1. Khóa chính
- Phase 1 khuyến nghị **UUID v7 / ULID** (hoặc UUID v4) cho PK phân tán & merge.
- Mã nghiệp vụ (`code`) **không** dùng làm PK; có unique theo tenant.

---

## 4. Cột chuẩn (mọi bảng nghiệp vụ)

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | Khóa chính |
| `tenant_id` | UUID | YES | FK → sys.tenant |
| `created_at` | timestamptz | YES | UTC |
| `created_by` | UUID | NO | FK → sys.app_user (nullable cho job) |
| `updated_at` | timestamptz | YES | UTC |
| `updated_by` | UUID | NO | User cập nhật cuối |
| `is_deleted` | boolean | YES | Mặc định false — soft delete |
| `deleted_at` | timestamptz | NO | Thời điểm xóa mềm |
| `row_version` | int/xid | YES | Optimistic concurrency |

### 4.1. Cột chứng từ (document header)

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `doc_no` | varchar(50) | YES | Số chứng từ theo sequence SYS |
| `doc_date` | date | YES | Ngày chứng từ |
| `status` | varchar(30) | YES | Vòng đời: Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | NO | Đơn vị/chi nhánh phát sinh |
| `currency_code` | char(3) | NO | ISO 4217 |
| `remark` | text | NO | Ghi chú |
| `posted_at` | timestamptz | NO | Thời điểm post sổ/kho |
| `correlation_id` | UUID | NO | Trace liên module / event |

---

## 5. Multi-tenant & phân quyền (RBAC + data scope 4 tầng)

### 5.1. Hai trục bắt buộc

| Trục | Câu hỏi | Cơ chế |
|---|---|---|
| Functional RBAC | Được **làm gì**? | `User → user_role → role → role_permission → permission` |
| Data scope | Được **thấy dữ liệu nào**? | `JobLevel.default_scope_type` (Own/Team/Department/All) + `user_data_scope` (chi nhánh/kho…) |

### 5.2. Data scope 4 tầng (tham chiếu Digi `ScopeType`)

| Giá trị | Ý nghĩa lọc dòng |
|---|---|
| `Own` | Chỉ bản ghi của chính user |
| `Team` | Bản thân + user có `manager_user_id = current` |
| `Department` | Phòng ban chính (+ kiêm nhiệm) và **cây con** `sys.department` |
| `All` | Không lọc theo người/phòng; vẫn tôn trọng tenant |

Thứ tự hiệu lực: nếu bất kỳ role active có `bypass_data_scope = true` → coi như `All`; ngược lại lấy `job_level.default_scope_type`. Phạm vi chi nhánh/kho/điểm bán bổ sung qua `sys.user_data_scope`.

### 5.3. Thực thể tổ chức liên quan authz

| Thực thể | Vai trò |
|---|---|
| `sys.org_unit` | Công ty / chi nhánh (pháp lý, kho thuộc CN…) |
| `sys.department` | Phòng ban — trục scope `Department` |
| `sys.job_level` | Cấp bậc — mang `default_scope_type` |
| `hrm.job_title` | Chức danh nghiệp vụ (khác JobLevel); có thể gợi ý `job_level_id` |

| Cơ chế | Thiết kế |
|---|---|
| Tenant isolation | Mọi query bắt buộc filter `tenant_id` (middleware + RLS tùy chọn) |
| Field-level | `sys.field_permission` (Hidden/Masked/Read/Write) |
| Menu | `sys.menu_item` lọc theo license module + `permission_code` |
| Row Level Security | Khuyến nghị bật RLS PostgreSQL ở Phase 2 hoặc tenant nhạy cảm |
| Cross-tenant | **Cấm** trừ Super Platform Admin (ngoài phạm vi app tenant) |

---

## 6. Trạng thái & máy trạng thái

- Lưu `status` dạng mã ổn định (English snake/Pascal), không lưu nhãn UI.
- Chuyển trạng thái ghi `status_history` (bảng riêng hoặc audit) với `from_status`, `to_status`, `changed_by`, `reason`.
- Sau `Posted`/`Locked`: không update trực tiếp; dùng chứng từ điều chỉnh.

---

## 7. Tiền tệ, số lượng, làm tròn

| Loại | Kiểu | Quy ước |
|---|---|---|
| Số lượng | `numeric(18,6)` | Làm tròn theo UoM |
| Đơn giá / thành tiền | `numeric(18,4)` / `numeric(18,2)` | Tiền theo currency |
| Tỷ giá | `numeric(18,8)` | Tại thời điểm chứng từ |
| Thuế % | `numeric(9,4)` | |

---

## 8. File, audit, outbox

| Kho | Schema.bảng | Ghi chú |
|---|---|---|
| File metadata | `sys.file_object` | Binary ở object storage |
| Audit | `sys.audit_log` | before/after JSON |
| Login | `sys.login_log` | |
| Outbox | `sys.integration_outbox` | Đồng bộ event INT-03 |
| Inbox | `sys.integration_inbox` | Idempotency consumer |

---

## 9. Quan hệ xuyên schema

1. FK vật lý **khuyến nghị** khi cùng DB và module luôn đi kèm (ví dụ `inv` → `sys`).
2. Với soft dependency: lưu **UUID tham chiếu** + không bắt buộc FK cứng (tránh gãy khi tắt module) — kiểm tra tồn tại ở tầng ứng dụng.
3. Snapshot mã/tên trên dòng chứng từ để giữ lịch sử khi master đổi.

---

## 10. Ánh xạ tài liệu

| Nội dung | Tài liệu |
|---|---|
| Schema SYS/WF | DDD-02 |
| HRM…PRT thương mại | DDD-03 |
| PUR…MFG | DDD-04 |
| FIN…BI | DDD-05 |
| Index, bảo mật, migration | DDD-06 |
| Sự kiện | INT-03 |

---

*Hết DDD-01-v1.0.*
