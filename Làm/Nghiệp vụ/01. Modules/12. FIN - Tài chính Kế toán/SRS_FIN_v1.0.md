# SRS-FIN-v1.0 — Tài chính – Kế toán

> Tài liệu đặc tả yêu cầu phần mềm (Software Requirements Specification) cho module ERP bán độc lập.
> Trạng thái: **Đề xuất / chờ duyệt nghiệp vụ**. Không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu & lịch sử thay đổi

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-FIN-v1.0` |
| Module | `FIN` — Tài chính – Kế toán |
| Phiên bản | 1.0 |
| Ngày lập | 03/08/2026 |
| Ngôn ngữ | Tiếng Việt |
| Phân loại | Nghiệp vụ / BA |
| Lớp sản phẩm | Tài chính |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | SYS |
| Khuyến nghị kèm | CRM, PUR, INV, POS, HRM, AST |
| Số nhóm chức năng | 13 |
| Số use case / chức năng | 83 |

| Phiên bản | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Solution | Sinh SRS từ danh mục chức năng generic v3 + meta nghiệp vụ | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích tài liệu
Tài liệu này mô tả đầy đủ yêu cầu nghiệp vụ và yêu cầu hệ thống của module **Tài chính – Kế toán**, làm cơ sở để thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai cấu trúc source code.

### 1.2. Tóm tắt module
Module FIN cung cấp COA, kỳ kế toán, sổ cái, quỹ–ngân hàng, AR/AP, thuế/HĐĐT (khung), ghi nhận doanh thu–chi phí từ module khác, ngân sách và báo cáo tài chính quản trị.

### 1.3. Mục tiêu nghiệp vụ
1. Single source of truth cho số liệu tài chính quản trị.
2. Đối soát công nợ phải thu/trả.
3. Khóa sổ kỳ có kiểm soát.
4. Nhận bút toán tự động từ các module vận hành.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / Ban giám đốc dự án
- Business Analyst, Solution Architect
- Trưởng nhóm Dev/QA
- Đội triển khai & Presales (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- COA/period, GL, cash/bank, AR, AP, e-invoice framework, tax, revenue/cost postings, budget, FIN reports.

### 2.2. Out of Scope
- Thay thế hoàn toàn phần mềm thuế chuyên sâu (có thể tích hợp).
- Hợp nhất báo cáo tập đoàn phức tạp đa chuẩn kế toán ngay phase 1.

### 2.3. Nguyên tắc đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`.
- **Khuyến nghị kèm** để có giá trị E2E: CRM, PUR, INV, POS, HRM, AST.
- Tính năng ngành (F&B, sản xuất rời rạc, phân phối…) cấu hình bằng template khi triển khai, không hard-code vào SRS gốc.

---

## 3. Tác nhân & stakeholder

| Tác nhân | Trách nhiệm chính |
|---|---|
| Chief Accountant | COA, kỳ, khóa sổ |
| GL Accountant | Bút toán / sổ cái |
| AR Accountant | Công nợ khách |
| AP Accountant | Công nợ NCC |
| Treasurer | Quỹ – ngân hàng |
| CFO / Manager | Báo cáo & ngân sách |

---

## 4. Thuật ngữ & viết tắt

| Thuật ngữ | Định nghĩa |
|---|---|
| COA | Chart of Accounts |
| GL | General Ledger |
| AR/AP | Accounts Receivable / Payable |
| Trial balance | Cân đối phát sinh |
| Posting | Hạch toán từ chứng từ nguồn |
| UC | Use Case / chức năng nguyên tử trong catalog |
| MoSCoW | Must / Should / Could / Won't (ưu tiên) |
| Data scope | Phạm vi dữ liệu theo tổ chức/kho/… do SYS kiểm soát |

---

## 5. Ngữ cảnh module & phụ thuộc

### 5.1. Vị trí trong kiến trúc sản phẩm
Module `FIN` thuộc lớp **Tài chính**. Mọi truy cập đi qua lớp nền `SYS` (xác thực, RBAC, license, audit, file, thông báo).

### 5.2. Phụ thuộc & tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM/POS | Doanh thu & thanh toán |
| Tích hợp | PUR/INV | AP & kho |
| Tích hợp | HRM | Chi phí lương |
| Tích hợp | AST | Khấu hao |
| Tích hợp | LOG | COD |
| Tích hợp | E-invoice provider | HĐĐT |
| Tích hợp | Bank | Sao kê / chi hộ |

### 5.3. Ràng buộc license
- API/UI của `FIN` chỉ mở khi license module active.
- Dataset BI liên quan module chỉ mở khi vừa có license `BI` vừa có license module nguồn.

---

## 6. Catalog chức năng (Module → Nhóm → UC)

**Tổng hợp:** 13 nhóm | 83 chức năng/use case.

| STT | Mã nhóm | Nhóm chức năng | Số UC |
|---:|---|---|---:|
| 1 | `FIN-01` | Danh mục kế toán | 9 |
| 2 | `FIN-02` | Sổ cái & bút toán | 8 |
| 3 | `FIN-03` | Quỹ tiền mặt | 6 |
| 4 | `FIN-04` | Ngân hàng | 6 |
| 5 | `FIN-05` | Công nợ phải thu (AR) | 9 |
| 6 | `FIN-06` | Công nợ phải trả (AP) | 8 |
| 7 | `FIN-07` | Hóa đơn điện tử | 5 |
| 8 | `FIN-08` | Thuế | 5 |
| 9 | `FIN-09` | Doanh thu & giá vốn | 6 |
| 10 | `FIN-10` | Chi phí & phân bổ | 5 |
| 11 | `FIN-11` | Kết chuyển & khóa sổ | 4 |
| 12 | `FIN-12` | Ngân sách tài chính | 4 |
| 13 | `FIN-13` | Báo cáo tài chính & quản trị | 8 |

<details>
<summary>Bảng đầy đủ mã UC (bấm để mở)</summary>

| Mã UC | Nhóm | Tên chức năng | Ưu tiên | MoSCoW |
|---|---|---|---|---|
| `UC_FIN_001` | Danh mục kế toán | Hệ thống tài khoản (COA) | Bắt buộc | Must |
| `UC_FIN_002` | Danh mục kế toán | Nhóm tài khoản / chỉ tiêu | Bắt buộc | Must |
| `UC_FIN_003` | Danh mục kế toán | Kỳ kế toán / năm tài chính | Bắt buộc | Must |
| `UC_FIN_004` | Danh mục kế toán | Khóa sổ kỳ / mở lại | Bắt buộc | Must |
| `UC_FIN_005` | Danh mục kế toán | Đồng tiền hạch toán & tỷ giá | Cao | Should |
| `UC_FIN_006` | Danh mục kế toán | Trung tâm chi phí | Bắt buộc | Must |
| `UC_FIN_007` | Danh mục kế toán | Khoản mục thu/chi | Cao | Should |
| `UC_FIN_008` | Danh mục kế toán | Hình thức thanh toán | Bắt buộc | Must |
| `UC_FIN_009` | Danh mục kế toán | Danh mục thuế | Bắt buộc | Must |
| `UC_FIN_010` | Sổ cái & bút toán | Tạo bút toán thủ công | Bắt buộc | Must |
| `UC_FIN_011` | Sổ cái & bút toán | Bút toán định kỳ / mẫu | Cao | Should |
| `UC_FIN_012` | Sổ cái & bút toán | Đảo bút toán | Bắt buộc | Must |
| `UC_FIN_013` | Sổ cái & bút toán | Xem sổ cái theo tài khoản | Bắt buộc | Must |
| `UC_FIN_014` | Sổ cái & bút toán | Sổ chi tiết theo đối tượng | Bắt buộc | Must |
| `UC_FIN_015` | Sổ cái & bút toán | Nhận bút toán tự động | Bắt buộc | Must |
| `UC_FIN_016` | Sổ cái & bút toán | Kiểm soát bút toán lệch Nợ/Có | Bắt buộc | Must |
| `UC_FIN_017` | Sổ cái & bút toán | Đính kèm chứng từ gốc | Cao | Should |
| `UC_FIN_018` | Quỹ tiền mặt | Danh mục quỹ / thủ quỹ | Bắt buộc | Must |
| `UC_FIN_019` | Quỹ tiền mặt | Phiếu thu tiền mặt | Bắt buộc | Must |
| `UC_FIN_020` | Quỹ tiền mặt | Phiếu chi tiền mặt | Bắt buộc | Must |
| `UC_FIN_021` | Quỹ tiền mặt | Đề nghị tạm ứng / hoàn ứng | Cao | Should |
| `UC_FIN_022` | Quỹ tiền mặt | Kiểm kê quỹ | Cao | Should |
| `UC_FIN_023` | Quỹ tiền mặt | Báo cáo sổ quỹ | Bắt buộc | Must |
| `UC_FIN_024` | Ngân hàng | Danh mục tài khoản ngân hàng | Bắt buộc | Must |
| `UC_FIN_025` | Ngân hàng | Giấy báo Nợ / Có | Bắt buộc | Must |
| `UC_FIN_026` | Ngân hàng | Đối soát sao kê ngân hàng | Bắt buộc | Must |
| `UC_FIN_027` | Ngân hàng | Đề nghị chuyển khoản | Bắt buộc | Must |
| `UC_FIN_028` | Ngân hàng | Import sao kê | Cao | Should |
| `UC_FIN_029` | Ngân hàng | Theo dõi số dư ngân hàng | Bắt buộc | Must |
| `UC_FIN_030` | Công nợ phải thu (AR) | Tạo hóa đơn phải thu | Bắt buộc | Must |
| `UC_FIN_031` | Công nợ phải thu (AR) | Công nợ theo khách / hóa đơn | Bắt buộc | Must |
| `UC_FIN_032` | Công nợ phải thu (AR) | Thu tiền & phân bổ | Bắt buộc | Must |
| `UC_FIN_033` | Công nợ phải thu (AR) | Bù trừ công nợ | Cao | Should |
| `UC_FIN_034` | Công nợ phải thu (AR) | Nhắc nợ tự động | Cao | Should |
| `UC_FIN_035` | Công nợ phải thu (AR) | Cảnh báo vượt hạn mức | Bắt buộc | Must |
| `UC_FIN_036` | Công nợ phải thu (AR) | Bảng tuổi nợ phải thu | Bắt buộc | Must |
| `UC_FIN_037` | Công nợ phải thu (AR) | Xử lý nợ khó đòi | Trung bình | Could |
| `UC_FIN_038` | Công nợ phải thu (AR) | Đối soát COD về AR | Cao | Should |
| `UC_FIN_039` | Công nợ phải trả (AP) | Tạo hóa đơn phải trả | Bắt buộc | Must |
| `UC_FIN_040` | Công nợ phải trả (AP) | Công nợ theo nhà cung cấp | Bắt buộc | Must |
| `UC_FIN_041` | Công nợ phải trả (AP) | Đề nghị thanh toán | Bắt buộc | Must |
| `UC_FIN_042` | Công nợ phải trả (AP) | Duyệt chi trả | Bắt buộc | Must |
| `UC_FIN_043` | Công nợ phải trả (AP) | Thanh toán & phân bổ AP | Bắt buộc | Must |
| `UC_FIN_044` | Công nợ phải trả (AP) | Bảng tuổi nợ phải trả | Bắt buộc | Must |
| `UC_FIN_045` | Công nợ phải trả (AP) | Tạm ứng nhà cung cấp | Cao | Should |
| `UC_FIN_046` | Công nợ phải trả (AP) | Đối soát 3 chiều | Cao | Should |
| `UC_FIN_047` | Hóa đơn điện tử | Cấu hình nhà cung cấp HĐĐT | Cao | Should |
| `UC_FIN_048` | Hóa đơn điện tử | Phát hành hóa đơn điện tử | Cao | Should |
| `UC_FIN_049` | Hóa đơn điện tử | Điều chỉnh / thay thế / hủy | Cao | Should |
| `UC_FIN_050` | Hóa đơn điện tử | Tra cứu trạng thái phát hành | Cao | Should |
| `UC_FIN_051` | Hóa đơn điện tử | Lưu trữ bảng kê HĐĐT | Cao | Should |
| `UC_FIN_052` | Thuế | Tính thuế GTGT đầu ra / đầu vào | Bắt buộc | Must |
| `UC_FIN_053` | Thuế | Bảng kê hóa đơn GTGT | Bắt buộc | Must |
| `UC_FIN_054` | Thuế | Tờ khai thuế GTGT | Cao | Should |
| `UC_FIN_055` | Thuế | Thuế TNCN từ lương | Cao | Should |
| `UC_FIN_056` | Thuế | Cấu hình thuế suất | Bắt buộc | Must |
| `UC_FIN_057` | Doanh thu & giá vốn | Ghi nhận doanh thu từ POS | Bắt buộc | Must |
| `UC_FIN_058` | Doanh thu & giá vốn | Ghi nhận doanh thu từ đơn | Bắt buộc | Must |
| `UC_FIN_059` | Doanh thu & giá vốn | Ghi nhận doanh thu dự án | Cao | Should |
| `UC_FIN_060` | Doanh thu & giá vốn | Ghi nhận giá vốn hàng bán | Bắt buộc | Must |
| `UC_FIN_061` | Doanh thu & giá vốn | Doanh thu nhận trước | Trung bình | Could |
| `UC_FIN_062` | Doanh thu & giá vốn | Chiết khấu làm giảm doanh thu | Bắt buộc | Must |
| `UC_FIN_063` | Chi phí & phân bổ | Ghi nhận chi phí hoạt động | Bắt buộc | Must |
| `UC_FIN_064` | Chi phí & phân bổ | Phân bổ chi phí | Cao | Should |
| `UC_FIN_065` | Chi phí & phân bổ | Chi phí lương từ HRM | Bắt buộc | Must |
| `UC_FIN_066` | Chi phí & phân bổ | Chi phí marketing từ CRM | Cao | Should |
| `UC_FIN_067` | Chi phí & phân bổ | Tạm ứng chi phí / quyết toán | Cao | Should |
| `UC_FIN_068` | Kết chuyển & khóa sổ | Kết chuyển lãi/lỗ cuối kỳ | Bắt buộc | Must |
| `UC_FIN_069` | Kết chuyển & khóa sổ | Đối chiếu công nợ – sổ cái | Bắt buộc | Must |
| `UC_FIN_070` | Kết chuyển & khóa sổ | Checklist khóa sổ tháng | Cao | Should |
| `UC_FIN_071` | Kết chuyển & khóa sổ | Khóa sổ năm tài chính | Bắt buộc | Must |
| `UC_FIN_072` | Ngân sách tài chính | Lập ngân sách theo kỳ | Cao | Should |
| `UC_FIN_073` | Ngân sách tài chính | So sánh thực tế vs ngân sách | Cao | Should |
| `UC_FIN_074` | Ngân sách tài chính | Cảnh báo vượt ngân sách | Cao | Should |
| `UC_FIN_075` | Ngân sách tài chính | Phiên bản ngân sách | Trung bình | Could |
| `UC_FIN_076` | Báo cáo tài chính & quản trị | Bảng cân đối phát sinh | Bắt buộc | Must |
| `UC_FIN_077` | Báo cáo tài chính & quản trị | Báo cáo P&L quản trị | Bắt buộc | Must |
| `UC_FIN_078` | Báo cáo tài chính & quản trị | Bảng cân đối kế toán | Bắt buộc | Must |
| `UC_FIN_079` | Báo cáo tài chính & quản trị | Báo cáo lưu chuyển tiền tệ | Bắt buộc | Must |
| `UC_FIN_080` | Báo cáo tài chính & quản trị | P&L theo chi nhánh / đơn vị | Bắt buộc | Must |
| `UC_FIN_081` | Báo cáo tài chính & quản trị | Báo cáo công nợ tổng hợp | Bắt buộc | Must |
| `UC_FIN_082` | Báo cáo tài chính & quản trị | Dashboard tài chính | Bắt buộc | Must |
| `UC_FIN_083` | Báo cáo tài chính & quản trị | Xuất báo cáo tài chính | Bắt buộc | Must |

</details>

---

## 7. Đặc tả chức năng theo nhóm

Mỗi UC bên dưới gồm: mô tả, tác nhân, tiền/hậu điều kiện, luồng chính, quy tắc, tiêu chí chấp nhận và ưu tiên. Đây là mức đặc tả BA để chốt phạm vi; chi tiết UI/API sẽ bổ sung ở giai đoạn thiết kế.

### 7.1. Danh mục kế toán (`FIN-01`)

Nhóm này gồm **9** chức năng. Tác nhân mặc định: **Chief Accountant**.

#### UC_FIN_001 — Hệ thống tài khoản (COA)

- **Mô tả:** Chart of accounts
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hệ thống tài khoản (COA)
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Chart of accounts)
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
  - AC1: Thực hiện thành công thao tác “Hệ thống tài khoản (COA)” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_002 — Nhóm tài khoản / chỉ tiêu

- **Mô tả:** Account groups
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhóm tài khoản / chỉ tiêu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Account groups)
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
  - AC1: Thực hiện thành công thao tác “Nhóm tài khoản / chỉ tiêu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_003 — Kỳ kế toán / năm tài chính

- **Mô tả:** Fiscal calendar
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Kỳ kế toán / năm tài chính
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Fiscal calendar)
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
  - AC1: Thực hiện thành công thao tác “Kỳ kế toán / năm tài chính” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_004 — Khóa sổ kỳ / mở lại

- **Mô tả:** Period lock/unlock
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khóa sổ kỳ / mở lại
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Period lock/unlock)
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
  - AC1: Thực hiện thành công thao tác “Khóa sổ kỳ / mở lại” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_005 — Đồng tiền hạch toán & tỷ giá

- **Mô tả:** Currency & exchange rates
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đồng tiền hạch toán & tỷ giá
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Currency & exchange rates)
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
  - AC1: Thực hiện thành công thao tác “Đồng tiền hạch toán & tỷ giá” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_006 — Trung tâm chi phí

- **Mô tả:** Cost center master
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Trung tâm chi phí
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cost center master)
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
  - AC1: Thực hiện thành công thao tác “Trung tâm chi phí” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_007 — Khoản mục thu/chi

- **Mô tả:** Cash flow line items
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khoản mục thu/chi
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash flow line items)
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
  - AC1: Thực hiện thành công thao tác “Khoản mục thu/chi” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_008 — Hình thức thanh toán

- **Mô tả:** Payment methods
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hình thức thanh toán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payment methods)
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
  - AC1: Thực hiện thành công thao tác “Hình thức thanh toán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_009 — Danh mục thuế

- **Mô tả:** Tax code master
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục thuế” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.2. Sổ cái & bút toán (`FIN-02`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **GL Accountant**.

#### UC_FIN_010 — Tạo bút toán thủ công

- **Mô tả:** Manual journal entry
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo bút toán thủ công” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_011 — Bút toán định kỳ / mẫu

- **Mô tả:** Recurring journal
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bút toán định kỳ / mẫu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Recurring journal)
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
  - AC1: Thực hiện thành công thao tác “Bút toán định kỳ / mẫu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_012 — Đảo bút toán

- **Mô tả:** Journal reversal
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đảo bút toán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Journal reversal)
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
  - AC1: Thực hiện thành công thao tác “Đảo bút toán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_013 — Xem sổ cái theo tài khoản

- **Mô tả:** GL inquiry
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem sổ cái theo tài khoản
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (GL inquiry)
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
  - AC1: Thực hiện thành công thao tác “Xem sổ cái theo tài khoản” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_014 — Sổ chi tiết theo đối tượng

- **Mô tả:** Subledger inquiry
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Sổ chi tiết theo đối tượng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Subledger inquiry)
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
  - AC1: Thực hiện thành công thao tác “Sổ chi tiết theo đối tượng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_015 — Nhận bút toán tự động

- **Mô tả:** Auto posting from modules
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhận bút toán tự động
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Auto posting from modules)
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
  - AC1: Thực hiện thành công thao tác “Nhận bút toán tự động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_016 — Kiểm soát bút toán lệch Nợ/Có

- **Mô tả:** Debit/credit balance check
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Kiểm soát bút toán lệch Nợ/Có
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Debit/credit balance check)
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
  - AC1: Thực hiện thành công thao tác “Kiểm soát bút toán lệch Nợ/Có” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_017 — Đính kèm chứng từ gốc

- **Mô tả:** Voucher attachment
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đính kèm chứng từ gốc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Voucher attachment)
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
  - AC1: Thực hiện thành công thao tác “Đính kèm chứng từ gốc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.3. Quỹ tiền mặt (`FIN-03`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **Treasurer**.

#### UC_FIN_018 — Danh mục quỹ / thủ quỹ

- **Mô tả:** Cash books
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục quỹ / thủ quỹ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_019 — Phiếu thu tiền mặt

- **Mô tả:** Cash receipt
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phiếu thu tiền mặt
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash receipt)
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
  - AC1: Thực hiện thành công thao tác “Phiếu thu tiền mặt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_020 — Phiếu chi tiền mặt

- **Mô tả:** Cash payment
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phiếu chi tiền mặt
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
  - AC1: Thực hiện thành công thao tác “Phiếu chi tiền mặt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_021 — Đề nghị tạm ứng / hoàn ứng

- **Mô tả:** Advance & settlement
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đề nghị tạm ứng / hoàn ứng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Advance & settlement)
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
  - AC1: Thực hiện thành công thao tác “Đề nghị tạm ứng / hoàn ứng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_022 — Kiểm kê quỹ

- **Mô tả:** Cash count
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Kiểm kê quỹ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash count)
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
  - AC1: Thực hiện thành công thao tác “Kiểm kê quỹ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_023 — Báo cáo sổ quỹ

- **Mô tả:** Cash book report
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo sổ quỹ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.4. Ngân hàng (`FIN-04`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **Treasurer**.

#### UC_FIN_024 — Danh mục tài khoản ngân hàng

- **Mô tả:** Bank account master
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục tài khoản ngân hàng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_025 — Giấy báo Nợ / Có

- **Mô tả:** Bank voucher
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Giấy báo Nợ / Có
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bank voucher)
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
  - AC1: Thực hiện thành công thao tác “Giấy báo Nợ / Có” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_026 — Đối soát sao kê ngân hàng

- **Mô tả:** Bank reconciliation
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đối soát sao kê ngân hàng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bank reconciliation)
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
  - AC1: Thực hiện thành công thao tác “Đối soát sao kê ngân hàng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_027 — Đề nghị chuyển khoản

- **Mô tả:** Payment order
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đề nghị chuyển khoản
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payment order)
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
  - AC1: Thực hiện thành công thao tác “Đề nghị chuyển khoản” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_028 — Import sao kê

- **Mô tả:** Bank statement import
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Import sao kê
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bank statement import)
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
  - AC1: Thực hiện thành công thao tác “Import sao kê” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_029 — Theo dõi số dư ngân hàng

- **Mô tả:** Bank balance tracking
- **Tác nhân chính:** Treasurer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Theo dõi số dư ngân hàng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bank balance tracking)
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
  - AC1: Thực hiện thành công thao tác “Theo dõi số dư ngân hàng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.5. Công nợ phải thu (AR) (`FIN-05`)

Nhóm này gồm **9** chức năng. Tác nhân mặc định: **AR Accountant**.

#### UC_FIN_030 — Tạo hóa đơn phải thu

- **Mô tả:** AR invoice
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo hóa đơn phải thu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_031 — Công nợ theo khách / hóa đơn

- **Mô tả:** AR open items
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Công nợ theo khách / hóa đơn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (AR open items)
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
  - AC1: Thực hiện thành công thao tác “Công nợ theo khách / hóa đơn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_032 — Thu tiền & phân bổ

- **Mô tả:** Cash application
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thu tiền & phân bổ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cash application)
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
  - AC1: Thực hiện thành công thao tác “Thu tiền & phân bổ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_033 — Bù trừ công nợ

- **Mô tả:** AR offset
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bù trừ công nợ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (AR offset)
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
  - AC1: Thực hiện thành công thao tác “Bù trừ công nợ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_034 — Nhắc nợ tự động

- **Mô tả:** Dunning
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhắc nợ tự động
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Dunning)
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
  - AC1: Thực hiện thành công thao tác “Nhắc nợ tự động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_035 — Cảnh báo vượt hạn mức

- **Mô tả:** Credit limit alert
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo vượt hạn mức
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Credit limit alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo vượt hạn mức” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_036 — Bảng tuổi nợ phải thu

- **Mô tả:** AR aging report
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng tuổi nợ phải thu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (AR aging report)
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
  - AC1: Thực hiện thành công thao tác “Bảng tuổi nợ phải thu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_037 — Xử lý nợ khó đòi

- **Mô tả:** Bad debt provision
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xử lý nợ khó đòi
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bad debt provision)
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
  - AC1: Thực hiện thành công thao tác “Xử lý nợ khó đòi” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_038 — Đối soát COD về AR

- **Mô tả:** COD to AR reconciliation
- **Tác nhân chính:** AR Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đối soát COD về AR
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (COD to AR reconciliation)
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
  - AC1: Thực hiện thành công thao tác “Đối soát COD về AR” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.6. Công nợ phải trả (AP) (`FIN-06`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **AP Accountant**.

#### UC_FIN_039 — Tạo hóa đơn phải trả

- **Mô tả:** AP invoice
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo hóa đơn phải trả” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_040 — Công nợ theo nhà cung cấp

- **Mô tả:** AP open items
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Công nợ theo nhà cung cấp
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (AP open items)
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
  - AC1: Thực hiện thành công thao tác “Công nợ theo nhà cung cấp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_041 — Đề nghị thanh toán

- **Mô tả:** Payment proposal
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đề nghị thanh toán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payment proposal)
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
  - AC1: Thực hiện thành công thao tác “Đề nghị thanh toán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_042 — Duyệt chi trả

- **Mô tả:** Payment approval
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt chi trả” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_043 — Thanh toán & phân bổ AP

- **Mô tả:** AP payment application
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thanh toán & phân bổ AP
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (AP payment application)
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
  - AC1: Thực hiện thành công thao tác “Thanh toán & phân bổ AP” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_044 — Bảng tuổi nợ phải trả

- **Mô tả:** AP aging report
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng tuổi nợ phải trả
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (AP aging report)
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
  - AC1: Thực hiện thành công thao tác “Bảng tuổi nợ phải trả” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_045 — Tạm ứng nhà cung cấp

- **Mô tả:** Vendor prepayment
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tạm ứng nhà cung cấp
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Vendor prepayment)
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
  - AC1: Thực hiện thành công thao tác “Tạm ứng nhà cung cấp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_046 — Đối soát 3 chiều

- **Mô tả:** 3-way match from FIN view
- **Tác nhân chính:** AP Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đối soát 3 chiều
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (3-way match from FIN view)
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
  - AC1: Thực hiện thành công thao tác “Đối soát 3 chiều” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.7. Hóa đơn điện tử (`FIN-07`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **GL Accountant**.

#### UC_FIN_047 — Cấu hình nhà cung cấp HĐĐT

- **Mô tả:** E-invoice provider setup
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình nhà cung cấp HĐĐT” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_048 — Phát hành hóa đơn điện tử

- **Mô tả:** Issue e-invoice
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phát hành hóa đơn điện tử
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Issue e-invoice)
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
  - AC1: Thực hiện thành công thao tác “Phát hành hóa đơn điện tử” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_049 — Điều chỉnh / thay thế / hủy

- **Mô tả:** E-invoice adjustment
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Điều chỉnh / thay thế / hủy
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (E-invoice adjustment)
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
  - AC1: Thực hiện thành công thao tác “Điều chỉnh / thay thế / hủy” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_050 — Tra cứu trạng thái phát hành

- **Mô tả:** E-invoice status
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tra cứu trạng thái phát hành
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (E-invoice status)
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
  - AC1: Thực hiện thành công thao tác “Tra cứu trạng thái phát hành” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_051 — Lưu trữ bảng kê HĐĐT

- **Mô tả:** E-invoice registry
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lưu trữ bảng kê HĐĐT
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (E-invoice registry)
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
  - AC1: Thực hiện thành công thao tác “Lưu trữ bảng kê HĐĐT” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.8. Thuế (`FIN-08`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **GL Accountant**.

#### UC_FIN_052 — Tính thuế GTGT đầu ra / đầu vào

- **Mô tả:** VAT calculation
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tính thuế GTGT đầu ra / đầu vào
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (VAT calculation)
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
  - AC1: Thực hiện thành công thao tác “Tính thuế GTGT đầu ra / đầu vào” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_053 — Bảng kê hóa đơn GTGT

- **Mô tả:** VAT listing
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng kê hóa đơn GTGT
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (VAT listing)
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
  - AC1: Thực hiện thành công thao tác “Bảng kê hóa đơn GTGT” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_054 — Tờ khai thuế GTGT

- **Mô tả:** VAT return preparation
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tờ khai thuế GTGT
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (VAT return preparation)
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
  - AC1: Thực hiện thành công thao tác “Tờ khai thuế GTGT” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_055 — Thuế TNCN từ lương

- **Mô tả:** Personal income tax from payroll
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thuế TNCN từ lương
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Personal income tax from payroll)
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
  - AC1: Thực hiện thành công thao tác “Thuế TNCN từ lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_056 — Cấu hình thuế suất

- **Mô tả:** Tax rate configuration
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thuế suất” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.9. Doanh thu & giá vốn (`FIN-09`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **GL Accountant**.

#### UC_FIN_057 — Ghi nhận doanh thu từ POS

- **Mô tả:** POS revenue posting
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận doanh thu từ POS
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (POS revenue posting)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận doanh thu từ POS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_058 — Ghi nhận doanh thu từ đơn

- **Mô tả:** Order revenue posting
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận doanh thu từ đơn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Order revenue posting)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận doanh thu từ đơn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_059 — Ghi nhận doanh thu dự án

- **Mô tả:** Project revenue recognition
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận doanh thu dự án
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Project revenue recognition)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận doanh thu dự án” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_060 — Ghi nhận giá vốn hàng bán

- **Mô tả:** COGS posting
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận giá vốn hàng bán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (COGS posting)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận giá vốn hàng bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_061 — Doanh thu nhận trước

- **Mô tả:** Deferred revenue
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Doanh thu nhận trước
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Deferred revenue)
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
  - AC1: Thực hiện thành công thao tác “Doanh thu nhận trước” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_062 — Chiết khấu làm giảm doanh thu

- **Mô tả:** Revenue deductions
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chiết khấu làm giảm doanh thu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Revenue deductions)
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
  - AC1: Thực hiện thành công thao tác “Chiết khấu làm giảm doanh thu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.10. Chi phí & phân bổ (`FIN-10`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **GL Accountant**.

#### UC_FIN_063 — Ghi nhận chi phí hoạt động

- **Mô tả:** Operating expense entry
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận chi phí hoạt động
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Operating expense entry)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận chi phí hoạt động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_064 — Phân bổ chi phí

- **Mô tả:** Cost allocation
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân bổ chi phí
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cost allocation)
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
  - AC1: Thực hiện thành công thao tác “Phân bổ chi phí” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_065 — Chi phí lương từ HRM

- **Mô tả:** Payroll cost posting
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chi phí lương từ HRM
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payroll cost posting)
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
  - AC1: Thực hiện thành công thao tác “Chi phí lương từ HRM” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_066 — Chi phí marketing từ CRM

- **Mô tả:** Marketing cost posting
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chi phí marketing từ CRM
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Marketing cost posting)
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
  - AC1: Thực hiện thành công thao tác “Chi phí marketing từ CRM” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_067 — Tạm ứng chi phí / quyết toán

- **Mô tả:** Expense claim & settlement
- **Tác nhân chính:** GL Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tạm ứng chi phí / quyết toán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Expense claim & settlement)
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
  - AC1: Thực hiện thành công thao tác “Tạm ứng chi phí / quyết toán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.11. Kết chuyển & khóa sổ (`FIN-11`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **Chief Accountant**.

#### UC_FIN_068 — Kết chuyển lãi/lỗ cuối kỳ

- **Mô tả:** Closing entries
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Kết chuyển lãi/lỗ cuối kỳ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Closing entries)
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
  - AC1: Thực hiện thành công thao tác “Kết chuyển lãi/lỗ cuối kỳ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_069 — Đối chiếu công nợ – sổ cái

- **Mô tả:** Subledger reconciliation
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đối chiếu công nợ – sổ cái
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Subledger reconciliation)
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
  - AC1: Thực hiện thành công thao tác “Đối chiếu công nợ – sổ cái” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_070 — Checklist khóa sổ tháng

- **Mô tả:** Month-end checklist
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist khóa sổ tháng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Month-end checklist)
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
  - AC1: Thực hiện thành công thao tác “Checklist khóa sổ tháng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_071 — Khóa sổ năm tài chính

- **Mô tả:** Year-end close
- **Tác nhân chính:** Chief Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khóa sổ năm tài chính
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Year-end close)
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
  - AC1: Thực hiện thành công thao tác “Khóa sổ năm tài chính” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.12. Ngân sách tài chính (`FIN-12`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **CFO / Manager**.

#### UC_FIN_072 — Lập ngân sách theo kỳ

- **Mô tả:** Budget planning
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lập ngân sách theo kỳ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget planning)
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
  - AC1: Thực hiện thành công thao tác “Lập ngân sách theo kỳ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_073 — So sánh thực tế vs ngân sách

- **Mô tả:** Budget vs actual
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: So sánh thực tế vs ngân sách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget vs actual)
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
  - AC1: Thực hiện thành công thao tác “So sánh thực tế vs ngân sách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_074 — Cảnh báo vượt ngân sách

- **Mô tả:** Budget alert
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo vượt ngân sách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo vượt ngân sách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FIN_075 — Phiên bản ngân sách

- **Mô tả:** Budget versioning
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phiên bản ngân sách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Budget versioning)
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
  - AC1: Thực hiện thành công thao tác “Phiên bản ngân sách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.13. Báo cáo tài chính & quản trị (`FIN-13`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **CFO / Manager**.

#### UC_FIN_076 — Bảng cân đối phát sinh

- **Mô tả:** Trial balance
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng cân đối phát sinh
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Trial balance)
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
  - AC1: Thực hiện thành công thao tác “Bảng cân đối phát sinh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_077 — Báo cáo P&L quản trị

- **Mô tả:** Income statement
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo P&L quản trị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_078 — Bảng cân đối kế toán

- **Mô tả:** Balance sheet
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng cân đối kế toán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Balance sheet)
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
  - AC1: Thực hiện thành công thao tác “Bảng cân đối kế toán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_079 — Báo cáo lưu chuyển tiền tệ

- **Mô tả:** Cash flow statement
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo lưu chuyển tiền tệ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_080 — P&L theo chi nhánh / đơn vị

- **Mô tả:** Dimensional P&L
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: P&L theo chi nhánh / đơn vị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Dimensional P&L)
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
  - AC1: Thực hiện thành công thao tác “P&L theo chi nhánh / đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_081 — Báo cáo công nợ tổng hợp

- **Mô tả:** AR/AP summary
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo công nợ tổng hợp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_082 — Dashboard tài chính

- **Mô tả:** Financial dashboard
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Dashboard tài chính” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FIN_083 — Xuất báo cáo tài chính

- **Mô tả:** Export financial reports
- **Tác nhân chính:** CFO / Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FIN`.
  - License module `FIN` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất báo cáo tài chính” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

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

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

### WF-FIN-02 — Thu công nợ khách

**Mục tiêu:** Phân bổ thanh toán vào hóa đơn

| Bước | Mô tả |
|---:|---|
| 1 | Phát sinh AR từ đơn/HĐ/POS |
| 2 | Nhận tiền quỹ/NH |
| 3 | Allocate vào hóa đơn mở |
| 4 | Cập nhật aging; nhắc nợ nếu quá hạn |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

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

### 9.1. Xuất xứ & kiểm soát dữ liệu
- Master dùng chung (KH, SP, chi nhánh…) tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ nghiệp vụ có trạng thái vòng đời rõ ràng (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete hoặc trạng thái ngưng dùng là mặc định; hạn chế xóa cứng.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-FIN-01: Mọi bút toán phải cân Nợ=Có.
- BR-FIN-02: Không sửa trực tiếp chứng từ đã khóa kỳ; phải bút toán điều chỉnh/đảo.
- BR-FIN-03: Posting từ module nguồn phải idempotent (không nhân đôi).
- BR-FIN-04: Thanh toán AR/AP phải phân bổ hoặc ghi nhận unapplied rõ ràng.
- BR-FIN-05: Hạn mức duyệt chi theo workflow WF.
- BR-FIN-GEN-01: Mọi thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-FIN-GEN-02: Mọi chứng từ có mã duy nhất theo rule Sequence của SYS.
- BR-FIN-GEN-03: Thao tác sau khi khóa kỳ/chốt sổ (nếu có) phải đi đường điều chỉnh có kiểm soát.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Toàn vẹn | ACID cho post sổ cái |
| Hiệu năng | Trial balance kỳ hiện tại truy vấn chấp nhận được với khối lượng SMB/mid-market |
| Bảo mật | Tách quyền xem báo cáo lợi nhuận / chi tiết lương |
| Usability | Form có validate rõ; bảng có lọc/phân trang; hỗ trợ tiếng Việt |
| Reliability | Không mất chứng từ đã post; giao dịch quan trọng atomic |
| Maintainability | Permission và cấu hình không hard-code trong source nghiệp vụ |
| Observability | Có log ứng dụng + audit nghiệp vụ tách bạch |

---

## 12. Tích hợp & sự kiện

### 12.1. Ma trận tích hợp

| Thành phần | Mô tả |
|---|---|
| CRM/POS | Doanh thu & thanh toán |
| PUR/INV | AP & kho |
| HRM | Chi phí lương |
| AST | Khấu hao |
| LOG | COD |
| E-invoice provider | HĐĐT |
| Bank | Sao kê / chi hộ |

### 12.2. Sự kiện (logical)
- `FIN.EntityCreated` / `FIN.EntityUpdated` / `FIN.EntityStatusChanged`
- `FIN.DocumentSubmitted` / `FIN.DocumentApproved` / `FIN.DocumentPosted`
- Mapping cụ thể API/topic sẽ định nghĩa ở tài liệu Interface Spec sau khi chốt SRS.

---

## 13. Phân quyền & bảo mật

### 13.1. Permission catalog (đề xuất)

- `fin.coa.manage`
- `fin.journal.post`
- `fin.ar.manage`
- `fin.ap.manage`
- `fin.cash.manage`
- `fin.period.lock`
- `fin.report.view`
- `fin.budget.manage`

### 13.2. Nguyên tắc
- Deny by default; chỉ mở theo role.
- Data scope theo chi nhánh/kho/đơn vị do SYS quyết định.
- Field-level security cho dữ liệu nhạy cảm (lương, công nợ chi tiết, giá vốn…) khi áp dụng.
- Mọi thay đổi phân quyền và thao tác critical ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| DSO/DPO | Giám sát vận hành module `FIN` |
| Cash position | Giám sát vận hành module `FIN` |
| Budget vs actual | Giám sát vận hành module `FIN` |
| AR overdue % | Giám sát vận hành module `FIN` |
| Month-end close days | Giám sát vận hành module `FIN` |

Báo cáo chi tiết vận hành nằm trong từng nhóm “Báo cáo…” của Mục 7; tổng hợp điều hành nằm trên module `BI` khi khách mua thêm.

---

## 15. Giả định, rủi ro & câu hỏi mở

### 15.1. Giả định
- Chuẩn báo cáo quản trị ưu tiên; báo cáo thuế chi tiết có thể qua tích hợp.

### 15.2. Câu hỏi mở cần chốt
- Áp dụng chế độ kế toán nào làm mặc định template VN?
- Multi-currency bắt buộc phase 1 hay phase sau?

### 15.3. Rủi ro
- Phụ thuộc module khác chưa mua → một số workflow E2E chỉ chạy được một phần (cần nêu rõ khi bán gói).
- Cấu hình quá linh hoạt có thể làm tăng effort QA; cần bộ template mặc định.
- Chưa chốt chuẩn kế toán/thuế chi tiết có thể ảnh hưởng FIN và posting.

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện nghiệm thu module
1. 100% UC ưu tiên **Bắt buộc (Must)** của `FIN` pass UAT.
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
| Use case IDs | `UC_FIN_001` … `UC_FIN_083` |

---

*Hết tài liệu SRS-FIN-v1.0.*
