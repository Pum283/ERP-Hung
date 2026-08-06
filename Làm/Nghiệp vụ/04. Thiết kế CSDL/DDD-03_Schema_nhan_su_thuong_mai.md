# DDD-03-v1.0 — Mô hình dữ liệu nhân sự & thương mại (HRM, LMS, CRM, POS, PRT)

> **Logical Data Model — People & Commerce**
> *Database Design Document (DDD)* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-03-v1.0` |
| Tên | Mô hình dữ liệu nhân sự & thương mại (HRM, LMS, CRM, POS, PRT) |
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

Thiết kế schema **`hrm`**, **`lms`**, **`crm`**, **`pos`**, **`prt`** — vòng đời người và vòng đời bán hàng/khách hàng.

---

## 2. Schema `hrm`

### 2.1. Sơ đồ logic

```text
 job_title (→ sys.job_level) / employee_type
 department/job_level/manager sync ↔ sys.app_user
          \
           employee ── contract ── contract_appendix
              │
              ├─ employment_status_history
              ├─ shift_assignment ← shift_template
              ├─ attendance_punch → timesheet / timesheet_line
              ├─ leave_balance / leave_request
              ├─ payroll_period → payslip / payslip_line
              └─ offboarding_checklist
 recruitment_request → candidate → job_posting
```

### 2.2. Thực thể chính

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `employee` | Hồ sơ nhân sự | `id` | FK tenant; user_id; org_unit; department; job_level |
| `contract` | Hợp đồng LĐ | `id` | FK employee |
| `contract_appendix` | Phụ lục HĐ | `id` | FK contract |
| `employment_status_history` | Lịch sử trạng thái | `id` | FK employee |
| `recruitment_request` | Đề xuất tuyển | `id` | FK org/position |
| `candidate` | Ứng viên | `id` | FK request optional |
| `job_posting` | Tin tuyển | `id` | FK request |
| `onboarding_task` | Checklist nhận việc | `id` | FK employee |
| `shift_template` | Mẫu ca | `id` | FK tenant |
| `shift_assignment` | Xếp ca | `id` | FK employee, template |
| `attendance_punch` | Chấm công thô | `id` | FK employee |
| `timesheet` | Bảng công kỳ | `id` | FK tenant, period |
| `timesheet_line` | Dòng công NV | `id` | FK timesheet, employee |
| `leave_type` | Loại nghỉ | `id` | FK tenant |
| `leave_balance` | Quỹ phép | `id` | FK employee, leave_type |
| `leave_request` | Đơn nghỉ | `id` | FK employee |
| `payroll_period` | Kỳ lương | `id` | FK tenant |
| `payslip` | Phiếu lương | `id` | FK period, employee |
| `payslip_line` | Dòng lương | `id` | FK payslip |
| `transfer_order` | Điều động | `id` | FK employee |
| `offboarding_case` | Hồ sơ nghỉ việc | `id` | FK employee |

### 2.3. `hrm.employee` — cột trọng yếu

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES |  |
| `employee_code` | varchar(40) | YES | Unique theo tenant |
| `user_id` | UUID | NO | FK sys.app_user |
| `full_name` | varchar(200) | YES |  |
| `dob` | date | NO |  |
| `gender` | varchar(20) | NO |  |
| `national_id_enc` | bytea/varchar | NO | Mã hóa/field security |
| `org_unit_id` | UUID | YES | Đơn vị chính |
| `job_title_id` | UUID | NO |  |
| `employee_type_id` | UUID | NO |  |
| `status` | varchar(30) | YES | Probation/Active/Terminated… |
| `hire_date` | date | NO |  |
| `terminate_date` | date | NO |  |

### 2.4. Ràng buộc HRM
- `employee_code` không tái sử dụng sau terminate (BR-HRM-01).
- `payslip` chỉ tạo từ `timesheet` đã Locked (trừ điều chỉnh).
- Dữ liệu lương: cột nhạy cảm — ACL field-level ở API; DB có thể mã hóa cột.

---

## 3. Schema `lms`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `course` | Khóa học | `id` | FK tenant |
| `course_version` | Phiên bản nội dung | `id` | FK course |
| `lesson` | Bài học | `id` | FK course_version |
| `assessment` | Bài kiểm tra | `id` | FK course_version |
| `learning_path` | Lộ trình | `id` | FK tenant |
| `enrollment` | Ghi danh | `id` | FK course, learner |
| `learning_progress` | Tiến độ | `id` | FK enrollment |
| `assessment_attempt` | Lần thi | `id` | FK assessment, learner |
| `certificate` | Chứng chỉ | `id` | FK enrollment; → HRM event |
| `training_class` | Lớp offline | `id` | FK course |

`learner` tham chiếu `hrm.employee_id` và/hoặc `sys.app_user_id`.

---

## 4. Schema `crm`

### 4.1. Sơ đồ logic

```text
 customer ── contact
     │
     ├─ lead → opportunity → quote → sales_order → sales_order_line
     ├─ campaign / voucher
     ├─ activity / visit
     └─ conversation / case
```

### 4.2. Thực thể

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `customer` | Khách hàng | `id` | FK tenant |
| `contact` | Người liên hệ | `id` | FK customer |
| `lead` | Đầu mối | `id` | FK tenant |
| `opportunity` | Cơ hội | `id` | FK customer/lead |
| `quote` | Báo giá | `id` | FK customer |
| `quote_line` | Dòng báo giá | `id` | FK quote; item_id ref INV |
| `sales_order` | Đơn bán | `id` | FK customer |
| `sales_order_line` | Dòng đơn | `id` | FK sales_order; item_id |
| `campaign` | Chiến dịch | `id` | FK tenant |
| `voucher` | Mã KM | `id` | FK campaign optional |
| `activity` | Hoạt động CSKH/BH | `id` | FK customer |
| `sales_case` | Case/khiếu nại | `id` | FK customer |

### 4.3. `crm.sales_order` — cột trọng yếu

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES |  |
| `doc_no` | varchar(50) | YES | Sequence |
| `customer_id` | UUID | YES | FK customer |
| `status` | varchar(30) | YES | Draft…Confirmed…Closed |
| `order_date` | date | YES |  |
| `currency_code` | char(3) | YES |  |
| `total_amount` | numeric(18,2) | YES |  |
| `warehouse_id` | UUID | NO | Ref inv.warehouse |
| `promised_date` | date | NO |  |
| `correlation_id` | UUID | NO | E2E trace |

---

## 5. Schema `pos`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `store` | Cửa hàng | `id` | FK org_unit/tenant |
| `terminal` | Máy POS | `id` | FK store |
| `sellable_item` | Hàng bán / map item | `id` | FK store; item_id |
| `price_list` | Bảng giá | `id` | FK tenant |
| `price_list_item` | Giá theo SP | `id` | FK price_list, item |
| `recipe` | Định mức trừ kho | `id` | FK sellable_item |
| `recipe_line` | NVL của món | `id` | FK recipe; item_id |
| `cash_shift` | Ca quỹ | `id` | FK store, terminal, user |
| `pos_order` | Hóa đơn bán | `id` | FK shift |
| `pos_order_line` | Dòng bán | `id` | FK pos_order |
| `pos_payment` | Thanh toán | `id` | FK pos_order |

Trừ tồn: giao dịch đồng bộ với `inv` (API) + event `PosSaleCompleted`.

---

## 6. Schema `prt`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `portal_account` | Tài khoản cổng | `id` | FK tenant; link customer/vendor/user |
| `portal_role` | Vai trò cổng | `id` | FK tenant |
| `portal_notification` | Thông báo portal | `id` | FK account |
| `self_service_ticket` | Ticket KH tạo | `id` | → FSM/CRM ref |
| `portal_document_share` | Chia sẻ chứng từ | `id` | doc ref + ACL |

Portal **không** nhân bản đơn hàng/công nợ — đọc projection từ CRM/FIN/LOG theo quyền.

---

## 7. Index gợi ý

| Bảng | Index |
|---|---|
| hrm.employee | `uq(tenant_id, employee_code)`; `ix(tenant_id, org_unit_id, status)` |
| hrm.leave_request | `ix(tenant_id, status, from_date)` |
| crm.customer | `uq(tenant_id, code)`; `ix(tenant_id, tax_code)` |
| crm.sales_order | `uq(tenant_id, doc_no)`; `ix(tenant_id, status, order_date)` |
| pos.pos_order | `ix(shift_id)`; `ix(tenant_id, created_at)` |

---

*Hết DDD-03-v1.0.*
