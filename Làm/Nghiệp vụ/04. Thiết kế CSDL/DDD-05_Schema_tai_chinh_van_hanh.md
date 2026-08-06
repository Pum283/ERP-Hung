# DDD-05-v1.0 — Mô hình dữ liệu tài chính & vận hành (FIN, AST, FSM, PJM, BI)

> **Logical Data Model — Finance & Operations**
> *Database Design Document (DDD)* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-05-v1.0` |
| Tên | Mô hình dữ liệu tài chính & vận hành (FIN, AST, FSM, PJM, BI) |
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

Thiết kế schema **`fin`**, **`ast`**, **`fsm`**, **`pjm`**, **`bi`**.

---

## 2. Schema `fin`

### 2.1. Sơ đồ

```text
 account (COA) ── fiscal_period
 journal_entry ── journal_line
 ar_invoice / ar_receipt
 ap_invoice / ap_payment
 cash_book / bank_account / bank_txn
 tax_code / tax_txn
```

### 2.2. Thực thể

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `account` | Tài khoản KT | `id` | FK tenant; code unique |
| `fiscal_year` | Năm tài chính | `id` | FK tenant |
| `fiscal_period` | Kỳ KT | `id` | FK fiscal_year; status Open/Closed |
| `journal_entry` | Chứng từ ghi sổ | `id` | FK period |
| `journal_line` | Dòng Nợ/Có | `id` | FK journal, account |
| `ar_invoice` | Hóa đơn phải thu | `id` | FK customer ref |
| `ar_invoice_line` | Dòng HĐ | `id` | FK ar_invoice |
| `ar_receipt` | Phiếu thu | `id` | FK customer |
| `ar_allocation` | Khớp thu–hóa đơn | `id` | FK receipt, invoice |
| `ap_invoice` | Hóa đơn phải trả | `id` | FK vendor ref |
| `ap_payment` | Phiếu chi NCC | `id` | FK vendor |
| `cash_book` | Sổ quỹ | `id` | FK org |
| `bank_account` | TK ngân hàng | `id` | FK tenant |
| `bank_transaction` | Giao dịch NH | `id` | FK bank_account |
| `tax_code` | Mã thuế | `id` | FK tenant |
| `cost_center` | Trung tâm CP | `id` | FK tenant |

### 2.3. Ràng buộc FIN
- Mỗi `journal_entry` posted: tổng Nợ = tổng Có.
- Không post vào `fiscal_period` Closed (trừ chứng từ điều chỉnh có quyền).
- Nguồn module (HRM payroll, POS sale…) lưu `source_module`, `source_doc_id` trên journal/AR/AP.

### 2.4. `fin.journal_line`

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `journal_id` | UUID | YES | FK journal_entry |
| `line_no` | int | YES |  |
| `account_id` | UUID | YES | FK account |
| `debit` | numeric(18,2) | YES | ≥ 0 |
| `credit` | numeric(18,2) | YES | ≥ 0 |
| `cost_center_id` | UUID | NO |  |
| `partner_type` | varchar(20) | NO | Customer/Vendor/Employee |
| `partner_id` | UUID | NO | Ref mềm |
| `memo` | varchar(500) | NO |  |

---

## 3. Schema `ast`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `asset_category` | Nhóm tài sản | `id` | FK tenant |
| `asset` | Tài sản cố định | `id` | FK category; employee custodian |
| `asset_component` | Bộ phận TS | `id` | FK asset |
| `asset_acquisition` | Ghi tăng | `id` | FK asset; PUR/FIN ref |
| `depreciation_run` | Đợt KH | `id` | FK period |
| `depreciation_line` | Dòng KH | `id` | FK run, asset → FIN |
| `asset_transfer` | Điều chuyển TS | `id` | FK asset |
| `asset_disposal` | Thanh lý | `id` | FK asset |
| `asset_maintenance` | Bảo trì | `id` | FK asset |

---

## 4. Schema `fsm`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `service_contract` | HĐ dịch vụ/BH | `id` | FK customer |
| `ticket` | Ticket | `id` | FK customer; PRT/CRM |
| `work_order` | Phiếu kỹ thuật | `id` | FK ticket |
| `work_order_part` | Linh kiện dùng | `id` | FK WO; item → INV |
| `work_order_time` | Giờ công KT | `id` | FK WO; technician |
| `sla_policy` | Chính sách SLA | `id` | FK tenant |
| `technician_skill` | Kỹ năng KT | `id` | FK user/employee |
| `appointment` | Lịch hẹn | `id` | FK WO |

---

## 5. Schema `pjm`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `project` | Dự án | `id` | FK customer/contract ref |
| `project_member` | Thành viên | `id` | FK project, employee |
| `wbs_node` | Cấu trúc WBS | `id` | FK project |
| `task` | Công việc | `id` | FK wbs/project |
| `task_dependency` | Phụ thuộc task | `id` | FK task |
| `project_budget` | Ngân sách | `id` | FK project |
| `project_cost_actual` | Chi phí thực | `id` | source INV/FIN/HR |
| `project_milestone` | Mốc | `id` | FK project |
| `change_request` | CR dự án | `id` | FK project → WF |
| `project_document` | Tài liệu DA | `id` | FK file_object |

---

## 6. Schema `bi` (metadata)

> BI Phase 1 lưu **metadata & phân quyền dataset**; dữ liệu phân tích có thể là view/materialized view hoặc kho riêng.

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `dataset` | Đăng ký dataset | `id` | module nguồn + license |
| `dataset_field` | Trường dataset | `id` | FK dataset |
| `dashboard` | Dashboard | `id` | FK tenant |
| `dashboard_widget` | Widget | `id` | FK dashboard, dataset |
| `report_definition` | Định nghĩa báo cáo | `id` | FK tenant |
| `report_schedule` | Lịch gửi báo cáo | `id` | FK report |
| `report_run_log` | Lịch sử chạy | `id` | FK report |
| `bi_acl` | Quyền xem dataset/dashboard | `id` | FK role/user |

---

## 7. Liên kết tài chính xuyên module

| Nguồn | Đích FIN | Khóa truy vết |
|---|---|---|
| PosSaleCompleted / ShiftClosed | AR/Doanh thu/Quỹ | source_doc_id |
| CrmSalesOrder + Delivery | AR Invoice | correlation_id |
| PurGoodsReceived + Invoice | AP | PO/GRN ids |
| HrmPayrollPosted | Journal chi phí lương | payroll_period_id |
| AstDepreciationPosted | Journal KH | depreciation_run_id |
| FsmWorkOrderClosed | AR phí DV | work_order_id |

---

*Hết DDD-05-v1.0.*
