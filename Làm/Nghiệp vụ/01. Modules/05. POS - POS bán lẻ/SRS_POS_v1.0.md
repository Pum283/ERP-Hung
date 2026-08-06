# SRS-POS-v1.0 — POS bán lẻ

> Tài liệu đặc tả yêu cầu phần mềm (Software Requirements Specification) cho module ERP bán độc lập.
> Trạng thái: **Đề xuất / chờ duyệt nghiệp vụ**. Không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu & lịch sử thay đổi

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-POS-v1.0` |
| Module | `POS` — POS bán lẻ |
| Phiên bản | 1.0 |
| Ngày lập | 03/08/2026 |
| Ngôn ngữ | Tiếng Việt |
| Phân loại | Nghiệp vụ / BA |
| Lớp sản phẩm | Bán hàng & Khách hàng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | SYS |
| Khuyến nghị kèm | INV, FIN, CRM |
| Số nhóm chức năng | 10 |
| Số use case / chức năng | 72 |

| Phiên bản | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Solution | Sinh SRS từ danh mục chức năng generic v3 + meta nghiệp vụ | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích tài liệu
Tài liệu này mô tả đầy đủ yêu cầu nghiệp vụ và yêu cầu hệ thống của module **POS bán lẻ**, làm cơ sở để thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai cấu trúc source code.

### 1.2. Tóm tắt module
Module POS phục vụ bán tại điểm bán: danh mục hàng/món, giá, khuyến mại, thanh toán, ca thu ngân, đồng bộ tồn và doanh thu, báo cáo cửa hàng/chuỗi.

### 1.3. Mục tiêu nghiệp vụ
1. Thanh toán nhanh, kiểm soát quỹ ca.
2. Đồng bộ tồn/định mức với kho.
3. Chuẩn hóa menu/giá/KM toàn chuỗi.
4. Cung cấp số liệu doanh thu realtime cho quản lý.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / Ban giám đốc dự án
- Business Analyst, Solution Architect
- Trưởng nhóm Dev/QA
- Đội triển khai & Presales (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Store/terminal config, sellable catalog, pricing, promo at POS, checkout, shift/cash, loyalty light, stock sync, POS reports, chain push config.

### 2.2. Out of Scope
- CRM pipeline đầy đủ.
- Kế toán sổ cái (FIN).
- WMS chi tiết vị trí kho tổng (INV).

### 2.3. Nguyên tắc đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`.
- **Khuyến nghị kèm** để có giá trị E2E: INV, FIN, CRM.
- Tính năng ngành (F&B, sản xuất rời rạc, phân phối…) cấu hình bằng template khi triển khai, không hard-code vào SRS gốc.

---

## 3. Tác nhân & stakeholder

| Tác nhân | Trách nhiệm chính |
|---|---|
| Cashier | Thu ngân |
| Store Manager | Duyệt giảm giá/hủy, đóng ca, báo cáo CH |
| Chain Admin | Đẩy menu/giá/KM xuống điểm bán |
| Inventory Clerk (CH) | Nhận hàng/kiểm kê nhanh tại CH |

---

## 4. Thuật ngữ & viết tắt

| Thuật ngữ | Định nghĩa |
|---|---|
| Terminal | Máy POS tại quầy |
| Shift | Ca thu ngân |
| BOM/Recipe | Định mức nguyên liệu gắn SP/món |
| Void | Hủy món/bill có kiểm soát |
| UC | Use Case / chức năng nguyên tử trong catalog |
| MoSCoW | Must / Should / Could / Won't (ưu tiên) |
| Data scope | Phạm vi dữ liệu theo tổ chức/kho/… do SYS kiểm soát |

---

## 5. Ngữ cảnh module & phụ thuộc

### 5.1. Vị trí trong kiến trúc sản phẩm
Module `POS` thuộc lớp **Bán hàng & Khách hàng**. Mọi truy cập đi qua lớp nền `SYS` (xác thực, RBAC, license, audit, file, thông báo).

### 5.2. Phụ thuộc & tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | INV | Trừ tồn, nhận hàng CH, kiểm kê nhanh |
| Tích hợp | FIN | Post doanh thu/ca/quỹ |
| Tích hợp | CRM | KM, loyalty, gắn KH |
| Tích hợp | Payment QR/Card | Cổng thanh toán |

### 5.3. Ràng buộc license
- API/UI của `POS` chỉ mở khi license module active.
- Dataset BI liên quan module chỉ mở khi vừa có license `BI` vừa có license module nguồn.

---

## 6. Catalog chức năng (Module → Nhóm → UC)

**Tổng hợp:** 10 nhóm | 72 chức năng/use case.

| STT | Mã nhóm | Nhóm chức năng | Số UC |
|---:|---|---|---:|
| 1 | `POS-01` | Cấu hình điểm bán & thiết bị | 8 |
| 2 | `POS-02` | Catalog bán hàng | 7 |
| 3 | `POS-03` | Bảng giá & thuế | 5 |
| 4 | `POS-04` | Khuyến mại tại quầy | 5 |
| 5 | `POS-05` | Giao dịch bán hàng | 16 |
| 6 | `POS-06` | Ca thu ngân & quỹ | 8 |
| 7 | `POS-07` | Khách hàng & loyalty | 4 |
| 8 | `POS-08` | Đồng bộ tồn & back-office | 7 |
| 9 | `POS-09` | Báo cáo POS | 8 |
| 10 | `POS-10` | Vận hành chuỗi | 4 |

<details>
<summary>Bảng đầy đủ mã UC (bấm để mở)</summary>

| Mã UC | Nhóm | Tên chức năng | Ưu tiên | MoSCoW |
|---|---|---|---|---|
| `UC_POS_001` | Cấu hình điểm bán & thiết bị | Khai báo điểm bán POS | Bắt buộc | Must |
| `UC_POS_002` | Cấu hình điểm bán & thiết bị | Khai báo quầy / máy POS | Bắt buộc | Must |
| `UC_POS_003` | Cấu hình điểm bán & thiết bị | Cấu hình máy in hóa đơn | Bắt buộc | Must |
| `UC_POS_004` | Cấu hình điểm bán & thiết bị | Cấu hình máy in bếp/khu vực | Cao | Should |
| `UC_POS_005` | Cấu hình điểm bán & thiết bị | Cấu hình ngăn kéo tiền | Cao | Should |
| `UC_POS_006` | Cấu hình điểm bán & thiết bị | Cấu hình thiết bị quét mã | Cao | Should |
| `UC_POS_007` | Cấu hình điểm bán & thiết bị | Phân quyền thu ngân trên POS | Bắt buộc | Must |
| `UC_POS_008` | Cấu hình điểm bán & thiết bị | Chế độ offline tạm | Thấp | Won't / Later |
| `UC_POS_009` | Catalog bán hàng | Danh mục nhóm sản phẩm | Bắt buộc | Must |
| `UC_POS_010` | Catalog bán hàng | Danh mục sản phẩm bán | Bắt buộc | Must |
| `UC_POS_011` | Catalog bán hàng | Thuộc tính sản phẩm | Cao | Should |
| `UC_POS_012` | Catalog bán hàng | BOM / định mức nguyên liệu | Bắt buộc | Must |
| `UC_POS_013` | Catalog bán hàng | Ảnh sản phẩm / thứ tự hiển thị | Cao | Should |
| `UC_POS_014` | Catalog bán hàng | Ngưng bán sản phẩm tạm thời | Bắt buộc | Must |
| `UC_POS_015` | Catalog bán hàng | Đồng bộ catalog từ back-office | Bắt buộc | Must |
| `UC_POS_016` | Bảng giá & thuế | Bảng giá theo điểm bán | Bắt buộc | Must |
| `UC_POS_017` | Bảng giá & thuế | Giá theo khung giờ | Cao | Should |
| `UC_POS_018` | Bảng giá & thuế | Giá theo ngày trong tuần | Trung bình | Could |
| `UC_POS_019` | Bảng giá & thuế | Cấu hình thuế GTGT | Bắt buộc | Must |
| `UC_POS_020` | Bảng giá & thuế | Làm tròn tiền | Cao | Should |
| `UC_POS_021` | Khuyến mại tại quầy | Áp dụng chương trình khuyến mại | Bắt buộc | Must |
| `UC_POS_022` | Khuyến mại tại quầy | Nhập mã voucher | Bắt buộc | Must |
| `UC_POS_023` | Khuyến mại tại quầy | Khuyến mại theo combo | Cao | Should |
| `UC_POS_024` | Khuyến mại tại quầy | Giảm giá tay có quyền | Bắt buộc | Must |
| `UC_POS_025` | Khuyến mại tại quầy | Báo cáo khuyến mại | Cao | Should |
| `UC_POS_026` | Giao dịch bán hàng | Mở đơn / chọn khu vực | Bắt buộc | Must |
| `UC_POS_027` | Giao dịch bán hàng | Thêm / sửa / xóa sản phẩm | Bắt buộc | Must |
| `UC_POS_028` | Giao dịch bán hàng | Tách bill / gộp bill | Cao | Should |
| `UC_POS_029` | Giao dịch bán hàng | Chuyển đơn giữa quầy | Cao | Should |
| `UC_POS_030` | Giao dịch bán hàng | Ghi chú đơn hàng | Cao | Should |
| `UC_POS_031` | Giao dịch bán hàng | Gửi lệnh khu vực chế biến | Cao | Should |
| `UC_POS_032` | Giao dịch bán hàng | Tạm tính / giữ đơn | Bắt buộc | Must |
| `UC_POS_033` | Giao dịch bán hàng | Thanh toán tiền mặt | Bắt buộc | Must |
| `UC_POS_034` | Giao dịch bán hàng | Thanh toán chuyển khoản / QR | Bắt buộc | Must |
| `UC_POS_035` | Giao dịch bán hàng | Thanh toán thẻ / ví điện tử | Bắt buộc | Must |
| `UC_POS_036` | Giao dịch bán hàng | Thanh toán hỗn hợp | Cao | Should |
| `UC_POS_037` | Giao dịch bán hàng | In hóa đơn | Bắt buộc | Must |
| `UC_POS_038` | Giao dịch bán hàng | Hủy sản phẩm | Bắt buộc | Must |
| `UC_POS_039` | Giao dịch bán hàng | Hủy cả bill | Bắt buộc | Must |
| `UC_POS_040` | Giao dịch bán hàng | Trả hàng / hoàn tiền | Bắt buộc | Must |
| `UC_POS_041` | Giao dịch bán hàng | Gợi ý bán kèm | Thấp | Won't / Later |
| `UC_POS_042` | Ca thu ngân & quỹ | Mở ca thu ngân | Bắt buộc | Must |
| `UC_POS_043` | Ca thu ngân & quỹ | Nhập tiền đầu ca | Bắt buộc | Must |
| `UC_POS_044` | Ca thu ngân & quỹ | Nộp tiền / rút tiền ca | Cao | Should |
| `UC_POS_045` | Ca thu ngân & quỹ | Xem doanh thu trong ca | Bắt buộc | Must |
| `UC_POS_046` | Ca thu ngân & quỹ | Đóng ca & đếm quỹ | Bắt buộc | Must |
| `UC_POS_047` | Ca thu ngân & quỹ | Đối soát lệch quỹ | Bắt buộc | Must |
| `UC_POS_048` | Ca thu ngân & quỹ | In báo cáo ca | Bắt buộc | Must |
| `UC_POS_049` | Ca thu ngân & quỹ | Duyệt xác nhận ca | Cao | Should |
| `UC_POS_050` | Khách hàng & loyalty | Gắn khách hàng vào đơn | Cao | Should |
| `UC_POS_051` | Khách hàng & loyalty | Tích điểm loyalty | Trung bình | Could |
| `UC_POS_052` | Khách hàng & loyalty | Đổi điểm / ưu đãi | Trung bình | Could |
| `UC_POS_053` | Khách hàng & loyalty | Tra cứu lịch sử mua | Cao | Should |
| `UC_POS_054` | Đồng bộ tồn & back-office | Trừ tồn theo BOM khi bán | Bắt buộc | Must |
| `UC_POS_055` | Đồng bộ tồn & back-office | Cảnh báo hết / sắp hết | Bắt buộc | Must |
| `UC_POS_056` | Đồng bộ tồn & back-office | Tạo đề nghị nhập hàng | Cao | Should |
| `UC_POS_057` | Đồng bộ tồn & back-office | Nhận hàng từ kho trung tâm | Cao | Should |
| `UC_POS_058` | Đồng bộ tồn & back-office | Kiểm kê nhanh | Cao | Should |
| `UC_POS_059` | Đồng bộ tồn & back-office | Đồng bộ doanh thu ca sang FIN | Bắt buộc | Must |
| `UC_POS_060` | Đồng bộ tồn & back-office | Đồng bộ đơn sang CRM | Trung bình | Could |
| `UC_POS_061` | Báo cáo POS | Doanh thu theo giờ / ngày / ca | Bắt buộc | Must |
| `UC_POS_062` | Báo cáo POS | Doanh thu theo sản phẩm | Bắt buộc | Must |
| `UC_POS_063` | Báo cáo POS | Doanh thu theo thu ngân | Bắt buộc | Must |
| `UC_POS_064` | Báo cáo POS | Tỷ lệ hủy / giảm giá | Bắt buộc | Must |
| `UC_POS_065` | Báo cáo POS | Cost lý thuyết vs thực tế | Cao | Should |
| `UC_POS_066` | Báo cáo POS | Top sản phẩm bán chạy | Cao | Should |
| `UC_POS_067` | Báo cáo POS | So sánh điểm bán | Cao | Should |
| `UC_POS_068` | Báo cáo POS | Xuất báo cáo POS | Bắt buộc | Must |
| `UC_POS_069` | Vận hành chuỗi | Giám sát doanh thu chuỗi realtime | Cao | Should |
| `UC_POS_070` | Vận hành chuỗi | Phân phối catalog / giá / khuyến mại | Bắt buộc | Must |
| `UC_POS_071` | Vận hành chuỗi | Chuẩn hóa catalog toàn chuỗi | Bắt buộc | Must |
| `UC_POS_072` | Vận hành chuỗi | Cấu hình target doanh thu | Cao | Should |

</details>

---

## 7. Đặc tả chức năng theo nhóm

Mỗi UC bên dưới gồm: mô tả, tác nhân, tiền/hậu điều kiện, luồng chính, quy tắc, tiêu chí chấp nhận và ưu tiên. Đây là mức đặc tả BA để chốt phạm vi; chi tiết UI/API sẽ bổ sung ở giai đoạn thiết kế.

### 7.1. Cấu hình điểm bán & thiết bị (`POS-01`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **Chain Admin / Store Manager**.

#### UC_POS_001 — Khai báo điểm bán POS

- **Mô tả:** POS location setup
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Khai báo điểm bán POS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_002 — Khai báo quầy / máy POS

- **Mô tả:** Terminal configuration
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Khai báo quầy / máy POS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_003 — Cấu hình máy in hóa đơn

- **Mô tả:** Receipt printer setup
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình máy in hóa đơn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_004 — Cấu hình máy in bếp/khu vực

- **Mô tả:** Kitchen printer setup
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình máy in bếp/khu vực” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_005 — Cấu hình ngăn kéo tiền

- **Mô tả:** Cash drawer setup
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình ngăn kéo tiền” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_006 — Cấu hình thiết bị quét mã

- **Mô tả:** Barcode scanner setup
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thiết bị quét mã” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_007 — Phân quyền thu ngân trên POS

- **Mô tả:** POS user roles
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân quyền thu ngân trên POS
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (POS user roles)
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
  - AC1: Thực hiện thành công thao tác “Phân quyền thu ngân trên POS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_008 — Chế độ offline tạm

- **Mô tả:** Offline mode
- **Tác nhân chính:** Chain Admin / Store Manager
- **Ưu tiên danh mục:** Thấp → **MoSCoW:** Won't / Later
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chế độ offline tạm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Offline mode)
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
  - AC1: Thực hiện thành công thao tác “Chế độ offline tạm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.2. Catalog bán hàng (`POS-02`)

Nhóm này gồm **7** chức năng. Tác nhân mặc định: **Chain Admin**.

#### UC_POS_009 — Danh mục nhóm sản phẩm

- **Mô tả:** Product categories
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục nhóm sản phẩm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_010 — Danh mục sản phẩm bán

- **Mô tả:** Sellable items
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục sản phẩm bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_011 — Thuộc tính sản phẩm

- **Mô tả:** Product variants/modifiers
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thuộc tính sản phẩm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Product variants/modifiers)
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
  - AC1: Thực hiện thành công thao tác “Thuộc tính sản phẩm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_012 — BOM / định mức nguyên liệu

- **Mô tả:** Product recipe
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: BOM / định mức nguyên liệu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Product recipe)
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
  - AC1: Thực hiện thành công thao tác “BOM / định mức nguyên liệu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_013 — Ảnh sản phẩm / thứ tự hiển thị

- **Mô tả:** Product display
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ảnh sản phẩm / thứ tự hiển thị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Product display)
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
  - AC1: Thực hiện thành công thao tác “Ảnh sản phẩm / thứ tự hiển thị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_014 — Ngưng bán sản phẩm tạm thời

- **Mô tả:** 86 item
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ngưng bán sản phẩm tạm thời
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (86 item)
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
  - AC1: Thực hiện thành công thao tác “Ngưng bán sản phẩm tạm thời” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_015 — Đồng bộ catalog từ back-office

- **Mô tả:** Catalog sync
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Kích hoạt đồng bộ thủ công hoặc theo sự kiện/job
  2. Hệ thống lấy dữ liệu nguồn và ánh xạ sang đích
  3. Ghi nhận kết quả/ thành công/ lỗi có thể retry
  4. Cập nhật trạng thái đồng bộ trên bản ghi liên quan
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Đồng bộ catalog từ back-office” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.3. Bảng giá & thuế (`POS-03`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **Chain Admin**.

#### UC_POS_016 — Bảng giá theo điểm bán

- **Mô tả:** Location price list
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng giá theo điểm bán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Location price list)
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
  - AC1: Thực hiện thành công thao tác “Bảng giá theo điểm bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_017 — Giá theo khung giờ

- **Mô tả:** Time-based pricing
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Giá theo khung giờ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Time-based pricing)
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
  - AC1: Thực hiện thành công thao tác “Giá theo khung giờ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_018 — Giá theo ngày trong tuần

- **Mô tả:** Day-based pricing
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Giá theo ngày trong tuần
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Day-based pricing)
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
  - AC1: Thực hiện thành công thao tác “Giá theo ngày trong tuần” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_019 — Cấu hình thuế GTGT

- **Mô tả:** Tax configuration
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thuế GTGT” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_020 — Làm tròn tiền

- **Mô tả:** Rounding rules
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Làm tròn tiền
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Rounding rules)
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
  - AC1: Thực hiện thành công thao tác “Làm tròn tiền” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.4. Khuyến mại tại quầy (`POS-04`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **Cashier / Store Manager**.

#### UC_POS_021 — Áp dụng chương trình khuyến mại

- **Mô tả:** Apply promotions
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Áp dụng chương trình khuyến mại
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Apply promotions)
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
  - AC1: Thực hiện thành công thao tác “Áp dụng chương trình khuyến mại” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_022 — Nhập mã voucher

- **Mô tả:** Voucher redemption
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhập mã voucher
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Voucher redemption)
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
  - AC1: Thực hiện thành công thao tác “Nhập mã voucher” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_023 — Khuyến mại theo combo

- **Mô tả:** Combo/bundle pricing
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khuyến mại theo combo
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Combo/bundle pricing)
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
  - AC1: Thực hiện thành công thao tác “Khuyến mại theo combo” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_024 — Giảm giá tay có quyền

- **Mô tả:** Manual discount with approval
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Giảm giá tay có quyền
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Manual discount with approval)
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
  - AC1: Thực hiện thành công thao tác “Giảm giá tay có quyền” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_025 — Báo cáo khuyến mại

- **Mô tả:** Promotion usage report
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo khuyến mại” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.5. Giao dịch bán hàng (`POS-05`)

Nhóm này gồm **16** chức năng. Tác nhân mặc định: **Cashier**.

#### UC_POS_026 — Mở đơn / chọn khu vực

- **Mô tả:** New transaction
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Mở đơn / chọn khu vực
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (New transaction)
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
  - AC1: Thực hiện thành công thao tác “Mở đơn / chọn khu vực” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_027 — Thêm / sửa / xóa sản phẩm

- **Mô tả:** Line item management
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thêm / sửa / xóa sản phẩm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Line item management)
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
  - AC1: Thực hiện thành công thao tác “Thêm / sửa / xóa sản phẩm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_028 — Tách bill / gộp bill

- **Mô tả:** Split/merge bills
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tách bill / gộp bill
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Split/merge bills)
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
  - AC1: Thực hiện thành công thao tác “Tách bill / gộp bill” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_029 — Chuyển đơn giữa quầy

- **Mô tả:** Transfer order
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển đơn giữa quầy
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Transfer order)
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
  - AC1: Thực hiện thành công thao tác “Chuyển đơn giữa quầy” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_030 — Ghi chú đơn hàng

- **Mô tả:** Order notes
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi chú đơn hàng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Order notes)
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
  - AC1: Thực hiện thành công thao tác “Ghi chú đơn hàng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_031 — Gửi lệnh khu vực chế biến

- **Mô tả:** Send to kitchen/preparation
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gửi lệnh khu vực chế biến
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Send to kitchen/preparation)
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
  - AC1: Thực hiện thành công thao tác “Gửi lệnh khu vực chế biến” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_032 — Tạm tính / giữ đơn

- **Mô tả:** Hold transaction
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tạm tính / giữ đơn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Hold transaction)
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
  - AC1: Thực hiện thành công thao tác “Tạm tính / giữ đơn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_033 — Thanh toán tiền mặt

- **Mô tả:** Cash payment
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thanh toán tiền mặt
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash payment)
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
  - AC1: Thực hiện thành công thao tác “Thanh toán tiền mặt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_034 — Thanh toán chuyển khoản / QR

- **Mô tả:** Bank transfer/QR payment
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thanh toán chuyển khoản / QR
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bank transfer/QR payment)
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
  - AC1: Thực hiện thành công thao tác “Thanh toán chuyển khoản / QR” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_035 — Thanh toán thẻ / ví điện tử

- **Mô tả:** Card/e-wallet payment
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thanh toán thẻ / ví điện tử
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Card/e-wallet payment)
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
  - AC1: Thực hiện thành công thao tác “Thanh toán thẻ / ví điện tử” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_036 — Thanh toán hỗn hợp

- **Mô tả:** Split payment
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thanh toán hỗn hợp
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Split payment)
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
  - AC1: Thực hiện thành công thao tác “Thanh toán hỗn hợp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_037 — In hóa đơn

- **Mô tả:** Print receipt
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: In hóa đơn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Print receipt)
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
  - AC1: Thực hiện thành công thao tác “In hóa đơn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_038 — Hủy sản phẩm

- **Mô tả:** Void item
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hủy sản phẩm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Void item)
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
  - AC1: Thực hiện thành công thao tác “Hủy sản phẩm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_039 — Hủy cả bill

- **Mô tả:** Void transaction
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hủy cả bill
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Void transaction)
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
  - AC1: Thực hiện thành công thao tác “Hủy cả bill” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_040 — Trả hàng / hoàn tiền

- **Mô tả:** Refund transaction
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Trả hàng / hoàn tiền
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Refund transaction)
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
  - AC1: Thực hiện thành công thao tác “Trả hàng / hoàn tiền” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_041 — Gợi ý bán kèm

- **Mô tả:** Upsell suggestions
- **Tác nhân chính:** Cashier
- **Ưu tiên danh mục:** Thấp → **MoSCoW:** Won't / Later
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gợi ý bán kèm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Upsell suggestions)
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
  - AC1: Thực hiện thành công thao tác “Gợi ý bán kèm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.6. Ca thu ngân & quỹ (`POS-06`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **Cashier / Store Manager**.

#### UC_POS_042 — Mở ca thu ngân

- **Mô tả:** Open shift
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Mở ca thu ngân
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Open shift)
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
  - AC1: Thực hiện thành công thao tác “Mở ca thu ngân” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_043 — Nhập tiền đầu ca

- **Mô tả:** Starting float
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhập tiền đầu ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Starting float)
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
  - AC1: Thực hiện thành công thao tác “Nhập tiền đầu ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_044 — Nộp tiền / rút tiền ca

- **Mô tả:** Cash in/out
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nộp tiền / rút tiền ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash in/out)
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
  - AC1: Thực hiện thành công thao tác “Nộp tiền / rút tiền ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_045 — Xem doanh thu trong ca

- **Mô tả:** Shift sales realtime
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem doanh thu trong ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Shift sales realtime)
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
  - AC1: Thực hiện thành công thao tác “Xem doanh thu trong ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_046 — Đóng ca & đếm quỹ

- **Mô tả:** Close shift & count
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đóng ca & đếm quỹ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Close shift & count)
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
  - AC1: Thực hiện thành công thao tác “Đóng ca & đếm quỹ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_047 — Đối soát lệch quỹ

- **Mô tả:** Cash variance
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đối soát lệch quỹ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash variance)
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
  - AC1: Thực hiện thành công thao tác “Đối soát lệch quỹ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_048 — In báo cáo ca

- **Mô tả:** Shift report
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “In báo cáo ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_049 — Duyệt xác nhận ca

- **Mô tả:** Shift approval
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt xác nhận ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.7. Khách hàng & loyalty (`POS-07`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **Cashier / Store Manager**.

#### UC_POS_050 — Gắn khách hàng vào đơn

- **Mô tả:** Customer identification
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn khách hàng vào đơn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Customer identification)
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
  - AC1: Thực hiện thành công thao tác “Gắn khách hàng vào đơn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_051 — Tích điểm loyalty

- **Mô tả:** Earn loyalty points
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tích điểm loyalty
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Earn loyalty points)
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
  - AC1: Thực hiện thành công thao tác “Tích điểm loyalty” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_052 — Đổi điểm / ưu đãi

- **Mô tả:** Redeem rewards
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đổi điểm / ưu đãi
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Redeem rewards)
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
  - AC1: Thực hiện thành công thao tác “Đổi điểm / ưu đãi” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_053 — Tra cứu lịch sử mua

- **Mô tả:** Purchase history
- **Tác nhân chính:** Cashier / Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tra cứu lịch sử mua
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Purchase history)
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
  - AC1: Thực hiện thành công thao tác “Tra cứu lịch sử mua” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.8. Đồng bộ tồn & back-office (`POS-08`)

Nhóm này gồm **7** chức năng. Tác nhân mặc định: **Store Manager / Inventory Clerk (CH)**.

#### UC_POS_054 — Trừ tồn theo BOM khi bán

- **Mô tả:** Auto inventory depletion
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Trừ tồn theo BOM khi bán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Auto inventory depletion)
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
  - AC1: Thực hiện thành công thao tác “Trừ tồn theo BOM khi bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_055 — Cảnh báo hết / sắp hết

- **Mô tả:** Low stock alert
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo hết / sắp hết
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Low stock alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo hết / sắp hết” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_056 — Tạo đề nghị nhập hàng

- **Mô tả:** Stock requisition
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo đề nghị nhập hàng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_057 — Nhận hàng từ kho trung tâm

- **Mô tả:** Receiving at location
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhận hàng từ kho trung tâm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Receiving at location)
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
  - AC1: Thực hiện thành công thao tác “Nhận hàng từ kho trung tâm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_058 — Kiểm kê nhanh

- **Mô tả:** Quick stocktake
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Kiểm kê nhanh
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Quick stocktake)
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
  - AC1: Thực hiện thành công thao tác “Kiểm kê nhanh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_059 — Đồng bộ doanh thu ca sang FIN

- **Mô tả:** Sales posting to GL
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Kích hoạt đồng bộ thủ công hoặc theo sự kiện/job
  2. Hệ thống lấy dữ liệu nguồn và ánh xạ sang đích
  3. Ghi nhận kết quả/ thành công/ lỗi có thể retry
  4. Cập nhật trạng thái đồng bộ trên bản ghi liên quan
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Đồng bộ doanh thu ca sang FIN” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_060 — Đồng bộ đơn sang CRM

- **Mô tả:** CRM sync
- **Tác nhân chính:** Store Manager / Inventory Clerk (CH)
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Kích hoạt đồng bộ thủ công hoặc theo sự kiện/job
  2. Hệ thống lấy dữ liệu nguồn và ánh xạ sang đích
  3. Ghi nhận kết quả/ thành công/ lỗi có thể retry
  4. Cập nhật trạng thái đồng bộ trên bản ghi liên quan
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Đồng bộ đơn sang CRM” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.9. Báo cáo POS (`POS-09`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **Store Manager**.

#### UC_POS_061 — Doanh thu theo giờ / ngày / ca

- **Mô tả:** Sales by time
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Doanh thu theo giờ / ngày / ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Sales by time)
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
  - AC1: Thực hiện thành công thao tác “Doanh thu theo giờ / ngày / ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_062 — Doanh thu theo sản phẩm

- **Mô tả:** Product mix
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Doanh thu theo sản phẩm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Product mix)
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
  - AC1: Thực hiện thành công thao tác “Doanh thu theo sản phẩm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_063 — Doanh thu theo thu ngân

- **Mô tả:** Cashier performance
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Doanh thu theo thu ngân
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cashier performance)
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
  - AC1: Thực hiện thành công thao tác “Doanh thu theo thu ngân” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_064 — Tỷ lệ hủy / giảm giá

- **Mô tả:** Void & discount analysis
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tỷ lệ hủy / giảm giá
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Void & discount analysis)
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
  - AC1: Thực hiện thành công thao tác “Tỷ lệ hủy / giảm giá” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_065 — Cost lý thuyết vs thực tế

- **Mô tả:** Recipe vs usage variance
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cost lý thuyết vs thực tế
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Recipe vs usage variance)
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
  - AC1: Thực hiện thành công thao tác “Cost lý thuyết vs thực tế” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_066 — Top sản phẩm bán chạy

- **Mô tả:** Bestsellers
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Top sản phẩm bán chạy
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bestsellers)
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
  - AC1: Thực hiện thành công thao tác “Top sản phẩm bán chạy” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_067 — So sánh điểm bán

- **Mô tả:** Location comparison
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: So sánh điểm bán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Location comparison)
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
  - AC1: Thực hiện thành công thao tác “So sánh điểm bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_068 — Xuất báo cáo POS

- **Mô tả:** Export POS reports
- **Tác nhân chính:** Store Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất báo cáo POS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.10. Vận hành chuỗi (`POS-10`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **Chain Admin**.

#### UC_POS_069 — Giám sát doanh thu chuỗi realtime

- **Mô tả:** Chain-wide monitoring
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Giám sát doanh thu chuỗi realtime
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Chain-wide monitoring)
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
  - AC1: Thực hiện thành công thao tác “Giám sát doanh thu chuỗi realtime” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_POS_070 — Phân phối catalog / giá / khuyến mại

- **Mô tả:** Push configuration
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân phối catalog / giá / khuyến mại
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Push configuration)
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
  - AC1: Thực hiện thành công thao tác “Phân phối catalog / giá / khuyến mại” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_071 — Chuẩn hóa catalog toàn chuỗi

- **Mô tả:** Master catalog
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuẩn hóa catalog toàn chuỗi
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Master catalog)
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
  - AC1: Thực hiện thành công thao tác “Chuẩn hóa catalog toàn chuỗi” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_POS_072 — Cấu hình target doanh thu

- **Mô tả:** Sales target by location
- **Tác nhân chính:** Chain Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `POS`.
  - License module `POS` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình target doanh thu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

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

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

### WF-POS-02 — Đẩy cấu hình chuỗi

**Mục tiêu:** Điểm bán nhận menu/giá/KM mới

| Bước | Mô tả |
|---:|---|
| 1 | Chain Admin cập nhật master catalog/giá/KM |
| 2 | Chọn điểm bán đích và phát hành |
| 3 | Terminal đồng bộ phiên bản cấu hình |
| 4 | Báo cáo điểm chưa đồng bộ |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

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

### 9.1. Xuất xứ & kiểm soát dữ liệu
- Master dùng chung (KH, SP, chi nhánh…) tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ nghiệp vụ có trạng thái vòng đời rõ ràng (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete hoặc trạng thái ngưng dùng là mặc định; hạn chế xóa cứng.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-POS-01: Hủy món/bill sau thanh toán hoặc sau gửi chế biến cần quyền riêng.
- BR-POS-02: Không đóng ca khi còn đơn mở (trừ force có quyền).
- BR-POS-03: Lệch quỹ phải ghi nhận lý do.
- BR-POS-04: Terminal chỉ bán SP đang active và còn cho phép tại cửa hàng.
- BR-POS-GEN-01: Mọi thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-POS-GEN-02: Mọi chứng từ có mã duy nhất theo rule Sequence của SYS.
- BR-POS-GEN-03: Thao tác sau khi khóa kỳ/chốt sổ (nếu có) phải đi đường điều chỉnh có kiểm soát.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Tốc độ | Thêm món và thanh toán cảm nhận < 1s trên LAN cửa hàng |
| Sẵn sàng | Hỗ trợ hàng đợi offline hạn chế (tùy gói) |
| In ấn | Tương thích máy in nhiệt phổ biến |
| Usability | Form có validate rõ; bảng có lọc/phân trang; hỗ trợ tiếng Việt |
| Reliability | Không mất chứng từ đã post; giao dịch quan trọng atomic |
| Maintainability | Permission và cấu hình không hard-code trong source nghiệp vụ |
| Observability | Có log ứng dụng + audit nghiệp vụ tách bạch |

---

## 12. Tích hợp & sự kiện

### 12.1. Ma trận tích hợp

| Thành phần | Mô tả |
|---|---|
| INV | Trừ tồn, nhận hàng CH, kiểm kê nhanh |
| FIN | Post doanh thu/ca/quỹ |
| CRM | KM, loyalty, gắn KH |
| Payment QR/Card | Cổng thanh toán |

### 12.2. Sự kiện (logical)
- `POS.EntityCreated` / `POS.EntityUpdated` / `POS.EntityStatusChanged`
- `POS.DocumentSubmitted` / `POS.DocumentApproved` / `POS.DocumentPosted`
- Mapping cụ thể API/topic sẽ định nghĩa ở tài liệu Interface Spec sau khi chốt SRS.

---

## 13. Phân quyền & bảo mật

### 13.1. Permission catalog (đề xuất)

- `pos.sale.create`
- `pos.discount.apply`
- `pos.void.approve`
- `pos.shift.open_close`
- `pos.config.manage`
- `pos.report.view`

### 13.2. Nguyên tắc
- Deny by default; chỉ mở theo role.
- Data scope theo chi nhánh/kho/đơn vị do SYS quyết định.
- Field-level security cho dữ liệu nhạy cảm (lương, công nợ chi tiết, giá vốn…) khi áp dụng.
- Mọi thay đổi phân quyền và thao tác critical ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Doanh thu theo giờ/ca/CH | Giám sát vận hành module `POS` |
| Tỷ lệ void/discount | Giám sát vận hành module `POS` |
| Thời gian phục vụ trung bình | Giám sát vận hành module `POS` |
| Food/material cost variance (khi có recipe+INV) | Giám sát vận hành module `POS` |

Báo cáo chi tiết vận hành nằm trong từng nhóm “Báo cáo…” của Mục 7; tổng hợp điều hành nằm trên module `BI` khi khách mua thêm.

---

## 15. Giả định, rủi ro & câu hỏi mở

### 15.1. Giả định
- Mỗi điểm bán thuộc một chi nhánh/org trong SYS.

### 15.2. Câu hỏi mở cần chốt
- Phase 1 có KDS bếp đầy đủ hay chỉ in lệnh?
- Có hỗ trợ đặt bàn/đặt trước không?

### 15.3. Rủi ro
- Phụ thuộc module khác chưa mua → một số workflow E2E chỉ chạy được một phần (cần nêu rõ khi bán gói).
- Cấu hình quá linh hoạt có thể làm tăng effort QA; cần bộ template mặc định.
- Chưa chốt chuẩn kế toán/thuế chi tiết có thể ảnh hưởng FIN và posting.

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện nghiệm thu module
1. 100% UC ưu tiên **Bắt buộc (Must)** của `POS` pass UAT.
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
| Use case IDs | `UC_POS_001` … `UC_POS_072` |

---

*Hết tài liệu SRS-POS-v1.0.*
