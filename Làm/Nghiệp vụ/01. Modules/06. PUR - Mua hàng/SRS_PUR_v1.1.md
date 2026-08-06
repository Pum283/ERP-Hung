# SRS-PUR-v1.1 — Mua hàng (Procurement)

> **Software Requirements Specification — Module PUR**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-PUR-v1.1` |
| Module | `PUR` — Mua hàng (Procurement) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Chuỗi cung ứng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `INV`, `FIN`, `WF` |
| Số nhóm / UC | 9 nhóm / 52 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Mua hàng (Procurement)** (`PUR`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Quản lý NCC, nhu cầu mua (PR), RFQ, PO, nhận hàng, hóa đơn mua và báo cáo chi tiêu.

### 1.3. Mục tiêu đo được
1. Chuẩn hóa quy trình PR→PO→GRN→Invoice.
2. Kiểm soát duyệt theo hạn mức.
3. Hỗ trợ đối soát 3 chiều với kho và kế toán.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Vendor, price, PR, RFQ, PO, receiving, AP invoice match, purchase reports.

### 2.2. Out of Scope
- Quản lý tồn chi tiết (INV).
- Thanh toán sổ quỹ đầy đủ (FIN).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `INV`, `FIN`, `WF`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Buyer | Tạo PR/PO, RFQ |
| Requester | Yêu cầu mua từ đơn vị |
| Purchasing Manager | Duyệt mua, chọn NCC |
| Warehouse Receiver | Nhận hàng theo PO |
| AP Clerk | Hóa đơn NCC / 3-way match |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| PR | Purchase Requisition — yêu cầu mua |
| RFQ | Request for Quotation |
| PO | Purchase Order |
| GRN | Goods Receipt Note |
| 3-way match | Đối chiếu PO–GRN–Invoice |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- PUR (Mua hàng (Procurement))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `PUR` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `PUR` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | INV | Nhập kho từ GRN, cảnh báo tồn min→PR |
| Tích hợp | FIN | AP, tạm ứng NCC |
| Tích hợp | WF | Duyệt PR/PO/thanh toán |
| Tích hợp | PRT | Portal NCC xác nhận PO (tùy gói) |

---

## 6. Catalog chức năng

**Tổng:** 9 nhóm · 52 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `PUR-01` | Danh mục nhà cung cấp | 8 | 2 | 5 | 1 |
| 2 | `PUR-02` | Nguồn cung & giá mua | 5 | 2 | 2 | 1 |
| 3 | `PUR-03` | Yêu cầu mua hàng (PR) | 7 | 4 | 3 | 0 |
| 4 | `PUR-04` | Báo giá & chọn nhà cung cấp (RFQ) | 5 | 0 | 5 | 0 |
| 5 | `PUR-05` | Đơn mua hàng (PO) | 8 | 7 | 1 | 0 |
| 6 | `PUR-06` | Nhận hàng & trả nhà cung cấp | 6 | 3 | 3 | 0 |
| 7 | `PUR-07` | Hóa đơn mua & đối soát | 5 | 3 | 2 | 0 |
| 8 | `PUR-08` | Hợp đồng mua & khung giá | 3 | 0 | 3 | 0 |
| 9 | `PUR-09` | Báo cáo mua hàng | 5 | 3 | 1 | 1 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_PUR_001` | Danh mục nhà cung cấp | Tạo / cập nhật nhà cung cấp | Must |
| `UC_PUR_002` | Danh mục nhà cung cấp | Phân loại nhóm nhà cung cấp | Should |
| `UC_PUR_003` | Danh mục nhà cung cấp | Người liên hệ & điều khoản | Must |
| `UC_PUR_004` | Danh mục nhà cung cấp | Lead time & MOQ | Should |
| `UC_PUR_005` | Danh mục nhà cung cấp | Đánh giá chất lượng nhà cung cấp | Should |
| `UC_PUR_006` | Danh mục nhà cung cấp | Blacklist / ngưng dùng | Should |
| `UC_PUR_007` | Danh mục nhà cung cấp | Import danh sách nhà cung cấp | Could |
| `UC_PUR_008` | Danh mục nhà cung cấp | Hồ sơ pháp lý | Should |
| `UC_PUR_009` | Nguồn cung & giá mua | Gắn sản phẩm – nhà cung cấp | Must |
| `UC_PUR_010` | Nguồn cung & giá mua | Bảng giá mua theo nhà cung cấp | Must |
| `UC_PUR_011` | Nguồn cung & giá mua | Hiệu lực bảng giá mua | Should |
| `UC_PUR_012` | Nguồn cung & giá mua | Lịch sử giá mua | Should |
| `UC_PUR_013` | Nguồn cung & giá mua | Cảnh báo tăng giá bất thường | Could |
| `UC_PUR_014` | Yêu cầu mua hàng (PR) | Tạo PR từ đơn vị | Must |
| `UC_PUR_015` | Yêu cầu mua hàng (PR) | Tạo PR từ cảnh báo tồn min | Should |
| `UC_PUR_016` | Yêu cầu mua hàng (PR) | Gộp nhiều nhu cầu thành PR | Should |
| `UC_PUR_017` | Yêu cầu mua hàng (PR) | Luồng duyệt PR | Must |
| `UC_PUR_018` | Yêu cầu mua hàng (PR) | Từ chối / trả lại PR | Must |
| `UC_PUR_019` | Yêu cầu mua hàng (PR) | Theo dõi trạng thái PR | Must |
| `UC_PUR_020` | Yêu cầu mua hàng (PR) | Hủy PR | Should |
| `UC_PUR_021` | Báo giá & chọn nhà cung cấp (RFQ) | Tạo RFQ gửi nhiều nhà cung cấp | Should |
| `UC_PUR_022` | Báo giá & chọn nhà cung cấp (RFQ) | Nhập báo giá từ nhà cung cấp | Should |
| `UC_PUR_023` | Báo giá & chọn nhà cung cấp (RFQ) | So sánh giá / điều kiện | Should |
| `UC_PUR_024` | Báo giá & chọn nhà cung cấp (RFQ) | Chọn nhà cung cấp thắng | Should |
| `UC_PUR_025` | Báo giá & chọn nhà cung cấp (RFQ) | Chuyển RFQ thành PO | Should |
| `UC_PUR_026` | Đơn mua hàng (PO) | Tạo PO từ PR/RFQ | Must |
| `UC_PUR_027` | Đơn mua hàng (PO) | Duyệt PO theo hạn mức | Must |
| `UC_PUR_028` | Đơn mua hàng (PO) | Gửi PO cho nhà cung cấp | Must |
| `UC_PUR_029` | Đơn mua hàng (PO) | Xác nhận PO từ nhà cung cấp | Should |
| `UC_PUR_030` | Đơn mua hàng (PO) | Sửa PO phiên bản | Must |
| `UC_PUR_031` | Đơn mua hàng (PO) | Theo dõi nhận hàng từng phần | Must |
| `UC_PUR_032` | Đơn mua hàng (PO) | Đóng / hủy PO | Must |
| `UC_PUR_033` | Đơn mua hàng (PO) | In / xuất PO | Must |
| `UC_PUR_034` | Nhận hàng & trả nhà cung cấp | Tạo phiếu nhận hàng theo PO | Must |
| `UC_PUR_035` | Nhận hàng & trả nhà cung cấp | Nhận hàng lệch số lượng / chất lượng | Must |
| `UC_PUR_036` | Nhận hàng & trả nhà cung cấp | Từ chối lô hàng không đạt | Should |
| `UC_PUR_037` | Nhận hàng & trả nhà cung cấp | Đẩy nhập kho sang INV | Must |
| `UC_PUR_038` | Nhận hàng & trả nhà cung cấp | Trả hàng nhà cung cấp | Should |
| `UC_PUR_039` | Nhận hàng & trả nhà cung cấp | Biên bản giao nhận | Should |
| `UC_PUR_040` | Hóa đơn mua & đối soát | Nhập hóa đơn nhà cung cấp | Must |
| `UC_PUR_041` | Hóa đơn mua & đối soát | Đối soát 3 chiều PO–GRN–Invoice | Must |
| `UC_PUR_042` | Hóa đơn mua & đối soát | Xử lý chênh lệch | Should |
| `UC_PUR_043` | Hóa đơn mua & đối soát | Đẩy công nợ sang FIN AP | Must |
| `UC_PUR_044` | Hóa đơn mua & đối soát | Tạm ứng nhà cung cấp | Should |
| `UC_PUR_045` | Hợp đồng mua & khung giá | Hợp đồng mua khung | Should |
| `UC_PUR_046` | Hợp đồng mua & khung giá | Theo dõi sản lượng / giá trị còn lại | Should |
| `UC_PUR_047` | Hợp đồng mua & khung giá | Cảnh báo hết hạn hợp đồng | Should |
| `UC_PUR_048` | Báo cáo mua hàng | Báo cáo mua theo nhà cung cấp / SP | Must |
| `UC_PUR_049` | Báo cáo mua hàng | Báo cáo đúng hạn giao hàng | Should |
| `UC_PUR_050` | Báo cáo mua hàng | Báo cáo tiết kiệm từ RFQ | Could |
| `UC_PUR_051` | Báo cáo mua hàng | Open PR / Open PO aging | Must |
| `UC_PUR_052` | Báo cáo mua hàng | Xuất báo cáo mua hàng | Must |

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

### 7.1. Danh mục nhà cung cấp (`PUR-01`)

Nhóm **Danh mục nhà cung cấp** gồm **8** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 2 |

**Bảng 1. Đặc tả Use Case "Tạo / cập nhật nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_001 |
| **Tên Use Case** | Tạo / cập nhật nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Tạo / cập nhật nhà cung cấp" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo / cập nhật nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo / cập nhật nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo / cập nhật nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer mở chức năng «Tạo / cập nhật nhà cung cấp» trong nhóm Danh mục nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo / cập nhật nhà cung cấp» (Vendor master).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo / cập nhật nhà cung cấp» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo / cập nhật nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 2. Đặc tả Use Case "Phân loại nhóm nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_002 |
| **Tên Use Case** | Phân loại nhóm nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Phân loại nhóm nhà cung cấp" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor classification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại nhóm nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại nhóm nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại nhóm nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Phân loại nhóm nhà cung cấp» trong nhóm Danh mục nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor classification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại nhóm nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại nhóm nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Người liên hệ & điều khoản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_003 |
| **Tên Use Case** | Người liên hệ & điều khoản |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Người liên hệ & điều khoản" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Contact & payment terms |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Người liên hệ & điều khoản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Người liên hệ & điều khoản» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Người liên hệ & điều khoản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Người liên hệ & điều khoản» trong nhóm Danh mục nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Contact & payment terms).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Người liên hệ & điều khoản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Người liên hệ & điều khoản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Lead time & MOQ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_004 |
| **Tên Use Case** | Lead time & MOQ |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Lead time & MOQ" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Supply parameters |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lead time & MOQ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lead time & MOQ» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lead time & MOQ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Lead time & MOQ» trong nhóm Danh mục nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Supply parameters).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lead time & MOQ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lead time & MOQ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 5. Đặc tả Use Case "Đánh giá chất lượng nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_005 |
| **Tên Use Case** | Đánh giá chất lượng nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Đánh giá chất lượng nhà cung cấp" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor scorecard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá chất lượng nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá chất lượng nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá chất lượng nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Đánh giá chất lượng nhà cung cấp» trong nhóm Danh mục nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor scorecard).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá chất lượng nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá chất lượng nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 6. Đặc tả Use Case "Blacklist / ngưng dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_006 |
| **Tên Use Case** | Blacklist / ngưng dùng |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Blacklist / ngưng dùng" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Block vendor |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Blacklist / ngưng dùng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`, `BR-PUR-CAN-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Blacklist / ngưng dùng» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Blacklist / ngưng dùng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer chọn đối tượng cần hủy/ngưng trong «Blacklist / ngưng dùng».<br>2. Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.<br>3. Yêu cầu lý do; xác nhận cảnh báo tác động.<br>4. Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.<br>5. Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Blacklist / ngưng dùng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 7. Đặc tả Use Case "Import danh sách nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_007 |
| **Tên Use Case** | Import danh sách nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Import danh sách nhà cung cấp" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor import |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import danh sách nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`, `BR-PUR-IMP-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import danh sách nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import danh sách nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer tải file mẫu (nếu có) và chọn file import cho «Import danh sách nhà cung cấp».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import danh sách nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn. |

**Bảng 8. Đặc tả Use Case "Hồ sơ pháp lý"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_008 |
| **Tên Use Case** | Hồ sơ pháp lý |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Hồ sơ pháp lý" thuộc nhóm Danh mục nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hồ sơ pháp lý» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hồ sơ pháp lý» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hồ sơ pháp lý» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Hồ sơ pháp lý» trong nhóm Danh mục nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor documents).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hồ sơ pháp lý».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hồ sơ pháp lý» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Nguồn cung & giá mua (`PUR-02`)

Nhóm **Nguồn cung & giá mua** gồm **5** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 2 |

**Bảng 9. Đặc tả Use Case "Gắn sản phẩm – nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_009 |
| **Tên Use Case** | Gắn sản phẩm – nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Gắn sản phẩm – nhà cung cấp" thuộc nhóm Nguồn cung & giá mua trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Item-vendor linkage |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn sản phẩm – nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn sản phẩm – nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn sản phẩm – nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Gắn sản phẩm – nhà cung cấp» trong nhóm Nguồn cung & giá mua.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Item-vendor linkage).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn sản phẩm – nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn sản phẩm – nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Bảng giá mua theo nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_010 |
| **Tên Use Case** | Bảng giá mua theo nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Bảng giá mua theo nhà cung cấp" thuộc nhóm Nguồn cung & giá mua trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Purchase price list |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng giá mua theo nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng giá mua theo nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng giá mua theo nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Bảng giá mua theo nhà cung cấp» trong nhóm Nguồn cung & giá mua.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Purchase price list).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng giá mua theo nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng giá mua theo nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "Hiệu lực bảng giá mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_011 |
| **Tên Use Case** | Hiệu lực bảng giá mua |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Hiệu lực bảng giá mua" thuộc nhóm Nguồn cung & giá mua trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Price validity period |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hiệu lực bảng giá mua» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hiệu lực bảng giá mua» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hiệu lực bảng giá mua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Hiệu lực bảng giá mua» trong nhóm Nguồn cung & giá mua.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Price validity period).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hiệu lực bảng giá mua».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hiệu lực bảng giá mua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Lịch sử giá mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_012 |
| **Tên Use Case** | Lịch sử giá mua |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Lịch sử giá mua" thuộc nhóm Nguồn cung & giá mua trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Price history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử giá mua» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử giá mua» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử giá mua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer mở «Lịch sử giá mua» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Price history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử giá mua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 13. Đặc tả Use Case "Cảnh báo tăng giá bất thường"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_013 |
| **Tên Use Case** | Cảnh báo tăng giá bất thường |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Cảnh báo tăng giá bất thường" thuộc nhóm Nguồn cung & giá mua trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Price spike alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo tăng giá bất thường» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo tăng giá bất thường» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo tăng giá bất thường» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Buyer kích hoạt kiểm tra điều kiện «Cảnh báo tăng giá bất thường».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Price spike alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo tăng giá bất thường» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Yêu cầu mua hàng (PR) (`PUR-03`)

Nhóm **Yêu cầu mua hàng (PR)** gồm **7** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 4 |

**Bảng 14. Đặc tả Use Case "Tạo PR từ đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_014 |
| **Tên Use Case** | Tạo PR từ đơn vị |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Tạo PR từ đơn vị" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Purchase requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo PR từ đơn vị» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo PR từ đơn vị» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo PR từ đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester mở chức năng «Tạo PR từ đơn vị» trong nhóm Yêu cầu mua hàng (PR).<br>2. Hệ thống kiểm tra license `PUR`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo PR từ đơn vị» (Purchase requisition).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo PR từ đơn vị» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo PR từ đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 15. Đặc tả Use Case "Tạo PR từ cảnh báo tồn min"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_015 |
| **Tên Use Case** | Tạo PR từ cảnh báo tồn min |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Tạo PR từ cảnh báo tồn min" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Auto PR from reorder point |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo PR từ cảnh báo tồn min» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo PR từ cảnh báo tồn min» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo PR từ cảnh báo tồn min» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Requester kích hoạt kiểm tra điều kiện «Tạo PR từ cảnh báo tồn min».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Auto PR from reorder point).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo PR từ cảnh báo tồn min» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Gộp nhiều nhu cầu thành PR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_016 |
| **Tên Use Case** | Gộp nhiều nhu cầu thành PR |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Gộp nhiều nhu cầu thành PR" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: PR consolidation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gộp nhiều nhu cầu thành PR» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gộp nhiều nhu cầu thành PR» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gộp nhiều nhu cầu thành PR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Gộp nhiều nhu cầu thành PR» trong nhóm Yêu cầu mua hàng (PR).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (PR consolidation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gộp nhiều nhu cầu thành PR».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gộp nhiều nhu cầu thành PR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Luồng duyệt PR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_017 |
| **Tên Use Case** | Luồng duyệt PR |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Luồng duyệt PR" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: PR approval workflow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Luồng duyệt PR» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`, `BR-PUR-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Luồng duyệt PR» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Luồng duyệt PR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester mở hộp chờ / chứng từ cần xử lý cho «Luồng duyệt PR».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Luồng duyệt PR», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Luồng duyệt PR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 18. Đặc tả Use Case "Từ chối / trả lại PR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_018 |
| **Tên Use Case** | Từ chối / trả lại PR |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Từ chối / trả lại PR" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Reject/return PR |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Từ chối / trả lại PR» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Từ chối / trả lại PR» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Từ chối / trả lại PR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester mở chứng từ liên quan «Từ chối / trả lại PR».<br>2. Xem nội dung và chọn [Từ chối] / trả bổ sung.<br>3. Nhập lý do bắt buộc (không cho để trống).<br>4. Hệ thống cập nhật trạng thái Rejected/Returned, ghi Audit, thông báo người gửi.<br>5. Người gửi có thể chỉnh sửa và gửi lại theo quy trình. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Từ chối / trả lại PR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 19. Đặc tả Use Case "Theo dõi trạng thái PR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_019 |
| **Tên Use Case** | Theo dõi trạng thái PR |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Theo dõi trạng thái PR" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: PR status tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi trạng thái PR» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi trạng thái PR» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi trạng thái PR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Theo dõi trạng thái PR» trong nhóm Yêu cầu mua hàng (PR).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (PR status tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi trạng thái PR».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi trạng thái PR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Hủy PR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_020 |
| **Tên Use Case** | Hủy PR |
| **Tác nhân** | Requester |
| **Mô tả chức năng** | Cho phép Requester thực hiện chức năng "Hủy PR" thuộc nhóm Yêu cầu mua hàng (PR) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Cancel PR |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Requester] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy PR» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy PR» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy PR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Requester khởi tạo thao tác «Hủy PR» trong nhóm Yêu cầu mua hàng (PR).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Cancel PR).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hủy PR».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy PR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Báo giá & chọn nhà cung cấp (RFQ) (`PUR-04`)

Nhóm **Báo giá & chọn nhà cung cấp (RFQ)** gồm **5** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 0 |

**Bảng 21. Đặc tả Use Case "Tạo RFQ gửi nhiều nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_021 |
| **Tên Use Case** | Tạo RFQ gửi nhiều nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Tạo RFQ gửi nhiều nhà cung cấp" thuộc nhóm Báo giá & chọn nhà cung cấp (RFQ) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: RFQ creation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo RFQ gửi nhiều nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo RFQ gửi nhiều nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo RFQ gửi nhiều nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer mở chức năng «Tạo RFQ gửi nhiều nhà cung cấp» trong nhóm Báo giá & chọn nhà cung cấp (RFQ).<br>2. Hệ thống kiểm tra license `PUR`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo RFQ gửi nhiều nhà cung cấp» (RFQ creation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo RFQ gửi nhiều nhà cung cấp» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo RFQ gửi nhiều nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 22. Đặc tả Use Case "Nhập báo giá từ nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_022 |
| **Tên Use Case** | Nhập báo giá từ nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Nhập báo giá từ nhà cung cấp" thuộc nhóm Báo giá & chọn nhà cung cấp (RFQ) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor quotations |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập báo giá từ nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập báo giá từ nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập báo giá từ nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Nhập báo giá từ nhà cung cấp» trong nhóm Báo giá & chọn nhà cung cấp (RFQ).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor quotations).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập báo giá từ nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập báo giá từ nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "So sánh giá / điều kiện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_023 |
| **Tên Use Case** | So sánh giá / điều kiện |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "So sánh giá / điều kiện" thuộc nhóm Báo giá & chọn nhà cung cấp (RFQ) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Quote comparison |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh giá / điều kiện» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh giá / điều kiện» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh giá / điều kiện» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «So sánh giá / điều kiện» trong nhóm Báo giá & chọn nhà cung cấp (RFQ).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Quote comparison).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «So sánh giá / điều kiện».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh giá / điều kiện» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Chọn nhà cung cấp thắng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_024 |
| **Tên Use Case** | Chọn nhà cung cấp thắng |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Chọn nhà cung cấp thắng" thuộc nhóm Báo giá & chọn nhà cung cấp (RFQ) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Award vendor |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chọn nhà cung cấp thắng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chọn nhà cung cấp thắng» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chọn nhà cung cấp thắng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Chọn nhà cung cấp thắng» trong nhóm Báo giá & chọn nhà cung cấp (RFQ).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Award vendor).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chọn nhà cung cấp thắng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chọn nhà cung cấp thắng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Chuyển RFQ thành PO"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_025 |
| **Tên Use Case** | Chuyển RFQ thành PO |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Chuyển RFQ thành PO" thuộc nhóm Báo giá & chọn nhà cung cấp (RFQ) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: RFQ to PO |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển RFQ thành PO» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển RFQ thành PO» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển RFQ thành PO» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Chuyển RFQ thành PO» trong nhóm Báo giá & chọn nhà cung cấp (RFQ).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (RFQ to PO).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển RFQ thành PO».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển RFQ thành PO» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Đơn mua hàng (PO) (`PUR-05`)

Nhóm **Đơn mua hàng (PO)** gồm **8** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 7 |

**Bảng 26. Đặc tả Use Case "Tạo PO từ PR/RFQ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_026 |
| **Tên Use Case** | Tạo PO từ PR/RFQ |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Tạo PO từ PR/RFQ" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Create PO |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo PO từ PR/RFQ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo PO từ PR/RFQ» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo PO từ PR/RFQ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer mở chức năng «Tạo PO từ PR/RFQ» trong nhóm Đơn mua hàng (PO).<br>2. Hệ thống kiểm tra license `PUR`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo PO từ PR/RFQ» (Create PO).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo PO từ PR/RFQ» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo PO từ PR/RFQ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 27. Đặc tả Use Case "Duyệt PO theo hạn mức"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_027 |
| **Tên Use Case** | Duyệt PO theo hạn mức |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Duyệt PO theo hạn mức" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: PO approval workflow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt PO theo hạn mức» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`, `BR-PUR-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt PO theo hạn mức» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt PO theo hạn mức» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer mở hộp chờ / chứng từ cần xử lý cho «Duyệt PO theo hạn mức».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt PO theo hạn mức», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt PO theo hạn mức» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 28. Đặc tả Use Case "Gửi PO cho nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_028 |
| **Tên Use Case** | Gửi PO cho nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Gửi PO cho nhà cung cấp" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Send PO to vendor |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gửi PO cho nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gửi PO cho nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi PO cho nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer hoàn thiện dữ liệu cho «Gửi PO cho nhà cung cấp» ở trạng thái nháp.<br>2. Chọn [Gửi duyệt / Xác nhận] (submit).<br>3. Hệ thống validate đủ điều kiện gửi; chuyển trạng thái Submitted/In Approval.<br>4. Tạo việc duyệt (WF hoặc duyệt nội module); gửi thông báo.<br>5. Khóa sửa một phần theo policy khi đang chờ duyệt. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gửi PO cho nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 29. Đặc tả Use Case "Xác nhận PO từ nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_029 |
| **Tên Use Case** | Xác nhận PO từ nhà cung cấp |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Xác nhận PO từ nhà cung cấp" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor acknowledgment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận PO từ nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận PO từ nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận PO từ nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Xác nhận PO từ nhà cung cấp» trong nhóm Đơn mua hàng (PO).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor acknowledgment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận PO từ nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận PO từ nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Sửa PO phiên bản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_030 |
| **Tên Use Case** | Sửa PO phiên bản |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Sửa PO phiên bản" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: PO revision |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sửa PO phiên bản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sửa PO phiên bản» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sửa PO phiên bản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Sửa PO phiên bản» trong nhóm Đơn mua hàng (PO).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (PO revision).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sửa PO phiên bản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sửa PO phiên bản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 31. Đặc tả Use Case "Theo dõi nhận hàng từng phần"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_031 |
| **Tên Use Case** | Theo dõi nhận hàng từng phần |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Theo dõi nhận hàng từng phần" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Partial receipt tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi nhận hàng từng phần» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`, `BR-PUR-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi nhận hàng từng phần» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi nhận hàng từng phần» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer mở chứng từ nhận liên quan «Theo dõi nhận hàng từng phần».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi nhận hàng từng phần» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý. |

**Bảng 32. Đặc tả Use Case "Đóng / hủy PO"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_032 |
| **Tên Use Case** | Đóng / hủy PO |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "Đóng / hủy PO" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Close/cancel PO |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng / hủy PO» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng / hủy PO» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng / hủy PO» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer khởi tạo thao tác «Đóng / hủy PO» trong nhóm Đơn mua hàng (PO).<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Close/cancel PO).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng / hủy PO».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng / hủy PO» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 33. Đặc tả Use Case "In / xuất PO"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_033 |
| **Tên Use Case** | In / xuất PO |
| **Tác nhân** | Buyer |
| **Mô tả chức năng** | Cho phép Buyer thực hiện chức năng "In / xuất PO" thuộc nhóm Đơn mua hàng (PO) trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Print PO |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Buyer] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In / xuất PO» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In / xuất PO» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In / xuất PO» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Buyer mở «In / xuất PO», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «In / xuất PO» (Print PO).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In / xuất PO» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Nhận hàng & trả nhà cung cấp (`PUR-06`)

Nhóm **Nhận hàng & trả nhà cung cấp** gồm **6** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 34. Đặc tả Use Case "Tạo phiếu nhận hàng theo PO"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_034 |
| **Tên Use Case** | Tạo phiếu nhận hàng theo PO |
| **Tác nhân** | Warehouse Receiver |
| **Mô tả chức năng** | Cho phép Warehouse Receiver thực hiện chức năng "Tạo phiếu nhận hàng theo PO" thuộc nhóm Nhận hàng & trả nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Goods receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Warehouse Receiver] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phiếu nhận hàng theo PO» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phiếu nhận hàng theo PO» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phiếu nhận hàng theo PO» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Warehouse Receiver mở chức năng «Tạo phiếu nhận hàng theo PO» trong nhóm Nhận hàng & trả nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phiếu nhận hàng theo PO» (Goods receipt).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phiếu nhận hàng theo PO» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phiếu nhận hàng theo PO» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 35. Đặc tả Use Case "Nhận hàng lệch số lượng / chất lượng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_035 |
| **Tên Use Case** | Nhận hàng lệch số lượng / chất lượng |
| **Tác nhân** | Warehouse Receiver |
| **Mô tả chức năng** | Cho phép Warehouse Receiver thực hiện chức năng "Nhận hàng lệch số lượng / chất lượng" thuộc nhóm Nhận hàng & trả nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Variance receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Warehouse Receiver] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhận hàng lệch số lượng / chất lượng» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`, `BR-PUR-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhận hàng lệch số lượng / chất lượng» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhận hàng lệch số lượng / chất lượng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Warehouse Receiver mở chứng từ nhận liên quan «Nhận hàng lệch số lượng / chất lượng».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhận hàng lệch số lượng / chất lượng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý. |

**Bảng 36. Đặc tả Use Case "Từ chối lô hàng không đạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_036 |
| **Tên Use Case** | Từ chối lô hàng không đạt |
| **Tác nhân** | Warehouse Receiver |
| **Mô tả chức năng** | Cho phép Warehouse Receiver thực hiện chức năng "Từ chối lô hàng không đạt" thuộc nhóm Nhận hàng & trả nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Reject lot |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Warehouse Receiver] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Từ chối lô hàng không đạt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Từ chối lô hàng không đạt» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Từ chối lô hàng không đạt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Warehouse Receiver mở chứng từ liên quan «Từ chối lô hàng không đạt».<br>2. Xem nội dung và chọn [Từ chối] / trả bổ sung.<br>3. Nhập lý do bắt buộc (không cho để trống).<br>4. Hệ thống cập nhật trạng thái Rejected/Returned, ghi Audit, thông báo người gửi.<br>5. Người gửi có thể chỉnh sửa và gửi lại theo quy trình. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Từ chối lô hàng không đạt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 37. Đặc tả Use Case "Đẩy nhập kho sang INV"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_037 |
| **Tên Use Case** | Đẩy nhập kho sang INV |
| **Tác nhân** | Warehouse Receiver |
| **Mô tả chức năng** | Cho phép Warehouse Receiver thực hiện chức năng "Đẩy nhập kho sang INV" thuộc nhóm Nhận hàng & trả nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Post to inventory |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Warehouse Receiver] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đẩy nhập kho sang INV» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đẩy nhập kho sang INV» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đẩy nhập kho sang INV» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Warehouse Receiver khởi tạo thao tác «Đẩy nhập kho sang INV» trong nhóm Nhận hàng & trả nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Post to inventory).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đẩy nhập kho sang INV».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đẩy nhập kho sang INV» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 38. Đặc tả Use Case "Trả hàng nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_038 |
| **Tên Use Case** | Trả hàng nhà cung cấp |
| **Tác nhân** | Warehouse Receiver |
| **Mô tả chức năng** | Cho phép Warehouse Receiver thực hiện chức năng "Trả hàng nhà cung cấp" thuộc nhóm Nhận hàng & trả nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Return to vendor |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Warehouse Receiver] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trả hàng nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trả hàng nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trả hàng nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Warehouse Receiver khởi tạo thao tác «Trả hàng nhà cung cấp» trong nhóm Nhận hàng & trả nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Return to vendor).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Trả hàng nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trả hàng nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 39. Đặc tả Use Case "Biên bản giao nhận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_039 |
| **Tên Use Case** | Biên bản giao nhận |
| **Tác nhân** | Warehouse Receiver |
| **Mô tả chức năng** | Cho phép Warehouse Receiver thực hiện chức năng "Biên bản giao nhận" thuộc nhóm Nhận hàng & trả nhà cung cấp trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Receiving report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Warehouse Receiver] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Biên bản giao nhận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Biên bản giao nhận» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Biên bản giao nhận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Warehouse Receiver khởi tạo thao tác «Biên bản giao nhận» trong nhóm Nhận hàng & trả nhà cung cấp.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Receiving report).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Biên bản giao nhận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Biên bản giao nhận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Hóa đơn mua & đối soát (`PUR-07`)

Nhóm **Hóa đơn mua & đối soát** gồm **5** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 40. Đặc tả Use Case "Nhập hóa đơn nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_040 |
| **Tên Use Case** | Nhập hóa đơn nhà cung cấp |
| **Tác nhân** | AP Clerk |
| **Mô tả chức năng** | Cho phép AP Clerk thực hiện chức năng "Nhập hóa đơn nhà cung cấp" thuộc nhóm Hóa đơn mua & đối soát trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor invoice entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Clerk] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập hóa đơn nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập hóa đơn nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập hóa đơn nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Clerk khởi tạo thao tác «Nhập hóa đơn nhà cung cấp» trong nhóm Hóa đơn mua & đối soát.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor invoice entry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập hóa đơn nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập hóa đơn nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 41. Đặc tả Use Case "Đối soát 3 chiều PO–GRN–Invoice"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_041 |
| **Tên Use Case** | Đối soát 3 chiều PO–GRN–Invoice |
| **Tác nhân** | AP Clerk |
| **Mô tả chức năng** | Cho phép AP Clerk thực hiện chức năng "Đối soát 3 chiều PO–GRN–Invoice" thuộc nhóm Hóa đơn mua & đối soát trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: 3-way matching |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Clerk] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát 3 chiều PO–GRN–Invoice» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát 3 chiều PO–GRN–Invoice» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát 3 chiều PO–GRN–Invoice» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Clerk khởi tạo thao tác «Đối soát 3 chiều PO–GRN–Invoice» trong nhóm Hóa đơn mua & đối soát.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (3-way matching).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát 3 chiều PO–GRN–Invoice».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát 3 chiều PO–GRN–Invoice» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 42. Đặc tả Use Case "Xử lý chênh lệch"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_042 |
| **Tên Use Case** | Xử lý chênh lệch |
| **Tác nhân** | AP Clerk |
| **Mô tả chức năng** | Cho phép AP Clerk thực hiện chức năng "Xử lý chênh lệch" thuộc nhóm Hóa đơn mua & đối soát trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Match exception handling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Clerk] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xử lý chênh lệch» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xử lý chênh lệch» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xử lý chênh lệch» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AP Clerk khởi tạo thao tác «Xử lý chênh lệch» trong nhóm Hóa đơn mua & đối soát.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Match exception handling).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xử lý chênh lệch».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xử lý chênh lệch» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 43. Đặc tả Use Case "Đẩy công nợ sang FIN AP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_043 |
| **Tên Use Case** | Đẩy công nợ sang FIN AP |
| **Tác nhân** | AP Clerk |
| **Mô tả chức năng** | Cho phép AP Clerk thực hiện chức năng "Đẩy công nợ sang FIN AP" thuộc nhóm Hóa đơn mua & đối soát trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Post to AP |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Clerk] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đẩy công nợ sang FIN AP» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đẩy công nợ sang FIN AP» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đẩy công nợ sang FIN AP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Clerk khởi tạo thao tác «Đẩy công nợ sang FIN AP» trong nhóm Hóa đơn mua & đối soát.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Post to AP).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đẩy công nợ sang FIN AP».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đẩy công nợ sang FIN AP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 44. Đặc tả Use Case "Tạm ứng nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_044 |
| **Tên Use Case** | Tạm ứng nhà cung cấp |
| **Tác nhân** | AP Clerk |
| **Mô tả chức năng** | Cho phép AP Clerk thực hiện chức năng "Tạm ứng nhà cung cấp" thuộc nhóm Hóa đơn mua & đối soát trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Vendor prepayment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Clerk] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạm ứng nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạm ứng nhà cung cấp» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạm ứng nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AP Clerk khởi tạo thao tác «Tạm ứng nhà cung cấp» trong nhóm Hóa đơn mua & đối soát.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor prepayment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tạm ứng nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạm ứng nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.8. Hợp đồng mua & khung giá (`PUR-08`)

Nhóm **Hợp đồng mua & khung giá** gồm **3** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 3 |
| Must | 0 |

**Bảng 45. Đặc tả Use Case "Hợp đồng mua khung"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_045 |
| **Tên Use Case** | Hợp đồng mua khung |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Hợp đồng mua khung" thuộc nhóm Hợp đồng mua & khung giá trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Blanket purchase agreement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hợp đồng mua khung» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hợp đồng mua khung» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hợp đồng mua khung» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Purchasing Manager khởi tạo thao tác «Hợp đồng mua khung» trong nhóm Hợp đồng mua & khung giá.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Blanket purchase agreement).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hợp đồng mua khung».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hợp đồng mua khung» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 46. Đặc tả Use Case "Theo dõi sản lượng / giá trị còn lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_046 |
| **Tên Use Case** | Theo dõi sản lượng / giá trị còn lại |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Theo dõi sản lượng / giá trị còn lại" thuộc nhóm Hợp đồng mua & khung giá trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Call-off tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi sản lượng / giá trị còn lại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi sản lượng / giá trị còn lại» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi sản lượng / giá trị còn lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Purchasing Manager khởi tạo thao tác «Theo dõi sản lượng / giá trị còn lại» trong nhóm Hợp đồng mua & khung giá.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Call-off tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi sản lượng / giá trị còn lại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi sản lượng / giá trị còn lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 47. Đặc tả Use Case "Cảnh báo hết hạn hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_047 |
| **Tên Use Case** | Cảnh báo hết hạn hợp đồng |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Cảnh báo hết hạn hợp đồng" thuộc nhóm Hợp đồng mua & khung giá trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Contract expiry alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo hết hạn hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo hết hạn hợp đồng» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo hết hạn hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Purchasing Manager kích hoạt kiểm tra điều kiện «Cảnh báo hết hạn hợp đồng».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Contract expiry alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo hết hạn hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.9. Báo cáo mua hàng (`PUR-09`)

Nhóm **Báo cáo mua hàng** gồm **5** use case của module `PUR`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 48. Đặc tả Use Case "Báo cáo mua theo nhà cung cấp / SP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_048 |
| **Tên Use Case** | Báo cáo mua theo nhà cung cấp / SP |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Báo cáo mua theo nhà cung cấp / SP" thuộc nhóm Báo cáo mua hàng trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Spend analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo mua theo nhà cung cấp / SP» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo mua theo nhà cung cấp / SP» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo mua theo nhà cung cấp / SP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Purchasing Manager mở «Báo cáo mua theo nhà cung cấp / SP» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Spend analysis); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo mua theo nhà cung cấp / SP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 49. Đặc tả Use Case "Báo cáo đúng hạn giao hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_049 |
| **Tên Use Case** | Báo cáo đúng hạn giao hàng |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Báo cáo đúng hạn giao hàng" thuộc nhóm Báo cáo mua hàng trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: OTIF report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo đúng hạn giao hàng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo đúng hạn giao hàng» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo đúng hạn giao hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Purchasing Manager mở «Báo cáo đúng hạn giao hàng» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (OTIF report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo đúng hạn giao hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 50. Đặc tả Use Case "Báo cáo tiết kiệm từ RFQ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_050 |
| **Tên Use Case** | Báo cáo tiết kiệm từ RFQ |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Báo cáo tiết kiệm từ RFQ" thuộc nhóm Báo cáo mua hàng trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Savings report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo tiết kiệm từ RFQ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo tiết kiệm từ RFQ» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo tiết kiệm từ RFQ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Purchasing Manager mở «Báo cáo tiết kiệm từ RFQ» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Savings report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo tiết kiệm từ RFQ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 51. Đặc tả Use Case "Open PR / Open PO aging"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_051 |
| **Tên Use Case** | Open PR / Open PO aging |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Open PR / Open PO aging" thuộc nhóm Báo cáo mua hàng trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Procurement backlog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Open PR / Open PO aging» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Open PR / Open PO aging» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Open PR / Open PO aging» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Purchasing Manager khởi tạo thao tác «Open PR / Open PO aging» trong nhóm Báo cáo mua hàng.<br>2. Hệ thống kiểm tra license `PUR`, quyền RBAC và tiền điều kiện nghiệp vụ (Procurement backlog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Open PR / Open PO aging».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Open PR / Open PO aging» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 52. Đặc tả Use Case "Xuất báo cáo mua hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_PUR_052 |
| **Tên Use Case** | Xuất báo cáo mua hàng |
| **Tác nhân** | Purchasing Manager |
| **Mô tả chức năng** | Cho phép Purchasing Manager thực hiện chức năng "Xuất báo cáo mua hàng" thuộc nhóm Báo cáo mua hàng trong module PUR — Mua hàng (Procurement). Mô tả chi tiết: Export purchase reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Purchasing Manager] và được cấp quyền RBAC tương ứng.<br>• License module `PUR` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo mua hàng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-PUR-SCOPE-01`, `BR-PUR-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo mua hàng» được lưu nhất quán trong module `PUR`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo mua hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Purchasing Manager mở «Xuất báo cáo mua hàng», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo mua hàng» (Export purchase reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo mua hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-PUR-01 — Mua hàng chuẩn PR→PO→GRN→Invoice

**Mục tiêu:** Mua đúng nhu cầu, đúng giá, đủ chứng từ

| Bước | Mô tả |
|---:|---|
| 1 | Tạo PR và duyệt theo hạn mức |
| 2 | RFQ hoặc chọn NCC/bảng giá |
| 3 | Tạo và duyệt PO; gửi NCC |
| 4 | Nhận hàng (GRN) đẩy nhập INV |
| 5 | Nhập hóa đơn; 3-way match; đẩy AP sang FIN |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Vendor` | NCC |
| `VendorItemPrice` | Giá mua |
| `PurchaseRequisition` | PR |
| `RFQ` | Yêu cầu báo giá |
| `PurchaseOrder` | PO |
| `GoodsReceipt` | GRN |
| `VendorInvoice` | Hóa đơn mua |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `PUR`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-PUR-01: PO vượt hạn mức phải qua workflow.
- BR-PUR-02: Nhận vượt PO chỉ cho phép trong dung sai cấu hình.
- BR-PUR-03: Không đóng PO khi còn số lượng mở vượt ngưỡng policy.
- BR-PUR-04: Hóa đơn lệch match phải xử lý exception trước khi post AP.
- BR-PUR-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-PUR-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-PUR-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Truy vết | Mọi PO truy vết được PR/RFQ/người duyệt |
| Hiệu năng | Danh sách open PO phân trang nhanh |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `PUR` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `pur.vendor.manage` | Quyền chức năng module |
| `pur.pr.manage` | Quyền chức năng module |
| `pur.po.manage` | Quyền chức năng module |
| `pur.po.approve` | Quyền chức năng module |
| `pur.grn.manage` | Quyền chức năng module |
| `pur.invoice.match` | Quyền chức năng module |
| `pur.report.view` | Quyền chức năng module |
| `pur.*.view` | Xem trong data scope |
| `pur.*.manage` | Tạo/sửa trong data scope |
| `pur.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Spend theo NCC/SP | Theo dõi vận hành module |
| OTIF NCC | Theo dõi vận hành module |
| Open PR/PO aging | Theo dõi vận hành module |
| Savings từ RFQ | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Đơn vị tính và SKU dùng chung master với INV.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có hỗ trợ hợp đồng khung call-off ngay phase 1?

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
| Bản SRS này | `SRS_PUR_v1.1.md` / `.docx` |
| UC IDs | `UC_PUR_001` … |

---

*Hết tài liệu SRS-PUR-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
