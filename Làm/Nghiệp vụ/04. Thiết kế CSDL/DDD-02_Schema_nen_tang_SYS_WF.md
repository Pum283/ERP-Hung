# DDD-02-v1.0 — Mô hình dữ liệu schema nền tảng (SYS, WF)

> **Logical Data Model — Platform Schemas**
> *Database Design Document (DDD)* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-02-v1.0` |
| Tên | Mô hình dữ liệu schema nền tảng (SYS, WF) |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Thiết kế CSDL (Solution / DBA) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Đầu vào | SRS module v1.1 · INT v1.0 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ Database Design Document | Chờ duyệt |
| 1.0.1 | 04/08/2026 | Thêm `conversation` / `conversation_member` / `chat_message` (SYS-13) | Chờ duyệt |

---

## 1. Giới thiệu

Thiết kế các bảng cốt lõi schema **`sys`** và **`wf`** — nền cho mọi module nghiệp vụ.

---

## 2. Sơ đồ quan hệ logic (SYS)

```text
 tenant ─┬─ org_unit (Company/Branch)
         ├─ department (cây phòng; thuộc org_unit)
         ├─ job_level (default_scope_type: Own|Team|Department|All)
         ├─ app_user ─┬─ department_id / job_level_id / manager_user_id
         │            ├─ user_department (kiêm nhiệm)
         │            ├─ user_role ── role (bypass_data_scope)
         │            │                 └── role_permission ── permission
         │            ├─ user_data_scope (OrgUnit/Warehouse/Store…)
         │            └─ session
         ├─ field_permission (theo role + entity.field)
         ├─ menu_item (license + permission_code)
         ├─ license ── license_module
         ├─ sequence_rule / setting / file_object
         ├─ notification_* / audit_log / login_log
         ├─ conversation / conversation_member / chat_message  (SYS-13 nhắn tin)
         └─ api_key / webhook / integration_outbox|inbox
```

### 2.1. Mô hình phân quyền chuẩn (chốt)

1. **Role + Permission** — quyền chức năng API/UI (`{module}.{resource}.{action}`).
2. **Department + JobLevel** — tổ chức người và **data scope 4 tầng** (Own/Team/Department/All).
3. **user_data_scope** — phạm vi theo chi nhánh / kho / cửa hàng / dự án (ERP đa điểm).
4. **field_permission + menu_item** — bảo vệ trường nhạy cảm và menu.
5. Khi bật HRM: đồng bộ `employee.department_id / job_level_id / manager` → `app_user` (authz vẫn chạy nếu chưa mua HRM).

---

## 3. Bảng schema `sys`

### 3.1. Danh mục thực thể

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `tenant` | Không gian thuê bao | `id` | — |
| `org_unit` | Cây công ty/chi nhánh | `id` | FK tenant, parent_id |
| `department` | Cây phòng ban (scope Department) | `id` | FK tenant, org_unit, parent |
| `job_level` | Cấp bậc + default_scope_type | `id` | FK tenant |
| `app_user` | Tài khoản đăng nhập | `id` | FK tenant; dept/job_level/manager |
| `user_department` | Phòng kiêm nhiệm | `id` | FK user, department |
| `role` | Vai trò RBAC (+ bypass_data_scope) | `id` | FK tenant |
| `permission` | Catalog quyền chức năng | `id` | uq(code); module/resource/action |
| `role_permission` | Gán quyền cho role | `id` | FK role, permission |
| `user_role` | User ↔ Role (có hiệu lực/thu hồi) | `id` | FK user, role |
| `user_data_scope` | Phạm vi theo đối tượng (CN/kho…) | `id` | FK user + dimension/scope_id |
| `field_permission` | Quyền theo trường | `id` | FK role; entity.field |
| `menu_item` | Menu UI | `id` | permission_code + module |
| `license` | Hợp đồng license tenant | `id` | FK tenant |
| `license_module` | Module được bật | `id` | FK license; module_code |
| `sequence_rule` | Sinh số chứng từ | `id` | FK tenant; pattern |
| `setting` | Cấu hình key-value | `id` | FK tenant; key unique |
| `file_object` | Metadata file | `id` | FK tenant; storage_key |
| `notification_template` | Mẫu thông báo | `id` | FK tenant |
| `notification_log` | Lịch sử gửi | `id` | FK tenant |
| `conversation` | Hội thoại 1-1 / nhóm (SYS-13) | `id` | FK tenant; kind Direct/Group |
| `conversation_member` | Thành viên + unread/mute | `id` | FK conversation, app_user; uq(tenant,conv,user) |
| `chat_message` | Tin nhắn realtime | `id` | FK conversation, sender; attachment_file_id? |
| `audit_log` | Nhật ký thay đổi | `id` | FK tenant |
| `login_log` | Nhật ký đăng nhập | `id` | FK tenant, user |
| `api_key` | Khóa tích hợp | `id` | FK tenant |
| `webhook_subscription` | Webhook outbound | `id` | FK tenant |
| `integration_outbox` | Outbox event | `id` | FK tenant |
| `integration_inbox` | Inbox idempotent | `id` | FK tenant; uq event_id |
| `session` | Phiên đăng nhập | `id` | FK user |

### 3.2. `sys.job_level` — cấp bậc & scope mặc định

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES | FK tenant |
| `code` | varchar(40) | YES | STAFF/MANAGER/DIRECTOR… |
| `name` | varchar(200) | YES | Tên hiển thị |
| `level_order` | int | YES | Thứ tự cấp |
| `default_scope_type` | varchar(20) | YES | Own|Team|Department|All |
| `is_active` | boolean | YES |  |

### 3.3. `sys.department`

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES | FK tenant |
| `code` | varchar(40) | YES | Unique theo tenant |
| `name` | varchar(200) | YES |  |
| `parent_id` | UUID | NO | Cây phòng |
| `org_unit_id` | UUID | YES | Chi nhánh thuộc về |
| `manager_user_id` | UUID | NO | Trưởng phòng |
| `path` | varchar(500) | YES | Materialized path |
| `is_active` | boolean | YES |  |

### 3.4. `sys.app_user` — thuộc tính chính (authz)

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES | FK tenant |
| `username` | varchar(100) | YES | Unique theo tenant |
| `display_name` | varchar(200) | NO |  |
| `email` | varchar(255) | NO | Unique theo tenant nếu có |
| `password_hash` | varchar(255) | NO | Null nếu SSO-only |
| `status` | varchar(20) | YES | Active/Locked/Disabled |
| `primary_org_unit_id` | UUID | NO | Chi nhánh chính |
| `department_id` | UUID | NO | Phòng ban chính → scope Department |
| `job_level_id` | UUID | NO | → default_scope_type |
| `manager_user_id` | UUID | NO | → scope Team |
| `employee_id` | UUID | NO | Ref mềm hrm.employee |
| `failed_login_count` | int | YES | Chống brute-force |

### 3.5. `sys.role` / `sys.permission`

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `role.code` | varchar(50) | YES | Unique theo tenant |
| `role.bypass_data_scope` | boolean | YES | true → All (super) |
| `role.is_system` | boolean | YES | Không xóa |
| `permission.code` | varchar(100) | YES | {module}.{resource}.{action} |
| `permission.module_code` | varchar(10) | YES | SYS/HRM/… |
| `permission.resource` | varchar(80) | YES | employee, leave… |
| `permission.action` | varchar(40) | YES | Create/Read/Update/Delete/Approve… |

### 3.6. `sys.integration_outbox`

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES |  |
| `event_id` | UUID | YES | Unique publish |
| `event_type` | varchar(120) | YES | INT-03 |
| `aggregate_type` | varchar(80) | NO |  |
| `aggregate_id` | UUID | NO |  |
| `payload_json` | jsonb | YES | Envelope + payload |
| `status` | varchar(20) | YES | New/Published/Failed |
| `occurred_at` | timestamptz | YES |  |
| `published_at` | timestamptz | NO |  |
| `retry_count` | int | YES |  |

---

## 4. Schema `wf` — phê duyệt

### 4.1. Thực thể

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `wf_definition` | Định nghĩa quy trình | `id` | FK tenant; module/doc_type |
| `wf_definition_version` | Phiên bản quy trình | `id` | FK definition |
| `wf_node` | Bước duyệt | `id` | FK version |
| `wf_transition` | Chuyển bước | `id` | FK version |
| `wf_instance` | Phiên bản chạy | `id` | FK definition; doc ref |
| `wf_task` | Việc chờ duyệt | `id` | FK instance; assignee |
| `wf_task_action` | Hành động duyệt | `id` | FK task |
| `wf_delegation` | Ủy quyền | `id` | FK tenant; user |

### 4.2. Liên kết chứng từ nguồn
`wf_instance` lưu `source_module`, `source_doc_type`, `source_doc_id` (UUID) — **không** FK cứng sang mọi bảng chứng từ.

---

## 5. Ràng buộc & index gợi ý (SYS)

| Bảng | Index / Unique |
|---|---|
| app_user | `uq(tenant_id, username)`; `ix(tenant_id, email)`; `ix(department_id)`; `ix(manager_user_id)` |
| department | `uq(tenant_id, code)`; `ix(tenant_id, path)` |
| job_level | `uq(tenant_id, code)` |
| role | `uq(tenant_id, code)` |
| permission | `uq(code)` |
| role_permission | `uq(role_id, permission_id)` |
| user_role | `uq(user_id, role_id)` where active; `ix(user_id, is_active)` |
| user_department | `uq(user_id, department_id)` |
| user_data_scope | `ix(user_id, dimension, scope_id)` |
| field_permission | `uq(role_id, entity_type, field_name)` |
| menu_item | `uq(tenant_id, code)` hoặc global catalog |
| license_module | `uq(license_id, module_code)` |
| sequence_rule | `uq(tenant_id, code)` |
| integration_outbox | `uq(event_id)`; `ix(status, created_at)` |
| integration_inbox | `uq(event_id, consumer)` |
| audit_log | `ix(tenant_id, entity_type, entity_id, created_at)` |

---

## 6. Runtime gợi ý (không gắn stack)

1. API check permission code (không nhét full permission vào JWT).
2. Resolve effective scope: bypass role → else JobLevel → áp filter Own/Team/Department/All.
3. AND thêm filter `user_data_scope` theo chứng từ (`org_unit_id`, `warehouse_id`…).
4. `/me` trả roles, permission codes, effective_scope_type, accessible department ids.

---

## 7. Truy vết SRS
Ánh xạ SYS-03 (UC_SYS_023…033) và nhóm Auth trong `SRS_SYS_v1.1`. Tham chiếu mẫu Digi: Role/Permission/Department/JobLevel + `ScopeType`.

---

*Hết DDD-02-v1.0.*
