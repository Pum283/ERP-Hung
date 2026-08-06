# SRS-PRT-v1.1 — Cổng khách hàng / đối tác (Portal)

> **Software Requirements Specification — Module PRT**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-PRT-v1.1` |
| Module | `PRT` — Cổng khách hàng / đối tác (Portal) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Bán hàng & Khách hàng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `CRM`, `LOG`, `FIN`, `FSM` |
| Số nhóm / UC | 7 nhóm / 38 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Cổng khách hàng / đối tác (Portal)** (`PRT`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Cổng tự phục vụ cho khách hàng (và tùy gói: NCC/đại lý): tài khoản, đơn hàng, công nợ, ticket hỗ trợ/bảo hành, tài liệu và cấu hình tính năng theo gói.

### 1.3. Mục tiêu đo được
1. Giảm tải CSKH/Sales Admin cho tra cứu đơn và công nợ.
2. Cho phép tạo/theo dõi ticket dịch vụ.
3. Mở rộng B2B self-service theo gói bán.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Portal auth, customer orders/shipments, AR view/pay framework, tickets, content, vendor/dealer portal optional, portal admin.

### 2.2. Out of Scope
- CMS marketing website đầy đủ.
- Marketplace đa vendor.

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `CRM`, `LOG`, `FIN`, `FSM`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Customer User | Liên hệ phía KH |
| Customer Admin | Quản lý nhiều liên hệ DN |
| Vendor User | Người dùng NCC (tùy gói) |
| Portal Admin | Cấu hình nội dung & feature flags |
| Internal Ops | Xử lý yêu cầu phát sinh từ portal |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Self-service | Khách tự thao tác không qua nội bộ |
| Feature flag | Bật/tắt tính năng portal theo gói |
| Dealer portal | Cổng đại lý B2B |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- PRT (Cổng khách hàng / đối tác (Portal))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `PRT` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `PRT` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM | KH, đơn, case |
| Tích hợp | LOG | Tracking |
| Tích hợp | FIN | AR/thanh toán |
| Tích hợp | FSM | Ticket BH |
| Tích hợp | PUR | PO phía NCC |
| Tích hợp | SYS | Auth/file/thông báo |

---

## 6. Catalog chức năng

**Tổng:** 7 nhóm · 38 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `PRT-01` | Tài khoản portal | 6 | 3 | 3 | 0 |
| 2 | `PRT-02` | Đơn hàng & giao nhận (khách hàng) | 7 | 2 | 3 | 2 |
| 3 | `PRT-03` | Công nợ & thanh toán (khách hàng) | 5 | 3 | 1 | 1 |
| 4 | `PRT-04` | Hỗ trợ & bảo hành (khách hàng) | 6 | 2 | 2 | 2 |
| 5 | `PRT-05` | Kiến thức & tài liệu khách hàng | 4 | 0 | 1 | 3 |
| 6 | `PRT-06` | Portal nhà cung cấp / đối tác | 6 | 0 | 0 | 6 |
| 7 | `PRT-07` | Báo cáo & quản trị portal | 4 | 1 | 2 | 1 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_PRT_001` | Tài khoản portal | Đăng ký tài khoản khách hàng | Must |
| `UC_PRT_002` | Tài khoản portal | Đăng nhập / quên mật khẩu | Must |
| `UC_PRT_003` | Tài khoản portal | Liên kết tài khoản với mã khách | Must |
| `UC_PRT_004` | Tài khoản portal | Quản lý nhiều liên hệ | Should |
| `UC_PRT_005` | Tài khoản portal | Phân quyền liên hệ | Should |
| `UC_PRT_006` | Tài khoản portal | Xác thực email/SĐT | Should |
| `UC_PRT_007` | Đơn hàng & giao nhận (khách hàng) | Xem danh sách đơn hàng | Must |
| `UC_PRT_008` | Đơn hàng & giao nhận (khách hàng) | Xem chi tiết & trạng thái đơn | Must |
| `UC_PRT_009` | Đơn hàng & giao nhận (khách hàng) | Theo dõi vận đơn | Should |
| `UC_PRT_010` | Đơn hàng & giao nhận (khách hàng) | Tải hóa đơn / biên bản | Should |
| `UC_PRT_011` | Đơn hàng & giao nhận (khách hàng) | Yêu cầu trả hàng / khiếu nại | Should |
| `UC_PRT_012` | Đơn hàng & giao nhận (khách hàng) | Đặt hàng lại | Could |
| `UC_PRT_013` | Đơn hàng & giao nhận (khách hàng) | Tạo yêu cầu báo giá | Later |
| `UC_PRT_014` | Công nợ & thanh toán (khách hàng) | Xem công nợ hiện tại | Must |
| `UC_PRT_015` | Công nợ & thanh toán (khách hàng) | Xem bảng kê hóa đơn chưa thanh toán | Must |
| `UC_PRT_016` | Công nợ & thanh toán (khách hàng) | Lịch sử thanh toán | Must |
| `UC_PRT_017` | Công nợ & thanh toán (khách hàng) | Thanh toán online | Could |
| `UC_PRT_018` | Công nợ & thanh toán (khách hàng) | Đối chiếu sao kê | Should |
| `UC_PRT_019` | Hỗ trợ & bảo hành (khách hàng) | Tạo ticket hỗ trợ | Must |
| `UC_PRT_020` | Hỗ trợ & bảo hành (khách hàng) | Xem trạng thái ticket | Must |
| `UC_PRT_021` | Hỗ trợ & bảo hành (khách hàng) | Trao đổi / gửi ảnh | Should |
| `UC_PRT_022` | Hỗ trợ & bảo hành (khách hàng) | Xem thiết bị đã mua | Should |
| `UC_PRT_023` | Hỗ trợ & bảo hành (khách hàng) | Đặt lịch bảo trì | Could |
| `UC_PRT_024` | Hỗ trợ & bảo hành (khách hàng) | Đánh giá dịch vụ | Could |
| `UC_PRT_025` | Kiến thức & tài liệu khách hàng | Xem catalogue / bảng giá | Could |
| `UC_PRT_026` | Kiến thức & tài liệu khách hàng | Tải tài liệu kỹ thuật | Should |
| `UC_PRT_027` | Kiến thức & tài liệu khách hàng | Thông báo từ nhà cung cấp | Could |
| `UC_PRT_028` | Kiến thức & tài liệu khách hàng | Đăng ký nhận bản tin | Later |
| `UC_PRT_029` | Portal nhà cung cấp / đối tác | Đăng nhập portal nhà cung cấp | Could |
| `UC_PRT_030` | Portal nhà cung cấp / đối tác | Xem PO được gửi | Could |
| `UC_PRT_031` | Portal nhà cung cấp / đối tác | Xác nhận PO / ngày giao | Could |
| `UC_PRT_032` | Portal nhà cung cấp / đối tác | Gửi thông báo sẵn sàng giao | Later |
| `UC_PRT_033` | Portal nhà cung cấp / đối tác | Xem công nợ phía nhà cung cấp | Later |
| `UC_PRT_034` | Portal nhà cung cấp / đối tác | Portal đại lý | Later |
| `UC_PRT_035` | Báo cáo & quản trị portal | Thống kê lượt dùng portal | Should |
| `UC_PRT_036` | Báo cáo & quản trị portal | Quản trị nội dung portal | Could |
| `UC_PRT_037` | Báo cáo & quản trị portal | Cấu hình module portal theo gói | Must |
| `UC_PRT_038` | Báo cáo & quản trị portal | Nhật ký thao tác phía portal | Should |

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

### 7.1. Tài khoản portal (`PRT-01`)

Nhóm **Tài khoản portal** gồm **6** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 1. Đặc tả Use Case "Đăng ký tài khoản khách hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_001 |
| **Tên Use Case** | Đăng ký tài khoản khách hàng |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Đăng ký tài khoản khách hàng" thuộc nhóm Tài khoản portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Customer signup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng ký tài khoản khách hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng ký tài khoản khách hàng» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng ký tài khoản khách hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Đăng ký tài khoản khách hàng» trong nhóm Tài khoản portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer signup).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng ký tài khoản khách hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng ký tài khoản khách hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Đăng nhập / quên mật khẩu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_002 |
| **Tên Use Case** | Đăng nhập / quên mật khẩu |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Đăng nhập / quên mật khẩu" thuộc nhóm Tài khoản portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Portal authentication |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [Portal Admin] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng nhập / quên mật khẩu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng nhập / quên mật khẩu» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng nhập / quên mật khẩu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Đăng nhập / quên mật khẩu» trong nhóm Tài khoản portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Portal authentication).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng nhập / quên mật khẩu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng nhập / quên mật khẩu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Liên kết tài khoản với mã khách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_003 |
| **Tên Use Case** | Liên kết tài khoản với mã khách |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Liên kết tài khoản với mã khách" thuộc nhóm Tài khoản portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Link customer account |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Liên kết tài khoản với mã khách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Liên kết tài khoản với mã khách» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Liên kết tài khoản với mã khách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Liên kết tài khoản với mã khách» trong nhóm Tài khoản portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Link customer account).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Liên kết tài khoản với mã khách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Liên kết tài khoản với mã khách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Quản lý nhiều liên hệ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_004 |
| **Tên Use Case** | Quản lý nhiều liên hệ |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Quản lý nhiều liên hệ" thuộc nhóm Tài khoản portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Multi-contact management |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý nhiều liên hệ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý nhiều liên hệ» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý nhiều liên hệ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Portal Admin mở danh mục quản lý «Quản lý nhiều liên hệ» (cổng khách hàng / self-service; nhóm «Tài khoản portal»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý nhiều liên hệ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 5. Đặc tả Use Case "Phân quyền liên hệ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_005 |
| **Tên Use Case** | Phân quyền liên hệ |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Phân quyền liên hệ" thuộc nhóm Tài khoản portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Contact permissions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân quyền liên hệ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân quyền liên hệ» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân quyền liên hệ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Phân quyền liên hệ» trong nhóm Tài khoản portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Contact permissions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân quyền liên hệ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân quyền liên hệ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 6. Đặc tả Use Case "Xác thực email/SĐT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_006 |
| **Tên Use Case** | Xác thực email/SĐT |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Xác thực email/SĐT" thuộc nhóm Tài khoản portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Contact verification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác thực email/SĐT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác thực email/SĐT» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác thực email/SĐT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Xác thực email/SĐT» trong nhóm Tài khoản portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Contact verification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác thực email/SĐT».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác thực email/SĐT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Đơn hàng & giao nhận (khách hàng) (`PRT-02`)

Nhóm **Đơn hàng & giao nhận (khách hàng)** gồm **7** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 2 |

**Bảng 7. Đặc tả Use Case "Xem danh sách đơn hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_007 |
| **Tên Use Case** | Xem danh sách đơn hàng |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem danh sách đơn hàng" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: My orders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem danh sách đơn hàng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem danh sách đơn hàng» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem danh sách đơn hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở «Xem danh sách đơn hàng» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (My orders).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem danh sách đơn hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Xem chi tiết & trạng thái đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_008 |
| **Tên Use Case** | Xem chi tiết & trạng thái đơn |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem chi tiết & trạng thái đơn" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Order detail & status |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem chi tiết & trạng thái đơn» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem chi tiết & trạng thái đơn» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem chi tiết & trạng thái đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở «Xem chi tiết & trạng thái đơn» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Order detail & status).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem chi tiết & trạng thái đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Theo dõi vận đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_009 |
| **Tên Use Case** | Theo dõi vận đơn |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Theo dõi vận đơn" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Shipment tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi vận đơn» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi vận đơn» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi vận đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Theo dõi vận đơn» trong nhóm Đơn hàng & giao nhận (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Shipment tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi vận đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi vận đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Tải hóa đơn / biên bản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_010 |
| **Tên Use Case** | Tải hóa đơn / biên bản |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Tải hóa đơn / biên bản" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Download documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tải hóa đơn / biên bản» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tải hóa đơn / biên bản» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tải hóa đơn / biên bản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Tải hóa đơn / biên bản» trong nhóm Đơn hàng & giao nhận (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Download documents).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tải hóa đơn / biên bản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tải hóa đơn / biên bản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "Yêu cầu trả hàng / khiếu nại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_011 |
| **Tên Use Case** | Yêu cầu trả hàng / khiếu nại |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Yêu cầu trả hàng / khiếu nại" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Return/complaint request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Yêu cầu trả hàng / khiếu nại» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Yêu cầu trả hàng / khiếu nại» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Yêu cầu trả hàng / khiếu nại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Yêu cầu trả hàng / khiếu nại» trong nhóm Đơn hàng & giao nhận (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Return/complaint request).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Yêu cầu trả hàng / khiếu nại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Yêu cầu trả hàng / khiếu nại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Đặt hàng lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_012 |
| **Tên Use Case** | Đặt hàng lại |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Đặt hàng lại" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Reorder |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đặt hàng lại» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đặt hàng lại» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đặt hàng lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Đặt hàng lại» trong nhóm Đơn hàng & giao nhận (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Reorder).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đặt hàng lại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đặt hàng lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 13. Đặc tả Use Case "Tạo yêu cầu báo giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_013 |
| **Tên Use Case** | Tạo yêu cầu báo giá |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Tạo yêu cầu báo giá" thuộc nhóm Đơn hàng & giao nhận (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: RFQ from portal |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo yêu cầu báo giá» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo yêu cầu báo giá» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo yêu cầu báo giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User mở chức năng «Tạo yêu cầu báo giá» trong nhóm Đơn hàng & giao nhận (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo yêu cầu báo giá» (RFQ from portal).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo yêu cầu báo giá» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo yêu cầu báo giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

### 7.3. Công nợ & thanh toán (khách hàng) (`PRT-03`)

Nhóm **Công nợ & thanh toán (khách hàng)** gồm **5** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 14. Đặc tả Use Case "Xem công nợ hiện tại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_014 |
| **Tên Use Case** | Xem công nợ hiện tại |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem công nợ hiện tại" thuộc nhóm Công nợ & thanh toán (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: My AR balance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem công nợ hiện tại» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem công nợ hiện tại» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem công nợ hiện tại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở «Xem công nợ hiện tại» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (My AR balance).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem công nợ hiện tại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Xem bảng kê hóa đơn chưa thanh toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_015 |
| **Tên Use Case** | Xem bảng kê hóa đơn chưa thanh toán |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem bảng kê hóa đơn chưa thanh toán" thuộc nhóm Công nợ & thanh toán (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Open invoices |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem bảng kê hóa đơn chưa thanh toán» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem bảng kê hóa đơn chưa thanh toán» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem bảng kê hóa đơn chưa thanh toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở «Xem bảng kê hóa đơn chưa thanh toán» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Open invoices).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem bảng kê hóa đơn chưa thanh toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Lịch sử thanh toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_016 |
| **Tên Use Case** | Lịch sử thanh toán |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Lịch sử thanh toán" thuộc nhóm Công nợ & thanh toán (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Payment history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử thanh toán» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử thanh toán» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử thanh toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở «Lịch sử thanh toán» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Payment history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử thanh toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Thanh toán online"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_017 |
| **Tên Use Case** | Thanh toán online |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Thanh toán online" thuộc nhóm Công nợ & thanh toán (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Online payment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh toán online» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`, `BR-PRT-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh toán online» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh toán online» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User chọn chứng từ cần thu/chi trong «Thanh toán online».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh toán online» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận. |

**Bảng 18. Đặc tả Use Case "Đối chiếu sao kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_018 |
| **Tên Use Case** | Đối chiếu sao kê |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Đối chiếu sao kê" thuộc nhóm Công nợ & thanh toán (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: AR statement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối chiếu sao kê» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối chiếu sao kê» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối chiếu sao kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Đối chiếu sao kê» trong nhóm Công nợ & thanh toán (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (AR statement).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối chiếu sao kê».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối chiếu sao kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Hỗ trợ & bảo hành (khách hàng) (`PRT-04`)

Nhóm **Hỗ trợ & bảo hành (khách hàng)** gồm **6** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 2 |

**Bảng 19. Đặc tả Use Case "Tạo ticket hỗ trợ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_019 |
| **Tên Use Case** | Tạo ticket hỗ trợ |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Tạo ticket hỗ trợ" thuộc nhóm Hỗ trợ & bảo hành (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Create support ticket |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo ticket hỗ trợ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo ticket hỗ trợ» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo ticket hỗ trợ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở chức năng «Tạo ticket hỗ trợ» trong nhóm Hỗ trợ & bảo hành (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo ticket hỗ trợ» (Create support ticket).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo ticket hỗ trợ» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo ticket hỗ trợ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 20. Đặc tả Use Case "Xem trạng thái ticket"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_020 |
| **Tên Use Case** | Xem trạng thái ticket |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem trạng thái ticket" thuộc nhóm Hỗ trợ & bảo hành (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Ticket status |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem trạng thái ticket» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem trạng thái ticket» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem trạng thái ticket» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Customer User mở «Xem trạng thái ticket» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Ticket status).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem trạng thái ticket» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Trao đổi / gửi ảnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_021 |
| **Tên Use Case** | Trao đổi / gửi ảnh |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Trao đổi / gửi ảnh" thuộc nhóm Hỗ trợ & bảo hành (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Ticket conversation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trao đổi / gửi ảnh» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trao đổi / gửi ảnh» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trao đổi / gửi ảnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User hoàn thiện dữ liệu cho «Trao đổi / gửi ảnh» ở trạng thái nháp.<br>2. Chọn [Gửi duyệt / Xác nhận] (submit).<br>3. Hệ thống validate đủ điều kiện gửi; chuyển trạng thái Submitted/In Approval.<br>4. Tạo việc duyệt (WF hoặc duyệt nội module); gửi thông báo.<br>5. Khóa sửa một phần theo policy khi đang chờ duyệt. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trao đổi / gửi ảnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 22. Đặc tả Use Case "Xem thiết bị đã mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_022 |
| **Tên Use Case** | Xem thiết bị đã mua |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem thiết bị đã mua" thuộc nhóm Hỗ trợ & bảo hành (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: My equipment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem thiết bị đã mua» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem thiết bị đã mua» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem thiết bị đã mua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User mở «Xem thiết bị đã mua» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (My equipment).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem thiết bị đã mua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Đặt lịch bảo trì"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_023 |
| **Tên Use Case** | Đặt lịch bảo trì |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Đặt lịch bảo trì" thuộc nhóm Hỗ trợ & bảo hành (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Book maintenance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đặt lịch bảo trì» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đặt lịch bảo trì» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đặt lịch bảo trì» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Đặt lịch bảo trì» trong nhóm Hỗ trợ & bảo hành (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Book maintenance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đặt lịch bảo trì».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đặt lịch bảo trì» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Đánh giá dịch vụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_024 |
| **Tên Use Case** | Đánh giá dịch vụ |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Đánh giá dịch vụ" thuộc nhóm Hỗ trợ & bảo hành (khách hàng) trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Rate service |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá dịch vụ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá dịch vụ» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá dịch vụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Đánh giá dịch vụ» trong nhóm Hỗ trợ & bảo hành (khách hàng).<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Rate service).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá dịch vụ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá dịch vụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Kiến thức & tài liệu khách hàng (`PRT-05`)

Nhóm **Kiến thức & tài liệu khách hàng** gồm **4** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 25. Đặc tả Use Case "Xem catalogue / bảng giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_025 |
| **Tên Use Case** | Xem catalogue / bảng giá |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Xem catalogue / bảng giá" thuộc nhóm Kiến thức & tài liệu khách hàng trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Product catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem catalogue / bảng giá» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem catalogue / bảng giá» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem catalogue / bảng giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User mở «Xem catalogue / bảng giá» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Product catalog).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem catalogue / bảng giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Tải tài liệu kỹ thuật"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_026 |
| **Tên Use Case** | Tải tài liệu kỹ thuật |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Tải tài liệu kỹ thuật" thuộc nhóm Kiến thức & tài liệu khách hàng trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Download documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tải tài liệu kỹ thuật» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tải tài liệu kỹ thuật» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tải tài liệu kỹ thuật» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Tải tài liệu kỹ thuật» trong nhóm Kiến thức & tài liệu khách hàng.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Download documents).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tải tài liệu kỹ thuật».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tải tài liệu kỹ thuật» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 27. Đặc tả Use Case "Thông báo từ nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_027 |
| **Tên Use Case** | Thông báo từ nhà cung cấp |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Thông báo từ nhà cung cấp" thuộc nhóm Kiến thức & tài liệu khách hàng trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Announcements |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thông báo từ nhà cung cấp» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thông báo từ nhà cung cấp» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thông báo từ nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Thông báo từ nhà cung cấp» trong nhóm Kiến thức & tài liệu khách hàng.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Announcements).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thông báo từ nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thông báo từ nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 28. Đặc tả Use Case "Đăng ký nhận bản tin"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_028 |
| **Tên Use Case** | Đăng ký nhận bản tin |
| **Tác nhân** | Customer User |
| **Mô tả chức năng** | Cho phép Customer User thực hiện chức năng "Đăng ký nhận bản tin" thuộc nhóm Kiến thức & tài liệu khách hàng trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Newsletter subscription |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Customer User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng ký nhận bản tin» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng ký nhận bản tin» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng ký nhận bản tin» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Customer User khởi tạo thao tác «Đăng ký nhận bản tin» trong nhóm Kiến thức & tài liệu khách hàng.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Newsletter subscription).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng ký nhận bản tin».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng ký nhận bản tin» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Portal nhà cung cấp / đối tác (`PRT-06`)

Nhóm **Portal nhà cung cấp / đối tác** gồm **6** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 0 |

**Bảng 29. Đặc tả Use Case "Đăng nhập portal nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_029 |
| **Tên Use Case** | Đăng nhập portal nhà cung cấp |
| **Tác nhân** | Vendor User |
| **Mô tả chức năng** | Cho phép Vendor User thực hiện chức năng "Đăng nhập portal nhà cung cấp" thuộc nhóm Portal nhà cung cấp / đối tác trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Vendor login |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [Vendor User] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng nhập portal nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng nhập portal nhà cung cấp» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng nhập portal nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Vendor User khởi tạo thao tác «Đăng nhập portal nhà cung cấp» trong nhóm Portal nhà cung cấp / đối tác.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor login).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng nhập portal nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng nhập portal nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Xem PO được gửi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_030 |
| **Tên Use Case** | Xem PO được gửi |
| **Tác nhân** | Vendor User |
| **Mô tả chức năng** | Cho phép Vendor User thực hiện chức năng "Xem PO được gửi" thuộc nhóm Portal nhà cung cấp / đối tác trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: View purchase orders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Vendor User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem PO được gửi» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem PO được gửi» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem PO được gửi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Vendor User mở «Xem PO được gửi» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (View purchase orders).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem PO được gửi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 31. Đặc tả Use Case "Xác nhận PO / ngày giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_031 |
| **Tên Use Case** | Xác nhận PO / ngày giao |
| **Tác nhân** | Vendor User |
| **Mô tả chức năng** | Cho phép Vendor User thực hiện chức năng "Xác nhận PO / ngày giao" thuộc nhóm Portal nhà cung cấp / đối tác trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: PO acknowledgment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Vendor User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận PO / ngày giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận PO / ngày giao» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận PO / ngày giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Vendor User khởi tạo thao tác «Xác nhận PO / ngày giao» trong nhóm Portal nhà cung cấp / đối tác.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (PO acknowledgment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận PO / ngày giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận PO / ngày giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 32. Đặc tả Use Case "Gửi thông báo sẵn sàng giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_032 |
| **Tên Use Case** | Gửi thông báo sẵn sàng giao |
| **Tác nhân** | Vendor User |
| **Mô tả chức năng** | Cho phép Vendor User thực hiện chức năng "Gửi thông báo sẵn sàng giao" thuộc nhóm Portal nhà cung cấp / đối tác trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: ASN notification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Vendor User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gửi thông báo sẵn sàng giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gửi thông báo sẵn sàng giao» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi thông báo sẵn sàng giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Vendor User hoàn thiện dữ liệu cho «Gửi thông báo sẵn sàng giao» ở trạng thái nháp.<br>2. Chọn [Gửi duyệt / Xác nhận] (submit).<br>3. Hệ thống validate đủ điều kiện gửi; chuyển trạng thái Submitted/In Approval.<br>4. Tạo việc duyệt (WF hoặc duyệt nội module); gửi thông báo.<br>5. Khóa sửa một phần theo policy khi đang chờ duyệt. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gửi thông báo sẵn sàng giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 33. Đặc tả Use Case "Xem công nợ phía nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_033 |
| **Tên Use Case** | Xem công nợ phía nhà cung cấp |
| **Tác nhân** | Vendor User |
| **Mô tả chức năng** | Cho phép Vendor User thực hiện chức năng "Xem công nợ phía nhà cung cấp" thuộc nhóm Portal nhà cung cấp / đối tác trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Vendor AP view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Vendor User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem công nợ phía nhà cung cấp» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem công nợ phía nhà cung cấp» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem công nợ phía nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Vendor User mở «Xem công nợ phía nhà cung cấp» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Vendor AP view).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem công nợ phía nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 34. Đặc tả Use Case "Portal đại lý"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_034 |
| **Tên Use Case** | Portal đại lý |
| **Tác nhân** | Vendor User |
| **Mô tả chức năng** | Cho phép Vendor User thực hiện chức năng "Portal đại lý" thuộc nhóm Portal nhà cung cấp / đối tác trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Dealer portal |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Vendor User] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Portal đại lý» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Portal đại lý» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Portal đại lý» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Vendor User khởi tạo thao tác «Portal đại lý» trong nhóm Portal nhà cung cấp / đối tác.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Dealer portal).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Portal đại lý».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Portal đại lý» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Báo cáo & quản trị portal (`PRT-07`)

Nhóm **Báo cáo & quản trị portal** gồm **4** use case của module `PRT`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 1 |

**Bảng 35. Đặc tả Use Case "Thống kê lượt dùng portal"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_035 |
| **Tên Use Case** | Thống kê lượt dùng portal |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Thống kê lượt dùng portal" thuộc nhóm Báo cáo & quản trị portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Usage statistics |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thống kê lượt dùng portal» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thống kê lượt dùng portal» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thống kê lượt dùng portal» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Thống kê lượt dùng portal» trong nhóm Báo cáo & quản trị portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Usage statistics).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thống kê lượt dùng portal».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thống kê lượt dùng portal» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Quản trị nội dung portal"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_036 |
| **Tên Use Case** | Quản trị nội dung portal |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Quản trị nội dung portal" thuộc nhóm Báo cáo & quản trị portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: CMS light |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản trị nội dung portal» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản trị nội dung portal» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản trị nội dung portal» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Portal Admin mở danh mục quản lý «Quản trị nội dung portal» (cổng khách hàng / self-service; nhóm «Báo cáo & quản trị portal»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản trị nội dung portal» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 37. Đặc tả Use Case "Cấu hình module portal theo gói"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_037 |
| **Tên Use Case** | Cấu hình module portal theo gói |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Cấu hình module portal theo gói" thuộc nhóm Báo cáo & quản trị portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Portal features by plan |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình module portal theo gói» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình module portal theo gói» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình module portal theo gói» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Portal Admin mở màn hình cấu hình «Cấu hình module portal theo gói» trong Báo cáo & quản trị portal.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Portal features by plan) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình module portal theo gói» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 38. Đặc tả Use Case "Nhật ký thao tác phía portal"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PRT_038 |
| **Tên Use Case** | Nhật ký thao tác phía portal |
| **Tác nhân** | Portal Admin |
| **Mô tả chức năng** | Cho phép Portal Admin thực hiện chức năng "Nhật ký thao tác phía portal" thuộc nhóm Báo cáo & quản trị portal trong module PRT — Cổng khách hàng / đối tác (Portal). Mô tả chi tiết: Portal audit log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Portal Admin] và được cấp quyền RBAC tương ứng.<br>• License module `PRT` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhật ký thao tác phía portal» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PRT-SCOPE-01`, `BR-PRT-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhật ký thao tác phía portal» được lưu nhất quán trong module `PRT`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhật ký thao tác phía portal» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Portal Admin khởi tạo thao tác «Nhật ký thao tác phía portal» trong nhóm Báo cáo & quản trị portal.<br>2. Hệ thống kiểm tra license `PRT`, quyền RBAC và tiền điều kiện nghiệp vụ (Portal audit log).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhật ký thao tác phía portal».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhật ký thao tác phía portal» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-PRT-01 — KH theo dõi đơn và tạo ticket

**Mục tiêu:** Tự phục vụ sau bán

| Bước | Mô tả |
|---:|---|
| 1 | Đăng ký/liên kết tài khoản với mã KH |
| 2 | Xem đơn/vận đơn/hóa đơn |
| 3 | Tạo ticket BH/hỗ trợ; theo dõi trạng thái |
| 4 | Đánh giá dịch vụ sau đóng ticket |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `PortalAccount / PortalContact` | Tài khoản cổng |
| `PortalFeatureConfig` | Tính năng theo gói |
| `PortalTicket` | Ticket từ cổng (map FSM/CRM) |
| `PortalContent` | Nội dung/catalogue |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `PRT`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-PRT-01: User portal chỉ thấy dữ liệu KH/NCC được liên kết.
- BR-PRT-02: Tính năng hiển thị theo feature flag gói.
- BR-PRT-03: Thao tác tạo ticket/đơn phải rate-limit và audit.
- BR-PRT-04: Thanh toán online (nếu bật) phải đối soát với FIN.
- BR-PRT-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-PRT-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-PRT-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| UX | Mobile-friendly |
| Bảo mật | Tách bề mặt tấn công khỏi back-office; 2FA tùy cấu hình |
| Uptime | Cổng public cần HA riêng khi production |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `PRT` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `prt.account.manage` | Quyền chức năng module |
| `prt.order.view` | Quyền chức năng module |
| `prt.ar.view` | Quyền chức năng module |
| `prt.ticket.create` | Quyền chức năng module |
| `prt.content.manage` | Quyền chức năng module |
| `prt.vendor.access` | Quyền chức năng module |
| `prt.*.view` | Xem trong data scope |
| `prt.*.manage` | Tạo/sửa trong data scope |
| `prt.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Portal MAU | Theo dõi vận hành module |
| % ticket tạo từ portal | Theo dõi vận hành module |
| % tra cứu đơn self-service | Theo dõi vận hành module |
| CSAT portal | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Mỗi Customer User map 1–n Contact thuộc 1 Customer CRM.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Đăng ký tự do hay chỉ invite từ nội bộ phase 1?
2. Dealer portal có giá/tồn realtime không?

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
| Bản SRS này | `SRS_PRT_v1.1.md` / `.docx` |
| UC IDs | `UC_PRT_001` … |

---

*Hết tài liệu SRS-PRT-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
