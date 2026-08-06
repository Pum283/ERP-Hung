# PKG-02-v1.0 — Catalog SKU module

> **Sellable Module Catalog (SKU Sheets)**
> *Product Packaging & Commercial Catalog* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Presales / Solution**.
> Generic — không gắn khách/ngành cứng; giá tiền theo bảng giá nội bộ.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `PKG-02-v1.0` |
| Tên | Catalog SKU module |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Gói bán hàng (Presales / BA / Delivery) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Đầu vào | SRS module v1.1 · INT-02 · DDD license |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ Gói bán hàng | Chờ duyệt |

---

## 1. Giới thiệu

Danh mục **16 SKU module** — đơn vị bán nhỏ nhất (SYS là SKU nền, không bán độc lập).

---

## 2. Bảng tổng hợp SKU

| Mã | Tên | Bán riêng | Hard (Must) | Soft (Should) | Người mua chính | Giá trị 1 dòng |
|---|---|---|---|---|---|---|
| `SYS` | Hệ thống nền tảng | Không — luôn kèm | — | — | Mọi khách | Auth, RBAC, license, org, audit, bus, **nhắn tin realtime** |
| `HRM` | Quản trị nhân sự | Có | SYS | WF, FIN, LMS, AST | HCNS / People Ops | Hồ sơ–công–phép–lương–nghỉ việc |
| `LMS` | Đào tạo | Có | SYS | HRM, CRM, FIN | Đào tạo / Compliance | Khóa học, thi, chứng chỉ, lộ trình |
| `CRM` | CRM & Bán hàng | Có | SYS | INV, LOG, FIN, WF, PRT | Sales / CSKH | Lead→Opportunity→SO→công nợ |
| `POS` | POS bán lẻ | Có | SYS | INV, FIN, CRM | Retail Ops | Ca bán, thanh toán, tồn quầy |
| `PUR` | Mua hàng | Có | SYS | INV, FIN, WF | Procurement | PR→PO→nhận hàng→AP |
| `INV` | Kho & Tồn kho | Có | SYS | PUR, LOG, POS, MFG, FIN | Warehouse / Supply | Master hàng, tồn, nhập xuất |
| `LOG` | Giao vận | Có | SYS, INV | CRM, FIN, PRT | Logistics | Lệnh giao, tracking, đối soát |
| `MFG` | Sản xuất | Có | SYS, INV | PUR, FIN, HRM, CRM | Production | BOM, lệnh SX, NVL/TP |
| `FSM` | Dịch vụ kỹ thuật | Có | SYS | CRM, INV, FIN, PRT | Field Service | Ticket, lịch kỹ thuật, linh kiện |
| `PJM` | Quản lý dự án | Có | SYS | CRM, INV, FIN, HRM, WF | PMO / Delivery | Dự án, task, ngân sách, quyết toán |
| `FIN` | Tài chính – Kế toán | Có | SYS | CRM, PUR, INV, POS, HRM, AST | Kế toán / CFO | GL, AR/AP, sổ, khóa kỳ |
| `AST` | Quản lý tài sản | Có | SYS | FIN, HRM, PUR | Admin / Asset | Tài sản, khấu hao, bàn giao |
| `WF` | Công việc & Phê duyệt | Có (hoặc kèm SYS) | SYS | Mọi module có duyệt | Governance | Quy trình duyệt tập trung |
| `BI` | Báo cáo & BI | Có | SYS | Dataset theo license nguồn | Management | Dashboard, dataset có kiểm soát |
| `PRT` | Cổng khách hàng | Có | SYS | CRM, LOG, FIN, FSM, PUR | CSKH / Self-service | Portal KH: đơn, công nợ, ticket |

---

## 3. Chi tiết từng SKU

### 3.SYS. `SYS` — Hệ thống nền tảng

**Giá trị bán:** Nền tảng bắt buộc: đăng nhập, RBAC + data scope 4 tầng, org/department/job level, license, menu, file, thông báo, **nhắn tin realtime (SYS-13 / SignalR)**, audit, outbox/inbox.

**Ghi chú đóng gói:**
- Không tắt khi còn module khác active
- Quota user/tenant áp dụng tại đây

**Hay nằm trong bundle:** Mọi gói

### 3.HRM. `HRM` — Quản trị nhân sự

**Giá trị bán:** Vòng đời nhân sự: hồ sơ, hợp đồng, chấm công/phép, lương (khung), điều chuyển, nghỉ việc.

**Ghi chú đóng gói:**
- Cần SYS
- Nên kèm WF nếu duyệt phép/tuyển tập trung
- FIN nếu post chi phí lương

**Hay nằm trong bundle:** Gói Nhân sự số, Enterprise full

### 3.LMS. `LMS` — Đào tạo

**Giá trị bán:** Đào tạo nội bộ / compliance: chương trình, lớp, bài thi, chứng chỉ, lộ trình theo vị trí.

**Ghi chú đóng gói:**
- Chạy độc lập trên SYS
- Gán NV theo vị trí tốt hơn khi có HRM

**Hay nằm trong bundle:** Gói Nhân sự số, Compliance

### 3.CRM. `CRM` — CRM & Bán hàng

**Giá trị bán:** Pipeline bán hàng B2B/B2C: lead, cơ hội, báo giá, đơn bán, CSKH.

**Ghi chú đóng gói:**
- Giao hàng & tồn cần INV+LOG
- Công nợ cần FIN
- Portal cần PRT

**Hay nằm trong bundle:** Gói Bán hàng & phân phối, Dịch vụ & dự án

### 3.POS. `POS` — POS bán lẻ

**Giá trị bán:** Bán tại quầy: ca, giỏ hàng, thanh toán, khuyến mãi tại điểm bán.

**Ghi chú đóng gói:**
- Trừ tồn realtime nên có INV
- Đối soát quỹ/doanh thu nên có FIN

**Hay nằm trong bundle:** Gói Bán lẻ

### 3.PUR. `PUR` — Mua hàng

**Giá trị bán:** Mua hàng: nhu cầu, PR, PO, theo dõi NCC, nhận hàng gắn kho.

**Ghi chú đóng gói:**
- GRN/tồn cần INV
- Công nợ NCC cần FIN
- Duyệt PR/PO nên có WF

**Hay nằm trong bundle:** Gói Mua–Kho–SX, Procure-to-Pay

### 3.INV. `INV` — Kho & Tồn kho

**Giá trị bán:** Master hàng hóa, kho, tồn, nhập/xuất/điều chuyển, kiểm kê.

**Ghi chú đóng gói:**
- Hard cho LOG/MFG
- Trục trung tâm chuỗi cung ứng

**Hay nằm trong bundle:** Hầu hết gói vận hành hàng hóa

### 3.LOG. `LOG` — Giao vận

**Giá trị bán:** Giao vận: lệnh giao, tuyến, trạng thái giao, đối soát vận chuyển.

**Ghi chú đóng gói:**
- **Must có INV**
- Theo dõi đơn/KH tốt hơn với CRM
- Portal tracking với PRT

**Hay nằm trong bundle:** Gói Bán hàng & phân phối

### 3.MFG. `MFG` — Sản xuất

**Giá trị bán:** Sản xuất: BOM, lệnh SX, xuất NVL, nhập TP, báo cáo sản lượng.

**Ghi chú đóng gói:**
- **Must có INV**
- Giá thành/FIN và mua NVL/PUR nên kèm

**Hay nằm trong bundle:** Gói Mua–Kho–SX

### 3.FSM. `FSM` — Dịch vụ kỹ thuật

**Giá trị bán:** Dịch vụ hiện trường: ticket, lịch kỹ thuật viên, vật tư hiện trường.

**Ghi chú đóng gói:**
- CRM cho hợp đồng KH
- INV cho linh kiện
- PRT cho self-service

**Hay nằm trong bundle:** Gói Dịch vụ & dự án

### 3.PJM. `PJM` — Quản lý dự án

**Giá trị bán:** Quản lý dự án: WBS/task, tiến độ, ngân sách, quyết toán.

**Ghi chú đóng gói:**
- Doanh thu/chi phí với FIN+CRM
- Vật tư với INV

**Hay nằm trong bundle:** Gói Dịch vụ & dự án

### 3.FIN. `FIN` — Tài chính – Kế toán

**Giá trị bán:** Kế toán: danh mục TK, chứng từ, AR/AP, sổ cái, khóa kỳ, báo cáo tài chính khung.

**Ghi chú đóng gói:**
- Là hub post từ nhiều module
- Không bắt buộc Day-1 mọi SME — nhưng cần cho E2E tiền

**Hay nằm trong bundle:** Mọi gói có dòng tiền / sổ sách

### 3.AST. `AST` — Quản lý tài sản

**Giá trị bán:** Tài sản cố định / CCDC: danh mục, bàn giao, khấu hao, thanh lý.

**Ghi chú đóng gói:**
- Khấu hao/FIN
- Thu hồi khi nghỉ việc/HRM

**Hay nằm trong bundle:** Gói Nhân sự số, Enterprise

### 3.WF. `WF` — Công việc & Phê duyệt

**Giá trị bán:** Engine phê duyệt tập trung: định nghĩa quy trình, task, ủy quyền.

**Ghi chú đóng gói:**
- Không bắt buộc nếu module có duyệt nội bộ đơn giản
- Khuyến nghị khi nhiều chứng từ Must approve

**Hay nằm trong bundle:** Mọi gói governance

### 3.BI. `BI` — Báo cáo & BI

**Giá trị bán:** Lớp báo cáo/dashboard; dataset chỉ mở theo license module nguồn.

**Ghi chú đóng gói:**
- Không thay thế báo cáo vận hành trong từng module
- Bán kèm khi cần C-level view

**Hay nằm trong bundle:** Add-on hầu hết gói

### 3.PRT. `PRT` — Cổng khách hàng

**Giá trị bán:** Cổng khách hàng / đối tác: xem đơn, công nợ, ticket, tài liệu.

**Ghi chú đóng gói:**
- Phụ thuộc dữ liệu CRM/LOG/FIN/FSM
- Bảo mật portal role riêng

**Hay nằm trong bundle:** Gói Bán hàng, Dịch vụ

---

## 4. Quy tắc đặt hàng SKU

1. Luôn có dòng `SYS` trên báo giá.
2. Nếu chọn `LOG` hoặc `MFG` → bắt buộc có `INV` cùng Phase.
3. Mỗi SKU Soft nên có cột Phase (1/2) trên báo giá.
4. Không đổi tên mã module trên hợp đồng (`HRM`, `CRM`…) — giữ ổn định license.

---

*Hết PKG-02-v1.0.*
