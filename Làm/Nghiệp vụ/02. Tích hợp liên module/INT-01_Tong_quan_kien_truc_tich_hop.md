# INT-01-v1.0 — Tổng quan kiến trúc tích hợp liên module

> **Integration Architecture Overview**
> Bộ tài liệu *Tích hợp liên module* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt nghiệp vụ / Solution**.
> Generic — không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `INT-01-v1.0` |
| Tên | Tổng quan kiến trúc tích hợp liên module |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Tích hợp liên module (BA / Solution) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Phụ thuộc | Bộ SRS module v1.1 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ tích hợp liên module | Chờ duyệt |
| 1.0.1 | 04/08/2026 | Bổ sung kênh realtime chat nội bộ (SYS-13 / SignalR) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả **nguyên tắc và mô hình tích hợp** giữa 16 module ERP, làm căn cứ chung trước khi thiết kế API/Event và cấu trúc source.

### 1.2. Phạm vi
- Tích hợp **nội bộ** giữa các module sản phẩm (qua SYS).
- Khung tích hợp **bên ngoài** (SSO, email/SMS, thanh toán, 3PL, HĐĐT) — mức logic.
- Không mô tả chi tiết UI từng màn hình (thuộc SRS module).

### 1.3. Đối tượng đọc
- Solution Architect, Tech Lead
- Business Analyst (chốt journey E2E)
- Presales (giải thích gói bán kèm module)

---

## 2. Bản đồ sản phẩm

```text
                         +------------------+
                         |   Clients Web    |
                         |  (Shell + packs) |
                         +--------+---------+
                                  |
                         +--------v---------+
                         |       SYS        |
                         | Auth RBAC License|
                         | Org File Notify  |
                         | Chat realtime    |
                         |   Event Bus      |
                         +--------+---------+
                                  |
     +------------+-------+-------+--------+-----------+
     |            |       |       |        |           |
   HRM/LMS     CRM/POS  PUR/INV  MFG/FSM  FIN/AST    WF/BI/PRT
               LOG/PJM
```

| Lớp | Module | Vai trò tích hợp |
|---|---|---|
| Nền tảng | SYS | Identity, quyền, license, bus, file, thông báo |
| Ngang | WF, BI, PRT | Duyệt / phân tích / cổng ngoài — nối nhiều module |
| Nhân sự | HRM, LMS | Vòng đời người & năng lực |
| Thương mại | CRM, POS, PRT | Bán hàng & trải nghiệm KH |
| Chuỗi cung ứng | PUR, INV, LOG, MFG | Mua – kho – giao – sản xuất |
| Tài sản & tiền | FIN, AST, FSM, PJM | Tiền, tài sản, dịch vụ, dự án |

---

## 3. Nguyên tắc tích hợp (bắt buộc)

| # | Nguyên tắc | Mô tả |
|---:|---|---|
| P1 | SYS-first | Mọi module bắt buộc phụ thuộc SYS; không tự auth/RBAC riêng. |
| P2 | Event over hard-call | Module không gọi API nội bộ module khác trực tiếp (trừ đọc master qua contract SYS/shared). Ưu tiên publish/subscribe. |
| P3 | Single owner | Mỗi master data có đúng một module sở hữu; module khác chỉ reference ID. |
| P4 | License gate | Middleware SYS chặn API/UI khi license module tắt; dữ liệu không bị xóa. |
| P5 | Data scope | Mọi truy vấn xuyên module tôn trọng data scope (chi nhánh/kho/đơn vị). |
| P6 | Idempotent consumer | Consumer sự kiện xử lý được trùng lặp an toàn (ít nhất một lần giao). |
| P7 | Audit & trace | Sự kiện nghiệp vụ trọng yếu có correlation id + audit. |
| P8 | Degrade gracefully | Module phụ thuộc mềm thiếu license → ẩn tính năng, không làm sập module chính. |

---

## 4. Mô hình giao tiếp

### 4.1. Ba kênh chuẩn

| Kênh | Khi nào dùng | Ví dụ |
|---|---|---|
| **Command / API đồng bộ** | Cần kết quả ngay, cùng transaction biên | Reserve tồn khi xác nhận SO |
| **Domain Event (async)** | Thông báo đã xảy ra, fan-out nhiều subscriber | `PayrollPosted` → FIN tạo bút toán |
| **Shared read model** | Tra cứu master ổn định, ít thay đổi | CustomerId, ItemId, EmployeeId |

### 4.2. Vai trò Event Bus (SYS)
1. Module nguồn **publish** sự kiện sau khi commit giao dịch thành công.
2. SYS lưu **Outbox** (đảm bảo không mất event).
3. Subscriber nhận, xử lý idempotent, ghi **Inbox/Processed**. 
4. Lỗi → retry có backoff + Dead Letter + cảnh báo vận hành.

### 4.3. Đồng bộ vs bất đồng bộ (gợi ý)
| Tình huống | Kiểu |
|---|---|
| Kiểm tra ATP / trừ tồn tại quầy | Đồng bộ |
| Post lương sang FIN | Bất đồng bộ + đối soát |
| Gửi thông báo duyệt | Bất đồng bộ |
| Tạo lệnh giao từ đơn | Đồng bộ tạo chứng từ + event trạng thái |

---

## 5. Ranh giới trách nhiệm SYS

| Khả năng | SYS cung cấp | Module nghiệp vụ |
|---|---|---|
| Đăng nhập / phiên | Có | Không tự quản |
| Permission catalog | Đăng ký & enforce | Khai báo mã quyền của mình |
| Sequence / mã chứng từ | Cấp số | Định nghĩa pattern nghiệp vụ |
| File đính kèm | Lưu & ACL | Gắn metadata nghiệp vụ |
| Thông báo | Template + kênh | Trigger nội dung |
| Workflow duyệt | Không bắt buộc sở hữu | Có thể dùng WF hoặc duyệt nội bộ |
| Event Bus | Hạ tầng | Publish / subscribe business events |

---

## 6. Master data — chủ sở hữu

| Thực thể | Module sở hữu | Module tham chiếu chính |
|---|---|---|
| Tenant / User / Role / Org | SYS | Tất cả |
| Employee / Contract | HRM | LMS, FIN, PJM, AST |
| Customer / Lead / Opportunity | CRM | POS, LOG, FIN, FSM, PRT |
| Vendor | PUR | FIN (AP), INV (GRN) |
| Item / UoM / Warehouse | INV *(hoặc master hàng hóa chung thuộc INV)* | POS, PUR, LOG, MFG, CRM |
| Chart of Accounts / Period | FIN | HRM, AST, CRM, PUR |
| Asset | AST | FIN, HRM, PUR |
| Project | PJM | FIN, INV, HRM, CRM |
| Course / Certificate | LMS | HRM |
| Ticket / Work Order | FSM | CRM, INV, PRT |

> Quy tắc: **cấm nhân bản lệch** — khi cần hiển thị, đọc theo ID hoặc projection chỉ đọc.

---

## 7. License & đóng gói bán

### 7.1. Hard dependency
- Mọi module nghiệp vụ: **SYS**.
- LOG: khuyến nghị cứng **INV** (xuất/nhập giao).
- MFG: khuyến nghị cứng **INV** (NVL/TP).

### 7.2. Soft dependency (tính năng E2E)
Ví dụ: HRM chạy được thiếu FIN (không post bút toán); CRM chạy được thiếu LOG (không tạo chuyến).
Chi tiết ma trận: tài liệu **INT-02**.

---

## 8. An toàn & tuân thủ tích hợp

- Mọi API liên module đi qua auth SYS + permission + license.
- Field nhạy cảm (lương, CCCD, giá vốn) không lộ qua event thô nếu subscriber thiếu quyền.
- Webhook outbound (SYS) ký secret; không nhúng credential module vào event payload.
- CorrelationId bắt buộc trên event nghiệp vụ trọng yếu để QA/trace.

---

## 9. Truy vết tài liệu

| Artifact | Vị trí |
|---|---|
| SRS từng module | `../01. Modules/**/SRS_*_v1.1.docx` |
| Chuẩn tích hợp | `00_CHUAN_TAI_LIEU.md` |
| Ma trận phụ thuộc | `INT-02` |
| Catalog sự kiện | `INT-03` |
| Luồng E2E | `INT-04` |
| Đồng bộ & lỗi | `INT-05` |

---

*Hết INT-01-v1.0.*
