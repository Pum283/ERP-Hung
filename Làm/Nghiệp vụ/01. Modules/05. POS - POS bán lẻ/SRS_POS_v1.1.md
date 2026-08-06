# SRS-POS-v1.1 — POS bán lẻ

> **Software Requirements Specification — Module POS**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-POS-v1.1` |
| Module | `POS` — POS bán lẻ |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Bán hàng & Khách hàng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `INV`, `FIN`, `CRM` |
| Số nhóm / UC | 10 nhóm / 72 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **POS bán lẻ** (`POS`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module POS phục vụ bán tại điểm bán: danh mục hàng/món, giá, khuyến mại, thanh toán, ca thu ngân, đồng bộ tồn và doanh thu, báo cáo cửa hàng/chuỗi.

### 1.3. Mục tiêu đo được
1. Thanh toán nhanh, kiểm soát quỹ ca.
2. Đồng bộ tồn/định mức với kho.
3. Chuẩn hóa menu/giá/KM toàn chuỗi.
4. Cung cấp số liệu doanh thu realtime cho quản lý.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Store/terminal config, sellable catalog, pricing, promo at POS, checkout, shift/cash, loyalty light, stock sync, POS reports, chain push config.

### 2.2. Out of Scope
- CRM pipeline đầy đủ.
- Kế toán sổ cái (FIN).
- WMS chi tiết vị trí kho tổng (INV).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `INV`, `FIN`, `CRM`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Cashier | Thu ngân |
| Store Manager | Duyệt giảm giá/hủy, đóng ca, báo cáo CH |
| Chain Admin | Đẩy menu/giá/KM xuống điểm bán |
| Inventory Clerk (CH) | Nhận hàng/kiểm kê nhanh tại CH |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Terminal | Máy POS tại quầy |
| Shift | Ca thu ngân |
| BOM/Recipe | Định mức nguyên liệu gắn SP/món |
| Void | Hủy món/bill có kiểm soát |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- POS (POS bán lẻ)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `POS` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `POS` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | INV | Trừ tồn, nhận hàng CH, kiểm kê nhanh |
| Tích hợp | FIN | Post doanh thu/ca/quỹ |
| Tích hợp | CRM | KM, loyalty, gắn KH |
| Tích hợp | Payment QR/Card | Cổng thanh toán |

---

## 6. Catalog chức năng

**Tổng:** 10 nhóm · 72 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `POS-01` | Cấu hình điểm bán & thiết bị | 8 | 4 | 3 | 1 |
| 2 | `POS-02` | Catalog bán hàng | 7 | 5 | 2 | 0 |
| 3 | `POS-03` | Bảng giá & thuế | 5 | 2 | 2 | 1 |
| 4 | `POS-04` | Khuyến mại tại quầy | 5 | 3 | 2 | 0 |
| 5 | `POS-05` | Giao dịch bán hàng | 16 | 10 | 5 | 1 |
| 6 | `POS-06` | Ca thu ngân & quỹ | 8 | 6 | 2 | 0 |
| 7 | `POS-07` | Khách hàng & loyalty | 4 | 0 | 2 | 2 |
| 8 | `POS-08` | Đồng bộ tồn & back-office | 7 | 3 | 3 | 1 |
| 9 | `POS-09` | Báo cáo POS | 8 | 5 | 3 | 0 |
| 10 | `POS-10` | Vận hành chuỗi | 4 | 2 | 2 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_POS_001` | Cấu hình điểm bán & thiết bị | Khai báo điểm bán POS | Must |
| `UC_POS_002` | Cấu hình điểm bán & thiết bị | Khai báo quầy / máy POS | Must |
| `UC_POS_003` | Cấu hình điểm bán & thiết bị | Cấu hình máy in hóa đơn | Must |
| `UC_POS_004` | Cấu hình điểm bán & thiết bị | Cấu hình máy in bếp/khu vực | Should |
| `UC_POS_005` | Cấu hình điểm bán & thiết bị | Cấu hình ngăn kéo tiền | Should |
| `UC_POS_006` | Cấu hình điểm bán & thiết bị | Cấu hình thiết bị quét mã | Should |
| `UC_POS_007` | Cấu hình điểm bán & thiết bị | Phân quyền thu ngân trên POS | Must |
| `UC_POS_008` | Cấu hình điểm bán & thiết bị | Chế độ offline tạm | Later |
| `UC_POS_009` | Catalog bán hàng | Danh mục nhóm sản phẩm | Must |
| `UC_POS_010` | Catalog bán hàng | Danh mục sản phẩm bán | Must |
| `UC_POS_011` | Catalog bán hàng | Thuộc tính sản phẩm | Should |
| `UC_POS_012` | Catalog bán hàng | BOM / định mức nguyên liệu | Must |
| `UC_POS_013` | Catalog bán hàng | Ảnh sản phẩm / thứ tự hiển thị | Should |
| `UC_POS_014` | Catalog bán hàng | Ngưng bán sản phẩm tạm thời | Must |
| `UC_POS_015` | Catalog bán hàng | Đồng bộ catalog từ back-office | Must |
| `UC_POS_016` | Bảng giá & thuế | Bảng giá theo điểm bán | Must |
| `UC_POS_017` | Bảng giá & thuế | Giá theo khung giờ | Should |
| `UC_POS_018` | Bảng giá & thuế | Giá theo ngày trong tuần | Could |
| `UC_POS_019` | Bảng giá & thuế | Cấu hình thuế GTGT | Must |
| `UC_POS_020` | Bảng giá & thuế | Làm tròn tiền | Should |
| `UC_POS_021` | Khuyến mại tại quầy | Áp dụng chương trình khuyến mại | Must |
| `UC_POS_022` | Khuyến mại tại quầy | Nhập mã voucher | Must |
| `UC_POS_023` | Khuyến mại tại quầy | Khuyến mại theo combo | Should |
| `UC_POS_024` | Khuyến mại tại quầy | Giảm giá tay có quyền | Must |
| `UC_POS_025` | Khuyến mại tại quầy | Báo cáo khuyến mại | Should |
| `UC_POS_026` | Giao dịch bán hàng | Mở đơn / chọn khu vực | Must |
| `UC_POS_027` | Giao dịch bán hàng | Thêm / sửa / xóa sản phẩm | Must |
| `UC_POS_028` | Giao dịch bán hàng | Tách bill / gộp bill | Should |
| `UC_POS_029` | Giao dịch bán hàng | Chuyển đơn giữa quầy | Should |
| `UC_POS_030` | Giao dịch bán hàng | Ghi chú đơn hàng | Should |
| `UC_POS_031` | Giao dịch bán hàng | Gửi lệnh khu vực chế biến | Should |
| `UC_POS_032` | Giao dịch bán hàng | Tạm tính / giữ đơn | Must |
| `UC_POS_033` | Giao dịch bán hàng | Thanh toán tiền mặt | Must |
| `UC_POS_034` | Giao dịch bán hàng | Thanh toán chuyển khoản / QR | Must |
| `UC_POS_035` | Giao dịch bán hàng | Thanh toán thẻ / ví điện tử | Must |
| `UC_POS_036` | Giao dịch bán hàng | Thanh toán hỗn hợp | Should |
| `UC_POS_037` | Giao dịch bán hàng | In hóa đơn | Must |
| `UC_POS_038` | Giao dịch bán hàng | Hủy sản phẩm | Must |
| `UC_POS_039` | Giao dịch bán hàng | Hủy cả bill | Must |
| `UC_POS_040` | Giao dịch bán hàng | Trả hàng / hoàn tiền | Must |
| `UC_POS_041` | Giao dịch bán hàng | Gợi ý bán kèm | Later |
| `UC_POS_042` | Ca thu ngân & quỹ | Mở ca thu ngân | Must |
| `UC_POS_043` | Ca thu ngân & quỹ | Nhập tiền đầu ca | Must |
| `UC_POS_044` | Ca thu ngân & quỹ | Nộp tiền / rút tiền ca | Should |
| `UC_POS_045` | Ca thu ngân & quỹ | Xem doanh thu trong ca | Must |
| `UC_POS_046` | Ca thu ngân & quỹ | Đóng ca & đếm quỹ | Must |
| `UC_POS_047` | Ca thu ngân & quỹ | Đối soát lệch quỹ | Must |
| `UC_POS_048` | Ca thu ngân & quỹ | In báo cáo ca | Must |
| `UC_POS_049` | Ca thu ngân & quỹ | Duyệt xác nhận ca | Should |
| `UC_POS_050` | Khách hàng & loyalty | Gắn khách hàng vào đơn | Should |
| `UC_POS_051` | Khách hàng & loyalty | Tích điểm loyalty | Could |
| `UC_POS_052` | Khách hàng & loyalty | Đổi điểm / ưu đãi | Could |
| `UC_POS_053` | Khách hàng & loyalty | Tra cứu lịch sử mua | Should |
| `UC_POS_054` | Đồng bộ tồn & back-office | Trừ tồn theo BOM khi bán | Must |
| `UC_POS_055` | Đồng bộ tồn & back-office | Cảnh báo hết / sắp hết | Must |
| `UC_POS_056` | Đồng bộ tồn & back-office | Tạo đề nghị nhập hàng | Should |
| `UC_POS_057` | Đồng bộ tồn & back-office | Nhận hàng từ kho trung tâm | Should |
| `UC_POS_058` | Đồng bộ tồn & back-office | Kiểm kê nhanh | Should |
| `UC_POS_059` | Đồng bộ tồn & back-office | Đồng bộ doanh thu ca sang FIN | Must |
| `UC_POS_060` | Đồng bộ tồn & back-office | Đồng bộ đơn sang CRM | Could |
| `UC_POS_061` | Báo cáo POS | Doanh thu theo giờ / ngày / ca | Must |
| `UC_POS_062` | Báo cáo POS | Doanh thu theo sản phẩm | Must |
| `UC_POS_063` | Báo cáo POS | Doanh thu theo thu ngân | Must |
| `UC_POS_064` | Báo cáo POS | Tỷ lệ hủy / giảm giá | Must |
| `UC_POS_065` | Báo cáo POS | Cost lý thuyết vs thực tế | Should |
| `UC_POS_066` | Báo cáo POS | Top sản phẩm bán chạy | Should |
| `UC_POS_067` | Báo cáo POS | So sánh điểm bán | Should |
| `UC_POS_068` | Báo cáo POS | Xuất báo cáo POS | Must |
| `UC_POS_069` | Vận hành chuỗi | Giám sát doanh thu chuỗi realtime | Should |
| `UC_POS_070` | Vận hành chuỗi | Phân phối catalog / giá / khuyến mại | Must |
| `UC_POS_071` | Vận hành chuỗi | Chuẩn hóa catalog toàn chuỗi | Must |
| `UC_POS_072` | Vận hành chuỗi | Cấu hình target doanh thu | Should |

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

### 7.1. Cấu hình điểm bán & thiết bị (`POS-01`)

Nhóm **Cấu hình điểm bán & thiết bị** gồm **8** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 4 |

**Bảng 1. Đặc tả Use Case "Khai báo điểm bán POS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_001 |
| **Tên Use Case** | Khai báo điểm bán POS |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Khai báo điểm bán POS" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: POS location setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khai báo điểm bán POS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khai báo điểm bán POS» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khai báo điểm bán POS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Khai báo điểm bán POS» trong Cấu hình điểm bán & thiết bị.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (POS location setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khai báo điểm bán POS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 2. Đặc tả Use Case "Khai báo quầy / máy POS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_002 |
| **Tên Use Case** | Khai báo quầy / máy POS |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Khai báo quầy / máy POS" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: Terminal configuration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khai báo quầy / máy POS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khai báo quầy / máy POS» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khai báo quầy / máy POS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Khai báo quầy / máy POS» trong Cấu hình điểm bán & thiết bị.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Terminal configuration) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khai báo quầy / máy POS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 3. Đặc tả Use Case "Cấu hình máy in hóa đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_003 |
| **Tên Use Case** | Cấu hình máy in hóa đơn |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Cấu hình máy in hóa đơn" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: Receipt printer setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình máy in hóa đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình máy in hóa đơn» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình máy in hóa đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Cấu hình máy in hóa đơn» trong Cấu hình điểm bán & thiết bị.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Receipt printer setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình máy in hóa đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 4. Đặc tả Use Case "Cấu hình máy in bếp/khu vực"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_004 |
| **Tên Use Case** | Cấu hình máy in bếp/khu vực |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Cấu hình máy in bếp/khu vực" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: Kitchen printer setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình máy in bếp/khu vực» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình máy in bếp/khu vực» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình máy in bếp/khu vực» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Cấu hình máy in bếp/khu vực» trong Cấu hình điểm bán & thiết bị.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Kitchen printer setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình máy in bếp/khu vực» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 5. Đặc tả Use Case "Cấu hình ngăn kéo tiền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_005 |
| **Tên Use Case** | Cấu hình ngăn kéo tiền |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Cấu hình ngăn kéo tiền" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: Cash drawer setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình ngăn kéo tiền» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình ngăn kéo tiền» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình ngăn kéo tiền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Cấu hình ngăn kéo tiền» trong Cấu hình điểm bán & thiết bị.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Cash drawer setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình ngăn kéo tiền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 6. Đặc tả Use Case "Cấu hình thiết bị quét mã"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_006 |
| **Tên Use Case** | Cấu hình thiết bị quét mã |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Cấu hình thiết bị quét mã" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: Barcode scanner setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thiết bị quét mã» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thiết bị quét mã» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thiết bị quét mã» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Cấu hình thiết bị quét mã» trong Cấu hình điểm bán & thiết bị.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Barcode scanner setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thiết bị quét mã» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 7. Đặc tả Use Case "Phân quyền thu ngân trên POS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_007 |
| **Tên Use Case** | Phân quyền thu ngân trên POS |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Phân quyền thu ngân trên POS" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: POS user roles |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân quyền thu ngân trên POS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân quyền thu ngân trên POS» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân quyền thu ngân trên POS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Phân quyền thu ngân trên POS» trong nhóm Cấu hình điểm bán & thiết bị.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (POS user roles).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân quyền thu ngân trên POS».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân quyền thu ngân trên POS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 8. Đặc tả Use Case "Chế độ offline tạm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_008 |
| **Tên Use Case** | Chế độ offline tạm |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Chế độ offline tạm" thuộc nhóm Cấu hình điểm bán & thiết bị trong module POS — POS bán lẻ. Mô tả chi tiết: Offline mode |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chế độ offline tạm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chế độ offline tạm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chế độ offline tạm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Chế độ offline tạm» trong nhóm Cấu hình điểm bán & thiết bị.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Offline mode).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chế độ offline tạm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chế độ offline tạm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.2. Catalog bán hàng (`POS-02`)

Nhóm **Catalog bán hàng** gồm **7** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 9. Đặc tả Use Case "Danh mục nhóm sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_009 |
| **Tên Use Case** | Danh mục nhóm sản phẩm |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Danh mục nhóm sản phẩm" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Product categories |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục nhóm sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục nhóm sản phẩm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục nhóm sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Danh mục nhóm sản phẩm» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Product categories).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục nhóm sản phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục nhóm sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 10. Đặc tả Use Case "Danh mục sản phẩm bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_010 |
| **Tên Use Case** | Danh mục sản phẩm bán |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Danh mục sản phẩm bán" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Sellable items |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục sản phẩm bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục sản phẩm bán» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục sản phẩm bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Danh mục sản phẩm bán» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Sellable items).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục sản phẩm bán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục sản phẩm bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 11. Đặc tả Use Case "Thuộc tính sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_011 |
| **Tên Use Case** | Thuộc tính sản phẩm |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Thuộc tính sản phẩm" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Product variants/modifiers |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thuộc tính sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thuộc tính sản phẩm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thuộc tính sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Thuộc tính sản phẩm» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Product variants/modifiers).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thuộc tính sản phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thuộc tính sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 12. Đặc tả Use Case "BOM / định mức nguyên liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_012 |
| **Tên Use Case** | BOM / định mức nguyên liệu |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "BOM / định mức nguyên liệu" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Product recipe |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «BOM / định mức nguyên liệu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «BOM / định mức nguyên liệu» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «BOM / định mức nguyên liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «BOM / định mức nguyên liệu» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Product recipe).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «BOM / định mức nguyên liệu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «BOM / định mức nguyên liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 13. Đặc tả Use Case "Ảnh sản phẩm / thứ tự hiển thị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_013 |
| **Tên Use Case** | Ảnh sản phẩm / thứ tự hiển thị |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Ảnh sản phẩm / thứ tự hiển thị" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Product display |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ảnh sản phẩm / thứ tự hiển thị» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ảnh sản phẩm / thứ tự hiển thị» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ảnh sản phẩm / thứ tự hiển thị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Ảnh sản phẩm / thứ tự hiển thị» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Product display).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ảnh sản phẩm / thứ tự hiển thị».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ảnh sản phẩm / thứ tự hiển thị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 14. Đặc tả Use Case "Ngưng bán sản phẩm tạm thời"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_014 |
| **Tên Use Case** | Ngưng bán sản phẩm tạm thời |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Ngưng bán sản phẩm tạm thời" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: 86 item |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ngưng bán sản phẩm tạm thời» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ngưng bán sản phẩm tạm thời» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ngưng bán sản phẩm tạm thời» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Ngưng bán sản phẩm tạm thời» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (86 item).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ngưng bán sản phẩm tạm thời».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ngưng bán sản phẩm tạm thời» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 15. Đặc tả Use Case "Đồng bộ catalog từ back-office"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_015 |
| **Tên Use Case** | Đồng bộ catalog từ back-office |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Đồng bộ catalog từ back-office" thuộc nhóm Catalog bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Catalog sync |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ catalog từ back-office» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ catalog từ back-office» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ catalog từ back-office» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Đồng bộ catalog từ back-office» trong nhóm Catalog bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Catalog sync).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ catalog từ back-office».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ catalog từ back-office» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.3. Bảng giá & thuế (`POS-03`)

Nhóm **Bảng giá & thuế** gồm **5** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 2 |

**Bảng 16. Đặc tả Use Case "Bảng giá theo điểm bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_016 |
| **Tên Use Case** | Bảng giá theo điểm bán |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Bảng giá theo điểm bán" thuộc nhóm Bảng giá & thuế trong module POS — POS bán lẻ. Mô tả chi tiết: Location price list |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng giá theo điểm bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng giá theo điểm bán» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng giá theo điểm bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Bảng giá theo điểm bán» trong nhóm Bảng giá & thuế.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Location price list).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng giá theo điểm bán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng giá theo điểm bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 17. Đặc tả Use Case "Giá theo khung giờ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_017 |
| **Tên Use Case** | Giá theo khung giờ |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Giá theo khung giờ" thuộc nhóm Bảng giá & thuế trong module POS — POS bán lẻ. Mô tả chi tiết: Time-based pricing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giá theo khung giờ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giá theo khung giờ» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giá theo khung giờ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Giá theo khung giờ» trong nhóm Bảng giá & thuế.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Time-based pricing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giá theo khung giờ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giá theo khung giờ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 18. Đặc tả Use Case "Giá theo ngày trong tuần"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_018 |
| **Tên Use Case** | Giá theo ngày trong tuần |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Giá theo ngày trong tuần" thuộc nhóm Bảng giá & thuế trong module POS — POS bán lẻ. Mô tả chi tiết: Day-based pricing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giá theo ngày trong tuần» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giá theo ngày trong tuần» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giá theo ngày trong tuần» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Giá theo ngày trong tuần» trong nhóm Bảng giá & thuế.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Day-based pricing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giá theo ngày trong tuần».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giá theo ngày trong tuần» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 19. Đặc tả Use Case "Cấu hình thuế GTGT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_019 |
| **Tên Use Case** | Cấu hình thuế GTGT |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Cấu hình thuế GTGT" thuộc nhóm Bảng giá & thuế trong module POS — POS bán lẻ. Mô tả chi tiết: Tax configuration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thuế GTGT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thuế GTGT» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thuế GTGT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Cấu hình thuế GTGT» trong Bảng giá & thuế.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Tax configuration) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thuế GTGT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 20. Đặc tả Use Case "Làm tròn tiền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_020 |
| **Tên Use Case** | Làm tròn tiền |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Làm tròn tiền" thuộc nhóm Bảng giá & thuế trong module POS — POS bán lẻ. Mô tả chi tiết: Rounding rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Làm tròn tiền» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Làm tròn tiền» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Làm tròn tiền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Làm tròn tiền» trong nhóm Bảng giá & thuế.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Rounding rules).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Làm tròn tiền».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Làm tròn tiền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.4. Khuyến mại tại quầy (`POS-04`)

Nhóm **Khuyến mại tại quầy** gồm **5** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 21. Đặc tả Use Case "Áp dụng chương trình khuyến mại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_021 |
| **Tên Use Case** | Áp dụng chương trình khuyến mại |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Áp dụng chương trình khuyến mại" thuộc nhóm Khuyến mại tại quầy trong module POS — POS bán lẻ. Mô tả chi tiết: Apply promotions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Áp dụng chương trình khuyến mại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Áp dụng chương trình khuyến mại» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Áp dụng chương trình khuyến mại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Áp dụng chương trình khuyến mại» trong nhóm Khuyến mại tại quầy.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Apply promotions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Áp dụng chương trình khuyến mại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Áp dụng chương trình khuyến mại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 22. Đặc tả Use Case "Nhập mã voucher"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_022 |
| **Tên Use Case** | Nhập mã voucher |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Nhập mã voucher" thuộc nhóm Khuyến mại tại quầy trong module POS — POS bán lẻ. Mô tả chi tiết: Voucher redemption |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập mã voucher» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập mã voucher» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập mã voucher» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Nhập mã voucher» trong nhóm Khuyến mại tại quầy.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Voucher redemption).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập mã voucher».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập mã voucher» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 23. Đặc tả Use Case "Khuyến mại theo combo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_023 |
| **Tên Use Case** | Khuyến mại theo combo |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Khuyến mại theo combo" thuộc nhóm Khuyến mại tại quầy trong module POS — POS bán lẻ. Mô tả chi tiết: Combo/bundle pricing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khuyến mại theo combo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khuyến mại theo combo» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khuyến mại theo combo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Khuyến mại theo combo» trong nhóm Khuyến mại tại quầy.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Combo/bundle pricing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khuyến mại theo combo».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khuyến mại theo combo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 24. Đặc tả Use Case "Giảm giá tay có quyền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_024 |
| **Tên Use Case** | Giảm giá tay có quyền |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Giảm giá tay có quyền" thuộc nhóm Khuyến mại tại quầy trong module POS — POS bán lẻ. Mô tả chi tiết: Manual discount with approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giảm giá tay có quyền» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giảm giá tay có quyền» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giảm giá tay có quyền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Giảm giá tay có quyền» trong nhóm Khuyến mại tại quầy.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Manual discount with approval).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giảm giá tay có quyền».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giảm giá tay có quyền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 25. Đặc tả Use Case "Báo cáo khuyến mại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_025 |
| **Tên Use Case** | Báo cáo khuyến mại |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Báo cáo khuyến mại" thuộc nhóm Khuyến mại tại quầy trong module POS — POS bán lẻ. Mô tả chi tiết: Promotion usage report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo khuyến mại» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo khuyến mại» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo khuyến mại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier mở «Báo cáo khuyến mại» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Promotion usage report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo khuyến mại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.5. Giao dịch bán hàng (`POS-05`)

Nhóm **Giao dịch bán hàng** gồm **16** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 16 |
| Must | 10 |

**Bảng 26. Đặc tả Use Case "Mở đơn / chọn khu vực"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_026 |
| **Tên Use Case** | Mở đơn / chọn khu vực |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Mở đơn / chọn khu vực" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: New transaction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mở đơn / chọn khu vực» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mở đơn / chọn khu vực» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mở đơn / chọn khu vực» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Mở đơn / chọn khu vực» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (New transaction).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mở đơn / chọn khu vực».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mở đơn / chọn khu vực» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 27. Đặc tả Use Case "Thêm / sửa / xóa sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_027 |
| **Tên Use Case** | Thêm / sửa / xóa sản phẩm |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Thêm / sửa / xóa sản phẩm" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Line item management |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thêm / sửa / xóa sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thêm / sửa / xóa sản phẩm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thêm / sửa / xóa sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier mở chức năng «Thêm / sửa / xóa sản phẩm» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Thêm / sửa / xóa sản phẩm» (Line item management).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Thêm / sửa / xóa sản phẩm» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thêm / sửa / xóa sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 28. Đặc tả Use Case "Tách bill / gộp bill"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_028 |
| **Tên Use Case** | Tách bill / gộp bill |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Tách bill / gộp bill" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Split/merge bills |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tách bill / gộp bill» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tách bill / gộp bill» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tách bill / gộp bill» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Tách bill / gộp bill» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Split/merge bills).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tách bill / gộp bill».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tách bill / gộp bill» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 29. Đặc tả Use Case "Chuyển đơn giữa quầy"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_029 |
| **Tên Use Case** | Chuyển đơn giữa quầy |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Chuyển đơn giữa quầy" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Transfer order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển đơn giữa quầy» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển đơn giữa quầy» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển đơn giữa quầy» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Chuyển đơn giữa quầy» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Transfer order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển đơn giữa quầy».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển đơn giữa quầy» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 30. Đặc tả Use Case "Ghi chú đơn hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_030 |
| **Tên Use Case** | Ghi chú đơn hàng |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Ghi chú đơn hàng" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Order notes |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi chú đơn hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi chú đơn hàng» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi chú đơn hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Ghi chú đơn hàng» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Order notes).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi chú đơn hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi chú đơn hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 31. Đặc tả Use Case "Gửi lệnh khu vực chế biến"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_031 |
| **Tên Use Case** | Gửi lệnh khu vực chế biến |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Gửi lệnh khu vực chế biến" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Send to kitchen/preparation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gửi lệnh khu vực chế biến» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gửi lệnh khu vực chế biến» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi lệnh khu vực chế biến» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier hoàn thiện dữ liệu cho «Gửi lệnh khu vực chế biến» ở trạng thái nháp.<br>2. Chọn [Gửi duyệt / Xác nhận] (submit).<br>3. Hệ thống validate đủ điều kiện gửi; chuyển trạng thái Submitted/In Approval.<br>4. Tạo việc duyệt (WF hoặc duyệt nội module); gửi thông báo.<br>5. Khóa sửa một phần theo policy khi đang chờ duyệt. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gửi lệnh khu vực chế biến» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>9.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 32. Đặc tả Use Case "Tạm tính / giữ đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_032 |
| **Tên Use Case** | Tạm tính / giữ đơn |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Tạm tính / giữ đơn" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Hold transaction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạm tính / giữ đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạm tính / giữ đơn» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạm tính / giữ đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Tạm tính / giữ đơn» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Hold transaction).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tạm tính / giữ đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạm tính / giữ đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 33. Đặc tả Use Case "Thanh toán tiền mặt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_033 |
| **Tên Use Case** | Thanh toán tiền mặt |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Thanh toán tiền mặt" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Cash payment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh toán tiền mặt» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh toán tiền mặt» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh toán tiền mặt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier chọn chứng từ cần thu/chi trong «Thanh toán tiền mặt».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh toán tiền mặt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 34. Đặc tả Use Case "Thanh toán chuyển khoản / QR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_034 |
| **Tên Use Case** | Thanh toán chuyển khoản / QR |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Thanh toán chuyển khoản / QR" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Bank transfer/QR payment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh toán chuyển khoản / QR» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh toán chuyển khoản / QR» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh toán chuyển khoản / QR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier chọn chứng từ cần thu/chi trong «Thanh toán chuyển khoản / QR».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh toán chuyển khoản / QR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 35. Đặc tả Use Case "Thanh toán thẻ / ví điện tử"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_035 |
| **Tên Use Case** | Thanh toán thẻ / ví điện tử |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Thanh toán thẻ / ví điện tử" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Card/e-wallet payment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh toán thẻ / ví điện tử» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh toán thẻ / ví điện tử» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh toán thẻ / ví điện tử» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier chọn chứng từ cần thu/chi trong «Thanh toán thẻ / ví điện tử».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh toán thẻ / ví điện tử» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 36. Đặc tả Use Case "Thanh toán hỗn hợp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_036 |
| **Tên Use Case** | Thanh toán hỗn hợp |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Thanh toán hỗn hợp" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Split payment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh toán hỗn hợp» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh toán hỗn hợp» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh toán hỗn hợp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier chọn chứng từ cần thu/chi trong «Thanh toán hỗn hợp».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh toán hỗn hợp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 37. Đặc tả Use Case "In hóa đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_037 |
| **Tên Use Case** | In hóa đơn |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "In hóa đơn" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Print receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In hóa đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In hóa đơn» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In hóa đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «In hóa đơn» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Print receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «In hóa đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In hóa đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 38. Đặc tả Use Case "Hủy sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_038 |
| **Tên Use Case** | Hủy sản phẩm |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Hủy sản phẩm" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Void item |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy sản phẩm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Hủy sản phẩm» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Void item).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hủy sản phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 39. Đặc tả Use Case "Hủy cả bill"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_039 |
| **Tên Use Case** | Hủy cả bill |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Hủy cả bill" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Void transaction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy cả bill» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy cả bill» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy cả bill» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Hủy cả bill» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Void transaction).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hủy cả bill».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy cả bill» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 40. Đặc tả Use Case "Trả hàng / hoàn tiền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_040 |
| **Tên Use Case** | Trả hàng / hoàn tiền |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Trả hàng / hoàn tiền" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Refund transaction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trả hàng / hoàn tiền» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trả hàng / hoàn tiền» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trả hàng / hoàn tiền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier chọn chứng từ cần thu/chi trong «Trả hàng / hoàn tiền».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trả hàng / hoàn tiền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 41. Đặc tả Use Case "Gợi ý bán kèm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_041 |
| **Tên Use Case** | Gợi ý bán kèm |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Gợi ý bán kèm" thuộc nhóm Giao dịch bán hàng trong module POS — POS bán lẻ. Mô tả chi tiết: Upsell suggestions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gợi ý bán kèm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gợi ý bán kèm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gợi ý bán kèm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Gợi ý bán kèm» trong nhóm Giao dịch bán hàng.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Upsell suggestions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gợi ý bán kèm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gợi ý bán kèm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.6. Ca thu ngân & quỹ (`POS-06`)

Nhóm **Ca thu ngân & quỹ** gồm **8** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 6 |

**Bảng 42. Đặc tả Use Case "Mở ca thu ngân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_042 |
| **Tên Use Case** | Mở ca thu ngân |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Mở ca thu ngân" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Open shift |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mở ca thu ngân» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mở ca thu ngân» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mở ca thu ngân» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Mở ca thu ngân» trong nhóm Ca thu ngân & quỹ.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Open shift).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mở ca thu ngân».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mở ca thu ngân» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 43. Đặc tả Use Case "Nhập tiền đầu ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_043 |
| **Tên Use Case** | Nhập tiền đầu ca |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Nhập tiền đầu ca" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Starting float |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập tiền đầu ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập tiền đầu ca» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập tiền đầu ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Nhập tiền đầu ca» trong nhóm Ca thu ngân & quỹ.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Starting float).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập tiền đầu ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập tiền đầu ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 44. Đặc tả Use Case "Nộp tiền / rút tiền ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_044 |
| **Tên Use Case** | Nộp tiền / rút tiền ca |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Nộp tiền / rút tiền ca" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Cash in/out |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nộp tiền / rút tiền ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nộp tiền / rút tiền ca» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nộp tiền / rút tiền ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Nộp tiền / rút tiền ca» trong nhóm Ca thu ngân & quỹ.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Cash in/out).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nộp tiền / rút tiền ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nộp tiền / rút tiền ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 45. Đặc tả Use Case "Xem doanh thu trong ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_045 |
| **Tên Use Case** | Xem doanh thu trong ca |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Xem doanh thu trong ca" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Shift sales realtime |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem doanh thu trong ca» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem doanh thu trong ca» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem doanh thu trong ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier mở «Xem doanh thu trong ca» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Shift sales realtime).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem doanh thu trong ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 46. Đặc tả Use Case "Đóng ca & đếm quỹ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_046 |
| **Tên Use Case** | Đóng ca & đếm quỹ |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Đóng ca & đếm quỹ" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Close shift & count |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng ca & đếm quỹ» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng ca & đếm quỹ» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng ca & đếm quỹ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier chọn kỳ/ca/đối tượng cần khóa trong «Đóng ca & đếm quỹ».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng ca & đếm quỹ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 47. Đặc tả Use Case "Đối soát lệch quỹ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_047 |
| **Tên Use Case** | Đối soát lệch quỹ |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Đối soát lệch quỹ" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Cash variance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát lệch quỹ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát lệch quỹ» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát lệch quỹ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Đối soát lệch quỹ» trong nhóm Ca thu ngân & quỹ.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Cash variance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát lệch quỹ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát lệch quỹ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 48. Đặc tả Use Case "In báo cáo ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_048 |
| **Tên Use Case** | In báo cáo ca |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "In báo cáo ca" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Shift report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In báo cáo ca» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In báo cáo ca» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In báo cáo ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cashier mở «In báo cáo ca» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Shift report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In báo cáo ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 49. Đặc tả Use Case "Duyệt xác nhận ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_049 |
| **Tên Use Case** | Duyệt xác nhận ca |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Duyệt xác nhận ca" thuộc nhóm Ca thu ngân & quỹ trong module POS — POS bán lẻ. Mô tả chi tiết: Shift approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt xác nhận ca» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt xác nhận ca» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt xác nhận ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Cashier mở hộp chờ / chứng từ cần xử lý cho «Duyệt xác nhận ca».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt xác nhận ca», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt xác nhận ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.7. Khách hàng & loyalty (`POS-07`)

Nhóm **Khách hàng & loyalty** gồm **4** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 50. Đặc tả Use Case "Gắn khách hàng vào đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_050 |
| **Tên Use Case** | Gắn khách hàng vào đơn |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Gắn khách hàng vào đơn" thuộc nhóm Khách hàng & loyalty trong module POS — POS bán lẻ. Mô tả chi tiết: Customer identification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn khách hàng vào đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn khách hàng vào đơn» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn khách hàng vào đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Gắn khách hàng vào đơn» trong nhóm Khách hàng & loyalty.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer identification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn khách hàng vào đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn khách hàng vào đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 51. Đặc tả Use Case "Tích điểm loyalty"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_051 |
| **Tên Use Case** | Tích điểm loyalty |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Tích điểm loyalty" thuộc nhóm Khách hàng & loyalty trong module POS — POS bán lẻ. Mô tả chi tiết: Earn loyalty points |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tích điểm loyalty» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tích điểm loyalty» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tích điểm loyalty» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Tích điểm loyalty» trong nhóm Khách hàng & loyalty.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Earn loyalty points).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tích điểm loyalty».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tích điểm loyalty» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 52. Đặc tả Use Case "Đổi điểm / ưu đãi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_052 |
| **Tên Use Case** | Đổi điểm / ưu đãi |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Đổi điểm / ưu đãi" thuộc nhóm Khách hàng & loyalty trong module POS — POS bán lẻ. Mô tả chi tiết: Redeem rewards |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đổi điểm / ưu đãi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đổi điểm / ưu đãi» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đổi điểm / ưu đãi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier khởi tạo thao tác «Đổi điểm / ưu đãi» trong nhóm Khách hàng & loyalty.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Redeem rewards).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đổi điểm / ưu đãi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đổi điểm / ưu đãi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 53. Đặc tả Use Case "Tra cứu lịch sử mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_053 |
| **Tên Use Case** | Tra cứu lịch sử mua |
| **Tác nhân** | Cashier |
| **Mô tả chức năng** | Cho phép Cashier thực hiện chức năng "Tra cứu lịch sử mua" thuộc nhóm Khách hàng & loyalty trong module POS — POS bán lẻ. Mô tả chi tiết: Purchase history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cashier] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tra cứu lịch sử mua» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tra cứu lịch sử mua» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tra cứu lịch sử mua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cashier mở «Tra cứu lịch sử mua» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Purchase history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tra cứu lịch sử mua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.8. Đồng bộ tồn & back-office (`POS-08`)

Nhóm **Đồng bộ tồn & back-office** gồm **7** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 3 |

**Bảng 54. Đặc tả Use Case "Trừ tồn theo BOM khi bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_054 |
| **Tên Use Case** | Trừ tồn theo BOM khi bán |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Trừ tồn theo BOM khi bán" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: Auto inventory depletion |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trừ tồn theo BOM khi bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trừ tồn theo BOM khi bán» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trừ tồn theo BOM khi bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Trừ tồn theo BOM khi bán» trong nhóm Đồng bộ tồn & back-office.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Auto inventory depletion).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Trừ tồn theo BOM khi bán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trừ tồn theo BOM khi bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 55. Đặc tả Use Case "Cảnh báo hết / sắp hết"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_055 |
| **Tên Use Case** | Cảnh báo hết / sắp hết |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Cảnh báo hết / sắp hết" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: Low stock alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo hết / sắp hết» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo hết / sắp hết» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo hết / sắp hết» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Store Manager kích hoạt kiểm tra điều kiện «Cảnh báo hết / sắp hết».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Low stock alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo hết / sắp hết» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 56. Đặc tả Use Case "Tạo đề nghị nhập hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_056 |
| **Tên Use Case** | Tạo đề nghị nhập hàng |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Tạo đề nghị nhập hàng" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: Stock requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đề nghị nhập hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đề nghị nhập hàng» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đề nghị nhập hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager mở chức năng «Tạo đề nghị nhập hàng» trong nhóm Đồng bộ tồn & back-office.<br>2. Hệ thống kiểm tra license `POS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đề nghị nhập hàng» (Stock requisition).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đề nghị nhập hàng» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đề nghị nhập hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 57. Đặc tả Use Case "Nhận hàng từ kho trung tâm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_057 |
| **Tên Use Case** | Nhận hàng từ kho trung tâm |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Nhận hàng từ kho trung tâm" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: Receiving at location |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhận hàng từ kho trung tâm» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`, `BR-POS-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhận hàng từ kho trung tâm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhận hàng từ kho trung tâm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager mở chứng từ nhận liên quan «Nhận hàng từ kho trung tâm».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhận hàng từ kho trung tâm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 58. Đặc tả Use Case "Kiểm kê nhanh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_058 |
| **Tên Use Case** | Kiểm kê nhanh |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Kiểm kê nhanh" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: Quick stocktake |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm kê nhanh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm kê nhanh» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm kê nhanh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Kiểm kê nhanh» trong nhóm Đồng bộ tồn & back-office.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Quick stocktake).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm kê nhanh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm kê nhanh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 59. Đặc tả Use Case "Đồng bộ doanh thu ca sang FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_059 |
| **Tên Use Case** | Đồng bộ doanh thu ca sang FIN |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Đồng bộ doanh thu ca sang FIN" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: Sales posting to GL |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ doanh thu ca sang FIN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ doanh thu ca sang FIN» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ doanh thu ca sang FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Đồng bộ doanh thu ca sang FIN» trong nhóm Đồng bộ tồn & back-office.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Sales posting to GL).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ doanh thu ca sang FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ doanh thu ca sang FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 60. Đặc tả Use Case "Đồng bộ đơn sang CRM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_060 |
| **Tên Use Case** | Đồng bộ đơn sang CRM |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Đồng bộ đơn sang CRM" thuộc nhóm Đồng bộ tồn & back-office trong module POS — POS bán lẻ. Mô tả chi tiết: CRM sync |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ đơn sang CRM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ đơn sang CRM» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ đơn sang CRM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Đồng bộ đơn sang CRM» trong nhóm Đồng bộ tồn & back-office.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (CRM sync).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ đơn sang CRM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ đơn sang CRM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.9. Báo cáo POS (`POS-09`)

Nhóm **Báo cáo POS** gồm **8** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 5 |

**Bảng 61. Đặc tả Use Case "Doanh thu theo giờ / ngày / ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_061 |
| **Tên Use Case** | Doanh thu theo giờ / ngày / ca |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Doanh thu theo giờ / ngày / ca" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Sales by time |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Doanh thu theo giờ / ngày / ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Doanh thu theo giờ / ngày / ca» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Doanh thu theo giờ / ngày / ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Doanh thu theo giờ / ngày / ca» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Sales by time).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Doanh thu theo giờ / ngày / ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Doanh thu theo giờ / ngày / ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 62. Đặc tả Use Case "Doanh thu theo sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_062 |
| **Tên Use Case** | Doanh thu theo sản phẩm |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Doanh thu theo sản phẩm" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Product mix |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Doanh thu theo sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Doanh thu theo sản phẩm» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Doanh thu theo sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Doanh thu theo sản phẩm» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Product mix).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Doanh thu theo sản phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Doanh thu theo sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 63. Đặc tả Use Case "Doanh thu theo thu ngân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_063 |
| **Tên Use Case** | Doanh thu theo thu ngân |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Doanh thu theo thu ngân" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Cashier performance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Doanh thu theo thu ngân» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Doanh thu theo thu ngân» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Doanh thu theo thu ngân» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Doanh thu theo thu ngân» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Cashier performance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Doanh thu theo thu ngân».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Doanh thu theo thu ngân» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 64. Đặc tả Use Case "Tỷ lệ hủy / giảm giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_064 |
| **Tên Use Case** | Tỷ lệ hủy / giảm giá |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Tỷ lệ hủy / giảm giá" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Void & discount analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tỷ lệ hủy / giảm giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tỷ lệ hủy / giảm giá» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tỷ lệ hủy / giảm giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Tỷ lệ hủy / giảm giá» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Void & discount analysis).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tỷ lệ hủy / giảm giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tỷ lệ hủy / giảm giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 65. Đặc tả Use Case "Cost lý thuyết vs thực tế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_065 |
| **Tên Use Case** | Cost lý thuyết vs thực tế |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Cost lý thuyết vs thực tế" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Recipe vs usage variance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cost lý thuyết vs thực tế» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cost lý thuyết vs thực tế» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cost lý thuyết vs thực tế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Cost lý thuyết vs thực tế» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Recipe vs usage variance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Cost lý thuyết vs thực tế».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cost lý thuyết vs thực tế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 66. Đặc tả Use Case "Top sản phẩm bán chạy"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_066 |
| **Tên Use Case** | Top sản phẩm bán chạy |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Top sản phẩm bán chạy" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Bestsellers |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Top sản phẩm bán chạy» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Top sản phẩm bán chạy» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Top sản phẩm bán chạy» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «Top sản phẩm bán chạy» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Bestsellers).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Top sản phẩm bán chạy».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Top sản phẩm bán chạy» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 67. Đặc tả Use Case "So sánh điểm bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_067 |
| **Tên Use Case** | So sánh điểm bán |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "So sánh điểm bán" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Location comparison |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh điểm bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh điểm bán» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh điểm bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Store Manager khởi tạo thao tác «So sánh điểm bán» trong nhóm Báo cáo POS.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Location comparison).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «So sánh điểm bán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh điểm bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 68. Đặc tả Use Case "Xuất báo cáo POS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_068 |
| **Tên Use Case** | Xuất báo cáo POS |
| **Tác nhân** | Store Manager |
| **Mô tả chức năng** | Cho phép Store Manager thực hiện chức năng "Xuất báo cáo POS" thuộc nhóm Báo cáo POS trong module POS — POS bán lẻ. Mô tả chi tiết: Export POS reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Store Manager] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo POS» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo POS» được lưu nhất quán trong module `POS`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo POS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Store Manager mở «Xuất báo cáo POS», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo POS» (Export POS reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo POS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

### 7.10. Vận hành chuỗi (`POS-10`)

Nhóm **Vận hành chuỗi** gồm **4** use case của module `POS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 2 |

**Bảng 69. Đặc tả Use Case "Giám sát doanh thu chuỗi realtime"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_069 |
| **Tên Use Case** | Giám sát doanh thu chuỗi realtime |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Giám sát doanh thu chuỗi realtime" thuộc nhóm Vận hành chuỗi trong module POS — POS bán lẻ. Mô tả chi tiết: Chain-wide monitoring |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giám sát doanh thu chuỗi realtime» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giám sát doanh thu chuỗi realtime» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giám sát doanh thu chuỗi realtime» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Giám sát doanh thu chuỗi realtime» trong nhóm Vận hành chuỗi.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Chain-wide monitoring).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giám sát doanh thu chuỗi realtime».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giám sát doanh thu chuỗi realtime» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 70. Đặc tả Use Case "Phân phối catalog / giá / khuyến mại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_070 |
| **Tên Use Case** | Phân phối catalog / giá / khuyến mại |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Phân phối catalog / giá / khuyến mại" thuộc nhóm Vận hành chuỗi trong module POS — POS bán lẻ. Mô tả chi tiết: Push configuration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân phối catalog / giá / khuyến mại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân phối catalog / giá / khuyến mại» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân phối catalog / giá / khuyến mại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Phân phối catalog / giá / khuyến mại» trong nhóm Vận hành chuỗi.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Push configuration).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân phối catalog / giá / khuyến mại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân phối catalog / giá / khuyến mại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 71. Đặc tả Use Case "Chuẩn hóa catalog toàn chuỗi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_071 |
| **Tên Use Case** | Chuẩn hóa catalog toàn chuỗi |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Chuẩn hóa catalog toàn chuỗi" thuộc nhóm Vận hành chuỗi trong module POS — POS bán lẻ. Mô tả chi tiết: Master catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuẩn hóa catalog toàn chuỗi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuẩn hóa catalog toàn chuỗi» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuẩn hóa catalog toàn chuỗi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chain Admin khởi tạo thao tác «Chuẩn hóa catalog toàn chuỗi» trong nhóm Vận hành chuỗi.<br>2. Hệ thống kiểm tra license `POS`, quyền RBAC và tiền điều kiện nghiệp vụ (Master catalog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuẩn hóa catalog toàn chuỗi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuẩn hóa catalog toàn chuỗi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

**Bảng 72. Đặc tả Use Case "Cấu hình target doanh thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_POS_072 |
| **Tên Use Case** | Cấu hình target doanh thu |
| **Tác nhân** | Chain Admin |
| **Mô tả chức năng** | Cho phép Chain Admin thực hiện chức năng "Cấu hình target doanh thu" thuộc nhóm Vận hành chuỗi trong module POS — POS bán lẻ. Mô tả chi tiết: Sales target by location |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chain Admin] và được cấp quyền RBAC tương ứng.<br>• License module `POS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình target doanh thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-POS-SCOPE-01`, `BR-POS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình target doanh thu» được lưu nhất quán trong module `POS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình target doanh thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chain Admin mở màn hình cấu hình «Cấu hình target doanh thu» trong Vận hành chuỗi.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Sales target by location) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình target doanh thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ. |

---

## 8. Workflow end-to-end

### WF-POS-01 — Bán hàng trong ca

**Mục tiêu:** Hoàn tất giao dịch và ghi nhận doanh thu ca

| Bước | Mô tả |
|---:|---|
| 1 | Mở ca, nhập tiền đầu ca |
| 2 | Tạo đơn, thêm SP/món, áp KM/voucher |
| 3 | Thanh toán một/nhiều hình thức; in hóa đơn |
| 4 | Trừ tồn theo rule; cộng doanh thu ca |
| 5 | Đóng ca, đếm quỹ, đối soát lệch |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-POS-02 — Đẩy cấu hình chuỗi

**Mục tiêu:** Điểm bán nhận menu/giá/KM mới

| Bước | Mô tả |
|---:|---|
| 1 | Chain Admin cập nhật master catalog/giá/KM |
| 2 | Chọn điểm bán đích và phát hành |
| 3 | Terminal đồng bộ phiên bản cấu hình |
| 4 | Báo cáo điểm chưa đồng bộ |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Store / Terminal` | Điểm bán & máy |
| `SellableItem / Modifier / Recipe` | Hàng bán & định mức |
| `PriceList` | Bảng giá |
| `PosOrder / PosPayment` | Giao dịch |
| `CashShift` | Ca quỹ |
| `StoreRequisition` | Đề nghị hàng |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `POS`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-POS-01: Hủy món/bill sau thanh toán hoặc sau gửi chế biến cần quyền riêng.
- BR-POS-02: Không đóng ca khi còn đơn mở (trừ force có quyền).
- BR-POS-03: Lệch quỹ phải ghi nhận lý do.
- BR-POS-04: Terminal chỉ bán SP đang active và còn cho phép tại cửa hàng.
- BR-POS-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-POS-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-POS-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Tốc độ | Thêm món và thanh toán cảm nhận < 1s trên LAN cửa hàng |
| Sẵn sàng | Hỗ trợ hàng đợi offline hạn chế (tùy gói) |
| In ấn | Tương thích máy in nhiệt phổ biến |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `POS` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `pos.sale.create` | Quyền chức năng module |
| `pos.discount.apply` | Quyền chức năng module |
| `pos.void.approve` | Quyền chức năng module |
| `pos.shift.open_close` | Quyền chức năng module |
| `pos.config.manage` | Quyền chức năng module |
| `pos.report.view` | Quyền chức năng module |
| `pos.*.view` | Xem trong data scope |
| `pos.*.manage` | Tạo/sửa trong data scope |
| `pos.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Doanh thu theo giờ/ca/CH | Theo dõi vận hành module |
| Tỷ lệ void/discount | Theo dõi vận hành module |
| Thời gian phục vụ trung bình | Theo dõi vận hành module |
| Food/material cost variance (khi có recipe+INV) | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Mỗi điểm bán thuộc một chi nhánh/org trong SYS.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Phase 1 có KDS bếp đầy đủ hay chỉ in lệnh?
2. Có hỗ trợ đặt bàn/đặt trước không?

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
| Bản SRS này | `SRS_POS_v1.1.md` / `.docx` |
| UC IDs | `UC_POS_001` … |

---

*Hết tài liệu SRS-POS-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
