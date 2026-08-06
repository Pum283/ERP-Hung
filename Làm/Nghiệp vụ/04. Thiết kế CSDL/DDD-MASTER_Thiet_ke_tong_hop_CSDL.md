# DDD-MASTER-v1.0 — Thiết kế tổng hợp cơ sở dữ liệu

> **Database Design Document — Master Consolidated**
> Tài liệu tổng hợp danh mục toàn bộ bảng và mô tả chi tiết từng trường dữ liệu.
> Phiên bản **1.0** · Ngày 04/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Nguồn: DDD-01…06 · SRS module v1.1 · INT v1.0. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-MASTER-v1.0` |
| Tên | Thiết kế tổng hợp cơ sở dữ liệu |
| Phiên bản | 1.0 |
| Ngày | 04/08/2026 |
| Số bảng | 173 |
| Số trường (ước lượng liệt kê) | 2666 |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 04/08/2026 | Tổng hợp danh mục bảng + chi tiết trường toàn hệ | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Cung cấp **một tài liệu duy nhất** để tra cứu nhanh toàn bộ bảng CSDL ERP và chi tiết từng trường — phục vụ BA, Solution, DBA và Dev khi thiết kế migration / ORM.

### 1.2. Cấu trúc tài liệu
1. **Phần A** — Danh mục tổng hợp tất cả bảng (module · nhóm · tên · chức năng).
2. **Phần B** — Mô tả chi tiết trường từng bảng (tên · kiểu · ý nghĩa · ghi chú).

### 1.3. Quy ước
- Tên bảng dạng `schema.table` (PostgreSQL-oriented).
- Hầu hết bảng nghiệp vụ có cột chuẩn: `id`, `tenant_id`, `created_*`, `updated_*`, `is_deleted`, `row_version` (xem DDD-01).
- Kiểu dữ liệu là **gợi ý logic Phase 1**; có thể map sang SQL Server tương đương (DDD-06).

### 1.4. Mô hình phân quyền (chốt sớm — hạn chế sửa lại)

| Trục | Thực thể chính | Nội dung |
|---|---|---|
| Quyền chức năng | `role`, `permission`, `role_permission`, `user_role` | User được **làm gì** (`module.resource.action`) |
| Tổ chức người | `department`, `job_level`, `app_user`, `user_department` | Phòng ban + cấp bậc |
| Data scope 4 tầng | `job_level.default_scope_type` + `role.bypass_data_scope` | Own / Team / Department / All |
| Phạm vi đa điểm | `user_data_scope` | Chi nhánh / kho / cửa hàng / dự án |
| Bổ sung | `field_permission`, `menu_item` | Trường nhạy cảm + menu UI |

Chi tiết quan hệ và runtime: **DDD-01 §5** và **DDD-02 §2–3**. Tham chiếu mẫu: Digi ERP (`ScopeType` + Role/Permission/Department/JobLevel).

---

## 2. Phần A — Danh mục tổng hợp tất cả bảng

Tổng số: **173 bảng**.

| STT | Module | Nhóm bảng | Tên bảng | Chức năng bảng |
|---:|---|---|---|---|
| 1 | SYS | Tenant & tổ chức | `sys.tenant` | Không gian dữ liệu khách hàng thuê bao |
| 2 | SYS | Tenant & tổ chức | `sys.org_unit` | Cây tổ chức pháp lý / chi nhánh (Company/Branch) |
| 3 | SYS | Tenant & tổ chức | `sys.department` | Phòng ban (cây phòng) — trục data scope Department |
| 4 | SYS | Tenant & tổ chức | `sys.job_level` | Cấp bậc — gắn mặc định data scope 4 tầng |
| 5 | SYS | Người dùng & bảo mật | `sys.app_user` | Tài khoản đăng nhập hệ thống |
| 6 | SYS | Người dùng & bảo mật | `sys.user_department` | Phòng ban kiêm nhiệm / phụ của user |
| 7 | SYS | Người dùng & bảo mật | `sys.role` | Vai trò RBAC (quyền chức năng) |
| 8 | SYS | Người dùng & bảo mật | `sys.permission` | Danh mục mã quyền chức năng (catalog toàn sản phẩm) |
| 9 | SYS | Người dùng & bảo mật | `sys.role_permission` | Gán permission vào role |
| 10 | SYS | Người dùng & bảo mật | `sys.user_role` | Gán role cho user |
| 11 | SYS | Người dùng & bảo mật | `sys.user_data_scope` | Phạm vi dữ liệu theo đối tượng (bổ sung / ghi đè) |
| 12 | SYS | Người dùng & bảo mật | `sys.field_permission` | Quyền theo trường nhạy cảm (field-level) |
| 13 | SYS | Người dùng & bảo mật | `sys.menu_item` | Menu chức năng UI (lọc theo license + permission) |
| 14 | SYS | Người dùng & bảo mật | `sys.session` | Phiên đăng nhập đang hoạt động |
| 15 | SYS | License & cấu hình | `sys.license` | Hợp đồng / gói license của tenant |
| 16 | SYS | License & cấu hình | `sys.license_module` | Module được bật trong license |
| 17 | SYS | License & cấu hình | `sys.sequence_rule` | Quy tắc sinh số chứng từ |
| 18 | SYS | License & cấu hình | `sys.setting` | Cấu hình key-value theo tenant |
| 19 | SYS | File & thông báo | `sys.file_object` | Metadata file đính kèm (binary ở object storage) |
| 20 | SYS | File & thông báo | `sys.notification_template` | Mẫu thông báo email/SMS/in-app |
| 21 | SYS | File & thông báo | `sys.notification_log` | Lịch sử gửi thông báo |
| 22 | SYS | Nhắn tin realtime | `sys.conversation` | Hội thoại 1-1 / nhóm (SYS-13) |
| 23 | SYS | Nhắn tin realtime | `sys.conversation_member` | Thành viên hội thoại + unread/mute |
| 24 | SYS | Nhắn tin realtime | `sys.chat_message` | Tin nhắn realtime |
| 25 | SYS | Audit & tích hợp | `sys.audit_log` | Nhật ký thay đổi dữ liệu |
| 26 | SYS | Audit & tích hợp | `sys.login_log` | Nhật ký đăng nhập / thất bại |
| 27 | SYS | Audit & tích hợp | `sys.api_key` | Khóa API tích hợp |
| 28 | SYS | Audit & tích hợp | `sys.webhook_subscription` | Đăng ký webhook outbound |
| 29 | SYS | Audit & tích hợp | `sys.integration_outbox` | Hàng đợi phát sự kiện (Outbox) |
| 30 | SYS | Audit & tích hợp | `sys.integration_inbox` | Inbox chống xử lý trùng sự kiện |
| 31 | WF | Định nghĩa quy trình | `wf.wf_definition` | Định nghĩa quy trình duyệt theo loại chứng từ |
| 32 | WF | Định nghĩa quy trình | `wf.wf_definition_version` | Phiên bản quy trình (immutable khi đã chạy) |
| 33 | WF | Định nghĩa quy trình | `wf.wf_node` | Bước/nút trong quy trình |
| 34 | WF | Định nghĩa quy trình | `wf.wf_transition` | Chuyển tiếp giữa các bước |
| 35 | WF | Thực thi duyệt | `wf.wf_instance` | Một lần chạy quy trình trên chứng từ |
| 36 | WF | Thực thi duyệt | `wf.wf_task` | Việc chờ duyệt của một bước |
| 37 | WF | Thực thi duyệt | `wf.wf_task_action` | Hành động duyệt trên task |
| 38 | WF | Thực thi duyệt | `wf.wf_delegation` | Ủy quyền duyệt tạm thời |
| 39 | HRM | Danh mục nhân sự | `hrm.job_title` | Danh mục chức danh (khác JobLevel phân quyền) |
| 40 | HRM | Danh mục nhân sự | `hrm.employee_type` | Loại nhân sự (CT/TV/Part-time…) |
| 41 | HRM | Hồ sơ nhân sự | `hrm.employee` | Hồ sơ nhân sự master |
| 42 | HRM | Hồ sơ nhân sự | `hrm.employment_status_history` | Lịch sử thay đổi trạng thái nhân sự |
| 43 | HRM | Hợp đồng | `hrm.contract` | Hợp đồng lao động |
| 44 | HRM | Hợp đồng | `hrm.contract_appendix` | Phụ lục hợp đồng |
| 45 | HRM | Tuyển dụng | `hrm.recruitment_request` | Phiếu đề xuất tuyển dụng |
| 46 | HRM | Tuyển dụng | `hrm.candidate` | Hồ sơ ứng viên |
| 47 | HRM | Tuyển dụng | `hrm.job_posting` | Tin tuyển dụng |
| 48 | HRM | Chấm công & ca | `hrm.shift_template` | Mẫu ca làm việc |
| 49 | HRM | Chấm công & ca | `hrm.shift_assignment` | Phân công ca cho nhân viên |
| 50 | HRM | Chấm công & ca | `hrm.attendance_punch` | Bản ghi chấm công thô |
| 51 | HRM | Chấm công & ca | `hrm.timesheet` | Bảng công theo kỳ |
| 52 | HRM | Chấm công & ca | `hrm.timesheet_line` | Dòng công từng nhân viên trong kỳ |
| 53 | HRM | Nghỉ phép | `hrm.leave_type` | Danh mục loại nghỉ |
| 54 | HRM | Nghỉ phép | `hrm.leave_balance` | Quỹ phép của nhân viên |
| 55 | HRM | Nghỉ phép | `hrm.leave_request` | Đơn xin nghỉ |
| 56 | HRM | Lương | `hrm.payroll_period` | Kỳ tính lương |
| 57 | HRM | Lương | `hrm.payslip` | Phiếu lương nhân viên |
| 58 | HRM | Lương | `hrm.payslip_line` | Chi tiết dòng phiếu lương |
| 59 | HRM | Biến động & nghỉ việc | `hrm.transfer_order` | Lệnh điều động / thăng chức |
| 60 | HRM | Biến động & nghỉ việc | `hrm.offboarding_case` | Hồ sơ nghỉ việc / offboarding |
| 61 | LMS | Nội dung đào tạo | `lms.course` | Khóa học |
| 62 | LMS | Nội dung đào tạo | `lms.course_version` | Phiên bản nội dung khóa học |
| 63 | LMS | Nội dung đào tạo | `lms.lesson` | Bài học trong khóa |
| 64 | LMS | Nội dung đào tạo | `lms.assessment` | Bài kiểm tra / bài thi |
| 65 | LMS | Ghi danh & tiến độ | `lms.enrollment` | Ghi danh học viên vào khóa |
| 66 | LMS | Ghi danh & tiến độ | `lms.assessment_attempt` | Lần làm bài kiểm tra |
| 67 | LMS | Ghi danh & tiến độ | `lms.certificate` | Chứng chỉ hoàn thành |
| 68 | LMS | Lớp học | `lms.training_class` | Lớp đào tạo offline/hybrid |
| 69 | CRM | Khách hàng | `crm.customer` | Khách hàng / tổ chức mua hàng |
| 70 | CRM | Khách hàng | `crm.contact` | Người liên hệ của khách hàng |
| 71 | CRM | Phễu bán | `crm.lead` | Đầu mối tiềm năng |
| 72 | CRM | Phễu bán | `crm.opportunity` | Cơ hội bán hàng |
| 73 | CRM | Báo giá & đơn | `crm.quote` | Báo giá |
| 74 | CRM | Báo giá & đơn | `crm.quote_line` | Dòng báo giá |
| 75 | CRM | Báo giá & đơn | `crm.sales_order` | Đơn bán hàng |
| 76 | CRM | Báo giá & đơn | `crm.sales_order_line` | Dòng đơn bán |
| 77 | CRM | Marketing & CSKH | `crm.campaign` | Chiến dịch marketing |
| 78 | CRM | Marketing & CSKH | `crm.sales_case` | Case / khiếu nại CSKH |
| 79 | POS | Cấu hình điểm bán | `pos.store` | Cửa hàng / điểm bán |
| 80 | POS | Cấu hình điểm bán | `pos.terminal` | Máy POS / terminal |
| 81 | POS | Cấu hình điểm bán | `pos.price_list` | Bảng giá bán |
| 82 | POS | Cấu hình điểm bán | `pos.price_list_item` | Giá theo sản phẩm trong bảng giá |
| 83 | POS | Giao dịch bán | `pos.cash_shift` | Ca quỹ thu ngân |
| 84 | POS | Giao dịch bán | `pos.pos_order` | Hóa đơn / giao dịch bán tại quầy |
| 85 | POS | Giao dịch bán | `pos.pos_order_line` | Dòng hàng trên hóa đơn POS |
| 86 | POS | Giao dịch bán | `pos.pos_payment` | Thanh toán của hóa đơn POS |
| 87 | POS | Định mức | `pos.recipe` | Định mức trừ kho cho món/SP |
| 88 | POS | Định mức | `pos.recipe_line` | Dòng NVL trong định mức |
| 89 | PUR | Nhà cung cấp | `pur.vendor` | Nhà cung cấp |
| 90 | PUR | Nhà cung cấp | `pur.vendor_item_price` | Bảng giá mua theo NCC–hàng |
| 91 | PUR | Chứng từ mua | `pur.purchase_requisition` | Phiếu yêu cầu mua hàng (PR) |
| 92 | PUR | Chứng từ mua | `pur.purchase_requisition_line` | Dòng PR |
| 93 | PUR | Chứng từ mua | `pur.purchase_order` | Đơn mua hàng (PO) |
| 94 | PUR | Chứng từ mua | `pur.purchase_order_line` | Dòng PO |
| 95 | PUR | Chứng từ mua | `pur.goods_receipt` | Phiếu nhập hàng từ NCC (GRN) |
| 96 | PUR | Chứng từ mua | `pur.goods_receipt_line` | Dòng GRN |
| 97 | INV | Master hàng hóa | `inv.item` | Danh mục hàng hóa / NVL / TP / dịch vụ |
| 98 | INV | Master hàng hóa | `inv.item_category` | Nhóm hàng hóa |
| 99 | INV | Master hàng hóa | `inv.uom_conversion` | Quy đổi đơn vị tính theo hàng |
| 100 | INV | Kho & vị trí | `inv.warehouse` | Kho hàng |
| 101 | INV | Kho & vị trí | `inv.bin_location` | Vị trí / ngăn trong kho |
| 102 | INV | Tồn kho | `inv.stock_balance` | Tồn kho hiện tại theo chiều item–kho–vị trí–lô |
| 103 | INV | Tồn kho | `inv.lot` | Lô hàng |
| 104 | INV | Tồn kho | `inv.serial_no` | Số serial theo từng đơn vị hàng |
| 105 | INV | Chứng từ kho | `inv.stock_document` | Chứng từ kho (nhập/xuất/chuyển/điều chỉnh) |
| 106 | INV | Chứng từ kho | `inv.stock_document_line` | Dòng chứng từ kho |
| 107 | INV | Giữ hàng | `inv.reservation` | Phiếu giữ hàng (reserve) |
| 108 | INV | Giữ hàng | `inv.reservation_line` | Dòng giữ hàng |
| 109 | INV | Kiểm kê | `inv.stock_count` | Phiếu kiểm kê |
| 110 | INV | Kiểm kê | `inv.stock_count_line` | Dòng kiểm kê |
| 111 | LOG | Vận hành giao hàng | `log.carrier` | Đơn vị vận chuyển / 3PL |
| 112 | LOG | Vận hành giao hàng | `log.vehicle` | Phương tiện giao hàng |
| 113 | LOG | Vận hành giao hàng | `log.driver` | Tài xế / nhân viên giao |
| 114 | LOG | Vận hành giao hàng | `log.shipment` | Chuyến / lô giao hàng |
| 115 | LOG | Vận hành giao hàng | `log.shipment_line` | Dòng hàng trên chuyến giao |
| 116 | LOG | Vận hành giao hàng | `log.shipment_tracking` | Mốc tracking trạng thái giao |
| 117 | LOG | Vận hành giao hàng | `log.cod_collection` | Thu hộ COD |
| 118 | MFG | Định mức & quy trình | `mfg.bom_header` | BOM định mức NVL cho thành phẩm |
| 119 | MFG | Định mức & quy trình | `mfg.bom_line` | Dòng NVL trong BOM |
| 120 | MFG | Định mức & quy trình | `mfg.routing` | Quy trình sản xuất |
| 121 | MFG | Định mức & quy trình | `mfg.routing_operation` | Công đoạn trong routing |
| 122 | MFG | Lệnh sản xuất | `mfg.work_order` | Lệnh sản xuất |
| 123 | MFG | Lệnh sản xuất | `mfg.work_order_material` | NVL cấp phát / định mức cho LSX |
| 124 | MFG | Lệnh sản xuất | `mfg.work_order_output` | Nhập thành phẩm từ LSX |
| 125 | MFG | QC | `mfg.qc_inspection` | Phiếu kiểm chất lượng |
| 126 | FIN | Danh mục kế toán | `fin.account` | Hệ thống tài khoản kế toán (COA) |
| 127 | FIN | Danh mục kế toán | `fin.fiscal_year` | Năm tài chính |
| 128 | FIN | Danh mục kế toán | `fin.fiscal_period` | Kỳ kế toán (tháng/quý) |
| 129 | FIN | Sổ cái | `fin.journal_entry` | Chứng từ ghi sổ / bút toán |
| 130 | FIN | Sổ cái | `fin.journal_line` | Dòng Nợ/Có của bút toán |
| 131 | FIN | Công nợ phải thu | `fin.ar_invoice` | Hóa đơn phải thu |
| 132 | FIN | Công nợ phải thu | `fin.ar_receipt` | Phiếu thu tiền KH |
| 133 | FIN | Công nợ phải thu | `fin.ar_allocation` | Khớp phiếu thu với hóa đơn |
| 134 | FIN | Công nợ phải trả | `fin.ap_invoice` | Hóa đơn phải trả NCC |
| 135 | FIN | Công nợ phải trả | `fin.ap_payment` | Phiếu chi trả NCC |
| 136 | FIN | Quỹ & ngân hàng | `fin.bank_account` | Tài khoản ngân hàng / quỹ |
| 137 | FIN | Quỹ & ngân hàng | `fin.bank_transaction` | Giao dịch ngân hàng / quỹ |
| 138 | FIN | Danh mục kế toán | `fin.cost_center` | Trung tâm chi phí |
| 139 | FIN | Thuế | `fin.tax_code` | Mã thuế |
| 140 | AST | Tài sản | `ast.asset_category` | Nhóm tài sản cố định |
| 141 | AST | Tài sản | `ast.asset` | Hồ sơ tài sản cố định |
| 142 | AST | Tài sản | `ast.asset_acquisition` | Chứng từ ghi tăng TS |
| 143 | AST | Khấu hao | `ast.depreciation_run` | Đợt chạy khấu hao |
| 144 | AST | Khấu hao | `ast.depreciation_line` | Dòng khấu hao theo tài sản |
| 145 | AST | Biến động TS | `ast.asset_transfer` | Điều chuyển tài sản |
| 146 | AST | Biến động TS | `ast.asset_disposal` | Thanh lý / ghi giảm TS |
| 147 | FSM | Dịch vụ kỹ thuật | `fsm.service_contract` | Hợp đồng dịch vụ / bảo hành |
| 148 | FSM | Dịch vụ kỹ thuật | `fsm.sla_policy` | Chính sách SLA |
| 149 | FSM | Dịch vụ kỹ thuật | `fsm.ticket` | Ticket yêu cầu hỗ trợ / sự cố |
| 150 | FSM | Dịch vụ kỹ thuật | `fsm.work_order` | Phiếu công việc kỹ thuật hiện trường |
| 151 | FSM | Dịch vụ kỹ thuật | `fsm.work_order_part` | Linh kiện dùng trên phiếu KT |
| 152 | FSM | Dịch vụ kỹ thuật | `fsm.work_order_time` | Giờ công kỹ thuật viên |
| 153 | FSM | Dịch vụ kỹ thuật | `fsm.appointment` | Lịch hẹn hiện trường |
| 154 | PJM | Dự án | `pjm.project` | Dự án |
| 155 | PJM | Dự án | `pjm.project_member` | Thành viên dự án |
| 156 | PJM | Kế hoạch | `pjm.wbs_node` | Nút WBS / cấu trúc phân rã công việc |
| 157 | PJM | Kế hoạch | `pjm.task` | Công việc / task dự án |
| 158 | PJM | Kế hoạch | `pjm.milestone` | Mốc dự án |
| 159 | PJM | Chi phí & thay đổi | `pjm.project_budget_line` | Dòng ngân sách dự án |
| 160 | PJM | Chi phí & thay đổi | `pjm.change_request` | Yêu cầu thay đổi phạm vi/CR |
| 161 | BI | Metadata BI | `bi.dataset` | Đăng ký dataset phân tích |
| 162 | BI | Metadata BI | `bi.dataset_field` | Trường trong dataset |
| 163 | BI | Metadata BI | `bi.dashboard` | Dashboard tổng hợp |
| 164 | BI | Metadata BI | `bi.dashboard_widget` | Widget trên dashboard |
| 165 | BI | Metadata BI | `bi.report_definition` | Định nghĩa báo cáo |
| 166 | BI | Metadata BI | `bi.report_schedule` | Lịch chạy/gửi báo cáo |
| 167 | BI | Metadata BI | `bi.bi_acl` | Phân quyền xem dataset/dashboard |
| 168 | PRT | Cổng khách hàng | `prt.portal_account` | Tài khoản đăng nhập cổng KH/NCC |
| 169 | PRT | Cổng khách hàng | `prt.portal_role` | Vai trò trên cổng |
| 170 | PRT | Cổng khách hàng | `prt.portal_account_role` | Gán role cho tài khoản cổng |
| 171 | PRT | Cổng khách hàng | `prt.self_service_ticket` | Ticket KH tự tạo trên cổng |
| 172 | PRT | Cổng khách hàng | `prt.portal_notification` | Thông báo hiển thị trên cổng |
| 173 | PRT | Cổng khách hàng | `prt.portal_document_share` | Chia sẻ chứng từ cho tài khoản cổng |

---

## 3. Phần B — Mô tả chi tiết trường theo từng bảng

Mỗi bảng dưới đây có bảng con với các cột: **Tên trường**, **Kiểu dữ liệu**, **Ý nghĩa**, **Ghi chú**.

### 3.1. Module SYS

#### Bảng 1. `sys.tenant` — Không gian dữ liệu khách hàng thuê bao

- **Module:** SYS
- **Nhóm bảng:** Tenant & tổ chức

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(30) | Mã tenant | Unique toàn hệ |
| `name` | varchar(200) | Tên tenant |  |
| `status` | varchar(20) | Trạng thái | Active/Suspended/Closed |
| `timezone` | varchar(64) | Múi giờ mặc định | VD Asia/Ho_Chi_Minh |
| `default_locale` | varchar(10) | Ngôn ngữ mặc định | vi-VN |
| `default_currency` | char(3) | Tiền tệ mặc định | VND |

#### Bảng 2. `sys.org_unit` — Cây tổ chức pháp lý / chi nhánh (Company/Branch)

- **Module:** SYS
- **Nhóm bảng:** Tenant & tổ chức

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã đơn vị | Unique theo tenant |
| `name` | varchar(200) | Tên đơn vị |  |
| `parent_id` | UUID | Đơn vị cha | FK self; null = gốc |
| `unit_type` | varchar(30) | Loại đơn vị | Company/Branch/Division (không dùng thay Department) |
| `path` | varchar(500) | Đường dẫn cây | Materialized path |
| `manager_user_id` | UUID | Người quản lý đơn vị | Ref app_user; optional |
| `sort_order` | int | Thứ tự hiển thị |  |
| `is_active` | boolean | Còn hiệu lực |  |

#### Bảng 3. `sys.department` — Phòng ban (cây phòng) — trục data scope Department

- **Module:** SYS
- **Nhóm bảng:** Tenant & tổ chức

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã phòng ban | Unique theo tenant |
| `name` | varchar(200) | Tên phòng ban |  |
| `parent_id` | UUID | Phòng ban cha | FK self; null = gốc |
| `org_unit_id` | UUID | Chi nhánh / đơn vị thuộc về | FK org_unit (Branch) |
| `manager_user_id` | UUID | Trưởng phòng | Ref app_user |
| `path` | varchar(500) | Đường dẫn cây phòng | Materialized path — scope gồm con |
| `sort_order` | int | Thứ tự hiển thị |  |
| `is_active` | boolean | Còn hiệu lực |  |

#### Bảng 4. `sys.job_level` — Cấp bậc — gắn mặc định data scope 4 tầng

- **Module:** SYS
- **Nhóm bảng:** Tenant & tổ chức

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã cấp bậc | Unique theo tenant; VD STAFF/MANAGER/DIRECTOR |
| `name` | varchar(200) | Tên cấp bậc |  |
| `level_order` | int | Thứ tự cấp (cao → thấp hoặc ngược) | Dùng so sánh cấp |
| `default_scope_type` | varchar(20) | Phạm vi dữ liệu mặc định | Own\|Team\|Department\|All (Digi ScopeType) |
| `description` | text | Mô tả |  |
| `is_active` | boolean | Còn dùng |  |

#### Bảng 5. `sys.app_user` — Tài khoản đăng nhập hệ thống

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `username` | varchar(100) | Tên đăng nhập | Unique theo tenant |
| `display_name` | varchar(200) | Tên hiển thị |  |
| `email` | varchar(255) | Email | Unique theo tenant nếu có |
| `phone` | varchar(30) | Số điện thoại |  |
| `password_hash` | varchar(255) | Hash mật khẩu | Null nếu SSO-only |
| `status` | varchar(20) | Trạng thái tài khoản | Active/Locked/Disabled |
| `must_change_password` | boolean | Bắt buộc đổi mật khẩu |  |
| `last_login_at` | timestamptz | Lần đăng nhập cuối |  |
| `failed_login_count` | int | Số lần đăng nhập sai liên tiếp | Reset khi login OK |
| `locked_until` | timestamptz | Khóa tạm đến thời điểm |  |
| `primary_org_unit_id` | UUID | Chi nhánh/đơn vị chính | FK org_unit |
| `department_id` | UUID | Phòng ban chính | FK department — nền scope Department |
| `job_level_id` | UUID | Cấp bậc | FK job_level — lấy default_scope_type |
| `manager_user_id` | UUID | Quản lý trực tiếp | FK self — nền scope Team |
| `employee_id` | UUID | Liên kết hồ sơ NV | Ref mềm hrm.employee; sync khi có HRM |

#### Bảng 6. `sys.user_department` — Phòng ban kiêm nhiệm / phụ của user

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `user_id` | UUID | Người dùng | FK app_user |
| `department_id` | UUID | Phòng ban | FK department |
| `is_primary` | boolean | Có phải phòng chính | Chỉ 1 primary/user |
| `valid_from` | date | Hiệu lực từ |  |
| `valid_to` | date | Hiệu lực đến | Null = không hạn |

#### Bảng 7. `sys.role` — Vai trò RBAC (quyền chức năng)

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã role | Unique theo tenant |
| `name` | varchar(200) | Tên role |  |
| `description` | text | Mô tả |  |
| `bypass_data_scope` | boolean | Bỏ qua lọc data scope | true = coi như All + full quyền nếu là super |
| `is_system` | boolean | Role hệ thống không xóa | VD SUPER_ADMIN |
| `is_active` | boolean | Đang hiệu lực |  |
| `sort_order` | int | Thứ tự hiển thị |  |

#### Bảng 8. `sys.permission` — Danh mục mã quyền chức năng (catalog toàn sản phẩm)

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `module_code` | varchar(10) | Module sở hữu quyền | SYS/HRM/CRM… |
| `code` | varchar(100) | Mã quyền | Unique; dạng {module}.{resource}.{action} |
| `name` | varchar(200) | Tên hiển thị |  |
| `resource` | varchar(80) | Tài nguyên | VD employee, leave, sales_order |
| `action` | varchar(40) | Hành động | Create/Read/Update/Delete/Approve/Assign/Export/Manage… |
| `description` | text | Mô tả chi tiết |  |
| `is_active` | boolean | Còn dùng |  |

#### Bảng 9. `sys.role_permission` — Gán permission vào role

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `role_id` | UUID | Vai trò | FK role |
| `permission_id` | UUID | Quyền | FK permission |

#### Bảng 10. `sys.user_role` — Gán role cho user

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `user_id` | UUID | Người dùng | FK app_user |
| `role_id` | UUID | Vai trò | FK role |
| `is_active` | boolean | Đang hiệu lực | false hoặc revoked_at = thu hồi mềm |
| `valid_from` | timestamptz | Hiệu lực từ |  |
| `valid_to` | timestamptz | Hiệu lực đến | Null = không hạn |
| `revoked_at` | timestamptz | Thời điểm thu hồi |  |
| `assigned_by` | UUID | Người gán | Ref app_user |

#### Bảng 11. `sys.user_data_scope` — Phạm vi dữ liệu theo đối tượng (bổ sung / ghi đè)

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `user_id` | UUID | Người dùng | FK app_user |
| `dimension` | varchar(30) | Chiều phạm vi | OrgUnit/Department/Warehouse/Store/Project… |
| `scope_id` | UUID | ID đối tượng phạm vi | Theo dimension |
| `include_children` | boolean | Gồm cây con | Áp dụng OrgUnit/Department |
| `access_level` | varchar(20) | Mức truy cập | Read/Write |
| `source` | varchar(30) | Nguồn gán | Manual/Import/Sync — không thay JobLevel mặc định |
| `valid_from` | date | Hiệu lực từ |  |
| `valid_to` | date | Hiệu lực đến | Null = không hạn |

#### Bảng 12. `sys.field_permission` — Quyền theo trường nhạy cảm (field-level)

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `role_id` | UUID | Vai trò | FK role |
| `entity_type` | varchar(80) | Loại thực thể | VD hrm.employee |
| `field_name` | varchar(80) | Tên trường | VD national_id_enc, base_salary |
| `access_level` | varchar(20) | Mức | Hidden/Masked/Read/Write |

#### Bảng 13. `sys.menu_item` — Menu chức năng UI (lọc theo license + permission)

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(80) | Mã menu | Unique theo tenant hoặc global+override |
| `parent_id` | UUID | Menu cha | FK self |
| `module_code` | varchar(10) | Module |  |
| `title` | varchar(200) | Nhãn hiển thị |  |
| `route_path` | varchar(255) | Đường dẫn FE |  |
| `permission_code` | varchar(100) | Quyền cần có để hiện | Null = chỉ cần license module |
| `icon` | varchar(80) | Icon |  |
| `sort_order` | int | Thứ tự |  |
| `is_active` | boolean | Đang hiện trong catalog |  |

#### Bảng 14. `sys.session` — Phiên đăng nhập đang hoạt động

- **Module:** SYS
- **Nhóm bảng:** Người dùng & bảo mật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `user_id` | UUID | Người dùng | FK app_user |
| `refresh_token_hash` | varchar(255) | Hash refresh token |  |
| `ip_address` | varchar(45) | IP đăng nhập |  |
| `user_agent` | varchar(500) | Thiết bị / trình duyệt |  |
| `expires_at` | timestamptz | Hết hạn phiên |  |
| `revoked_at` | timestamptz | Thời điểm thu hồi |  |

#### Bảng 15. `sys.license` — Hợp đồng / gói license của tenant

- **Module:** SYS
- **Nhóm bảng:** License & cấu hình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `plan_code` | varchar(50) | Mã gói |  |
| `valid_from` | date | Ngày bắt đầu |  |
| `valid_to` | date | Ngày hết hạn |  |
| `max_users` | int | Hạn mức user |  |
| `status` | varchar(20) | Trạng thái license | Active/Expired |

#### Bảng 16. `sys.license_module` — Module được bật trong license

- **Module:** SYS
- **Nhóm bảng:** License & cấu hình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `license_id` | UUID | License | FK license |
| `module_code` | varchar(10) | Mã module | HRM/CRM… |
| `is_enabled` | boolean | Đang bật |  |
| `quota_json` | jsonb | Hạn mức riêng module | Optional |

#### Bảng 17. `sys.sequence_rule` — Quy tắc sinh số chứng từ

- **Module:** SYS
- **Nhóm bảng:** License & cấu hình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã sequence | Unique theo tenant |
| `pattern` | varchar(100) | Mẫu sinh số | VD {YYYY}{MM}-{SEQ:5} |
| `current_value` | bigint | Giá trị hiện tại |  |
| `reset_policy` | varchar(30) | Chính sách reset | Never/Yearly/Monthly |
| `padding` | int | Độ dài phần số |  |

#### Bảng 18. `sys.setting` — Cấu hình key-value theo tenant

- **Module:** SYS
- **Nhóm bảng:** License & cấu hình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `setting_key` | varchar(100) | Khóa cấu hình | Unique theo tenant |
| `setting_value` | text | Giá trị | Có thể JSON string |
| `value_type` | varchar(20) | Kiểu giá trị | string/number/bool/json |
| `module_code` | varchar(10) | Module liên quan |  |

#### Bảng 19. `sys.file_object` — Metadata file đính kèm (binary ở object storage)

- **Module:** SYS
- **Nhóm bảng:** File & thông báo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `storage_key` | varchar(500) | Key trên object storage |  |
| `file_name` | varchar(255) | Tên file gốc |  |
| `content_type` | varchar(100) | MIME type |  |
| `size_bytes` | bigint | Dung lượng |  |
| `checksum` | varchar(64) | Hash kiểm tra toàn vẹn |  |
| `owner_module` | varchar(10) | Module gắn file |  |
| `owner_type` | varchar(50) | Loại đối tượng nghiệp vụ |  |
| `owner_id` | UUID | ID đối tượng nghiệp vụ |  |

#### Bảng 20. `sys.notification_template` — Mẫu thông báo email/SMS/in-app

- **Module:** SYS
- **Nhóm bảng:** File & thông báo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã mẫu | Unique theo tenant |
| `channel` | varchar(20) | Kênh | Email/SMS/InApp/Push |
| `subject_template` | varchar(500) | Tiêu đề mẫu |  |
| `body_template` | text | Nội dung mẫu | Placeholder |
| `locale` | varchar(10) | Ngôn ngữ mẫu |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 21. `sys.notification_log` — Lịch sử gửi thông báo

- **Module:** SYS
- **Nhóm bảng:** File & thông báo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `template_code` | varchar(50) | Mã mẫu đã dùng |  |
| `channel` | varchar(20) | Kênh gửi |  |
| `recipient` | varchar(255) | Người nhận |  |
| `payload_json` | jsonb | Dữ liệu render |  |
| `status` | varchar(20) | Kết quả gửi | Pending/Sent/Failed |
| `error_message` | text | Lỗi nếu thất bại |  |
| `sent_at` | timestamptz | Thời điểm gửi thành công |  |

#### Bảng 22. `sys.conversation` — Hội thoại 1-1 / nhóm (SYS-13)

- **Module:** SYS
- **Nhóm bảng:** Nhắn tin realtime

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `kind` | varchar(20) | Loại hội thoại | Direct/Group |
| `title` | nvarchar(200) | Tên nhóm | Nullable với Direct |
| `created_by` | UUID | Người tạo | FK → sys.app_user |

#### Bảng 23. `sys.conversation_member` — Thành viên hội thoại + unread/mute

- **Module:** SYS
- **Nhóm bảng:** Nhắn tin realtime

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `conversation_id` | UUID | Hội thoại | FK → sys.conversation |
| `user_id` | UUID | Thành viên | FK → sys.app_user |
| `last_read_at` | timestamptz | Mốc đã đọc |  |
| `muted` | boolean | Tắt thông báo hội thoại | Mặc định false |

#### Bảng 24. `sys.chat_message` — Tin nhắn realtime

- **Module:** SYS
- **Nhóm bảng:** Nhắn tin realtime

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `conversation_id` | UUID | Hội thoại | FK → sys.conversation |
| `sender_user_id` | UUID | Người gửi | FK → sys.app_user |
| `body` | nvarchar(max) | Nội dung text |  |
| `attachment_file_id` | UUID | File đính kèm | FK → sys.file_object; nullable |
| `sent_at` | timestamptz | Thời điểm gửi |  |
| `recalled_at` | timestamptz | Thu hồi | Nullable |

#### Bảng 25. `sys.audit_log` — Nhật ký thay đổi dữ liệu

- **Module:** SYS
- **Nhóm bảng:** Audit & tích hợp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `entity_type` | varchar(80) | Loại thực thể |  |
| `entity_id` | UUID | ID bản ghi |  |
| `action` | varchar(30) | Hành động | Create/Update/Delete |
| `before_json` | jsonb | Giá trị trước |  |
| `after_json` | jsonb | Giá trị sau |  |
| `actor_user_id` | UUID | Người thực hiện |  |
| `ip_address` | varchar(45) | IP |  |

#### Bảng 26. `sys.login_log` — Nhật ký đăng nhập / thất bại

- **Module:** SYS
- **Nhóm bảng:** Audit & tích hợp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `user_id` | UUID | User (nếu xác định được) |  |
| `username_attempt` | varchar(100) | Chuỗi đăng nhập thử |  |
| `success` | boolean | Thành công hay không |  |
| `fail_reason` | varchar(100) | Lý do thất bại |  |
| `ip_address` | varchar(45) | IP |  |
| `user_agent` | varchar(500) | User agent |  |

#### Bảng 27. `sys.api_key` — Khóa API tích hợp

- **Module:** SYS
- **Nhóm bảng:** Audit & tích hợp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `name` | varchar(100) | Tên key |  |
| `key_hash` | varchar(255) | Hash API key | Không lưu plaintext |
| `scopes_json` | jsonb | Phạm vi quyền |  |
| `expires_at` | timestamptz | Hết hạn |  |
| `revoked_at` | timestamptz | Thu hồi |  |
| `last_used_at` | timestamptz | Lần dùng cuối |  |

#### Bảng 28. `sys.webhook_subscription` — Đăng ký webhook outbound

- **Module:** SYS
- **Nhóm bảng:** Audit & tích hợp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `event_types_json` | jsonb | Danh sách event đăng ký |  |
| `target_url` | varchar(500) | URL nhận |  |
| `secret_hash` | varchar(255) | Secret ký request |  |
| `is_active` | boolean | Đang bật |  |

#### Bảng 29. `sys.integration_outbox` — Hàng đợi phát sự kiện (Outbox)

- **Module:** SYS
- **Nhóm bảng:** Audit & tích hợp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `event_id` | UUID | ID sự kiện duy nhất | Unique |
| `event_type` | varchar(120) | Loại sự kiện | Theo INT-03 |
| `aggregate_type` | varchar(80) | Loại aggregate nguồn |  |
| `aggregate_id` | UUID | ID aggregate nguồn |  |
| `payload_json` | jsonb | Envelope + payload |  |
| `status` | varchar(20) | Trạng thái publish | New/Published/Failed |
| `occurred_at` | timestamptz | Thời điểm nghiệp vụ xảy ra |  |
| `published_at` | timestamptz | Thời điểm publish |  |
| `retry_count` | int | Số lần retry |  |
| `last_error` | text | Lỗi gần nhất |  |

#### Bảng 30. `sys.integration_inbox` — Inbox chống xử lý trùng sự kiện

- **Module:** SYS
- **Nhóm bảng:** Audit & tích hợp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `event_id` | UUID | ID sự kiện | Unique cùng consumer |
| `consumer` | varchar(80) | Tên consumer/module |  |
| `processed_at` | timestamptz | Thời điểm xử lý xong |  |
| `result` | varchar(30) | Kết quả | Success/Skipped/Failed |

### 3.2. Module WF

#### Bảng 31. `wf.wf_definition` — Định nghĩa quy trình duyệt theo loại chứng từ

- **Module:** WF
- **Nhóm bảng:** Định nghĩa quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã quy trình | Unique theo tenant |
| `name` | varchar(200) | Tên quy trình |  |
| `source_module` | varchar(10) | Module nguồn |  |
| `doc_type` | varchar(50) | Loại chứng từ áp dụng |  |
| `is_active` | boolean | Đang hiệu lực |  |

#### Bảng 32. `wf.wf_definition_version` — Phiên bản quy trình (immutable khi đã chạy)

- **Module:** WF
- **Nhóm bảng:** Định nghĩa quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `definition_id` | UUID | Quy trình gốc | FK wf_definition |
| `version_no` | int | Số phiên bản |  |
| `published_at` | timestamptz | Thời điểm phát hành |  |
| `is_current` | boolean | Đang là bản hiện hành |  |

#### Bảng 33. `wf.wf_node` — Bước/nút trong quy trình

- **Module:** WF
- **Nhóm bảng:** Định nghĩa quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `version_id` | UUID | Phiên bản quy trình | FK |
| `node_key` | varchar(50) | Mã bước |  |
| `node_type` | varchar(30) | Loại nút | Start/Approve/Condition/End |
| `name` | varchar(200) | Tên bước |  |
| `assignee_rule_json` | jsonb | Rule gán người duyệt |  |

#### Bảng 34. `wf.wf_transition` — Chuyển tiếp giữa các bước

- **Module:** WF
- **Nhóm bảng:** Định nghĩa quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `version_id` | UUID | Phiên bản quy trình | FK |
| `from_node_id` | UUID | Nút nguồn | FK wf_node |
| `to_node_id` | UUID | Nút đích | FK wf_node |
| `condition_json` | jsonb | Điều kiện chuyển |  |

#### Bảng 35. `wf.wf_instance` — Một lần chạy quy trình trên chứng từ

- **Module:** WF
- **Nhóm bảng:** Thực thi duyệt

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `definition_version_id` | UUID | Version quy trình | FK |
| `source_module` | varchar(10) | Module chứng từ |  |
| `source_doc_type` | varchar(50) | Loại chứng từ |  |
| `source_doc_id` | UUID | ID chứng từ nguồn | Ref mềm |
| `status` | varchar(30) | Trạng thái instance | Running/Approved/Rejected/Cancelled |
| `started_by` | UUID | Người khởi tạo |  |
| `completed_at` | timestamptz | Hoàn tất |  |

#### Bảng 36. `wf.wf_task` — Việc chờ duyệt của một bước

- **Module:** WF
- **Nhóm bảng:** Thực thi duyệt

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `instance_id` | UUID | Instance | FK |
| `node_id` | UUID | Bước | FK |
| `assignee_user_id` | UUID | Người được gán |  |
| `status` | varchar(30) | Trạng thái task | Pending/Done/Skipped |
| `due_at` | timestamptz | Hạn xử lý |  |
| `acted_at` | timestamptz | Thời điểm xử lý |  |

#### Bảng 37. `wf.wf_task_action` — Hành động duyệt trên task

- **Module:** WF
- **Nhóm bảng:** Thực thi duyệt

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `task_id` | UUID | Task | FK |
| `action` | varchar(30) | Hành động | Approve/Reject/Return/Delegate |
| `comment` | text | Ý kiến |  |
| `acted_by` | UUID | Người thực hiện |  |
| `acted_at` | timestamptz | Thời điểm |  |

#### Bảng 38. `wf.wf_delegation` — Ủy quyền duyệt tạm thời

- **Module:** WF
- **Nhóm bảng:** Thực thi duyệt

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `from_user_id` | UUID | Người ủy quyền |  |
| `to_user_id` | UUID | Người nhận ủy quyền |  |
| `valid_from` | timestamptz | Từ |  |
| `valid_to` | timestamptz | Đến |  |
| `scope_json` | jsonb | Phạm vi ủy quyền | Optional filter doc type |

### 3.3. Module HRM

#### Bảng 39. `hrm.job_title` — Danh mục chức danh (khác JobLevel phân quyền)

- **Module:** HRM
- **Nhóm bảng:** Danh mục nhân sự

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã chức danh | Unique theo tenant |
| `name` | varchar(200) | Tên chức danh |  |
| `job_level_id` | UUID | Cấp bậc mặc định gợi ý | Ref sys.job_level — sync sang user khi nhận việc |
| `is_active` | boolean | Còn dùng |  |

#### Bảng 40. `hrm.employee_type` — Loại nhân sự (CT/TV/Part-time…)

- **Module:** HRM
- **Nhóm bảng:** Danh mục nhân sự

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã loại |  |
| `name` | varchar(200) | Tên loại |  |
| `is_active` | boolean | Còn dùng |  |

#### Bảng 41. `hrm.employee` — Hồ sơ nhân sự master

- **Module:** HRM
- **Nhóm bảng:** Hồ sơ nhân sự

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `employee_code` | varchar(40) | Mã nhân sự | Unique theo tenant; không tái sử dụng |
| `user_id` | UUID | Tài khoản SYS | Ref sys.app_user |
| `full_name` | varchar(200) | Họ và tên |  |
| `dob` | date | Ngày sinh |  |
| `gender` | varchar(20) | Giới tính |  |
| `national_id_enc` | varchar(255) | CCCD/CMND (mã hóa/mask) | Field security |
| `email` | varchar(255) | Email cá nhân/công ty |  |
| `phone` | varchar(30) | SĐT |  |
| `org_unit_id` | UUID | Chi nhánh/đơn vị chính | Ref sys.org_unit |
| `department_id` | UUID | Phòng ban chính | Ref sys.department — đồng bộ app_user.department_id |
| `job_title_id` | UUID | Chức danh | FK job_title |
| `job_level_id` | UUID | Cấp bậc | Ref sys.job_level — đồng bộ app_user.job_level_id |
| `manager_employee_id` | UUID | Quản lý trực tiếp (NS) | FK self — map sang app_user.manager_user_id |
| `employee_type_id` | UUID | Loại NS | FK employee_type |
| `status` | varchar(30) | Trạng thái NS | Probation/Active/Terminated… |
| `hire_date` | date | Ngày vào làm |  |
| `probation_end_date` | date | Hết hạn thử việc |  |
| `terminate_date` | date | Ngày nghỉ việc |  |

#### Bảng 42. `hrm.employment_status_history` — Lịch sử thay đổi trạng thái nhân sự

- **Module:** HRM
- **Nhóm bảng:** Hồ sơ nhân sự

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `employee_id` | UUID | Nhân sự | FK employee |
| `from_status` | varchar(30) | Trạng thái cũ |  |
| `to_status` | varchar(30) | Trạng thái mới |  |
| `effective_date` | date | Ngày hiệu lực |  |
| `reason` | text | Lý do |  |

#### Bảng 43. `hrm.contract` — Hợp đồng lao động

- **Module:** HRM
- **Nhóm bảng:** Hợp đồng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `employee_id` | UUID | Nhân sự | FK employee |
| `contract_no` | varchar(50) | Số HĐ |  |
| `contract_type` | varchar(30) | Loại HĐ |  |
| `signed_date` | date | Ngày ký |  |
| `effective_from` | date | Hiệu lực từ |  |
| `effective_to` | date | Hiệu lực đến |  |
| `base_salary` | numeric(18,2) | Lương HĐ | Nhạy cảm |
| `currency_code` | char(3) | Tiền tệ lương |  |
| `status` | varchar(30) | Trạng thái HĐ | Active/Expired/Terminated |
| `file_id` | UUID | File scan HĐ | Ref sys.file_object |

#### Bảng 44. `hrm.contract_appendix` — Phụ lục hợp đồng

- **Module:** HRM
- **Nhóm bảng:** Hợp đồng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `contract_id` | UUID | Hợp đồng gốc | FK contract |
| `appendix_no` | varchar(50) | Số phụ lục |  |
| `effective_from` | date | Hiệu lực từ |  |
| `content_summary` | text | Tóm tắt thay đổi |  |
| `file_id` | UUID | File đính kèm |  |

#### Bảng 45. `hrm.recruitment_request` — Phiếu đề xuất tuyển dụng

- **Module:** HRM
- **Nhóm bảng:** Tuyển dụng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `position_name` | varchar(200) | Vị trí cần tuyển |  |
| `headcount` | int | Số lượng |  |
| `org_unit_id` | UUID | Đơn vị đề xuất |  |
| `reason` | text | Lý do tuyển |  |
| `requested_by` | UUID | Người đề xuất |  |

#### Bảng 46. `hrm.candidate` — Hồ sơ ứng viên

- **Module:** HRM
- **Nhóm bảng:** Tuyển dụng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `full_name` | varchar(200) | Họ tên ứng viên |  |
| `email` | varchar(255) | Email |  |
| `phone` | varchar(30) | SĐT |  |
| `request_id` | UUID | Đề xuất tuyển liên quan | FK optional |
| `source_channel` | varchar(50) | Kênh ứng tuyển |  |
| `status` | varchar(30) | Trạng thái pipeline | New/Screening/Offer/Hired/Rejected |
| `cv_file_id` | UUID | File CV |  |

#### Bảng 47. `hrm.job_posting` — Tin tuyển dụng

- **Module:** HRM
- **Nhóm bảng:** Tuyển dụng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `request_id` | UUID | Đề xuất đã duyệt | FK |
| `title` | varchar(200) | Tiêu đề tin |  |
| `content` | text | Nội dung tin |  |
| `channel` | varchar(50) | Kênh đăng |  |
| `published_at` | timestamptz | Ngày đăng |  |
| `closed_at` | timestamptz | Ngày đóng tin |  |
| `status` | varchar(30) | Trạng thái tin |  |

#### Bảng 48. `hrm.shift_template` — Mẫu ca làm việc

- **Module:** HRM
- **Nhóm bảng:** Chấm công & ca

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã ca |  |
| `name` | varchar(100) | Tên ca |  |
| `start_time` | time | Giờ vào |  |
| `end_time` | time | Giờ ra |  |
| `break_minutes` | int | Phút nghỉ giữa ca |  |
| `is_overnight` | boolean | Ca qua đêm |  |

#### Bảng 49. `hrm.shift_assignment` — Phân công ca cho nhân viên

- **Module:** HRM
- **Nhóm bảng:** Chấm công & ca

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `employee_id` | UUID | Nhân sự | FK |
| `shift_template_id` | UUID | Mẫu ca | FK |
| `work_date` | date | Ngày làm |  |
| `status` | varchar(30) | Trạng thái xếp ca |  |

#### Bảng 50. `hrm.attendance_punch` — Bản ghi chấm công thô

- **Module:** HRM
- **Nhóm bảng:** Chấm công & ca

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `employee_id` | UUID | Nhân sự | FK |
| `punch_time` | timestamptz | Thời điểm chấm |  |
| `punch_type` | varchar(20) | Loại | In/Out |
| `source` | varchar(30) | Nguồn | Device/App/Manual |
| `device_code` | varchar(50) | Mã máy chấm |  |
| `geo_lat` | numeric(10,7) | Vĩ độ (nếu app) |  |
| `geo_lng` | numeric(10,7) | Kinh độ (nếu app) |  |

#### Bảng 51. `hrm.timesheet` — Bảng công theo kỳ

- **Module:** HRM
- **Nhóm bảng:** Chấm công & ca

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `period_code` | varchar(20) | Mã kỳ công | VD 2026-08 |
| `org_unit_id` | UUID | Phạm vi đơn vị |  |
| `status` | varchar(30) | Trạng thái | Open/Locked |
| `locked_at` | timestamptz | Thời điểm khóa |  |
| `locked_by` | UUID | Người khóa |  |

#### Bảng 52. `hrm.timesheet_line` — Dòng công từng nhân viên trong kỳ

- **Module:** HRM
- **Nhóm bảng:** Chấm công & ca

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `timesheet_id` | UUID | Bảng công | FK |
| `employee_id` | UUID | Nhân sự | FK |
| `work_days` | numeric(8,2) | Ngày công |  |
| `ot_hours` | numeric(8,2) | Giờ OT |  |
| `late_minutes` | int | Phút đi trễ |  |
| `leave_days` | numeric(8,2) | Ngày nghỉ |  |
| `note` | varchar(500) | Ghi chú dòng |  |

#### Bảng 53. `hrm.leave_type` — Danh mục loại nghỉ

- **Module:** HRM
- **Nhóm bảng:** Nghỉ phép

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã loại nghỉ |  |
| `name` | varchar(100) | Tên loại nghỉ |  |
| `paid` | boolean | Nghỉ hưởng lương |  |
| `affects_balance` | boolean | Trừ quỹ phép |  |

#### Bảng 54. `hrm.leave_balance` — Quỹ phép của nhân viên

- **Module:** HRM
- **Nhóm bảng:** Nghỉ phép

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `employee_id` | UUID | Nhân sự | FK |
| `leave_type_id` | UUID | Loại nghỉ | FK |
| `year` | int | Năm áp dụng |  |
| `entitled` | numeric(8,2) | Được cấp |  |
| `used` | numeric(8,2) | Đã dùng |  |
| `remaining` | numeric(8,2) | Còn lại | Có thể computed |

#### Bảng 55. `hrm.leave_request` — Đơn xin nghỉ

- **Module:** HRM
- **Nhóm bảng:** Nghỉ phép

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `employee_id` | UUID | Nhân sự | FK |
| `leave_type_id` | UUID | Loại nghỉ | FK |
| `from_date` | date | Từ ngày |  |
| `to_date` | date | Đến ngày |  |
| `days` | numeric(8,2) | Số ngày nghỉ |  |
| `reason` | text | Lý do |  |

#### Bảng 56. `hrm.payroll_period` — Kỳ tính lương

- **Module:** HRM
- **Nhóm bảng:** Lương

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `period_code` | varchar(20) | Mã kỳ lương | Unique theo tenant |
| `from_date` | date | Từ ngày |  |
| `to_date` | date | Đến ngày |  |
| `status` | varchar(30) | Trạng thái kỳ | Open/Calculating/Posted/Locked |
| `timesheet_id` | UUID | Bảng công nguồn | Ref |

#### Bảng 57. `hrm.payslip` — Phiếu lương nhân viên

- **Module:** HRM
- **Nhóm bảng:** Lương

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `payroll_period_id` | UUID | Kỳ lương | FK |
| `employee_id` | UUID | Nhân sự | FK |
| `gross_amount` | numeric(18,2) | Tổng thu nhập | Nhạy cảm |
| `deduction_amount` | numeric(18,2) | Tổng khấu trừ |  |
| `net_amount` | numeric(18,2) | Thực lĩnh |  |
| `currency_code` | char(3) | Tiền tệ |  |
| `status` | varchar(30) | Trạng thái phiếu |  |

#### Bảng 58. `hrm.payslip_line` — Chi tiết dòng phiếu lương

- **Module:** HRM
- **Nhóm bảng:** Lương

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `payslip_id` | UUID | Phiếu lương | FK |
| `line_type` | varchar(30) | Loại dòng | Earning/Deduction |
| `component_code` | varchar(50) | Mã thành phần lương |  |
| `component_name` | varchar(200) | Tên thành phần |  |
| `amount` | numeric(18,2) | Số tiền |  |

#### Bảng 59. `hrm.transfer_order` — Lệnh điều động / thăng chức

- **Module:** HRM
- **Nhóm bảng:** Biến động & nghỉ việc

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `employee_id` | UUID | Nhân sự | FK |
| `from_org_unit_id` | UUID | Đơn vị cũ |  |
| `to_org_unit_id` | UUID | Đơn vị mới |  |
| `from_job_title_id` | UUID | Chức danh cũ |  |
| `to_job_title_id` | UUID | Chức danh mới |  |
| `effective_date` | date | Ngày hiệu lực |  |

#### Bảng 60. `hrm.offboarding_case` — Hồ sơ nghỉ việc / offboarding

- **Module:** HRM
- **Nhóm bảng:** Biến động & nghỉ việc

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `employee_id` | UUID | Nhân sự | FK |
| `last_working_date` | date | Ngày làm việc cuối |  |
| `reason` | text | Lý do nghỉ |  |
| `checklist_status` | varchar(30) | Tiến độ checklist |  |

### 3.4. Module LMS

#### Bảng 61. `lms.course` — Khóa học

- **Module:** LMS
- **Nhóm bảng:** Nội dung đào tạo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã khóa |  |
| `title` | varchar(200) | Tên khóa |  |
| `description` | text | Mô tả |  |
| `course_type` | varchar(30) | Loại | Online/Offline/Blended |
| `status` | varchar(30) | Trạng thái | Draft/Published/Archived |

#### Bảng 62. `lms.course_version` — Phiên bản nội dung khóa học

- **Module:** LMS
- **Nhóm bảng:** Nội dung đào tạo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `course_id` | UUID | Khóa học | FK |
| `version_no` | int | Số version |  |
| `is_current` | boolean | Bản đang dùng |  |
| `published_at` | timestamptz | Ngày xuất bản |  |

#### Bảng 63. `lms.lesson` — Bài học trong khóa

- **Module:** LMS
- **Nhóm bảng:** Nội dung đào tạo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `course_version_id` | UUID | Version khóa | FK |
| `title` | varchar(200) | Tiêu đề bài |  |
| `sort_order` | int | Thứ tự |  |
| `content_type` | varchar(30) | Loại nội dung | Video/Doc/SCORM… |
| `content_ref` | varchar(500) | Tham chiếu nội dung/file |  |
| `duration_minutes` | int | Thời lượng ước tính |  |

#### Bảng 64. `lms.assessment` — Bài kiểm tra / bài thi

- **Module:** LMS
- **Nhóm bảng:** Nội dung đào tạo

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `course_version_id` | UUID | Version khóa | FK |
| `title` | varchar(200) | Tên bài kiểm tra |  |
| `pass_score` | numeric(5,2) | Điểm đạt |  |
| `max_attempts` | int | Số lần làm tối đa |  |
| `time_limit_minutes` | int | Thời gian làm bài |  |

#### Bảng 65. `lms.enrollment` — Ghi danh học viên vào khóa

- **Module:** LMS
- **Nhóm bảng:** Ghi danh & tiến độ

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `course_id` | UUID | Khóa học | FK |
| `learner_user_id` | UUID | User học viên | Ref sys.app_user |
| `employee_id` | UUID | NV (nếu có) | Ref hrm.employee |
| `enrolled_at` | timestamptz | Thời điểm ghi danh |  |
| `status` | varchar(30) | Trạng thái học | Enrolled/InProgress/Completed/Failed |
| `progress_percent` | numeric(5,2) | % hoàn thành |  |

#### Bảng 66. `lms.assessment_attempt` — Lần làm bài kiểm tra

- **Module:** LMS
- **Nhóm bảng:** Ghi danh & tiến độ

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `assessment_id` | UUID | Bài kiểm tra | FK |
| `enrollment_id` | UUID | Ghi danh | FK |
| `attempt_no` | int | Lần thứ |  |
| `score` | numeric(5,2) | Điểm |  |
| `passed` | boolean | Đạt/không đạt |  |
| `started_at` | timestamptz | Bắt đầu |  |
| `submitted_at` | timestamptz | Nộp bài |  |

#### Bảng 67. `lms.certificate` — Chứng chỉ hoàn thành

- **Module:** LMS
- **Nhóm bảng:** Ghi danh & tiến độ

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `enrollment_id` | UUID | Ghi danh | FK |
| `certificate_no` | varchar(50) | Số chứng chỉ |  |
| `issued_at` | timestamptz | Ngày cấp |  |
| `expires_at` | date | Hết hạn (nếu có) |  |
| `file_id` | UUID | File chứng chỉ |  |

#### Bảng 68. `lms.training_class` — Lớp đào tạo offline/hybrid

- **Module:** LMS
- **Nhóm bảng:** Lớp học

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `course_id` | UUID | Khóa học | FK |
| `class_code` | varchar(40) | Mã lớp |  |
| `start_date` | date | Ngày bắt đầu |  |
| `end_date` | date | Ngày kết thúc |  |
| `location` | varchar(255) | Địa điểm |  |
| `instructor_name` | varchar(200) | Giảng viên |  |
| `capacity` | int | Sĩ số tối đa |  |
| `status` | varchar(30) | Trạng thái lớp |  |

### 3.5. Module CRM

#### Bảng 69. `crm.customer` — Khách hàng / tổ chức mua hàng

- **Module:** CRM
- **Nhóm bảng:** Khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã KH | Unique theo tenant |
| `name` | varchar(200) | Tên KH |  |
| `customer_type` | varchar(30) | Loại KH | Company/Individual |
| `tax_code` | varchar(50) | Mã số thuế |  |
| `phone` | varchar(30) | SĐT |  |
| `email` | varchar(255) | Email |  |
| `billing_address` | text | Địa chỉ xuất HĐ |  |
| `shipping_address` | text | Địa chỉ giao mặc định |  |
| `owner_user_id` | UUID | NV phụ trách |  |
| `status` | varchar(30) | Trạng thái | Active/Inactive |
| `credit_limit` | numeric(18,2) | Hạn mức công nợ |  |

#### Bảng 70. `crm.contact` — Người liên hệ của khách hàng

- **Module:** CRM
- **Nhóm bảng:** Khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `customer_id` | UUID | Khách hàng | FK |
| `full_name` | varchar(200) | Họ tên |  |
| `title` | varchar(100) | Chức danh |  |
| `email` | varchar(255) | Email |  |
| `phone` | varchar(30) | SĐT |  |
| `is_primary` | boolean | Liên hệ chính |  |

#### Bảng 71. `crm.lead` — Đầu mối tiềm năng

- **Module:** CRM
- **Nhóm bảng:** Phễu bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `full_name` | varchar(200) | Tên lead |  |
| `company_name` | varchar(200) | Công ty |  |
| `phone` | varchar(30) | SĐT |  |
| `email` | varchar(255) | Email |  |
| `source` | varchar(50) | Nguồn lead |  |
| `status` | varchar(30) | Trạng thái | New/Qualified/Disqualified/Converted |
| `owner_user_id` | UUID | NV phụ trách |  |

#### Bảng 72. `crm.opportunity` — Cơ hội bán hàng

- **Module:** CRM
- **Nhóm bảng:** Phễu bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `name` | varchar(200) | Tên cơ hội |  |
| `customer_id` | UUID | KH | FK optional |
| `lead_id` | UUID | Lead nguồn | FK optional |
| `stage` | varchar(30) | Giai đoạn pipeline |  |
| `amount` | numeric(18,2) | Giá trị ước tính |  |
| `currency_code` | char(3) | Tiền tệ |  |
| `expected_close_date` | date | Ngày chốt dự kiến |  |
| `probability` | numeric(5,2) | % thành công |  |
| `owner_user_id` | UUID | NV phụ trách |  |
| `status` | varchar(30) | Open/Won/Lost |  |

#### Bảng 73. `crm.quote` — Báo giá

- **Module:** CRM
- **Nhóm bảng:** Báo giá & đơn

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `customer_id` | UUID | KH | FK |
| `opportunity_id` | UUID | Cơ hội | Optional |
| `valid_until` | date | Hiệu lực đến |  |
| `currency_code` | char(3) | Tiền tệ |  |
| `total_amount` | numeric(18,2) | Tổng tiền |  |

#### Bảng 74. `crm.quote_line` — Dòng báo giá

- **Module:** CRM
- **Nhóm bảng:** Báo giá & đơn

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `quote_id` | UUID | Báo giá | FK |
| `line_no` | int | Số dòng |  |
| `item_id` | UUID | Hàng hóa | Ref inv.item |
| `item_code_snapshot` | varchar(40) | Mã hàng snapshot |  |
| `item_name_snapshot` | varchar(200) | Tên hàng snapshot |  |
| `qty` | numeric(18,6) | Số lượng |  |
| `uom` | varchar(20) | ĐVT |  |
| `unit_price` | numeric(18,4) | Đơn giá |  |
| `discount_percent` | numeric(9,4) | % CK |  |
| `line_amount` | numeric(18,2) | Thành tiền |  |

#### Bảng 75. `crm.sales_order` — Đơn bán hàng

- **Module:** CRM
- **Nhóm bảng:** Báo giá & đơn

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `customer_id` | UUID | KH | FK |
| `quote_id` | UUID | Báo giá nguồn | Optional |
| `currency_code` | char(3) | Tiền tệ |  |
| `total_amount` | numeric(18,2) | Tổng tiền |  |
| `warehouse_id` | UUID | Kho xuất dự kiến | Ref inv.warehouse |
| `promised_date` | date | Ngày giao hẹn |  |

#### Bảng 76. `crm.sales_order_line` — Dòng đơn bán

- **Module:** CRM
- **Nhóm bảng:** Báo giá & đơn

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `sales_order_id` | UUID | Đơn bán | FK |
| `line_no` | int | Số dòng |  |
| `item_id` | UUID | Hàng hóa | Ref inv.item |
| `item_code_snapshot` | varchar(40) | Mã hàng snapshot |  |
| `item_name_snapshot` | varchar(200) | Tên hàng snapshot |  |
| `qty` | numeric(18,6) | Số lượng đặt |  |
| `qty_delivered` | numeric(18,6) | Số lượng đã giao |  |
| `uom` | varchar(20) | ĐVT |  |
| `unit_price` | numeric(18,4) | Đơn giá |  |
| `line_amount` | numeric(18,2) | Thành tiền |  |

#### Bảng 77. `crm.campaign` — Chiến dịch marketing

- **Module:** CRM
- **Nhóm bảng:** Marketing & CSKH

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã chiến dịch |  |
| `name` | varchar(200) | Tên chiến dịch |  |
| `channel` | varchar(50) | Kênh |  |
| `start_date` | date | Bắt đầu |  |
| `end_date` | date | Kết thúc |  |
| `budget` | numeric(18,2) | Ngân sách |  |
| `status` | varchar(30) | Trạng thái |  |

#### Bảng 78. `crm.sales_case` — Case / khiếu nại CSKH

- **Module:** CRM
- **Nhóm bảng:** Marketing & CSKH

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `customer_id` | UUID | KH | FK |
| `subject` | varchar(200) | Tiêu đề |  |
| `priority` | varchar(20) | Mức ưu tiên |  |
| `channel` | varchar(30) | Kênh tiếp nhận |  |
| `owner_user_id` | UUID | Người xử lý |  |

### 3.6. Module POS

#### Bảng 79. `pos.store` — Cửa hàng / điểm bán

- **Module:** POS
- **Nhóm bảng:** Cấu hình điểm bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã CH |  |
| `name` | varchar(200) | Tên CH |  |
| `org_unit_id` | UUID | Đơn vị tổ chức | Ref sys.org_unit |
| `warehouse_id` | UUID | Kho gắn CH | Ref inv.warehouse |
| `address` | text | Địa chỉ |  |
| `is_active` | boolean | Đang hoạt động |  |

#### Bảng 80. `pos.terminal` — Máy POS / terminal

- **Module:** POS
- **Nhóm bảng:** Cấu hình điểm bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `store_id` | UUID | Cửa hàng | FK |
| `code` | varchar(40) | Mã máy |  |
| `name` | varchar(100) | Tên máy |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 81. `pos.price_list` — Bảng giá bán

- **Module:** POS
- **Nhóm bảng:** Cấu hình điểm bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã bảng giá |  |
| `name` | varchar(100) | Tên bảng giá |  |
| `currency_code` | char(3) | Tiền tệ |  |
| `valid_from` | date | Hiệu lực từ |  |
| `valid_to` | date | Hiệu lực đến |  |
| `is_active` | boolean | Đang áp dụng |  |

#### Bảng 82. `pos.price_list_item` — Giá theo sản phẩm trong bảng giá

- **Module:** POS
- **Nhóm bảng:** Cấu hình điểm bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `price_list_id` | UUID | Bảng giá | FK |
| `item_id` | UUID | Hàng | Ref inv.item |
| `unit_price` | numeric(18,4) | Đơn giá |  |
| `uom` | varchar(20) | ĐVT |  |

#### Bảng 83. `pos.cash_shift` — Ca quỹ thu ngân

- **Module:** POS
- **Nhóm bảng:** Giao dịch bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `store_id` | UUID | Cửa hàng | FK |
| `terminal_id` | UUID | Máy POS | FK |
| `cashier_user_id` | UUID | Thu ngân |  |
| `opened_at` | timestamptz | Mở ca |  |
| `closed_at` | timestamptz | Đóng ca |  |
| `opening_amount` | numeric(18,2) | Tiền đầu ca |  |
| `closing_amount` | numeric(18,2) | Tiền cuối ca |  |
| `status` | varchar(30) | Open/Closed |  |

#### Bảng 84. `pos.pos_order` — Hóa đơn / giao dịch bán tại quầy

- **Module:** POS
- **Nhóm bảng:** Giao dịch bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `shift_id` | UUID | Ca quỹ | FK |
| `doc_no` | varchar(50) | Số HĐ |  |
| `customer_id` | UUID | KH (nếu có) | Ref crm.customer |
| `order_time` | timestamptz | Thời điểm bán |  |
| `status` | varchar(30) | Completed/Void/Returned |  |
| `total_amount` | numeric(18,2) | Tổng tiền |  |
| `currency_code` | char(3) | Tiền tệ |  |

#### Bảng 85. `pos.pos_order_line` — Dòng hàng trên hóa đơn POS

- **Module:** POS
- **Nhóm bảng:** Giao dịch bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `pos_order_id` | UUID | Hóa đơn | FK |
| `line_no` | int | Số dòng |  |
| `item_id` | UUID | Hàng bán | Ref inv.item |
| `qty` | numeric(18,6) | Số lượng |  |
| `unit_price` | numeric(18,4) | Đơn giá |  |
| `line_amount` | numeric(18,2) | Thành tiền |  |
| `discount_amount` | numeric(18,2) | Chiết khấu dòng |  |

#### Bảng 86. `pos.pos_payment` — Thanh toán của hóa đơn POS

- **Module:** POS
- **Nhóm bảng:** Giao dịch bán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `pos_order_id` | UUID | Hóa đơn | FK |
| `method` | varchar(30) | Phương thức | Cash/Card/QR/Transfer |
| `amount` | numeric(18,2) | Số tiền |  |
| `ref_no` | varchar(100) | Mã tham chiếu giao dịch |  |
| `paid_at` | timestamptz | Thời điểm thanh toán |  |

#### Bảng 87. `pos.recipe` — Định mức trừ kho cho món/SP

- **Module:** POS
- **Nhóm bảng:** Định mức

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | SP bán | Ref inv.item |
| `name` | varchar(200) | Tên định mức |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 88. `pos.recipe_line` — Dòng NVL trong định mức

- **Module:** POS
- **Nhóm bảng:** Định mức

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `recipe_id` | UUID | Định mức | FK |
| `component_item_id` | UUID | NVL | Ref inv.item |
| `qty` | numeric(18,6) | Định mức NVL / 1 SP |  |
| `uom` | varchar(20) | ĐVT NVL |  |

### 3.7. Module PUR

#### Bảng 89. `pur.vendor` — Nhà cung cấp

- **Module:** PUR
- **Nhóm bảng:** Nhà cung cấp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã NCC | Unique theo tenant |
| `name` | varchar(200) | Tên NCC |  |
| `tax_code` | varchar(50) | MST |  |
| `phone` | varchar(30) | SĐT |  |
| `email` | varchar(255) | Email |  |
| `address` | text | Địa chỉ |  |
| `payment_term_days` | int | Công nợ (ngày) |  |
| `status` | varchar(30) | Active/Inactive |  |

#### Bảng 90. `pur.vendor_item_price` — Bảng giá mua theo NCC–hàng

- **Module:** PUR
- **Nhóm bảng:** Nhà cung cấp

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `vendor_id` | UUID | NCC | FK |
| `item_id` | UUID | Hàng | Ref inv.item |
| `unit_price` | numeric(18,4) | Đơn giá mua |  |
| `currency_code` | char(3) | Tiền tệ |  |
| `valid_from` | date | Hiệu lực từ |  |
| `valid_to` | date | Hiệu lực đến |  |
| `min_qty` | numeric(18,6) | SL tối thiểu |  |

#### Bảng 91. `pur.purchase_requisition` — Phiếu yêu cầu mua hàng (PR)

- **Module:** PUR
- **Nhóm bảng:** Chứng từ mua

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `requested_by` | UUID | Người yêu cầu |  |
| `needed_date` | date | Ngày cần hàng |  |

#### Bảng 92. `pur.purchase_requisition_line` — Dòng PR

- **Module:** PUR
- **Nhóm bảng:** Chứng từ mua

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `pr_id` | UUID | PR | FK |
| `line_no` | int | Số dòng |  |
| `item_id` | UUID | Hàng | Ref inv.item |
| `qty` | numeric(18,6) | SL yêu cầu |  |
| `uom` | varchar(20) | ĐVT |  |
| `needed_date` | date | Ngày cần |  |

#### Bảng 93. `pur.purchase_order` — Đơn mua hàng (PO)

- **Module:** PUR
- **Nhóm bảng:** Chứng từ mua

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `vendor_id` | UUID | NCC | FK |
| `currency_code` | char(3) | Tiền tệ |  |
| `total_amount` | numeric(18,2) | Tổng tiền |  |
| `expected_date` | date | Ngày giao dự kiến |  |
| `warehouse_id` | UUID | Kho nhận | Ref inv.warehouse |

#### Bảng 94. `pur.purchase_order_line` — Dòng PO

- **Module:** PUR
- **Nhóm bảng:** Chứng từ mua

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `po_id` | UUID | PO | FK |
| `line_no` | int | Số dòng |  |
| `item_id` | UUID | Hàng | Ref inv.item |
| `qty_ordered` | numeric(18,6) | SL đặt |  |
| `qty_received` | numeric(18,6) | SL đã nhận |  |
| `uom` | varchar(20) | ĐVT |  |
| `unit_price` | numeric(18,4) | Đơn giá |  |
| `line_amount` | numeric(18,2) | Thành tiền |  |

#### Bảng 95. `pur.goods_receipt` — Phiếu nhập hàng từ NCC (GRN)

- **Module:** PUR
- **Nhóm bảng:** Chứng từ mua

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `po_id` | UUID | PO nguồn | FK optional |
| `vendor_id` | UUID | NCC | FK |
| `warehouse_id` | UUID | Kho nhận | Ref inv.warehouse |
| `received_at` | timestamptz | Thời điểm nhận |  |

#### Bảng 96. `pur.goods_receipt_line` — Dòng GRN

- **Module:** PUR
- **Nhóm bảng:** Chứng từ mua

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `grn_id` | UUID | GRN | FK |
| `po_line_id` | UUID | Dòng PO | Optional |
| `item_id` | UUID | Hàng | Ref inv.item |
| `qty_received` | numeric(18,6) | SL nhận |  |
| `uom` | varchar(20) | ĐVT |  |
| `lot_code` | varchar(50) | Mã lô (nếu có) |  |

### 3.8. Module INV

#### Bảng 97. `inv.item` — Danh mục hàng hóa / NVL / TP / dịch vụ

- **Module:** INV
- **Nhóm bảng:** Master hàng hóa

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã hàng (SKU) | Unique theo tenant |
| `name` | varchar(200) | Tên hàng |  |
| `item_type` | varchar(30) | Loại | Goods/Service/Raw/Finished/Recipe |
| `category_id` | UUID | Nhóm hàng | FK item_category |
| `base_uom` | varchar(20) | ĐVT cơ bản |  |
| `is_stockable` | boolean | Quản lý tồn kho |  |
| `is_sales_item` | boolean | Được bán |  |
| `is_purchase_item` | boolean | Được mua |  |
| `status` | varchar(30) | Active/Inactive |  |

#### Bảng 98. `inv.item_category` — Nhóm hàng hóa

- **Module:** INV
- **Nhóm bảng:** Master hàng hóa

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã nhóm |  |
| `name` | varchar(200) | Tên nhóm |  |
| `parent_id` | UUID | Nhóm cha | Self FK |

#### Bảng 99. `inv.uom_conversion` — Quy đổi đơn vị tính theo hàng

- **Module:** INV
- **Nhóm bảng:** Master hàng hóa

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | Hàng | FK |
| `from_uom` | varchar(20) | ĐVT nguồn |  |
| `to_uom` | varchar(20) | ĐVT đích |  |
| `factor` | numeric(18,8) | Hệ số quy đổi |  |

#### Bảng 100. `inv.warehouse` — Kho hàng

- **Module:** INV
- **Nhóm bảng:** Kho & vị trí

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã kho |  |
| `name` | varchar(200) | Tên kho |  |
| `org_unit_id` | UUID | Đơn vị quản lý | Ref sys.org_unit |
| `address` | text | Địa chỉ kho |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 101. `inv.bin_location` — Vị trí / ngăn trong kho

- **Module:** INV
- **Nhóm bảng:** Kho & vị trí

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `warehouse_id` | UUID | Kho | FK |
| `code` | varchar(40) | Mã vị trí |  |
| `name` | varchar(100) | Tên vị trí |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 102. `inv.stock_balance` — Tồn kho hiện tại theo chiều item–kho–vị trí–lô

- **Module:** INV
- **Nhóm bảng:** Tồn kho

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | Hàng | FK |
| `warehouse_id` | UUID | Kho | FK |
| `bin_id` | UUID | Vị trí | FK optional |
| `lot_id` | UUID | Lô | FK optional |
| `qty_on_hand` | numeric(18,6) | Tồn thực tế |  |
| `qty_reserved` | numeric(18,6) | Tồn đã giữ |  |
| `qty_available` | numeric(18,6) | Tồn khả dụng | on_hand - reserved |

#### Bảng 103. `inv.lot` — Lô hàng

- **Module:** INV
- **Nhóm bảng:** Tồn kho

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | Hàng | FK |
| `lot_code` | varchar(50) | Mã lô |  |
| `manufacture_date` | date | NSX |  |
| `expiry_date` | date | HSD |  |

#### Bảng 104. `inv.serial_no` — Số serial theo từng đơn vị hàng

- **Module:** INV
- **Nhóm bảng:** Tồn kho

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | Hàng | FK |
| `serial` | varchar(100) | Số serial | Unique theo tenant |
| `status` | varchar(30) | InStock/Issued/Scrapped |  |
| `warehouse_id` | UUID | Kho hiện tại |  |

#### Bảng 105. `inv.stock_document` — Chứng từ kho (nhập/xuất/chuyển/điều chỉnh)

- **Module:** INV
- **Nhóm bảng:** Chứng từ kho

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `doc_type` | varchar(30) | Loại CT | In/Out/Transfer/Adjust |
| `warehouse_id` | UUID | Kho chính | FK |
| `warehouse_to_id` | UUID | Kho đích (chuyển) | Optional |
| `source_module` | varchar(10) | Module nguồn |  |
| `source_doc_id` | UUID | ID chứng từ nguồn |  |
| `posted_at` | timestamptz | Thời điểm post tồn |  |

#### Bảng 106. `inv.stock_document_line` — Dòng chứng từ kho

- **Module:** INV
- **Nhóm bảng:** Chứng từ kho

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `stock_document_id` | UUID | Chứng từ kho | FK |
| `line_no` | int | Số dòng |  |
| `item_id` | UUID | Hàng | FK |
| `qty` | numeric(18,6) | Số lượng |  |
| `uom` | varchar(20) | ĐVT |  |
| `bin_id` | UUID | Vị trí |  |
| `lot_id` | UUID | Lô |  |
| `unit_cost` | numeric(18,4) | Đơn giá vốn | Optional |

#### Bảng 107. `inv.reservation` — Phiếu giữ hàng (reserve)

- **Module:** INV
- **Nhóm bảng:** Giữ hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `source_module` | varchar(10) | Module nguồn | CRM/POS/MFG… |
| `source_doc_type` | varchar(50) | Loại chứng từ nguồn |  |
| `source_doc_id` | UUID | ID chứng từ nguồn |  |
| `warehouse_id` | UUID | Kho giữ | FK |
| `status` | varchar(30) | Active/Released/Consumed/Cancelled |  |
| `expires_at` | timestamptz | Hết hạn giữ |  |

#### Bảng 108. `inv.reservation_line` — Dòng giữ hàng

- **Module:** INV
- **Nhóm bảng:** Giữ hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `reservation_id` | UUID | Phiếu giữ | FK |
| `item_id` | UUID | Hàng | FK |
| `qty` | numeric(18,6) | SL giữ |  |
| `uom` | varchar(20) | ĐVT |  |

#### Bảng 109. `inv.stock_count` — Phiếu kiểm kê

- **Module:** INV
- **Nhóm bảng:** Kiểm kê

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `warehouse_id` | UUID | Kho kiểm | FK |
| `counted_at` | timestamptz | Thời điểm kiểm |  |

#### Bảng 110. `inv.stock_count_line` — Dòng kiểm kê

- **Module:** INV
- **Nhóm bảng:** Kiểm kê

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `stock_count_id` | UUID | Phiếu kiểm | FK |
| `item_id` | UUID | Hàng | FK |
| `system_qty` | numeric(18,6) | SL sổ |  |
| `counted_qty` | numeric(18,6) | SL thực đếm |  |
| `variance_qty` | numeric(18,6) | Chênh lệch |  |
| `bin_id` | UUID | Vị trí |  |
| `lot_id` | UUID | Lô |  |

### 3.9. Module LOG

#### Bảng 111. `log.carrier` — Đơn vị vận chuyển / 3PL

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã ĐVVC |  |
| `name` | varchar(200) | Tên ĐVVC |  |
| `contact_phone` | varchar(30) | SĐT |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 112. `log.vehicle` — Phương tiện giao hàng

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `plate_no` | varchar(20) | Biển số |  |
| `vehicle_type` | varchar(30) | Loại xe |  |
| `capacity_kg` | numeric(18,2) | Tải trọng |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 113. `log.driver` — Tài xế / nhân viên giao

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `full_name` | varchar(200) | Họ tên |  |
| `phone` | varchar(30) | SĐT |  |
| `employee_id` | UUID | Liên kết NS (nếu có) | Ref hrm.employee |
| `license_no` | varchar(50) | Số GPLX |  |
| `is_active` | boolean | Đang làm việc |  |

#### Bảng 114. `log.shipment` — Chuyến / lô giao hàng

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `source_module` | varchar(10) | Module nguồn đơn | CRM… |
| `source_doc_id` | UUID | ID đơn nguồn |  |
| `customer_id` | UUID | KH nhận | Ref crm.customer |
| `carrier_id` | UUID | ĐVVC | FK optional |
| `driver_id` | UUID | Tài xế | FK optional |
| `vehicle_id` | UUID | Xe | FK optional |
| `shipped_at` | timestamptz | Xuất giao |  |
| `delivered_at` | timestamptz | Giao xong |  |
| `cod_amount` | numeric(18,2) | Tiền COD |  |

#### Bảng 115. `log.shipment_line` — Dòng hàng trên chuyến giao

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `shipment_id` | UUID | Chuyến giao | FK |
| `item_id` | UUID | Hàng | Ref inv.item |
| `qty` | numeric(18,6) | SL giao |  |
| `uom` | varchar(20) | ĐVT |  |
| `sales_order_line_id` | UUID | Dòng SO nguồn | Optional |

#### Bảng 116. `log.shipment_tracking` — Mốc tracking trạng thái giao

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `shipment_id` | UUID | Chuyến giao | FK |
| `tracking_time` | timestamptz | Thời điểm |  |
| `status_code` | varchar(30) | Mã trạng thái |  |
| `description` | varchar(500) | Mô tả |  |
| `location_text` | varchar(255) | Vị trí |  |

#### Bảng 117. `log.cod_collection` — Thu hộ COD

- **Module:** LOG
- **Nhóm bảng:** Vận hành giao hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `shipment_id` | UUID | Chuyến giao | FK |
| `amount` | numeric(18,2) | Số tiền COD |  |
| `collected_at` | timestamptz | Thời điểm thu |  |
| `status` | varchar(30) | Collected/Remitted |  |
| `fin_doc_id` | UUID | Chứng từ FIN liên quan | Ref mềm |

### 3.10. Module MFG

#### Bảng 118. `mfg.bom_header` — BOM định mức NVL cho thành phẩm

- **Module:** MFG
- **Nhóm bảng:** Định mức & quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | Thành phẩm | Ref inv.item |
| `code` | varchar(40) | Mã BOM |  |
| `version_no` | int | Version BOM |  |
| `is_active` | boolean | Đang dùng |  |
| `effective_from` | date | Hiệu lực từ |  |

#### Bảng 119. `mfg.bom_line` — Dòng NVL trong BOM

- **Module:** MFG
- **Nhóm bảng:** Định mức & quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `bom_id` | UUID | BOM | FK |
| `component_item_id` | UUID | NVL | Ref inv.item |
| `qty` | numeric(18,6) | Định mức / 1 TP |  |
| `uom` | varchar(20) | ĐVT |  |
| `scrap_percent` | numeric(9,4) | % hao hụt |  |

#### Bảng 120. `mfg.routing` — Quy trình sản xuất

- **Module:** MFG
- **Nhóm bảng:** Định mức & quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `item_id` | UUID | TP áp dụng | Ref inv.item |
| `code` | varchar(40) | Mã routing |  |
| `name` | varchar(200) | Tên quy trình |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 121. `mfg.routing_operation` — Công đoạn trong routing

- **Module:** MFG
- **Nhóm bảng:** Định mức & quy trình

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `routing_id` | UUID | Routing | FK |
| `op_seq` | int | Thứ tự công đoạn |  |
| `op_name` | varchar(200) | Tên công đoạn |  |
| `work_center` | varchar(100) | Tổ/máy |  |
| `std_minutes` | numeric(10,2) | Thời gian chuẩn (phút) |  |

#### Bảng 122. `mfg.work_order` — Lệnh sản xuất

- **Module:** MFG
- **Nhóm bảng:** Lệnh sản xuất

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `item_id` | UUID | Thành phẩm | Ref inv.item |
| `bom_id` | UUID | BOM dùng | FK optional |
| `routing_id` | UUID | Routing dùng | FK optional |
| `qty_planned` | numeric(18,6) | SL kế hoạch |  |
| `qty_completed` | numeric(18,6) | SL hoàn thành |  |
| `warehouse_id` | UUID | Kho NVL/TP | Ref inv.warehouse |
| `due_date` | date | Hạn hoàn thành |  |

#### Bảng 123. `mfg.work_order_material` — NVL cấp phát / định mức cho LSX

- **Module:** MFG
- **Nhóm bảng:** Lệnh sản xuất

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `work_order_id` | UUID | LSX | FK |
| `item_id` | UUID | NVL | Ref inv.item |
| `qty_required` | numeric(18,6) | SL cần |  |
| `qty_issued` | numeric(18,6) | SL đã xuất |  |
| `uom` | varchar(20) | ĐVT |  |

#### Bảng 124. `mfg.work_order_output` — Nhập thành phẩm từ LSX

- **Module:** MFG
- **Nhóm bảng:** Lệnh sản xuất

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `work_order_id` | UUID | LSX | FK |
| `item_id` | UUID | TP | Ref inv.item |
| `qty` | numeric(18,6) | SL nhập |  |
| `uom` | varchar(20) | ĐVT |  |
| `lot_id` | UUID | Lô TP | Optional |
| `stock_document_id` | UUID | CT nhập kho | Ref inv.stock_document |

#### Bảng 125. `mfg.qc_inspection` — Phiếu kiểm chất lượng

- **Module:** MFG
- **Nhóm bảng:** QC

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `work_order_id` | UUID | LSX | FK optional |
| `lot_id` | UUID | Lô kiểm |  |
| `result` | varchar(20) | Pass/Fail/Conditional |  |
| `inspected_at` | timestamptz | Thời điểm kiểm |  |
| `inspected_by` | UUID | Người kiểm |  |

### 3.11. Module FIN

#### Bảng 126. `fin.account` — Hệ thống tài khoản kế toán (COA)

- **Module:** FIN
- **Nhóm bảng:** Danh mục kế toán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(30) | Số hiệu TK | Unique theo tenant |
| `name` | varchar(200) | Tên TK |  |
| `account_type` | varchar(30) | Loại TK | Asset/Liability/Equity/Revenue/Expense |
| `parent_id` | UUID | TK cha | Self FK |
| `is_postable` | boolean | Được hạch toán trực tiếp |  |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 127. `fin.fiscal_year` — Năm tài chính

- **Module:** FIN
- **Nhóm bảng:** Danh mục kế toán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `year_code` | varchar(10) | Mã năm | VD 2026 |
| `start_date` | date | Ngày bắt đầu |  |
| `end_date` | date | Ngày kết thúc |  |
| `status` | varchar(20) | Open/Closed |  |

#### Bảng 128. `fin.fiscal_period` — Kỳ kế toán (tháng/quý)

- **Module:** FIN
- **Nhóm bảng:** Danh mục kế toán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `fiscal_year_id` | UUID | Năm TC | FK |
| `period_code` | varchar(20) | Mã kỳ | VD 2026-08 |
| `start_date` | date | Từ ngày |  |
| `end_date` | date | Đến ngày |  |
| `status` | varchar(20) | Open/Closed |  |

#### Bảng 129. `fin.journal_entry` — Chứng từ ghi sổ / bút toán

- **Module:** FIN
- **Nhóm bảng:** Sổ cái

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `fiscal_period_id` | UUID | Kỳ KT | FK |
| `source_module` | varchar(10) | Module nguồn |  |
| `source_doc_id` | UUID | ID chứng từ nguồn |  |
| `posted_at` | timestamptz | Thời điểm post |  |
| `total_debit` | numeric(18,2) | Tổng Nợ |  |
| `total_credit` | numeric(18,2) | Tổng Có | Phải = Nợ khi posted |

#### Bảng 130. `fin.journal_line` — Dòng Nợ/Có của bút toán

- **Module:** FIN
- **Nhóm bảng:** Sổ cái

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `journal_id` | UUID | Bút toán | FK |
| `line_no` | int | Số dòng |  |
| `account_id` | UUID | Tài khoản | FK account |
| `debit` | numeric(18,2) | Phát sinh Nợ | ≥ 0 |
| `credit` | numeric(18,2) | Phát sinh Có | ≥ 0 |
| `cost_center_id` | UUID | Trung tâm CP | Optional |
| `partner_type` | varchar(20) | Loại đối tượng | Customer/Vendor/Employee |
| `partner_id` | UUID | ID đối tượng | Ref mềm |
| `memo` | varchar(500) | Diễn giải dòng |  |

#### Bảng 131. `fin.ar_invoice` — Hóa đơn phải thu

- **Module:** FIN
- **Nhóm bảng:** Công nợ phải thu

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `customer_id` | UUID | KH | Ref crm.customer |
| `currency_code` | char(3) | Tiền tệ |  |
| `total_amount` | numeric(18,2) | Tổng tiền HĐ |  |
| `open_amount` | numeric(18,2) | Còn phải thu |  |
| `due_date` | date | Hạn thanh toán |  |

#### Bảng 132. `fin.ar_receipt` — Phiếu thu tiền KH

- **Module:** FIN
- **Nhóm bảng:** Công nợ phải thu

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `customer_id` | UUID | KH | Ref crm.customer |
| `amount` | numeric(18,2) | Số tiền thu |  |
| `method` | varchar(30) | Phương thức thu |  |
| `bank_account_id` | UUID | TKNH/quỹ | Optional |

#### Bảng 133. `fin.ar_allocation` — Khớp phiếu thu với hóa đơn

- **Module:** FIN
- **Nhóm bảng:** Công nợ phải thu

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `receipt_id` | UUID | Phiếu thu | FK |
| `invoice_id` | UUID | Hóa đơn AR | FK |
| `amount` | numeric(18,2) | Số tiền khớp |  |

#### Bảng 134. `fin.ap_invoice` — Hóa đơn phải trả NCC

- **Module:** FIN
- **Nhóm bảng:** Công nợ phải trả

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `vendor_id` | UUID | NCC | Ref pur.vendor |
| `currency_code` | char(3) | Tiền tệ |  |
| `total_amount` | numeric(18,2) | Tổng tiền |  |
| `open_amount` | numeric(18,2) | Còn phải trả |  |
| `due_date` | date | Hạn thanh toán |  |
| `po_id` | UUID | PO liên quan | Optional |

#### Bảng 135. `fin.ap_payment` — Phiếu chi trả NCC

- **Module:** FIN
- **Nhóm bảng:** Công nợ phải trả

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `vendor_id` | UUID | NCC | Ref pur.vendor |
| `amount` | numeric(18,2) | Số tiền chi |  |
| `method` | varchar(30) | Phương thức chi |  |
| `bank_account_id` | UUID | TKNH/quỹ |  |

#### Bảng 136. `fin.bank_account` — Tài khoản ngân hàng / quỹ

- **Module:** FIN
- **Nhóm bảng:** Quỹ & ngân hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã TK |  |
| `name` | varchar(200) | Tên TK/quỹ |  |
| `account_kind` | varchar(20) | Bank/Cash |  |
| `bank_name` | varchar(200) | Ngân hàng |  |
| `account_number` | varchar(50) | Số TK |  |
| `currency_code` | char(3) | Tiền tệ |  |
| `gl_account_id` | UUID | TK kế toán map | FK account |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 137. `fin.bank_transaction` — Giao dịch ngân hàng / quỹ

- **Module:** FIN
- **Nhóm bảng:** Quỹ & ngân hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `bank_account_id` | UUID | TK/quỹ | FK |
| `txn_date` | date | Ngày GD |  |
| `amount` | numeric(18,2) | Số tiền (+/-) |  |
| `description` | varchar(500) | Diễn giải |  |
| `ref_no` | varchar(100) | Số tham chiếu |  |
| `source_doc_id` | UUID | Chứng từ nguồn |  |

#### Bảng 138. `fin.cost_center` — Trung tâm chi phí

- **Module:** FIN
- **Nhóm bảng:** Danh mục kế toán

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã TTCP |  |
| `name` | varchar(200) | Tên TTCP |  |
| `org_unit_id` | UUID | Đơn vị gắn | Optional |
| `is_active` | boolean | Đang dùng |  |

#### Bảng 139. `fin.tax_code` — Mã thuế

- **Module:** FIN
- **Nhóm bảng:** Thuế

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(30) | Mã thuế |  |
| `name` | varchar(100) | Tên thuế |  |
| `rate` | numeric(9,4) | Thuế suất % |  |
| `is_active` | boolean | Đang dùng |  |

### 3.12. Module AST

#### Bảng 140. `ast.asset_category` — Nhóm tài sản cố định

- **Module:** AST
- **Nhóm bảng:** Tài sản

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã nhóm |  |
| `name` | varchar(200) | Tên nhóm |  |
| `depr_method` | varchar(30) | PP khấu hao mặc định | StraightLine… |
| `useful_life_months` | int | Thời gian sử dụng (tháng) |  |

#### Bảng 141. `ast.asset` — Hồ sơ tài sản cố định

- **Module:** AST
- **Nhóm bảng:** Tài sản

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã TS | Unique theo tenant |
| `name` | varchar(200) | Tên TS |  |
| `category_id` | UUID | Nhóm TS | FK |
| `status` | varchar(30) | InUse/Idle/Disposed |  |
| `acquisition_date` | date | Ngày ghi tăng |  |
| `acquisition_cost` | numeric(18,2) | Nguyên giá |  |
| `accum_depr` | numeric(18,2) | Hao mòn lũy kế |  |
| `net_book_value` | numeric(18,2) | Giá trị còn lại |  |
| `custodian_employee_id` | UUID | Người đang giữ | Ref hrm.employee |
| `org_unit_id` | UUID | Đơn vị quản lý |  |
| `location_text` | varchar(255) | Vị trí đặt |  |

#### Bảng 142. `ast.asset_acquisition` — Chứng từ ghi tăng TS

- **Module:** AST
- **Nhóm bảng:** Tài sản

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `asset_id` | UUID | Tài sản | FK |
| `source_module` | varchar(10) | Nguồn mua/vốn hóa | PUR/PJM… |
| `source_doc_id` | UUID | ID chứng từ nguồn |  |
| `amount` | numeric(18,2) | Giá trị ghi tăng |  |

#### Bảng 143. `ast.depreciation_run` — Đợt chạy khấu hao

- **Module:** AST
- **Nhóm bảng:** Khấu hao

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `period_code` | varchar(20) | Kỳ KH |  |
| `run_date` | date | Ngày chạy |  |
| `status` | varchar(30) | Draft/Posted |  |
| `posted_journal_id` | UUID | Bút toán FIN | Ref mềm |

#### Bảng 144. `ast.depreciation_line` — Dòng khấu hao theo tài sản

- **Module:** AST
- **Nhóm bảng:** Khấu hao

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `run_id` | UUID | Đợt KH | FK |
| `asset_id` | UUID | Tài sản | FK |
| `depr_amount` | numeric(18,2) | Số tiền KH kỳ |  |

#### Bảng 145. `ast.asset_transfer` — Điều chuyển tài sản

- **Module:** AST
- **Nhóm bảng:** Biến động TS

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `asset_id` | UUID | Tài sản | FK |
| `from_org_unit_id` | UUID | Đơn vị cũ |  |
| `to_org_unit_id` | UUID | Đơn vị mới |  |
| `from_custodian_id` | UUID | Người giữ cũ |  |
| `to_custodian_id` | UUID | Người giữ mới |  |
| `effective_date` | date | Ngày hiệu lực |  |

#### Bảng 146. `ast.asset_disposal` — Thanh lý / ghi giảm TS

- **Module:** AST
- **Nhóm bảng:** Biến động TS

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `asset_id` | UUID | Tài sản | FK |
| `disposal_date` | date | Ngày thanh lý |  |
| `disposal_amount` | numeric(18,2) | Giá trị thanh lý |  |
| `reason` | text | Lý do |  |

### 3.13. Module FSM

#### Bảng 147. `fsm.service_contract` — Hợp đồng dịch vụ / bảo hành

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `customer_id` | UUID | KH | Ref crm.customer |
| `contract_no` | varchar(50) | Số HĐ |  |
| `start_date` | date | Bắt đầu |  |
| `end_date` | date | Kết thúc |  |
| `sla_policy_id` | UUID | Chính sách SLA | FK optional |
| `status` | varchar(30) | Active/Expired |  |

#### Bảng 148. `fsm.sla_policy` — Chính sách SLA

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã SLA |  |
| `name` | varchar(200) | Tên SLA |  |
| `response_minutes` | int | SLA phản hồi (phút) |  |
| `resolve_minutes` | int | SLA xử lý (phút) |  |

#### Bảng 149. `fsm.ticket` — Ticket yêu cầu hỗ trợ / sự cố

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `customer_id` | UUID | KH | Ref crm.customer |
| `subject` | varchar(200) | Tiêu đề |  |
| `priority` | varchar(20) | Mức ưu tiên |  |
| `channel` | varchar(30) | Kênh tạo | PRT/Phone/Email |
| `sla_due_at` | timestamptz | Hạn SLA |  |
| `assignee_user_id` | UUID | Người phụ trách |  |

#### Bảng 150. `fsm.work_order` — Phiếu công việc kỹ thuật hiện trường

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `org_unit_id` | UUID | Đơn vị/chi nhánh phát sinh | FK/ref sys.org_unit |
| `remark` | text | Ghi chú |  |
| `correlation_id` | UUID | Mã truy vết liên module | Khớp event INT |
| `ticket_id` | UUID | Ticket nguồn | FK optional |
| `customer_id` | UUID | KH | Ref crm.customer |
| `technician_user_id` | UUID | Kỹ thuật viên |  |
| `scheduled_start` | timestamptz | Lịch bắt đầu |  |
| `scheduled_end` | timestamptz | Lịch kết thúc |  |
| `completed_at` | timestamptz | Hoàn thành |  |

#### Bảng 151. `fsm.work_order_part` — Linh kiện dùng trên phiếu KT

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `work_order_id` | UUID | Phiếu KT | FK |
| `item_id` | UUID | Linh kiện | Ref inv.item |
| `qty` | numeric(18,6) | SL dùng |  |
| `uom` | varchar(20) | ĐVT |  |
| `stock_document_id` | UUID | CT xuất kho | Optional |

#### Bảng 152. `fsm.work_order_time` — Giờ công kỹ thuật viên

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `work_order_id` | UUID | Phiếu KT | FK |
| `technician_user_id` | UUID | KTV |  |
| `minutes` | int | Số phút |  |
| `work_date` | date | Ngày thực hiện |  |
| `note` | varchar(500) | Ghi chú |  |

#### Bảng 153. `fsm.appointment` — Lịch hẹn hiện trường

- **Module:** FSM
- **Nhóm bảng:** Dịch vụ kỹ thuật

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `work_order_id` | UUID | Phiếu KT | FK |
| `start_at` | timestamptz | Bắt đầu hẹn |  |
| `end_at` | timestamptz | Kết thúc hẹn |  |
| `address` | text | Địa chỉ hẹn |  |
| `status` | varchar(30) | Scheduled/Done/Cancelled |  |

### 3.14. Module PJM

#### Bảng 154. `pjm.project` — Dự án

- **Module:** PJM
- **Nhóm bảng:** Dự án

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã dự án | Unique theo tenant |
| `name` | varchar(200) | Tên dự án |  |
| `customer_id` | UUID | KH | Ref crm.customer |
| `manager_user_id` | UUID | PM |  |
| `start_date` | date | Ngày bắt đầu |  |
| `end_date` | date | Ngày kết thúc KH |  |
| `status` | varchar(30) | Planned/Active/Closed |  |
| `budget_amount` | numeric(18,2) | Ngân sách |  |
| `currency_code` | char(3) | Tiền tệ |  |

#### Bảng 155. `pjm.project_member` — Thành viên dự án

- **Module:** PJM
- **Nhóm bảng:** Dự án

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `project_id` | UUID | Dự án | FK |
| `employee_id` | UUID | Nhân sự | Ref hrm.employee |
| `role_name` | varchar(100) | Vai trò trong DA |  |
| `allocation_percent` | numeric(5,2) | % phân bổ |  |
| `joined_at` | date | Ngày tham gia |  |

#### Bảng 156. `pjm.wbs_node` — Nút WBS / cấu trúc phân rã công việc

- **Module:** PJM
- **Nhóm bảng:** Kế hoạch

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `project_id` | UUID | Dự án | FK |
| `parent_id` | UUID | Nút cha | Self FK |
| `code` | varchar(40) | Mã WBS |  |
| `name` | varchar(200) | Tên hạng mục |  |
| `sort_order` | int | Thứ tự |  |

#### Bảng 157. `pjm.task` — Công việc / task dự án

- **Module:** PJM
- **Nhóm bảng:** Kế hoạch

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `project_id` | UUID | Dự án | FK |
| `wbs_id` | UUID | WBS | FK optional |
| `name` | varchar(200) | Tên task |  |
| `assignee_user_id` | UUID | Người thực hiện |  |
| `start_date` | date | Bắt đầu |  |
| `due_date` | date | Hạn |  |
| `progress_percent` | numeric(5,2) | % hoàn thành |  |
| `status` | varchar(30) | Todo/Doing/Done |  |

#### Bảng 158. `pjm.milestone` — Mốc dự án

- **Module:** PJM
- **Nhóm bảng:** Kế hoạch

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `project_id` | UUID | Dự án | FK |
| `name` | varchar(200) | Tên mốc |  |
| `due_date` | date | Ngày mốc |  |
| `status` | varchar(30) | Pending/Achieved |  |

#### Bảng 159. `pjm.project_budget_line` — Dòng ngân sách dự án

- **Module:** PJM
- **Nhóm bảng:** Chi phí & thay đổi

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `project_id` | UUID | Dự án | FK |
| `category` | varchar(50) | Hạng mục NS |  |
| `amount` | numeric(18,2) | Số tiền NS |  |

#### Bảng 160. `pjm.change_request` — Yêu cầu thay đổi phạm vi/CR

- **Module:** PJM
- **Nhóm bảng:** Chi phí & thay đổi

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `doc_no` | varchar(50) | Số chứng từ | Unique theo tenant; Sequence SYS |
| `doc_date` | date | Ngày chứng từ |  |
| `status` | varchar(30) | Trạng thái vòng đời | Draft/Submitted/Approved/Posted/Cancelled… |
| `project_id` | UUID | Dự án | FK |
| `title` | varchar(200) | Tiêu đề CR |  |
| `impact_summary` | text | Tóm tắt ảnh hưởng |  |
| `requested_by` | UUID | Người đề xuất |  |

### 3.15. Module BI

#### Bảng 161. `bi.dataset` — Đăng ký dataset phân tích

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã dataset |  |
| `name` | varchar(200) | Tên dataset |  |
| `source_module` | varchar(10) | Module nguồn |  |
| `description` | text | Mô tả |  |
| `is_active` | boolean | Đang mở | Phụ thuộc license module nguồn |

#### Bảng 162. `bi.dataset_field` — Trường trong dataset

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `dataset_id` | UUID | Dataset | FK |
| `field_name` | varchar(80) | Tên trường |  |
| `data_type` | varchar(30) | Kiểu logic |  |
| `label` | varchar(200) | Nhãn hiển thị |  |

#### Bảng 163. `bi.dashboard` — Dashboard tổng hợp

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã dashboard |  |
| `name` | varchar(200) | Tên dashboard |  |
| `owner_user_id` | UUID | Chủ sở hữu |  |
| `is_published` | boolean | Đã phát hành |  |

#### Bảng 164. `bi.dashboard_widget` — Widget trên dashboard

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `dashboard_id` | UUID | Dashboard | FK |
| `dataset_id` | UUID | Dataset | FK |
| `widget_type` | varchar(30) | Loại | Chart/Table/KPI |
| `config_json` | jsonb | Cấu hình hiển thị |  |
| `sort_order` | int | Thứ tự |  |

#### Bảng 165. `bi.report_definition` — Định nghĩa báo cáo

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(50) | Mã báo cáo |  |
| `name` | varchar(200) | Tên báo cáo |  |
| `dataset_id` | UUID | Dataset | FK |
| `query_config_json` | jsonb | Cấu hình truy vấn/bộ lọc |  |

#### Bảng 166. `bi.report_schedule` — Lịch chạy/gửi báo cáo

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `report_id` | UUID | Báo cáo | FK |
| `cron_expr` | varchar(80) | Biểu thức lịch |  |
| `channel` | varchar(20) | Kênh gửi | Email |
| `recipients_json` | jsonb | Danh sách nhận |  |
| `is_active` | boolean | Đang bật |  |

#### Bảng 167. `bi.bi_acl` — Phân quyền xem dataset/dashboard

- **Module:** BI
- **Nhóm bảng:** Metadata BI

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `object_type` | varchar(30) | Dataset/Dashboard/Report |  |
| `object_id` | UUID | ID đối tượng |  |
| `principal_type` | varchar(20) | User/Role |  |
| `principal_id` | UUID | ID user/role |  |
| `access_level` | varchar(20) | View/Edit |  |

### 3.16. Module PRT

#### Bảng 168. `prt.portal_account` — Tài khoản đăng nhập cổng KH/NCC

- **Module:** PRT
- **Nhóm bảng:** Cổng khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `login_email` | varchar(255) | Email đăng nhập | Unique theo tenant |
| `password_hash` | varchar(255) | Hash mật khẩu |  |
| `account_type` | varchar(20) | Customer/Vendor |  |
| `customer_id` | UUID | KH liên kết | Ref crm.customer |
| `vendor_id` | UUID | NCC liên kết | Ref pur.vendor |
| `user_id` | UUID | User SYS (nếu map) | Optional |
| `status` | varchar(20) | Active/Disabled |  |
| `last_login_at` | timestamptz | Đăng nhập cuối |  |

#### Bảng 169. `prt.portal_role` — Vai trò trên cổng

- **Module:** PRT
- **Nhóm bảng:** Cổng khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `code` | varchar(40) | Mã role cổng |  |
| `name` | varchar(100) | Tên role |  |
| `permissions_json` | jsonb | Quyền chức năng cổng |  |

#### Bảng 170. `prt.portal_account_role` — Gán role cho tài khoản cổng

- **Module:** PRT
- **Nhóm bảng:** Cổng khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `portal_account_id` | UUID | Tài khoản cổng | FK |
| `portal_role_id` | UUID | Role cổng | FK |

#### Bảng 171. `prt.self_service_ticket` — Ticket KH tự tạo trên cổng

- **Module:** PRT
- **Nhóm bảng:** Cổng khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `portal_account_id` | UUID | TK cổng | FK |
| `customer_id` | UUID | KH | Ref crm.customer |
| `subject` | varchar(200) | Tiêu đề |  |
| `description` | text | Mô tả chi tiết |  |
| `status` | varchar(30) | New/InProgress/Closed |  |
| `fsm_ticket_id` | UUID | Ticket FSM đồng bộ | Ref mềm |

#### Bảng 172. `prt.portal_notification` — Thông báo hiển thị trên cổng

- **Module:** PRT
- **Nhóm bảng:** Cổng khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `portal_account_id` | UUID | TK cổng | FK |
| `title` | varchar(200) | Tiêu đề |  |
| `body` | text | Nội dung |  |
| `is_read` | boolean | Đã đọc |  |
| `read_at` | timestamptz | Thời điểm đọc |  |
| `ref_type` | varchar(50) | Loại đối tượng liên quan | Order/Invoice/Ticket |
| `ref_id` | UUID | ID đối tượng liên quan |  |

#### Bảng 173. `prt.portal_document_share` — Chia sẻ chứng từ cho tài khoản cổng

- **Module:** PRT
- **Nhóm bảng:** Cổng khách hàng

| Tên trường | Kiểu dữ liệu | Ý nghĩa | Ghi chú |
|---|---|---|---|
| `id` | UUID | Khóa chính | PK |
| `tenant_id` | UUID | Định danh tenant | FK → sys.tenant; index |
| `created_at` | timestamptz | Thời điểm tạo (UTC) | NOT NULL |
| `created_by` | UUID | Người tạo | FK → sys.app_user; nullable với job hệ thống |
| `updated_at` | timestamptz | Thời điểm cập nhật cuối (UTC) | NOT NULL |
| `updated_by` | UUID | Người cập nhật cuối | FK → sys.app_user |
| `is_deleted` | boolean | Cờ xóa mềm | Mặc định false |
| `deleted_at` | timestamptz | Thời điểm xóa mềm | Null nếu chưa xóa |
| `row_version` | int | Phiên bản hàng (optimistic lock) | Tăng mỗi lần update |
| `portal_account_id` | UUID | TK cổng | FK |
| `doc_module` | varchar(10) | Module chứng từ |  |
| `doc_type` | varchar(50) | Loại chứng từ |  |
| `doc_id` | UUID | ID chứng từ |  |
| `can_download` | boolean | Cho tải file |  |
| `shared_at` | timestamptz | Thời điểm share |  |

---

## 4. Truy vết

| Tài liệu liên quan | Vị trí |
|---|---|
| Chuẩn DDD | `00_CHUAN_TAI_LIEU_DDD.md` |
| DDD chi tiết theo nhóm | `DDD-01` … `DDD-06` |
| Tích hợp / sự kiện | `../02. Tích hợp liên module` |
| SRS module | `../01. Modules` |

---

*Hết DDD-MASTER-v1.0 — Thiết kế tổng hợp cơ sở dữ liệu.*
