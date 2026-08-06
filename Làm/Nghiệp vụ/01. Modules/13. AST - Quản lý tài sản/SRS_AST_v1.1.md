# SRS-AST-v1.1 — Quản lý tài sản

> **Software Requirements Specification — Module AST**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-AST-v1.1` |
| Module | `AST` — Quản lý tài sản |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Tài chính |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `FIN`, `HRM`, `PUR` |
| Số nhóm / UC | 6 nhóm / 34 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Quản lý tài sản** (`AST`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Quản lý TSCĐ/CCDC: thẻ tài sản, khấu hao, ghi tăng/giảm, điều chuyển, kiểm kê, cấp phát và báo cáo giá trị còn lại.

### 1.3. Mục tiêu đo được
1. Theo dõi và định vị tài sản.
2. Tính khấu hao định kỳ và post FIN.
3. Kiểm kê và thu hồi khi nhân sự nghỉ việc.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Asset master, depreciation, acquire/transfer/dispose, stocktake, tools issue, AST reports.

### 2.2. Out of Scope
- FSM bảo trì hiện trường khách hàng (khác tài sản nội bộ).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `FIN`, `HRM`, `PUR`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Asset Accountant | Khấu hao & sổ TS |
| Asset Custodian | Quản lý hiện vật |
| Department Manager | Người giữ TS |
| HR Officer | Thu hồi khi nghỉ việc |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| NBV | Net Book Value — giá trị còn lại |
| Depreciation | Khấu hao |
| CCDC | Công cụ dụng cụ |
| Disposal | Thanh lý/ghi giảm |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- AST (Quản lý tài sản)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `AST` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `AST` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | FIN | Ghi tăng/KH/thanh lý |
| Tích hợp | PUR | Mua sắm vốn hóa |
| Tích hợp | HRM | Thu hồi khi offboarding |
| Tích hợp | PJM | Vốn hóa từ dự án |

---

## 6. Catalog chức năng

**Tổng:** 6 nhóm · 34 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `AST-01` | Danh mục tài sản | 7 | 4 | 2 | 1 |
| 2 | `AST-02` | Khấu hao | 6 | 5 | 1 | 0 |
| 3 | `AST-03` | Ghi tăng – ghi giảm | 7 | 4 | 1 | 2 |
| 4 | `AST-04` | Kiểm kê & bảo trì tài sản | 5 | 2 | 2 | 1 |
| 5 | `AST-05` | Công cụ dụng cụ & cấp phát | 4 | 0 | 3 | 1 |
| 6 | `AST-06` | Báo cáo tài sản | 5 | 4 | 1 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_AST_001` | Danh mục tài sản | Danh mục nhóm TSCĐ | Must |
| `UC_AST_002` | Danh mục tài sản | Tạo thẻ tài sản | Must |
| `UC_AST_003` | Danh mục tài sản | Thông tin nguyên giá / ngày ghi tăng | Must |
| `UC_AST_004` | Danh mục tài sản | Gắn vị trí / chi nhánh | Must |
| `UC_AST_005` | Danh mục tài sản | Ảnh & tài liệu kèm | Should |
| `UC_AST_006` | Danh mục tài sản | Import danh mục tài sản | Could |
| `UC_AST_007` | Danh mục tài sản | In tem mã tài sản | Should |
| `UC_AST_008` | Khấu hao | Cấu hình phương pháp khấu hao | Must |
| `UC_AST_009` | Khấu hao | Cấu hình thời gian / tỷ lệ | Must |
| `UC_AST_010` | Khấu hao | Tính khấu hao định kỳ | Must |
| `UC_AST_011` | Khấu hao | Xem sổ khấu hao | Must |
| `UC_AST_012` | Khấu hao | Đẩy bút toán khấu hao sang FIN | Must |
| `UC_AST_013` | Khấu hao | Tạm dừng / điều chỉnh khấu hao | Should |
| `UC_AST_014` | Ghi tăng – ghi giảm | Ghi tăng từ mua sắm | Must |
| `UC_AST_015` | Ghi tăng – ghi giảm | Ghi tăng từ xây dựng | Could |
| `UC_AST_016` | Ghi tăng – ghi giảm | Điều chuyển tài sản nội bộ | Must |
| `UC_AST_017` | Ghi tăng – ghi giảm | Bàn giao tài sản cho nhân viên | Must |
| `UC_AST_018` | Ghi tăng – ghi giảm | Thanh lý / nhượng bán | Must |
| `UC_AST_019` | Ghi tăng – ghi giảm | Ghi giảm do mất mát | Should |
| `UC_AST_020` | Ghi tăng – ghi giảm | Đánh giá lại nguyên giá | Later |
| `UC_AST_021` | Kiểm kê & bảo trì tài sản | Tạo đợt kiểm kê tài sản | Must |
| `UC_AST_022` | Kiểm kê & bảo trì tài sản | Đối chiếu thiếu / thừa | Must |
| `UC_AST_023` | Kiểm kê & bảo trì tài sản | Lịch bảo trì TSCĐ | Could |
| `UC_AST_024` | Kiểm kê & bảo trì tài sản | Lịch sử sửa chữa | Should |
| `UC_AST_025` | Kiểm kê & bảo trì tài sản | Cảnh báo tài sản sắp hết khấu hao | Should |
| `UC_AST_026` | Công cụ dụng cụ & cấp phát | Quản lý công cụ dụng cụ | Should |
| `UC_AST_027` | Công cụ dụng cụ & cấp phát | Cấp phát công cụ cho nhân viên | Should |
| `UC_AST_028` | Công cụ dụng cụ & cấp phát | Thu hồi công cụ | Should |
| `UC_AST_029` | Công cụ dụng cụ & cấp phát | Phân bổ chi phí công cụ | Could |
| `UC_AST_030` | Báo cáo tài sản | Sổ tài sản cố định | Must |
| `UC_AST_031` | Báo cáo tài sản | Báo cáo khấu hao theo kỳ | Must |
| `UC_AST_032` | Báo cáo tài sản | Báo cáo tài sản theo vị trí | Must |
| `UC_AST_033` | Báo cáo tài sản | Giá trị còn lại theo nhóm | Should |
| `UC_AST_034` | Báo cáo tài sản | Xuất báo cáo tài sản | Must |

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

### 7.1. Danh mục tài sản (`AST-01`)

Nhóm **Danh mục tài sản** gồm **7** use case của module `AST`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 4 |

**Bảng 1. Đặc tả Use Case "Danh mục nhóm TSCĐ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_001 |
| **Tên Use Case** | Danh mục nhóm TSCĐ |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Danh mục nhóm TSCĐ" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset category |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục nhóm TSCĐ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục nhóm TSCĐ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục nhóm TSCĐ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Danh mục nhóm TSCĐ» trong nhóm Danh mục tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset category).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục nhóm TSCĐ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục nhóm TSCĐ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Tạo thẻ tài sản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_002 |
| **Tên Use Case** | Tạo thẻ tài sản |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Tạo thẻ tài sản" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset card |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo thẻ tài sản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo thẻ tài sản» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo thẻ tài sản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở chức năng «Tạo thẻ tài sản» trong nhóm Danh mục tài sản.<br>2. Hệ thống kiểm tra license `AST`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo thẻ tài sản» (Asset card).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo thẻ tài sản» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo thẻ tài sản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 3. Đặc tả Use Case "Thông tin nguyên giá / ngày ghi tăng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_003 |
| **Tên Use Case** | Thông tin nguyên giá / ngày ghi tăng |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Thông tin nguyên giá / ngày ghi tăng" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Acquisition info |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thông tin nguyên giá / ngày ghi tăng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thông tin nguyên giá / ngày ghi tăng» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thông tin nguyên giá / ngày ghi tăng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Thông tin nguyên giá / ngày ghi tăng» trong nhóm Danh mục tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Acquisition info).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thông tin nguyên giá / ngày ghi tăng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thông tin nguyên giá / ngày ghi tăng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Gắn vị trí / chi nhánh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_004 |
| **Tên Use Case** | Gắn vị trí / chi nhánh |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Gắn vị trí / chi nhánh" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset allocation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn vị trí / chi nhánh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn vị trí / chi nhánh» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn vị trí / chi nhánh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Gắn vị trí / chi nhánh» trong nhóm Danh mục tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset allocation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn vị trí / chi nhánh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn vị trí / chi nhánh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 5. Đặc tả Use Case "Ảnh & tài liệu kèm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_005 |
| **Tên Use Case** | Ảnh & tài liệu kèm |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Ảnh & tài liệu kèm" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ảnh & tài liệu kèm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ảnh & tài liệu kèm» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ảnh & tài liệu kèm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Ảnh & tài liệu kèm» trong nhóm Danh mục tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset documents).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ảnh & tài liệu kèm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ảnh & tài liệu kèm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 6. Đặc tả Use Case "Import danh mục tài sản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_006 |
| **Tên Use Case** | Import danh mục tài sản |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Import danh mục tài sản" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset import |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import danh mục tài sản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`, `BR-AST-IMP-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import danh mục tài sản» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import danh mục tài sản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant tải file mẫu (nếu có) và chọn file import cho «Import danh mục tài sản».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import danh mục tài sản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn. |

**Bảng 7. Đặc tả Use Case "In tem mã tài sản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_007 |
| **Tên Use Case** | In tem mã tài sản |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "In tem mã tài sản" thuộc nhóm Danh mục tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset label printing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In tem mã tài sản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In tem mã tài sản» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In tem mã tài sản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «In tem mã tài sản» trong nhóm Danh mục tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset label printing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «In tem mã tài sản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In tem mã tài sản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Khấu hao (`AST-02`)

Nhóm **Khấu hao** gồm **6** use case của module `AST`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 5 |

**Bảng 8. Đặc tả Use Case "Cấu hình phương pháp khấu hao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_008 |
| **Tên Use Case** | Cấu hình phương pháp khấu hao |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Cấu hình phương pháp khấu hao" thuộc nhóm Khấu hao trong module AST — Quản lý tài sản. Mô tả chi tiết: Depreciation method |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình phương pháp khấu hao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình phương pháp khấu hao» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình phương pháp khấu hao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở màn hình cấu hình «Cấu hình phương pháp khấu hao» trong Khấu hao.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Depreciation method) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình phương pháp khấu hao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 9. Đặc tả Use Case "Cấu hình thời gian / tỷ lệ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_009 |
| **Tên Use Case** | Cấu hình thời gian / tỷ lệ |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Cấu hình thời gian / tỷ lệ" thuộc nhóm Khấu hao trong module AST — Quản lý tài sản. Mô tả chi tiết: Useful life & rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thời gian / tỷ lệ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thời gian / tỷ lệ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thời gian / tỷ lệ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở màn hình cấu hình «Cấu hình thời gian / tỷ lệ» trong Khấu hao.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Useful life & rate) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thời gian / tỷ lệ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 10. Đặc tả Use Case "Tính khấu hao định kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_010 |
| **Tên Use Case** | Tính khấu hao định kỳ |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Tính khấu hao định kỳ" thuộc nhóm Khấu hao trong module AST — Quản lý tài sản. Mô tả chi tiết: Run depreciation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính khấu hao định kỳ» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu nguồn (công, tồn, tỷ giá…) đã sẵn sàng và đạt điều kiện chốt. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`, `BR-AST-CALC-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính khấu hao định kỳ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính khấu hao định kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Kết quả tính toán tái lập được với cùng input/rule (deterministic trong cùng phiên bản rule).<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant chọn phạm vi tính toán cho «Tính khấu hao định kỳ» (kỳ, đơn vị, bộ lọc).<br>2. Hệ thống nạp dữ liệu nguồn liên quan (Run depreciation).<br>3. Chạy engine tính theo rule cấu hình; log chi tiết từng bước lỗi nếu có.<br>4. Hiển thị kết quả nháp để rà soát; cho phép điều chỉnh có audit trước khi chốt.<br>5. Xác nhận ghi nhận kết quả chính thức; phát sự kiện cho FIN/module liên quan nếu cần.<br>6. Thông báo hoàn tất và cập nhật trạng thái kỳ/tính toán. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính khấu hao định kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thiếu dữ liệu nguồn hoặc rule cấu hình không đầy đủ → dừng job, liệt kê lỗi chi tiết để sửa. |

**Bảng 11. Đặc tả Use Case "Xem sổ khấu hao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_011 |
| **Tên Use Case** | Xem sổ khấu hao |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Xem sổ khấu hao" thuộc nhóm Khấu hao trong module AST — Quản lý tài sản. Mô tả chi tiết: Depreciation book |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem sổ khấu hao» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem sổ khấu hao» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem sổ khấu hao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở «Xem sổ khấu hao» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Depreciation book).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem sổ khấu hao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Đẩy bút toán khấu hao sang FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_012 |
| **Tên Use Case** | Đẩy bút toán khấu hao sang FIN |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Đẩy bút toán khấu hao sang FIN" thuộc nhóm Khấu hao trong module AST — Quản lý tài sản. Mô tả chi tiết: Post depreciation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đẩy bút toán khấu hao sang FIN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đẩy bút toán khấu hao sang FIN» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đẩy bút toán khấu hao sang FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Đẩy bút toán khấu hao sang FIN» trong nhóm Khấu hao.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Post depreciation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đẩy bút toán khấu hao sang FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đẩy bút toán khấu hao sang FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 13. Đặc tả Use Case "Tạm dừng / điều chỉnh khấu hao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_013 |
| **Tên Use Case** | Tạm dừng / điều chỉnh khấu hao |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Tạm dừng / điều chỉnh khấu hao" thuộc nhóm Khấu hao trong module AST — Quản lý tài sản. Mô tả chi tiết: Depreciation adjustment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạm dừng / điều chỉnh khấu hao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạm dừng / điều chỉnh khấu hao» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạm dừng / điều chỉnh khấu hao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant tìm và mở bản ghi liên quan tới «Tạm dừng / điều chỉnh khấu hao» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Tạm dừng / điều chỉnh khấu hao» (Depreciation adjustment).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạm dừng / điều chỉnh khấu hao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

### 7.3. Ghi tăng – ghi giảm (`AST-03`)

Nhóm **Ghi tăng – ghi giảm** gồm **7** use case của module `AST`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 4 |

**Bảng 14. Đặc tả Use Case "Ghi tăng từ mua sắm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_014 |
| **Tên Use Case** | Ghi tăng từ mua sắm |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Ghi tăng từ mua sắm" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: Capitalize from purchase |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi tăng từ mua sắm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi tăng từ mua sắm» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi tăng từ mua sắm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Ghi tăng từ mua sắm» trong nhóm Ghi tăng – ghi giảm.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Capitalize from purchase).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi tăng từ mua sắm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi tăng từ mua sắm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Ghi tăng từ xây dựng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_015 |
| **Tên Use Case** | Ghi tăng từ xây dựng |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Ghi tăng từ xây dựng" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: AUC to fixed asset |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi tăng từ xây dựng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi tăng từ xây dựng» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi tăng từ xây dựng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Ghi tăng từ xây dựng» trong nhóm Ghi tăng – ghi giảm.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (AUC to fixed asset).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi tăng từ xây dựng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi tăng từ xây dựng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Điều chuyển tài sản nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_016 |
| **Tên Use Case** | Điều chuyển tài sản nội bộ |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Điều chuyển tài sản nội bộ" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: Internal asset transfer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điều chuyển tài sản nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điều chuyển tài sản nội bộ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điều chuyển tài sản nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant tìm và mở bản ghi liên quan tới «Điều chuyển tài sản nội bộ» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Điều chuyển tài sản nội bộ» (Internal asset transfer).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điều chuyển tài sản nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 17. Đặc tả Use Case "Bàn giao tài sản cho nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_017 |
| **Tên Use Case** | Bàn giao tài sản cho nhân viên |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Bàn giao tài sản cho nhân viên" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset handover |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bàn giao tài sản cho nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bàn giao tài sản cho nhân viên» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bàn giao tài sản cho nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Bàn giao tài sản cho nhân viên» trong nhóm Ghi tăng – ghi giảm.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset handover).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bàn giao tài sản cho nhân viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bàn giao tài sản cho nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "Thanh lý / nhượng bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_018 |
| **Tên Use Case** | Thanh lý / nhượng bán |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Thanh lý / nhượng bán" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset disposal/sale |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh lý / nhượng bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`, `BR-AST-CAN-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh lý / nhượng bán» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh lý / nhượng bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant chọn đối tượng cần hủy/ngưng trong «Thanh lý / nhượng bán».<br>2. Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.<br>3. Yêu cầu lý do; xác nhận cảnh báo tác động.<br>4. Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.<br>5. Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh lý / nhượng bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 19. Đặc tả Use Case "Ghi giảm do mất mát"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_019 |
| **Tên Use Case** | Ghi giảm do mất mát |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Ghi giảm do mất mát" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset write-off |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi giảm do mất mát» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi giảm do mất mát» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi giảm do mất mát» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Ghi giảm do mất mát» trong nhóm Ghi tăng – ghi giảm.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset write-off).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi giảm do mất mát».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi giảm do mất mát» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Đánh giá lại nguyên giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_020 |
| **Tên Use Case** | Đánh giá lại nguyên giá |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Đánh giá lại nguyên giá" thuộc nhóm Ghi tăng – ghi giảm trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset revaluation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá lại nguyên giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá lại nguyên giá» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá lại nguyên giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Đánh giá lại nguyên giá» trong nhóm Ghi tăng – ghi giảm.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Asset revaluation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá lại nguyên giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá lại nguyên giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Kiểm kê & bảo trì tài sản (`AST-04`)

Nhóm **Kiểm kê & bảo trì tài sản** gồm **5** use case của module `AST`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 2 |

**Bảng 21. Đặc tả Use Case "Tạo đợt kiểm kê tài sản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_021 |
| **Tên Use Case** | Tạo đợt kiểm kê tài sản |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Tạo đợt kiểm kê tài sản" thuộc nhóm Kiểm kê & bảo trì tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset physical count |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đợt kiểm kê tài sản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đợt kiểm kê tài sản» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đợt kiểm kê tài sản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Custodian mở chức năng «Tạo đợt kiểm kê tài sản» trong nhóm Kiểm kê & bảo trì tài sản.<br>2. Hệ thống kiểm tra license `AST`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đợt kiểm kê tài sản» (Asset physical count).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đợt kiểm kê tài sản» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đợt kiểm kê tài sản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 22. Đặc tả Use Case "Đối chiếu thiếu / thừa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_022 |
| **Tên Use Case** | Đối chiếu thiếu / thừa |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Đối chiếu thiếu / thừa" thuộc nhóm Kiểm kê & bảo trì tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Count variance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối chiếu thiếu / thừa» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối chiếu thiếu / thừa» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối chiếu thiếu / thừa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Custodian khởi tạo thao tác «Đối chiếu thiếu / thừa» trong nhóm Kiểm kê & bảo trì tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Count variance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối chiếu thiếu / thừa».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối chiếu thiếu / thừa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Lịch bảo trì TSCĐ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_023 |
| **Tên Use Case** | Lịch bảo trì TSCĐ |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Lịch bảo trì TSCĐ" thuộc nhóm Kiểm kê & bảo trì tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Maintenance schedule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch bảo trì TSCĐ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch bảo trì TSCĐ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch bảo trì TSCĐ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Custodian khởi tạo thao tác «Lịch bảo trì TSCĐ» trong nhóm Kiểm kê & bảo trì tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Maintenance schedule).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lịch bảo trì TSCĐ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch bảo trì TSCĐ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Lịch sử sửa chữa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_024 |
| **Tên Use Case** | Lịch sử sửa chữa |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Lịch sử sửa chữa" thuộc nhóm Kiểm kê & bảo trì tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Repair history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử sửa chữa» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử sửa chữa» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử sửa chữa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Custodian mở «Lịch sử sửa chữa» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Repair history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử sửa chữa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Cảnh báo tài sản sắp hết khấu hao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_025 |
| **Tên Use Case** | Cảnh báo tài sản sắp hết khấu hao |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Cảnh báo tài sản sắp hết khấu hao" thuộc nhóm Kiểm kê & bảo trì tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: End-of-life alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo tài sản sắp hết khấu hao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo tài sản sắp hết khấu hao» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo tài sản sắp hết khấu hao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Asset Custodian kích hoạt kiểm tra điều kiện «Cảnh báo tài sản sắp hết khấu hao».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (End-of-life alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo tài sản sắp hết khấu hao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Công cụ dụng cụ & cấp phát (`AST-05`)

Nhóm **Công cụ dụng cụ & cấp phát** gồm **4** use case của module `AST`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 26. Đặc tả Use Case "Quản lý công cụ dụng cụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_026 |
| **Tên Use Case** | Quản lý công cụ dụng cụ |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Quản lý công cụ dụng cụ" thuộc nhóm Công cụ dụng cụ & cấp phát trong module AST — Quản lý tài sản. Mô tả chi tiết: Tools & supplies |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý công cụ dụng cụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý công cụ dụng cụ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý công cụ dụng cụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Custodian mở danh mục quản lý «Quản lý công cụ dụng cụ» (tài sản / khấu hao / bàn giao; nhóm «Công cụ dụng cụ & cấp phát»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý công cụ dụng cụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 27. Đặc tả Use Case "Cấp phát công cụ cho nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_027 |
| **Tên Use Case** | Cấp phát công cụ cho nhân viên |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Cấp phát công cụ cho nhân viên" thuộc nhóm Công cụ dụng cụ & cấp phát trong module AST — Quản lý tài sản. Mô tả chi tiết: Issue tools |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấp phát công cụ cho nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấp phát công cụ cho nhân viên» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấp phát công cụ cho nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Custodian khởi tạo thao tác «Cấp phát công cụ cho nhân viên» trong nhóm Công cụ dụng cụ & cấp phát.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Issue tools).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Cấp phát công cụ cho nhân viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấp phát công cụ cho nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 28. Đặc tả Use Case "Thu hồi công cụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_028 |
| **Tên Use Case** | Thu hồi công cụ |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Thu hồi công cụ" thuộc nhóm Công cụ dụng cụ & cấp phát trong module AST — Quản lý tài sản. Mô tả chi tiết: Return tools |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thu hồi công cụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thu hồi công cụ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thu hồi công cụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Custodian khởi tạo thao tác «Thu hồi công cụ» trong nhóm Công cụ dụng cụ & cấp phát.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Return tools).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thu hồi công cụ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thu hồi công cụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Phân bổ chi phí công cụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_029 |
| **Tên Use Case** | Phân bổ chi phí công cụ |
| **Tác nhân** | Asset Custodian |
| **Mô tả chức năng** | Cho phép Asset Custodian thực hiện chức năng "Phân bổ chi phí công cụ" thuộc nhóm Công cụ dụng cụ & cấp phát trong module AST — Quản lý tài sản. Mô tả chi tiết: Tools cost allocation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Custodian] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân bổ chi phí công cụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân bổ chi phí công cụ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân bổ chi phí công cụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Custodian khởi tạo thao tác «Phân bổ chi phí công cụ» trong nhóm Công cụ dụng cụ & cấp phát.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Tools cost allocation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân bổ chi phí công cụ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân bổ chi phí công cụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Báo cáo tài sản (`AST-06`)

Nhóm **Báo cáo tài sản** gồm **5** use case của module `AST`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 4 |

**Bảng 30. Đặc tả Use Case "Sổ tài sản cố định"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_030 |
| **Tên Use Case** | Sổ tài sản cố định |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Sổ tài sản cố định" thuộc nhóm Báo cáo tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Fixed asset register |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sổ tài sản cố định» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sổ tài sản cố định» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sổ tài sản cố định» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Sổ tài sản cố định» trong nhóm Báo cáo tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Fixed asset register).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sổ tài sản cố định».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sổ tài sản cố định» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 31. Đặc tả Use Case "Báo cáo khấu hao theo kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_031 |
| **Tên Use Case** | Báo cáo khấu hao theo kỳ |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Báo cáo khấu hao theo kỳ" thuộc nhóm Báo cáo tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Depreciation report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo khấu hao theo kỳ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo khấu hao theo kỳ» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo khấu hao theo kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở «Báo cáo khấu hao theo kỳ» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Depreciation report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo khấu hao theo kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 32. Đặc tả Use Case "Báo cáo tài sản theo vị trí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_032 |
| **Tên Use Case** | Báo cáo tài sản theo vị trí |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Báo cáo tài sản theo vị trí" thuộc nhóm Báo cáo tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Asset by location |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo tài sản theo vị trí» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo tài sản theo vị trí» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo tài sản theo vị trí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở «Báo cáo tài sản theo vị trí» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Asset by location); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo tài sản theo vị trí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 33. Đặc tả Use Case "Giá trị còn lại theo nhóm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_033 |
| **Tên Use Case** | Giá trị còn lại theo nhóm |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Giá trị còn lại theo nhóm" thuộc nhóm Báo cáo tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Net book value by category |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giá trị còn lại theo nhóm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giá trị còn lại theo nhóm» được lưu nhất quán trong module `AST`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giá trị còn lại theo nhóm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Asset Accountant khởi tạo thao tác «Giá trị còn lại theo nhóm» trong nhóm Báo cáo tài sản.<br>2. Hệ thống kiểm tra license `AST`, quyền RBAC và tiền điều kiện nghiệp vụ (Net book value by category).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giá trị còn lại theo nhóm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giá trị còn lại theo nhóm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 34. Đặc tả Use Case "Xuất báo cáo tài sản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_AST_034 |
| **Tên Use Case** | Xuất báo cáo tài sản |
| **Tác nhân** | Asset Accountant |
| **Mô tả chức năng** | Cho phép Asset Accountant thực hiện chức năng "Xuất báo cáo tài sản" thuộc nhóm Báo cáo tài sản trong module AST — Quản lý tài sản. Mô tả chi tiết: Export asset reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Asset Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `AST` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo tài sản» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-AST-SCOPE-01`, `BR-AST-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo tài sản» được lưu nhất quán trong module `AST`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo tài sản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Asset Accountant mở «Xuất báo cáo tài sản», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo tài sản» (Export asset reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo tài sản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-AST-01 — Ghi tăng → khấu hao → thanh lý

**Mục tiêu:** Vòng đời tài sản đầy đủ

| Bước | Mô tả |
|---:|---|
| 1 | Ghi tăng từ mua/dự án; tạo thẻ TS |
| 2 | Gán vị trí/người giữ; cấu hình KH |
| 3 | Chạy khấu hao định kỳ; post FIN |
| 4 | Điều chuyển/kiểm kê khi cần |
| 5 | Thanh lý/ghi giảm có duyệt; cập nhật sổ |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `AssetCategory / Asset` | Danh mục & thẻ |
| `DepreciationPolicy / DepreciationEntry` | Khấu hao |
| `AssetTransfer / AssetDisposal` | Biến động |
| `AssetCountSession` | Kiểm kê |
| `ToolIssue` | Cấp phát CCDC |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `AST`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-AST-01: Không xóa thẻ TS đã khấu hao; chỉ ghi giảm.
- BR-AST-02: Khấu hao không vượt nguyên giá (trừ policy đặc biệt).
- BR-AST-03: Thanh lý phải có phê duyệt và chứng từ.
- BR-AST-04: Điều chuyển đổi custodian phải ghi nhận ngày hiệu lực.
- BR-AST-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-AST-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-AST-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Batch depreciation | Chạy KH hàng tháng theo job |
| Label | In tem mã TS |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `AST` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `ast.asset.manage` | Quyền chức năng module |
| `ast.depreciation.run` | Quyền chức năng module |
| `ast.transfer.manage` | Quyền chức năng module |
| `ast.dispose.approve` | Quyền chức năng module |
| `ast.count.manage` | Quyền chức năng module |
| `ast.report.view` | Quyền chức năng module |
| `ast.*.view` | Xem trong data scope |
| `ast.*.manage` | Tạo/sửa trong data scope |
| `ast.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| NBV by category | Theo dõi vận hành module |
| Assets missing on count | Theo dõi vận hành module |
| Depreciation by period | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Phương pháp KH cấu hình theo nhóm TS.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có quản lý tài sản thuê tài chính phase 1?

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
| Bản SRS này | `SRS_AST_v1.1.md` / `.docx` |
| UC IDs | `UC_AST_001` … |

---

*Hết tài liệu SRS-AST-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
