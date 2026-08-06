# SRS-FIN-v1.1 — Tài chính – Kế toán

> **Software Requirements Specification — Module FIN**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-FIN-v1.1` |
| Module | `FIN` — Tài chính – Kế toán |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Tài chính |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `CRM`, `PUR`, `INV`, `POS`, `HRM`, `AST` |
| Số nhóm / UC | 13 nhóm / 83 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Tài chính – Kế toán** (`FIN`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module FIN cung cấp COA, kỳ kế toán, sổ cái, quỹ–ngân hàng, AR/AP, thuế/HĐĐT (khung), ghi nhận doanh thu–chi phí từ module khác, ngân sách và báo cáo tài chính quản trị.

### 1.3. Mục tiêu đo được
1. Single source of truth cho số liệu tài chính quản trị.
2. Đối soát công nợ phải thu/trả.
3. Khóa sổ kỳ có kiểm soát.
4. Nhận bút toán tự động từ các module vận hành.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- COA/period, GL, cash/bank, AR, AP, e-invoice framework, tax, revenue/cost postings, budget, FIN reports.

### 2.2. Out of Scope
- Thay thế hoàn toàn phần mềm thuế chuyên sâu (có thể tích hợp).
- Hợp nhất báo cáo tập đoàn phức tạp đa chuẩn kế toán ngay phase 1.

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `CRM`, `PUR`, `INV`, `POS`, `HRM`, `AST`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Chief Accountant | COA, kỳ, khóa sổ |
| GL Accountant | Bút toán / sổ cái |
| AR Accountant | Công nợ khách |
| AP Accountant | Công nợ NCC |
| Treasurer | Quỹ – ngân hàng |
| CFO / Manager | Báo cáo & ngân sách |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| COA | Chart of Accounts |
| GL | General Ledger |
| AR/AP | Accounts Receivable / Payable |
| Trial balance | Cân đối phát sinh |
| Posting | Hạch toán từ chứng từ nguồn |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- FIN (Tài chính – Kế toán)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `FIN` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `FIN` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM/POS | Doanh thu & thanh toán |
| Tích hợp | PUR/INV | AP & kho |
| Tích hợp | HRM | Chi phí lương |
| Tích hợp | AST | Khấu hao |
| Tích hợp | LOG | COD |
| Tích hợp | E-invoice provider | HĐĐT |
| Tích hợp | Bank | Sao kê / chi hộ |

---

## 6. Catalog chức năng

**Tổng:** 13 nhóm · 83 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `FIN-01` | Danh mục kế toán | 9 | 7 | 2 | 0 |
| 2 | `FIN-02` | Sổ cái & bút toán | 8 | 6 | 2 | 0 |
| 3 | `FIN-03` | Quỹ tiền mặt | 6 | 4 | 2 | 0 |
| 4 | `FIN-04` | Ngân hàng | 6 | 5 | 1 | 0 |
| 5 | `FIN-05` | Công nợ phải thu (AR) | 9 | 5 | 3 | 1 |
| 6 | `FIN-06` | Công nợ phải trả (AP) | 8 | 6 | 2 | 0 |
| 7 | `FIN-07` | Hóa đơn điện tử | 5 | 0 | 5 | 0 |
| 8 | `FIN-08` | Thuế | 5 | 3 | 2 | 0 |
| 9 | `FIN-09` | Doanh thu & giá vốn | 6 | 4 | 1 | 1 |
| 10 | `FIN-10` | Chi phí & phân bổ | 5 | 2 | 3 | 0 |
| 11 | `FIN-11` | Kết chuyển & khóa sổ | 4 | 3 | 1 | 0 |
| 12 | `FIN-12` | Ngân sách tài chính | 4 | 0 | 3 | 1 |
| 13 | `FIN-13` | Báo cáo tài chính & quản trị | 8 | 8 | 0 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_FIN_001` | Danh mục kế toán | Hệ thống tài khoản (COA) | Must |
| `UC_FIN_002` | Danh mục kế toán | Nhóm tài khoản / chỉ tiêu | Must |
| `UC_FIN_003` | Danh mục kế toán | Kỳ kế toán / năm tài chính | Must |
| `UC_FIN_004` | Danh mục kế toán | Khóa sổ kỳ / mở lại | Must |
| `UC_FIN_005` | Danh mục kế toán | Đồng tiền hạch toán & tỷ giá | Should |
| `UC_FIN_006` | Danh mục kế toán | Trung tâm chi phí | Must |
| `UC_FIN_007` | Danh mục kế toán | Khoản mục thu/chi | Should |
| `UC_FIN_008` | Danh mục kế toán | Hình thức thanh toán | Must |
| `UC_FIN_009` | Danh mục kế toán | Danh mục thuế | Must |
| `UC_FIN_010` | Sổ cái & bút toán | Tạo bút toán thủ công | Must |
| `UC_FIN_011` | Sổ cái & bút toán | Bút toán định kỳ / mẫu | Should |
| `UC_FIN_012` | Sổ cái & bút toán | Đảo bút toán | Must |
| `UC_FIN_013` | Sổ cái & bút toán | Xem sổ cái theo tài khoản | Must |
| `UC_FIN_014` | Sổ cái & bút toán | Sổ chi tiết theo đối tượng | Must |
| `UC_FIN_015` | Sổ cái & bút toán | Nhận bút toán tự động | Must |
| `UC_FIN_016` | Sổ cái & bút toán | Kiểm soát bút toán lệch Nợ/Có | Must |
| `UC_FIN_017` | Sổ cái & bút toán | Đính kèm chứng từ gốc | Should |
| `UC_FIN_018` | Quỹ tiền mặt | Danh mục quỹ / thủ quỹ | Must |
| `UC_FIN_019` | Quỹ tiền mặt | Phiếu thu tiền mặt | Must |
| `UC_FIN_020` | Quỹ tiền mặt | Phiếu chi tiền mặt | Must |
| `UC_FIN_021` | Quỹ tiền mặt | Đề nghị tạm ứng / hoàn ứng | Should |
| `UC_FIN_022` | Quỹ tiền mặt | Kiểm kê quỹ | Should |
| `UC_FIN_023` | Quỹ tiền mặt | Báo cáo sổ quỹ | Must |
| `UC_FIN_024` | Ngân hàng | Danh mục tài khoản ngân hàng | Must |
| `UC_FIN_025` | Ngân hàng | Giấy báo Nợ / Có | Must |
| `UC_FIN_026` | Ngân hàng | Đối soát sao kê ngân hàng | Must |
| `UC_FIN_027` | Ngân hàng | Đề nghị chuyển khoản | Must |
| `UC_FIN_028` | Ngân hàng | Import sao kê | Should |
| `UC_FIN_029` | Ngân hàng | Theo dõi số dư ngân hàng | Must |
| `UC_FIN_030` | Công nợ phải thu (AR) | Tạo hóa đơn phải thu | Must |
| `UC_FIN_031` | Công nợ phải thu (AR) | Công nợ theo khách / hóa đơn | Must |
| `UC_FIN_032` | Công nợ phải thu (AR) | Thu tiền & phân bổ | Must |
| `UC_FIN_033` | Công nợ phải thu (AR) | Bù trừ công nợ | Should |
| `UC_FIN_034` | Công nợ phải thu (AR) | Nhắc nợ tự động | Should |
| `UC_FIN_035` | Công nợ phải thu (AR) | Cảnh báo vượt hạn mức | Must |
| `UC_FIN_036` | Công nợ phải thu (AR) | Bảng tuổi nợ phải thu | Must |
| `UC_FIN_037` | Công nợ phải thu (AR) | Xử lý nợ khó đòi | Could |
| `UC_FIN_038` | Công nợ phải thu (AR) | Đối soát COD về AR | Should |
| `UC_FIN_039` | Công nợ phải trả (AP) | Tạo hóa đơn phải trả | Must |
| `UC_FIN_040` | Công nợ phải trả (AP) | Công nợ theo nhà cung cấp | Must |
| `UC_FIN_041` | Công nợ phải trả (AP) | Đề nghị thanh toán | Must |
| `UC_FIN_042` | Công nợ phải trả (AP) | Duyệt chi trả | Must |
| `UC_FIN_043` | Công nợ phải trả (AP) | Thanh toán & phân bổ AP | Must |
| `UC_FIN_044` | Công nợ phải trả (AP) | Bảng tuổi nợ phải trả | Must |
| `UC_FIN_045` | Công nợ phải trả (AP) | Tạm ứng nhà cung cấp | Should |
| `UC_FIN_046` | Công nợ phải trả (AP) | Đối soát 3 chiều | Should |
| `UC_FIN_047` | Hóa đơn điện tử | Cấu hình nhà cung cấp HĐĐT | Should |
| `UC_FIN_048` | Hóa đơn điện tử | Phát hành hóa đơn điện tử | Should |
| `UC_FIN_049` | Hóa đơn điện tử | Điều chỉnh / thay thế / hủy | Should |
| `UC_FIN_050` | Hóa đơn điện tử | Tra cứu trạng thái phát hành | Should |
| `UC_FIN_051` | Hóa đơn điện tử | Lưu trữ bảng kê HĐĐT | Should |
| `UC_FIN_052` | Thuế | Tính thuế GTGT đầu ra / đầu vào | Must |
| `UC_FIN_053` | Thuế | Bảng kê hóa đơn GTGT | Must |
| `UC_FIN_054` | Thuế | Tờ khai thuế GTGT | Should |
| `UC_FIN_055` | Thuế | Thuế TNCN từ lương | Should |
| `UC_FIN_056` | Thuế | Cấu hình thuế suất | Must |
| `UC_FIN_057` | Doanh thu & giá vốn | Ghi nhận doanh thu từ POS | Must |
| `UC_FIN_058` | Doanh thu & giá vốn | Ghi nhận doanh thu từ đơn | Must |
| `UC_FIN_059` | Doanh thu & giá vốn | Ghi nhận doanh thu dự án | Should |
| `UC_FIN_060` | Doanh thu & giá vốn | Ghi nhận giá vốn hàng bán | Must |
| `UC_FIN_061` | Doanh thu & giá vốn | Doanh thu nhận trước | Could |
| `UC_FIN_062` | Doanh thu & giá vốn | Chiết khấu làm giảm doanh thu | Must |
| `UC_FIN_063` | Chi phí & phân bổ | Ghi nhận chi phí hoạt động | Must |
| `UC_FIN_064` | Chi phí & phân bổ | Phân bổ chi phí | Should |
| `UC_FIN_065` | Chi phí & phân bổ | Chi phí lương từ HRM | Must |
| `UC_FIN_066` | Chi phí & phân bổ | Chi phí marketing từ CRM | Should |
| `UC_FIN_067` | Chi phí & phân bổ | Tạm ứng chi phí / quyết toán | Should |
| `UC_FIN_068` | Kết chuyển & khóa sổ | Kết chuyển lãi/lỗ cuối kỳ | Must |
| `UC_FIN_069` | Kết chuyển & khóa sổ | Đối chiếu công nợ – sổ cái | Must |
| `UC_FIN_070` | Kết chuyển & khóa sổ | Checklist khóa sổ tháng | Should |
| `UC_FIN_071` | Kết chuyển & khóa sổ | Khóa sổ năm tài chính | Must |
| `UC_FIN_072` | Ngân sách tài chính | Lập ngân sách theo kỳ | Should |
| `UC_FIN_073` | Ngân sách tài chính | So sánh thực tế vs ngân sách | Should |
| `UC_FIN_074` | Ngân sách tài chính | Cảnh báo vượt ngân sách | Should |
| `UC_FIN_075` | Ngân sách tài chính | Phiên bản ngân sách | Could |
| `UC_FIN_076` | Báo cáo tài chính & quản trị | Bảng cân đối phát sinh | Must |
| `UC_FIN_077` | Báo cáo tài chính & quản trị | Báo cáo P&L quản trị | Must |
| `UC_FIN_078` | Báo cáo tài chính & quản trị | Bảng cân đối kế toán | Must |
| `UC_FIN_079` | Báo cáo tài chính & quản trị | Báo cáo lưu chuyển tiền tệ | Must |
| `UC_FIN_080` | Báo cáo tài chính & quản trị | P&L theo chi nhánh / đơn vị | Must |
| `UC_FIN_081` | Báo cáo tài chính & quản trị | Báo cáo công nợ tổng hợp | Must |
| `UC_FIN_082` | Báo cáo tài chính & quản trị | Dashboard tài chính | Must |
| `UC_FIN_083` | Báo cáo tài chính & quản trị | Xuất báo cáo tài chính | Must |

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

### 7.1. Danh mục kế toán (`FIN-01`)

Nhóm **Danh mục kế toán** gồm **9** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 7 |

**Bảng 1. Đặc tả Use Case "Hệ thống tài khoản (COA)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_001 |
| **Tên Use Case** | Hệ thống tài khoản (COA) |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Hệ thống tài khoản (COA)" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Chart of accounts |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hệ thống tài khoản (COA)» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hệ thống tài khoản (COA)» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hệ thống tài khoản (COA)» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Hệ thống tài khoản (COA)» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Chart of accounts).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hệ thống tài khoản (COA)».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hệ thống tài khoản (COA)» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 2. Đặc tả Use Case "Nhóm tài khoản / chỉ tiêu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_002 |
| **Tên Use Case** | Nhóm tài khoản / chỉ tiêu |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Nhóm tài khoản / chỉ tiêu" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Account groups |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhóm tài khoản / chỉ tiêu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhóm tài khoản / chỉ tiêu» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhóm tài khoản / chỉ tiêu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Nhóm tài khoản / chỉ tiêu» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Account groups).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhóm tài khoản / chỉ tiêu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhóm tài khoản / chỉ tiêu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 3. Đặc tả Use Case "Kỳ kế toán / năm tài chính"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_003 |
| **Tên Use Case** | Kỳ kế toán / năm tài chính |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Kỳ kế toán / năm tài chính" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Fiscal calendar |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kỳ kế toán / năm tài chính» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kỳ kế toán / năm tài chính» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kỳ kế toán / năm tài chính» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Kỳ kế toán / năm tài chính» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Fiscal calendar).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kỳ kế toán / năm tài chính».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kỳ kế toán / năm tài chính» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 4. Đặc tả Use Case "Khóa sổ kỳ / mở lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_004 |
| **Tên Use Case** | Khóa sổ kỳ / mở lại |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Khóa sổ kỳ / mở lại" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Period lock/unlock |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa sổ kỳ / mở lại» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-LOCK-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa sổ kỳ / mở lại» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa sổ kỳ / mở lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant chọn kỳ/ca/đối tượng cần khóa trong «Khóa sổ kỳ / mở lại».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa sổ kỳ / mở lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 5. Đặc tả Use Case "Đồng tiền hạch toán & tỷ giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_005 |
| **Tên Use Case** | Đồng tiền hạch toán & tỷ giá |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Đồng tiền hạch toán & tỷ giá" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Currency & exchange rates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng tiền hạch toán & tỷ giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng tiền hạch toán & tỷ giá» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng tiền hạch toán & tỷ giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Đồng tiền hạch toán & tỷ giá» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Currency & exchange rates).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng tiền hạch toán & tỷ giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng tiền hạch toán & tỷ giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 6. Đặc tả Use Case "Trung tâm chi phí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_006 |
| **Tên Use Case** | Trung tâm chi phí |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Trung tâm chi phí" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cost center master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trung tâm chi phí» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trung tâm chi phí» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trung tâm chi phí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Trung tâm chi phí» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Cost center master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Trung tâm chi phí».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trung tâm chi phí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 7. Đặc tả Use Case "Khoản mục thu/chi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_007 |
| **Tên Use Case** | Khoản mục thu/chi |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Khoản mục thu/chi" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash flow line items |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khoản mục thu/chi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khoản mục thu/chi» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khoản mục thu/chi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Khoản mục thu/chi» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Cash flow line items).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khoản mục thu/chi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khoản mục thu/chi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 8. Đặc tả Use Case "Hình thức thanh toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_008 |
| **Tên Use Case** | Hình thức thanh toán |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Hình thức thanh toán" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Payment methods |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hình thức thanh toán» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-PAY-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hình thức thanh toán» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hình thức thanh toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant chọn chứng từ cần thu/chi trong «Hình thức thanh toán».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hình thức thanh toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 9. Đặc tả Use Case "Danh mục thuế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_009 |
| **Tên Use Case** | Danh mục thuế |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Danh mục thuế" thuộc nhóm Danh mục kế toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Tax code master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục thuế» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục thuế» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục thuế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Danh mục thuế» trong nhóm Danh mục kế toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Tax code master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục thuế».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục thuế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.2. Sổ cái & bút toán (`FIN-02`)

Nhóm **Sổ cái & bút toán** gồm **8** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 6 |

**Bảng 10. Đặc tả Use Case "Tạo bút toán thủ công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_010 |
| **Tên Use Case** | Tạo bút toán thủ công |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Tạo bút toán thủ công" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Manual journal entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo bút toán thủ công» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo bút toán thủ công» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo bút toán thủ công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant mở chức năng «Tạo bút toán thủ công» trong nhóm Sổ cái & bút toán.<br>2. Hệ thống kiểm tra license `FIN`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo bút toán thủ công» (Manual journal entry).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo bút toán thủ công» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo bút toán thủ công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 11. Đặc tả Use Case "Bút toán định kỳ / mẫu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_011 |
| **Tên Use Case** | Bút toán định kỳ / mẫu |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Bút toán định kỳ / mẫu" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Recurring journal |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bút toán định kỳ / mẫu» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bút toán định kỳ / mẫu» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bút toán định kỳ / mẫu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Bút toán định kỳ / mẫu» trong nhóm Sổ cái & bút toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Recurring journal).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bút toán định kỳ / mẫu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bút toán định kỳ / mẫu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 12. Đặc tả Use Case "Đảo bút toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_012 |
| **Tên Use Case** | Đảo bút toán |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Đảo bút toán" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Journal reversal |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đảo bút toán» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đảo bút toán» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đảo bút toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Đảo bút toán» trong nhóm Sổ cái & bút toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Journal reversal).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đảo bút toán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đảo bút toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 13. Đặc tả Use Case "Xem sổ cái theo tài khoản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_013 |
| **Tên Use Case** | Xem sổ cái theo tài khoản |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Xem sổ cái theo tài khoản" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: GL inquiry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem sổ cái theo tài khoản» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem sổ cái theo tài khoản» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem sổ cái theo tài khoản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant mở «Xem sổ cái theo tài khoản» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (GL inquiry).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem sổ cái theo tài khoản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 14. Đặc tả Use Case "Sổ chi tiết theo đối tượng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_014 |
| **Tên Use Case** | Sổ chi tiết theo đối tượng |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Sổ chi tiết theo đối tượng" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Subledger inquiry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sổ chi tiết theo đối tượng» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sổ chi tiết theo đối tượng» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sổ chi tiết theo đối tượng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Sổ chi tiết theo đối tượng» trong nhóm Sổ cái & bút toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Subledger inquiry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sổ chi tiết theo đối tượng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sổ chi tiết theo đối tượng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 15. Đặc tả Use Case "Nhận bút toán tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_015 |
| **Tên Use Case** | Nhận bút toán tự động |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Nhận bút toán tự động" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Auto posting from modules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhận bút toán tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhận bút toán tự động» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhận bút toán tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Nhận bút toán tự động» trong nhóm Sổ cái & bút toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Auto posting from modules).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhận bút toán tự động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhận bút toán tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 16. Đặc tả Use Case "Kiểm soát bút toán lệch Nợ/Có"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_016 |
| **Tên Use Case** | Kiểm soát bút toán lệch Nợ/Có |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Kiểm soát bút toán lệch Nợ/Có" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Debit/credit balance check |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm soát bút toán lệch Nợ/Có» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm soát bút toán lệch Nợ/Có» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm soát bút toán lệch Nợ/Có» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Kiểm soát bút toán lệch Nợ/Có» trong nhóm Sổ cái & bút toán.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Debit/credit balance check).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm soát bút toán lệch Nợ/Có».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm soát bút toán lệch Nợ/Có» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 17. Đặc tả Use Case "Đính kèm chứng từ gốc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_017 |
| **Tên Use Case** | Đính kèm chứng từ gốc |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Đính kèm chứng từ gốc" thuộc nhóm Sổ cái & bút toán trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Voucher attachment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đính kèm chứng từ gốc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đính kèm chứng từ gốc» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đính kèm chứng từ gốc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant mở bản ghi liên quan và chọn «Đính kèm chứng từ gốc».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đính kèm chứng từ gốc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.3. Quỹ tiền mặt (`FIN-03`)

Nhóm **Quỹ tiền mặt** gồm **6** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 18. Đặc tả Use Case "Danh mục quỹ / thủ quỹ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_018 |
| **Tên Use Case** | Danh mục quỹ / thủ quỹ |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Danh mục quỹ / thủ quỹ" thuộc nhóm Quỹ tiền mặt trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash books |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục quỹ / thủ quỹ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục quỹ / thủ quỹ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục quỹ / thủ quỹ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Danh mục quỹ / thủ quỹ» trong nhóm Quỹ tiền mặt.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Cash books).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục quỹ / thủ quỹ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục quỹ / thủ quỹ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 19. Đặc tả Use Case "Phiếu thu tiền mặt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_019 |
| **Tên Use Case** | Phiếu thu tiền mặt |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Phiếu thu tiền mặt" thuộc nhóm Quỹ tiền mặt trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiếu thu tiền mặt» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-PAY-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiếu thu tiền mặt» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiếu thu tiền mặt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer chọn chứng từ cần thu/chi trong «Phiếu thu tiền mặt».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiếu thu tiền mặt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 20. Đặc tả Use Case "Phiếu chi tiền mặt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_020 |
| **Tên Use Case** | Phiếu chi tiền mặt |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Phiếu chi tiền mặt" thuộc nhóm Quỹ tiền mặt trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash payment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiếu chi tiền mặt» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-PAY-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiếu chi tiền mặt» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiếu chi tiền mặt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer chọn chứng từ cần thu/chi trong «Phiếu chi tiền mặt».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiếu chi tiền mặt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 21. Đặc tả Use Case "Đề nghị tạm ứng / hoàn ứng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_021 |
| **Tên Use Case** | Đề nghị tạm ứng / hoàn ứng |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Đề nghị tạm ứng / hoàn ứng" thuộc nhóm Quỹ tiền mặt trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Advance & settlement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề nghị tạm ứng / hoàn ứng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề nghị tạm ứng / hoàn ứng» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề nghị tạm ứng / hoàn ứng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Đề nghị tạm ứng / hoàn ứng» trong nhóm Quỹ tiền mặt.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Advance & settlement).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đề nghị tạm ứng / hoàn ứng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề nghị tạm ứng / hoàn ứng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 22. Đặc tả Use Case "Kiểm kê quỹ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_022 |
| **Tên Use Case** | Kiểm kê quỹ |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Kiểm kê quỹ" thuộc nhóm Quỹ tiền mặt trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash count |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm kê quỹ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm kê quỹ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm kê quỹ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Kiểm kê quỹ» trong nhóm Quỹ tiền mặt.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Cash count).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm kê quỹ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm kê quỹ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 23. Đặc tả Use Case "Báo cáo sổ quỹ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_023 |
| **Tên Use Case** | Báo cáo sổ quỹ |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Báo cáo sổ quỹ" thuộc nhóm Quỹ tiền mặt trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash book report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo sổ quỹ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo sổ quỹ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo sổ quỹ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer mở «Báo cáo sổ quỹ» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Cash book report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo sổ quỹ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.4. Ngân hàng (`FIN-04`)

Nhóm **Ngân hàng** gồm **6** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 5 |

**Bảng 24. Đặc tả Use Case "Danh mục tài khoản ngân hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_024 |
| **Tên Use Case** | Danh mục tài khoản ngân hàng |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Danh mục tài khoản ngân hàng" thuộc nhóm Ngân hàng trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Bank account master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục tài khoản ngân hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục tài khoản ngân hàng» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục tài khoản ngân hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Danh mục tài khoản ngân hàng» trong nhóm Ngân hàng.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Bank account master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục tài khoản ngân hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục tài khoản ngân hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 25. Đặc tả Use Case "Giấy báo Nợ / Có"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_025 |
| **Tên Use Case** | Giấy báo Nợ / Có |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Giấy báo Nợ / Có" thuộc nhóm Ngân hàng trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Bank voucher |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giấy báo Nợ / Có» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giấy báo Nợ / Có» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giấy báo Nợ / Có» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Giấy báo Nợ / Có» trong nhóm Ngân hàng.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Bank voucher).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giấy báo Nợ / Có».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giấy báo Nợ / Có» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 26. Đặc tả Use Case "Đối soát sao kê ngân hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_026 |
| **Tên Use Case** | Đối soát sao kê ngân hàng |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Đối soát sao kê ngân hàng" thuộc nhóm Ngân hàng trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Bank reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát sao kê ngân hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát sao kê ngân hàng» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát sao kê ngân hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Đối soát sao kê ngân hàng» trong nhóm Ngân hàng.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Bank reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát sao kê ngân hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát sao kê ngân hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 27. Đặc tả Use Case "Đề nghị chuyển khoản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_027 |
| **Tên Use Case** | Đề nghị chuyển khoản |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Đề nghị chuyển khoản" thuộc nhóm Ngân hàng trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Payment order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề nghị chuyển khoản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề nghị chuyển khoản» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề nghị chuyển khoản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Đề nghị chuyển khoản» trong nhóm Ngân hàng.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Payment order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đề nghị chuyển khoản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề nghị chuyển khoản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 28. Đặc tả Use Case "Import sao kê"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_028 |
| **Tên Use Case** | Import sao kê |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Import sao kê" thuộc nhóm Ngân hàng trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Bank statement import |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import sao kê» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-IMP-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import sao kê» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import sao kê» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Treasurer tải file mẫu (nếu có) và chọn file import cho «Import sao kê».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import sao kê» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 29. Đặc tả Use Case "Theo dõi số dư ngân hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_029 |
| **Tên Use Case** | Theo dõi số dư ngân hàng |
| **Tác nhân** | Treasurer |
| **Mô tả chức năng** | Cho phép Treasurer thực hiện chức năng "Theo dõi số dư ngân hàng" thuộc nhóm Ngân hàng trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Bank balance tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Treasurer] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi số dư ngân hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi số dư ngân hàng» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi số dư ngân hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Treasurer khởi tạo thao tác «Theo dõi số dư ngân hàng» trong nhóm Ngân hàng.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Bank balance tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi số dư ngân hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi số dư ngân hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.5. Công nợ phải thu (AR) (`FIN-05`)

Nhóm **Công nợ phải thu (AR)** gồm **9** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 5 |

**Bảng 30. Đặc tả Use Case "Tạo hóa đơn phải thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_030 |
| **Tên Use Case** | Tạo hóa đơn phải thu |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Tạo hóa đơn phải thu" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AR invoice |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo hóa đơn phải thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo hóa đơn phải thu» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hóa đơn phải thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AR Accountant mở chức năng «Tạo hóa đơn phải thu» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo hóa đơn phải thu» (AR invoice).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo hóa đơn phải thu» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo hóa đơn phải thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 31. Đặc tả Use Case "Công nợ theo khách / hóa đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_031 |
| **Tên Use Case** | Công nợ theo khách / hóa đơn |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Công nợ theo khách / hóa đơn" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AR open items |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Công nợ theo khách / hóa đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Công nợ theo khách / hóa đơn» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Công nợ theo khách / hóa đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AR Accountant khởi tạo thao tác «Công nợ theo khách / hóa đơn» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (AR open items).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Công nợ theo khách / hóa đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Công nợ theo khách / hóa đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 32. Đặc tả Use Case "Thu tiền & phân bổ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_032 |
| **Tên Use Case** | Thu tiền & phân bổ |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Thu tiền & phân bổ" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash application |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thu tiền & phân bổ» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-PAY-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thu tiền & phân bổ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thu tiền & phân bổ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AR Accountant chọn chứng từ cần thu/chi trong «Thu tiền & phân bổ».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thu tiền & phân bổ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 33. Đặc tả Use Case "Bù trừ công nợ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_033 |
| **Tên Use Case** | Bù trừ công nợ |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Bù trừ công nợ" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AR offset |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bù trừ công nợ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bù trừ công nợ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bù trừ công nợ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AR Accountant khởi tạo thao tác «Bù trừ công nợ» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (AR offset).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bù trừ công nợ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bù trừ công nợ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 34. Đặc tả Use Case "Nhắc nợ tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_034 |
| **Tên Use Case** | Nhắc nợ tự động |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Nhắc nợ tự động" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Dunning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhắc nợ tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhắc nợ tự động» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhắc nợ tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AR Accountant khởi tạo thao tác «Nhắc nợ tự động» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Dunning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhắc nợ tự động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhắc nợ tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 35. Đặc tả Use Case "Cảnh báo vượt hạn mức"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_035 |
| **Tên Use Case** | Cảnh báo vượt hạn mức |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Cảnh báo vượt hạn mức" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Credit limit alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo vượt hạn mức» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo vượt hạn mức» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo vượt hạn mức» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc AR Accountant kích hoạt kiểm tra điều kiện «Cảnh báo vượt hạn mức».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Credit limit alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo vượt hạn mức» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 36. Đặc tả Use Case "Bảng tuổi nợ phải thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_036 |
| **Tên Use Case** | Bảng tuổi nợ phải thu |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Bảng tuổi nợ phải thu" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AR aging report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng tuổi nợ phải thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng tuổi nợ phải thu» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng tuổi nợ phải thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AR Accountant khởi tạo thao tác «Bảng tuổi nợ phải thu» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (AR aging report).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng tuổi nợ phải thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng tuổi nợ phải thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 37. Đặc tả Use Case "Xử lý nợ khó đòi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_037 |
| **Tên Use Case** | Xử lý nợ khó đòi |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Xử lý nợ khó đòi" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Bad debt provision |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xử lý nợ khó đòi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xử lý nợ khó đòi» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xử lý nợ khó đòi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AR Accountant khởi tạo thao tác «Xử lý nợ khó đòi» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Bad debt provision).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xử lý nợ khó đòi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xử lý nợ khó đòi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 38. Đặc tả Use Case "Đối soát COD về AR"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_038 |
| **Tên Use Case** | Đối soát COD về AR |
| **Tác nhân** | AR Accountant |
| **Mô tả chức năng** | Cho phép AR Accountant thực hiện chức năng "Đối soát COD về AR" thuộc nhóm Công nợ phải thu (AR) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: COD to AR reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AR Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát COD về AR» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát COD về AR» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát COD về AR» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AR Accountant khởi tạo thao tác «Đối soát COD về AR» trong nhóm Công nợ phải thu (AR).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (COD to AR reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát COD về AR».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát COD về AR» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.6. Công nợ phải trả (AP) (`FIN-06`)

Nhóm **Công nợ phải trả (AP)** gồm **8** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 6 |

**Bảng 39. Đặc tả Use Case "Tạo hóa đơn phải trả"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_039 |
| **Tên Use Case** | Tạo hóa đơn phải trả |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Tạo hóa đơn phải trả" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AP invoice |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo hóa đơn phải trả» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo hóa đơn phải trả» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hóa đơn phải trả» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Accountant mở chức năng «Tạo hóa đơn phải trả» trong nhóm Công nợ phải trả (AP).<br>2. Hệ thống kiểm tra license `FIN`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo hóa đơn phải trả» (AP invoice).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo hóa đơn phải trả» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo hóa đơn phải trả» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 40. Đặc tả Use Case "Công nợ theo nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_040 |
| **Tên Use Case** | Công nợ theo nhà cung cấp |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Công nợ theo nhà cung cấp" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AP open items |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Công nợ theo nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Công nợ theo nhà cung cấp» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Công nợ theo nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Accountant khởi tạo thao tác «Công nợ theo nhà cung cấp» trong nhóm Công nợ phải trả (AP).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (AP open items).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Công nợ theo nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Công nợ theo nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 41. Đặc tả Use Case "Đề nghị thanh toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_041 |
| **Tên Use Case** | Đề nghị thanh toán |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Đề nghị thanh toán" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Payment proposal |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề nghị thanh toán» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-PAY-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề nghị thanh toán» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề nghị thanh toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Accountant chọn chứng từ cần thu/chi trong «Đề nghị thanh toán».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề nghị thanh toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 42. Đặc tả Use Case "Duyệt chi trả"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_042 |
| **Tên Use Case** | Duyệt chi trả |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Duyệt chi trả" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Payment approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt chi trả» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-APPR-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt chi trả» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt chi trả» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Accountant mở hộp chờ / chứng từ cần xử lý cho «Duyệt chi trả».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt chi trả», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt chi trả» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 43. Đặc tả Use Case "Thanh toán & phân bổ AP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_043 |
| **Tên Use Case** | Thanh toán & phân bổ AP |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Thanh toán & phân bổ AP" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AP payment application |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh toán & phân bổ AP» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-PAY-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh toán & phân bổ AP» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh toán & phân bổ AP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Accountant chọn chứng từ cần thu/chi trong «Thanh toán & phân bổ AP».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh toán & phân bổ AP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 44. Đặc tả Use Case "Bảng tuổi nợ phải trả"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_044 |
| **Tên Use Case** | Bảng tuổi nợ phải trả |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Bảng tuổi nợ phải trả" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AP aging report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng tuổi nợ phải trả» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng tuổi nợ phải trả» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng tuổi nợ phải trả» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. AP Accountant khởi tạo thao tác «Bảng tuổi nợ phải trả» trong nhóm Công nợ phải trả (AP).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (AP aging report).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng tuổi nợ phải trả».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng tuổi nợ phải trả» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 45. Đặc tả Use Case "Tạm ứng nhà cung cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_045 |
| **Tên Use Case** | Tạm ứng nhà cung cấp |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Tạm ứng nhà cung cấp" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Vendor prepayment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạm ứng nhà cung cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạm ứng nhà cung cấp» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạm ứng nhà cung cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AP Accountant khởi tạo thao tác «Tạm ứng nhà cung cấp» trong nhóm Công nợ phải trả (AP).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Vendor prepayment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tạm ứng nhà cung cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạm ứng nhà cung cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 46. Đặc tả Use Case "Đối soát 3 chiều"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_046 |
| **Tên Use Case** | Đối soát 3 chiều |
| **Tác nhân** | AP Accountant |
| **Mô tả chức năng** | Cho phép AP Accountant thực hiện chức năng "Đối soát 3 chiều" thuộc nhóm Công nợ phải trả (AP) trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: 3-way match from FIN view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [AP Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát 3 chiều» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát 3 chiều» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát 3 chiều» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. AP Accountant khởi tạo thao tác «Đối soát 3 chiều» trong nhóm Công nợ phải trả (AP).<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (3-way match from FIN view).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát 3 chiều».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát 3 chiều» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.7. Hóa đơn điện tử (`FIN-07`)

Nhóm **Hóa đơn điện tử** gồm **5** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 0 |

**Bảng 47. Đặc tả Use Case "Cấu hình nhà cung cấp HĐĐT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_047 |
| **Tên Use Case** | Cấu hình nhà cung cấp HĐĐT |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Cấu hình nhà cung cấp HĐĐT" thuộc nhóm Hóa đơn điện tử trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: E-invoice provider setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình nhà cung cấp HĐĐT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình nhà cung cấp HĐĐT» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình nhà cung cấp HĐĐT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant mở màn hình cấu hình «Cấu hình nhà cung cấp HĐĐT» trong Hóa đơn điện tử.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (E-invoice provider setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình nhà cung cấp HĐĐT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 48. Đặc tả Use Case "Phát hành hóa đơn điện tử"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_048 |
| **Tên Use Case** | Phát hành hóa đơn điện tử |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Phát hành hóa đơn điện tử" thuộc nhóm Hóa đơn điện tử trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Issue e-invoice |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phát hành hóa đơn điện tử» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phát hành hóa đơn điện tử» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phát hành hóa đơn điện tử» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Phát hành hóa đơn điện tử» trong nhóm Hóa đơn điện tử.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Issue e-invoice).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phát hành hóa đơn điện tử».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phát hành hóa đơn điện tử» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 49. Đặc tả Use Case "Điều chỉnh / thay thế / hủy"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_049 |
| **Tên Use Case** | Điều chỉnh / thay thế / hủy |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Điều chỉnh / thay thế / hủy" thuộc nhóm Hóa đơn điện tử trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: E-invoice adjustment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điều chỉnh / thay thế / hủy» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điều chỉnh / thay thế / hủy» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điều chỉnh / thay thế / hủy» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant tìm và mở bản ghi liên quan tới «Điều chỉnh / thay thế / hủy» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Điều chỉnh / thay thế / hủy» (E-invoice adjustment).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điều chỉnh / thay thế / hủy» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 50. Đặc tả Use Case "Tra cứu trạng thái phát hành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_050 |
| **Tên Use Case** | Tra cứu trạng thái phát hành |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Tra cứu trạng thái phát hành" thuộc nhóm Hóa đơn điện tử trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: E-invoice status |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tra cứu trạng thái phát hành» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tra cứu trạng thái phát hành» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tra cứu trạng thái phát hành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant mở «Tra cứu trạng thái phát hành» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (E-invoice status).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tra cứu trạng thái phát hành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 51. Đặc tả Use Case "Lưu trữ bảng kê HĐĐT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_051 |
| **Tên Use Case** | Lưu trữ bảng kê HĐĐT |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Lưu trữ bảng kê HĐĐT" thuộc nhóm Hóa đơn điện tử trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: E-invoice registry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lưu trữ bảng kê HĐĐT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lưu trữ bảng kê HĐĐT» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lưu trữ bảng kê HĐĐT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Lưu trữ bảng kê HĐĐT» trong nhóm Hóa đơn điện tử.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (E-invoice registry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lưu trữ bảng kê HĐĐT».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lưu trữ bảng kê HĐĐT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.8. Thuế (`FIN-08`)

Nhóm **Thuế** gồm **5** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 3 |

**Bảng 52. Đặc tả Use Case "Tính thuế GTGT đầu ra / đầu vào"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_052 |
| **Tên Use Case** | Tính thuế GTGT đầu ra / đầu vào |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Tính thuế GTGT đầu ra / đầu vào" thuộc nhóm Thuế trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: VAT calculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính thuế GTGT đầu ra / đầu vào» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính thuế GTGT đầu ra / đầu vào» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính thuế GTGT đầu ra / đầu vào» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Tính thuế GTGT đầu ra / đầu vào» trong nhóm Thuế.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (VAT calculation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tính thuế GTGT đầu ra / đầu vào».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính thuế GTGT đầu ra / đầu vào» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 53. Đặc tả Use Case "Bảng kê hóa đơn GTGT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_053 |
| **Tên Use Case** | Bảng kê hóa đơn GTGT |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Bảng kê hóa đơn GTGT" thuộc nhóm Thuế trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: VAT listing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng kê hóa đơn GTGT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng kê hóa đơn GTGT» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng kê hóa đơn GTGT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Bảng kê hóa đơn GTGT» trong nhóm Thuế.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (VAT listing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng kê hóa đơn GTGT».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng kê hóa đơn GTGT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 54. Đặc tả Use Case "Tờ khai thuế GTGT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_054 |
| **Tên Use Case** | Tờ khai thuế GTGT |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Tờ khai thuế GTGT" thuộc nhóm Thuế trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: VAT return preparation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tờ khai thuế GTGT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tờ khai thuế GTGT» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tờ khai thuế GTGT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Tờ khai thuế GTGT» trong nhóm Thuế.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (VAT return preparation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tờ khai thuế GTGT».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tờ khai thuế GTGT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 55. Đặc tả Use Case "Thuế TNCN từ lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_055 |
| **Tên Use Case** | Thuế TNCN từ lương |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Thuế TNCN từ lương" thuộc nhóm Thuế trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Personal income tax from payroll |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thuế TNCN từ lương» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thuế TNCN từ lương» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thuế TNCN từ lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Thuế TNCN từ lương» trong nhóm Thuế.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Personal income tax from payroll).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thuế TNCN từ lương».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thuế TNCN từ lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 56. Đặc tả Use Case "Cấu hình thuế suất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_056 |
| **Tên Use Case** | Cấu hình thuế suất |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Cấu hình thuế suất" thuộc nhóm Thuế trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Tax rate configuration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thuế suất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thuế suất» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thuế suất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant mở màn hình cấu hình «Cấu hình thuế suất» trong Thuế.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Tax rate configuration) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thuế suất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.9. Doanh thu & giá vốn (`FIN-09`)

Nhóm **Doanh thu & giá vốn** gồm **6** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 57. Đặc tả Use Case "Ghi nhận doanh thu từ POS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_057 |
| **Tên Use Case** | Ghi nhận doanh thu từ POS |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Ghi nhận doanh thu từ POS" thuộc nhóm Doanh thu & giá vốn trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: POS revenue posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận doanh thu từ POS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận doanh thu từ POS» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận doanh thu từ POS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Ghi nhận doanh thu từ POS» trong nhóm Doanh thu & giá vốn.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (POS revenue posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận doanh thu từ POS».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận doanh thu từ POS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 58. Đặc tả Use Case "Ghi nhận doanh thu từ đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_058 |
| **Tên Use Case** | Ghi nhận doanh thu từ đơn |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Ghi nhận doanh thu từ đơn" thuộc nhóm Doanh thu & giá vốn trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Order revenue posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận doanh thu từ đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận doanh thu từ đơn» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận doanh thu từ đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Ghi nhận doanh thu từ đơn» trong nhóm Doanh thu & giá vốn.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Order revenue posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận doanh thu từ đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận doanh thu từ đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 59. Đặc tả Use Case "Ghi nhận doanh thu dự án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_059 |
| **Tên Use Case** | Ghi nhận doanh thu dự án |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Ghi nhận doanh thu dự án" thuộc nhóm Doanh thu & giá vốn trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Project revenue recognition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận doanh thu dự án» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận doanh thu dự án» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận doanh thu dự án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Ghi nhận doanh thu dự án» trong nhóm Doanh thu & giá vốn.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Project revenue recognition).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận doanh thu dự án».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận doanh thu dự án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 60. Đặc tả Use Case "Ghi nhận giá vốn hàng bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_060 |
| **Tên Use Case** | Ghi nhận giá vốn hàng bán |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Ghi nhận giá vốn hàng bán" thuộc nhóm Doanh thu & giá vốn trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: COGS posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận giá vốn hàng bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận giá vốn hàng bán» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận giá vốn hàng bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Ghi nhận giá vốn hàng bán» trong nhóm Doanh thu & giá vốn.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (COGS posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận giá vốn hàng bán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận giá vốn hàng bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 61. Đặc tả Use Case "Doanh thu nhận trước"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_061 |
| **Tên Use Case** | Doanh thu nhận trước |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Doanh thu nhận trước" thuộc nhóm Doanh thu & giá vốn trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Deferred revenue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Doanh thu nhận trước» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Doanh thu nhận trước» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Doanh thu nhận trước» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Doanh thu nhận trước» trong nhóm Doanh thu & giá vốn.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Deferred revenue).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Doanh thu nhận trước».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Doanh thu nhận trước» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 62. Đặc tả Use Case "Chiết khấu làm giảm doanh thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_062 |
| **Tên Use Case** | Chiết khấu làm giảm doanh thu |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Chiết khấu làm giảm doanh thu" thuộc nhóm Doanh thu & giá vốn trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Revenue deductions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chiết khấu làm giảm doanh thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chiết khấu làm giảm doanh thu» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chiết khấu làm giảm doanh thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Chiết khấu làm giảm doanh thu» trong nhóm Doanh thu & giá vốn.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Revenue deductions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chiết khấu làm giảm doanh thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chiết khấu làm giảm doanh thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.10. Chi phí & phân bổ (`FIN-10`)

Nhóm **Chi phí & phân bổ** gồm **5** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 2 |

**Bảng 63. Đặc tả Use Case "Ghi nhận chi phí hoạt động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_063 |
| **Tên Use Case** | Ghi nhận chi phí hoạt động |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Ghi nhận chi phí hoạt động" thuộc nhóm Chi phí & phân bổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Operating expense entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận chi phí hoạt động» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận chi phí hoạt động» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận chi phí hoạt động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Ghi nhận chi phí hoạt động» trong nhóm Chi phí & phân bổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Operating expense entry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận chi phí hoạt động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận chi phí hoạt động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 64. Đặc tả Use Case "Phân bổ chi phí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_064 |
| **Tên Use Case** | Phân bổ chi phí |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Phân bổ chi phí" thuộc nhóm Chi phí & phân bổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cost allocation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân bổ chi phí» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân bổ chi phí» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân bổ chi phí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Phân bổ chi phí» trong nhóm Chi phí & phân bổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Cost allocation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân bổ chi phí».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân bổ chi phí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 65. Đặc tả Use Case "Chi phí lương từ HRM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_065 |
| **Tên Use Case** | Chi phí lương từ HRM |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Chi phí lương từ HRM" thuộc nhóm Chi phí & phân bổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Payroll cost posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chi phí lương từ HRM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chi phí lương từ HRM» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chi phí lương từ HRM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Chi phí lương từ HRM» trong nhóm Chi phí & phân bổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Payroll cost posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chi phí lương từ HRM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chi phí lương từ HRM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 66. Đặc tả Use Case "Chi phí marketing từ CRM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_066 |
| **Tên Use Case** | Chi phí marketing từ CRM |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Chi phí marketing từ CRM" thuộc nhóm Chi phí & phân bổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Marketing cost posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chi phí marketing từ CRM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chi phí marketing từ CRM» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chi phí marketing từ CRM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Chi phí marketing từ CRM» trong nhóm Chi phí & phân bổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Marketing cost posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chi phí marketing từ CRM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chi phí marketing từ CRM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 67. Đặc tả Use Case "Tạm ứng chi phí / quyết toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_067 |
| **Tên Use Case** | Tạm ứng chi phí / quyết toán |
| **Tác nhân** | GL Accountant |
| **Mô tả chức năng** | Cho phép GL Accountant thực hiện chức năng "Tạm ứng chi phí / quyết toán" thuộc nhóm Chi phí & phân bổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Expense claim & settlement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [GL Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạm ứng chi phí / quyết toán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạm ứng chi phí / quyết toán» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạm ứng chi phí / quyết toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. GL Accountant khởi tạo thao tác «Tạm ứng chi phí / quyết toán» trong nhóm Chi phí & phân bổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Expense claim & settlement).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tạm ứng chi phí / quyết toán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạm ứng chi phí / quyết toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.11. Kết chuyển & khóa sổ (`FIN-11`)

Nhóm **Kết chuyển & khóa sổ** gồm **4** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 3 |

**Bảng 68. Đặc tả Use Case "Kết chuyển lãi/lỗ cuối kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_068 |
| **Tên Use Case** | Kết chuyển lãi/lỗ cuối kỳ |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Kết chuyển lãi/lỗ cuối kỳ" thuộc nhóm Kết chuyển & khóa sổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Closing entries |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kết chuyển lãi/lỗ cuối kỳ» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kết chuyển lãi/lỗ cuối kỳ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kết chuyển lãi/lỗ cuối kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Kết chuyển lãi/lỗ cuối kỳ» trong nhóm Kết chuyển & khóa sổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Closing entries).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kết chuyển lãi/lỗ cuối kỳ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kết chuyển lãi/lỗ cuối kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 69. Đặc tả Use Case "Đối chiếu công nợ – sổ cái"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_069 |
| **Tên Use Case** | Đối chiếu công nợ – sổ cái |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Đối chiếu công nợ – sổ cái" thuộc nhóm Kết chuyển & khóa sổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Subledger reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối chiếu công nợ – sổ cái» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối chiếu công nợ – sổ cái» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối chiếu công nợ – sổ cái» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant khởi tạo thao tác «Đối chiếu công nợ – sổ cái» trong nhóm Kết chuyển & khóa sổ.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Subledger reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối chiếu công nợ – sổ cái».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối chiếu công nợ – sổ cái» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 70. Đặc tả Use Case "Checklist khóa sổ tháng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_070 |
| **Tên Use Case** | Checklist khóa sổ tháng |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Checklist khóa sổ tháng" thuộc nhóm Kết chuyển & khóa sổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Month-end checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist khóa sổ tháng» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-LOCK-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist khóa sổ tháng» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist khóa sổ tháng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. Chief Accountant chọn kỳ/ca/đối tượng cần khóa trong «Checklist khóa sổ tháng».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist khóa sổ tháng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 71. Đặc tả Use Case "Khóa sổ năm tài chính"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_071 |
| **Tên Use Case** | Khóa sổ năm tài chính |
| **Tác nhân** | Chief Accountant |
| **Mô tả chức năng** | Cho phép Chief Accountant thực hiện chức năng "Khóa sổ năm tài chính" thuộc nhóm Kết chuyển & khóa sổ trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Year-end close |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Chief Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa sổ năm tài chính» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-LOCK-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa sổ năm tài chính» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa sổ năm tài chính» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Chief Accountant chọn kỳ/ca/đối tượng cần khóa trong «Khóa sổ năm tài chính».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa sổ năm tài chính» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.12. Ngân sách tài chính (`FIN-12`)

Nhóm **Ngân sách tài chính** gồm **4** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 72. Đặc tả Use Case "Lập ngân sách theo kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_072 |
| **Tên Use Case** | Lập ngân sách theo kỳ |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Lập ngân sách theo kỳ" thuộc nhóm Ngân sách tài chính trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Budget planning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lập ngân sách theo kỳ» đã được cấu hình trong phạm vi data scope.<br>• Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lập ngân sách theo kỳ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lập ngân sách theo kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CFO khởi tạo thao tác «Lập ngân sách theo kỳ» trong nhóm Ngân sách tài chính.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Budget planning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lập ngân sách theo kỳ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lập ngân sách theo kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 73. Đặc tả Use Case "So sánh thực tế vs ngân sách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_073 |
| **Tên Use Case** | So sánh thực tế vs ngân sách |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "So sánh thực tế vs ngân sách" thuộc nhóm Ngân sách tài chính trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Budget vs actual |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh thực tế vs ngân sách» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh thực tế vs ngân sách» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh thực tế vs ngân sách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CFO mở «So sánh thực tế vs ngân sách» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Budget vs actual); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh thực tế vs ngân sách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 74. Đặc tả Use Case "Cảnh báo vượt ngân sách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_074 |
| **Tên Use Case** | Cảnh báo vượt ngân sách |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Cảnh báo vượt ngân sách" thuộc nhóm Ngân sách tài chính trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Budget alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo vượt ngân sách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo vượt ngân sách» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo vượt ngân sách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc CFO kích hoạt kiểm tra điều kiện «Cảnh báo vượt ngân sách».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Budget alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo vượt ngân sách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 75. Đặc tả Use Case "Phiên bản ngân sách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_075 |
| **Tên Use Case** | Phiên bản ngân sách |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Phiên bản ngân sách" thuộc nhóm Ngân sách tài chính trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Budget versioning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiên bản ngân sách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiên bản ngân sách» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiên bản ngân sách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CFO khởi tạo thao tác «Phiên bản ngân sách» trong nhóm Ngân sách tài chính.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Budget versioning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phiên bản ngân sách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiên bản ngân sách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

### 7.13. Báo cáo tài chính & quản trị (`FIN-13`)

Nhóm **Báo cáo tài chính & quản trị** gồm **8** use case của module `FIN`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 8 |

**Bảng 76. Đặc tả Use Case "Bảng cân đối phát sinh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_076 |
| **Tên Use Case** | Bảng cân đối phát sinh |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Bảng cân đối phát sinh" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Trial balance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng cân đối phát sinh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng cân đối phát sinh» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng cân đối phát sinh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO khởi tạo thao tác «Bảng cân đối phát sinh» trong nhóm Báo cáo tài chính & quản trị.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Trial balance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng cân đối phát sinh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng cân đối phát sinh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 77. Đặc tả Use Case "Báo cáo P&L quản trị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_077 |
| **Tên Use Case** | Báo cáo P&L quản trị |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Báo cáo P&L quản trị" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Income statement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo P&L quản trị» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo P&L quản trị» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo P&L quản trị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO mở «Báo cáo P&L quản trị» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Income statement); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo P&L quản trị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 78. Đặc tả Use Case "Bảng cân đối kế toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_078 |
| **Tên Use Case** | Bảng cân đối kế toán |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Bảng cân đối kế toán" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Balance sheet |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng cân đối kế toán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng cân đối kế toán» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng cân đối kế toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO khởi tạo thao tác «Bảng cân đối kế toán» trong nhóm Báo cáo tài chính & quản trị.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Balance sheet).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng cân đối kế toán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng cân đối kế toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 79. Đặc tả Use Case "Báo cáo lưu chuyển tiền tệ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_079 |
| **Tên Use Case** | Báo cáo lưu chuyển tiền tệ |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Báo cáo lưu chuyển tiền tệ" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Cash flow statement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo lưu chuyển tiền tệ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo lưu chuyển tiền tệ» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo lưu chuyển tiền tệ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO mở «Báo cáo lưu chuyển tiền tệ» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Cash flow statement); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo lưu chuyển tiền tệ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 80. Đặc tả Use Case "P&L theo chi nhánh / đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_080 |
| **Tên Use Case** | P&L theo chi nhánh / đơn vị |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "P&L theo chi nhánh / đơn vị" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Dimensional P&L |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «P&L theo chi nhánh / đơn vị» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «P&L theo chi nhánh / đơn vị» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «P&L theo chi nhánh / đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO khởi tạo thao tác «P&L theo chi nhánh / đơn vị» trong nhóm Báo cáo tài chính & quản trị.<br>2. Hệ thống kiểm tra license `FIN`, quyền RBAC và tiền điều kiện nghiệp vụ (Dimensional P&L).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «P&L theo chi nhánh / đơn vị».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «P&L theo chi nhánh / đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 81. Đặc tả Use Case "Báo cáo công nợ tổng hợp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_081 |
| **Tên Use Case** | Báo cáo công nợ tổng hợp |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Báo cáo công nợ tổng hợp" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: AR/AP summary |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo công nợ tổng hợp» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo công nợ tổng hợp» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo công nợ tổng hợp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO mở «Báo cáo công nợ tổng hợp» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (AR/AP summary); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo công nợ tổng hợp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 82. Đặc tả Use Case "Dashboard tài chính"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_082 |
| **Tên Use Case** | Dashboard tài chính |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Dashboard tài chính" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Financial dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard tài chính» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard tài chính» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard tài chính» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO mở «Dashboard tài chính» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Financial dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard tài chính» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

**Bảng 83. Đặc tả Use Case "Xuất báo cáo tài chính"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_FIN_083 |
| **Tên Use Case** | Xuất báo cáo tài chính |
| **Tác nhân** | CFO |
| **Mô tả chức năng** | Cho phép CFO thực hiện chức năng "Xuất báo cáo tài chính" thuộc nhóm Báo cáo tài chính & quản trị trong module FIN — Tài chính – Kế toán. Mô tả chi tiết: Export financial reports |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CFO] và được cấp quyền RBAC tương ứng.<br>• License module `FIN` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo tài chính» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-FIN-SCOPE-01`, `BR-FIN-AUD-01`, `BR-FIN-BALANCE-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo tài chính» được lưu nhất quán trong module `FIN`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo tài chính» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CFO mở «Xuất báo cáo tài chính», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo tài chính» (Export financial reports).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo tài chính» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa. |

---

## 8. Workflow end-to-end

### WF-FIN-01 — Khóa sổ tháng

**Mục tiêu:** Chốt số liệu kỳ

| Bước | Mô tả |
|---:|---|
| 1 | Đối chiếu AR/AP/kho/quỹ với sổ cái |
| 2 | Xử lý chênh lệch; hoàn tất bút toán điều chỉnh |
| 3 | Chạy kết chuyển cuối kỳ |
| 4 | Checklist month-end; khóa kỳ |
| 5 | Xuất P&L / BS / Cashflow quản trị |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-FIN-02 — Thu công nợ khách

**Mục tiêu:** Phân bổ thanh toán vào hóa đơn

| Bước | Mô tả |
|---:|---|
| 1 | Phát sinh AR từ đơn/HĐ/POS |
| 2 | Nhận tiền quỹ/NH |
| 3 | Allocate vào hóa đơn mở |
| 4 | Cập nhật aging; nhắc nợ nếu quá hạn |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Account / FiscalPeriod` | Danh mục kế toán |
| `JournalEntry / JournalLine` | Bút toán |
| `CashBook / BankAccount` | Quỹ–NH |
| `ArInvoice / ArReceipt` | Phải thu |
| `ApInvoice / ApPayment` | Phải trả |
| `TaxCode / EInvoice` | Thuế & HĐĐT |
| `Budget` | Ngân sách |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `FIN`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-FIN-01: Mọi bút toán phải cân Nợ=Có.
- BR-FIN-02: Không sửa trực tiếp chứng từ đã khóa kỳ; phải bút toán điều chỉnh/đảo.
- BR-FIN-03: Posting từ module nguồn phải idempotent (không nhân đôi).
- BR-FIN-04: Thanh toán AR/AP phải phân bổ hoặc ghi nhận unapplied rõ ràng.
- BR-FIN-05: Hạn mức duyệt chi theo workflow WF.
- BR-FIN-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-FIN-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-FIN-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Toàn vẹn | ACID cho post sổ cái |
| Hiệu năng | Trial balance kỳ hiện tại truy vấn chấp nhận được với khối lượng SMB/mid-market |
| Bảo mật | Tách quyền xem báo cáo lợi nhuận / chi tiết lương |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `FIN` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `fin.coa.manage` | Quyền chức năng module |
| `fin.journal.post` | Quyền chức năng module |
| `fin.ar.manage` | Quyền chức năng module |
| `fin.ap.manage` | Quyền chức năng module |
| `fin.cash.manage` | Quyền chức năng module |
| `fin.period.lock` | Quyền chức năng module |
| `fin.report.view` | Quyền chức năng module |
| `fin.budget.manage` | Quyền chức năng module |
| `fin.*.view` | Xem trong data scope |
| `fin.*.manage` | Tạo/sửa trong data scope |
| `fin.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| DSO/DPO | Theo dõi vận hành module |
| Cash position | Theo dõi vận hành module |
| Budget vs actual | Theo dõi vận hành module |
| AR overdue % | Theo dõi vận hành module |
| Month-end close days | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Chuẩn báo cáo quản trị ưu tiên; báo cáo thuế chi tiết có thể qua tích hợp.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Áp dụng chế độ kế toán nào làm mặc định template VN?
2. Multi-currency bắt buộc phase 1 hay phase sau?

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
| Bản SRS này | `SRS_FIN_v1.1.md` / `.docx` |
| UC IDs | `UC_FIN_001` … |

---

*Hết tài liệu SRS-FIN-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
