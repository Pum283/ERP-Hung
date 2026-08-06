# SRS-CRM-v1.1 — CRM & Bán hàng

> **Software Requirements Specification — Module CRM**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-CRM-v1.1` |
| Module | `CRM` — CRM & Bán hàng |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Bán hàng & Khách hàng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `INV`, `LOG`, `FIN`, `WF`, `PRT` |
| Số nhóm / UC | 15 nhóm / 131 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **CRM & Bán hàng** (`CRM`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module CRM quản lý khách hàng, marketing, omnichannel, lead–cơ hội–báo giá–đơn hàng, bán hàng hiện trường, sales admin, CSKH, hoa hồng và báo cáo doanh số. Thiết kế generic cho B2B/B2C/phân phối.

### 1.3. Mục tiêu đo được
1. Một hồ sơ khách 360° dùng chung toàn công ty.
2. Chuẩn hóa phễu Lead → Order.
3. Kiểm soát giá, chiết khấu, hạn mức công nợ.
4. Đo hiệu quả marketing và năng suất sales.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Customer master, campaign, promo, omnichannel inbox, lead, opportunity, quote, order, route sales, sales admin, contract/policy, CS, commission, CRM reports.

### 2.2. Out of Scope
- Thanh toán quầy POS (POS).
- Xuất kho/giao hàng chi tiết (INV/LOG).
- Hạch toán kế toán đầy đủ (FIN).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `INV`, `LOG`, `FIN`, `WF`, `PRT`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Sales Rep | Chăm lead/KH, báo giá, đơn |
| Sales Manager | Pipeline, duyệt chiết khấu, KPI đội |
| Sales Admin | Xử lý hàng đợi đơn, đối soát chứng từ |
| Marketer | Campaign, nguồn lead, KM |
| Contact Center Agent | Inbox omnichannel |
| CSKH | Ticket sau bán, loyalty |
| Khách hàng (gián tiếp) | Qua PRT/kênh chat |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Lead | Đầu mối tiềm năng chưa thành KH/cơ hội |
| Opportunity | Cơ hội bán có giá trị và giai đoạn |
| Quote | Báo giá |
| Reserve stock | Giữ tồn khi đơn được duyệt |
| Credit limit | Hạn mức công nợ |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- CRM (CRM & Bán hàng)
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `CRM` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `CRM` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | INV | ATP, reserve, giá vốn tham chiếu |
| Tích hợp | LOG | Lệnh giao từ đơn |
| Tích hợp | FIN | Công nợ, thanh toán, doanh thu |
| Tích hợp | WF | Duyệt báo giá/chiết khấu/vượt hạn mức |
| Tích hợp | FSM | Escalate ticket kỹ thuật |
| Tích hợp | PRT | KH tự phục vụ đơn/công nợ/ticket |
| Tích hợp | Meta/Zalo/Web | Lead & messaging |

---

## 6. Catalog chức năng

**Tổng:** 15 nhóm · 131 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `CRM-01` | Master khách hàng | 15 | 12 | 3 | 0 |
| 2 | `CRM-02` | Marketing – chiến dịch | 8 | 3 | 4 | 1 |
| 3 | `CRM-03` | Marketing – nguồn & đo lường | 8 | 4 | 4 | 0 |
| 4 | `CRM-04` | Khuyến mại & voucher | 7 | 5 | 2 | 0 |
| 5 | `CRM-05` | Omnichannel & chatbot | 10 | 1 | 7 | 2 |
| 6 | `CRM-06` | Lead | 13 | 11 | 2 | 0 |
| 7 | `CRM-07` | Cơ hội bán hàng | 8 | 5 | 2 | 1 |
| 8 | `CRM-08` | Báo giá | 9 | 6 | 3 | 0 |
| 9 | `CRM-09` | Sales Online / đơn hàng | 10 | 6 | 3 | 1 |
| 10 | `CRM-10` | Sales Offline / Route sales | 10 | 0 | 9 | 1 |
| 11 | `CRM-11` | Sales Admin | 7 | 4 | 2 | 1 |
| 12 | `CRM-12` | Hợp đồng & chính sách bán | 6 | 2 | 4 | 0 |
| 13 | `CRM-13` | Chăm sóc khách hàng | 8 | 2 | 3 | 3 |
| 14 | `CRM-14` | Hoa hồng & KPI sales | 6 | 1 | 4 | 1 |
| 15 | `CRM-15` | Báo cáo CRM | 6 | 4 | 2 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_CRM_001` | Master khách hàng | Tạo khách hàng cá nhân | Must |
| `UC_CRM_002` | Master khách hàng | Tạo khách hàng doanh nghiệp | Must |
| `UC_CRM_003` | Master khách hàng | Cập nhật thông tin khách hàng | Must |
| `UC_CRM_004` | Master khách hàng | Kiểm tra trùng SĐT / MST | Must |
| `UC_CRM_005` | Master khách hàng | Gộp khách hàng trùng | Must |
| `UC_CRM_006` | Master khách hàng | Phân loại tệp khách hàng | Must |
| `UC_CRM_007` | Master khách hàng | Đánh giá tiềm năng | Should |
| `UC_CRM_008` | Master khách hàng | Gán người phụ trách | Must |
| `UC_CRM_009` | Master khách hàng | Bàn giao khách hàng | Must |
| `UC_CRM_010` | Master khách hàng | Hồ sơ khách 360° | Must |
| `UC_CRM_011` | Master khách hàng | Danh sách người liên hệ | Must |
| `UC_CRM_012` | Master khách hàng | Lịch sử thay đổi dữ liệu | Should |
| `UC_CRM_013` | Master khách hàng | Ngưng sử dụng / blacklist | Should |
| `UC_CRM_014` | Master khách hàng | Import / export khách hàng | Must |
| `UC_CRM_015` | Master khách hàng | Tìm kiếm khách đa tiêu chí | Must |
| `UC_CRM_016` | Marketing – chiến dịch | Tạo campaign marketing | Must |
| `UC_CRM_017` | Marketing – chiến dịch | Quản lý nhóm quảng cáo | Should |
| `UC_CRM_018` | Marketing – chiến dịch | Gắn sản phẩm / đối tượng mục tiêu | Should |
| `UC_CRM_019` | Marketing – chiến dịch | Ghi nhận chi phí quảng cáo | Must |
| `UC_CRM_020` | Marketing – chiến dịch | Gắn ngân sách & theo dõi | Should |
| `UC_CRM_021` | Marketing – chiến dịch | Đánh giá hậu chiến dịch | Should |
| `UC_CRM_022` | Marketing – chiến dịch | Nhân bản campaign | Could |
| `UC_CRM_023` | Marketing – chiến dịch | Đóng campaign | Must |
| `UC_CRM_024` | Marketing – nguồn & đo lường | Danh mục nguồn lead | Must |
| `UC_CRM_025` | Marketing – nguồn & đo lường | Đồng bộ lead mạng xã hội | Should |
| `UC_CRM_026` | Marketing – nguồn & đo lường | Đồng bộ lead website / landing | Must |
| `UC_CRM_027` | Marketing – nguồn & đo lường | Đồng bộ kênh khác | Should |
| `UC_CRM_028` | Marketing – nguồn & đo lường | Attribution nguồn khách | Should |
| `UC_CRM_029` | Marketing – nguồn & đo lường | Tính CPL / CAC / ROAS / ROI | Must |
| `UC_CRM_030` | Marketing – nguồn & đo lường | Funnel marketing đến doanh thu | Should |
| `UC_CRM_031` | Marketing – nguồn & đo lường | Dashboard marketing | Must |
| `UC_CRM_032` | Khuyến mại & voucher | Tạo chương trình khuyến mại | Must |
| `UC_CRM_033` | Khuyến mại & voucher | Cấu hình điều kiện khuyến mại | Must |
| `UC_CRM_034` | Khuyến mại & voucher | Sinh mã voucher | Must |
| `UC_CRM_035` | Khuyến mại & voucher | Giới hạn lượt dùng voucher | Must |
| `UC_CRM_036` | Khuyến mại & voucher | Đồng bộ khuyến mại sang POS | Should |
| `UC_CRM_037` | Khuyến mại & voucher | Áp dụng khuyến mại trên báo giá | Must |
| `UC_CRM_038` | Khuyến mại & voucher | Báo cáo sử dụng voucher | Should |
| `UC_CRM_039` | Omnichannel & chatbot | Hộp thư tập trung đa kênh | Should |
| `UC_CRM_040` | Omnichannel & chatbot | Tiếp nhận hội thoại mới | Should |
| `UC_CRM_041` | Omnichannel & chatbot | Phân phối hội thoại theo rule | Should |
| `UC_CRM_042` | Omnichannel & chatbot | Chuyển hội thoại giữa agent | Should |
| `UC_CRM_043` | Omnichannel & chatbot | SLA phản hồi & cảnh báo | Should |
| `UC_CRM_044` | Omnichannel & chatbot | Chatbot kịch bản | Could |
| `UC_CRM_045` | Omnichannel & chatbot | Chatbot thu thập lead | Should |
| `UC_CRM_046` | Omnichannel & chatbot | Chuyển bot sang agent | Should |
| `UC_CRM_047` | Omnichannel & chatbot | Lưu lịch sử chat | Must |
| `UC_CRM_048` | Omnichannel & chatbot | Đánh giá CSAT | Could |
| `UC_CRM_049` | Lead | Tạo lead thủ công | Must |
| `UC_CRM_050` | Lead | Tiếp nhận lead tự động | Must |
| `UC_CRM_051` | Lead | Phân bổ lead cho sales | Must |
| `UC_CRM_052` | Lead | Lead scoring | Should |
| `UC_CRM_053` | Lead | Cập nhật trạng thái pipeline | Must |
| `UC_CRM_054` | Lead | Task follow-up lead | Must |
| `UC_CRM_055` | Lead | Nhắc việc follow-up | Must |
| `UC_CRM_056` | Lead | Nhật ký chăm sóc lead | Must |
| `UC_CRM_057` | Lead | Chuyển lead thành cơ hội | Must |
| `UC_CRM_058` | Lead | Đánh dấu lead mất | Must |
| `UC_CRM_059` | Lead | Gộp lead trùng | Should |
| `UC_CRM_060` | Lead | Import lead Excel | Must |
| `UC_CRM_061` | Lead | Báo cáo chuyển đổi lead | Must |
| `UC_CRM_062` | Cơ hội bán hàng | Tạo cơ hội từ lead/khách | Must |
| `UC_CRM_063` | Cơ hội bán hàng | Pipeline cơ hội theo giai đoạn | Must |
| `UC_CRM_064` | Cơ hội bán hàng | Dự báo doanh thu | Should |
| `UC_CRM_065` | Cơ hội bán hàng | Gắn sản phẩm / giá trị ước tính | Must |
| `UC_CRM_066` | Cơ hội bán hàng | Đối thủ / ghi chú đàm phán | Could |
| `UC_CRM_067` | Cơ hội bán hàng | Chuyển cơ hội sang báo giá | Must |
| `UC_CRM_068` | Cơ hội bán hàng | Đóng thắng / thua | Must |
| `UC_CRM_069` | Cơ hội bán hàng | Báo cáo win-rate | Should |
| `UC_CRM_070` | Báo giá | Tạo báo giá từ cơ hội | Must |
| `UC_CRM_071` | Báo giá | Thêm dòng sản phẩm / dịch vụ | Must |
| `UC_CRM_072` | Báo giá | Áp chính sách giá / bảng giá | Must |
| `UC_CRM_073` | Báo giá | Xin duyệt chiết khấu | Must |
| `UC_CRM_074` | Báo giá | Gửi báo giá PDF/email | Must |
| `UC_CRM_075` | Báo giá | Phiên bản báo giá | Should |
| `UC_CRM_076` | Báo giá | Hết hạn báo giá tự động | Should |
| `UC_CRM_077` | Báo giá | Chuyển báo giá thành đơn | Must |
| `UC_CRM_078` | Báo giá | In mẫu báo giá | Should |
| `UC_CRM_079` | Sales Online / đơn hàng | Tạo đơn hàng từ báo giá | Must |
| `UC_CRM_080` | Sales Online / đơn hàng | Tiếp nhận đơn từ kênh online | Should |
| `UC_CRM_081` | Sales Online / đơn hàng | Cập nhật trạng thái đơn | Must |
| `UC_CRM_082` | Sales Online / đơn hàng | Giữ tồn khi duyệt đơn | Must |
| `UC_CRM_083` | Sales Online / đơn hàng | Tách / gộp đơn | Could |
| `UC_CRM_084` | Sales Online / đơn hàng | Hủy đơn có kiểm soát | Must |
| `UC_CRM_085` | Sales Online / đơn hàng | Trả hàng / điều chỉnh đơn | Should |
| `UC_CRM_086` | Sales Online / đơn hàng | Gắn hợp đồng | Should |
| `UC_CRM_087` | Sales Online / đơn hàng | Theo dõi thanh toán | Must |
| `UC_CRM_088` | Sales Online / đơn hàng | Đẩy đơn sang kho / giao vận | Must |
| `UC_CRM_089` | Sales Offline / Route sales | Phân vùng / tuyến bán hàng | Should |
| `UC_CRM_090` | Sales Offline / Route sales | Phân loại tần suất visit | Should |
| `UC_CRM_091` | Sales Offline / Route sales | Lập kế hoạch visit | Should |
| `UC_CRM_092` | Sales Offline / Route sales | Check-in / check-out GPS | Should |
| `UC_CRM_093` | Sales Offline / Route sales | Ghi nhận mục đích – kết quả visit | Should |
| `UC_CRM_094` | Sales Offline / Route sales | Ghi nhận nhu cầu khách hàng | Should |
| `UC_CRM_095` | Sales Offline / Route sales | Đặt hàng tại điểm thăm | Should |
| `UC_CRM_096` | Sales Offline / Route sales | Xem lịch sử visit | Should |
| `UC_CRM_097` | Sales Offline / Route sales | AI gợi ý việc ưu tiên | Later |
| `UC_CRM_098` | Sales Offline / Route sales | Dashboard doanh số field | Should |
| `UC_CRM_099` | Sales Admin | Hàng đợi đơn cần xử lý | Must |
| `UC_CRM_100` | Sales Admin | Kiểm tra tồn / xác nhận giữ hàng | Must |
| `UC_CRM_101` | Sales Admin | Soạn lệnh xuất / giao | Must |
| `UC_CRM_102` | Sales Admin | Đối soát chứng từ đơn | Should |
| `UC_CRM_103` | Sales Admin | Xử lý khiếu nại đơn hàng | Should |
| `UC_CRM_104` | Sales Admin | Theo dõi đơn chậm xử lý | Must |
| `UC_CRM_105` | Sales Admin | Báo cáo năng suất Sales Admin | Could |
| `UC_CRM_106` | Hợp đồng & chính sách bán | Quản lý hợp đồng bán | Should |
| `UC_CRM_107` | Hợp đồng & chính sách bán | Đính kèm file hợp đồng | Should |
| `UC_CRM_108` | Hợp đồng & chính sách bán | Theo dõi hiệu lực / tái tục | Should |
| `UC_CRM_109` | Hợp đồng & chính sách bán | Chính sách giá theo nhóm KH | Must |
| `UC_CRM_110` | Hợp đồng & chính sách bán | Chính sách công nợ / hạn mức | Must |
| `UC_CRM_111` | Hợp đồng & chính sách bán | Chặn bán khi vượt công nợ | Should |
| `UC_CRM_112` | Chăm sóc khách hàng | Tạo ticket hỗ trợ | Must |
| `UC_CRM_113` | Chăm sóc khách hàng | Phân loại khiếu nại / yêu cầu | Must |
| `UC_CRM_114` | Chăm sóc khách hàng | Chuyển ticket sang FSM | Should |
| `UC_CRM_115` | Chăm sóc khách hàng | Lịch chăm sóc / nhắc tái mua | Should |
| `UC_CRM_116` | Chăm sóc khách hàng | Chương trình loyalty | Could |
| `UC_CRM_117` | Chăm sóc khách hàng | Tích điểm / đổi quà | Could |
| `UC_CRM_118` | Chăm sóc khách hàng | Khảo sát hài lòng | Could |
| `UC_CRM_119` | Chăm sóc khách hàng | Báo cáo retention / tái mua | Should |
| `UC_CRM_120` | Hoa hồng & KPI sales | Cấu hình rule hoa hồng | Should |
| `UC_CRM_121` | Hoa hồng & KPI sales | Tính hoa hồng theo kỳ | Should |
| `UC_CRM_122` | Hoa hồng & KPI sales | Duyệt bảng hoa hồng | Should |
| `UC_CRM_123` | Hoa hồng & KPI sales | Đồng bộ hoa hồng sang HRM/FIN | Should |
| `UC_CRM_124` | Hoa hồng & KPI sales | KPI doanh số theo nhân viên | Must |
| `UC_CRM_125` | Hoa hồng & KPI sales | Bảng xếp hạng sales | Could |
| `UC_CRM_126` | Báo cáo CRM | Dashboard Ban lãnh đạo sales | Must |
| `UC_CRM_127` | Báo cáo CRM | Báo cáo pipeline & forecast | Must |
| `UC_CRM_128` | Báo cáo CRM | Báo cáo theo nguồn / campaign | Must |
| `UC_CRM_129` | Báo cáo CRM | Báo cáo theo nhân viên / vùng | Must |
| `UC_CRM_130` | Báo cáo CRM | Báo cáo công nợ bán | Should |
| `UC_CRM_131` | Báo cáo CRM | Xuất báo cáo định kỳ | Should |

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

### 7.1. Master khách hàng (`CRM-01`)

Nhóm **Master khách hàng** gồm **15** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 15 |
| Must | 12 |

**Bảng 1. Đặc tả Use Case "Tạo khách hàng cá nhân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_001 |
| **Tên Use Case** | Tạo khách hàng cá nhân |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Tạo khách hàng cá nhân" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Individual customer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo khách hàng cá nhân» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo khách hàng cá nhân» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo khách hàng cá nhân» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin mở chức năng «Tạo khách hàng cá nhân» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo khách hàng cá nhân» (Individual customer).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo khách hàng cá nhân» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo khách hàng cá nhân» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 2. Đặc tả Use Case "Tạo khách hàng doanh nghiệp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_002 |
| **Tên Use Case** | Tạo khách hàng doanh nghiệp |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Tạo khách hàng doanh nghiệp" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Corporate account |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo khách hàng doanh nghiệp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo khách hàng doanh nghiệp» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo khách hàng doanh nghiệp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin mở chức năng «Tạo khách hàng doanh nghiệp» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo khách hàng doanh nghiệp» (Corporate account).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo khách hàng doanh nghiệp» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo khách hàng doanh nghiệp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 3. Đặc tả Use Case "Cập nhật thông tin khách hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_003 |
| **Tên Use Case** | Cập nhật thông tin khách hàng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Cập nhật thông tin khách hàng" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Update customer info |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cập nhật thông tin khách hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cập nhật thông tin khách hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cập nhật thông tin khách hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin tìm và mở bản ghi liên quan tới «Cập nhật thông tin khách hàng» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Cập nhật thông tin khách hàng» (Update customer info).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cập nhật thông tin khách hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 4. Đặc tả Use Case "Kiểm tra trùng SĐT / MST"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_004 |
| **Tên Use Case** | Kiểm tra trùng SĐT / MST |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Kiểm tra trùng SĐT / MST" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Duplicate detection |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm tra trùng SĐT / MST» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm tra trùng SĐT / MST» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm tra trùng SĐT / MST» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Kiểm tra trùng SĐT / MST» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Duplicate detection).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm tra trùng SĐT / MST».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm tra trùng SĐT / MST» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 5. Đặc tả Use Case "Gộp khách hàng trùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_005 |
| **Tên Use Case** | Gộp khách hàng trùng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Gộp khách hàng trùng" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Merge accounts |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gộp khách hàng trùng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gộp khách hàng trùng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gộp khách hàng trùng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Gộp khách hàng trùng» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Merge accounts).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gộp khách hàng trùng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gộp khách hàng trùng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 6. Đặc tả Use Case "Phân loại tệp khách hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_006 |
| **Tên Use Case** | Phân loại tệp khách hàng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Phân loại tệp khách hàng" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Customer segmentation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại tệp khách hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại tệp khách hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại tệp khách hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Phân loại tệp khách hàng» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer segmentation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại tệp khách hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại tệp khách hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 7. Đặc tả Use Case "Đánh giá tiềm năng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_007 |
| **Tên Use Case** | Đánh giá tiềm năng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Đánh giá tiềm năng" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Potential scoring |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá tiềm năng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá tiềm năng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá tiềm năng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Đánh giá tiềm năng» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Potential scoring).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá tiềm năng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá tiềm năng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Gán người phụ trách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_008 |
| **Tên Use Case** | Gán người phụ trách |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Gán người phụ trách" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Account ownership |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán người phụ trách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán người phụ trách» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán người phụ trách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin chọn đối tượng nguồn trong «Gán người phụ trách».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán người phụ trách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Bàn giao khách hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_009 |
| **Tên Use Case** | Bàn giao khách hàng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Bàn giao khách hàng" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Account transfer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bàn giao khách hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bàn giao khách hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bàn giao khách hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Bàn giao khách hàng» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Account transfer).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bàn giao khách hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bàn giao khách hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Hồ sơ khách 360°"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_010 |
| **Tên Use Case** | Hồ sơ khách 360° |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Hồ sơ khách 360°" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: 360° customer view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hồ sơ khách 360°» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hồ sơ khách 360°» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hồ sơ khách 360°» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Hồ sơ khách 360°» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (360° customer view).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hồ sơ khách 360°».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hồ sơ khách 360°» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "Danh sách người liên hệ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_011 |
| **Tên Use Case** | Danh sách người liên hệ |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Danh sách người liên hệ" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Contact management |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh sách người liên hệ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh sách người liên hệ» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh sách người liên hệ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Danh sách người liên hệ» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Contact management).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh sách người liên hệ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh sách người liên hệ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Lịch sử thay đổi dữ liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_012 |
| **Tên Use Case** | Lịch sử thay đổi dữ liệu |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Lịch sử thay đổi dữ liệu" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Field audit trail |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử thay đổi dữ liệu» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử thay đổi dữ liệu» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử thay đổi dữ liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Admin mở «Lịch sử thay đổi dữ liệu» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Field audit trail).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử thay đổi dữ liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 13. Đặc tả Use Case "Ngưng sử dụng / blacklist"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_013 |
| **Tên Use Case** | Ngưng sử dụng / blacklist |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Ngưng sử dụng / blacklist" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Inactive/block customer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ngưng sử dụng / blacklist» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ngưng sử dụng / blacklist» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ngưng sử dụng / blacklist» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Ngưng sử dụng / blacklist» trong nhóm Master khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Inactive/block customer).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ngưng sử dụng / blacklist».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ngưng sử dụng / blacklist» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 14. Đặc tả Use Case "Import / export khách hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_014 |
| **Tên Use Case** | Import / export khách hàng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Import / export khách hàng" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Customer data import/export |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import / export khách hàng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import / export khách hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import / export khách hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin mở «Import / export khách hàng», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Import / export khách hàng» (Customer data import/export).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import / export khách hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 15. Đặc tả Use Case "Tìm kiếm khách đa tiêu chí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_015 |
| **Tên Use Case** | Tìm kiếm khách đa tiêu chí |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Tìm kiếm khách đa tiêu chí" thuộc nhóm Master khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Advanced customer search |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tìm kiếm khách đa tiêu chí» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tìm kiếm khách đa tiêu chí» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tìm kiếm khách đa tiêu chí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin mở «Tìm kiếm khách đa tiêu chí» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Advanced customer search).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tìm kiếm khách đa tiêu chí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.2. Marketing – chiến dịch (`CRM-02`)

Nhóm **Marketing – chiến dịch** gồm **8** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 3 |

**Bảng 16. Đặc tả Use Case "Tạo campaign marketing"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_016 |
| **Tên Use Case** | Tạo campaign marketing |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Tạo campaign marketing" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Create campaign |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo campaign marketing» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo campaign marketing» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo campaign marketing» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer mở chức năng «Tạo campaign marketing» trong nhóm Marketing – chiến dịch.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo campaign marketing» (Create campaign).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo campaign marketing» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo campaign marketing» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 17. Đặc tả Use Case "Quản lý nhóm quảng cáo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_017 |
| **Tên Use Case** | Quản lý nhóm quảng cáo |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Quản lý nhóm quảng cáo" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Ad group management |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý nhóm quảng cáo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý nhóm quảng cáo» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý nhóm quảng cáo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer mở danh mục quản lý «Quản lý nhóm quảng cáo» (khách hàng / cơ hội / báo giá – đơn hàng; nhóm «Marketing – chiến dịch»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý nhóm quảng cáo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 18. Đặc tả Use Case "Gắn sản phẩm / đối tượng mục tiêu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_018 |
| **Tên Use Case** | Gắn sản phẩm / đối tượng mục tiêu |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Gắn sản phẩm / đối tượng mục tiêu" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Targeting setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn sản phẩm / đối tượng mục tiêu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn sản phẩm / đối tượng mục tiêu» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn sản phẩm / đối tượng mục tiêu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Gắn sản phẩm / đối tượng mục tiêu» trong nhóm Marketing – chiến dịch.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Targeting setup).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn sản phẩm / đối tượng mục tiêu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn sản phẩm / đối tượng mục tiêu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 19. Đặc tả Use Case "Ghi nhận chi phí quảng cáo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_019 |
| **Tên Use Case** | Ghi nhận chi phí quảng cáo |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Ghi nhận chi phí quảng cáo" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Campaign cost tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận chi phí quảng cáo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận chi phí quảng cáo» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận chi phí quảng cáo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Ghi nhận chi phí quảng cáo» trong nhóm Marketing – chiến dịch.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Campaign cost tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận chi phí quảng cáo».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận chi phí quảng cáo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Gắn ngân sách & theo dõi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_020 |
| **Tên Use Case** | Gắn ngân sách & theo dõi |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Gắn ngân sách & theo dõi" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Budget tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn ngân sách & theo dõi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn ngân sách & theo dõi» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn ngân sách & theo dõi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Gắn ngân sách & theo dõi» trong nhóm Marketing – chiến dịch.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Budget tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn ngân sách & theo dõi».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn ngân sách & theo dõi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Đánh giá hậu chiến dịch"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_021 |
| **Tên Use Case** | Đánh giá hậu chiến dịch |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Đánh giá hậu chiến dịch" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Campaign performance review |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá hậu chiến dịch» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá hậu chiến dịch» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá hậu chiến dịch» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Đánh giá hậu chiến dịch» trong nhóm Marketing – chiến dịch.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Campaign performance review).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá hậu chiến dịch».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá hậu chiến dịch» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Nhân bản campaign"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_022 |
| **Tên Use Case** | Nhân bản campaign |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Nhân bản campaign" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Clone campaign |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhân bản campaign» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhân bản campaign» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhân bản campaign» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Nhân bản campaign» trong nhóm Marketing – chiến dịch.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Clone campaign).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhân bản campaign».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhân bản campaign» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Đóng campaign"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_023 |
| **Tên Use Case** | Đóng campaign |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Đóng campaign" thuộc nhóm Marketing – chiến dịch trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Close campaign |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng campaign» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng campaign» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng campaign» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer chọn kỳ/ca/đối tượng cần khóa trong «Đóng campaign».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng campaign» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

### 7.3. Marketing – nguồn & đo lường (`CRM-03`)

Nhóm **Marketing – nguồn & đo lường** gồm **8** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 4 |

**Bảng 24. Đặc tả Use Case "Danh mục nguồn lead"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_024 |
| **Tên Use Case** | Danh mục nguồn lead |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Danh mục nguồn lead" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead source taxonomy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục nguồn lead» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục nguồn lead» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục nguồn lead» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Danh mục nguồn lead» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead source taxonomy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục nguồn lead».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục nguồn lead» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Đồng bộ lead mạng xã hội"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_025 |
| **Tên Use Case** | Đồng bộ lead mạng xã hội |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Đồng bộ lead mạng xã hội" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Social media lead sync |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ lead mạng xã hội» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ lead mạng xã hội» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ lead mạng xã hội» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Đồng bộ lead mạng xã hội» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Social media lead sync).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ lead mạng xã hội».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ lead mạng xã hội» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Đồng bộ lead website / landing"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_026 |
| **Tên Use Case** | Đồng bộ lead website / landing |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Đồng bộ lead website / landing" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Website form integration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ lead website / landing» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ lead website / landing» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ lead website / landing» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Đồng bộ lead website / landing» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Website form integration).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ lead website / landing».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ lead website / landing» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 27. Đặc tả Use Case "Đồng bộ kênh khác"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_027 |
| **Tên Use Case** | Đồng bộ kênh khác |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Đồng bộ kênh khác" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Multi-channel lead capture |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ kênh khác» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ kênh khác» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ kênh khác» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Đồng bộ kênh khác» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Multi-channel lead capture).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ kênh khác».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ kênh khác» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 28. Đặc tả Use Case "Attribution nguồn khách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_028 |
| **Tên Use Case** | Attribution nguồn khách |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Attribution nguồn khách" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Source attribution |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Attribution nguồn khách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Attribution nguồn khách» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Attribution nguồn khách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Attribution nguồn khách» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Source attribution).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Attribution nguồn khách».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Attribution nguồn khách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Tính CPL / CAC / ROAS / ROI"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_029 |
| **Tên Use Case** | Tính CPL / CAC / ROAS / ROI |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Tính CPL / CAC / ROAS / ROI" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Marketing metrics |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính CPL / CAC / ROAS / ROI» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính CPL / CAC / ROAS / ROI» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính CPL / CAC / ROAS / ROI» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Tính CPL / CAC / ROAS / ROI» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Marketing metrics).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tính CPL / CAC / ROAS / ROI».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính CPL / CAC / ROAS / ROI» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Funnel marketing đến doanh thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_030 |
| **Tên Use Case** | Funnel marketing đến doanh thu |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Funnel marketing đến doanh thu" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Full marketing funnel |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Funnel marketing đến doanh thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Funnel marketing đến doanh thu» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Funnel marketing đến doanh thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Funnel marketing đến doanh thu» trong nhóm Marketing – nguồn & đo lường.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Full marketing funnel).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Funnel marketing đến doanh thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Funnel marketing đến doanh thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 31. Đặc tả Use Case "Dashboard marketing"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_031 |
| **Tên Use Case** | Dashboard marketing |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Dashboard marketing" thuộc nhóm Marketing – nguồn & đo lường trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Marketing dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard marketing» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard marketing» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard marketing» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer mở «Dashboard marketing» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Marketing dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard marketing» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Khuyến mại & voucher (`CRM-04`)

Nhóm **Khuyến mại & voucher** gồm **7** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 32. Đặc tả Use Case "Tạo chương trình khuyến mại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_032 |
| **Tên Use Case** | Tạo chương trình khuyến mại |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Tạo chương trình khuyến mại" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Promotion setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo chương trình khuyến mại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo chương trình khuyến mại» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo chương trình khuyến mại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer mở chức năng «Tạo chương trình khuyến mại» trong nhóm Khuyến mại & voucher.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo chương trình khuyến mại» (Promotion setup).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo chương trình khuyến mại» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo chương trình khuyến mại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 33. Đặc tả Use Case "Cấu hình điều kiện khuyến mại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_033 |
| **Tên Use Case** | Cấu hình điều kiện khuyến mại |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Cấu hình điều kiện khuyến mại" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Promotion rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình điều kiện khuyến mại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình điều kiện khuyến mại» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình điều kiện khuyến mại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer mở màn hình cấu hình «Cấu hình điều kiện khuyến mại» trong Khuyến mại & voucher.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Promotion rules) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình điều kiện khuyến mại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 34. Đặc tả Use Case "Sinh mã voucher"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_034 |
| **Tên Use Case** | Sinh mã voucher |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Sinh mã voucher" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Voucher generation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sinh mã voucher» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sinh mã voucher» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sinh mã voucher» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer mở chức năng «Sinh mã voucher» trong nhóm Khuyến mại & voucher.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Sinh mã voucher» (Voucher generation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Sinh mã voucher» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sinh mã voucher» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 35. Đặc tả Use Case "Giới hạn lượt dùng voucher"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_035 |
| **Tên Use Case** | Giới hạn lượt dùng voucher |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Giới hạn lượt dùng voucher" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Usage limit control |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giới hạn lượt dùng voucher» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giới hạn lượt dùng voucher» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giới hạn lượt dùng voucher» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Giới hạn lượt dùng voucher» trong nhóm Khuyến mại & voucher.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Usage limit control).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Giới hạn lượt dùng voucher».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giới hạn lượt dùng voucher» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Đồng bộ khuyến mại sang POS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_036 |
| **Tên Use Case** | Đồng bộ khuyến mại sang POS |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Đồng bộ khuyến mại sang POS" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: POS promotion sync |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ khuyến mại sang POS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ khuyến mại sang POS» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ khuyến mại sang POS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Đồng bộ khuyến mại sang POS» trong nhóm Khuyến mại & voucher.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (POS promotion sync).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ khuyến mại sang POS».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ khuyến mại sang POS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 37. Đặc tả Use Case "Áp dụng khuyến mại trên báo giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_037 |
| **Tên Use Case** | Áp dụng khuyến mại trên báo giá |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Áp dụng khuyến mại trên báo giá" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Quote/order promotion |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Áp dụng khuyến mại trên báo giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Áp dụng khuyến mại trên báo giá» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Áp dụng khuyến mại trên báo giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Marketer khởi tạo thao tác «Áp dụng khuyến mại trên báo giá» trong nhóm Khuyến mại & voucher.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Quote/order promotion).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Áp dụng khuyến mại trên báo giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Áp dụng khuyến mại trên báo giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 38. Đặc tả Use Case "Báo cáo sử dụng voucher"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_038 |
| **Tên Use Case** | Báo cáo sử dụng voucher |
| **Tác nhân** | Marketer |
| **Mô tả chức năng** | Cho phép Marketer thực hiện chức năng "Báo cáo sử dụng voucher" thuộc nhóm Khuyến mại & voucher trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Voucher redemption report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Marketer] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo sử dụng voucher» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo sử dụng voucher» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo sử dụng voucher» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Marketer mở «Báo cáo sử dụng voucher» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Voucher redemption report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo sử dụng voucher» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Omnichannel & chatbot (`CRM-05`)

Nhóm **Omnichannel & chatbot** gồm **10** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 1 |

**Bảng 39. Đặc tả Use Case "Hộp thư tập trung đa kênh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_039 |
| **Tên Use Case** | Hộp thư tập trung đa kênh |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Hộp thư tập trung đa kênh" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Unified inbox |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hộp thư tập trung đa kênh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hộp thư tập trung đa kênh» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hộp thư tập trung đa kênh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Hộp thư tập trung đa kênh» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Unified inbox).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hộp thư tập trung đa kênh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hộp thư tập trung đa kênh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 40. Đặc tả Use Case "Tiếp nhận hội thoại mới"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_040 |
| **Tên Use Case** | Tiếp nhận hội thoại mới |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Tiếp nhận hội thoại mới" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Conversation intake |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiếp nhận hội thoại mới» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiếp nhận hội thoại mới» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiếp nhận hội thoại mới» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Tiếp nhận hội thoại mới» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Conversation intake).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiếp nhận hội thoại mới».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiếp nhận hội thoại mới» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 41. Đặc tả Use Case "Phân phối hội thoại theo rule"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_041 |
| **Tên Use Case** | Phân phối hội thoại theo rule |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Phân phối hội thoại theo rule" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Conversation routing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân phối hội thoại theo rule» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân phối hội thoại theo rule» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân phối hội thoại theo rule» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Phân phối hội thoại theo rule» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Conversation routing).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân phối hội thoại theo rule».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân phối hội thoại theo rule» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 42. Đặc tả Use Case "Chuyển hội thoại giữa agent"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_042 |
| **Tên Use Case** | Chuyển hội thoại giữa agent |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Chuyển hội thoại giữa agent" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Transfer conversation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển hội thoại giữa agent» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển hội thoại giữa agent» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển hội thoại giữa agent» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Chuyển hội thoại giữa agent» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Transfer conversation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển hội thoại giữa agent».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển hội thoại giữa agent» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 43. Đặc tả Use Case "SLA phản hồi & cảnh báo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_043 |
| **Tên Use Case** | SLA phản hồi & cảnh báo |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "SLA phản hồi & cảnh báo" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Response SLA tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «SLA phản hồi & cảnh báo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «SLA phản hồi & cảnh báo» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «SLA phản hồi & cảnh báo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Contact Center Agent kích hoạt kiểm tra điều kiện «SLA phản hồi & cảnh báo».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Response SLA tracking).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «SLA phản hồi & cảnh báo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 44. Đặc tả Use Case "Chatbot kịch bản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_044 |
| **Tên Use Case** | Chatbot kịch bản |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Chatbot kịch bản" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Conversation bot flow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chatbot kịch bản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chatbot kịch bản» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chatbot kịch bản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Chatbot kịch bản» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Conversation bot flow).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chatbot kịch bản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chatbot kịch bản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 45. Đặc tả Use Case "Chatbot thu thập lead"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_045 |
| **Tên Use Case** | Chatbot thu thập lead |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Chatbot thu thập lead" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead capture bot |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chatbot thu thập lead» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chatbot thu thập lead» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chatbot thu thập lead» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Chatbot thu thập lead» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead capture bot).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chatbot thu thập lead».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chatbot thu thập lead» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 46. Đặc tả Use Case "Chuyển bot sang agent"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_046 |
| **Tên Use Case** | Chuyển bot sang agent |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Chuyển bot sang agent" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Bot-to-human handoff |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển bot sang agent» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển bot sang agent» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển bot sang agent» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Chuyển bot sang agent» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Bot-to-human handoff).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển bot sang agent».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển bot sang agent» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 47. Đặc tả Use Case "Lưu lịch sử chat"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_047 |
| **Tên Use Case** | Lưu lịch sử chat |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Lưu lịch sử chat" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Conversation history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lưu lịch sử chat» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lưu lịch sử chat» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lưu lịch sử chat» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Contact Center Agent mở «Lưu lịch sử chat» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Conversation history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lưu lịch sử chat» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 48. Đặc tả Use Case "Đánh giá CSAT"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_048 |
| **Tên Use Case** | Đánh giá CSAT |
| **Tác nhân** | Contact Center Agent |
| **Mô tả chức năng** | Cho phép Contact Center Agent thực hiện chức năng "Đánh giá CSAT" thuộc nhóm Omnichannel & chatbot trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Customer satisfaction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Contact Center Agent] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá CSAT» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá CSAT» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá CSAT» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Contact Center Agent khởi tạo thao tác «Đánh giá CSAT» trong nhóm Omnichannel & chatbot.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer satisfaction).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá CSAT».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá CSAT» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Lead (`CRM-06`)

Nhóm **Lead** gồm **13** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 13 |
| Must | 11 |

**Bảng 49. Đặc tả Use Case "Tạo lead thủ công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_049 |
| **Tên Use Case** | Tạo lead thủ công |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tạo lead thủ công" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Manual lead entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo lead thủ công» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo lead thủ công» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo lead thủ công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở chức năng «Tạo lead thủ công» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo lead thủ công» (Manual lead entry).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo lead thủ công» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo lead thủ công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 50. Đặc tả Use Case "Tiếp nhận lead tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_050 |
| **Tên Use Case** | Tiếp nhận lead tự động |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tiếp nhận lead tự động" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Auto lead capture |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiếp nhận lead tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiếp nhận lead tự động» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiếp nhận lead tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Tiếp nhận lead tự động» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Auto lead capture).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiếp nhận lead tự động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiếp nhận lead tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 51. Đặc tả Use Case "Phân bổ lead cho sales"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_051 |
| **Tên Use Case** | Phân bổ lead cho sales |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Phân bổ lead cho sales" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân bổ lead cho sales» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân bổ lead cho sales» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân bổ lead cho sales» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Phân bổ lead cho sales» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead assignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân bổ lead cho sales».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân bổ lead cho sales» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 52. Đặc tả Use Case "Lead scoring"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_052 |
| **Tên Use Case** | Lead scoring |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Lead scoring" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead scoring model |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lead scoring» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lead scoring» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lead scoring» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Lead scoring» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead scoring model).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lead scoring».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lead scoring» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 53. Đặc tả Use Case "Cập nhật trạng thái pipeline"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_053 |
| **Tên Use Case** | Cập nhật trạng thái pipeline |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Cập nhật trạng thái pipeline" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead stage tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cập nhật trạng thái pipeline» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cập nhật trạng thái pipeline» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cập nhật trạng thái pipeline» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở «Cập nhật trạng thái pipeline» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Lead stage tracking).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cập nhật trạng thái pipeline» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 54. Đặc tả Use Case "Task follow-up lead"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_054 |
| **Tên Use Case** | Task follow-up lead |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Task follow-up lead" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead activity task |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Task follow-up lead» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Task follow-up lead» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Task follow-up lead» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Task follow-up lead» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead activity task).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Task follow-up lead».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Task follow-up lead» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 55. Đặc tả Use Case "Nhắc việc follow-up"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_055 |
| **Tên Use Case** | Nhắc việc follow-up |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Nhắc việc follow-up" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Follow-up reminder |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhắc việc follow-up» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhắc việc follow-up» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhắc việc follow-up» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Nhắc việc follow-up» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Follow-up reminder).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhắc việc follow-up».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhắc việc follow-up» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 56. Đặc tả Use Case "Nhật ký chăm sóc lead"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_056 |
| **Tên Use Case** | Nhật ký chăm sóc lead |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Nhật ký chăm sóc lead" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead activity log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhật ký chăm sóc lead» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhật ký chăm sóc lead» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhật ký chăm sóc lead» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Nhật ký chăm sóc lead» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead activity log).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhật ký chăm sóc lead».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhật ký chăm sóc lead» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 57. Đặc tả Use Case "Chuyển lead thành cơ hội"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_057 |
| **Tên Use Case** | Chuyển lead thành cơ hội |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Chuyển lead thành cơ hội" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead qualification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển lead thành cơ hội» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển lead thành cơ hội» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển lead thành cơ hội» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Chuyển lead thành cơ hội» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Lead qualification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển lead thành cơ hội».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển lead thành cơ hội» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 58. Đặc tả Use Case "Đánh dấu lead mất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_058 |
| **Tên Use Case** | Đánh dấu lead mất |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Đánh dấu lead mất" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Mark lead as lost |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh dấu lead mất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh dấu lead mất» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh dấu lead mất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Đánh dấu lead mất» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Mark lead as lost).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh dấu lead mất».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh dấu lead mất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 59. Đặc tả Use Case "Gộp lead trùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_059 |
| **Tên Use Case** | Gộp lead trùng |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Gộp lead trùng" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Merge duplicate leads |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gộp lead trùng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gộp lead trùng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gộp lead trùng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Gộp lead trùng» trong nhóm Lead.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Merge duplicate leads).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gộp lead trùng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gộp lead trùng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 60. Đặc tả Use Case "Import lead Excel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_060 |
| **Tên Use Case** | Import lead Excel |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Import lead Excel" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Bulk lead import |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import lead Excel» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-IMP-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import lead Excel» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import lead Excel» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep tải file mẫu (nếu có) và chọn file import cho «Import lead Excel».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import lead Excel» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn. |

**Bảng 61. Đặc tả Use Case "Báo cáo chuyển đổi lead"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_061 |
| **Tên Use Case** | Báo cáo chuyển đổi lead |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Báo cáo chuyển đổi lead" thuộc nhóm Lead trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Lead conversion report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo chuyển đổi lead» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo chuyển đổi lead» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo chuyển đổi lead» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở «Báo cáo chuyển đổi lead» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Lead conversion report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo chuyển đổi lead» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Cơ hội bán hàng (`CRM-07`)

Nhóm **Cơ hội bán hàng** gồm **8** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 5 |

**Bảng 62. Đặc tả Use Case "Tạo cơ hội từ lead/khách"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_062 |
| **Tên Use Case** | Tạo cơ hội từ lead/khách |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tạo cơ hội từ lead/khách" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Create opportunity |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo cơ hội từ lead/khách» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo cơ hội từ lead/khách» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo cơ hội từ lead/khách» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở chức năng «Tạo cơ hội từ lead/khách» trong nhóm Cơ hội bán hàng.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo cơ hội từ lead/khách» (Create opportunity).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo cơ hội từ lead/khách» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo cơ hội từ lead/khách» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 63. Đặc tả Use Case "Pipeline cơ hội theo giai đoạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_063 |
| **Tên Use Case** | Pipeline cơ hội theo giai đoạn |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Pipeline cơ hội theo giai đoạn" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Opportunity stages |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Pipeline cơ hội theo giai đoạn» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Pipeline cơ hội theo giai đoạn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Pipeline cơ hội theo giai đoạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở «Pipeline cơ hội theo giai đoạn» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Opportunity stages).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Pipeline cơ hội theo giai đoạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 64. Đặc tả Use Case "Dự báo doanh thu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_064 |
| **Tên Use Case** | Dự báo doanh thu |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Dự báo doanh thu" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Revenue forecast |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dự báo doanh thu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dự báo doanh thu» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dự báo doanh thu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Dự báo doanh thu» trong nhóm Cơ hội bán hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Revenue forecast).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Dự báo doanh thu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dự báo doanh thu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 65. Đặc tả Use Case "Gắn sản phẩm / giá trị ước tính"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_065 |
| **Tên Use Case** | Gắn sản phẩm / giá trị ước tính |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Gắn sản phẩm / giá trị ước tính" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Opportunity lines |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn sản phẩm / giá trị ước tính» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu nguồn (công, tồn, tỷ giá…) đã sẵn sàng và đạt điều kiện chốt. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-CALC-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn sản phẩm / giá trị ước tính» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn sản phẩm / giá trị ước tính» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Kết quả tính toán tái lập được với cùng input/rule (deterministic trong cùng phiên bản rule).<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep chọn phạm vi tính toán cho «Gắn sản phẩm / giá trị ước tính» (kỳ, đơn vị, bộ lọc).<br>2. Hệ thống nạp dữ liệu nguồn liên quan (Opportunity lines).<br>3. Chạy engine tính theo rule cấu hình; log chi tiết từng bước lỗi nếu có.<br>4. Hiển thị kết quả nháp để rà soát; cho phép điều chỉnh có audit trước khi chốt.<br>5. Xác nhận ghi nhận kết quả chính thức; phát sự kiện cho FIN/module liên quan nếu cần.<br>6. Thông báo hoàn tất và cập nhật trạng thái kỳ/tính toán. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn sản phẩm / giá trị ước tính» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thiếu dữ liệu nguồn hoặc rule cấu hình không đầy đủ → dừng job, liệt kê lỗi chi tiết để sửa. |

**Bảng 66. Đặc tả Use Case "Đối thủ / ghi chú đàm phán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_066 |
| **Tên Use Case** | Đối thủ / ghi chú đàm phán |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Đối thủ / ghi chú đàm phán" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Competitor notes |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối thủ / ghi chú đàm phán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối thủ / ghi chú đàm phán» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối thủ / ghi chú đàm phán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Đối thủ / ghi chú đàm phán» trong nhóm Cơ hội bán hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Competitor notes).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối thủ / ghi chú đàm phán».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối thủ / ghi chú đàm phán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 67. Đặc tả Use Case "Chuyển cơ hội sang báo giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_067 |
| **Tên Use Case** | Chuyển cơ hội sang báo giá |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Chuyển cơ hội sang báo giá" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Create quote from opportunity |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển cơ hội sang báo giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển cơ hội sang báo giá» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển cơ hội sang báo giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Chuyển cơ hội sang báo giá» trong nhóm Cơ hội bán hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Create quote from opportunity).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển cơ hội sang báo giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển cơ hội sang báo giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 68. Đặc tả Use Case "Đóng thắng / thua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_068 |
| **Tên Use Case** | Đóng thắng / thua |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Đóng thắng / thua" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Close won/lost |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng thắng / thua» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng thắng / thua» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng thắng / thua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Đóng thắng / thua» trong nhóm Cơ hội bán hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Close won/lost).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng thắng / thua».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng thắng / thua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 69. Đặc tả Use Case "Báo cáo win-rate"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_069 |
| **Tên Use Case** | Báo cáo win-rate |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Báo cáo win-rate" thuộc nhóm Cơ hội bán hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Win rate analytics |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo win-rate» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo win-rate» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo win-rate» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep mở «Báo cáo win-rate» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Win rate analytics); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo win-rate» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.8. Báo giá (`CRM-08`)

Nhóm **Báo giá** gồm **9** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 6 |

**Bảng 70. Đặc tả Use Case "Tạo báo giá từ cơ hội"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_070 |
| **Tên Use Case** | Tạo báo giá từ cơ hội |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tạo báo giá từ cơ hội" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Create quote |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo báo giá từ cơ hội» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo báo giá từ cơ hội» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo báo giá từ cơ hội» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở chức năng «Tạo báo giá từ cơ hội» trong nhóm Báo giá.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo báo giá từ cơ hội» (Create quote).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo báo giá từ cơ hội» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo báo giá từ cơ hội» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 71. Đặc tả Use Case "Thêm dòng sản phẩm / dịch vụ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_071 |
| **Tên Use Case** | Thêm dòng sản phẩm / dịch vụ |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Thêm dòng sản phẩm / dịch vụ" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Quote line items |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thêm dòng sản phẩm / dịch vụ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thêm dòng sản phẩm / dịch vụ» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thêm dòng sản phẩm / dịch vụ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở chức năng «Thêm dòng sản phẩm / dịch vụ» trong nhóm Báo giá.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Thêm dòng sản phẩm / dịch vụ» (Quote line items).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Thêm dòng sản phẩm / dịch vụ» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thêm dòng sản phẩm / dịch vụ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 72. Đặc tả Use Case "Áp chính sách giá / bảng giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_072 |
| **Tên Use Case** | Áp chính sách giá / bảng giá |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Áp chính sách giá / bảng giá" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Price list application |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Áp chính sách giá / bảng giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Áp chính sách giá / bảng giá» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Áp chính sách giá / bảng giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Áp chính sách giá / bảng giá» trong nhóm Báo giá.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Price list application).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Áp chính sách giá / bảng giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Áp chính sách giá / bảng giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 73. Đặc tả Use Case "Xin duyệt chiết khấu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_073 |
| **Tên Use Case** | Xin duyệt chiết khấu |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Xin duyệt chiết khấu" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Discount approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xin duyệt chiết khấu» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xin duyệt chiết khấu» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xin duyệt chiết khấu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở hộp chờ / chứng từ cần xử lý cho «Xin duyệt chiết khấu».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Xin duyệt chiết khấu», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xin duyệt chiết khấu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 74. Đặc tả Use Case "Gửi báo giá PDF/email"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_074 |
| **Tên Use Case** | Gửi báo giá PDF/email |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Gửi báo giá PDF/email" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Send quote to customer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gửi báo giá PDF/email» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gửi báo giá PDF/email» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi báo giá PDF/email» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep hoàn thiện dữ liệu cho «Gửi báo giá PDF/email» ở trạng thái nháp.<br>2. Chọn [Gửi duyệt / Xác nhận] (submit).<br>3. Hệ thống validate đủ điều kiện gửi; chuyển trạng thái Submitted/In Approval.<br>4. Tạo việc duyệt (WF hoặc duyệt nội module); gửi thông báo.<br>5. Khóa sửa một phần theo policy khi đang chờ duyệt. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gửi báo giá PDF/email» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 75. Đặc tả Use Case "Phiên bản báo giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_075 |
| **Tên Use Case** | Phiên bản báo giá |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Phiên bản báo giá" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Quote versioning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiên bản báo giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiên bản báo giá» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiên bản báo giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Phiên bản báo giá» trong nhóm Báo giá.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Quote versioning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phiên bản báo giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiên bản báo giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 76. Đặc tả Use Case "Hết hạn báo giá tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_076 |
| **Tên Use Case** | Hết hạn báo giá tự động |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Hết hạn báo giá tự động" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Quote expiry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hết hạn báo giá tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hết hạn báo giá tự động» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hết hạn báo giá tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Hết hạn báo giá tự động» trong nhóm Báo giá.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Quote expiry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hết hạn báo giá tự động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hết hạn báo giá tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 77. Đặc tả Use Case "Chuyển báo giá thành đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_077 |
| **Tên Use Case** | Chuyển báo giá thành đơn |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Chuyển báo giá thành đơn" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Quote to order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển báo giá thành đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển báo giá thành đơn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển báo giá thành đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Chuyển báo giá thành đơn» trong nhóm Báo giá.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Quote to order).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển báo giá thành đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển báo giá thành đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 78. Đặc tả Use Case "In mẫu báo giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_078 |
| **Tên Use Case** | In mẫu báo giá |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "In mẫu báo giá" thuộc nhóm Báo giá trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Quote template printing |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In mẫu báo giá» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In mẫu báo giá» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In mẫu báo giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. Sales Rep mở «In mẫu báo giá», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «In mẫu báo giá» (Quote template printing).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In mẫu báo giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.9. Sales Online / đơn hàng (`CRM-09`)

Nhóm **Sales Online / đơn hàng** gồm **10** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 6 |

**Bảng 79. Đặc tả Use Case "Tạo đơn hàng từ báo giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_079 |
| **Tên Use Case** | Tạo đơn hàng từ báo giá |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tạo đơn hàng từ báo giá" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Create order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đơn hàng từ báo giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đơn hàng từ báo giá» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đơn hàng từ báo giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở chức năng «Tạo đơn hàng từ báo giá» trong nhóm Sales Online / đơn hàng.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đơn hàng từ báo giá» (Create order).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đơn hàng từ báo giá» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đơn hàng từ báo giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 80. Đặc tả Use Case "Tiếp nhận đơn từ kênh online"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_080 |
| **Tên Use Case** | Tiếp nhận đơn từ kênh online |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tiếp nhận đơn từ kênh online" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: E-commerce order intake |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiếp nhận đơn từ kênh online» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiếp nhận đơn từ kênh online» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiếp nhận đơn từ kênh online» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Tiếp nhận đơn từ kênh online» trong nhóm Sales Online / đơn hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (E-commerce order intake).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiếp nhận đơn từ kênh online».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiếp nhận đơn từ kênh online» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 81. Đặc tả Use Case "Cập nhật trạng thái đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_081 |
| **Tên Use Case** | Cập nhật trạng thái đơn |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Cập nhật trạng thái đơn" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Order workflow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cập nhật trạng thái đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cập nhật trạng thái đơn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cập nhật trạng thái đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep tìm và mở bản ghi liên quan tới «Cập nhật trạng thái đơn» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Cập nhật trạng thái đơn» (Order workflow).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cập nhật trạng thái đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 82. Đặc tả Use Case "Giữ tồn khi duyệt đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_082 |
| **Tên Use Case** | Giữ tồn khi duyệt đơn |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Giữ tồn khi duyệt đơn" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Stock reservation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Giữ tồn khi duyệt đơn» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Giữ tồn khi duyệt đơn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Giữ tồn khi duyệt đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep mở hộp chờ / chứng từ cần xử lý cho «Giữ tồn khi duyệt đơn».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Giữ tồn khi duyệt đơn», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Giữ tồn khi duyệt đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 83. Đặc tả Use Case "Tách / gộp đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_083 |
| **Tên Use Case** | Tách / gộp đơn |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Tách / gộp đơn" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Split/merge orders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tách / gộp đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tách / gộp đơn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tách / gộp đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Tách / gộp đơn» trong nhóm Sales Online / đơn hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Split/merge orders).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tách / gộp đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tách / gộp đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 84. Đặc tả Use Case "Hủy đơn có kiểm soát"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_084 |
| **Tên Use Case** | Hủy đơn có kiểm soát |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Hủy đơn có kiểm soát" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Order cancellation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy đơn có kiểm soát» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-CAN-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy đơn có kiểm soát» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy đơn có kiểm soát» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep chọn đối tượng cần hủy/ngưng trong «Hủy đơn có kiểm soát».<br>2. Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.<br>3. Yêu cầu lý do; xác nhận cảnh báo tác động.<br>4. Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.<br>5. Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy đơn có kiểm soát» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 85. Đặc tả Use Case "Trả hàng / điều chỉnh đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_085 |
| **Tên Use Case** | Trả hàng / điều chỉnh đơn |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Trả hàng / điều chỉnh đơn" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Return/adjustment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trả hàng / điều chỉnh đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trả hàng / điều chỉnh đơn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trả hàng / điều chỉnh đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep tìm và mở bản ghi liên quan tới «Trả hàng / điều chỉnh đơn» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Trả hàng / điều chỉnh đơn» (Return/adjustment).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trả hàng / điều chỉnh đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 86. Đặc tả Use Case "Gắn hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_086 |
| **Tên Use Case** | Gắn hợp đồng |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Gắn hợp đồng" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Link contract |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn hợp đồng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Gắn hợp đồng» trong nhóm Sales Online / đơn hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Link contract).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn hợp đồng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 87. Đặc tả Use Case "Theo dõi thanh toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_087 |
| **Tên Use Case** | Theo dõi thanh toán |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Theo dõi thanh toán" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Payment tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi thanh toán» đã được cấu hình trong phạm vi data scope.<br>• Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-PAY-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi thanh toán» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi thanh toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep chọn chứng từ cần thu/chi trong «Theo dõi thanh toán».<br>2. Nhập phương thức, số tiền, tham chiếu giao dịch.<br>3. Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.<br>4. Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.<br>5. Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi thanh toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận. |

**Bảng 88. Đặc tả Use Case "Đẩy đơn sang kho / giao vận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_088 |
| **Tên Use Case** | Đẩy đơn sang kho / giao vận |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Đẩy đơn sang kho / giao vận" thuộc nhóm Sales Online / đơn hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Fulfillment handoff |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đẩy đơn sang kho / giao vận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đẩy đơn sang kho / giao vận» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đẩy đơn sang kho / giao vận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Đẩy đơn sang kho / giao vận» trong nhóm Sales Online / đơn hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Fulfillment handoff).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đẩy đơn sang kho / giao vận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đẩy đơn sang kho / giao vận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.10. Sales Offline / Route sales (`CRM-10`)

Nhóm **Sales Offline / Route sales** gồm **10** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 0 |

**Bảng 89. Đặc tả Use Case "Phân vùng / tuyến bán hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_089 |
| **Tên Use Case** | Phân vùng / tuyến bán hàng |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Phân vùng / tuyến bán hàng" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Territory/route planning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân vùng / tuyến bán hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân vùng / tuyến bán hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân vùng / tuyến bán hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Phân vùng / tuyến bán hàng» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Territory/route planning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân vùng / tuyến bán hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân vùng / tuyến bán hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 90. Đặc tả Use Case "Phân loại tần suất visit"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_090 |
| **Tên Use Case** | Phân loại tần suất visit |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Phân loại tần suất visit" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Visit frequency planning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại tần suất visit» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại tần suất visit» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại tần suất visit» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Phân loại tần suất visit» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Visit frequency planning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại tần suất visit».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại tần suất visit» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 91. Đặc tả Use Case "Lập kế hoạch visit"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_091 |
| **Tên Use Case** | Lập kế hoạch visit |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Lập kế hoạch visit" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Visit schedule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lập kế hoạch visit» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lập kế hoạch visit» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lập kế hoạch visit» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Lập kế hoạch visit» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Visit schedule).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lập kế hoạch visit».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lập kế hoạch visit» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 92. Đặc tả Use Case "Check-in / check-out GPS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_092 |
| **Tên Use Case** | Check-in / check-out GPS |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Check-in / check-out GPS" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Field visit tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Check-in / check-out GPS» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Check-in / check-out GPS» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Check-in / check-out GPS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep mở chứng từ nhận liên quan «Check-in / check-out GPS».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Check-in / check-out GPS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý. |

**Bảng 93. Đặc tả Use Case "Ghi nhận mục đích – kết quả visit"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_093 |
| **Tên Use Case** | Ghi nhận mục đích – kết quả visit |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Ghi nhận mục đích – kết quả visit" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Visit report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận mục đích – kết quả visit» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận mục đích – kết quả visit» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận mục đích – kết quả visit» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Ghi nhận mục đích – kết quả visit» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Visit report).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận mục đích – kết quả visit».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận mục đích – kết quả visit» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 94. Đặc tả Use Case "Ghi nhận nhu cầu khách hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_094 |
| **Tên Use Case** | Ghi nhận nhu cầu khách hàng |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Ghi nhận nhu cầu khách hàng" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Customer need capture |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận nhu cầu khách hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận nhu cầu khách hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận nhu cầu khách hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Ghi nhận nhu cầu khách hàng» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer need capture).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận nhu cầu khách hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận nhu cầu khách hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 95. Đặc tả Use Case "Đặt hàng tại điểm thăm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_095 |
| **Tên Use Case** | Đặt hàng tại điểm thăm |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Đặt hàng tại điểm thăm" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Order at visit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đặt hàng tại điểm thăm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đặt hàng tại điểm thăm» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đặt hàng tại điểm thăm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «Đặt hàng tại điểm thăm» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Order at visit).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đặt hàng tại điểm thăm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đặt hàng tại điểm thăm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 96. Đặc tả Use Case "Xem lịch sử visit"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_096 |
| **Tên Use Case** | Xem lịch sử visit |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Xem lịch sử visit" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Visit history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem lịch sử visit» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem lịch sử visit» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem lịch sử visit» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep mở «Xem lịch sử visit» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Visit history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem lịch sử visit» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 97. Đặc tả Use Case "AI gợi ý việc ưu tiên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_097 |
| **Tên Use Case** | AI gợi ý việc ưu tiên |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "AI gợi ý việc ưu tiên" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: AI daily coaching |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «AI gợi ý việc ưu tiên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «AI gợi ý việc ưu tiên» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «AI gợi ý việc ưu tiên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep khởi tạo thao tác «AI gợi ý việc ưu tiên» trong nhóm Sales Offline / Route sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (AI daily coaching).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «AI gợi ý việc ưu tiên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «AI gợi ý việc ưu tiên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 98. Đặc tả Use Case "Dashboard doanh số field"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_098 |
| **Tên Use Case** | Dashboard doanh số field |
| **Tác nhân** | Sales Rep |
| **Mô tả chức năng** | Cho phép Sales Rep thực hiện chức năng "Dashboard doanh số field" thuộc nhóm Sales Offline / Route sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Field sales dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Rep] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard doanh số field» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard doanh số field» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard doanh số field» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Rep mở «Dashboard doanh số field» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Field sales dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard doanh số field» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.11. Sales Admin (`CRM-11`)

Nhóm **Sales Admin** gồm **7** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 4 |

**Bảng 99. Đặc tả Use Case "Hàng đợi đơn cần xử lý"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_099 |
| **Tên Use Case** | Hàng đợi đơn cần xử lý |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Hàng đợi đơn cần xử lý" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Order processing queue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hàng đợi đơn cần xử lý» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hàng đợi đơn cần xử lý» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hàng đợi đơn cần xử lý» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Hàng đợi đơn cần xử lý» trong nhóm Sales Admin.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Order processing queue).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hàng đợi đơn cần xử lý».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hàng đợi đơn cần xử lý» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 100. Đặc tả Use Case "Kiểm tra tồn / xác nhận giữ hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_100 |
| **Tên Use Case** | Kiểm tra tồn / xác nhận giữ hàng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Kiểm tra tồn / xác nhận giữ hàng" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Stock availability check |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm tra tồn / xác nhận giữ hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm tra tồn / xác nhận giữ hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm tra tồn / xác nhận giữ hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Kiểm tra tồn / xác nhận giữ hàng» trong nhóm Sales Admin.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Stock availability check).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm tra tồn / xác nhận giữ hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm tra tồn / xác nhận giữ hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 101. Đặc tả Use Case "Soạn lệnh xuất / giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_101 |
| **Tên Use Case** | Soạn lệnh xuất / giao |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Soạn lệnh xuất / giao" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Prepare fulfillment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Soạn lệnh xuất / giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Soạn lệnh xuất / giao» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Soạn lệnh xuất / giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Soạn lệnh xuất / giao» trong nhóm Sales Admin.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Prepare fulfillment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Soạn lệnh xuất / giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Soạn lệnh xuất / giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 102. Đặc tả Use Case "Đối soát chứng từ đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_102 |
| **Tên Use Case** | Đối soát chứng từ đơn |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Đối soát chứng từ đơn" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Document reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát chứng từ đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát chứng từ đơn» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát chứng từ đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Đối soát chứng từ đơn» trong nhóm Sales Admin.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Document reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát chứng từ đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát chứng từ đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 103. Đặc tả Use Case "Xử lý khiếu nại đơn hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_103 |
| **Tên Use Case** | Xử lý khiếu nại đơn hàng |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Xử lý khiếu nại đơn hàng" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Order complaint handling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xử lý khiếu nại đơn hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xử lý khiếu nại đơn hàng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xử lý khiếu nại đơn hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Xử lý khiếu nại đơn hàng» trong nhóm Sales Admin.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Order complaint handling).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xử lý khiếu nại đơn hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xử lý khiếu nại đơn hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 104. Đặc tả Use Case "Theo dõi đơn chậm xử lý"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_104 |
| **Tên Use Case** | Theo dõi đơn chậm xử lý |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Theo dõi đơn chậm xử lý" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Aging orders tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi đơn chậm xử lý» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi đơn chậm xử lý» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi đơn chậm xử lý» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Admin khởi tạo thao tác «Theo dõi đơn chậm xử lý» trong nhóm Sales Admin.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Aging orders tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi đơn chậm xử lý».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi đơn chậm xử lý» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 105. Đặc tả Use Case "Báo cáo năng suất Sales Admin"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_105 |
| **Tên Use Case** | Báo cáo năng suất Sales Admin |
| **Tác nhân** | Sales Admin |
| **Mô tả chức năng** | Cho phép Sales Admin thực hiện chức năng "Báo cáo năng suất Sales Admin" thuộc nhóm Sales Admin trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Admin productivity |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Admin] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo năng suất Sales Admin» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo năng suất Sales Admin» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo năng suất Sales Admin» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Admin mở «Báo cáo năng suất Sales Admin» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Admin productivity); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo năng suất Sales Admin» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.12. Hợp đồng & chính sách bán (`CRM-12`)

Nhóm **Hợp đồng & chính sách bán** gồm **6** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 2 |

**Bảng 106. Đặc tả Use Case "Quản lý hợp đồng bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_106 |
| **Tên Use Case** | Quản lý hợp đồng bán |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Quản lý hợp đồng bán" thuộc nhóm Hợp đồng & chính sách bán trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Sales contract management |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý hợp đồng bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý hợp đồng bán» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý hợp đồng bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager mở danh mục quản lý «Quản lý hợp đồng bán» (khách hàng / cơ hội / báo giá – đơn hàng; nhóm «Hợp đồng & chính sách bán»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý hợp đồng bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 107. Đặc tả Use Case "Đính kèm file hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_107 |
| **Tên Use Case** | Đính kèm file hợp đồng |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Đính kèm file hợp đồng" thuộc nhóm Hợp đồng & chính sách bán trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Contract attachments |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đính kèm file hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đính kèm file hợp đồng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đính kèm file hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager mở bản ghi liên quan và chọn «Đính kèm file hợp đồng».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đính kèm file hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 108. Đặc tả Use Case "Theo dõi hiệu lực / tái tục"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_108 |
| **Tên Use Case** | Theo dõi hiệu lực / tái tục |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Theo dõi hiệu lực / tái tục" thuộc nhóm Hợp đồng & chính sách bán trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Contract renewal tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi hiệu lực / tái tục» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi hiệu lực / tái tục» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi hiệu lực / tái tục» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Theo dõi hiệu lực / tái tục» trong nhóm Hợp đồng & chính sách bán.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Contract renewal tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi hiệu lực / tái tục».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi hiệu lực / tái tục» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 109. Đặc tả Use Case "Chính sách giá theo nhóm KH"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_109 |
| **Tên Use Case** | Chính sách giá theo nhóm KH |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Chính sách giá theo nhóm KH" thuộc nhóm Hợp đồng & chính sách bán trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Customer pricing policy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chính sách giá theo nhóm KH» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chính sách giá theo nhóm KH» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chính sách giá theo nhóm KH» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Chính sách giá theo nhóm KH» trong nhóm Hợp đồng & chính sách bán.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Customer pricing policy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chính sách giá theo nhóm KH».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chính sách giá theo nhóm KH» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 110. Đặc tả Use Case "Chính sách công nợ / hạn mức"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_110 |
| **Tên Use Case** | Chính sách công nợ / hạn mức |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Chính sách công nợ / hạn mức" thuộc nhóm Hợp đồng & chính sách bán trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Credit limit policy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chính sách công nợ / hạn mức» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chính sách công nợ / hạn mức» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chính sách công nợ / hạn mức» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Chính sách công nợ / hạn mức» trong nhóm Hợp đồng & chính sách bán.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Credit limit policy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chính sách công nợ / hạn mức».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chính sách công nợ / hạn mức» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 111. Đặc tả Use Case "Chặn bán khi vượt công nợ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_111 |
| **Tên Use Case** | Chặn bán khi vượt công nợ |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Chặn bán khi vượt công nợ" thuộc nhóm Hợp đồng & chính sách bán trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Credit block enforcement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chặn bán khi vượt công nợ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chặn bán khi vượt công nợ» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chặn bán khi vượt công nợ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Chặn bán khi vượt công nợ» trong nhóm Hợp đồng & chính sách bán.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Credit block enforcement).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chặn bán khi vượt công nợ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chặn bán khi vượt công nợ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.13. Chăm sóc khách hàng (`CRM-13`)

Nhóm **Chăm sóc khách hàng** gồm **8** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 2 |

**Bảng 112. Đặc tả Use Case "Tạo ticket hỗ trợ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_112 |
| **Tên Use Case** | Tạo ticket hỗ trợ |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Tạo ticket hỗ trợ" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Support ticket creation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo ticket hỗ trợ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo ticket hỗ trợ» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo ticket hỗ trợ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CSKH mở chức năng «Tạo ticket hỗ trợ» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo ticket hỗ trợ» (Support ticket creation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo ticket hỗ trợ» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo ticket hỗ trợ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 113. Đặc tả Use Case "Phân loại khiếu nại / yêu cầu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_113 |
| **Tên Use Case** | Phân loại khiếu nại / yêu cầu |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Phân loại khiếu nại / yêu cầu" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Ticket categorization |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại khiếu nại / yêu cầu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại khiếu nại / yêu cầu» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại khiếu nại / yêu cầu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. CSKH khởi tạo thao tác «Phân loại khiếu nại / yêu cầu» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Ticket categorization).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại khiếu nại / yêu cầu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại khiếu nại / yêu cầu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 114. Đặc tả Use Case "Chuyển ticket sang FSM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_114 |
| **Tên Use Case** | Chuyển ticket sang FSM |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Chuyển ticket sang FSM" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Escalate to field service |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển ticket sang FSM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển ticket sang FSM» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển ticket sang FSM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CSKH khởi tạo thao tác «Chuyển ticket sang FSM» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Escalate to field service).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển ticket sang FSM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển ticket sang FSM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 115. Đặc tả Use Case "Lịch chăm sóc / nhắc tái mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_115 |
| **Tên Use Case** | Lịch chăm sóc / nhắc tái mua |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Lịch chăm sóc / nhắc tái mua" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Care schedule & reminders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch chăm sóc / nhắc tái mua» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch chăm sóc / nhắc tái mua» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch chăm sóc / nhắc tái mua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CSKH khởi tạo thao tác «Lịch chăm sóc / nhắc tái mua» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Care schedule & reminders).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lịch chăm sóc / nhắc tái mua».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch chăm sóc / nhắc tái mua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 116. Đặc tả Use Case "Chương trình loyalty"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_116 |
| **Tên Use Case** | Chương trình loyalty |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Chương trình loyalty" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Loyalty program |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chương trình loyalty» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chương trình loyalty» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chương trình loyalty» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CSKH khởi tạo thao tác «Chương trình loyalty» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Loyalty program).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chương trình loyalty».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chương trình loyalty» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 117. Đặc tả Use Case "Tích điểm / đổi quà"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_117 |
| **Tên Use Case** | Tích điểm / đổi quà |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Tích điểm / đổi quà" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Points & rewards |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tích điểm / đổi quà» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tích điểm / đổi quà» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tích điểm / đổi quà» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CSKH khởi tạo thao tác «Tích điểm / đổi quà» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Points & rewards).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tích điểm / đổi quà».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tích điểm / đổi quà» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 118. Đặc tả Use Case "Khảo sát hài lòng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_118 |
| **Tên Use Case** | Khảo sát hài lòng |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Khảo sát hài lòng" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: CSAT/NPS surveys |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khảo sát hài lòng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khảo sát hài lòng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khảo sát hài lòng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CSKH khởi tạo thao tác «Khảo sát hài lòng» trong nhóm Chăm sóc khách hàng.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (CSAT/NPS surveys).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khảo sát hài lòng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khảo sát hài lòng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 119. Đặc tả Use Case "Báo cáo retention / tái mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_119 |
| **Tên Use Case** | Báo cáo retention / tái mua |
| **Tác nhân** | CSKH |
| **Mô tả chức năng** | Cho phép CSKH thực hiện chức năng "Báo cáo retention / tái mua" thuộc nhóm Chăm sóc khách hàng trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Customer retention |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [CSKH] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo retention / tái mua» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo retention / tái mua» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo retention / tái mua» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. CSKH mở «Báo cáo retention / tái mua» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Customer retention); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo retention / tái mua» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.14. Hoa hồng & KPI sales (`CRM-14`)

Nhóm **Hoa hồng & KPI sales** gồm **6** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 1 |

**Bảng 120. Đặc tả Use Case "Cấu hình rule hoa hồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_120 |
| **Tên Use Case** | Cấu hình rule hoa hồng |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Cấu hình rule hoa hồng" thuộc nhóm Hoa hồng & KPI sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Commission rules setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình rule hoa hồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình rule hoa hồng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình rule hoa hồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager mở màn hình cấu hình «Cấu hình rule hoa hồng» trong Hoa hồng & KPI sales.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Commission rules setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình rule hoa hồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 121. Đặc tả Use Case "Tính hoa hồng theo kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_121 |
| **Tên Use Case** | Tính hoa hồng theo kỳ |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Tính hoa hồng theo kỳ" thuộc nhóm Hoa hồng & KPI sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Commission calculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính hoa hồng theo kỳ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính hoa hồng theo kỳ» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính hoa hồng theo kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Tính hoa hồng theo kỳ» trong nhóm Hoa hồng & KPI sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Commission calculation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tính hoa hồng theo kỳ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính hoa hồng theo kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 122. Đặc tả Use Case "Duyệt bảng hoa hồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_122 |
| **Tên Use Case** | Duyệt bảng hoa hồng |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Duyệt bảng hoa hồng" thuộc nhóm Hoa hồng & KPI sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Commission approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt bảng hoa hồng» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`, `BR-CRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt bảng hoa hồng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt bảng hoa hồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Sales Manager mở hộp chờ / chứng từ cần xử lý cho «Duyệt bảng hoa hồng».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt bảng hoa hồng», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt bảng hoa hồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 123. Đặc tả Use Case "Đồng bộ hoa hồng sang HRM/FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_123 |
| **Tên Use Case** | Đồng bộ hoa hồng sang HRM/FIN |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Đồng bộ hoa hồng sang HRM/FIN" thuộc nhóm Hoa hồng & KPI sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Commission posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ hoa hồng sang HRM/FIN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ hoa hồng sang HRM/FIN» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ hoa hồng sang HRM/FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Đồng bộ hoa hồng sang HRM/FIN» trong nhóm Hoa hồng & KPI sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Commission posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ hoa hồng sang HRM/FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ hoa hồng sang HRM/FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 124. Đặc tả Use Case "KPI doanh số theo nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_124 |
| **Tên Use Case** | KPI doanh số theo nhân viên |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "KPI doanh số theo nhân viên" thuộc nhóm Hoa hồng & KPI sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Sales KPI tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «KPI doanh số theo nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «KPI doanh số theo nhân viên» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «KPI doanh số theo nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «KPI doanh số theo nhân viên» trong nhóm Hoa hồng & KPI sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Sales KPI tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «KPI doanh số theo nhân viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «KPI doanh số theo nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 125. Đặc tả Use Case "Bảng xếp hạng sales"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_125 |
| **Tên Use Case** | Bảng xếp hạng sales |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Bảng xếp hạng sales" thuộc nhóm Hoa hồng & KPI sales trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Sales leaderboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng xếp hạng sales» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng xếp hạng sales» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng xếp hạng sales» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager khởi tạo thao tác «Bảng xếp hạng sales» trong nhóm Hoa hồng & KPI sales.<br>2. Hệ thống kiểm tra license `CRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Sales leaderboard).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng xếp hạng sales».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng xếp hạng sales» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.15. Báo cáo CRM (`CRM-15`)

Nhóm **Báo cáo CRM** gồm **6** use case của module `CRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 126. Đặc tả Use Case "Dashboard Ban lãnh đạo sales"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_126 |
| **Tên Use Case** | Dashboard Ban lãnh đạo sales |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Dashboard Ban lãnh đạo sales" thuộc nhóm Báo cáo CRM trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Executive sales dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard Ban lãnh đạo sales» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard Ban lãnh đạo sales» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard Ban lãnh đạo sales» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager mở «Dashboard Ban lãnh đạo sales» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Executive sales dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard Ban lãnh đạo sales» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 127. Đặc tả Use Case "Báo cáo pipeline & forecast"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_127 |
| **Tên Use Case** | Báo cáo pipeline & forecast |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Báo cáo pipeline & forecast" thuộc nhóm Báo cáo CRM trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Pipeline report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo pipeline & forecast» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo pipeline & forecast» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo pipeline & forecast» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager mở «Báo cáo pipeline & forecast» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Pipeline report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo pipeline & forecast» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 128. Đặc tả Use Case "Báo cáo theo nguồn / campaign"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_128 |
| **Tên Use Case** | Báo cáo theo nguồn / campaign |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Báo cáo theo nguồn / campaign" thuộc nhóm Báo cáo CRM trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Source performance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo theo nguồn / campaign» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo theo nguồn / campaign» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo theo nguồn / campaign» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager mở «Báo cáo theo nguồn / campaign» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Source performance); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo theo nguồn / campaign» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 129. Đặc tả Use Case "Báo cáo theo nhân viên / vùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_129 |
| **Tên Use Case** | Báo cáo theo nhân viên / vùng |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Báo cáo theo nhân viên / vùng" thuộc nhóm Báo cáo CRM trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Multi-dimensional sales |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo theo nhân viên / vùng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo theo nhân viên / vùng» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo theo nhân viên / vùng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Sales Manager mở «Báo cáo theo nhân viên / vùng» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Multi-dimensional sales); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo theo nhân viên / vùng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 130. Đặc tả Use Case "Báo cáo công nợ bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_130 |
| **Tên Use Case** | Báo cáo công nợ bán |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Báo cáo công nợ bán" thuộc nhóm Báo cáo CRM trong module CRM — CRM & Bán hàng. Mô tả chi tiết: AR view from CRM |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo công nợ bán» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo công nợ bán» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo công nợ bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Sales Manager mở «Báo cáo công nợ bán» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (AR view from CRM); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo công nợ bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 131. Đặc tả Use Case "Xuất báo cáo định kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_CRM_131 |
| **Tên Use Case** | Xuất báo cáo định kỳ |
| **Tác nhân** | Sales Manager |
| **Mô tả chức năng** | Cho phép Sales Manager thực hiện chức năng "Xuất báo cáo định kỳ" thuộc nhóm Báo cáo CRM trong module CRM — CRM & Bán hàng. Mô tả chi tiết: Scheduled report export |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Sales Manager] và được cấp quyền RBAC tương ứng.<br>• License module `CRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo định kỳ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-CRM-SCOPE-01`, `BR-CRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo định kỳ» được lưu nhất quán trong module `CRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo định kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. Sales Manager mở «Xuất báo cáo định kỳ», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo định kỳ» (Scheduled report export).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo định kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-CRM-01 — Lead-to-Order

**Mục tiêu:** Từ lead đến đơn hàng đã xác nhận

| Bước | Mô tả |
|---:|---|
| 1 | Tiếp nhận lead (thủ công/tự động) |
| 2 | Phân bổ sales; scoring; follow-up |
| 3 | Qualify thành cơ hội |
| 4 | Lập báo giá; duyệt chiết khấu nếu vượt khung |
| 5 | Khách chấp nhận → tạo đơn |
| 6 | Kiểm tra tín dụng/tồn; reserve; chuyển fulfillment |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-CRM-02 — Campaign → đo ROI

**Mục tiêu:** Gắn chi phí marketing với lead/doanh thu

| Bước | Mô tả |
|---:|---|
| 1 | Tạo campaign và ngân sách |
| 2 | Thu lead theo nguồn/attribution |
| 3 | Chăm sóc và chuyển đổi |
| 4 | Ghi nhận doanh thu đơn gắn campaign |
| 5 | Báo cáo CPL/CAC/ROI |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-CRM-03 — Visit bán hàng hiện trường

**Mục tiêu:** Thực hiện kế hoạch thăm và chốt nhu cầu/đơn

| Bước | Mô tả |
|---:|---|
| 1 | Lập kế hoạch visit theo tệp KH |
| 2 | Check-in GPS; ghi nhận kết quả |
| 3 | Tạo hoạt động/cơ hội/đơn tại điểm |
| 4 | Đồng bộ về pipeline và dashboard |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Customer / Contact` | Khách & người liên hệ |
| `Campaign / Promo / Voucher` | Marketing & KM |
| `Conversation` | Hội thoại omnichannel |
| `Lead / Opportunity` | Phễu bán |
| `Quote / SalesOrder` | Báo giá & đơn |
| `VisitPlan / VisitLog` | Bán hiện trường |
| `SupportCase` | CSKH |
| `CommissionRule / CommissionSheet` | Hoa hồng |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `CRM`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-CRM-01: Không tạo trùng KH theo rule SĐT/MST (cấu hình).
- BR-CRM-02: Chiết khấu vượt khung bắt buộc qua workflow duyệt.
- BR-CRM-03: Đơn vượt hạn mức công nợ bị block hoặc cần duyệt vượt mức.
- BR-CRM-04: Mỗi lead/cơ hội có owner; bàn giao có lịch sử.
- BR-CRM-05: Đơn hủy sau reserve phải giải phóng tồn.
- BR-CRM-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-CRM-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-CRM-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Realtime inbox | Hội thoại đồng bộ gần realtime theo kênh tích hợp |
| Search | Tìm KH/đơn < 2s với bộ lọc phổ biến |
| Mobile | Sales field dùng được trên mobile |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `CRM` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `crm.customer.manage` | Quyền chức năng module |
| `crm.lead.manage` | Quyền chức năng module |
| `crm.opportunity.manage` | Quyền chức năng module |
| `crm.quote.manage` | Quyền chức năng module |
| `crm.order.manage` | Quyền chức năng module |
| `crm.discount.approve` | Quyền chức năng module |
| `crm.campaign.manage` | Quyền chức năng module |
| `crm.inbox.agent` | Quyền chức năng module |
| `crm.report.view` | Quyền chức năng module |
| `crm.*.view` | Xem trong data scope |
| `crm.*.manage` | Tạo/sửa trong data scope |
| `crm.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Tỷ lệ chuyển đổi lead→opportunity→order | Theo dõi vận hành module |
| Doanh số / margin theo NV–vùng–SP | Theo dõi vận hành module |
| Cycle time báo giá → đơn | Theo dõi vận hành module |
| CPL, CAC, ROI campaign | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Bảng giá và chính sách công nợ cấu hình theo tenant.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Phase 1 có tích hợp sàn TMĐT sâu hay chỉ khung intake đơn?
2. Omnichannel Zalo cá nhân có nằm trong phạm vi pháp lý triển khai không?

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
| Bản SRS này | `SRS_CRM_v1.1.md` / `.docx` |
| UC IDs | `UC_CRM_001` … |

---

*Hết tài liệu SRS-CRM-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
