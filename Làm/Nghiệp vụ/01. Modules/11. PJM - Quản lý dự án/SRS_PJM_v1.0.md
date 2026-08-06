# SRS-PJM-v1.0 — Quản lý dự án

> Tài liệu đặc tả yêu cầu phần mềm (Software Requirements Specification) cho module ERP bán độc lập.
> Trạng thái: **Đề xuất / chờ duyệt nghiệp vụ**. Không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu & lịch sử thay đổi

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-PJM-v1.0` |
| Module | `PJM` — Quản lý dự án |
| Phiên bản | 1.0 |
| Ngày lập | 03/08/2026 |
| Ngôn ngữ | Tiếng Việt |
| Phân loại | Nghiệp vụ / BA |
| Lớp sản phẩm | Sản xuất & Dịch vụ |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | SYS |
| Khuyến nghị kèm | CRM, INV, FIN, HRM, WF |
| Số nhóm chức năng | 7 |
| Số use case / chức năng | 42 |

| Phiên bản | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Solution | Sinh SRS từ danh mục chức năng generic v3 + meta nghiệp vụ | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích tài liệu
Tài liệu này mô tả đầy đủ yêu cầu nghiệp vụ và yêu cầu hệ thống của module **Quản lý dự án**, làm cơ sở để thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai cấu trúc source code.

### 1.2. Tóm tắt module
Quản lý dự án dịch vụ/triển khai: WBS, tiến độ, nguồn lực, chi phí, change request, nghiệm thu và P&L dự án.

### 1.3. Mục tiêu nghiệp vụ
1. Theo dõi tiến độ và ngân sách dự án realtime.
2. Gắn dự án với cơ hội/hợp đồng và vật tư.
3. Nghiệm thu – quyết toán rõ ràng.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / Ban giám đốc dự án
- Business Analyst, Solution Architect
- Trưởng nhóm Dev/QA
- Đội triển khai & Presales (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Project types/templates, initiation, WBS/schedule, resources/cost, execution checklists, acceptance, project P&L/reports.

### 2.2. Out of Scope
- PM phức tạp kiểu construction BIM.
- HRM full payroll.

### 2.3. Nguyên tắc đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`.
- **Khuyến nghị kèm** để có giá trị E2E: CRM, INV, FIN, HRM, WF.
- Tính năng ngành (F&B, sản xuất rời rạc, phân phối…) cấu hình bằng template khi triển khai, không hard-code vào SRS gốc.

---

## 3. Tác nhân & stakeholder

| Tác nhân | Trách nhiệm chính |
|---|---|
| Project Manager | Điều phối dự án |
| Project Member | Thực hiện hạng mục |
| PMO / Manager | Portfolio & duyệt NS |
| Customer Rep | Nghiệm thu |
| Cost Controller | Chi phí–doanh thu DA |

---

## 4. Thuật ngữ & viết tắt

| Thuật ngữ | Định nghĩa |
|---|---|
| WBS | Work Breakdown Structure |
| Milestone | Mốc tiến độ |
| CR | Change Request |
| Project P&L | Lãi lỗ theo dự án |
| UC | Use Case / chức năng nguyên tử trong catalog |
| MoSCoW | Must / Should / Could / Won't (ưu tiên) |
| Data scope | Phạm vi dữ liệu theo tổ chức/kho/… do SYS kiểm soát |

---

## 5. Ngữ cảnh module & phụ thuộc

### 5.1. Vị trí trong kiến trúc sản phẩm
Module `PJM` thuộc lớp **Sản xuất & Dịch vụ**. Mọi truy cập đi qua lớp nền `SYS` (xác thực, RBAC, license, audit, file, thông báo).

### 5.2. Phụ thuộc & tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM | Cơ hội/HĐ |
| Tích hợp | INV | Vật tư |
| Tích hợp | FIN | Doanh thu/chi phí |
| Tích hợp | HRM | Nhân sự DA |
| Tích hợp | FSM | Bảo hành sau bàn giao |
| Tích hợp | WF | Duyệt kickoff/CR |

### 5.3. Ràng buộc license
- API/UI của `PJM` chỉ mở khi license module active.
- Dataset BI liên quan module chỉ mở khi vừa có license `BI` vừa có license module nguồn.

---

## 6. Catalog chức năng (Module → Nhóm → UC)

**Tổng hợp:** 7 nhóm | 42 chức năng/use case.

| STT | Mã nhóm | Nhóm chức năng | Số UC |
|---:|---|---|---:|
| 1 | `PJM-01` | Danh mục dự án | 4 |
| 2 | `PJM-02` | Khởi tạo dự án | 6 |
| 3 | `PJM-03` | Kế hoạch & tiến độ | 8 |
| 4 | `PJM-04` | Nguồn lực & chi phí dự án | 6 |
| 5 | `PJM-05` | Thực hiện dự án | 6 |
| 6 | `PJM-06` | Nghiệm thu & đóng dự án | 7 |
| 7 | `PJM-07` | Báo cáo dự án | 5 |

<details>
<summary>Bảng đầy đủ mã UC (bấm để mở)</summary>

| Mã UC | Nhóm | Tên chức năng | Ưu tiên | MoSCoW |
|---|---|---|---|---|
| `UC_PJM_001` | Danh mục dự án | Loại dự án | Bắt buộc | Must |
| `UC_PJM_002` | Danh mục dự án | Mẫu hạng mục / WBS | Bắt buộc | Must |
| `UC_PJM_003` | Danh mục dự án | Mẫu checklist nghiệm thu | Cao | Should |
| `UC_PJM_004` | Danh mục dự án | Trạng thái dự án chuẩn | Bắt buộc | Must |
| `UC_PJM_005` | Khởi tạo dự án | Tạo dự án từ cơ hội CRM | Bắt buộc | Must |
| `UC_PJM_006` | Khởi tạo dự án | Tạo dự án thủ công | Bắt buộc | Must |
| `UC_PJM_007` | Khởi tạo dự án | Gắn khách hàng / hợp đồng | Bắt buộc | Must |
| `UC_PJM_008` | Khởi tạo dự án | Gán quản lý dự án / thành viên | Bắt buộc | Must |
| `UC_PJM_009` | Khởi tạo dự án | Ngân sách dự kiến & timeline | Bắt buộc | Must |
| `UC_PJM_010` | Khởi tạo dự án | Phê duyệt khởi động | Cao | Should |
| `UC_PJM_011` | Kế hoạch & tiến độ | Tạo hạng mục WBS | Bắt buộc | Must |
| `UC_PJM_012` | Kế hoạch & tiến độ | Gán người thực hiện | Bắt buộc | Must |
| `UC_PJM_013` | Kế hoạch & tiến độ | Cập nhật % hoàn thành | Bắt buộc | Must |
| `UC_PJM_014` | Kế hoạch & tiến độ | Milestone & deadline | Bắt buộc | Must |
| `UC_PJM_015` | Kế hoạch & tiến độ | Phụ thuộc giữa hạng mục | Trung bình | Could |
| `UC_PJM_016` | Kế hoạch & tiến độ | Gantt / timeline dự án | Cao | Should |
| `UC_PJM_017` | Kế hoạch & tiến độ | Cảnh báo trễ tiến độ | Bắt buộc | Must |
| `UC_PJM_018` | Kế hoạch & tiến độ | Nhật ký thay đổi kế hoạch | Cao | Should |
| `UC_PJM_019` | Nguồn lực & chi phí dự án | Phân công nhân sự | Bắt buộc | Must |
| `UC_PJM_020` | Nguồn lực & chi phí dự án | Timesheet theo dự án | Cao | Should |
| `UC_PJM_021` | Nguồn lực & chi phí dự án | Xuất nguyên vật liệu cho dự án | Bắt buộc | Must |
| `UC_PJM_022` | Nguồn lực & chi phí dự án | Ghi nhận chi phí phát sinh | Bắt buộc | Must |
| `UC_PJM_023` | Nguồn lực & chi phí dự án | Theo dõi ngân sách vs thực tế | Bắt buộc | Must |
| `UC_PJM_024` | Nguồn lực & chi phí dự án | Cảnh báo vượt ngân sách | Cao | Should |
| `UC_PJM_025` | Thực hiện dự án | Checklist khảo sát | Cao | Should |
| `UC_PJM_026` | Thực hiện dự án | Checklist lắp đặt | Cao | Should |
| `UC_PJM_027` | Thực hiện dự án | Checklist bàn giao | Cao | Should |
| `UC_PJM_028` | Thực hiện dự án | Ghi nhận ảnh / biên bản | Cao | Should |
| `UC_PJM_029` | Thực hiện dự án | Phát sinh change request | Cao | Should |
| `UC_PJM_030` | Thực hiện dự án | Duyệt change request | Cao | Should |
| `UC_PJM_031` | Nghiệm thu & đóng dự án | Biên bản nghiệm thu giai đoạn | Bắt buộc | Must |
| `UC_PJM_032` | Nghiệm thu & đóng dự án | Nghiệm thu cuối & bàn giao | Bắt buộc | Must |
| `UC_PJM_033` | Nghiệm thu & đóng dự án | Khách ký xác nhận | Bắt buộc | Must |
| `UC_PJM_034` | Nghiệm thu & đóng dự án | Ghi nhận doanh thu dự án | Bắt buộc | Must |
| `UC_PJM_035` | Nghiệm thu & đóng dự án | Quyết toán chi phí & P&L | Bắt buộc | Must |
| `UC_PJM_036` | Nghiệm thu & đóng dự án | Đóng dự án / lưu trữ | Bắt buộc | Must |
| `UC_PJM_037` | Nghiệm thu & đóng dự án | Bảo hành sau dự án | Cao | Should |
| `UC_PJM_038` | Báo cáo dự án | Portfolio dự án đang chạy | Bắt buộc | Must |
| `UC_PJM_039` | Báo cáo dự án | Tiến độ & sức khỏe dự án | Bắt buộc | Must |
| `UC_PJM_040` | Báo cáo dự án | Lợi nhuận theo dự án | Bắt buộc | Must |
| `UC_PJM_041` | Báo cáo dự án | Năng suất nguồn lực | Cao | Should |
| `UC_PJM_042` | Báo cáo dự án | Xuất báo cáo dự án | Bắt buộc | Must |

</details>

---

## 7. Đặc tả chức năng theo nhóm

Mỗi UC bên dưới gồm: mô tả, tác nhân, tiền/hậu điều kiện, luồng chính, quy tắc, tiêu chí chấp nhận và ưu tiên. Đây là mức đặc tả BA để chốt phạm vi; chi tiết UI/API sẽ bổ sung ở giai đoạn thiết kế.

### 7.1. Danh mục dự án (`PJM-01`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **PMO / Manager**.

#### UC_PJM_001 — Loại dự án

- **Mô tả:** Project types
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Loại dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project types)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Loại dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_002 — Mẫu hạng mục / WBS

- **Mô tả:** WBS templates
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Mẫu hạng mục / WBS
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (WBS templates)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Mẫu hạng mục / WBS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_003 — Mẫu checklist nghiệm thu

- **Mô tả:** Acceptance checklist template
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Mẫu checklist nghiệm thu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Acceptance checklist template)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Mẫu checklist nghiệm thu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_004 — Trạng thái dự án chuẩn

- **Mô tả:** Project status model
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Trạng thái dự án chuẩn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project status model)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Trạng thái dự án chuẩn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.2. Khởi tạo dự án (`PJM-02`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **Project Manager**.

#### UC_PJM_005 — Tạo dự án từ cơ hội CRM

- **Mô tả:** Project from opportunity
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng mở chức năng tương ứng trong module
  2. Nhập/chọn các trường bắt buộc theo form
  3. Hệ thống validate dữ liệu và ràng buộc duy nhất/tham chiếu
  4. Lưu bản ghi; ghi audit; hiển thị kết quả thành công
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Tạo dự án từ cơ hội CRM” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_006 — Tạo dự án thủ công

- **Mô tả:** Manual project creation
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng mở chức năng tương ứng trong module
  2. Nhập/chọn các trường bắt buộc theo form
  3. Hệ thống validate dữ liệu và ràng buộc duy nhất/tham chiếu
  4. Lưu bản ghi; ghi audit; hiển thị kết quả thành công
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Tạo dự án thủ công” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_007 — Gắn khách hàng / hợp đồng

- **Mô tả:** Project linkages
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn khách hàng / hợp đồng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project linkages)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Gắn khách hàng / hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_008 — Gán quản lý dự án / thành viên

- **Mô tả:** Project team assignment
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gán quản lý dự án / thành viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project team assignment)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Gán quản lý dự án / thành viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_009 — Ngân sách dự kiến & timeline

- **Mô tả:** Budget & schedule baseline
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ngân sách dự kiến & timeline
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget & schedule baseline)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Ngân sách dự kiến & timeline” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_010 — Phê duyệt khởi động

- **Mô tả:** Project kickoff approval
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người duyệt mở chứng từ từ hộp chờ hoặc liên kết thông báo
  2. Xem nội dung, lịch sử và ràng buộc nghiệp vụ
  3. Chọn Duyệt / Từ chối / Trả bổ sung kèm lý do nếu cần
  4. Hệ thống cập nhật trạng thái và phát sự kiện cho module nguồn
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Phê duyệt khởi động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.3. Kế hoạch & tiến độ (`PJM-03`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **Project Manager**.

#### UC_PJM_011 — Tạo hạng mục WBS

- **Mô tả:** WBS breakdown
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng mở chức năng tương ứng trong module
  2. Nhập/chọn các trường bắt buộc theo form
  3. Hệ thống validate dữ liệu và ràng buộc duy nhất/tham chiếu
  4. Lưu bản ghi; ghi audit; hiển thị kết quả thành công
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Tạo hạng mục WBS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_012 — Gán người thực hiện

- **Mô tả:** Task assignment
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gán người thực hiện
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Task assignment)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Gán người thực hiện” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_013 — Cập nhật % hoàn thành

- **Mô tả:** Progress update
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cập nhật % hoàn thành
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Progress update)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Cập nhật % hoàn thành” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_014 — Milestone & deadline

- **Mô tả:** Milestone tracking
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Milestone & deadline
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Milestone tracking)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Milestone & deadline” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_015 — Phụ thuộc giữa hạng mục

- **Mô tả:** Task dependencies
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phụ thuộc giữa hạng mục
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Task dependencies)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Phụ thuộc giữa hạng mục” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_016 — Gantt / timeline dự án

- **Mô tả:** Timeline visualization
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gantt / timeline dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Timeline visualization)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Gantt / timeline dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_017 — Cảnh báo trễ tiến độ

- **Mô tả:** Delay alert
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo trễ tiến độ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Delay alert)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Cảnh báo trễ tiến độ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_018 — Nhật ký thay đổi kế hoạch

- **Mô tả:** Plan change log
- **Tác nhân chính:** Project Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhật ký thay đổi kế hoạch
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Plan change log)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Nhật ký thay đổi kế hoạch” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.4. Nguồn lực & chi phí dự án (`PJM-04`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **Project Manager / Cost Controller**.

#### UC_PJM_019 — Phân công nhân sự

- **Mô tả:** Resource assignment
- **Tác nhân chính:** Project Manager / Cost Controller
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân công nhân sự
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Resource assignment)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Phân công nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_020 — Timesheet theo dự án

- **Mô tả:** Project timesheet
- **Tác nhân chính:** Project Manager / Cost Controller
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Timesheet theo dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project timesheet)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Timesheet theo dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_021 — Xuất nguyên vật liệu cho dự án

- **Mô tả:** Material issue to project
- **Tác nhân chính:** Project Manager / Cost Controller
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng chọn báo cáo/dashboard và bộ lọc
  2. Hệ thống kiểm tra quyền + data scope
  3. Truy vấn dữ liệu và hiển thị kết quả
  4. Người dùng xem chi tiết hoặc xuất Excel/PDF (nếu có quyền)
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Xuất nguyên vật liệu cho dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_022 — Ghi nhận chi phí phát sinh

- **Mô tả:** Expense entry
- **Tác nhân chính:** Project Manager / Cost Controller
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận chi phí phát sinh
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Expense entry)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Ghi nhận chi phí phát sinh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_023 — Theo dõi ngân sách vs thực tế

- **Mô tả:** Budget vs actual tracking
- **Tác nhân chính:** Project Manager / Cost Controller
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Theo dõi ngân sách vs thực tế
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget vs actual tracking)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Theo dõi ngân sách vs thực tế” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_024 — Cảnh báo vượt ngân sách

- **Mô tả:** Budget overrun alert
- **Tác nhân chính:** Project Manager / Cost Controller
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo vượt ngân sách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget overrun alert)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Cảnh báo vượt ngân sách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.5. Thực hiện dự án (`PJM-05`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **Project Member / PM**.

#### UC_PJM_025 — Checklist khảo sát

- **Mô tả:** Survey checklist
- **Tác nhân chính:** Project Member / PM
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist khảo sát
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Survey checklist)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Checklist khảo sát” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_026 — Checklist lắp đặt

- **Mô tả:** Installation checklist
- **Tác nhân chính:** Project Member / PM
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist lắp đặt
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Installation checklist)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Checklist lắp đặt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_027 — Checklist bàn giao

- **Mô tả:** Handover checklist
- **Tác nhân chính:** Project Member / PM
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist bàn giao
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Handover checklist)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Checklist bàn giao” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_028 — Ghi nhận ảnh / biên bản

- **Mô tả:** Site documentation
- **Tác nhân chính:** Project Member / PM
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận ảnh / biên bản
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Site documentation)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Ghi nhận ảnh / biên bản” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_029 — Phát sinh change request

- **Mô tả:** Change request
- **Tác nhân chính:** Project Member / PM
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phát sinh change request
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Change request)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Phát sinh change request” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_030 — Duyệt change request

- **Mô tả:** CR approval & impact
- **Tác nhân chính:** Project Member / PM
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người duyệt mở chứng từ từ hộp chờ hoặc liên kết thông báo
  2. Xem nội dung, lịch sử và ràng buộc nghiệp vụ
  3. Chọn Duyệt / Từ chối / Trả bổ sung kèm lý do nếu cần
  4. Hệ thống cập nhật trạng thái và phát sự kiện cho module nguồn
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Duyệt change request” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.6. Nghiệm thu & đóng dự án (`PJM-06`)

Nhóm này gồm **7** chức năng. Tác nhân mặc định: **Project Manager / Customer Rep**.

#### UC_PJM_031 — Biên bản nghiệm thu giai đoạn

- **Mô tả:** Phase acceptance
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Biên bản nghiệm thu giai đoạn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Phase acceptance)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Biên bản nghiệm thu giai đoạn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_032 — Nghiệm thu cuối & bàn giao

- **Mô tả:** Final acceptance
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nghiệm thu cuối & bàn giao
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Final acceptance)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Nghiệm thu cuối & bàn giao” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_033 — Khách ký xác nhận

- **Mô tả:** Customer sign-off
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khách ký xác nhận
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Customer sign-off)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Khách ký xác nhận” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_034 — Ghi nhận doanh thu dự án

- **Mô tả:** Revenue recognition
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận doanh thu dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Revenue recognition)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Ghi nhận doanh thu dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_035 — Quyết toán chi phí & P&L

- **Mô tả:** Project financial close
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quyết toán chi phí & P&L
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project financial close)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Quyết toán chi phí & P&L” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_036 — Đóng dự án / lưu trữ

- **Mô tả:** Close project
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đóng dự án / lưu trữ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Close project)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Đóng dự án / lưu trữ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_037 — Bảo hành sau dự án

- **Mô tả:** Post-project warranty
- **Tác nhân chính:** Project Manager / Customer Rep
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảo hành sau dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Post-project warranty)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Bảo hành sau dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.7. Báo cáo dự án (`PJM-07`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **PMO / Manager**.

#### UC_PJM_038 — Portfolio dự án đang chạy

- **Mô tả:** Project portfolio board
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Portfolio dự án đang chạy
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project portfolio board)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Portfolio dự án đang chạy” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_039 — Tiến độ & sức khỏe dự án

- **Mô tả:** RAG status report
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tiến độ & sức khỏe dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (RAG status report)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Tiến độ & sức khỏe dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_040 — Lợi nhuận theo dự án

- **Mô tả:** Project margin analysis
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lợi nhuận theo dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project margin analysis)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Lợi nhuận theo dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_PJM_041 — Năng suất nguồn lực

- **Mô tả:** Resource utilization
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Năng suất nguồn lực
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Resource utilization)
  3. Thực hiện xử lý nghiệp vụ / cập nhật dữ liệu
  4. Ghi nhận kết quả, thông báo (nếu có) và audit trail
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Năng suất nguồn lực” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_PJM_042 — Xuất báo cáo dự án

- **Mô tả:** Export project reports
- **Tác nhân chính:** PMO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `PJM`.
  - License module `PJM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng chọn báo cáo/dashboard và bộ lọc
  2. Hệ thống kiểm tra quyền + data scope
  3. Truy vấn dữ liệu và hiển thị kết quả
  4. Người dùng xem chi tiết hoặc xuất Excel/PDF (nếu có quyền)
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Xuất báo cáo dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

---

## 8. Workflow end-to-end

### WF-PJM-01 — Vòng đời dự án

**Mục tiêu:** Từ khởi tạo đến đóng DA

| Bước | Mô tả |
|---:|---|
| 1 | Tạo DA từ CRM/thủ công; duyệt kickoff |
| 2 | Lập WBS, milestone, ngân sách, team |
| 3 | Thực hiện; xuất vật tư; cập nhật tiến độ |
| 4 | Xử lý CR nếu phát sinh |
| 5 | Nghiệm thu; ghi doanh thu/chi phí; đóng DA |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Project / ProjectType` | Dự án |
| `WbsItem / Milestone` | Kế hoạch |
| `ProjectMember / TimeEntry` | Nguồn lực |
| `ProjectCost / ProjectBudget` | Tài chính DA |
| `ChangeRequest` | CR |
| `AcceptanceRecord` | Nghiệm thu |

### 9.1. Xuất xứ & kiểm soát dữ liệu
- Master dùng chung (KH, SP, chi nhánh…) tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ nghiệp vụ có trạng thái vòng đời rõ ràng (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete hoặc trạng thái ngưng dùng là mặc định; hạn chế xóa cứng.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-PJM-01: Vượt ngân sách quá ngưỡng phải cảnh báo/duyệt.
- BR-PJM-02: Đóng DA chỉ khi nghiệm thu cuối hoàn tất (hoặc hủy có duyệt).
- BR-PJM-03: CR làm đổi phạm vi/NS/tiến độ phải được duyệt trước thực hiện.
- BR-PJM-04: Xuất vật tư DA phải gắn mã dự án.
- BR-PJM-GEN-01: Mọi thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-PJM-GEN-02: Mọi chứng từ có mã duy nhất theo rule Sequence của SYS.
- BR-PJM-GEN-03: Thao tác sau khi khóa kỳ/chốt sổ (nếu có) phải đi đường điều chỉnh có kiểm soát.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Portfolio view | Xem nhiều DA theo trạng thái RAG |
| Audit | Lịch sử đổi kế hoạch/NS |
| Usability | Form có validate rõ; bảng có lọc/phân trang; hỗ trợ tiếng Việt |
| Reliability | Không mất chứng từ đã post; giao dịch quan trọng atomic |
| Maintainability | Permission và cấu hình không hard-code trong source nghiệp vụ |
| Observability | Có log ứng dụng + audit nghiệp vụ tách bạch |

---

## 12. Tích hợp & sự kiện

### 12.1. Ma trận tích hợp

| Thành phần | Mô tả |
|---|---|
| CRM | Cơ hội/HĐ |
| INV | Vật tư |
| FIN | Doanh thu/chi phí |
| HRM | Nhân sự DA |
| FSM | Bảo hành sau bàn giao |
| WF | Duyệt kickoff/CR |

### 12.2. Sự kiện (logical)
- `PJM.EntityCreated` / `PJM.EntityUpdated` / `PJM.EntityStatusChanged`
- `PJM.DocumentSubmitted` / `PJM.DocumentApproved` / `PJM.DocumentPosted`
- Mapping cụ thể API/topic sẽ định nghĩa ở tài liệu Interface Spec sau khi chốt SRS.

---

## 13. Phân quyền & bảo mật

### 13.1. Permission catalog (đề xuất)

- `pjm.project.manage`
- `pjm.plan.manage`
- `pjm.cost.manage`
- `pjm.accept.manage`
- `pjm.report.view`

### 13.2. Nguyên tắc
- Deny by default; chỉ mở theo role.
- Data scope theo chi nhánh/kho/đơn vị do SYS quyết định.
- Field-level security cho dữ liệu nhạy cảm (lương, công nợ chi tiết, giá vốn…) khi áp dụng.
- Mọi thay đổi phân quyền và thao tác critical ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| On-time milestone | Giám sát vận hành module `PJM` |
| Budget variance | Giám sát vận hành module `PJM` |
| Margin by project | Giám sát vận hành module `PJM` |
| Resource utilization | Giám sát vận hành module `PJM` |

Báo cáo chi tiết vận hành nằm trong từng nhóm “Báo cáo…” của Mục 7; tổng hợp điều hành nằm trên module `BI` khi khách mua thêm.

---

## 15. Giả định, rủi ro & câu hỏi mở

### 15.1. Giả định
- Áp dụng cho dự án dịch vụ/triển khai/nội bộ cấu hình được.

### 15.2. Câu hỏi mở cần chốt
- Có cần Gantt phụ thuộc phức tạp phase 1 không?

### 15.3. Rủi ro
- Phụ thuộc module khác chưa mua → một số workflow E2E chỉ chạy được một phần (cần nêu rõ khi bán gói).
- Cấu hình quá linh hoạt có thể làm tăng effort QA; cần bộ template mặc định.
- Chưa chốt chuẩn kế toán/thuế chi tiết có thể ảnh hưởng FIN và posting.

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện nghiệm thu module
1. 100% UC ưu tiên **Bắt buộc (Must)** của `PJM` pass UAT.
2. Các workflow E2E ở Mục 8 chạy thành công trên dữ liệu mẫu.
3. Phân quyền & data scope được kiểm thử với ít nhất 3 role.
4. Audit log ghi nhận các thao tác critical.
5. Tích hợp với `SYS` và các phụ thuộc bắt buộc hoạt động ổn định.
6. Tài liệu hướng dẫn cấu hình template mặc định đi kèm.

### 16.2. Truy vết
| Artifact | Liên kết |
|---|---|
| Catalog chức năng | `../00. Tổng quan/cay_chuc_nang_data.py` |
| Excel tổng hợp | `../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx` |
| Chuẩn viết SRS | `../00_CHUAN_VIET_SRS.md` |
| Use case IDs | `UC_PJM_001` … `UC_PJM_042` |

---

*Hết tài liệu SRS-PJM-v1.0.*
