# SRS-BI-v1.1 — Báo cáo & Business Intelligence

> **Software Requirements Specification — Module BI**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-BI-v1.1` |
| Module | `BI` — Báo cáo & Business Intelligence |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Quản trị & Báo cáo |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `Các module nghiệp vụ đã mua — dataset theo license` |
| Số nhóm / UC | 6 nhóm / 30 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Báo cáo & Business Intelligence** (`BI`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module BI cung cấp dataset theo license, dashboard quản trị, thư viện báo cáo chuẩn, cảnh báo KPI, self-service cơ bản và (phase sau) dự báo/AI.

### 1.3. Mục tiêu đo được
1. Một nơi xem KPI tổng hợp theo quyền và module đã mua.
2. Giảm phụ thuộc xuất Excel thủ công.
3. Cảnh báo sớm khi KPI vượt ngưỡng.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Datasets, dashboards, report catalog, alerts, self-service light, optional forecast.

### 2.2. Out of Scope
- Data warehouse doanh nghiệp tách biệt hoàn toàn (có thể phase sau).
- ETL ngoài hệ thống.

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `Các module nghiệp vụ đã mua — dataset theo license`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| BI Admin | Catalog dataset/KPI, phân quyền |
| Executive | Dashboard lãnh đạo |
| Manager | Báo cáo chuyên môn |
| Analyst | Self-service nhẹ |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Dataset | Tập dữ liệu được phép phân tích |
| KPI | Chỉ tiêu đo lường |
| Alert threshold | Ngưỡng cảnh báo |
| Refresh | Làm mới dữ liệu |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- BI (Báo cáo & Business Intelligence)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `BI` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `BI` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | Tất cả module | Sự kiện/dữ liệu nguồn |
| Tích hợp | SYS | License & ACL |
| Tích hợp | Email | Schedule report |

---

## 6. Catalog chức năng

**Tổng:** 6 nhóm · 30 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `BI-01` | Nền tảng dữ liệu báo cáo | 5 | 3 | 1 | 1 |
| 2 | `BI-02` | Dashboard quản trị | 7 | 3 | 4 | 0 |
| 3 | `BI-03` | Thư viện báo cáo chuẩn | 6 | 4 | 2 | 0 |
| 4 | `BI-04` | Cảnh báo & KPI | 4 | 2 | 2 | 0 |
| 5 | `BI-05` | Self-service & phân tích | 4 | 0 | 3 | 1 |
| 6 | `BI-06` | Dự báo & AI | 4 | 0 | 0 | 4 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_BI_001` | Nền tảng dữ liệu báo cáo | Catalog dataset theo module | Must |
| `UC_BI_002` | Nền tảng dữ liệu báo cáo | Làm mới dữ liệu định kỳ | Must |
| `UC_BI_003` | Nền tảng dữ liệu báo cáo | Phân quyền xem báo cáo | Must |
| `UC_BI_004` | Nền tảng dữ liệu báo cáo | Từ điển chỉ tiêu KPI | Should |
| `UC_BI_005` | Nền tảng dữ liệu báo cáo | Nhật ký truy cập báo cáo | Could |
| `UC_BI_006` | Dashboard quản trị | Dashboard Ban lãnh đạo | Must |
| `UC_BI_007` | Dashboard quản trị | Dashboard theo module | Must |
| `UC_BI_008` | Dashboard quản trị | Widget doanh thu – lợi nhuận | Must |
| `UC_BI_009` | Dashboard quản trị | Widget tồn – mua – giao | Should |
| `UC_BI_010` | Dashboard quản trị | Widget nhân sự – công | Should |
| `UC_BI_011` | Dashboard quản trị | Widget sales pipeline | Should |
| `UC_BI_012` | Dashboard quản trị | Tùy chỉnh bố cục theo role | Should |
| `UC_BI_013` | Thư viện báo cáo chuẩn | Danh mục báo cáo theo module | Must |
| `UC_BI_014` | Thư viện báo cáo chuẩn | Chạy báo cáo với bộ lọc | Must |
| `UC_BI_015` | Thư viện báo cáo chuẩn | Lưu bộ lọc / yêu thích | Should |
| `UC_BI_016` | Thư viện báo cáo chuẩn | Xuất Excel / PDF | Must |
| `UC_BI_017` | Thư viện báo cáo chuẩn | Gửi báo cáo email định kỳ | Should |
| `UC_BI_018` | Thư viện báo cáo chuẩn | So sánh kỳ / mục tiêu | Must |
| `UC_BI_019` | Cảnh báo & KPI | Cấu hình ngưỡng cảnh báo | Must |
| `UC_BI_020` | Cảnh báo & KPI | Cảnh báo realtime / digest | Should |
| `UC_BI_021` | Cảnh báo & KPI | Bảng theo dõi Target vs Actual | Must |
| `UC_BI_022` | Cảnh báo & KPI | Đăng ký nhận cảnh báo | Should |
| `UC_BI_023` | Self-service & phân tích | Tạo báo cáo tùy chỉnh | Could |
| `UC_BI_024` | Self-service & phân tích | Pivot / biểu đồ tương tác | Should |
| `UC_BI_025` | Self-service & phân tích | Chia sẻ báo cáo | Should |
| `UC_BI_026` | Self-service & phân tích | Xuất dataset đã lọc | Should |
| `UC_BI_027` | Dự báo & AI | Dự báo doanh thu | Later |
| `UC_BI_028` | Dự báo & AI | Dự báo nhu cầu | Later |
| `UC_BI_029` | Dự báo & AI | Phát hiện bất thường | Later |
| `UC_BI_030` | Dự báo & AI | Tóm tắt insight bằng AI | Later |

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

### 7.1. Nền tảng dữ liệu báo cáo (`BI-01`)

Nhóm **Nền tảng dữ liệu báo cáo** gồm **5** use case của module `BI`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 1. Đặc tả Use Case "Catalog dataset theo module"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_001 |
| **Tên Use Case** | Catalog dataset theo module |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Catalog dataset theo module" thuộc nhóm Nền tảng dữ liệu báo cáo trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Licensed dataset catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Catalog dataset theo module» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Catalog dataset theo module» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Catalog dataset theo module» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Catalog dataset theo module» trong nhóm Nền tảng dữ liệu báo cáo.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Licensed dataset catalog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Catalog dataset theo module».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Catalog dataset theo module» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Làm mới dữ liệu định kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_002 |
| **Tên Use Case** | Làm mới dữ liệu định kỳ |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Làm mới dữ liệu định kỳ" thuộc nhóm Nền tảng dữ liệu báo cáo trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Data refresh policy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Làm mới dữ liệu định kỳ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Làm mới dữ liệu định kỳ» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Làm mới dữ liệu định kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Làm mới dữ liệu định kỳ» trong nhóm Nền tảng dữ liệu báo cáo.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Data refresh policy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Làm mới dữ liệu định kỳ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Làm mới dữ liệu định kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Phân quyền xem báo cáo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_003 |
| **Tên Use Case** | Phân quyền xem báo cáo |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Phân quyền xem báo cáo" thuộc nhóm Nền tảng dữ liệu báo cáo trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: BI access control |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân quyền xem báo cáo» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân quyền xem báo cáo» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân quyền xem báo cáo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. BI Admin mở «Phân quyền xem báo cáo» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (BI access control); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân quyền xem báo cáo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Từ điển chỉ tiêu KPI"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_004 |
| **Tên Use Case** | Từ điển chỉ tiêu KPI |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Từ điển chỉ tiêu KPI" thuộc nhóm Nền tảng dữ liệu báo cáo trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: KPI dictionary |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Từ điển chỉ tiêu KPI» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Từ điển chỉ tiêu KPI» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Từ điển chỉ tiêu KPI» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Từ điển chỉ tiêu KPI» trong nhóm Nền tảng dữ liệu báo cáo.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (KPI dictionary).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Từ điển chỉ tiêu KPI».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Từ điển chỉ tiêu KPI» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 5. Đặc tả Use Case "Nhật ký truy cập báo cáo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_005 |
| **Tên Use Case** | Nhật ký truy cập báo cáo |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Nhật ký truy cập báo cáo" thuộc nhóm Nền tảng dữ liệu báo cáo trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Report access log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhật ký truy cập báo cáo» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhật ký truy cập báo cáo» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhật ký truy cập báo cáo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. BI Admin mở «Nhật ký truy cập báo cáo» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Report access log); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhật ký truy cập báo cáo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Dashboard quản trị (`BI-02`)

Nhóm **Dashboard quản trị** gồm **7** use case của module `BI`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 3 |

**Bảng 6. Đặc tả Use Case "Dashboard Ban lãnh đạo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_006 |
| **Tên Use Case** | Dashboard Ban lãnh đạo |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Dashboard Ban lãnh đạo" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Executive dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard Ban lãnh đạo» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard Ban lãnh đạo» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard Ban lãnh đạo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Executive mở «Dashboard Ban lãnh đạo» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Executive dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard Ban lãnh đạo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 7. Đặc tả Use Case "Dashboard theo module"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_007 |
| **Tên Use Case** | Dashboard theo module |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Dashboard theo module" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Module-specific dashboards |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard theo module» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard theo module» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard theo module» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Executive mở «Dashboard theo module» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Module-specific dashboards); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard theo module» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Widget doanh thu – lợi nhuận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_008 |
| **Tên Use Case** | Widget doanh thu – lợi nhuận |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Widget doanh thu – lợi nhuận" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Revenue & profit widgets |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Widget doanh thu – lợi nhuận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Widget doanh thu – lợi nhuận» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Widget doanh thu – lợi nhuận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Executive khởi tạo thao tác «Widget doanh thu – lợi nhuận» trong nhóm Dashboard quản trị.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Revenue & profit widgets).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Widget doanh thu – lợi nhuận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Widget doanh thu – lợi nhuận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Widget tồn – mua – giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_009 |
| **Tên Use Case** | Widget tồn – mua – giao |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Widget tồn – mua – giao" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Supply chain widgets |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Widget tồn – mua – giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Widget tồn – mua – giao» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Widget tồn – mua – giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Executive khởi tạo thao tác «Widget tồn – mua – giao» trong nhóm Dashboard quản trị.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Supply chain widgets).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Widget tồn – mua – giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Widget tồn – mua – giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Widget nhân sự – công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_010 |
| **Tên Use Case** | Widget nhân sự – công |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Widget nhân sự – công" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: HR widgets |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Widget nhân sự – công» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Widget nhân sự – công» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Widget nhân sự – công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Executive khởi tạo thao tác «Widget nhân sự – công» trong nhóm Dashboard quản trị.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (HR widgets).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Widget nhân sự – công».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Widget nhân sự – công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "Widget sales pipeline"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_011 |
| **Tên Use Case** | Widget sales pipeline |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Widget sales pipeline" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Sales widgets |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Widget sales pipeline» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Widget sales pipeline» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Widget sales pipeline» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Executive mở «Widget sales pipeline» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Sales widgets).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Widget sales pipeline» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Tùy chỉnh bố cục theo role"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_012 |
| **Tên Use Case** | Tùy chỉnh bố cục theo role |
| **Tác nhân** | Executive |
| **Mô tả chức năng** | Cho phép Executive thực hiện chức năng "Tùy chỉnh bố cục theo role" thuộc nhóm Dashboard quản trị trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Layout customization |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Executive] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tùy chỉnh bố cục theo role» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tùy chỉnh bố cục theo role» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tùy chỉnh bố cục theo role» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Executive khởi tạo thao tác «Tùy chỉnh bố cục theo role» trong nhóm Dashboard quản trị.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Layout customization).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tùy chỉnh bố cục theo role».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tùy chỉnh bố cục theo role» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Thư viện báo cáo chuẩn (`BI-03`)

Nhóm **Thư viện báo cáo chuẩn** gồm **6** use case của module `BI`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 13. Đặc tả Use Case "Danh mục báo cáo theo module"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_013 |
| **Tên Use Case** | Danh mục báo cáo theo module |
| **Tác nhân** | Manager |
| **Mô tả chức năng** | Cho phép Manager thực hiện chức năng "Danh mục báo cáo theo module" thuộc nhóm Thư viện báo cáo chuẩn trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Report catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Manager] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục báo cáo theo module» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục báo cáo theo module» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục báo cáo theo module» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Manager mở «Danh mục báo cáo theo module» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Report catalog); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục báo cáo theo module» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 14. Đặc tả Use Case "Chạy báo cáo với bộ lọc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_014 |
| **Tên Use Case** | Chạy báo cáo với bộ lọc |
| **Tác nhân** | Manager |
| **Mô tả chức năng** | Cho phép Manager thực hiện chức năng "Chạy báo cáo với bộ lọc" thuộc nhóm Thư viện báo cáo chuẩn trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Parameterized reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Manager] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chạy báo cáo với bộ lọc» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chạy báo cáo với bộ lọc» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chạy báo cáo với bộ lọc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Manager mở «Chạy báo cáo với bộ lọc» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Parameterized reports); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chạy báo cáo với bộ lọc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Lưu bộ lọc / yêu thích"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_015 |
| **Tên Use Case** | Lưu bộ lọc / yêu thích |
| **Tác nhân** | Manager |
| **Mô tả chức năng** | Cho phép Manager thực hiện chức năng "Lưu bộ lọc / yêu thích" thuộc nhóm Thư viện báo cáo chuẩn trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Saved report views |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Manager] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lưu bộ lọc / yêu thích» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lưu bộ lọc / yêu thích» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lưu bộ lọc / yêu thích» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Manager khởi tạo thao tác «Lưu bộ lọc / yêu thích» trong nhóm Thư viện báo cáo chuẩn.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Saved report views).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lưu bộ lọc / yêu thích».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lưu bộ lọc / yêu thích» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Xuất Excel / PDF"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_016 |
| **Tên Use Case** | Xuất Excel / PDF |
| **Tác nhân** | Manager |
| **Mô tả chức năng** | Cho phép Manager thực hiện chức năng "Xuất Excel / PDF" thuộc nhóm Thư viện báo cáo chuẩn trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Report export |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Manager] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất Excel / PDF» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất Excel / PDF» được lưu nhất quán trong module `BI`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất Excel / PDF» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Manager mở «Xuất Excel / PDF», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất Excel / PDF» (Report export).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất Excel / PDF» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Gửi báo cáo email định kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_017 |
| **Tên Use Case** | Gửi báo cáo email định kỳ |
| **Tác nhân** | Manager |
| **Mô tả chức năng** | Cho phép Manager thực hiện chức năng "Gửi báo cáo email định kỳ" thuộc nhóm Thư viện báo cáo chuẩn trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Scheduled report email |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Manager] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gửi báo cáo email định kỳ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gửi báo cáo email định kỳ» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi báo cáo email định kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Manager mở «Gửi báo cáo email định kỳ» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Scheduled report email); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gửi báo cáo email định kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "So sánh kỳ / mục tiêu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_018 |
| **Tên Use Case** | So sánh kỳ / mục tiêu |
| **Tác nhân** | Manager |
| **Mô tả chức năng** | Cho phép Manager thực hiện chức năng "So sánh kỳ / mục tiêu" thuộc nhóm Thư viện báo cáo chuẩn trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Period & target comparison |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Manager] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh kỳ / mục tiêu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh kỳ / mục tiêu» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh kỳ / mục tiêu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Manager khởi tạo thao tác «So sánh kỳ / mục tiêu» trong nhóm Thư viện báo cáo chuẩn.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Period & target comparison).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «So sánh kỳ / mục tiêu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh kỳ / mục tiêu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Cảnh báo & KPI (`BI-04`)

Nhóm **Cảnh báo & KPI** gồm **4** use case của module `BI`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 2 |

**Bảng 19. Đặc tả Use Case "Cấu hình ngưỡng cảnh báo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_019 |
| **Tên Use Case** | Cấu hình ngưỡng cảnh báo |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Cấu hình ngưỡng cảnh báo" thuộc nhóm Cảnh báo & KPI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: KPI threshold alerts |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình ngưỡng cảnh báo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình ngưỡng cảnh báo» được lưu nhất quán trong module `BI`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình ngưỡng cảnh báo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc BI Admin kích hoạt kiểm tra điều kiện «Cấu hình ngưỡng cảnh báo».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (KPI threshold alerts).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình ngưỡng cảnh báo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Cảnh báo realtime / digest"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_020 |
| **Tên Use Case** | Cảnh báo realtime / digest |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Cảnh báo realtime / digest" thuộc nhóm Cảnh báo & KPI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Alert channels |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo realtime / digest» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo realtime / digest» được lưu nhất quán trong module `BI`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo realtime / digest» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc BI Admin kích hoạt kiểm tra điều kiện «Cảnh báo realtime / digest».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Alert channels).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo realtime / digest» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Bảng theo dõi Target vs Actual"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_021 |
| **Tên Use Case** | Bảng theo dõi Target vs Actual |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Bảng theo dõi Target vs Actual" thuộc nhóm Cảnh báo & KPI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: KPI tracking board |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng theo dõi Target vs Actual» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng theo dõi Target vs Actual» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng theo dõi Target vs Actual» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Bảng theo dõi Target vs Actual» trong nhóm Cảnh báo & KPI.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (KPI tracking board).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng theo dõi Target vs Actual».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng theo dõi Target vs Actual» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Đăng ký nhận cảnh báo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_022 |
| **Tên Use Case** | Đăng ký nhận cảnh báo |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Đăng ký nhận cảnh báo" thuộc nhóm Cảnh báo & KPI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Subscribe to alerts |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng ký nhận cảnh báo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng ký nhận cảnh báo» được lưu nhất quán trong module `BI`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng ký nhận cảnh báo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc BI Admin kích hoạt kiểm tra điều kiện «Đăng ký nhận cảnh báo».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Subscribe to alerts).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng ký nhận cảnh báo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Self-service & phân tích (`BI-05`)

Nhóm **Self-service & phân tích** gồm **4** use case của module `BI`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 23. Đặc tả Use Case "Tạo báo cáo tùy chỉnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_023 |
| **Tên Use Case** | Tạo báo cáo tùy chỉnh |
| **Tác nhân** | Analyst |
| **Mô tả chức năng** | Cho phép Analyst thực hiện chức năng "Tạo báo cáo tùy chỉnh" thuộc nhóm Self-service & phân tích trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Self-service reporting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Analyst] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo báo cáo tùy chỉnh» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo báo cáo tùy chỉnh» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo báo cáo tùy chỉnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Analyst mở «Tạo báo cáo tùy chỉnh» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Self-service reporting); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo báo cáo tùy chỉnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Pivot / biểu đồ tương tác"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_024 |
| **Tên Use Case** | Pivot / biểu đồ tương tác |
| **Tác nhân** | Analyst |
| **Mô tả chức năng** | Cho phép Analyst thực hiện chức năng "Pivot / biểu đồ tương tác" thuộc nhóm Self-service & phân tích trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Interactive visualizations |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Analyst] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Pivot / biểu đồ tương tác» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Pivot / biểu đồ tương tác» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Pivot / biểu đồ tương tác» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Analyst khởi tạo thao tác «Pivot / biểu đồ tương tác» trong nhóm Self-service & phân tích.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Interactive visualizations).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Pivot / biểu đồ tương tác».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Pivot / biểu đồ tương tác» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Chia sẻ báo cáo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_025 |
| **Tên Use Case** | Chia sẻ báo cáo |
| **Tác nhân** | Analyst |
| **Mô tả chức năng** | Cho phép Analyst thực hiện chức năng "Chia sẻ báo cáo" thuộc nhóm Self-service & phân tích trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Share reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Analyst] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chia sẻ báo cáo» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chia sẻ báo cáo» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chia sẻ báo cáo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Analyst mở «Chia sẻ báo cáo» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Share reports); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chia sẻ báo cáo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Xuất dataset đã lọc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_026 |
| **Tên Use Case** | Xuất dataset đã lọc |
| **Tác nhân** | Analyst |
| **Mô tả chức năng** | Cho phép Analyst thực hiện chức năng "Xuất dataset đã lọc" thuộc nhóm Self-service & phân tích trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Filtered data extract |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Analyst] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất dataset đã lọc» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất dataset đã lọc» được lưu nhất quán trong module `BI`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất dataset đã lọc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. Analyst mở «Xuất dataset đã lọc», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất dataset đã lọc» (Filtered data extract).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất dataset đã lọc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Dự báo & AI (`BI-06`)

Nhóm **Dự báo & AI** gồm **4** use case của module `BI`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 27. Đặc tả Use Case "Dự báo doanh thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_027 |
| **Tên Use Case** | Dự báo doanh thu |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Dự báo doanh thu" thuộc nhóm Dự báo & AI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Sales forecast |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dự báo doanh thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dự báo doanh thu» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dự báo doanh thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Dự báo doanh thu» trong nhóm Dự báo & AI.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Sales forecast).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Dự báo doanh thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dự báo doanh thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 28. Đặc tả Use Case "Dự báo nhu cầu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_028 |
| **Tên Use Case** | Dự báo nhu cầu |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Dự báo nhu cầu" thuộc nhóm Dự báo & AI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Demand forecast |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dự báo nhu cầu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dự báo nhu cầu» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dự báo nhu cầu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Dự báo nhu cầu» trong nhóm Dự báo & AI.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Demand forecast).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Dự báo nhu cầu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dự báo nhu cầu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Phát hiện bất thường"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_029 |
| **Tên Use Case** | Phát hiện bất thường |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Phát hiện bất thường" thuộc nhóm Dự báo & AI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: Anomaly detection |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phát hiện bất thường» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phát hiện bất thường» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phát hiện bất thường» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Phát hiện bất thường» trong nhóm Dự báo & AI.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (Anomaly detection).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phát hiện bất thường».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phát hiện bất thường» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Tóm tắt insight bằng AI"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_BI_030 |
| **Tên Use Case** | Tóm tắt insight bằng AI |
| **Tác nhân** | BI Admin |
| **Mô tả chức năng** | Cho phép BI Admin thực hiện chức năng "Tóm tắt insight bằng AI" thuộc nhóm Dự báo & AI trong module BI — Báo cáo & Business Intelligence. Mô tả chi tiết: AI insights summary |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [BI Admin] và được cấp quyền RBAC tương ứng.<br>• License module `BI` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tóm tắt insight bằng AI» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-BI-SCOPE-01`, `BR-BI-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tóm tắt insight bằng AI» được lưu nhất quán trong module `BI`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tóm tắt insight bằng AI» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. BI Admin khởi tạo thao tác «Tóm tắt insight bằng AI» trong nhóm Dự báo & AI.<br>2. Hệ thống kiểm tra license `BI`, quyền RBAC và tiền điều kiện nghiệp vụ (AI insights summary).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tóm tắt insight bằng AI».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tóm tắt insight bằng AI» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-BI-01 — Phát hành dashboard theo gói license

**Mục tiêu:** User chỉ thấy KPI module được mua

| Bước | Mô tả |
|---:|---|
| 1 | Xác định module active trên license |
| 2 | Mở dataset/dashboard tương ứng |
| 3 | Gán quyền xem theo role |
| 4 | Cấu hình refresh và alert |
| 5 | User tiêu thụ báo cáo/xuất file/đăng ký email |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Dataset / DatasetField` | Nguồn phân tích |
| `Dashboard / Widget` | Trực quan |
| `ReportDefinition` | Báo cáo chuẩn |
| `KpiDefinition / AlertRule` | KPI & cảnh báo |
| `ScheduledReport` | Lịch gửi |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `BI`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-BI-01: Không expose dataset của module chưa license.
- BR-BI-02: Phân quyền BI không được vượt data scope SYS.
- BR-BI-03: Báo cáo tài chính nhạy cảm chỉ role được cấp.
- BR-BI-04: Mọi truy cập dataset quan trọng có thể ghi access log.
- BR-BI-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-BI-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-BI-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Hiệu năng dashboard | Tải dashboard chuẩn p95 < 5s với cache |
| Freshness | Refresh gần realtime hoặc định kỳ theo cấu hình |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `BI` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `bi.dashboard.view` | Quyền chức năng module |
| `bi.report.view` | Quyền chức năng module |
| `bi.dataset.manage` | Quyền chức năng module |
| `bi.alert.manage` | Quyền chức năng module |
| `bi.selfservice.use` | Quyền chức năng module |
| `bi.*.view` | Xem trong data scope |
| `bi.*.manage` | Tạo/sửa trong data scope |
| `bi.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Dashboard adoption | Theo dõi vận hành module |
| Scheduled report success rate | Theo dõi vận hành module |
| Alert response time | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Mỗi module nghiệp vụ cung cấp view/read-model cho BI.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Dùng OLTP trực tiếp có read replica hay bắt buộc ODS phase 1?

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
| Bản SRS này | `SRS_BI_v1.1.md` / `.docx` |
| UC IDs | `UC_BI_001` … |

---

*Hết tài liệu SRS-BI-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
