# SRS-PJM-v1.1 — Quản lý dự án

> **Software Requirements Specification — Module PJM**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-PJM-v1.1` |
| Module | `PJM` — Quản lý dự án |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Sản xuất & Dịch vụ |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `CRM`, `INV`, `FIN`, `HRM`, `WF` |
| Số nhóm / UC | 7 nhóm / 42 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Quản lý dự án** (`PJM`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Quản lý dự án dịch vụ/triển khai: WBS, tiến độ, nguồn lực, chi phí, change request, nghiệm thu và P&L dự án.

### 1.3. Mục tiêu đo được
1. Theo dõi tiến độ và ngân sách dự án realtime.
2. Gắn dự án với cơ hội/hợp đồng và vật tư.
3. Nghiệm thu – quyết toán rõ ràng.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Project types/templates, initiation, WBS/schedule, resources/cost, execution checklists, acceptance, project P&L/reports.

### 2.2. Out of Scope
- PM phức tạp kiểu construction BIM.
- HRM full payroll.

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `CRM`, `INV`, `FIN`, `HRM`, `WF`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Project Manager | Điều phối dự án |
| Project Member | Thực hiện hạng mục |
| PMO / Manager | Portfolio & duyệt NS |
| Customer Rep | Nghiệm thu |
| Cost Controller | Chi phí–doanh thu DA |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| WBS | Work Breakdown Structure |
| Milestone | Mốc tiến độ |
| CR | Change Request |
| Project P&L | Lãi lỗ theo dự án |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- PJM (Quản lý dự án)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `PJM` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `PJM` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM | Cơ hội/HĐ |
| Tích hợp | INV | Vật tư |
| Tích hợp | FIN | Doanh thu/chi phí |
| Tích hợp | HRM | Nhân sự DA |
| Tích hợp | FSM | Bảo hành sau bàn giao |
| Tích hợp | WF | Duyệt kickoff/CR |

---

## 6. Catalog chức năng

**Tổng:** 7 nhóm · 42 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `PJM-01` | Danh mục dự án | 4 | 3 | 1 | 0 |
| 2 | `PJM-02` | Khởi tạo dự án | 6 | 5 | 1 | 0 |
| 3 | `PJM-03` | Kế hoạch & tiến độ | 8 | 5 | 2 | 1 |
| 4 | `PJM-04` | Nguồn lực & chi phí dự án | 6 | 4 | 2 | 0 |
| 5 | `PJM-05` | Thực hiện dự án | 6 | 0 | 6 | 0 |
| 6 | `PJM-06` | Nghiệm thu & đóng dự án | 7 | 6 | 1 | 0 |
| 7 | `PJM-07` | Báo cáo dự án | 5 | 4 | 1 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_PJM_001` | Danh mục dự án | Loại dự án | Must |
| `UC_PJM_002` | Danh mục dự án | Mẫu hạng mục / WBS | Must |
| `UC_PJM_003` | Danh mục dự án | Mẫu checklist nghiệm thu | Should |
| `UC_PJM_004` | Danh mục dự án | Trạng thái dự án chuẩn | Must |
| `UC_PJM_005` | Khởi tạo dự án | Tạo dự án từ cơ hội CRM | Must |
| `UC_PJM_006` | Khởi tạo dự án | Tạo dự án thủ công | Must |
| `UC_PJM_007` | Khởi tạo dự án | Gắn khách hàng / hợp đồng | Must |
| `UC_PJM_008` | Khởi tạo dự án | Gán quản lý dự án / thành viên | Must |
| `UC_PJM_009` | Khởi tạo dự án | Ngân sách dự kiến & timeline | Must |
| `UC_PJM_010` | Khởi tạo dự án | Phê duyệt khởi động | Should |
| `UC_PJM_011` | Kế hoạch & tiến độ | Tạo hạng mục WBS | Must |
| `UC_PJM_012` | Kế hoạch & tiến độ | Gán người thực hiện | Must |
| `UC_PJM_013` | Kế hoạch & tiến độ | Cập nhật % hoàn thành | Must |
| `UC_PJM_014` | Kế hoạch & tiến độ | Milestone & deadline | Must |
| `UC_PJM_015` | Kế hoạch & tiến độ | Phụ thuộc giữa hạng mục | Could |
| `UC_PJM_016` | Kế hoạch & tiến độ | Gantt / timeline dự án | Should |
| `UC_PJM_017` | Kế hoạch & tiến độ | Cảnh báo trễ tiến độ | Must |
| `UC_PJM_018` | Kế hoạch & tiến độ | Nhật ký thay đổi kế hoạch | Should |
| `UC_PJM_019` | Nguồn lực & chi phí dự án | Phân công nhân sự | Must |
| `UC_PJM_020` | Nguồn lực & chi phí dự án | Timesheet theo dự án | Should |
| `UC_PJM_021` | Nguồn lực & chi phí dự án | Xuất nguyên vật liệu cho dự án | Must |
| `UC_PJM_022` | Nguồn lực & chi phí dự án | Ghi nhận chi phí phát sinh | Must |
| `UC_PJM_023` | Nguồn lực & chi phí dự án | Theo dõi ngân sách vs thực tế | Must |
| `UC_PJM_024` | Nguồn lực & chi phí dự án | Cảnh báo vượt ngân sách | Should |
| `UC_PJM_025` | Thực hiện dự án | Checklist khảo sát | Should |
| `UC_PJM_026` | Thực hiện dự án | Checklist lắp đặt | Should |
| `UC_PJM_027` | Thực hiện dự án | Checklist bàn giao | Should |
| `UC_PJM_028` | Thực hiện dự án | Ghi nhận ảnh / biên bản | Should |
| `UC_PJM_029` | Thực hiện dự án | Phát sinh change request | Should |
| `UC_PJM_030` | Thực hiện dự án | Duyệt change request | Should |
| `UC_PJM_031` | Nghiệm thu & đóng dự án | Biên bản nghiệm thu giai đoạn | Must |
| `UC_PJM_032` | Nghiệm thu & đóng dự án | Nghiệm thu cuối & bàn giao | Must |
| `UC_PJM_033` | Nghiệm thu & đóng dự án | Khách ký xác nhận | Must |
| `UC_PJM_034` | Nghiệm thu & đóng dự án | Ghi nhận doanh thu dự án | Must |
| `UC_PJM_035` | Nghiệm thu & đóng dự án | Quyết toán chi phí & P&L | Must |
| `UC_PJM_036` | Nghiệm thu & đóng dự án | Đóng dự án / lưu trữ | Must |
| `UC_PJM_037` | Nghiệm thu & đóng dự án | Bảo hành sau dự án | Should |
| `UC_PJM_038` | Báo cáo dự án | Portfolio dự án đang chạy | Must |
| `UC_PJM_039` | Báo cáo dự án | Tiến độ & sức khỏe dự án | Must |
| `UC_PJM_040` | Báo cáo dự án | Lợi nhuận theo dự án | Must |
| `UC_PJM_041` | Báo cáo dự án | Năng suất nguồn lực | Should |
| `UC_PJM_042` | Báo cáo dự án | Xuất báo cáo dự án | Must |

</details>

### 6.1. Đề xuất Phase
| Phase | Phạm vi gợi ý |
|---|---|
| Phase 1 — Go-live | Toàn bộ **Must** |
| Phase 2 — Vận hành nâng cao | Các **Should** |
| Phase 3 — Mở rộng | **Could / Later** |

---

## 7. Đặc tả Use Case theo nhóm

Mỗi use case được đặc tả bằng **một bảng thống nhất** gồm 8 trường: Use Case ID, Tên Use Case, Tác nhân, Mô tả chức năng, Điều kiện tiên quyết, Yêu cầu, Kịch bản chính, Kịch bản phụ.

### 7.1. Danh mục dự án (`PJM-01`)

Nhóm **Danh mục dự án** gồm **4** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 3 |

**Bảng 1. Đặc tả Use Case "Loại dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_001 |
| **Tên Use Case** | Loại dự án |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Loại dự án" thuộc nhóm Danh mục dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project types |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Loại dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Loại dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Loại dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Loại dự án» trong nhóm Danh mục dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project types).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Loại dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Loại dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Mẫu hạng mục / WBS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_002 |
| **Tên Use Case** | Mẫu hạng mục / WBS |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Mẫu hạng mục / WBS" thuộc nhóm Danh mục dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: WBS templates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mẫu hạng mục / WBS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mẫu hạng mục / WBS» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mẫu hạng mục / WBS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Mẫu hạng mục / WBS» trong nhóm Danh mục dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (WBS templates).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mẫu hạng mục / WBS».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mẫu hạng mục / WBS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Mẫu checklist nghiệm thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_003 |
| **Tên Use Case** | Mẫu checklist nghiệm thu |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Mẫu checklist nghiệm thu" thuộc nhóm Danh mục dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Acceptance checklist template |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mẫu checklist nghiệm thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mẫu checklist nghiệm thu» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mẫu checklist nghiệm thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Mẫu checklist nghiệm thu» trong nhóm Danh mục dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Acceptance checklist template).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mẫu checklist nghiệm thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mẫu checklist nghiệm thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Trạng thái dự án chuẩn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_004 |
| **Tên Use Case** | Trạng thái dự án chuẩn |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Trạng thái dự án chuẩn" thuộc nhóm Danh mục dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project status model |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trạng thái dự án chuẩn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trạng thái dự án chuẩn» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trạng thái dự án chuẩn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Trạng thái dự án chuẩn» trong nhóm Danh mục dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project status model).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Trạng thái dự án chuẩn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trạng thái dự án chuẩn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Khởi tạo dự án (`PJM-02`)

Nhóm **Khởi tạo dự án** gồm **6** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 5 |

**Bảng 5. Đặc tả Use Case "Tạo dự án từ cơ hội CRM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_005 |
| **Tên Use Case** | Tạo dự án từ cơ hội CRM |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Tạo dự án từ cơ hội CRM" thuộc nhóm Khởi tạo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project from opportunity |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo dự án từ cơ hội CRM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo dự án từ cơ hội CRM» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo dự án từ cơ hội CRM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager mở chức năng «Tạo dự án từ cơ hội CRM» trong nhóm Khởi tạo dự án.<br>2. Hệ thống kiểm tra license `PJM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo dự án từ cơ hội CRM» (Project from opportunity).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo dự án từ cơ hội CRM» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo dự án từ cơ hội CRM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 6. Đặc tả Use Case "Tạo dự án thủ công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_006 |
| **Tên Use Case** | Tạo dự án thủ công |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Tạo dự án thủ công" thuộc nhóm Khởi tạo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Manual project creation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo dự án thủ công» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo dự án thủ công» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo dự án thủ công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager mở chức năng «Tạo dự án thủ công» trong nhóm Khởi tạo dự án.<br>2. Hệ thống kiểm tra license `PJM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo dự án thủ công» (Manual project creation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo dự án thủ công» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo dự án thủ công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 7. Đặc tả Use Case "Gắn khách hàng / hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_007 |
| **Tên Use Case** | Gắn khách hàng / hợp đồng |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Gắn khách hàng / hợp đồng" thuộc nhóm Khởi tạo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project linkages |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn khách hàng / hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn khách hàng / hợp đồng» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn khách hàng / hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Gắn khách hàng / hợp đồng» trong nhóm Khởi tạo dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project linkages).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn khách hàng / hợp đồng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn khách hàng / hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Gán quản lý dự án / thành viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_008 |
| **Tên Use Case** | Gán quản lý dự án / thành viên |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Gán quản lý dự án / thành viên" thuộc nhóm Khởi tạo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project team assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán quản lý dự án / thành viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán quản lý dự án / thành viên» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán quản lý dự án / thành viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager chọn đối tượng nguồn trong «Gán quản lý dự án / thành viên».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán quản lý dự án / thành viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Ngân sách dự kiến & timeline"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_009 |
| **Tên Use Case** | Ngân sách dự kiến & timeline |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Ngân sách dự kiến & timeline" thuộc nhóm Khởi tạo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Budget & schedule baseline |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ngân sách dự kiến & timeline» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ngân sách dự kiến & timeline» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ngân sách dự kiến & timeline» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Ngân sách dự kiến & timeline» trong nhóm Khởi tạo dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Budget & schedule baseline).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ngân sách dự kiến & timeline».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ngân sách dự kiến & timeline» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Phê duyệt khởi động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_010 |
| **Tên Use Case** | Phê duyệt khởi động |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Phê duyệt khởi động" thuộc nhóm Khởi tạo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project kickoff approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phê duyệt khởi động» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`, `BR-PJM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phê duyệt khởi động» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phê duyệt khởi động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Project Manager mở hộp chờ / chứng từ cần xử lý cho «Phê duyệt khởi động».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Phê duyệt khởi động», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phê duyệt khởi động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

### 7.3. Kế hoạch & tiến độ (`PJM-03`)

Nhóm **Kế hoạch & tiến độ** gồm **8** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 5 |

**Bảng 11. Đặc tả Use Case "Tạo hạng mục WBS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_011 |
| **Tên Use Case** | Tạo hạng mục WBS |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Tạo hạng mục WBS" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: WBS breakdown |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo hạng mục WBS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo hạng mục WBS» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hạng mục WBS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager mở chức năng «Tạo hạng mục WBS» trong nhóm Kế hoạch & tiến độ.<br>2. Hệ thống kiểm tra license `PJM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo hạng mục WBS» (WBS breakdown).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo hạng mục WBS» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo hạng mục WBS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 12. Đặc tả Use Case "Gán người thực hiện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_012 |
| **Tên Use Case** | Gán người thực hiện |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Gán người thực hiện" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Task assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán người thực hiện» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán người thực hiện» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán người thực hiện» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager chọn đối tượng nguồn trong «Gán người thực hiện».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán người thực hiện» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 13. Đặc tả Use Case "Cập nhật % hoàn thành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_013 |
| **Tên Use Case** | Cập nhật % hoàn thành |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Cập nhật % hoàn thành" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Progress update |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cập nhật % hoàn thành» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cập nhật % hoàn thành» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cập nhật % hoàn thành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager tìm và mở bản ghi liên quan tới «Cập nhật % hoàn thành» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Cập nhật % hoàn thành» (Progress update).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cập nhật % hoàn thành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 14. Đặc tả Use Case "Milestone & deadline"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_014 |
| **Tên Use Case** | Milestone & deadline |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Milestone & deadline" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Milestone tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Milestone & deadline» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Milestone & deadline» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Milestone & deadline» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Milestone & deadline» trong nhóm Kế hoạch & tiến độ.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Milestone tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Milestone & deadline».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Milestone & deadline» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Phụ thuộc giữa hạng mục"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_015 |
| **Tên Use Case** | Phụ thuộc giữa hạng mục |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Phụ thuộc giữa hạng mục" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Task dependencies |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phụ thuộc giữa hạng mục» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phụ thuộc giữa hạng mục» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phụ thuộc giữa hạng mục» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Phụ thuộc giữa hạng mục» trong nhóm Kế hoạch & tiến độ.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Task dependencies).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phụ thuộc giữa hạng mục».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phụ thuộc giữa hạng mục» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Gantt / timeline dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_016 |
| **Tên Use Case** | Gantt / timeline dự án |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Gantt / timeline dự án" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Timeline visualization |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gantt / timeline dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gantt / timeline dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gantt / timeline dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Gantt / timeline dự án» trong nhóm Kế hoạch & tiến độ.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Timeline visualization).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gantt / timeline dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gantt / timeline dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Cảnh báo trễ tiến độ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_017 |
| **Tên Use Case** | Cảnh báo trễ tiến độ |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Cảnh báo trễ tiến độ" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Delay alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo trễ tiến độ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo trễ tiến độ» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo trễ tiến độ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Project Manager kích hoạt kiểm tra điều kiện «Cảnh báo trễ tiến độ».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Delay alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo trễ tiến độ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "Nhật ký thay đổi kế hoạch"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_018 |
| **Tên Use Case** | Nhật ký thay đổi kế hoạch |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Nhật ký thay đổi kế hoạch" thuộc nhóm Kế hoạch & tiến độ trong module PJM — Quản lý dự án. Mô tả chi tiết: Plan change log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhật ký thay đổi kế hoạch» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhật ký thay đổi kế hoạch» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhật ký thay đổi kế hoạch» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Nhật ký thay đổi kế hoạch» trong nhóm Kế hoạch & tiến độ.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Plan change log).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhật ký thay đổi kế hoạch».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhật ký thay đổi kế hoạch» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Nguồn lực & chi phí dự án (`PJM-04`)

Nhóm **Nguồn lực & chi phí dự án** gồm **6** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 19. Đặc tả Use Case "Phân công nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_019 |
| **Tên Use Case** | Phân công nhân sự |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Phân công nhân sự" thuộc nhóm Nguồn lực & chi phí dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Resource assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân công nhân sự» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân công nhân sự» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân công nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager chọn đối tượng nguồn trong «Phân công nhân sự».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân công nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Timesheet theo dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_020 |
| **Tên Use Case** | Timesheet theo dự án |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Timesheet theo dự án" thuộc nhóm Nguồn lực & chi phí dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project timesheet |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Timesheet theo dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Timesheet theo dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Timesheet theo dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Timesheet theo dự án» trong nhóm Nguồn lực & chi phí dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project timesheet).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Timesheet theo dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Timesheet theo dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Xuất nguyên vật liệu cho dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_021 |
| **Tên Use Case** | Xuất nguyên vật liệu cho dự án |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Xuất nguyên vật liệu cho dự án" thuộc nhóm Nguồn lực & chi phí dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Material issue to project |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất nguyên vật liệu cho dự án» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất nguyên vật liệu cho dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất nguyên vật liệu cho dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager mở «Xuất nguyên vật liệu cho dự án», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất nguyên vật liệu cho dự án» (Material issue to project).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất nguyên vật liệu cho dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Ghi nhận chi phí phát sinh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_022 |
| **Tên Use Case** | Ghi nhận chi phí phát sinh |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Ghi nhận chi phí phát sinh" thuộc nhóm Nguồn lực & chi phí dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Expense entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận chi phí phát sinh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận chi phí phát sinh» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận chi phí phát sinh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Ghi nhận chi phí phát sinh» trong nhóm Nguồn lực & chi phí dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Expense entry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận chi phí phát sinh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận chi phí phát sinh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Theo dõi ngân sách vs thực tế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_023 |
| **Tên Use Case** | Theo dõi ngân sách vs thực tế |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Theo dõi ngân sách vs thực tế" thuộc nhóm Nguồn lực & chi phí dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Budget vs actual tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi ngân sách vs thực tế» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi ngân sách vs thực tế» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi ngân sách vs thực tế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Theo dõi ngân sách vs thực tế» trong nhóm Nguồn lực & chi phí dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Budget vs actual tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi ngân sách vs thực tế».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi ngân sách vs thực tế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Cảnh báo vượt ngân sách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_024 |
| **Tên Use Case** | Cảnh báo vượt ngân sách |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Cảnh báo vượt ngân sách" thuộc nhóm Nguồn lực & chi phí dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Budget overrun alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo vượt ngân sách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo vượt ngân sách» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo vượt ngân sách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Project Manager kích hoạt kiểm tra điều kiện «Cảnh báo vượt ngân sách».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Budget overrun alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo vượt ngân sách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Thực hiện dự án (`PJM-05`)

Nhóm **Thực hiện dự án** gồm **6** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 0 |

**Bảng 25. Đặc tả Use Case "Checklist khảo sát"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_025 |
| **Tên Use Case** | Checklist khảo sát |
| **Tác nhân** | Project Member |
| **Mô tả chức năng** | Cho phép Project Member thực hiện chức năng "Checklist khảo sát" thuộc nhóm Thực hiện dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Survey checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Member] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist khảo sát» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist khảo sát» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist khảo sát» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Member khởi tạo thao tác «Checklist khảo sát» trong nhóm Thực hiện dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Survey checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist khảo sát».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist khảo sát» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Checklist lắp đặt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_026 |
| **Tên Use Case** | Checklist lắp đặt |
| **Tác nhân** | Project Member |
| **Mô tả chức năng** | Cho phép Project Member thực hiện chức năng "Checklist lắp đặt" thuộc nhóm Thực hiện dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Installation checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Member] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist lắp đặt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist lắp đặt» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist lắp đặt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Member khởi tạo thao tác «Checklist lắp đặt» trong nhóm Thực hiện dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Installation checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist lắp đặt».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist lắp đặt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 27. Đặc tả Use Case "Checklist bàn giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_027 |
| **Tên Use Case** | Checklist bàn giao |
| **Tác nhân** | Project Member |
| **Mô tả chức năng** | Cho phép Project Member thực hiện chức năng "Checklist bàn giao" thuộc nhóm Thực hiện dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Handover checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Member] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist bàn giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist bàn giao» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist bàn giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Member khởi tạo thao tác «Checklist bàn giao» trong nhóm Thực hiện dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Handover checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist bàn giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist bàn giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 28. Đặc tả Use Case "Ghi nhận ảnh / biên bản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_028 |
| **Tên Use Case** | Ghi nhận ảnh / biên bản |
| **Tác nhân** | Project Member |
| **Mô tả chức năng** | Cho phép Project Member thực hiện chức năng "Ghi nhận ảnh / biên bản" thuộc nhóm Thực hiện dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Site documentation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Member] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận ảnh / biên bản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận ảnh / biên bản» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận ảnh / biên bản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Member khởi tạo thao tác «Ghi nhận ảnh / biên bản» trong nhóm Thực hiện dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Site documentation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận ảnh / biên bản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận ảnh / biên bản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Phát sinh change request"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_029 |
| **Tên Use Case** | Phát sinh change request |
| **Tác nhân** | Project Member |
| **Mô tả chức năng** | Cho phép Project Member thực hiện chức năng "Phát sinh change request" thuộc nhóm Thực hiện dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Change request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Member] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phát sinh change request» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phát sinh change request» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phát sinh change request» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Member khởi tạo thao tác «Phát sinh change request» trong nhóm Thực hiện dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Change request).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phát sinh change request».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phát sinh change request» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Duyệt change request"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_030 |
| **Tên Use Case** | Duyệt change request |
| **Tác nhân** | Project Member |
| **Mô tả chức năng** | Cho phép Project Member thực hiện chức năng "Duyệt change request" thuộc nhóm Thực hiện dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: CR approval & impact |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Member] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt change request» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`, `BR-PJM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt change request» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt change request» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Project Member mở hộp chờ / chứng từ cần xử lý cho «Duyệt change request».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt change request», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt change request» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

### 7.6. Nghiệm thu & đóng dự án (`PJM-06`)

Nhóm **Nghiệm thu & đóng dự án** gồm **7** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 6 |

**Bảng 31. Đặc tả Use Case "Biên bản nghiệm thu giai đoạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_031 |
| **Tên Use Case** | Biên bản nghiệm thu giai đoạn |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Biên bản nghiệm thu giai đoạn" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Phase acceptance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Biên bản nghiệm thu giai đoạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Biên bản nghiệm thu giai đoạn» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Biên bản nghiệm thu giai đoạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Biên bản nghiệm thu giai đoạn» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Phase acceptance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Biên bản nghiệm thu giai đoạn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Biên bản nghiệm thu giai đoạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 32. Đặc tả Use Case "Nghiệm thu cuối & bàn giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_032 |
| **Tên Use Case** | Nghiệm thu cuối & bàn giao |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Nghiệm thu cuối & bàn giao" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Final acceptance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nghiệm thu cuối & bàn giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nghiệm thu cuối & bàn giao» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nghiệm thu cuối & bàn giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Nghiệm thu cuối & bàn giao» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Final acceptance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nghiệm thu cuối & bàn giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nghiệm thu cuối & bàn giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 33. Đặc tả Use Case "Khách ký xác nhận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_033 |
| **Tên Use Case** | Khách ký xác nhận |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Khách ký xác nhận" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Customer sign-off |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khách ký xác nhận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khách ký xác nhận» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khách ký xác nhận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Khách ký xác nhận» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer sign-off).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khách ký xác nhận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khách ký xác nhận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 34. Đặc tả Use Case "Ghi nhận doanh thu dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_034 |
| **Tên Use Case** | Ghi nhận doanh thu dự án |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Ghi nhận doanh thu dự án" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Revenue recognition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận doanh thu dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận doanh thu dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận doanh thu dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Ghi nhận doanh thu dự án» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Revenue recognition).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận doanh thu dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận doanh thu dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 35. Đặc tả Use Case "Quyết toán chi phí & P&L"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_035 |
| **Tên Use Case** | Quyết toán chi phí & P&L |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Quyết toán chi phí & P&L" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project financial close |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quyết toán chi phí & P&L» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quyết toán chi phí & P&L» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quyết toán chi phí & P&L» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Quyết toán chi phí & P&L» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project financial close).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Quyết toán chi phí & P&L».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quyết toán chi phí & P&L» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Đóng dự án / lưu trữ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_036 |
| **Tên Use Case** | Đóng dự án / lưu trữ |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Đóng dự án / lưu trữ" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Close project |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng dự án / lưu trữ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng dự án / lưu trữ» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng dự án / lưu trữ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Đóng dự án / lưu trữ» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Close project).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng dự án / lưu trữ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng dự án / lưu trữ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 37. Đặc tả Use Case "Bảo hành sau dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_037 |
| **Tên Use Case** | Bảo hành sau dự án |
| **Tác nhân** | Project Manager |
| **Mô tả chức năng** | Cho phép Project Manager thực hiện chức năng "Bảo hành sau dự án" thuộc nhóm Nghiệm thu & đóng dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Post-project warranty |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Project Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảo hành sau dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảo hành sau dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảo hành sau dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Project Manager khởi tạo thao tác «Bảo hành sau dự án» trong nhóm Nghiệm thu & đóng dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Post-project warranty).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảo hành sau dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảo hành sau dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Báo cáo dự án (`PJM-07`)

Nhóm **Báo cáo dự án** gồm **5** use case của module `PJM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 4 |

**Bảng 38. Đặc tả Use Case "Portfolio dự án đang chạy"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_038 |
| **Tên Use Case** | Portfolio dự án đang chạy |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Portfolio dự án đang chạy" thuộc nhóm Báo cáo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project portfolio board |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Portfolio dự án đang chạy» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Portfolio dự án đang chạy» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Portfolio dự án đang chạy» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Portfolio dự án đang chạy» trong nhóm Báo cáo dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project portfolio board).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Portfolio dự án đang chạy».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Portfolio dự án đang chạy» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 39. Đặc tả Use Case "Tiến độ & sức khỏe dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_039 |
| **Tên Use Case** | Tiến độ & sức khỏe dự án |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Tiến độ & sức khỏe dự án" thuộc nhóm Báo cáo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: RAG status report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiến độ & sức khỏe dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiến độ & sức khỏe dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiến độ & sức khỏe dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Tiến độ & sức khỏe dự án» trong nhóm Báo cáo dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (RAG status report).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiến độ & sức khỏe dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiến độ & sức khỏe dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 40. Đặc tả Use Case "Lợi nhuận theo dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_040 |
| **Tên Use Case** | Lợi nhuận theo dự án |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Lợi nhuận theo dự án" thuộc nhóm Báo cáo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Project margin analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lợi nhuận theo dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lợi nhuận theo dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lợi nhuận theo dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Lợi nhuận theo dự án» trong nhóm Báo cáo dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Project margin analysis).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lợi nhuận theo dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lợi nhuận theo dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 41. Đặc tả Use Case "Năng suất nguồn lực"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_041 |
| **Tên Use Case** | Năng suất nguồn lực |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Năng suất nguồn lực" thuộc nhóm Báo cáo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Resource utilization |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Năng suất nguồn lực» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Năng suất nguồn lực» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Năng suất nguồn lực» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. PMO khởi tạo thao tác «Năng suất nguồn lực» trong nhóm Báo cáo dự án.<br>2. Hệ thống kiểm tra license `PJM`, quyền RBAC và tiền điều kiện nghiệp vụ (Resource utilization).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Năng suất nguồn lực».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Năng suất nguồn lực» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 42. Đặc tả Use Case "Xuất báo cáo dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PJM_042 |
| **Tên Use Case** | Xuất báo cáo dự án |
| **Tác nhân** | PMO |
| **Mô tả chức năng** | Cho phép PMO thực hiện chức năng "Xuất báo cáo dự án" thuộc nhóm Báo cáo dự án trong module PJM — Quản lý dự án. Mô tả chi tiết: Export project reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [PMO] và được cấp quyền RBAC tương ứng.<br>• License module `PJM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo dự án» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PJM-SCOPE-01`, `BR-PJM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo dự án» được lưu nhất quán trong module `PJM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. PMO mở «Xuất báo cáo dự án», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo dự án» (Export project reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

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

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

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

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `PJM`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-PJM-01: Vượt ngân sách quá ngưỡng phải cảnh báo/duyệt.
- BR-PJM-02: Đóng DA chỉ khi nghiệm thu cuối hoàn tất (hoặc hủy có duyệt).
- BR-PJM-03: CR làm đổi phạm vi/NS/tiến độ phải được duyệt trước thực hiện.
- BR-PJM-04: Xuất vật tư DA phải gắn mã dự án.
- BR-PJM-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-PJM-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-PJM-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Portfolio view | Xem nhiều DA theo trạng thái RAG |
| Audit | Lịch sử đổi kế hoạch/NS |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `PJM` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `pjm.project.manage` | Quyền chức năng module |
| `pjm.plan.manage` | Quyền chức năng module |
| `pjm.cost.manage` | Quyền chức năng module |
| `pjm.accept.manage` | Quyền chức năng module |
| `pjm.report.view` | Quyền chức năng module |
| `pjm.*.view` | Xem trong data scope |
| `pjm.*.manage` | Tạo/sửa trong data scope |
| `pjm.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| On-time milestone | Theo dõi vận hành module |
| Budget variance | Theo dõi vận hành module |
| Margin by project | Theo dõi vận hành module |
| Resource utilization | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Áp dụng cho dự án dịch vụ/triển khai/nội bộ cấu hình được.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có cần Gantt phụ thuộc phức tạp phase 1 không?

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện chấp nhận module
1. 100% UC **Must** pass UAT.
2. Các workflow mục 8 chạy thành công trên môi trường demo.
3. Kiểm thử license: tắt module → menu mất + API 403; dữ liệu vẫn còn.
4. Kiểm thử RBAC + data scope với ≥ 2 role và ≥ 2 đơn vị/chi nhánh (nếu áp dụng).
5. Audit có before/after cho thao tác trọng yếu.
6. Không còn UC dùng luồng khuôn mẫu sai lệch nghiệp vụ.

### 16.2. Truy vết
| Artifact | Vị trí |
|---|---|
| Catalog chức năng | `../../00. Tổng quan/cay_chuc_nang_data.py` |
| Excel tổng hợp | `../../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx` |
| Chuẩn SRS | `../00_CHUAN_VIET_SRS.md` |
| Bản SRS này | `SRS_PJM_v1.1.md` / `.docx` |
| UC IDs | `UC_PJM_001` … |

---

*Hết tài liệu SRS-PJM-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
