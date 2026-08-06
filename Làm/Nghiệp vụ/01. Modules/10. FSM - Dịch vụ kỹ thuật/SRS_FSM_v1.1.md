# SRS-FSM-v1.1 — Dịch vụ kỹ thuật hiện trường (Field Service)

> **Software Requirements Specification — Module FSM**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-FSM-v1.1` |
| Module | `FSM` — Dịch vụ kỹ thuật hiện trường (Field Service) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Sản xuất & Dịch vụ |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `CRM`, `INV`, `FIN`, `PRT` |
| Số nhóm / UC | 9 nhóm / 50 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Dịch vụ kỹ thuật hiện trường (Field Service)** (`FSM`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Quản lý install base, ticket bảo hành/sửa chữa/bảo trì, phân công KTV, linh kiện, SLA, nghiệm thu và báo cáo dịch vụ.

### 1.3. Mục tiêu đo được
1. Chuẩn hóa tiếp nhận và xử lý ticket theo SLA.
2. Kiểm soát linh kiện mang đi/mang về.
3. Lưu lịch sử dịch vụ theo thiết bị khách.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Service catalog, install base, ticket lifecycle, scheduling, onsite work, parts, PM, technician app, FSM reports.

### 2.2. Out of Scope
- CRM bán hàng đầy đủ.
- Kho tổng WMS (INV).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `CRM`, `INV`, `FIN`, `PRT`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Dispatcher | Tiếp nhận & phân công |
| Technician | Xử lý hiện trường |
| Service Manager | SLA & năng suất |
| Parts Storekeeper | Cấp linh kiện |
| Customer (via PRT) | Tạo/theo dõi ticket |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Install base | Thiết bị đang vận hành tại khách |
| SLA | Cam kết thời gian phản hồi/xử lý |
| FTF | First Time Fix |
| PM | Preventive Maintenance |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- FSM (Dịch vụ kỹ thuật hiện trường (Field Service))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `FSM` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `FSM` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM | KH & escalate |
| Tích hợp | INV | Linh kiện |
| Tích hợp | FIN | Phí dịch vụ |
| Tích hợp | PRT | Self-service ticket |
| Tích hợp | PJM | Bảo hành sau bàn giao dự án |

---

## 6. Catalog chức năng

**Tổng:** 9 nhóm · 50 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `FSM-01` | Danh mục kỹ thuật | 7 | 4 | 3 | 0 |
| 2 | `FSM-02` | Install base – thiết bị tại khách | 5 | 3 | 2 | 0 |
| 3 | `FSM-03` | Tiếp nhận & phân công ticket | 7 | 6 | 1 | 0 |
| 4 | `FSM-04` | Thực hiện hiện trường | 8 | 3 | 5 | 0 |
| 5 | `FSM-05` | Nghiệm thu & đóng ticket | 5 | 2 | 2 | 1 |
| 6 | `FSM-06` | Bảo trì định kỳ | 4 | 0 | 4 | 0 |
| 7 | `FSM-07` | Kho linh kiện kỹ thuật | 4 | 3 | 1 | 0 |
| 8 | `FSM-08` | APP kỹ thuật viên | 4 | 2 | 1 | 1 |
| 9 | `FSM-09` | Báo cáo FSM | 6 | 4 | 2 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_FSM_001` | Danh mục kỹ thuật | Danh mục loại dịch vụ | Must |
| `UC_FSM_002` | Danh mục kỹ thuật | Danh mục mã lỗi | Must |
| `UC_FSM_003` | Danh mục kỹ thuật | Danh mục linh kiện | Must |
| `UC_FSM_004` | Danh mục kỹ thuật | Bảng giá dịch vụ | Should |
| `UC_FSM_005` | Danh mục kỹ thuật | Cấu hình SLA | Must |
| `UC_FSM_006` | Danh mục kỹ thuật | Kỹ năng / chứng chỉ kỹ thuật viên | Should |
| `UC_FSM_007` | Danh mục kỹ thuật | Vùng phụ trách | Should |
| `UC_FSM_008` | Install base – thiết bị tại khách | Hồ sơ thiết bị đã bán | Must |
| `UC_FSM_009` | Install base – thiết bị tại khách | Serial / model / ngày kích hoạt BH | Must |
| `UC_FSM_010` | Install base – thiết bị tại khách | Lịch sử bảo hành / sửa chữa | Must |
| `UC_FSM_011` | Install base – thiết bị tại khách | Cảnh báo hết hạn bảo hành | Should |
| `UC_FSM_012` | Install base – thiết bị tại khách | Hợp đồng bảo trì định kỳ | Should |
| `UC_FSM_013` | Tiếp nhận & phân công ticket | Tạo ticket từ kênh | Must |
| `UC_FSM_014` | Tiếp nhận & phân công ticket | Phân loại mức ưu tiên | Must |
| `UC_FSM_015` | Tiếp nhận & phân công ticket | Phân công kỹ thuật viên thủ công | Must |
| `UC_FSM_016` | Tiếp nhận & phân công ticket | Phân công theo rule | Should |
| `UC_FSM_017` | Tiếp nhận & phân công ticket | Đổi kỹ thuật viên / escalate | Must |
| `UC_FSM_018` | Tiếp nhận & phân công ticket | Lịch hẹn với khách | Must |
| `UC_FSM_019` | Tiếp nhận & phân công ticket | Xác nhận lịch trên APP | Must |
| `UC_FSM_020` | Thực hiện hiện trường | Check-in hiện trường GPS | Should |
| `UC_FSM_021` | Thực hiện hiện trường | Checklist công việc | Should |
| `UC_FSM_022` | Thực hiện hiện trường | Ghi nhận nguyên nhân & xử lý | Must |
| `UC_FSM_023` | Thực hiện hiện trường | Chụp ảnh trước/sau | Should |
| `UC_FSM_024` | Thực hiện hiện trường | Xuất linh kiện theo ticket | Must |
| `UC_FSM_025` | Thực hiện hiện trường | Hoàn linh kiện thừa | Should |
| `UC_FSM_026` | Thực hiện hiện trường | Ghi nhận phí sửa chữa | Should |
| `UC_FSM_027` | Thực hiện hiện trường | Check-out / hoàn thành | Must |
| `UC_FSM_028` | Nghiệm thu & đóng ticket | Khách ký nghiệm thu | Must |
| `UC_FSM_029` | Nghiệm thu & đóng ticket | Đánh giá dịch vụ | Could |
| `UC_FSM_030` | Nghiệm thu & đóng ticket | Đóng ticket đạt/trễ SLA | Must |
| `UC_FSM_031` | Nghiệm thu & đóng ticket | Tái mở ticket | Should |
| `UC_FSM_032` | Nghiệm thu & đóng ticket | Chuyển chi phí sang FIN | Should |
| `UC_FSM_033` | Bảo trì định kỳ | Lịch bảo trì theo thiết bị | Should |
| `UC_FSM_034` | Bảo trì định kỳ | Tự tạo ticket bảo trì đến hạn | Should |
| `UC_FSM_035` | Bảo trì định kỳ | Checklist bảo trì chuẩn | Should |
| `UC_FSM_036` | Bảo trì định kỳ | Báo cáo thực hiện bảo trì | Should |
| `UC_FSM_037` | Kho linh kiện kỹ thuật | Tồn linh kiện tại kho KT | Must |
| `UC_FSM_038` | Kho linh kiện kỹ thuật | Cấp linh kiện cho KTV | Must |
| `UC_FSM_039` | Kho linh kiện kỹ thuật | Đối soát linh kiện | Must |
| `UC_FSM_040` | Kho linh kiện kỹ thuật | Cảnh báo thất thoát | Should |
| `UC_FSM_041` | APP kỹ thuật viên | Danh sách việc hôm nay | Must |
| `UC_FSM_042` | APP kỹ thuật viên | Điều hướng / thông tin khách | Must |
| `UC_FSM_043` | APP kỹ thuật viên | Làm việc offline | Later |
| `UC_FSM_044` | APP kỹ thuật viên | Nộp quyết toán ngày | Should |
| `UC_FSM_045` | Báo cáo FSM | SLA compliance realtime | Must |
| `UC_FSM_046` | Báo cáo FSM | Năng suất kỹ thuật viên | Must |
| `UC_FSM_047` | Báo cáo FSM | Chi phí linh kiện | Must |
| `UC_FSM_048` | Báo cáo FSM | Tỷ lệ sửa lần đầu | Should |
| `UC_FSM_049` | Báo cáo FSM | Báo cáo bảo hành | Should |
| `UC_FSM_050` | Báo cáo FSM | Xuất báo cáo kỹ thuật | Must |

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

### 7.1. Danh mục kỹ thuật (`FSM-01`)

Nhóm **Danh mục kỹ thuật** gồm **7** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 4 |

**Bảng 1. Đặc tả Use Case "Danh mục loại dịch vụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_001 |
| **Tên Use Case** | Danh mục loại dịch vụ |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Danh mục loại dịch vụ" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service type master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục loại dịch vụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục loại dịch vụ» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục loại dịch vụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Danh mục loại dịch vụ» trong nhóm Danh mục kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Service type master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục loại dịch vụ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục loại dịch vụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Danh mục mã lỗi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_002 |
| **Tên Use Case** | Danh mục mã lỗi |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Danh mục mã lỗi" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Fault code catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục mã lỗi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục mã lỗi» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục mã lỗi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Danh mục mã lỗi» trong nhóm Danh mục kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Fault code catalog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục mã lỗi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục mã lỗi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Danh mục linh kiện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_003 |
| **Tên Use Case** | Danh mục linh kiện |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Danh mục linh kiện" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Spare parts catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục linh kiện» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục linh kiện» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục linh kiện» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Danh mục linh kiện» trong nhóm Danh mục kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Spare parts catalog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục linh kiện».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục linh kiện» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Bảng giá dịch vụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_004 |
| **Tên Use Case** | Bảng giá dịch vụ |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Bảng giá dịch vụ" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service pricing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng giá dịch vụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng giá dịch vụ» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng giá dịch vụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Bảng giá dịch vụ» trong nhóm Danh mục kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Service pricing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng giá dịch vụ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng giá dịch vụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 5. Đặc tả Use Case "Cấu hình SLA"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_005 |
| **Tên Use Case** | Cấu hình SLA |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Cấu hình SLA" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: SLA matrix |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình SLA» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình SLA» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình SLA» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager mở màn hình cấu hình «Cấu hình SLA» trong Danh mục kỹ thuật.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (SLA matrix) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình SLA» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 6. Đặc tả Use Case "Kỹ năng / chứng chỉ kỹ thuật viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_006 |
| **Tên Use Case** | Kỹ năng / chứng chỉ kỹ thuật viên |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Kỹ năng / chứng chỉ kỹ thuật viên" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Technician skill matrix |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kỹ năng / chứng chỉ kỹ thuật viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kỹ năng / chứng chỉ kỹ thuật viên» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kỹ năng / chứng chỉ kỹ thuật viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Kỹ năng / chứng chỉ kỹ thuật viên» trong nhóm Danh mục kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Technician skill matrix).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kỹ năng / chứng chỉ kỹ thuật viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kỹ năng / chứng chỉ kỹ thuật viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 7. Đặc tả Use Case "Vùng phụ trách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_007 |
| **Tên Use Case** | Vùng phụ trách |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Vùng phụ trách" thuộc nhóm Danh mục kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service territory |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Vùng phụ trách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Vùng phụ trách» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Vùng phụ trách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Vùng phụ trách» trong nhóm Danh mục kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Service territory).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Vùng phụ trách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Vùng phụ trách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Install base – thiết bị tại khách (`FSM-02`)

Nhóm **Install base – thiết bị tại khách** gồm **5** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 8. Đặc tả Use Case "Hồ sơ thiết bị đã bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_008 |
| **Tên Use Case** | Hồ sơ thiết bị đã bán |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Hồ sơ thiết bị đã bán" thuộc nhóm Install base – thiết bị tại khách trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Installed equipment base |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hồ sơ thiết bị đã bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hồ sơ thiết bị đã bán» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hồ sơ thiết bị đã bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Hồ sơ thiết bị đã bán» trong nhóm Install base – thiết bị tại khách.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Installed equipment base).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hồ sơ thiết bị đã bán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hồ sơ thiết bị đã bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Serial / model / ngày kích hoạt BH"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_009 |
| **Tên Use Case** | Serial / model / ngày kích hoạt BH |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Serial / model / ngày kích hoạt BH" thuộc nhóm Install base – thiết bị tại khách trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Warranty tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Serial / model / ngày kích hoạt BH» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Serial / model / ngày kích hoạt BH» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Serial / model / ngày kích hoạt BH» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Serial / model / ngày kích hoạt BH» trong nhóm Install base – thiết bị tại khách.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Warranty tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Serial / model / ngày kích hoạt BH».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Serial / model / ngày kích hoạt BH» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Lịch sử bảo hành / sửa chữa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_010 |
| **Tên Use Case** | Lịch sử bảo hành / sửa chữa |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Lịch sử bảo hành / sửa chữa" thuộc nhóm Install base – thiết bị tại khách trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử bảo hành / sửa chữa» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử bảo hành / sửa chữa» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử bảo hành / sửa chữa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager mở «Lịch sử bảo hành / sửa chữa» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Service history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử bảo hành / sửa chữa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "Cảnh báo hết hạn bảo hành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_011 |
| **Tên Use Case** | Cảnh báo hết hạn bảo hành |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Cảnh báo hết hạn bảo hành" thuộc nhóm Install base – thiết bị tại khách trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Warranty expiry alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo hết hạn bảo hành» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo hết hạn bảo hành» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo hết hạn bảo hành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Service Manager kích hoạt kiểm tra điều kiện «Cảnh báo hết hạn bảo hành».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Warranty expiry alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo hết hạn bảo hành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Hợp đồng bảo trì định kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_012 |
| **Tên Use Case** | Hợp đồng bảo trì định kỳ |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Hợp đồng bảo trì định kỳ" thuộc nhóm Install base – thiết bị tại khách trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Maintenance contract |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hợp đồng bảo trì định kỳ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hợp đồng bảo trì định kỳ» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hợp đồng bảo trì định kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Hợp đồng bảo trì định kỳ» trong nhóm Install base – thiết bị tại khách.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Maintenance contract).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hợp đồng bảo trì định kỳ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hợp đồng bảo trì định kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Tiếp nhận & phân công ticket (`FSM-03`)

Nhóm **Tiếp nhận & phân công ticket** gồm **7** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 6 |

**Bảng 13. Đặc tả Use Case "Tạo ticket từ kênh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_013 |
| **Tên Use Case** | Tạo ticket từ kênh |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Tạo ticket từ kênh" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service ticket creation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo ticket từ kênh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo ticket từ kênh» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo ticket từ kênh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Dispatcher mở chức năng «Tạo ticket từ kênh» trong nhóm Tiếp nhận & phân công ticket.<br>2. Hệ thống kiểm tra license `FSM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo ticket từ kênh» (Service ticket creation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo ticket từ kênh» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo ticket từ kênh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 14. Đặc tả Use Case "Phân loại mức ưu tiên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_014 |
| **Tên Use Case** | Phân loại mức ưu tiên |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Phân loại mức ưu tiên" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Priority classification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại mức ưu tiên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại mức ưu tiên» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại mức ưu tiên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Dispatcher khởi tạo thao tác «Phân loại mức ưu tiên» trong nhóm Tiếp nhận & phân công ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Priority classification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại mức ưu tiên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại mức ưu tiên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Phân công kỹ thuật viên thủ công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_015 |
| **Tên Use Case** | Phân công kỹ thuật viên thủ công |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Phân công kỹ thuật viên thủ công" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Manual technician assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân công kỹ thuật viên thủ công» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân công kỹ thuật viên thủ công» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân công kỹ thuật viên thủ công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Dispatcher chọn đối tượng nguồn trong «Phân công kỹ thuật viên thủ công».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân công kỹ thuật viên thủ công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Phân công theo rule"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_016 |
| **Tên Use Case** | Phân công theo rule |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Phân công theo rule" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Auto assignment by rule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân công theo rule» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân công theo rule» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân công theo rule» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Dispatcher chọn đối tượng nguồn trong «Phân công theo rule».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân công theo rule» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Đổi kỹ thuật viên / escalate"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_017 |
| **Tên Use Case** | Đổi kỹ thuật viên / escalate |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Đổi kỹ thuật viên / escalate" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Reassignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đổi kỹ thuật viên / escalate» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đổi kỹ thuật viên / escalate» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đổi kỹ thuật viên / escalate» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Dispatcher khởi tạo thao tác «Đổi kỹ thuật viên / escalate» trong nhóm Tiếp nhận & phân công ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Reassignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đổi kỹ thuật viên / escalate».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đổi kỹ thuật viên / escalate» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "Lịch hẹn với khách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_018 |
| **Tên Use Case** | Lịch hẹn với khách |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Lịch hẹn với khách" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Appointment scheduling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch hẹn với khách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch hẹn với khách» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch hẹn với khách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Dispatcher khởi tạo thao tác «Lịch hẹn với khách» trong nhóm Tiếp nhận & phân công ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Appointment scheduling).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lịch hẹn với khách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch hẹn với khách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 19. Đặc tả Use Case "Xác nhận lịch trên APP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_019 |
| **Tên Use Case** | Xác nhận lịch trên APP |
| **Tác nhân** | Dispatcher |
| **Mô tả chức năng** | Cho phép Dispatcher thực hiện chức năng "Xác nhận lịch trên APP" thuộc nhóm Tiếp nhận & phân công ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Accept job on mobile |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Dispatcher] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận lịch trên APP» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận lịch trên APP» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận lịch trên APP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Dispatcher khởi tạo thao tác «Xác nhận lịch trên APP» trong nhóm Tiếp nhận & phân công ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Accept job on mobile).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận lịch trên APP».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận lịch trên APP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Thực hiện hiện trường (`FSM-04`)

Nhóm **Thực hiện hiện trường** gồm **8** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 3 |

**Bảng 20. Đặc tả Use Case "Check-in hiện trường GPS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_020 |
| **Tên Use Case** | Check-in hiện trường GPS |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Check-in hiện trường GPS" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Onsite check-in |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Check-in hiện trường GPS» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`, `BR-FSM-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Check-in hiện trường GPS» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Check-in hiện trường GPS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician mở chứng từ nhận liên quan «Check-in hiện trường GPS».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Check-in hiện trường GPS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý. |

**Bảng 21. Đặc tả Use Case "Checklist công việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_021 |
| **Tên Use Case** | Checklist công việc |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Checklist công việc" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist công việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist công việc» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist công việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Checklist công việc» trong nhóm Thực hiện hiện trường.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Service checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist công việc».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist công việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Ghi nhận nguyên nhân & xử lý"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_022 |
| **Tên Use Case** | Ghi nhận nguyên nhân & xử lý |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Ghi nhận nguyên nhân & xử lý" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Work log & resolution |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận nguyên nhân & xử lý» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận nguyên nhân & xử lý» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận nguyên nhân & xử lý» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Ghi nhận nguyên nhân & xử lý» trong nhóm Thực hiện hiện trường.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Work log & resolution).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận nguyên nhân & xử lý».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận nguyên nhân & xử lý» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Chụp ảnh trước/sau"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_023 |
| **Tên Use Case** | Chụp ảnh trước/sau |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Chụp ảnh trước/sau" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Photo documentation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chụp ảnh trước/sau» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chụp ảnh trước/sau» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chụp ảnh trước/sau» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Chụp ảnh trước/sau» trong nhóm Thực hiện hiện trường.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Photo documentation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chụp ảnh trước/sau».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chụp ảnh trước/sau» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Xuất linh kiện theo ticket"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_024 |
| **Tên Use Case** | Xuất linh kiện theo ticket |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Xuất linh kiện theo ticket" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Parts consumption |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất linh kiện theo ticket» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất linh kiện theo ticket» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất linh kiện theo ticket» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician mở «Xuất linh kiện theo ticket», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất linh kiện theo ticket» (Parts consumption).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất linh kiện theo ticket» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Hoàn linh kiện thừa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_025 |
| **Tên Use Case** | Hoàn linh kiện thừa |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Hoàn linh kiện thừa" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Return unused parts |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hoàn linh kiện thừa» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hoàn linh kiện thừa» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hoàn linh kiện thừa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Hoàn linh kiện thừa» trong nhóm Thực hiện hiện trường.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Return unused parts).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hoàn linh kiện thừa».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hoàn linh kiện thừa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Ghi nhận phí sửa chữa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_026 |
| **Tên Use Case** | Ghi nhận phí sửa chữa |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Ghi nhận phí sửa chữa" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Chargeable service |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận phí sửa chữa» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận phí sửa chữa» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận phí sửa chữa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Ghi nhận phí sửa chữa» trong nhóm Thực hiện hiện trường.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Chargeable service).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận phí sửa chữa».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận phí sửa chữa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 27. Đặc tả Use Case "Check-out / hoàn thành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_027 |
| **Tên Use Case** | Check-out / hoàn thành |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Check-out / hoàn thành" thuộc nhóm Thực hiện hiện trường trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Complete job |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Check-out / hoàn thành» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Check-out / hoàn thành» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Check-out / hoàn thành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Check-out / hoàn thành» trong nhóm Thực hiện hiện trường.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Complete job).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Check-out / hoàn thành».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Check-out / hoàn thành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Nghiệm thu & đóng ticket (`FSM-05`)

Nhóm **Nghiệm thu & đóng ticket** gồm **5** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 2 |

**Bảng 28. Đặc tả Use Case "Khách ký nghiệm thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_028 |
| **Tên Use Case** | Khách ký nghiệm thu |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Khách ký nghiệm thu" thuộc nhóm Nghiệm thu & đóng ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Customer sign-off |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khách ký nghiệm thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khách ký nghiệm thu» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khách ký nghiệm thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Khách ký nghiệm thu» trong nhóm Nghiệm thu & đóng ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer sign-off).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khách ký nghiệm thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khách ký nghiệm thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Đánh giá dịch vụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_029 |
| **Tên Use Case** | Đánh giá dịch vụ |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Đánh giá dịch vụ" thuộc nhóm Nghiệm thu & đóng ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Service rating |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá dịch vụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá dịch vụ» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá dịch vụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Đánh giá dịch vụ» trong nhóm Nghiệm thu & đóng ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Service rating).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá dịch vụ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá dịch vụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Đóng ticket đạt/trễ SLA"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_030 |
| **Tên Use Case** | Đóng ticket đạt/trễ SLA |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Đóng ticket đạt/trễ SLA" thuộc nhóm Nghiệm thu & đóng ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Close ticket with SLA flag |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng ticket đạt/trễ SLA» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng ticket đạt/trễ SLA» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng ticket đạt/trễ SLA» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Đóng ticket đạt/trễ SLA» trong nhóm Nghiệm thu & đóng ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Close ticket with SLA flag).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng ticket đạt/trễ SLA».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng ticket đạt/trễ SLA» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 31. Đặc tả Use Case "Tái mở ticket"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_031 |
| **Tên Use Case** | Tái mở ticket |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Tái mở ticket" thuộc nhóm Nghiệm thu & đóng ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Reopen ticket |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tái mở ticket» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tái mở ticket» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tái mở ticket» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Tái mở ticket» trong nhóm Nghiệm thu & đóng ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Reopen ticket).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tái mở ticket».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tái mở ticket» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 32. Đặc tả Use Case "Chuyển chi phí sang FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_032 |
| **Tên Use Case** | Chuyển chi phí sang FIN |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Chuyển chi phí sang FIN" thuộc nhóm Nghiệm thu & đóng ticket trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Post service costs |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển chi phí sang FIN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển chi phí sang FIN» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển chi phí sang FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Chuyển chi phí sang FIN» trong nhóm Nghiệm thu & đóng ticket.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Post service costs).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển chi phí sang FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển chi phí sang FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Bảo trì định kỳ (`FSM-06`)

Nhóm **Bảo trì định kỳ** gồm **4** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 33. Đặc tả Use Case "Lịch bảo trì theo thiết bị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_033 |
| **Tên Use Case** | Lịch bảo trì theo thiết bị |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Lịch bảo trì theo thiết bị" thuộc nhóm Bảo trì định kỳ trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: PM schedule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch bảo trì theo thiết bị» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch bảo trì theo thiết bị» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch bảo trì theo thiết bị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Lịch bảo trì theo thiết bị» trong nhóm Bảo trì định kỳ.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (PM schedule).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lịch bảo trì theo thiết bị».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch bảo trì theo thiết bị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 34. Đặc tả Use Case "Tự tạo ticket bảo trì đến hạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_034 |
| **Tên Use Case** | Tự tạo ticket bảo trì đến hạn |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Tự tạo ticket bảo trì đến hạn" thuộc nhóm Bảo trì định kỳ trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Auto PM ticket generation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tự tạo ticket bảo trì đến hạn» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tự tạo ticket bảo trì đến hạn» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tự tạo ticket bảo trì đến hạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager mở chức năng «Tự tạo ticket bảo trì đến hạn» trong nhóm Bảo trì định kỳ.<br>2. Hệ thống kiểm tra license `FSM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tự tạo ticket bảo trì đến hạn» (Auto PM ticket generation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tự tạo ticket bảo trì đến hạn» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tự tạo ticket bảo trì đến hạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 35. Đặc tả Use Case "Checklist bảo trì chuẩn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_035 |
| **Tên Use Case** | Checklist bảo trì chuẩn |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Checklist bảo trì chuẩn" thuộc nhóm Bảo trì định kỳ trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: PM checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist bảo trì chuẩn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist bảo trì chuẩn» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist bảo trì chuẩn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Checklist bảo trì chuẩn» trong nhóm Bảo trì định kỳ.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (PM checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist bảo trì chuẩn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist bảo trì chuẩn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Báo cáo thực hiện bảo trì"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_036 |
| **Tên Use Case** | Báo cáo thực hiện bảo trì |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Báo cáo thực hiện bảo trì" thuộc nhóm Bảo trì định kỳ trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: PM compliance report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo thực hiện bảo trì» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo thực hiện bảo trì» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo thực hiện bảo trì» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager mở «Báo cáo thực hiện bảo trì» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (PM compliance report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo thực hiện bảo trì» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Kho linh kiện kỹ thuật (`FSM-07`)

Nhóm **Kho linh kiện kỹ thuật** gồm **4** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 3 |

**Bảng 37. Đặc tả Use Case "Tồn linh kiện tại kho KT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_037 |
| **Tên Use Case** | Tồn linh kiện tại kho KT |
| **Tác nhân** | Parts Storekeeper |
| **Mô tả chức năng** | Cho phép Parts Storekeeper thực hiện chức năng "Tồn linh kiện tại kho KT" thuộc nhóm Kho linh kiện kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Parts inventory view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Parts Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tồn linh kiện tại kho KT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tồn linh kiện tại kho KT» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tồn linh kiện tại kho KT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Parts Storekeeper khởi tạo thao tác «Tồn linh kiện tại kho KT» trong nhóm Kho linh kiện kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Parts inventory view).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tồn linh kiện tại kho KT».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tồn linh kiện tại kho KT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 38. Đặc tả Use Case "Cấp linh kiện cho KTV"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_038 |
| **Tên Use Case** | Cấp linh kiện cho KTV |
| **Tác nhân** | Parts Storekeeper |
| **Mô tả chức năng** | Cho phép Parts Storekeeper thực hiện chức năng "Cấp linh kiện cho KTV" thuộc nhóm Kho linh kiện kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Issue parts to technician |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Parts Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấp linh kiện cho KTV» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấp linh kiện cho KTV» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấp linh kiện cho KTV» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Parts Storekeeper khởi tạo thao tác «Cấp linh kiện cho KTV» trong nhóm Kho linh kiện kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Issue parts to technician).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Cấp linh kiện cho KTV».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấp linh kiện cho KTV» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 39. Đặc tả Use Case "Đối soát linh kiện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_039 |
| **Tên Use Case** | Đối soát linh kiện |
| **Tác nhân** | Parts Storekeeper |
| **Mô tả chức năng** | Cho phép Parts Storekeeper thực hiện chức năng "Đối soát linh kiện" thuộc nhóm Kho linh kiện kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Parts reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Parts Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát linh kiện» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát linh kiện» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát linh kiện» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Parts Storekeeper khởi tạo thao tác «Đối soát linh kiện» trong nhóm Kho linh kiện kỹ thuật.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Parts reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát linh kiện».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát linh kiện» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 40. Đặc tả Use Case "Cảnh báo thất thoát"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_040 |
| **Tên Use Case** | Cảnh báo thất thoát |
| **Tác nhân** | Parts Storekeeper |
| **Mô tả chức năng** | Cho phép Parts Storekeeper thực hiện chức năng "Cảnh báo thất thoát" thuộc nhóm Kho linh kiện kỹ thuật trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Parts loss alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Parts Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo thất thoát» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo thất thoát» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo thất thoát» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Parts Storekeeper kích hoạt kiểm tra điều kiện «Cảnh báo thất thoát».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Parts loss alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo thất thoát» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.8. APP kỹ thuật viên (`FSM-08`)

Nhóm **APP kỹ thuật viên** gồm **4** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 2 |

**Bảng 41. Đặc tả Use Case "Danh sách việc hôm nay"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_041 |
| **Tên Use Case** | Danh sách việc hôm nay |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Danh sách việc hôm nay" thuộc nhóm APP kỹ thuật viên trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: My jobs list |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh sách việc hôm nay» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh sách việc hôm nay» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh sách việc hôm nay» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Danh sách việc hôm nay» trong nhóm APP kỹ thuật viên.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (My jobs list).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh sách việc hôm nay».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh sách việc hôm nay» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 42. Đặc tả Use Case "Điều hướng / thông tin khách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_042 |
| **Tên Use Case** | Điều hướng / thông tin khách |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Điều hướng / thông tin khách" thuộc nhóm APP kỹ thuật viên trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Job detail & navigation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điều hướng / thông tin khách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điều hướng / thông tin khách» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điều hướng / thông tin khách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Điều hướng / thông tin khách» trong nhóm APP kỹ thuật viên.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Job detail & navigation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Điều hướng / thông tin khách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điều hướng / thông tin khách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 43. Đặc tả Use Case "Làm việc offline"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_043 |
| **Tên Use Case** | Làm việc offline |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Làm việc offline" thuộc nhóm APP kỹ thuật viên trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Offline mode |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Làm việc offline» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Làm việc offline» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Làm việc offline» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Làm việc offline» trong nhóm APP kỹ thuật viên.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Offline mode).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Làm việc offline».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Làm việc offline» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 44. Đặc tả Use Case "Nộp quyết toán ngày"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_044 |
| **Tên Use Case** | Nộp quyết toán ngày |
| **Tác nhân** | Technician |
| **Mô tả chức năng** | Cho phép Technician thực hiện chức năng "Nộp quyết toán ngày" thuộc nhóm APP kỹ thuật viên trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Daily closeout |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Technician] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nộp quyết toán ngày» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nộp quyết toán ngày» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nộp quyết toán ngày» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Technician khởi tạo thao tác «Nộp quyết toán ngày» trong nhóm APP kỹ thuật viên.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Daily closeout).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nộp quyết toán ngày».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nộp quyết toán ngày» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.9. Báo cáo FSM (`FSM-09`)

Nhóm **Báo cáo FSM** gồm **6** use case của module `FSM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 45. Đặc tả Use Case "SLA compliance realtime"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_045 |
| **Tên Use Case** | SLA compliance realtime |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "SLA compliance realtime" thuộc nhóm Báo cáo FSM trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: SLA dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «SLA compliance realtime» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «SLA compliance realtime» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «SLA compliance realtime» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «SLA compliance realtime» trong nhóm Báo cáo FSM.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (SLA dashboard).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «SLA compliance realtime».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «SLA compliance realtime» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 46. Đặc tả Use Case "Năng suất kỹ thuật viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_046 |
| **Tên Use Case** | Năng suất kỹ thuật viên |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Năng suất kỹ thuật viên" thuộc nhóm Báo cáo FSM trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Technician productivity |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Năng suất kỹ thuật viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Năng suất kỹ thuật viên» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Năng suất kỹ thuật viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Năng suất kỹ thuật viên» trong nhóm Báo cáo FSM.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Technician productivity).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Năng suất kỹ thuật viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Năng suất kỹ thuật viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 47. Đặc tả Use Case "Chi phí linh kiện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_047 |
| **Tên Use Case** | Chi phí linh kiện |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Chi phí linh kiện" thuộc nhóm Báo cáo FSM trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Parts cost by ticket |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chi phí linh kiện» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chi phí linh kiện» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chi phí linh kiện» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Chi phí linh kiện» trong nhóm Báo cáo FSM.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (Parts cost by ticket).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chi phí linh kiện».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chi phí linh kiện» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 48. Đặc tả Use Case "Tỷ lệ sửa lần đầu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_048 |
| **Tên Use Case** | Tỷ lệ sửa lần đầu |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Tỷ lệ sửa lần đầu" thuộc nhóm Báo cáo FSM trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: First time fix rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tỷ lệ sửa lần đầu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tỷ lệ sửa lần đầu» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tỷ lệ sửa lần đầu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager khởi tạo thao tác «Tỷ lệ sửa lần đầu» trong nhóm Báo cáo FSM.<br>2. Hệ thống kiểm tra license `FSM`, quyền RBAC và tiền điều kiện nghiệp vụ (First time fix rate).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tỷ lệ sửa lần đầu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tỷ lệ sửa lần đầu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 49. Đặc tả Use Case "Báo cáo bảo hành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_049 |
| **Tên Use Case** | Báo cáo bảo hành |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Báo cáo bảo hành" thuộc nhóm Báo cáo FSM trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Warranty split report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo bảo hành» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo bảo hành» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo bảo hành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Service Manager mở «Báo cáo bảo hành» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Warranty split report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo bảo hành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 50. Đặc tả Use Case "Xuất báo cáo kỹ thuật"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FSM_050 |
| **Tên Use Case** | Xuất báo cáo kỹ thuật |
| **Tác nhân** | Service Manager |
| **Mô tả chức năng** | Cho phép Service Manager thực hiện chức năng "Xuất báo cáo kỹ thuật" thuộc nhóm Báo cáo FSM trong module FSM — Dịch vụ kỹ thuật hiện trường (Field Service). Mô tả chi tiết: Export service reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Service Manager] và được cấp quyền RBAC tương ứng.<br>• License module `FSM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo kỹ thuật» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FSM-SCOPE-01`, `BR-FSM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo kỹ thuật» được lưu nhất quán trong module `FSM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo kỹ thuật» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Service Manager mở «Xuất báo cáo kỹ thuật», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo kỹ thuật» (Export service reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo kỹ thuật» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-FSM-01 — Xử lý ticket hiện trường

**Mục tiêu:** Đóng ticket có nghiệm thu và đối soát linh kiện

| Bước | Mô tả |
|---:|---|
| 1 | Tạo ticket từ hotline/CRM/PRT |
| 2 | Phân công KTV + lịch hẹn |
| 3 | Check-in; xử lý; xuất linh kiện |
| 4 | KH nghiệm thu; đóng ticket |
| 5 | Đối soát linh kiện; ghi phí/BH; post FIN nếu có |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `ServiceType / FaultCode / SlaPolicy` | Danh mục |
| `InstalledAsset` | Thiết bị tại KH |
| `ServiceTicket / Appointment` | Ticket & lịch |
| `TicketPartIssue` | Linh kiện |
| `PmSchedule` | Bảo trì định kỳ |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `FSM`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-FSM-01: Ticket trong hạn BH không tính phí công/linh kiện theo policy.
- BR-FSM-02: Đóng ticket bắt buộc có nghiệm thu (trừ hủy hợp lệ).
- BR-FSM-03: Linh kiện xuất phải gắn ticket.
- BR-FSM-04: Vi phạm SLA phải được gắn cờ và báo cáo.
- BR-FSM-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-FSM-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-FSM-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Mobile tech | APP KTV offline nhẹ tùy gói |
| SLA engine | Tính SLA theo lịch làm việc |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `FSM` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `fsm.ticket.manage` | Quyền chức năng module |
| `fsm.dispatch.assign` | Quyền chức năng module |
| `fsm.parts.issue` | Quyền chức năng module |
| `fsm.ticket.close` | Quyền chức năng module |
| `fsm.pm.manage` | Quyền chức năng module |
| `fsm.report.view` | Quyền chức năng module |
| `fsm.*.view` | Xem trong data scope |
| `fsm.*.manage` | Tạo/sửa trong data scope |
| `fsm.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| SLA compliance | Theo dõi vận hành module |
| FTF | Theo dõi vận hành module |
| Parts cost/ticket | Theo dõi vận hành module |
| Tech utilization | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Thiết bị có thể gắn serial từ đơn bán CRM/INV.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có tích hợp IoT/cảnh báo máy phase sau?

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
| Bản SRS này | `SRS_FSM_v1.1.md` / `.docx` |
| UC IDs | `UC_FSM_001` … |

---

*Hết tài liệu SRS-FSM-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
