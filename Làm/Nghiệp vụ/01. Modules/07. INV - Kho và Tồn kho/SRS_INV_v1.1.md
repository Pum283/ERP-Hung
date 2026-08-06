# SRS-INV-v1.1 — Kho & Tồn kho

> **Software Requirements Specification — Module INV**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-INV-v1.1` |
| Module | `INV` — Kho & Tồn kho |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Chuỗi cung ứng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `PUR`, `LOG`, `FIN`, `POS`, `MFG` |
| Số nhóm / UC | 11 nhóm / 70 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Kho & Tồn kho** (`INV`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module INV quản lý master hàng hóa, đa kho/vị trí, nhập–xuất–chuyển, tồn khả dụng/giữ hàng, lô–HSD–serial, kiểm kê, giá trị kho và báo cáo xuất nhập tồn.

### 1.3. Mục tiêu đo được
1. Tồn realtime chính xác (on-hand / reserved / available).
2. Truy vết lô/serial/HSD.
3. Hỗ trợ nhiều mục đích xuất (bán, SX, kỹ thuật, dự án).
4. Đồng bộ giá trị kho với FIN.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Item master, warehouse/bin, stock moves, reservation, lot/serial/expiry, stocktake, valuation, INV reports.

### 2.2. Out of Scope
- Mua hàng (PUR).
- Điều phối giao hàng (LOG).
- BOM/lệnh SX (MFG).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `PUR`, `LOG`, `FIN`, `POS`, `MFG`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Inventory Admin | Master hàng & cấu hình kho |
| Storekeeper | Nhập/xuất/chuyển/kiểm kê |
| Planner | Xem tồn khả dụng, đề nghị cấp hàng |
| Auditor | Đối chiếu kiểm kê |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| On-hand | Tồn thực tế trong kho |
| Reserved | Tồn đang giữ cho đơn/lệnh |
| Available | On-hand − Reserved (− policy khác) |
| FEFO | First Expiry First Out |
| Stocktake | Kiểm kê |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- INV (Kho & Tồn kho)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `INV` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `INV` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | PUR | GRN nhập |
| Tích hợp | LOG | Xuất giao |
| Tích hợp | POS | Trừ recipe/SP |
| Tích hợp | MFG | Xuất NVL – nhập TP |
| Tích hợp | FSM | Xuất linh kiện |
| Tích hợp | PJM | Xuất vật tư dự án |
| Tích hợp | FIN | Inventory posting |

---

## 6. Catalog chức năng

**Tổng:** 11 nhóm · 70 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `INV-01` | Danh mục sản phẩm | 10 | 8 | 1 | 1 |
| 2 | `INV-02` | Cấu hình kho & vị trí | 6 | 5 | 1 | 0 |
| 3 | `INV-03` | Nhập kho | 7 | 5 | 2 | 0 |
| 4 | `INV-04` | Xuất kho | 7 | 5 | 2 | 0 |
| 5 | `INV-05` | Chuyển kho | 6 | 4 | 2 | 0 |
| 6 | `INV-06` | Giữ hàng & tồn khả dụng | 6 | 6 | 0 | 0 |
| 7 | `INV-07` | Lô – HSD – Serial | 6 | 4 | 2 | 0 |
| 8 | `INV-08` | Kiểm kê | 7 | 5 | 2 | 0 |
| 9 | `INV-09` | Yêu cầu xuất / đề nghị hàng | 4 | 0 | 4 | 0 |
| 10 | `INV-10` | Giá trị kho & kế toán kho | 4 | 4 | 0 | 0 |
| 11 | `INV-11` | Báo cáo kho | 7 | 5 | 2 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_INV_001` | Danh mục sản phẩm | Tạo / sửa SKU sản phẩm | Must |
| `UC_INV_002` | Danh mục sản phẩm | Phân nhóm hàng / ngành hàng | Must |
| `UC_INV_003` | Danh mục sản phẩm | Đơn vị tính & quy đổi | Must |
| `UC_INV_004` | Danh mục sản phẩm | Thuộc tính hàng (lô, serial, HSD) | Must |
| `UC_INV_005` | Danh mục sản phẩm | Giá vốn / phương pháp tính giá | Must |
| `UC_INV_006` | Danh mục sản phẩm | Ảnh & mô tả sản phẩm | Could |
| `UC_INV_007` | Danh mục sản phẩm | Ngưng sử dụng SKU | Must |
| `UC_INV_008` | Danh mục sản phẩm | Import / export danh mục SP | Must |
| `UC_INV_009` | Danh mục sản phẩm | Barcode / QR theo sản phẩm | Should |
| `UC_INV_010` | Danh mục sản phẩm | Định mức tồn min/max/reorder | Must |
| `UC_INV_011` | Cấu hình kho & vị trí | Tạo kho | Must |
| `UC_INV_012` | Cấu hình kho & vị trí | Loại kho | Must |
| `UC_INV_013` | Cấu hình kho & vị trí | Vị trí / kệ / bin | Should |
| `UC_INV_014` | Cấu hình kho & vị trí | Gán thủ kho / quyền | Must |
| `UC_INV_015` | Cấu hình kho & vị trí | Cấu hình FEFO / FIFO | Must |
| `UC_INV_016` | Cấu hình kho & vị trí | Cho phép tồn âm hay không | Must |
| `UC_INV_017` | Nhập kho | Nhập từ mua hàng | Must |
| `UC_INV_018` | Nhập kho | Nhập từ sản xuất | Must |
| `UC_INV_019` | Nhập kho | Nhập điều chỉnh / kiểm kê | Must |
| `UC_INV_020` | Nhập kho | Nhập chuyển đến | Must |
| `UC_INV_021` | Nhập kho | Nhập trả từ khách | Should |
| `UC_INV_022` | Nhập kho | Nhập theo lô / HSD / serial | Must |
| `UC_INV_023` | Nhập kho | In tem lô / serial | Should |
| `UC_INV_024` | Xuất kho | Xuất bán / giao hàng | Must |
| `UC_INV_025` | Xuất kho | Xuất sản xuất | Must |
| `UC_INV_026` | Xuất kho | Xuất nội bộ / tiêu hao | Must |
| `UC_INV_027` | Xuất kho | Xuất cho dịch vụ kỹ thuật | Should |
| `UC_INV_028` | Xuất kho | Xuất cho dự án | Should |
| `UC_INV_029` | Xuất kho | Xuất theo FEFO tự động | Must |
| `UC_INV_030` | Xuất kho | Xuất điều chỉnh | Must |
| `UC_INV_031` | Chuyển kho | Tạo phiếu chuyển kho | Must |
| `UC_INV_032` | Chuyển kho | Duyệt chuyển kho | Should |
| `UC_INV_033` | Chuyển kho | Xuất bên gửi / nhập bên nhận | Must |
| `UC_INV_034` | Chuyển kho | Chuyển kho một bước | Should |
| `UC_INV_035` | Chuyển kho | Theo dõi hàng đang chuyển | Must |
| `UC_INV_036` | Chuyển kho | Chuyển từ kho trung tâm | Must |
| `UC_INV_037` | Giữ hàng & tồn khả dụng | Giữ hàng theo đơn đã duyệt | Must |
| `UC_INV_038` | Giữ hàng & tồn khả dụng | Giải phóng giữ hàng | Must |
| `UC_INV_039` | Giữ hàng & tồn khả dụng | Xem tồn thực tế | Must |
| `UC_INV_040` | Giữ hàng & tồn khả dụng | Xem tồn khả dụng | Must |
| `UC_INV_041` | Giữ hàng & tồn khả dụng | Xem tồn đang giữ / đang chuyển | Must |
| `UC_INV_042` | Giữ hàng & tồn khả dụng | Cảnh báo không đủ tồn | Must |
| `UC_INV_043` | Lô – HSD – Serial | Theo dõi tồn theo lô | Must |
| `UC_INV_044` | Lô – HSD – Serial | Cảnh báo cận date / quá date | Must |
| `UC_INV_045` | Lô – HSD – Serial | Chặn xuất hàng quá HSD | Must |
| `UC_INV_046` | Lô – HSD – Serial | Theo dõi serial | Should |
| `UC_INV_047` | Lô – HSD – Serial | Truy vết lô xuôi/ngược | Should |
| `UC_INV_048` | Lô – HSD – Serial | Báo cáo hàng sắp hết hạn | Must |
| `UC_INV_049` | Kiểm kê | Tạo phiếu kiểm kê | Must |
| `UC_INV_050` | Kiểm kê | Nhập số đếm thực tế | Must |
| `UC_INV_051` | Kiểm kê | Kiểm kê theo vị trí / nhóm | Should |
| `UC_INV_052` | Kiểm kê | Đối chiếu lệch kiểm kê | Must |
| `UC_INV_053` | Kiểm kê | Duyệt điều chỉnh sau kiểm kê | Must |
| `UC_INV_054` | Kiểm kê | Khóa giao dịch khi đang kiểm kê | Should |
| `UC_INV_055` | Kiểm kê | Báo cáo kết quả kiểm kê | Must |
| `UC_INV_056` | Yêu cầu xuất / đề nghị hàng | Đề nghị xuất nội bộ | Should |
| `UC_INV_057` | Yêu cầu xuất / đề nghị hàng | Đề nghị cấp hàng | Should |
| `UC_INV_058` | Yêu cầu xuất / đề nghị hàng | Duyệt đề nghị | Should |
| `UC_INV_059` | Yêu cầu xuất / đề nghị hàng | Chuyển đề nghị thành phiếu xuất | Should |
| `UC_INV_060` | Giá trị kho & kế toán kho | Xem giá trị tồn | Must |
| `UC_INV_061` | Giá trị kho & kế toán kho | Tính lại giá vốn | Must |
| `UC_INV_062` | Giá trị kho & kế toán kho | Đẩy bút toán kho sang FIN | Must |
| `UC_INV_063` | Giá trị kho & kế toán kho | Báo cáo giá trị tồn | Must |
| `UC_INV_064` | Báo cáo kho | Xuất nhập tồn theo kỳ | Must |
| `UC_INV_065` | Báo cáo kho | Thẻ kho / lịch sử sản phẩm | Must |
| `UC_INV_066` | Báo cáo kho | Hàng chậm luân chuyển | Should |
| `UC_INV_067` | Báo cáo kho | Hàng dưới min / trên max | Must |
| `UC_INV_068` | Báo cáo kho | Báo cáo xuất theo mục đích | Should |
| `UC_INV_069` | Báo cáo kho | Dashboard tồn & cảnh báo | Must |
| `UC_INV_070` | Báo cáo kho | Xuất báo cáo kho Excel | Must |

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

### 7.1. Danh mục sản phẩm (`INV-01`)

Nhóm **Danh mục sản phẩm** gồm **10** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 8 |

**Bảng 1. Đặc tả Use Case "Tạo / sửa SKU sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_001 |
| **Tên Use Case** | Tạo / sửa SKU sản phẩm |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Tạo / sửa SKU sản phẩm" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Item master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo / sửa SKU sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo / sửa SKU sản phẩm» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo / sửa SKU sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở chức năng «Tạo / sửa SKU sản phẩm» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo / sửa SKU sản phẩm» (Item master).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo / sửa SKU sản phẩm» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo / sửa SKU sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 2. Đặc tả Use Case "Phân nhóm hàng / ngành hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_002 |
| **Tên Use Case** | Phân nhóm hàng / ngành hàng |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Phân nhóm hàng / ngành hàng" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Category hierarchy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân nhóm hàng / ngành hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân nhóm hàng / ngành hàng» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân nhóm hàng / ngành hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Phân nhóm hàng / ngành hàng» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Category hierarchy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân nhóm hàng / ngành hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân nhóm hàng / ngành hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 3. Đặc tả Use Case "Đơn vị tính & quy đổi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_003 |
| **Tên Use Case** | Đơn vị tính & quy đổi |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Đơn vị tính & quy đổi" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: UOM & conversion |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đơn vị tính & quy đổi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đơn vị tính & quy đổi» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đơn vị tính & quy đổi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Đơn vị tính & quy đổi» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (UOM & conversion).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đơn vị tính & quy đổi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đơn vị tính & quy đổi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 4. Đặc tả Use Case "Thuộc tính hàng (lô, serial, HSD)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_004 |
| **Tên Use Case** | Thuộc tính hàng (lô, serial, HSD) |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Thuộc tính hàng (lô, serial, HSD)" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Tracking attributes |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thuộc tính hàng (lô, serial, HSD)» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thuộc tính hàng (lô, serial, HSD)» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thuộc tính hàng (lô, serial, HSD)» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Thuộc tính hàng (lô, serial, HSD)» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Tracking attributes).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thuộc tính hàng (lô, serial, HSD)».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thuộc tính hàng (lô, serial, HSD)» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 5. Đặc tả Use Case "Giá vốn / phương pháp tính giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_005 |
| **Tên Use Case** | Giá vốn / phương pháp tính giá |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Giá vốn / phương pháp tính giá" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Costing method |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giá vốn / phương pháp tính giá» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu nguồn (công, tồn, tỷ giá…) đã sẵn sàng và đạt điều kiện chốt. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-CALC-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giá vốn / phương pháp tính giá» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giá vốn / phương pháp tính giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Kết quả tính toán tái lập được với cùng input/rule (deterministic trong cùng phiên bản rule).<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin chọn phạm vi tính toán cho «Giá vốn / phương pháp tính giá» (kỳ, đơn vị, bộ lọc).<br>2. Hệ thống nạp dữ liệu nguồn liên quan (Costing method).<br>3. Chạy engine tính theo rule cấu hình; log chi tiết từng bước lỗi nếu có.<br>4. Hiển thị kết quả nháp để rà soát; cho phép điều chỉnh có audit trước khi chốt.<br>5. Xác nhận ghi nhận kết quả chính thức; phát sự kiện cho FIN/module liên quan nếu cần.<br>6. Thông báo hoàn tất và cập nhật trạng thái kỳ/tính toán. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giá vốn / phương pháp tính giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thiếu dữ liệu nguồn hoặc rule cấu hình không đầy đủ → dừng job, liệt kê lỗi chi tiết để sửa.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 6. Đặc tả Use Case "Ảnh & mô tả sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_006 |
| **Tên Use Case** | Ảnh & mô tả sản phẩm |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Ảnh & mô tả sản phẩm" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Product media |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ảnh & mô tả sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ảnh & mô tả sản phẩm» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ảnh & mô tả sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Ảnh & mô tả sản phẩm» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Product media).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ảnh & mô tả sản phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ảnh & mô tả sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 7. Đặc tả Use Case "Ngưng sử dụng SKU"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_007 |
| **Tên Use Case** | Ngưng sử dụng SKU |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Ngưng sử dụng SKU" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Discontinue item |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ngưng sử dụng SKU» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ngưng sử dụng SKU» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ngưng sử dụng SKU» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Ngưng sử dụng SKU» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Discontinue item).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ngưng sử dụng SKU».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ngưng sử dụng SKU» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 8. Đặc tả Use Case "Import / export danh mục SP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_008 |
| **Tên Use Case** | Import / export danh mục SP |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Import / export danh mục SP" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Item master import/export |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import / export danh mục SP» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import / export danh mục SP» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import / export danh mục SP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở «Import / export danh mục SP», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Import / export danh mục SP» (Item master import/export).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import / export danh mục SP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 9. Đặc tả Use Case "Barcode / QR theo sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_009 |
| **Tên Use Case** | Barcode / QR theo sản phẩm |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Barcode / QR theo sản phẩm" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Barcode management |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Barcode / QR theo sản phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Barcode / QR theo sản phẩm» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Barcode / QR theo sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Barcode / QR theo sản phẩm» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Barcode management).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Barcode / QR theo sản phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Barcode / QR theo sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 10. Đặc tả Use Case "Định mức tồn min/max/reorder"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_010 |
| **Tên Use Case** | Định mức tồn min/max/reorder |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Định mức tồn min/max/reorder" thuộc nhóm Danh mục sản phẩm trong module INV — Kho & Tồn kho. Mô tả chi tiết: Reorder parameters |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Định mức tồn min/max/reorder» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Định mức tồn min/max/reorder» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Định mức tồn min/max/reorder» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Định mức tồn min/max/reorder» trong nhóm Danh mục sản phẩm.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Reorder parameters).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Định mức tồn min/max/reorder».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Định mức tồn min/max/reorder» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.2. Cấu hình kho & vị trí (`INV-02`)

Nhóm **Cấu hình kho & vị trí** gồm **6** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 5 |

**Bảng 11. Đặc tả Use Case "Tạo kho"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_011 |
| **Tên Use Case** | Tạo kho |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Tạo kho" thuộc nhóm Cấu hình kho & vị trí trong module INV — Kho & Tồn kho. Mô tả chi tiết: Warehouse master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo kho» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo kho» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo kho» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở chức năng «Tạo kho» trong nhóm Cấu hình kho & vị trí.<br>2. Hệ thống kiểm tra license `INV`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo kho» (Warehouse master).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo kho» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo kho» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 12. Đặc tả Use Case "Loại kho"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_012 |
| **Tên Use Case** | Loại kho |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Loại kho" thuộc nhóm Cấu hình kho & vị trí trong module INV — Kho & Tồn kho. Mô tả chi tiết: Warehouse type |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Loại kho» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Loại kho» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Loại kho» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Loại kho» trong nhóm Cấu hình kho & vị trí.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Warehouse type).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Loại kho».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Loại kho» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 13. Đặc tả Use Case "Vị trí / kệ / bin"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_013 |
| **Tên Use Case** | Vị trí / kệ / bin |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Vị trí / kệ / bin" thuộc nhóm Cấu hình kho & vị trí trong module INV — Kho & Tồn kho. Mô tả chi tiết: Bin location |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Vị trí / kệ / bin» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Vị trí / kệ / bin» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Vị trí / kệ / bin» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Vị trí / kệ / bin» trong nhóm Cấu hình kho & vị trí.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Bin location).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Vị trí / kệ / bin».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Vị trí / kệ / bin» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 14. Đặc tả Use Case "Gán thủ kho / quyền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_014 |
| **Tên Use Case** | Gán thủ kho / quyền |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Gán thủ kho / quyền" thuộc nhóm Cấu hình kho & vị trí trong module INV — Kho & Tồn kho. Mô tả chi tiết: Warehouse access control |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán thủ kho / quyền» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán thủ kho / quyền» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán thủ kho / quyền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin chọn đối tượng nguồn trong «Gán thủ kho / quyền».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán thủ kho / quyền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 15. Đặc tả Use Case "Cấu hình FEFO / FIFO"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_015 |
| **Tên Use Case** | Cấu hình FEFO / FIFO |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Cấu hình FEFO / FIFO" thuộc nhóm Cấu hình kho & vị trí trong module INV — Kho & Tồn kho. Mô tả chi tiết: Lot issue strategy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình FEFO / FIFO» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình FEFO / FIFO» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình FEFO / FIFO» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở màn hình cấu hình «Cấu hình FEFO / FIFO» trong Cấu hình kho & vị trí.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Lot issue strategy) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình FEFO / FIFO» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 16. Đặc tả Use Case "Cho phép tồn âm hay không"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_016 |
| **Tên Use Case** | Cho phép tồn âm hay không |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Cho phép tồn âm hay không" thuộc nhóm Cấu hình kho & vị trí trong module INV — Kho & Tồn kho. Mô tả chi tiết: Negative stock policy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cho phép tồn âm hay không» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cho phép tồn âm hay không» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cho phép tồn âm hay không» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Cho phép tồn âm hay không» trong nhóm Cấu hình kho & vị trí.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Negative stock policy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Cho phép tồn âm hay không».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cho phép tồn âm hay không» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.3. Nhập kho (`INV-03`)

Nhóm **Nhập kho** gồm **7** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 17. Đặc tả Use Case "Nhập từ mua hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_017 |
| **Tên Use Case** | Nhập từ mua hàng |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập từ mua hàng" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Purchase receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập từ mua hàng» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập từ mua hàng» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập từ mua hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập từ mua hàng» trong nhóm Nhập kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Purchase receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập từ mua hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập từ mua hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 18. Đặc tả Use Case "Nhập từ sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_018 |
| **Tên Use Case** | Nhập từ sản xuất |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập từ sản xuất" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Production receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập từ sản xuất» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập từ sản xuất» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập từ sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập từ sản xuất» trong nhóm Nhập kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Production receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập từ sản xuất».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập từ sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 19. Đặc tả Use Case "Nhập điều chỉnh / kiểm kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_019 |
| **Tên Use Case** | Nhập điều chỉnh / kiểm kê |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập điều chỉnh / kiểm kê" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Adjustment receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập điều chỉnh / kiểm kê» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập điều chỉnh / kiểm kê» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập điều chỉnh / kiểm kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper tìm và mở bản ghi liên quan tới «Nhập điều chỉnh / kiểm kê» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Nhập điều chỉnh / kiểm kê» (Adjustment receipt).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập điều chỉnh / kiểm kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 20. Đặc tả Use Case "Nhập chuyển đến"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_020 |
| **Tên Use Case** | Nhập chuyển đến |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập chuyển đến" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Transfer-in receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập chuyển đến» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập chuyển đến» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập chuyển đến» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập chuyển đến» trong nhóm Nhập kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Transfer-in receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập chuyển đến».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập chuyển đến» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 21. Đặc tả Use Case "Nhập trả từ khách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_021 |
| **Tên Use Case** | Nhập trả từ khách |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập trả từ khách" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Sales return receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập trả từ khách» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập trả từ khách» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập trả từ khách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập trả từ khách» trong nhóm Nhập kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Sales return receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập trả từ khách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập trả từ khách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 22. Đặc tả Use Case "Nhập theo lô / HSD / serial"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_022 |
| **Tên Use Case** | Nhập theo lô / HSD / serial |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập theo lô / HSD / serial" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Lot/serial receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập theo lô / HSD / serial» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập theo lô / HSD / serial» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập theo lô / HSD / serial» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập theo lô / HSD / serial» trong nhóm Nhập kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Lot/serial receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập theo lô / HSD / serial».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập theo lô / HSD / serial» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 23. Đặc tả Use Case "In tem lô / serial"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_023 |
| **Tên Use Case** | In tem lô / serial |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "In tem lô / serial" thuộc nhóm Nhập kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Label printing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In tem lô / serial» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In tem lô / serial» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In tem lô / serial» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «In tem lô / serial» trong nhóm Nhập kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Label printing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «In tem lô / serial».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In tem lô / serial» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.4. Xuất kho (`INV-04`)

Nhóm **Xuất kho** gồm **7** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 24. Đặc tả Use Case "Xuất bán / giao hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_024 |
| **Tên Use Case** | Xuất bán / giao hàng |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất bán / giao hàng" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Sales issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất bán / giao hàng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất bán / giao hàng» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất bán / giao hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất bán / giao hàng», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất bán / giao hàng» (Sales issue).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất bán / giao hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 25. Đặc tả Use Case "Xuất sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_025 |
| **Tên Use Case** | Xuất sản xuất |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất sản xuất" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Production issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất sản xuất» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất sản xuất» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất sản xuất», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất sản xuất» (Production issue).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 26. Đặc tả Use Case "Xuất nội bộ / tiêu hao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_026 |
| **Tên Use Case** | Xuất nội bộ / tiêu hao |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất nội bộ / tiêu hao" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Internal consumption |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất nội bộ / tiêu hao» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất nội bộ / tiêu hao» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất nội bộ / tiêu hao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất nội bộ / tiêu hao», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất nội bộ / tiêu hao» (Internal consumption).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất nội bộ / tiêu hao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 27. Đặc tả Use Case "Xuất cho dịch vụ kỹ thuật"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_027 |
| **Tên Use Case** | Xuất cho dịch vụ kỹ thuật |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất cho dịch vụ kỹ thuật" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Service issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất cho dịch vụ kỹ thuật» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất cho dịch vụ kỹ thuật» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất cho dịch vụ kỹ thuật» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất cho dịch vụ kỹ thuật», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất cho dịch vụ kỹ thuật» (Service issue).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất cho dịch vụ kỹ thuật» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 28. Đặc tả Use Case "Xuất cho dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_028 |
| **Tên Use Case** | Xuất cho dự án |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất cho dự án" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Project issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất cho dự án» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất cho dự án» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất cho dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất cho dự án», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất cho dự án» (Project issue).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất cho dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 29. Đặc tả Use Case "Xuất theo FEFO tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_029 |
| **Tên Use Case** | Xuất theo FEFO tự động |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất theo FEFO tự động" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Auto lot picking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất theo FEFO tự động» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất theo FEFO tự động» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất theo FEFO tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất theo FEFO tự động», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất theo FEFO tự động» (Auto lot picking).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất theo FEFO tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 30. Đặc tả Use Case "Xuất điều chỉnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_030 |
| **Tên Use Case** | Xuất điều chỉnh |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất điều chỉnh" thuộc nhóm Xuất kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Adjustment issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất điều chỉnh» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất điều chỉnh» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất điều chỉnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất điều chỉnh», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất điều chỉnh» (Adjustment issue).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất điều chỉnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.5. Chuyển kho (`INV-05`)

Nhóm **Chuyển kho** gồm **6** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 31. Đặc tả Use Case "Tạo phiếu chuyển kho"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_031 |
| **Tên Use Case** | Tạo phiếu chuyển kho |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Tạo phiếu chuyển kho" thuộc nhóm Chuyển kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Transfer order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phiếu chuyển kho» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phiếu chuyển kho» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phiếu chuyển kho» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở chức năng «Tạo phiếu chuyển kho» trong nhóm Chuyển kho.<br>2. Hệ thống kiểm tra license `INV`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phiếu chuyển kho» (Transfer order).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phiếu chuyển kho» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phiếu chuyển kho» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 32. Đặc tả Use Case "Duyệt chuyển kho"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_032 |
| **Tên Use Case** | Duyệt chuyển kho |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Duyệt chuyển kho" thuộc nhóm Chuyển kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Transfer approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt chuyển kho» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt chuyển kho» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt chuyển kho» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Storekeeper mở hộp chờ / chứng từ cần xử lý cho «Duyệt chuyển kho».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt chuyển kho», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt chuyển kho» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 33. Đặc tả Use Case "Xuất bên gửi / nhập bên nhận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_033 |
| **Tên Use Case** | Xuất bên gửi / nhập bên nhận |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xuất bên gửi / nhập bên nhận" thuộc nhóm Chuyển kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Two-step transfer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất bên gửi / nhập bên nhận» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất bên gửi / nhập bên nhận» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất bên gửi / nhập bên nhận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xuất bên gửi / nhập bên nhận», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất bên gửi / nhập bên nhận» (Two-step transfer).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất bên gửi / nhập bên nhận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 34. Đặc tả Use Case "Chuyển kho một bước"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_034 |
| **Tên Use Case** | Chuyển kho một bước |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Chuyển kho một bước" thuộc nhóm Chuyển kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: One-step transfer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển kho một bước» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển kho một bước» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển kho một bước» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Chuyển kho một bước» trong nhóm Chuyển kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (One-step transfer).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển kho một bước».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển kho một bước» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 35. Đặc tả Use Case "Theo dõi hàng đang chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_035 |
| **Tên Use Case** | Theo dõi hàng đang chuyển |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Theo dõi hàng đang chuyển" thuộc nhóm Chuyển kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: In-transit inventory |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi hàng đang chuyển» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi hàng đang chuyển» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi hàng đang chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Theo dõi hàng đang chuyển» trong nhóm Chuyển kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (In-transit inventory).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi hàng đang chuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi hàng đang chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 36. Đặc tả Use Case "Chuyển từ kho trung tâm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_036 |
| **Tên Use Case** | Chuyển từ kho trung tâm |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Chuyển từ kho trung tâm" thuộc nhóm Chuyển kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Replenish locations |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển từ kho trung tâm» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển từ kho trung tâm» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển từ kho trung tâm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Chuyển từ kho trung tâm» trong nhóm Chuyển kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Replenish locations).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển từ kho trung tâm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển từ kho trung tâm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.6. Giữ hàng & tồn khả dụng (`INV-06`)

Nhóm **Giữ hàng & tồn khả dụng** gồm **6** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 6 |

**Bảng 37. Đặc tả Use Case "Giữ hàng theo đơn đã duyệt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_037 |
| **Tên Use Case** | Giữ hàng theo đơn đã duyệt |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Giữ hàng theo đơn đã duyệt" thuộc nhóm Giữ hàng & tồn khả dụng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Stock reservation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giữ hàng theo đơn đã duyệt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giữ hàng theo đơn đã duyệt» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giữ hàng theo đơn đã duyệt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở hộp chờ / chứng từ cần xử lý cho «Giữ hàng theo đơn đã duyệt».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Giữ hàng theo đơn đã duyệt», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giữ hàng theo đơn đã duyệt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 38. Đặc tả Use Case "Giải phóng giữ hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_038 |
| **Tên Use Case** | Giải phóng giữ hàng |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Giải phóng giữ hàng" thuộc nhóm Giữ hàng & tồn khả dụng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Release reservation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giải phóng giữ hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giải phóng giữ hàng» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giải phóng giữ hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Giải phóng giữ hàng» trong nhóm Giữ hàng & tồn khả dụng.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Release reservation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giải phóng giữ hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giải phóng giữ hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 39. Đặc tả Use Case "Xem tồn thực tế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_039 |
| **Tên Use Case** | Xem tồn thực tế |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xem tồn thực tế" thuộc nhóm Giữ hàng & tồn khả dụng trong module INV — Kho & Tồn kho. Mô tả chi tiết: On-hand inventory |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem tồn thực tế» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem tồn thực tế» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem tồn thực tế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xem tồn thực tế» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (On-hand inventory).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem tồn thực tế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 40. Đặc tả Use Case "Xem tồn khả dụng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_040 |
| **Tên Use Case** | Xem tồn khả dụng |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xem tồn khả dụng" thuộc nhóm Giữ hàng & tồn khả dụng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Available inventory |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem tồn khả dụng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem tồn khả dụng» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem tồn khả dụng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xem tồn khả dụng» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Available inventory).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem tồn khả dụng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 41. Đặc tả Use Case "Xem tồn đang giữ / đang chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_041 |
| **Tên Use Case** | Xem tồn đang giữ / đang chuyển |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Xem tồn đang giữ / đang chuyển" thuộc nhóm Giữ hàng & tồn khả dụng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Reserved & in-transit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem tồn đang giữ / đang chuyển» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem tồn đang giữ / đang chuyển» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem tồn đang giữ / đang chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Xem tồn đang giữ / đang chuyển» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Reserved & in-transit).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem tồn đang giữ / đang chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 42. Đặc tả Use Case "Cảnh báo không đủ tồn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_042 |
| **Tên Use Case** | Cảnh báo không đủ tồn |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Cảnh báo không đủ tồn" thuộc nhóm Giữ hàng & tồn khả dụng trong module INV — Kho & Tồn kho. Mô tả chi tiết: ATP check |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo không đủ tồn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo không đủ tồn» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo không đủ tồn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Storekeeper kích hoạt kiểm tra điều kiện «Cảnh báo không đủ tồn».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (ATP check).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo không đủ tồn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.7. Lô – HSD – Serial (`INV-07`)

Nhóm **Lô – HSD – Serial** gồm **6** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 43. Đặc tả Use Case "Theo dõi tồn theo lô"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_043 |
| **Tên Use Case** | Theo dõi tồn theo lô |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Theo dõi tồn theo lô" thuộc nhóm Lô – HSD – Serial trong module INV — Kho & Tồn kho. Mô tả chi tiết: Lot balance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi tồn theo lô» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi tồn theo lô» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi tồn theo lô» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Theo dõi tồn theo lô» trong nhóm Lô – HSD – Serial.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Lot balance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi tồn theo lô».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi tồn theo lô» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 44. Đặc tả Use Case "Cảnh báo cận date / quá date"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_044 |
| **Tên Use Case** | Cảnh báo cận date / quá date |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Cảnh báo cận date / quá date" thuộc nhóm Lô – HSD – Serial trong module INV — Kho & Tồn kho. Mô tả chi tiết: Expiry alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo cận date / quá date» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo cận date / quá date» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo cận date / quá date» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Storekeeper kích hoạt kiểm tra điều kiện «Cảnh báo cận date / quá date».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Expiry alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo cận date / quá date» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 45. Đặc tả Use Case "Chặn xuất hàng quá HSD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_045 |
| **Tên Use Case** | Chặn xuất hàng quá HSD |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Chặn xuất hàng quá HSD" thuộc nhóm Lô – HSD – Serial trong module INV — Kho & Tồn kho. Mô tả chi tiết: Block expired issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chặn xuất hàng quá HSD» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chặn xuất hàng quá HSD» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chặn xuất hàng quá HSD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Chặn xuất hàng quá HSD» trong nhóm Lô – HSD – Serial.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Block expired issue).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chặn xuất hàng quá HSD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chặn xuất hàng quá HSD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 46. Đặc tả Use Case "Theo dõi serial"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_046 |
| **Tên Use Case** | Theo dõi serial |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Theo dõi serial" thuộc nhóm Lô – HSD – Serial trong module INV — Kho & Tồn kho. Mô tả chi tiết: Serial tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi serial» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi serial» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi serial» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Theo dõi serial» trong nhóm Lô – HSD – Serial.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Serial tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi serial».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi serial» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 47. Đặc tả Use Case "Truy vết lô xuôi/ngược"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_047 |
| **Tên Use Case** | Truy vết lô xuôi/ngược |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Truy vết lô xuôi/ngược" thuộc nhóm Lô – HSD – Serial trong module INV — Kho & Tồn kho. Mô tả chi tiết: Lot traceability |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Truy vết lô xuôi/ngược» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Truy vết lô xuôi/ngược» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Truy vết lô xuôi/ngược» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Truy vết lô xuôi/ngược» trong nhóm Lô – HSD – Serial.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Lot traceability).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Truy vết lô xuôi/ngược».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Truy vết lô xuôi/ngược» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 48. Đặc tả Use Case "Báo cáo hàng sắp hết hạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_048 |
| **Tên Use Case** | Báo cáo hàng sắp hết hạn |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Báo cáo hàng sắp hết hạn" thuộc nhóm Lô – HSD – Serial trong module INV — Kho & Tồn kho. Mô tả chi tiết: Near-expiry report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo hàng sắp hết hạn» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo hàng sắp hết hạn» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo hàng sắp hết hạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Báo cáo hàng sắp hết hạn» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Near-expiry report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo hàng sắp hết hạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.8. Kiểm kê (`INV-08`)

Nhóm **Kiểm kê** gồm **7** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 49. Đặc tả Use Case "Tạo phiếu kiểm kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_049 |
| **Tên Use Case** | Tạo phiếu kiểm kê |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Tạo phiếu kiểm kê" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Stocktake session |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phiếu kiểm kê» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phiếu kiểm kê» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phiếu kiểm kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở chức năng «Tạo phiếu kiểm kê» trong nhóm Kiểm kê.<br>2. Hệ thống kiểm tra license `INV`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phiếu kiểm kê» (Stocktake session).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phiếu kiểm kê» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phiếu kiểm kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 50. Đặc tả Use Case "Nhập số đếm thực tế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_050 |
| **Tên Use Case** | Nhập số đếm thực tế |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập số đếm thực tế" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Count entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập số đếm thực tế» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập số đếm thực tế» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập số đếm thực tế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập số đếm thực tế» trong nhóm Kiểm kê.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Count entry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập số đếm thực tế».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập số đếm thực tế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 51. Đặc tả Use Case "Kiểm kê theo vị trí / nhóm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_051 |
| **Tên Use Case** | Kiểm kê theo vị trí / nhóm |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Kiểm kê theo vị trí / nhóm" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Cycle count |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm kê theo vị trí / nhóm» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm kê theo vị trí / nhóm» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm kê theo vị trí / nhóm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Kiểm kê theo vị trí / nhóm» trong nhóm Kiểm kê.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Cycle count).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm kê theo vị trí / nhóm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm kê theo vị trí / nhóm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 52. Đặc tả Use Case "Đối chiếu lệch kiểm kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_052 |
| **Tên Use Case** | Đối chiếu lệch kiểm kê |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Đối chiếu lệch kiểm kê" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Count variance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối chiếu lệch kiểm kê» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối chiếu lệch kiểm kê» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối chiếu lệch kiểm kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Đối chiếu lệch kiểm kê» trong nhóm Kiểm kê.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Count variance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối chiếu lệch kiểm kê».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối chiếu lệch kiểm kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 53. Đặc tả Use Case "Duyệt điều chỉnh sau kiểm kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_053 |
| **Tên Use Case** | Duyệt điều chỉnh sau kiểm kê |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Duyệt điều chỉnh sau kiểm kê" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Post adjustment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt điều chỉnh sau kiểm kê» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt điều chỉnh sau kiểm kê» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt điều chỉnh sau kiểm kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở hộp chờ / chứng từ cần xử lý cho «Duyệt điều chỉnh sau kiểm kê».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt điều chỉnh sau kiểm kê», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt điều chỉnh sau kiểm kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 54. Đặc tả Use Case "Khóa giao dịch khi đang kiểm kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_054 |
| **Tên Use Case** | Khóa giao dịch khi đang kiểm kê |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Khóa giao dịch khi đang kiểm kê" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Freeze transactions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa giao dịch khi đang kiểm kê» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa giao dịch khi đang kiểm kê» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa giao dịch khi đang kiểm kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. Storekeeper chọn kỳ/ca/đối tượng cần khóa trong «Khóa giao dịch khi đang kiểm kê».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa giao dịch khi đang kiểm kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 55. Đặc tả Use Case "Báo cáo kết quả kiểm kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_055 |
| **Tên Use Case** | Báo cáo kết quả kiểm kê |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Báo cáo kết quả kiểm kê" thuộc nhóm Kiểm kê trong module INV — Kho & Tồn kho. Mô tả chi tiết: Count report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo kết quả kiểm kê» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo kết quả kiểm kê» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo kết quả kiểm kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở «Báo cáo kết quả kiểm kê» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Count report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo kết quả kiểm kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.9. Yêu cầu xuất / đề nghị hàng (`INV-09`)

Nhóm **Yêu cầu xuất / đề nghị hàng** gồm **4** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 56. Đặc tả Use Case "Đề nghị xuất nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_056 |
| **Tên Use Case** | Đề nghị xuất nội bộ |
| **Tác nhân** | Planner |
| **Mô tả chức năng** | Cho phép Planner thực hiện chức năng "Đề nghị xuất nội bộ" thuộc nhóm Yêu cầu xuất / đề nghị hàng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Material request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Planner] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề nghị xuất nội bộ» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề nghị xuất nội bộ» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề nghị xuất nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Planner khởi tạo thao tác «Đề nghị xuất nội bộ» trong nhóm Yêu cầu xuất / đề nghị hàng.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Material request).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đề nghị xuất nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề nghị xuất nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 57. Đặc tả Use Case "Đề nghị cấp hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_057 |
| **Tên Use Case** | Đề nghị cấp hàng |
| **Tác nhân** | Planner |
| **Mô tả chức năng** | Cho phép Planner thực hiện chức năng "Đề nghị cấp hàng" thuộc nhóm Yêu cầu xuất / đề nghị hàng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Store requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Planner] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề nghị cấp hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề nghị cấp hàng» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề nghị cấp hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Planner khởi tạo thao tác «Đề nghị cấp hàng» trong nhóm Yêu cầu xuất / đề nghị hàng.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Store requisition).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đề nghị cấp hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề nghị cấp hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 58. Đặc tả Use Case "Duyệt đề nghị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_058 |
| **Tên Use Case** | Duyệt đề nghị |
| **Tác nhân** | Planner |
| **Mô tả chức năng** | Cho phép Planner thực hiện chức năng "Duyệt đề nghị" thuộc nhóm Yêu cầu xuất / đề nghị hàng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Approve requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Planner] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt đề nghị» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt đề nghị» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt đề nghị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Planner mở hộp chờ / chứng từ cần xử lý cho «Duyệt đề nghị».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt đề nghị», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt đề nghị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 59. Đặc tả Use Case "Chuyển đề nghị thành phiếu xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_059 |
| **Tên Use Case** | Chuyển đề nghị thành phiếu xuất |
| **Tác nhân** | Planner |
| **Mô tả chức năng** | Cho phép Planner thực hiện chức năng "Chuyển đề nghị thành phiếu xuất" thuộc nhóm Yêu cầu xuất / đề nghị hàng trong module INV — Kho & Tồn kho. Mô tả chi tiết: Convert to issue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Planner] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển đề nghị thành phiếu xuất» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển đề nghị thành phiếu xuất» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển đề nghị thành phiếu xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Planner khởi tạo thao tác «Chuyển đề nghị thành phiếu xuất» trong nhóm Yêu cầu xuất / đề nghị hàng.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Convert to issue).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển đề nghị thành phiếu xuất».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển đề nghị thành phiếu xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.10. Giá trị kho & kế toán kho (`INV-10`)

Nhóm **Giá trị kho & kế toán kho** gồm **4** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 4 |

**Bảng 60. Đặc tả Use Case "Xem giá trị tồn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_060 |
| **Tên Use Case** | Xem giá trị tồn |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Xem giá trị tồn" thuộc nhóm Giá trị kho & kế toán kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Stock valuation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem giá trị tồn» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem giá trị tồn» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem giá trị tồn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở «Xem giá trị tồn» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Stock valuation).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem giá trị tồn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 61. Đặc tả Use Case "Tính lại giá vốn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_061 |
| **Tên Use Case** | Tính lại giá vốn |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Tính lại giá vốn" thuộc nhóm Giá trị kho & kế toán kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Cost recalculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính lại giá vốn» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu nguồn (công, tồn, tỷ giá…) đã sẵn sàng và đạt điều kiện chốt. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-CALC-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính lại giá vốn» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính lại giá vốn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Kết quả tính toán tái lập được với cùng input/rule (deterministic trong cùng phiên bản rule).<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin chọn phạm vi tính toán cho «Tính lại giá vốn» (kỳ, đơn vị, bộ lọc).<br>2. Hệ thống nạp dữ liệu nguồn liên quan (Cost recalculation).<br>3. Chạy engine tính theo rule cấu hình; log chi tiết từng bước lỗi nếu có.<br>4. Hiển thị kết quả nháp để rà soát; cho phép điều chỉnh có audit trước khi chốt.<br>5. Xác nhận ghi nhận kết quả chính thức; phát sự kiện cho FIN/module liên quan nếu cần.<br>6. Thông báo hoàn tất và cập nhật trạng thái kỳ/tính toán. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính lại giá vốn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thiếu dữ liệu nguồn hoặc rule cấu hình không đầy đủ → dừng job, liệt kê lỗi chi tiết để sửa.<br>8.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 62. Đặc tả Use Case "Đẩy bút toán kho sang FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_062 |
| **Tên Use Case** | Đẩy bút toán kho sang FIN |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Đẩy bút toán kho sang FIN" thuộc nhóm Giá trị kho & kế toán kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Inventory GL posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đẩy bút toán kho sang FIN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đẩy bút toán kho sang FIN» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đẩy bút toán kho sang FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Đẩy bút toán kho sang FIN» trong nhóm Giá trị kho & kế toán kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Inventory GL posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đẩy bút toán kho sang FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đẩy bút toán kho sang FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 63. Đặc tả Use Case "Báo cáo giá trị tồn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_063 |
| **Tên Use Case** | Báo cáo giá trị tồn |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Báo cáo giá trị tồn" thuộc nhóm Giá trị kho & kế toán kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Inventory value report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo giá trị tồn» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo giá trị tồn» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo giá trị tồn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở «Báo cáo giá trị tồn» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Inventory value report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo giá trị tồn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

### 7.11. Báo cáo kho (`INV-11`)

Nhóm **Báo cáo kho** gồm **7** use case của module `INV`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 64. Đặc tả Use Case "Xuất nhập tồn theo kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_064 |
| **Tên Use Case** | Xuất nhập tồn theo kỳ |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Xuất nhập tồn theo kỳ" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Stock movement report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất nhập tồn theo kỳ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất nhập tồn theo kỳ» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất nhập tồn theo kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở «Xuất nhập tồn theo kỳ», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất nhập tồn theo kỳ» (Stock movement report).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất nhập tồn theo kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 65. Đặc tả Use Case "Thẻ kho / lịch sử sản phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_065 |
| **Tên Use Case** | Thẻ kho / lịch sử sản phẩm |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Thẻ kho / lịch sử sản phẩm" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Item ledger |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thẻ kho / lịch sử sản phẩm» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thẻ kho / lịch sử sản phẩm» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thẻ kho / lịch sử sản phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở «Thẻ kho / lịch sử sản phẩm» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Item ledger).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thẻ kho / lịch sử sản phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 66. Đặc tả Use Case "Hàng chậm luân chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_066 |
| **Tên Use Case** | Hàng chậm luân chuyển |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Hàng chậm luân chuyển" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Slow-moving analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hàng chậm luân chuyển» đã được cấu hình trong phạm vi data scope.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hàng chậm luân chuyển» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hàng chậm luân chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Hàng chậm luân chuyển» trong nhóm Báo cáo kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Slow-moving analysis).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hàng chậm luân chuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hàng chậm luân chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 67. Đặc tả Use Case "Hàng dưới min / trên max"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_067 |
| **Tên Use Case** | Hàng dưới min / trên max |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Hàng dưới min / trên max" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Reorder report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hàng dưới min / trên max» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hàng dưới min / trên max» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hàng dưới min / trên max» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin khởi tạo thao tác «Hàng dưới min / trên max» trong nhóm Báo cáo kho.<br>2. Hệ thống kiểm tra license `INV`, quyền RBAC và tiền điều kiện nghiệp vụ (Reorder report).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hàng dưới min / trên max».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hàng dưới min / trên max» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 68. Đặc tả Use Case "Báo cáo xuất theo mục đích"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_068 |
| **Tên Use Case** | Báo cáo xuất theo mục đích |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Báo cáo xuất theo mục đích" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Issue by purpose |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo xuất theo mục đích» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo xuất theo mục đích» được lưu nhất quán trong module `INV`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo xuất theo mục đích» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Inventory Admin mở «Báo cáo xuất theo mục đích» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Issue by purpose); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo xuất theo mục đích» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 69. Đặc tả Use Case "Dashboard tồn & cảnh báo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_069 |
| **Tên Use Case** | Dashboard tồn & cảnh báo |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Dashboard tồn & cảnh báo" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Inventory dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard tồn & cảnh báo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard tồn & cảnh báo» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard tồn & cảnh báo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Inventory Admin kích hoạt kiểm tra điều kiện «Dashboard tồn & cảnh báo».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Inventory dashboard).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard tồn & cảnh báo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

**Bảng 70. Đặc tả Use Case "Xuất báo cáo kho Excel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_INV_070 |
| **Tên Use Case** | Xuất báo cáo kho Excel |
| **Tác nhân** | Inventory Admin |
| **Mô tả chức năng** | Cho phép Inventory Admin thực hiện chức năng "Xuất báo cáo kho Excel" thuộc nhóm Báo cáo kho trong module INV — Kho & Tồn kho. Mô tả chi tiết: Export inventory reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Inventory Admin] và được cấp quyền RBAC tương ứng.<br>• License module `INV` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo kho Excel» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kho/vị trí thao tác thuộc data scope và còn hiệu lực. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-INV-SCOPE-01`, `BR-INV-AUD-01`, `BR-INV-STOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo kho Excel» được lưu nhất quán trong module `INV`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo kho Excel» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Inventory Admin mở «Xuất báo cáo kho Excel», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo kho Excel» (Export inventory reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo kho Excel» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê. |

---

## 8. Workflow end-to-end

### WF-INV-01 — Nhập – giữ – xuất bán

**Mục tiêu:** Đảm bảo ATP cho đơn hàng

| Bước | Mô tả |
|---:|---|
| 1 | Nhập kho từ GRN/SX/điều chỉnh |
| 2 | Đơn bán duyệt → reserve |
| 3 | Xuất kho theo lệnh giao/bán; trừ reserve + on-hand |
| 4 | Cập nhật thẻ kho và giá trị |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-INV-02 — Kiểm kê định kỳ

**Mục tiêu:** Chốt lệch tồn có duyệt

| Bước | Mô tả |
|---:|---|
| 1 | Tạo đợt kiểm kê; tùy chọn đóng băng giao dịch |
| 2 | Nhập số đếm; đối chiếu lệch |
| 3 | Duyệt điều chỉnh; post phiếu điều chỉnh |
| 4 | Báo cáo kết quả |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Item / UomConversion` | Hàng hóa |
| `Warehouse / Bin` | Kho & vị trí |
| `StockBalance / Lot / Serial` | Tồn & truy vết |
| `StockDocument (In/Out/Transfer/Adjust)` | Chứng từ kho |
| `Reservation` | Giữ hàng |
| `StocktakeSession` | Kiểm kê |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `INV`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-INV-01: Không cho xuất vượt available (trừ khi policy tồn âm bật).
- BR-INV-02: Hàng quản lý HSD mặc định xuất FEFO.
- BR-INV-03: Hàng quá HSD bị chặn xuất.
- BR-INV-04: Mọi thay đổi tồn phải qua chứng từ; không sửa số dư tay.
- BR-INV-05: Reserve phải được release khi hủy nguồn giữ hàng.
- BR-INV-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-INV-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-INV-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Realtime | Số tồn hiển thị gần realtime sau post chứng từ |
| Khối lượng | Hỗ trợ hàng trăm kho và hàng chục nghìn SKU |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `INV` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `inv.item.manage` | Quyền chức năng module |
| `inv.warehouse.manage` | Quyền chức năng module |
| `inv.doc.post` | Quyền chức năng module |
| `inv.reservation.manage` | Quyền chức năng module |
| `inv.stocktake.manage` | Quyền chức năng module |
| `inv.report.view` | Quyền chức năng module |
| `inv.*.view` | Xem trong data scope |
| `inv.*.manage` | Tạo/sửa trong data scope |
| `inv.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Độ chính xác tồn | Theo dõi vận hành module |
| Hàng cận date | Theo dõi vận hành module |
| Slow-moving | Theo dõi vận hành module |
| Giá trị tồn theo kho | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Phương pháp giá vốn cấu hình theo tenant/item (TBXQ/FIFO…).

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Phase 1 có quản lý bin/location chi tiết hay chỉ mức kho?

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
| Bản SRS này | `SRS_INV_v1.1.md` / `.docx` |
| UC IDs | `UC_INV_001` … |

---

*Hết tài liệu SRS-INV-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
