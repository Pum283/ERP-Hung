# SRS-WF-v1.1 — Công việc & Phê duyệt

> **Software Requirements Specification — Module WF**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-WF-v1.1` |
| Module | `WF` — Công việc & Phê duyệt |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Quản trị & Báo cáo |
| Bán riêng | Có (hoặc kèm SYS) |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `Tất cả module phát sinh chứng từ cần duyệt` |
| Số nhóm / UC | 7 nhóm / 40 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Công việc & Phê duyệt** (`WF`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Cung cấp task/ticket nội bộ, board công việc và engine workflow phê duyệt đa cấp dùng chung cho toàn ERP.

### 1.3. Mục tiêu đo được
1. Một hộp chờ duyệt thống nhất.
2. Cấu hình quy trình duyệt theo loại chứng từ và điều kiện.
3. Giảm tắc nghẽn phê duyệt bằng SLA và ủy quyền.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Tasks, boards, internal tickets, workflow designer, approval runtime, WF reports.

### 2.2. Out of Scope
- BPM cực phức tạp thay thế iBPMS doanh nghiệp lớn ngay phase 1.

### 2.3. Đóng gói bán
- **Bán riêng:** Có (hoặc kèm SYS)
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `Tất cả module phát sinh chứng từ cần duyệt`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Process Admin | Thiết kế workflow |
| Approver | Duyệt chứng từ |
| Requester | Trình duyệt / tạo task |
| Team Member | Thực hiện task |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Workflow | Quy trình duyệt có bước/điều kiện |
| Delegation | Ủy quyền duyệt tạm thời |
| Escalation | Chuyển cấp khi quá hạn |
| Inbox | Hộp chờ duyệt |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- WF (Công việc & Phê duyệt)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `WF` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `WF` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | SYS | User/role/thông báo |
| Tích hợp | HRM/CRM/PUR/FIN/PJM/… | Chứng từ nguồn |

---

## 6. Catalog chức năng

**Tổng:** 7 nhóm · 40 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `WF-01` | Danh mục công việc | 4 | 2 | 2 | 0 |
| 2 | `WF-02` | Giao việc & theo dõi | 8 | 5 | 3 | 0 |
| 3 | `WF-03` | Bảng làm việc (Board) | 4 | 1 | 3 | 0 |
| 4 | `WF-04` | Ticket nội bộ / helpdesk | 5 | 1 | 2 | 2 |
| 5 | `WF-05` | Thiết kế quy trình phê duyệt | 6 | 4 | 1 | 1 |
| 6 | `WF-06` | Thực thi phê duyệt | 8 | 6 | 2 | 0 |
| 7 | `WF-07` | Báo cáo quy trình & công việc | 5 | 2 | 3 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_WF_001` | Danh mục công việc | Loại công việc / ticket | Must |
| `UC_WF_002` | Danh mục công việc | Độ ưu tiên & SLA nội bộ | Should |
| `UC_WF_003` | Danh mục công việc | Mẫu công việc lặp lại | Should |
| `UC_WF_004` | Danh mục công việc | Nhóm / dự án nội bộ | Must |
| `UC_WF_005` | Giao việc & theo dõi | Tạo task / giao việc | Must |
| `UC_WF_006` | Giao việc & theo dõi | Gán người thực hiện / theo dõi | Must |
| `UC_WF_007` | Giao việc & theo dõi | Deadline / nhắc việc | Must |
| `UC_WF_008` | Giao việc & theo dõi | Checklist trong task | Should |
| `UC_WF_009` | Giao việc & theo dõi | Bình luận / đính kèm file | Must |
| `UC_WF_010` | Giao việc & theo dõi | Chuyển trạng thái task | Must |
| `UC_WF_011` | Giao việc & theo dõi | Ủy thác / chuyển người làm | Should |
| `UC_WF_012` | Giao việc & theo dõi | Task liên kết chứng từ ERP | Should |
| `UC_WF_013` | Bảng làm việc (Board) | Kanban theo nhóm/dự án | Should |
| `UC_WF_014` | Bảng làm việc (Board) | Lọc task theo tiêu chí | Must |
| `UC_WF_015` | Bảng làm việc (Board) | Calendar công việc | Should |
| `UC_WF_016` | Bảng làm việc (Board) | Workload theo người | Should |
| `UC_WF_017` | Ticket nội bộ / helpdesk | Tạo ticket nội bộ | Must |
| `UC_WF_018` | Ticket nội bộ / helpdesk | Phân loại & định tuyến | Should |
| `UC_WF_019` | Ticket nội bộ / helpdesk | Escalate ticket quá hạn | Should |
| `UC_WF_020` | Ticket nội bộ / helpdesk | CSAT nội bộ | Later |
| `UC_WF_021` | Ticket nội bộ / helpdesk | Kiến thức / FAQ nội bộ | Could |
| `UC_WF_022` | Thiết kế quy trình phê duyệt | Tạo mẫu workflow duyệt | Must |
| `UC_WF_023` | Thiết kế quy trình phê duyệt | Điều kiện duyệt theo quy tắc | Must |
| `UC_WF_024` | Thiết kế quy trình phê duyệt | Nhiều cấp duyệt tuần tự / song song | Must |
| `UC_WF_025` | Thiết kế quy trình phê duyệt | Gắn workflow vào loại chứng từ | Must |
| `UC_WF_026` | Thiết kế quy trình phê duyệt | Phiên bản quy trình | Should |
| `UC_WF_027` | Thiết kế quy trình phê duyệt | Mô phỏng / kiểm thử | Could |
| `UC_WF_028` | Thực thi phê duyệt | Hộp chờ duyệt của tôi | Must |
| `UC_WF_029` | Thực thi phê duyệt | Duyệt / từ chối / trả bổ sung | Must |
| `UC_WF_030` | Thực thi phê duyệt | Duyệt hàng loạt | Should |
| `UC_WF_031` | Thực thi phê duyệt | Duyệt trên mobile APP | Must |
| `UC_WF_032` | Thực thi phê duyệt | Ủy quyền duyệt tạm thời | Must |
| `UC_WF_033` | Thực thi phê duyệt | Nhắc duyệt / escalate | Must |
| `UC_WF_034` | Thực thi phê duyệt | Lịch sử duyệt & comment | Must |
| `UC_WF_035` | Thực thi phê duyệt | Thu hồi chứng từ đang chờ | Should |
| `UC_WF_036` | Báo cáo quy trình & công việc | Thời gian duyệt trung bình | Should |
| `UC_WF_037` | Báo cáo quy trình & công việc | Bottleneck cấp duyệt | Should |
| `UC_WF_038` | Báo cáo quy trình & công việc | Khối lượng task mở / quá hạn | Must |
| `UC_WF_039` | Báo cáo quy trình & công việc | Năng suất hoàn thành | Should |
| `UC_WF_040` | Báo cáo quy trình & công việc | Dashboard workflow | Must |

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

### 7.1. Danh mục công việc (`WF-01`)

Nhóm **Danh mục công việc** gồm **4** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 2 |

**Bảng 1. Đặc tả Use Case "Loại công việc / ticket"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_001 |
| **Tên Use Case** | Loại công việc / ticket |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Loại công việc / ticket" thuộc nhóm Danh mục công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Work types |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Loại công việc / ticket» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Loại công việc / ticket» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Loại công việc / ticket» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Loại công việc / ticket» trong nhóm Danh mục công việc.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Work types).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Loại công việc / ticket».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Loại công việc / ticket» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 2. Đặc tả Use Case "Độ ưu tiên & SLA nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_002 |
| **Tên Use Case** | Độ ưu tiên & SLA nội bộ |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Độ ưu tiên & SLA nội bộ" thuộc nhóm Danh mục công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Priority & internal SLA |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Độ ưu tiên & SLA nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Độ ưu tiên & SLA nội bộ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Độ ưu tiên & SLA nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Độ ưu tiên & SLA nội bộ» trong nhóm Danh mục công việc.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Priority & internal SLA).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Độ ưu tiên & SLA nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Độ ưu tiên & SLA nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 3. Đặc tả Use Case "Mẫu công việc lặp lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_003 |
| **Tên Use Case** | Mẫu công việc lặp lại |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Mẫu công việc lặp lại" thuộc nhóm Danh mục công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Recurring work templates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mẫu công việc lặp lại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mẫu công việc lặp lại» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mẫu công việc lặp lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Mẫu công việc lặp lại» trong nhóm Danh mục công việc.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Recurring work templates).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mẫu công việc lặp lại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mẫu công việc lặp lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 4. Đặc tả Use Case "Nhóm / dự án nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_004 |
| **Tên Use Case** | Nhóm / dự án nội bộ |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Nhóm / dự án nội bộ" thuộc nhóm Danh mục công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Workspaces |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhóm / dự án nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhóm / dự án nội bộ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhóm / dự án nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Nhóm / dự án nội bộ» trong nhóm Danh mục công việc.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Workspaces).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhóm / dự án nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhóm / dự án nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

### 7.2. Giao việc & theo dõi (`WF-02`)

Nhóm **Giao việc & theo dõi** gồm **8** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 5 |

**Bảng 5. Đặc tả Use Case "Tạo task / giao việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_005 |
| **Tên Use Case** | Tạo task / giao việc |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Tạo task / giao việc" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Create task |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo task / giao việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo task / giao việc» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo task / giao việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester mở chức năng «Tạo task / giao việc» trong nhóm Giao việc & theo dõi.<br>2. Hệ thống kiểm tra license `WF`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo task / giao việc» (Create task).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo task / giao việc» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo task / giao việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 6. Đặc tả Use Case "Gán người thực hiện / theo dõi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_006 |
| **Tên Use Case** | Gán người thực hiện / theo dõi |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Gán người thực hiện / theo dõi" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Assignee & followers |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán người thực hiện / theo dõi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán người thực hiện / theo dõi» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán người thực hiện / theo dõi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester chọn đối tượng nguồn trong «Gán người thực hiện / theo dõi».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán người thực hiện / theo dõi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 7. Đặc tả Use Case "Deadline / nhắc việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_007 |
| **Tên Use Case** | Deadline / nhắc việc |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Deadline / nhắc việc" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Due date & reminders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Deadline / nhắc việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Deadline / nhắc việc» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Deadline / nhắc việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Deadline / nhắc việc» trong nhóm Giao việc & theo dõi.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Due date & reminders).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Deadline / nhắc việc».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Deadline / nhắc việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 8. Đặc tả Use Case "Checklist trong task"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_008 |
| **Tên Use Case** | Checklist trong task |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Checklist trong task" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Sub-checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist trong task» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist trong task» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist trong task» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Checklist trong task» trong nhóm Giao việc & theo dõi.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Sub-checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist trong task».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist trong task» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 9. Đặc tả Use Case "Bình luận / đính kèm file"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_009 |
| **Tên Use Case** | Bình luận / đính kèm file |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Bình luận / đính kèm file" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Collaboration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bình luận / đính kèm file» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bình luận / đính kèm file» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bình luận / đính kèm file» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester mở bản ghi liên quan và chọn «Bình luận / đính kèm file».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bình luận / đính kèm file» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 10. Đặc tả Use Case "Chuyển trạng thái task"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_010 |
| **Tên Use Case** | Chuyển trạng thái task |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Chuyển trạng thái task" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Task status workflow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển trạng thái task» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển trạng thái task» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển trạng thái task» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester tìm và mở bản ghi liên quan tới «Chuyển trạng thái task» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Chuyển trạng thái task» (Task status workflow).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển trạng thái task» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 11. Đặc tả Use Case "Ủy thác / chuyển người làm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_011 |
| **Tên Use Case** | Ủy thác / chuyển người làm |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Ủy thác / chuyển người làm" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Task reassignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ủy thác / chuyển người làm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ủy thác / chuyển người làm» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ủy thác / chuyển người làm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Ủy thác / chuyển người làm» trong nhóm Giao việc & theo dõi.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Task reassignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ủy thác / chuyển người làm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ủy thác / chuyển người làm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 12. Đặc tả Use Case "Task liên kết chứng từ ERP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_012 |
| **Tên Use Case** | Task liên kết chứng từ ERP |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Task liên kết chứng từ ERP" thuộc nhóm Giao việc & theo dõi trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Link to documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Task liên kết chứng từ ERP» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Task liên kết chứng từ ERP» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Task liên kết chứng từ ERP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Task liên kết chứng từ ERP» trong nhóm Giao việc & theo dõi.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Link to documents).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Task liên kết chứng từ ERP».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Task liên kết chứng từ ERP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

### 7.3. Bảng làm việc (Board) (`WF-03`)

Nhóm **Bảng làm việc (Board)** gồm **4** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 1 |

**Bảng 13. Đặc tả Use Case "Kanban theo nhóm/dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_013 |
| **Tên Use Case** | Kanban theo nhóm/dự án |
| **Tác nhân** | Team Member |
| **Mô tả chức năng** | Cho phép Team Member thực hiện chức năng "Kanban theo nhóm/dự án" thuộc nhóm Bảng làm việc (Board) trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Kanban board |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Team Member] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kanban theo nhóm/dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kanban theo nhóm/dự án» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kanban theo nhóm/dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Team Member khởi tạo thao tác «Kanban theo nhóm/dự án» trong nhóm Bảng làm việc (Board).<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Kanban board).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kanban theo nhóm/dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kanban theo nhóm/dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 14. Đặc tả Use Case "Lọc task theo tiêu chí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_014 |
| **Tên Use Case** | Lọc task theo tiêu chí |
| **Tác nhân** | Team Member |
| **Mô tả chức năng** | Cho phép Team Member thực hiện chức năng "Lọc task theo tiêu chí" thuộc nhóm Bảng làm việc (Board) trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Task filters |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Team Member] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lọc task theo tiêu chí» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lọc task theo tiêu chí» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lọc task theo tiêu chí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Team Member khởi tạo thao tác «Lọc task theo tiêu chí» trong nhóm Bảng làm việc (Board).<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Task filters).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lọc task theo tiêu chí».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lọc task theo tiêu chí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 15. Đặc tả Use Case "Calendar công việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_015 |
| **Tên Use Case** | Calendar công việc |
| **Tác nhân** | Team Member |
| **Mô tả chức năng** | Cho phép Team Member thực hiện chức năng "Calendar công việc" thuộc nhóm Bảng làm việc (Board) trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Calendar view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Team Member] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Calendar công việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Calendar công việc» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Calendar công việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Team Member khởi tạo thao tác «Calendar công việc» trong nhóm Bảng làm việc (Board).<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Calendar view).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Calendar công việc».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Calendar công việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 16. Đặc tả Use Case "Workload theo người"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_016 |
| **Tên Use Case** | Workload theo người |
| **Tác nhân** | Team Member |
| **Mô tả chức năng** | Cho phép Team Member thực hiện chức năng "Workload theo người" thuộc nhóm Bảng làm việc (Board) trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Workload distribution |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Team Member] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Workload theo người» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Workload theo người» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Workload theo người» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Team Member khởi tạo thao tác «Workload theo người» trong nhóm Bảng làm việc (Board).<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Workload distribution).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Workload theo người».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Workload theo người» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

### 7.4. Ticket nội bộ / helpdesk (`WF-04`)

Nhóm **Ticket nội bộ / helpdesk** gồm **5** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 1 |

**Bảng 17. Đặc tả Use Case "Tạo ticket nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_017 |
| **Tên Use Case** | Tạo ticket nội bộ |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Tạo ticket nội bộ" thuộc nhóm Ticket nội bộ / helpdesk trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Internal helpdesk ticket |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo ticket nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo ticket nội bộ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo ticket nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester mở chức năng «Tạo ticket nội bộ» trong nhóm Ticket nội bộ / helpdesk.<br>2. Hệ thống kiểm tra license `WF`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo ticket nội bộ» (Internal helpdesk ticket).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo ticket nội bộ» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo ticket nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 18. Đặc tả Use Case "Phân loại & định tuyến"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_018 |
| **Tên Use Case** | Phân loại & định tuyến |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Phân loại & định tuyến" thuộc nhóm Ticket nội bộ / helpdesk trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Ticket routing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại & định tuyến» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại & định tuyến» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại & định tuyến» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Phân loại & định tuyến» trong nhóm Ticket nội bộ / helpdesk.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Ticket routing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại & định tuyến».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại & định tuyến» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 19. Đặc tả Use Case "Escalate ticket quá hạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_019 |
| **Tên Use Case** | Escalate ticket quá hạn |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Escalate ticket quá hạn" thuộc nhóm Ticket nội bộ / helpdesk trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Ticket escalation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Escalate ticket quá hạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Escalate ticket quá hạn» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Escalate ticket quá hạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Escalate ticket quá hạn» trong nhóm Ticket nội bộ / helpdesk.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Ticket escalation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Escalate ticket quá hạn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Escalate ticket quá hạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 20. Đặc tả Use Case "CSAT nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_020 |
| **Tên Use Case** | CSAT nội bộ |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "CSAT nội bộ" thuộc nhóm Ticket nội bộ / helpdesk trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Internal satisfaction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «CSAT nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «CSAT nội bộ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «CSAT nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «CSAT nội bộ» trong nhóm Ticket nội bộ / helpdesk.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Internal satisfaction).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «CSAT nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «CSAT nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 21. Đặc tả Use Case "Kiến thức / FAQ nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_021 |
| **Tên Use Case** | Kiến thức / FAQ nội bộ |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Kiến thức / FAQ nội bộ" thuộc nhóm Ticket nội bộ / helpdesk trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Knowledge base |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiến thức / FAQ nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiến thức / FAQ nội bộ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiến thức / FAQ nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Kiến thức / FAQ nội bộ» trong nhóm Ticket nội bộ / helpdesk.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Knowledge base).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiến thức / FAQ nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiến thức / FAQ nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

### 7.5. Thiết kế quy trình phê duyệt (`WF-05`)

Nhóm **Thiết kế quy trình phê duyệt** gồm **6** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 22. Đặc tả Use Case "Tạo mẫu workflow duyệt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_022 |
| **Tên Use Case** | Tạo mẫu workflow duyệt |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Tạo mẫu workflow duyệt" thuộc nhóm Thiết kế quy trình phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Workflow designer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo mẫu workflow duyệt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo mẫu workflow duyệt» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo mẫu workflow duyệt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin mở hộp chờ / chứng từ cần xử lý cho «Tạo mẫu workflow duyệt».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Tạo mẫu workflow duyệt», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo mẫu workflow duyệt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 23. Đặc tả Use Case "Điều kiện duyệt theo quy tắc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_023 |
| **Tên Use Case** | Điều kiện duyệt theo quy tắc |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Điều kiện duyệt theo quy tắc" thuộc nhóm Thiết kế quy trình phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Conditional approval rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điều kiện duyệt theo quy tắc» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điều kiện duyệt theo quy tắc» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điều kiện duyệt theo quy tắc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin mở hộp chờ / chứng từ cần xử lý cho «Điều kiện duyệt theo quy tắc».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Điều kiện duyệt theo quy tắc», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điều kiện duyệt theo quy tắc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 24. Đặc tả Use Case "Nhiều cấp duyệt tuần tự / song song"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_024 |
| **Tên Use Case** | Nhiều cấp duyệt tuần tự / song song |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Nhiều cấp duyệt tuần tự / song song" thuộc nhóm Thiết kế quy trình phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Multi-level approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhiều cấp duyệt tuần tự / song song» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhiều cấp duyệt tuần tự / song song» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhiều cấp duyệt tuần tự / song song» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin mở hộp chờ / chứng từ cần xử lý cho «Nhiều cấp duyệt tuần tự / song song».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Nhiều cấp duyệt tuần tự / song song», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhiều cấp duyệt tuần tự / song song» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 25. Đặc tả Use Case "Gắn workflow vào loại chứng từ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_025 |
| **Tên Use Case** | Gắn workflow vào loại chứng từ |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Gắn workflow vào loại chứng từ" thuộc nhóm Thiết kế quy trình phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Bind workflow to document type |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn workflow vào loại chứng từ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn workflow vào loại chứng từ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn workflow vào loại chứng từ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Gắn workflow vào loại chứng từ» trong nhóm Thiết kế quy trình phê duyệt.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Bind workflow to document type).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn workflow vào loại chứng từ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn workflow vào loại chứng từ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 26. Đặc tả Use Case "Phiên bản quy trình"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_026 |
| **Tên Use Case** | Phiên bản quy trình |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Phiên bản quy trình" thuộc nhóm Thiết kế quy trình phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Workflow versioning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiên bản quy trình» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiên bản quy trình» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiên bản quy trình» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Phiên bản quy trình» trong nhóm Thiết kế quy trình phê duyệt.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Workflow versioning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phiên bản quy trình».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiên bản quy trình» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 27. Đặc tả Use Case "Mô phỏng / kiểm thử"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_027 |
| **Tên Use Case** | Mô phỏng / kiểm thử |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Mô phỏng / kiểm thử" thuộc nhóm Thiết kế quy trình phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Workflow dry-run |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mô phỏng / kiểm thử» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mô phỏng / kiểm thử» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mô phỏng / kiểm thử» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Mô phỏng / kiểm thử» trong nhóm Thiết kế quy trình phê duyệt.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Workflow dry-run).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mô phỏng / kiểm thử».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mô phỏng / kiểm thử» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

### 7.6. Thực thi phê duyệt (`WF-06`)

Nhóm **Thực thi phê duyệt** gồm **8** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 6 |

**Bảng 28. Đặc tả Use Case "Hộp chờ duyệt của tôi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_028 |
| **Tên Use Case** | Hộp chờ duyệt của tôi |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Hộp chờ duyệt của tôi" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: My approval inbox |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hộp chờ duyệt của tôi» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hộp chờ duyệt của tôi» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hộp chờ duyệt của tôi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Hộp chờ duyệt của tôi».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Hộp chờ duyệt của tôi», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hộp chờ duyệt của tôi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 29. Đặc tả Use Case "Duyệt / từ chối / trả bổ sung"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_029 |
| **Tên Use Case** | Duyệt / từ chối / trả bổ sung |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Duyệt / từ chối / trả bổ sung" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Approval actions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt / từ chối / trả bổ sung» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt / từ chối / trả bổ sung» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt / từ chối / trả bổ sung» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Duyệt / từ chối / trả bổ sung».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt / từ chối / trả bổ sung», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt / từ chối / trả bổ sung» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 30. Đặc tả Use Case "Duyệt hàng loạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_030 |
| **Tên Use Case** | Duyệt hàng loạt |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Duyệt hàng loạt" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Bulk approve |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt hàng loạt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt hàng loạt» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt hàng loạt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Duyệt hàng loạt».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt hàng loạt», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt hàng loạt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 31. Đặc tả Use Case "Duyệt trên mobile APP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_031 |
| **Tên Use Case** | Duyệt trên mobile APP |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Duyệt trên mobile APP" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Mobile approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt trên mobile APP» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt trên mobile APP» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt trên mobile APP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Duyệt trên mobile APP».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt trên mobile APP», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt trên mobile APP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 32. Đặc tả Use Case "Ủy quyền duyệt tạm thời"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_032 |
| **Tên Use Case** | Ủy quyền duyệt tạm thời |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Ủy quyền duyệt tạm thời" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Delegation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ủy quyền duyệt tạm thời» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ủy quyền duyệt tạm thời» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ủy quyền duyệt tạm thời» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Ủy quyền duyệt tạm thời».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Ủy quyền duyệt tạm thời», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ủy quyền duyệt tạm thời» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 33. Đặc tả Use Case "Nhắc duyệt / escalate"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_033 |
| **Tên Use Case** | Nhắc duyệt / escalate |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Nhắc duyệt / escalate" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Approval SLA & reminders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhắc duyệt / escalate» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhắc duyệt / escalate» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhắc duyệt / escalate» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Nhắc duyệt / escalate».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Nhắc duyệt / escalate», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhắc duyệt / escalate» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 34. Đặc tả Use Case "Lịch sử duyệt & comment"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_034 |
| **Tên Use Case** | Lịch sử duyệt & comment |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Lịch sử duyệt & comment" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Approval history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử duyệt & comment» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử duyệt & comment» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử duyệt & comment» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Approver mở hộp chờ / chứng từ cần xử lý cho «Lịch sử duyệt & comment».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Lịch sử duyệt & comment», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử duyệt & comment» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 35. Đặc tả Use Case "Thu hồi chứng từ đang chờ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_035 |
| **Tên Use Case** | Thu hồi chứng từ đang chờ |
| **Tác nhân** | Approver |
| **Mô tả chức năng** | Cho phép Approver thực hiện chức năng "Thu hồi chứng từ đang chờ" thuộc nhóm Thực thi phê duyệt trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Recall submission |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Approver] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thu hồi chứng từ đang chờ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thu hồi chứng từ đang chờ» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thu hồi chứng từ đang chờ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Approver khởi tạo thao tác «Thu hồi chứng từ đang chờ» trong nhóm Thực thi phê duyệt.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Recall submission).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thu hồi chứng từ đang chờ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thu hồi chứng từ đang chờ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

### 7.7. Báo cáo quy trình & công việc (`WF-07`)

Nhóm **Báo cáo quy trình & công việc** gồm **5** use case của module `WF`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 2 |

**Bảng 36. Đặc tả Use Case "Thời gian duyệt trung bình"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_036 |
| **Tên Use Case** | Thời gian duyệt trung bình |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Thời gian duyệt trung bình" thuộc nhóm Báo cáo quy trình & công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Approval cycle time |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thời gian duyệt trung bình» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thời gian duyệt trung bình» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thời gian duyệt trung bình» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Process Admin mở hộp chờ / chứng từ cần xử lý cho «Thời gian duyệt trung bình».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Thời gian duyệt trung bình», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thời gian duyệt trung bình» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 37. Đặc tả Use Case "Bottleneck cấp duyệt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_037 |
| **Tên Use Case** | Bottleneck cấp duyệt |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Bottleneck cấp duyệt" thuộc nhóm Báo cáo quy trình & công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Approval bottleneck |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bottleneck cấp duyệt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`, `BR-WF-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bottleneck cấp duyệt» được lưu nhất quán trong module `WF`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bottleneck cấp duyệt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Process Admin mở hộp chờ / chứng từ cần xử lý cho «Bottleneck cấp duyệt».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Bottleneck cấp duyệt», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bottleneck cấp duyệt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 38. Đặc tả Use Case "Khối lượng task mở / quá hạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_038 |
| **Tên Use Case** | Khối lượng task mở / quá hạn |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Khối lượng task mở / quá hạn" thuộc nhóm Báo cáo quy trình & công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Task backlog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khối lượng task mở / quá hạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khối lượng task mở / quá hạn» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khối lượng task mở / quá hạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Khối lượng task mở / quá hạn» trong nhóm Báo cáo quy trình & công việc.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Task backlog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khối lượng task mở / quá hạn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khối lượng task mở / quá hạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 39. Đặc tả Use Case "Năng suất hoàn thành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_039 |
| **Tên Use Case** | Năng suất hoàn thành |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Năng suất hoàn thành" thuộc nhóm Báo cáo quy trình & công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Task completion rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Năng suất hoàn thành» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Năng suất hoàn thành» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Năng suất hoàn thành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Process Admin khởi tạo thao tác «Năng suất hoàn thành» trong nhóm Báo cáo quy trình & công việc.<br>2. Hệ thống kiểm tra license `WF`, quyền RBAC và tiền điều kiện nghiệp vụ (Task completion rate).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Năng suất hoàn thành».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Năng suất hoàn thành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

**Bảng 40. Đặc tả Use Case "Dashboard workflow"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_WF_040 |
| **Tên Use Case** | Dashboard workflow |
| **Tác nhân** | Process Admin |
| **Mô tả chức năng** | Cho phép Process Admin thực hiện chức năng "Dashboard workflow" thuộc nhóm Báo cáo quy trình & công việc trong module WF — Công việc & Phê duyệt. Mô tả chi tiết: Workflow dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Process Admin] và được cấp quyền RBAC tương ứng.<br>• License module `WF` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard workflow» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-WF-SCOPE-01`, `BR-WF-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard workflow» được lưu nhất quán trong module `WF`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard workflow» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Process Admin mở «Dashboard workflow» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Workflow dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard workflow» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin. |

---

## 8. Workflow end-to-end

### WF-WF-01 — Thiết kế và chạy quy trình duyệt

**Mục tiêu:** Chứng từ được duyệt đúng người đúng hạn

| Bước | Mô tả |
|---:|---|
| 1 | Process Admin tạo mẫu workflow và gắn loại chứng từ |
| 2 | Requester trình duyệt từ module nguồn |
| 3 | Approver nhận inbox; duyệt/từ chối/trả bổ sung |
| 4 | Escalation/ủy quyền nếu cần |
| 5 | Kết quả trả về module nguồn; lưu lịch sử |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Task / Ticket` | Công việc |
| `WorkflowDefinition / WorkflowVersion` | Mẫu quy trình |
| `ApprovalInstance / ApprovalStep` | Phiên duyệt |
| `DelegationRule` | Ủy quyền |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `WF`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-WF-01: Không bỏ qua bước duyệt bắt buộc trừ force-approve có quyền đặc biệt + audit.
- BR-WF-02: Ủy quyền có thời hạn; hết hạn tự hết hiệu lực.
- BR-WF-03: Từ chối phải có lý do.
- BR-WF-04: Module nguồn không được coi là approved nếu instance WF chưa completed.
- BR-WF-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-WF-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-WF-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Realtime inbox | Thông báo duyệt đẩy về app/web |
| Audit | Lưu đầy đủ lịch sử bước duyệt |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `WF` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `wf.task.manage` | Quyền chức năng module |
| `wf.workflow.design` | Quyền chức năng module |
| `wf.approve` | Quyền chức năng module |
| `wf.delegate.manage` | Quyền chức năng module |
| `wf.report.view` | Quyền chức năng module |
| `wf.*.view` | Xem trong data scope |
| `wf.*.manage` | Tạo/sửa trong data scope |
| `wf.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Avg approval cycle time | Theo dõi vận hành module |
| Overdue approvals | Theo dõi vận hành module |
| Task backlog | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Các module đăng ký document type + callback khi cài đặt.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có hỗ trợ duyệt song song nhiều nhánh ngay phase 1?

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
| Bản SRS này | `SRS_WF_v1.1.md` / `.docx` |
| UC IDs | `UC_WF_001` … |

---

*Hết tài liệu SRS-WF-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
