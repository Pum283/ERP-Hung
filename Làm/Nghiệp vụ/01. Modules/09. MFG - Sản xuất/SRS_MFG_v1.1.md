# SRS-MFG-v1.1 — Sản xuất (Manufacturing)

> **Software Requirements Specification — Module MFG**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-MFG-v1.1` |
| Module | `MFG` — Sản xuất (Manufacturing) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Sản xuất & Dịch vụ |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS`, `INV` |
| Khuyến nghị kèm | `PUR`, `FIN`, `HRM` |
| Số nhóm / UC | 8 nhóm / 46 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Sản xuất (Manufacturing)** (`MFG`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Quản lý BOM, kế hoạch/lệnh sản xuất, xuất NVL–nhập TP, giá thành sơ bộ, QC cơ bản và báo cáo sản lượng.

### 1.3. Mục tiêu đo được
1. Kiểm soát lệnh SX và tiêu hao NVL.
2. Tính giá thành đơn vị cơ bản.
3. Giảm sai lệch lý thuyết vs thực tế.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- BOM, planning/MRP-light, work order, issue/receipt, costing, QC basic, batch, MFG reports.

### 2.2. Out of Scope
- MES realtime máy móc nâng cao.
- APS tối ưu lịch phức tạp.

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`, `INV`
- **Khuyến nghị kèm (E2E):** `PUR`, `FIN`, `HRM`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Production Planner | Kế hoạch & lệnh |
| Shop Supervisor | Điều phối xưởng |
| Warehouse (SX) | Xuất NVL/nhập TP |
| QC Staff | Kiểm chất lượng |
| Cost Accountant | Giá thành |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| BOM | Bill of Materials |
| WO | Work Order — lệnh SX |
| WIP | Work in Progress |
| Yield | Tỷ lệ đạt |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- MFG (Sản xuất (Manufacturing))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `MFG` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `MFG` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | INV | Xuất NVL/nhập TP |
| Tích hợp | PUR | PR thiếu NVL |
| Tích hợp | FIN | Giá thành/bút toán |
| Tích hợp | CRM | Nhu cầu theo đơn |

---

## 6. Catalog chức năng

**Tổng:** 8 nhóm · 46 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `MFG-01` | Danh mục sản xuất | 5 | 3 | 2 | 0 |
| 2 | `MFG-02` | BOM & định mức | 6 | 3 | 2 | 1 |
| 3 | `MFG-03` | Kế hoạch sản xuất | 5 | 1 | 4 | 0 |
| 4 | `MFG-04` | Lệnh sản xuất | 10 | 7 | 2 | 1 |
| 5 | `MFG-05` | Giá thành sản xuất | 5 | 3 | 2 | 0 |
| 6 | `MFG-06` | Chất lượng (QC) | 5 | 0 | 5 | 0 |
| 7 | `MFG-07` | Sản xuất theo lô/mẻ | 4 | 0 | 3 | 1 |
| 8 | `MFG-08` | Báo cáo sản xuất | 6 | 5 | 0 | 1 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_MFG_001` | Danh mục sản xuất | Danh mục thành phẩm / bán thành phẩm | Must |
| `UC_MFG_002` | Danh mục sản xuất | Danh mục nguyên vật liệu | Must |
| `UC_MFG_003` | Danh mục sản xuất | Danh mục xưởng / dây chuyền | Must |
| `UC_MFG_004` | Danh mục sản xuất | Danh mục công đoạn | Should |
| `UC_MFG_005` | Danh mục sản xuất | Ca sản xuất / năng lực | Should |
| `UC_MFG_006` | BOM & định mức | Tạo BOM nhiều cấp | Must |
| `UC_MFG_007` | BOM & định mức | Phiên bản BOM | Must |
| `UC_MFG_008` | BOM & định mức | Định mức nguyên vật liệu | Must |
| `UC_MFG_009` | BOM & định mức | Định mức hao hụt | Should |
| `UC_MFG_010` | BOM & định mức | So sánh phiên bản BOM | Could |
| `UC_MFG_011` | BOM & định mức | Sao chép BOM | Should |
| `UC_MFG_012` | Kế hoạch sản xuất | Kế hoạch SX theo nhu cầu | Should |
| `UC_MFG_013` | Kế hoạch sản xuất | Kế hoạch SX theo đơn hàng | Must |
| `UC_MFG_014` | Kế hoạch sản xuất | Tính nhu cầu nguyên vật liệu | Should |
| `UC_MFG_015` | Kế hoạch sản xuất | Đề xuất mua nguyên vật liệu thiếu | Should |
| `UC_MFG_016` | Kế hoạch sản xuất | Lịch SX theo xưởng/ca | Should |
| `UC_MFG_017` | Lệnh sản xuất | Tạo lệnh sản xuất | Must |
| `UC_MFG_018` | Lệnh sản xuất | Duyệt lệnh sản xuất | Must |
| `UC_MFG_019` | Lệnh sản xuất | Phát hành lệnh / in phiếu | Must |
| `UC_MFG_020` | Lệnh sản xuất | Xuất nguyên vật liệu cho lệnh | Must |
| `UC_MFG_021` | Lệnh sản xuất | Ghi nhận tiến độ công đoạn | Should |
| `UC_MFG_022` | Lệnh sản xuất | Ghi nhận thành phẩm nhập kho | Must |
| `UC_MFG_023` | Lệnh sản xuất | Ghi nhận phế phẩm / hao hụt | Must |
| `UC_MFG_024` | Lệnh sản xuất | Tạm dừng / hủy lệnh | Should |
| `UC_MFG_025` | Lệnh sản xuất | Đóng lệnh sản xuất | Must |
| `UC_MFG_026` | Lệnh sản xuất | Lệnh sản xuất lại | Could |
| `UC_MFG_027` | Giá thành sản xuất | Tập hợp chi phí nguyên vật liệu | Must |
| `UC_MFG_028` | Giá thành sản xuất | Phân bổ nhân công / chi phí chung | Should |
| `UC_MFG_029` | Giá thành sản xuất | Giá thành đơn vị thành phẩm | Must |
| `UC_MFG_030` | Giá thành sản xuất | Đối chiếu lý thuyết vs thực tế | Should |
| `UC_MFG_031` | Giá thành sản xuất | Đẩy giá thành sang INV/FIN | Must |
| `UC_MFG_032` | Chất lượng (QC) | Tiêu chí QC đầu vào | Should |
| `UC_MFG_033` | Chất lượng (QC) | QC thành phẩm | Should |
| `UC_MFG_034` | Chất lượng (QC) | Ghi nhận lô đạt / không đạt | Should |
| `UC_MFG_035` | Chất lượng (QC) | Cách ly hàng lỗi | Should |
| `UC_MFG_036` | Chất lượng (QC) | Báo cáo tỷ lệ đạt QC | Should |
| `UC_MFG_037` | Sản xuất theo lô/mẻ | Lô/mẻ sản xuất | Should |
| `UC_MFG_038` | Sản xuất theo lô/mẻ | Ghi nhận thông số mẻ | Should |
| `UC_MFG_039` | Sản xuất theo lô/mẻ | Đóng gói & gắn tem | Should |
| `UC_MFG_040` | Sản xuất theo lô/mẻ | Định mức phối trộn | Could |
| `UC_MFG_041` | Báo cáo sản xuất | Tiến độ lệnh sản xuất | Must |
| `UC_MFG_042` | Báo cáo sản xuất | Sản lượng theo ngày/ca/xưởng | Must |
| `UC_MFG_043` | Báo cáo sản xuất | Tiêu hao nguyên vật liệu variance | Must |
| `UC_MFG_044` | Báo cáo sản xuất | Hiệu suất / OEE | Could |
| `UC_MFG_045` | Báo cáo sản xuất | Dashboard sản xuất | Must |
| `UC_MFG_046` | Báo cáo sản xuất | Xuất báo cáo sản xuất | Must |

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

### 7.1. Danh mục sản xuất (`MFG-01`)

Nhóm **Danh mục sản xuất** gồm **5** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 1. Đặc tả Use Case "Danh mục thành phẩm / bán thành phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_001 |
| **Tên Use Case** | Danh mục thành phẩm / bán thành phẩm |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Danh mục thành phẩm / bán thành phẩm" thuộc nhóm Danh mục sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Finished/semi-finished items |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục thành phẩm / bán thành phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục thành phẩm / bán thành phẩm» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục thành phẩm / bán thành phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Danh mục thành phẩm / bán thành phẩm» trong nhóm Danh mục sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Finished/semi-finished items).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục thành phẩm / bán thành phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục thành phẩm / bán thành phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Danh mục nguyên vật liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_002 |
| **Tên Use Case** | Danh mục nguyên vật liệu |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Danh mục nguyên vật liệu" thuộc nhóm Danh mục sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Raw material master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục nguyên vật liệu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục nguyên vật liệu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục nguyên vật liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Danh mục nguyên vật liệu» trong nhóm Danh mục sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Raw material master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục nguyên vật liệu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục nguyên vật liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Danh mục xưởng / dây chuyền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_003 |
| **Tên Use Case** | Danh mục xưởng / dây chuyền |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Danh mục xưởng / dây chuyền" thuộc nhóm Danh mục sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Work center master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục xưởng / dây chuyền» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục xưởng / dây chuyền» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục xưởng / dây chuyền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Danh mục xưởng / dây chuyền» trong nhóm Danh mục sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Work center master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục xưởng / dây chuyền».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục xưởng / dây chuyền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Danh mục công đoạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_004 |
| **Tên Use Case** | Danh mục công đoạn |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Danh mục công đoạn" thuộc nhóm Danh mục sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Operation master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục công đoạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục công đoạn» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục công đoạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Danh mục công đoạn» trong nhóm Danh mục sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Operation master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục công đoạn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục công đoạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 5. Đặc tả Use Case "Ca sản xuất / năng lực"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_005 |
| **Tên Use Case** | Ca sản xuất / năng lực |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Ca sản xuất / năng lực" thuộc nhóm Danh mục sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Capacity planning basics |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ca sản xuất / năng lực» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ca sản xuất / năng lực» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ca sản xuất / năng lực» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Ca sản xuất / năng lực» trong nhóm Danh mục sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Capacity planning basics).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ca sản xuất / năng lực».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ca sản xuất / năng lực» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. BOM & định mức (`MFG-02`)

Nhóm **BOM & định mức** gồm **6** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 6. Đặc tả Use Case "Tạo BOM nhiều cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_006 |
| **Tên Use Case** | Tạo BOM nhiều cấp |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Tạo BOM nhiều cấp" thuộc nhóm BOM & định mức trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Multi-level BOM |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo BOM nhiều cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo BOM nhiều cấp» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo BOM nhiều cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner mở chức năng «Tạo BOM nhiều cấp» trong nhóm BOM & định mức.<br>2. Hệ thống kiểm tra license `MFG`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo BOM nhiều cấp» (Multi-level BOM).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo BOM nhiều cấp» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo BOM nhiều cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 7. Đặc tả Use Case "Phiên bản BOM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_007 |
| **Tên Use Case** | Phiên bản BOM |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Phiên bản BOM" thuộc nhóm BOM & định mức trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: BOM versioning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiên bản BOM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiên bản BOM» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiên bản BOM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Phiên bản BOM» trong nhóm BOM & định mức.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (BOM versioning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phiên bản BOM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiên bản BOM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Định mức nguyên vật liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_008 |
| **Tên Use Case** | Định mức nguyên vật liệu |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Định mức nguyên vật liệu" thuộc nhóm BOM & định mức trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Material quantity per unit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Định mức nguyên vật liệu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Định mức nguyên vật liệu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Định mức nguyên vật liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Định mức nguyên vật liệu» trong nhóm BOM & định mức.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Material quantity per unit).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Định mức nguyên vật liệu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Định mức nguyên vật liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Định mức hao hụt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_009 |
| **Tên Use Case** | Định mức hao hụt |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Định mức hao hụt" thuộc nhóm BOM & định mức trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Scrap allowance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Định mức hao hụt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Định mức hao hụt» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Định mức hao hụt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Định mức hao hụt» trong nhóm BOM & định mức.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Scrap allowance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Định mức hao hụt».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Định mức hao hụt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "So sánh phiên bản BOM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_010 |
| **Tên Use Case** | So sánh phiên bản BOM |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "So sánh phiên bản BOM" thuộc nhóm BOM & định mức trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: BOM comparison |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh phiên bản BOM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh phiên bản BOM» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh phiên bản BOM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «So sánh phiên bản BOM» trong nhóm BOM & định mức.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (BOM comparison).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «So sánh phiên bản BOM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh phiên bản BOM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "Sao chép BOM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_011 |
| **Tên Use Case** | Sao chép BOM |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Sao chép BOM" thuộc nhóm BOM & định mức trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Clone BOM |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sao chép BOM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sao chép BOM» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sao chép BOM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Sao chép BOM» trong nhóm BOM & định mức.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Clone BOM).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sao chép BOM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sao chép BOM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Kế hoạch sản xuất (`MFG-03`)

Nhóm **Kế hoạch sản xuất** gồm **5** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 1 |

**Bảng 12. Đặc tả Use Case "Kế hoạch SX theo nhu cầu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_012 |
| **Tên Use Case** | Kế hoạch SX theo nhu cầu |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Kế hoạch SX theo nhu cầu" thuộc nhóm Kế hoạch sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Production planning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kế hoạch SX theo nhu cầu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kế hoạch SX theo nhu cầu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kế hoạch SX theo nhu cầu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Kế hoạch SX theo nhu cầu» trong nhóm Kế hoạch sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Production planning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kế hoạch SX theo nhu cầu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kế hoạch SX theo nhu cầu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 13. Đặc tả Use Case "Kế hoạch SX theo đơn hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_013 |
| **Tên Use Case** | Kế hoạch SX theo đơn hàng |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Kế hoạch SX theo đơn hàng" thuộc nhóm Kế hoạch sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Make-to-order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kế hoạch SX theo đơn hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kế hoạch SX theo đơn hàng» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kế hoạch SX theo đơn hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Kế hoạch SX theo đơn hàng» trong nhóm Kế hoạch sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Make-to-order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kế hoạch SX theo đơn hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kế hoạch SX theo đơn hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 14. Đặc tả Use Case "Tính nhu cầu nguyên vật liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_014 |
| **Tên Use Case** | Tính nhu cầu nguyên vật liệu |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Tính nhu cầu nguyên vật liệu" thuộc nhóm Kế hoạch sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: MRP light |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính nhu cầu nguyên vật liệu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính nhu cầu nguyên vật liệu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính nhu cầu nguyên vật liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Tính nhu cầu nguyên vật liệu» trong nhóm Kế hoạch sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (MRP light).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tính nhu cầu nguyên vật liệu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính nhu cầu nguyên vật liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Đề xuất mua nguyên vật liệu thiếu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_015 |
| **Tên Use Case** | Đề xuất mua nguyên vật liệu thiếu |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Đề xuất mua nguyên vật liệu thiếu" thuộc nhóm Kế hoạch sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Generate purchase requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề xuất mua nguyên vật liệu thiếu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề xuất mua nguyên vật liệu thiếu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề xuất mua nguyên vật liệu thiếu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Đề xuất mua nguyên vật liệu thiếu» trong nhóm Kế hoạch sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Generate purchase requisition).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đề xuất mua nguyên vật liệu thiếu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề xuất mua nguyên vật liệu thiếu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Lịch SX theo xưởng/ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_016 |
| **Tên Use Case** | Lịch SX theo xưởng/ca |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Lịch SX theo xưởng/ca" thuộc nhóm Kế hoạch sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Production schedule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch SX theo xưởng/ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch SX theo xưởng/ca» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch SX theo xưởng/ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Lịch SX theo xưởng/ca» trong nhóm Kế hoạch sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Production schedule).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lịch SX theo xưởng/ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch SX theo xưởng/ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Lệnh sản xuất (`MFG-04`)

Nhóm **Lệnh sản xuất** gồm **10** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 7 |

**Bảng 17. Đặc tả Use Case "Tạo lệnh sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_017 |
| **Tên Use Case** | Tạo lệnh sản xuất |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Tạo lệnh sản xuất" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Work order creation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo lệnh sản xuất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo lệnh sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo lệnh sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor mở chức năng «Tạo lệnh sản xuất» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo lệnh sản xuất» (Work order creation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo lệnh sản xuất» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo lệnh sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 18. Đặc tả Use Case "Duyệt lệnh sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_018 |
| **Tên Use Case** | Duyệt lệnh sản xuất |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Duyệt lệnh sản xuất" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Work order approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt lệnh sản xuất» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`, `BR-MFG-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt lệnh sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt lệnh sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor mở hộp chờ / chứng từ cần xử lý cho «Duyệt lệnh sản xuất».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt lệnh sản xuất», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt lệnh sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 19. Đặc tả Use Case "Phát hành lệnh / in phiếu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_019 |
| **Tên Use Case** | Phát hành lệnh / in phiếu |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Phát hành lệnh / in phiếu" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Release work order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phát hành lệnh / in phiếu» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phát hành lệnh / in phiếu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phát hành lệnh / in phiếu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor mở «Phát hành lệnh / in phiếu», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Phát hành lệnh / in phiếu» (Release work order).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phát hành lệnh / in phiếu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Xuất nguyên vật liệu cho lệnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_020 |
| **Tên Use Case** | Xuất nguyên vật liệu cho lệnh |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Xuất nguyên vật liệu cho lệnh" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Material issue to WO |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất nguyên vật liệu cho lệnh» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất nguyên vật liệu cho lệnh» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất nguyên vật liệu cho lệnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor mở «Xuất nguyên vật liệu cho lệnh», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất nguyên vật liệu cho lệnh» (Material issue to WO).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất nguyên vật liệu cho lệnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Ghi nhận tiến độ công đoạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_021 |
| **Tên Use Case** | Ghi nhận tiến độ công đoạn |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Ghi nhận tiến độ công đoạn" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Operation progress |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận tiến độ công đoạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận tiến độ công đoạn» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận tiến độ công đoạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Ghi nhận tiến độ công đoạn» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Operation progress).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận tiến độ công đoạn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận tiến độ công đoạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Ghi nhận thành phẩm nhập kho"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_022 |
| **Tên Use Case** | Ghi nhận thành phẩm nhập kho |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Ghi nhận thành phẩm nhập kho" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Finished goods receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận thành phẩm nhập kho» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận thành phẩm nhập kho» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận thành phẩm nhập kho» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Ghi nhận thành phẩm nhập kho» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Finished goods receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận thành phẩm nhập kho».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận thành phẩm nhập kho» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Ghi nhận phế phẩm / hao hụt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_023 |
| **Tên Use Case** | Ghi nhận phế phẩm / hao hụt |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Ghi nhận phế phẩm / hao hụt" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Scrap reporting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận phế phẩm / hao hụt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận phế phẩm / hao hụt» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận phế phẩm / hao hụt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Ghi nhận phế phẩm / hao hụt» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Scrap reporting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận phế phẩm / hao hụt».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận phế phẩm / hao hụt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Tạm dừng / hủy lệnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_024 |
| **Tên Use Case** | Tạm dừng / hủy lệnh |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Tạm dừng / hủy lệnh" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Hold/cancel work order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạm dừng / hủy lệnh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạm dừng / hủy lệnh» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạm dừng / hủy lệnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Tạm dừng / hủy lệnh» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Hold/cancel work order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tạm dừng / hủy lệnh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạm dừng / hủy lệnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Đóng lệnh sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_025 |
| **Tên Use Case** | Đóng lệnh sản xuất |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Đóng lệnh sản xuất" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Close work order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng lệnh sản xuất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng lệnh sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng lệnh sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Đóng lệnh sản xuất» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Close work order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng lệnh sản xuất».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng lệnh sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Lệnh sản xuất lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_026 |
| **Tên Use Case** | Lệnh sản xuất lại |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Lệnh sản xuất lại" thuộc nhóm Lệnh sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Rework order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lệnh sản xuất lại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lệnh sản xuất lại» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lệnh sản xuất lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Lệnh sản xuất lại» trong nhóm Lệnh sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Rework order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lệnh sản xuất lại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lệnh sản xuất lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Giá thành sản xuất (`MFG-05`)

Nhóm **Giá thành sản xuất** gồm **5** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 27. Đặc tả Use Case "Tập hợp chi phí nguyên vật liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_027 |
| **Tên Use Case** | Tập hợp chi phí nguyên vật liệu |
| **Tác nhân** | Cost Accountant |
| **Mô tả chức năng** | Cho phép Cost Accountant thực hiện chức năng "Tập hợp chi phí nguyên vật liệu" thuộc nhóm Giá thành sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Material cost rollup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cost Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tập hợp chi phí nguyên vật liệu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tập hợp chi phí nguyên vật liệu» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tập hợp chi phí nguyên vật liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cost Accountant khởi tạo thao tác «Tập hợp chi phí nguyên vật liệu» trong nhóm Giá thành sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Material cost rollup).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tập hợp chi phí nguyên vật liệu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tập hợp chi phí nguyên vật liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 28. Đặc tả Use Case "Phân bổ nhân công / chi phí chung"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_028 |
| **Tên Use Case** | Phân bổ nhân công / chi phí chung |
| **Tác nhân** | Cost Accountant |
| **Mô tả chức năng** | Cho phép Cost Accountant thực hiện chức năng "Phân bổ nhân công / chi phí chung" thuộc nhóm Giá thành sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Overhead allocation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cost Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân bổ nhân công / chi phí chung» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân bổ nhân công / chi phí chung» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân bổ nhân công / chi phí chung» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cost Accountant khởi tạo thao tác «Phân bổ nhân công / chi phí chung» trong nhóm Giá thành sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Overhead allocation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân bổ nhân công / chi phí chung».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân bổ nhân công / chi phí chung» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Giá thành đơn vị thành phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_029 |
| **Tên Use Case** | Giá thành đơn vị thành phẩm |
| **Tác nhân** | Cost Accountant |
| **Mô tả chức năng** | Cho phép Cost Accountant thực hiện chức năng "Giá thành đơn vị thành phẩm" thuộc nhóm Giá thành sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Unit cost calculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cost Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giá thành đơn vị thành phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giá thành đơn vị thành phẩm» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giá thành đơn vị thành phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cost Accountant khởi tạo thao tác «Giá thành đơn vị thành phẩm» trong nhóm Giá thành sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Unit cost calculation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giá thành đơn vị thành phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giá thành đơn vị thành phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Đối chiếu lý thuyết vs thực tế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_030 |
| **Tên Use Case** | Đối chiếu lý thuyết vs thực tế |
| **Tác nhân** | Cost Accountant |
| **Mô tả chức năng** | Cho phép Cost Accountant thực hiện chức năng "Đối chiếu lý thuyết vs thực tế" thuộc nhóm Giá thành sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Cost variance analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cost Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối chiếu lý thuyết vs thực tế» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối chiếu lý thuyết vs thực tế» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối chiếu lý thuyết vs thực tế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Cost Accountant khởi tạo thao tác «Đối chiếu lý thuyết vs thực tế» trong nhóm Giá thành sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Cost variance analysis).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối chiếu lý thuyết vs thực tế».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối chiếu lý thuyết vs thực tế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 31. Đặc tả Use Case "Đẩy giá thành sang INV/FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_031 |
| **Tên Use Case** | Đẩy giá thành sang INV/FIN |
| **Tác nhân** | Cost Accountant |
| **Mô tả chức năng** | Cho phép Cost Accountant thực hiện chức năng "Đẩy giá thành sang INV/FIN" thuộc nhóm Giá thành sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Post production costing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Cost Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đẩy giá thành sang INV/FIN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đẩy giá thành sang INV/FIN» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đẩy giá thành sang INV/FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Cost Accountant khởi tạo thao tác «Đẩy giá thành sang INV/FIN» trong nhóm Giá thành sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Post production costing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đẩy giá thành sang INV/FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đẩy giá thành sang INV/FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Chất lượng (QC) (`MFG-06`)

Nhóm **Chất lượng (QC)** gồm **5** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 0 |

**Bảng 32. Đặc tả Use Case "Tiêu chí QC đầu vào"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_032 |
| **Tên Use Case** | Tiêu chí QC đầu vào |
| **Tác nhân** | QC Staff |
| **Mô tả chức năng** | Cho phép QC Staff thực hiện chức năng "Tiêu chí QC đầu vào" thuộc nhóm Chất lượng (QC) trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Incoming quality control |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [QC Staff] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiêu chí QC đầu vào» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiêu chí QC đầu vào» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiêu chí QC đầu vào» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. QC Staff khởi tạo thao tác «Tiêu chí QC đầu vào» trong nhóm Chất lượng (QC).<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Incoming quality control).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiêu chí QC đầu vào».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiêu chí QC đầu vào» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 33. Đặc tả Use Case "QC thành phẩm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_033 |
| **Tên Use Case** | QC thành phẩm |
| **Tác nhân** | QC Staff |
| **Mô tả chức năng** | Cho phép QC Staff thực hiện chức năng "QC thành phẩm" thuộc nhóm Chất lượng (QC) trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Outgoing quality control |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [QC Staff] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «QC thành phẩm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «QC thành phẩm» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «QC thành phẩm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. QC Staff khởi tạo thao tác «QC thành phẩm» trong nhóm Chất lượng (QC).<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Outgoing quality control).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «QC thành phẩm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «QC thành phẩm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 34. Đặc tả Use Case "Ghi nhận lô đạt / không đạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_034 |
| **Tên Use Case** | Ghi nhận lô đạt / không đạt |
| **Tác nhân** | QC Staff |
| **Mô tả chức năng** | Cho phép QC Staff thực hiện chức năng "Ghi nhận lô đạt / không đạt" thuộc nhóm Chất lượng (QC) trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: QC result recording |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [QC Staff] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận lô đạt / không đạt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận lô đạt / không đạt» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận lô đạt / không đạt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. QC Staff khởi tạo thao tác «Ghi nhận lô đạt / không đạt» trong nhóm Chất lượng (QC).<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (QC result recording).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận lô đạt / không đạt».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận lô đạt / không đạt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 35. Đặc tả Use Case "Cách ly hàng lỗi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_035 |
| **Tên Use Case** | Cách ly hàng lỗi |
| **Tác nhân** | QC Staff |
| **Mô tả chức năng** | Cho phép QC Staff thực hiện chức năng "Cách ly hàng lỗi" thuộc nhóm Chất lượng (QC) trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Quarantine stock |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [QC Staff] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cách ly hàng lỗi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cách ly hàng lỗi» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cách ly hàng lỗi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. QC Staff khởi tạo thao tác «Cách ly hàng lỗi» trong nhóm Chất lượng (QC).<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Quarantine stock).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Cách ly hàng lỗi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cách ly hàng lỗi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Báo cáo tỷ lệ đạt QC"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_036 |
| **Tên Use Case** | Báo cáo tỷ lệ đạt QC |
| **Tác nhân** | QC Staff |
| **Mô tả chức năng** | Cho phép QC Staff thực hiện chức năng "Báo cáo tỷ lệ đạt QC" thuộc nhóm Chất lượng (QC) trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Quality yield report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [QC Staff] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo tỷ lệ đạt QC» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo tỷ lệ đạt QC» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo tỷ lệ đạt QC» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. QC Staff mở «Báo cáo tỷ lệ đạt QC» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Quality yield report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo tỷ lệ đạt QC» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Sản xuất theo lô/mẻ (`MFG-07`)

Nhóm **Sản xuất theo lô/mẻ** gồm **4** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 37. Đặc tả Use Case "Lô/mẻ sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_037 |
| **Tên Use Case** | Lô/mẻ sản xuất |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Lô/mẻ sản xuất" thuộc nhóm Sản xuất theo lô/mẻ trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Batch/lot production |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lô/mẻ sản xuất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lô/mẻ sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lô/mẻ sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Lô/mẻ sản xuất» trong nhóm Sản xuất theo lô/mẻ.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Batch/lot production).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lô/mẻ sản xuất».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lô/mẻ sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 38. Đặc tả Use Case "Ghi nhận thông số mẻ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_038 |
| **Tên Use Case** | Ghi nhận thông số mẻ |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Ghi nhận thông số mẻ" thuộc nhóm Sản xuất theo lô/mẻ trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Process parameters |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận thông số mẻ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận thông số mẻ» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận thông số mẻ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Ghi nhận thông số mẻ» trong nhóm Sản xuất theo lô/mẻ.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Process parameters).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận thông số mẻ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận thông số mẻ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 39. Đặc tả Use Case "Đóng gói & gắn tem"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_039 |
| **Tên Use Case** | Đóng gói & gắn tem |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Đóng gói & gắn tem" thuộc nhóm Sản xuất theo lô/mẻ trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Packaging & labeling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng gói & gắn tem» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng gói & gắn tem» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng gói & gắn tem» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Đóng gói & gắn tem» trong nhóm Sản xuất theo lô/mẻ.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Packaging & labeling).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng gói & gắn tem».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng gói & gắn tem» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 40. Đặc tả Use Case "Định mức phối trộn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_040 |
| **Tên Use Case** | Định mức phối trộn |
| **Tác nhân** | Shop Supervisor |
| **Mô tả chức năng** | Cho phép Shop Supervisor thực hiện chức năng "Định mức phối trộn" thuộc nhóm Sản xuất theo lô/mẻ trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Blend recipe |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Shop Supervisor] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Định mức phối trộn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Định mức phối trộn» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Định mức phối trộn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Shop Supervisor khởi tạo thao tác «Định mức phối trộn» trong nhóm Sản xuất theo lô/mẻ.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Blend recipe).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Định mức phối trộn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Định mức phối trộn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.8. Báo cáo sản xuất (`MFG-08`)

Nhóm **Báo cáo sản xuất** gồm **6** use case của module `MFG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 5 |

**Bảng 41. Đặc tả Use Case "Tiến độ lệnh sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_041 |
| **Tên Use Case** | Tiến độ lệnh sản xuất |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Tiến độ lệnh sản xuất" thuộc nhóm Báo cáo sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Work order status board |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiến độ lệnh sản xuất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiến độ lệnh sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiến độ lệnh sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Tiến độ lệnh sản xuất» trong nhóm Báo cáo sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Work order status board).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiến độ lệnh sản xuất».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiến độ lệnh sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 42. Đặc tả Use Case "Sản lượng theo ngày/ca/xưởng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_042 |
| **Tên Use Case** | Sản lượng theo ngày/ca/xưởng |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Sản lượng theo ngày/ca/xưởng" thuộc nhóm Báo cáo sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Production output |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sản lượng theo ngày/ca/xưởng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sản lượng theo ngày/ca/xưởng» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sản lượng theo ngày/ca/xưởng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Sản lượng theo ngày/ca/xưởng» trong nhóm Báo cáo sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Production output).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sản lượng theo ngày/ca/xưởng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sản lượng theo ngày/ca/xưởng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 43. Đặc tả Use Case "Tiêu hao nguyên vật liệu variance"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_043 |
| **Tên Use Case** | Tiêu hao nguyên vật liệu variance |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Tiêu hao nguyên vật liệu variance" thuộc nhóm Báo cáo sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Material usage variance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiêu hao nguyên vật liệu variance» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiêu hao nguyên vật liệu variance» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiêu hao nguyên vật liệu variance» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Tiêu hao nguyên vật liệu variance» trong nhóm Báo cáo sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Material usage variance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiêu hao nguyên vật liệu variance».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiêu hao nguyên vật liệu variance» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 44. Đặc tả Use Case "Hiệu suất / OEE"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_044 |
| **Tên Use Case** | Hiệu suất / OEE |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Hiệu suất / OEE" thuộc nhóm Báo cáo sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Manufacturing efficiency |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hiệu suất / OEE» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hiệu suất / OEE» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hiệu suất / OEE» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Production Planner khởi tạo thao tác «Hiệu suất / OEE» trong nhóm Báo cáo sản xuất.<br>2. Hệ thống kiểm tra license `MFG`, quyền RBAC và tiền điều kiện nghiệp vụ (Manufacturing efficiency).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hiệu suất / OEE».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hiệu suất / OEE» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 45. Đặc tả Use Case "Dashboard sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_045 |
| **Tên Use Case** | Dashboard sản xuất |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Dashboard sản xuất" thuộc nhóm Báo cáo sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Manufacturing dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard sản xuất» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner mở «Dashboard sản xuất» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Manufacturing dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 46. Đặc tả Use Case "Xuất báo cáo sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_MFG_046 |
| **Tên Use Case** | Xuất báo cáo sản xuất |
| **Tác nhân** | Production Planner |
| **Mô tả chức năng** | Cho phép Production Planner thực hiện chức năng "Xuất báo cáo sản xuất" thuộc nhóm Báo cáo sản xuất trong module MFG — Sản xuất (Manufacturing). Mô tả chi tiết: Export production reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Production Planner] và được cấp quyền RBAC tương ứng.<br>• License module `MFG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo sản xuất» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-MFG-SCOPE-01`, `BR-MFG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo sản xuất» được lưu nhất quán trong module `MFG`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Production Planner mở «Xuất báo cáo sản xuất», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo sản xuất» (Export production reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-MFG-01 — Lệnh sản xuất chuẩn

**Mục tiêu:** Hoàn thành WO và nhập TP

| Bước | Mô tả |
|---:|---|
| 1 | Tạo WO từ kế hoạch/đơn |
| 2 | Duyệt/phát hành; xuất NVL |
| 3 | Ghi nhận tiến độ/phế phẩm |
| 4 | QC (nếu bật); nhập TP |
| 5 | Đóng lệnh; tính giá thành; post FIN/INV |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Bom / BomVersion` | Định mức |
| `WorkCenter / Operation` | Năng lực & công đoạn |
| `WorkOrder` | Lệnh SX |
| `MaterialIssue / FgReceipt` | Xuất–nhập |
| `QcResult` | QC |
| `ProductionCost` | Giá thành |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `MFG`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-MFG-01: WO chỉ xuất NVL khi đã release.
- BR-MFG-02: Không nhập TP vượt dung sai WO nếu policy chặn.
- BR-MFG-03: Đóng WO khi đã xử lý đủ NVL/TP/phế theo rule.
- BR-MFG-04: BOM dùng để tính nhu cầu phải ở phiên bản hiệu lực.
- BR-MFG-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-MFG-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-MFG-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Batch jobs | MRP-light chạy theo lịch |
| Traceability | Truy vết NVL→TP theo lô khi bật |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `MFG` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `mfg.bom.manage` | Quyền chức năng module |
| `mfg.wo.manage` | Quyền chức năng module |
| `mfg.wo.release` | Quyền chức năng module |
| `mfg.issue.post` | Quyền chức năng module |
| `mfg.qc.manage` | Quyền chức năng module |
| `mfg.cost.run` | Quyền chức năng module |
| `mfg.report.view` | Quyền chức năng module |
| `mfg.*.view` | Xem trong data scope |
| `mfg.*.manage` | Tạo/sửa trong data scope |
| `mfg.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| WO on-time | Theo dõi vận hành module |
| Material variance | Theo dõi vận hành module |
| Yield/QC pass rate | Theo dõi vận hành module |
| Output by center | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Áp dụng cho sản xuất rời rạc hoặc theo mẻ cấu hình được.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có cần routing nhiều công đoạn ngay phase 1?

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
| Bản SRS này | `SRS_MFG_v1.1.md` / `.docx` |
| UC IDs | `UC_MFG_001` … |

---

*Hết tài liệu SRS-MFG-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
