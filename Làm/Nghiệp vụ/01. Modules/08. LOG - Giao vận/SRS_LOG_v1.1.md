# SRS-LOG-v1.1 — Giao vận (Logistics)

> **Software Requirements Specification — Module LOG**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-LOG-v1.1` |
| Module | `LOG` — Giao vận (Logistics) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Chuỗi cung ứng |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS`, `INV` |
| Khuyến nghị kèm | `CRM`, `FIN` |
| Số nhóm / UC | 7 nhóm / 39 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Giao vận (Logistics)** (`LOG`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Quản lý lệnh giao, điều phối tài xế/3PL, theo dõi vận đơn, COD, hoàn hàng và KPI giao hàng.

### 1.3. Mục tiêu đo được
1. Biến đơn hàng thành chuyến giao có trạng thái rõ ràng.
2. Đối soát COD 3 chiều.
3. Đo on-time delivery và tỷ lệ thất bại.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Carrier/fleet, delivery order, dispatch, tracking/POD, COD, returns, LOG reports.

### 2.2. Out of Scope
- Tối ưu lộ trình AI nâng cao (phase sau).
- WMS picking chi tiết sâu (INV).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`, `INV`
- **Khuyến nghị kèm (E2E):** `CRM`, `FIN`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| Logistics Coordinator | Tạo DO, phân công |
| Driver / 3PL | Cập nhật trạng thái, thu COD |
| Cashier/Accountant | Đối soát COD |
| Sales Admin | Theo dõi đơn giao |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| DO | Delivery Order — lệnh giao |
| POD | Proof of Delivery |
| COD | Cash on Delivery |
| ASN | Advanced Shipping Notice (NCC/3PL) |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- LOG (Giao vận (Logistics))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `LOG` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `LOG` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM | Nguồn đơn |
| Tích hợp | INV | Xuất/nhập hoàn |
| Tích hợp | FIN | COD & cước vận chuyển |
| Tích hợp | 3PL API | Tracking (khung) |

---

## 6. Catalog chức năng

**Tổng:** 7 nhóm · 39 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `LOG-01` | Cấu hình giao vận | 5 | 1 | 3 | 1 |
| 2 | `LOG-02` | Lệnh giao hàng | 7 | 6 | 1 | 0 |
| 3 | `LOG-03` | Điều phối & theo dõi | 7 | 3 | 2 | 2 |
| 4 | `LOG-04` | COD | 7 | 6 | 1 | 0 |
| 5 | `LOG-05` | Hoàn hàng & giao lại | 4 | 3 | 0 | 1 |
| 6 | `LOG-06` | Giao nội bộ | 3 | 0 | 3 | 0 |
| 7 | `LOG-07` | Báo cáo giao vận | 6 | 4 | 2 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_LOG_001` | Cấu hình giao vận | Danh mục đơn vị vận chuyển | Must |
| `UC_LOG_002` | Cấu hình giao vận | Danh mục tài xế / xe | Should |
| `UC_LOG_003` | Cấu hình giao vận | Bảng giá cước vận chuyển | Should |
| `UC_LOG_004` | Cấu hình giao vận | Cấu hình khu vực giao | Should |
| `UC_LOG_005` | Cấu hình giao vận | Cấu hình ca giao hàng | Could |
| `UC_LOG_006` | Lệnh giao hàng | Tạo lệnh giao từ đơn hàng | Must |
| `UC_LOG_007` | Lệnh giao hàng | Gộp nhiều đơn thành chuyến | Should |
| `UC_LOG_008` | Lệnh giao hàng | Tách lệnh giao nhiều đợt | Must |
| `UC_LOG_009` | Lệnh giao hàng | Pick list / soạn hàng | Must |
| `UC_LOG_010` | Lệnh giao hàng | Xác nhận xuất hàng giao | Must |
| `UC_LOG_011` | Lệnh giao hàng | In vận đơn / phiếu giao | Must |
| `UC_LOG_012` | Lệnh giao hàng | Hủy / hoàn lệnh giao | Must |
| `UC_LOG_013` | Điều phối & theo dõi | Phân công tài xế / đơn vị vận chuyển | Must |
| `UC_LOG_014` | Điều phối & theo dõi | Cập nhật trạng thái vận đơn | Must |
| `UC_LOG_015` | Điều phối & theo dõi | Tracking mã vận đơn | Could |
| `UC_LOG_016` | Điều phối & theo dõi | Chứng từ ký nhận (POD) | Should |
| `UC_LOG_017` | Điều phối & theo dõi | Ghi nhận giao thất bại | Must |
| `UC_LOG_018` | Điều phối & theo dõi | Hẹn giao lại | Should |
| `UC_LOG_019` | Điều phối & theo dõi | Theo dõi realtime trên bản đồ | Later |
| `UC_LOG_020` | COD | Đánh dấu đơn thu COD | Must |
| `UC_LOG_021` | COD | Ghi nhận số tiền COD | Must |
| `UC_LOG_022` | COD | Xác nhận đã thu COD | Must |
| `UC_LOG_023` | COD | Bàn giao tiền COD | Must |
| `UC_LOG_024` | COD | Đối soát 3 chiều COD | Must |
| `UC_LOG_025` | COD | Cảnh báo COD quá hạn | Must |
| `UC_LOG_026` | COD | Xử lý lệch COD | Should |
| `UC_LOG_027` | Hoàn hàng & giao lại | Tạo phiếu hoàn về kho | Must |
| `UC_LOG_028` | Hoàn hàng & giao lại | Kiểm đếm hàng hoàn | Must |
| `UC_LOG_029` | Hoàn hàng & giao lại | Nhập kho hàng hoàn | Must |
| `UC_LOG_030` | Hoàn hàng & giao lại | Chi phí phát sinh hoàn | Could |
| `UC_LOG_031` | Giao nội bộ | Lệnh giao nội bộ | Should |
| `UC_LOG_032` | Giao nội bộ | Xác nhận nhận hàng | Should |
| `UC_LOG_033` | Giao nội bộ | Đối soát giao nội bộ | Should |
| `UC_LOG_034` | Báo cáo giao vận | Tỷ lệ giao đúng hạn | Must |
| `UC_LOG_035` | Báo cáo giao vận | Tỷ lệ hoàn / thất bại | Must |
| `UC_LOG_036` | Báo cáo giao vận | Năng suất tài xế / chuyến | Should |
| `UC_LOG_037` | Báo cáo giao vận | Chi phí vận chuyển | Should |
| `UC_LOG_038` | Báo cáo giao vận | Báo cáo COD tồn / đã nộp | Must |
| `UC_LOG_039` | Báo cáo giao vận | Dashboard giao vận | Must |

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

### 7.1. Cấu hình giao vận (`LOG-01`)

Nhóm **Cấu hình giao vận** gồm **5** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 1 |

**Bảng 1. Đặc tả Use Case "Danh mục đơn vị vận chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_001 |
| **Tên Use Case** | Danh mục đơn vị vận chuyển |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Danh mục đơn vị vận chuyển" thuộc nhóm Cấu hình giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Carrier master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục đơn vị vận chuyển» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục đơn vị vận chuyển» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục đơn vị vận chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Danh mục đơn vị vận chuyển» trong nhóm Cấu hình giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Carrier master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục đơn vị vận chuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục đơn vị vận chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Danh mục tài xế / xe"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_002 |
| **Tên Use Case** | Danh mục tài xế / xe |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Danh mục tài xế / xe" thuộc nhóm Cấu hình giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Fleet master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục tài xế / xe» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục tài xế / xe» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục tài xế / xe» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Danh mục tài xế / xe» trong nhóm Cấu hình giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Fleet master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục tài xế / xe».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục tài xế / xe» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 3. Đặc tả Use Case "Bảng giá cước vận chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_003 |
| **Tên Use Case** | Bảng giá cước vận chuyển |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Bảng giá cước vận chuyển" thuộc nhóm Cấu hình giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Freight rates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng giá cước vận chuyển» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng giá cước vận chuyển» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng giá cước vận chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Bảng giá cước vận chuyển» trong nhóm Cấu hình giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Freight rates).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng giá cước vận chuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng giá cước vận chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 4. Đặc tả Use Case "Cấu hình khu vực giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_004 |
| **Tên Use Case** | Cấu hình khu vực giao |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Cấu hình khu vực giao" thuộc nhóm Cấu hình giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Delivery zone |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình khu vực giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình khu vực giao» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình khu vực giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator mở màn hình cấu hình «Cấu hình khu vực giao» trong Cấu hình giao vận.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Delivery zone) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình khu vực giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 5. Đặc tả Use Case "Cấu hình ca giao hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_005 |
| **Tên Use Case** | Cấu hình ca giao hàng |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Cấu hình ca giao hàng" thuộc nhóm Cấu hình giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Delivery shift |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình ca giao hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình ca giao hàng» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình ca giao hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator mở màn hình cấu hình «Cấu hình ca giao hàng» trong Cấu hình giao vận.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Delivery shift) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình ca giao hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

### 7.2. Lệnh giao hàng (`LOG-02`)

Nhóm **Lệnh giao hàng** gồm **7** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 6 |

**Bảng 6. Đặc tả Use Case "Tạo lệnh giao từ đơn hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_006 |
| **Tên Use Case** | Tạo lệnh giao từ đơn hàng |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Tạo lệnh giao từ đơn hàng" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Create delivery order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo lệnh giao từ đơn hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo lệnh giao từ đơn hàng» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo lệnh giao từ đơn hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator mở chức năng «Tạo lệnh giao từ đơn hàng» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo lệnh giao từ đơn hàng» (Create delivery order).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo lệnh giao từ đơn hàng» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo lệnh giao từ đơn hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 7. Đặc tả Use Case "Gộp nhiều đơn thành chuyến"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_007 |
| **Tên Use Case** | Gộp nhiều đơn thành chuyến |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Gộp nhiều đơn thành chuyến" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Route trip planning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gộp nhiều đơn thành chuyến» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gộp nhiều đơn thành chuyến» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gộp nhiều đơn thành chuyến» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Gộp nhiều đơn thành chuyến» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Route trip planning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gộp nhiều đơn thành chuyến».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gộp nhiều đơn thành chuyến» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Tách lệnh giao nhiều đợt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_008 |
| **Tên Use Case** | Tách lệnh giao nhiều đợt |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Tách lệnh giao nhiều đợt" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Partial delivery |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tách lệnh giao nhiều đợt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tách lệnh giao nhiều đợt» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tách lệnh giao nhiều đợt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Tách lệnh giao nhiều đợt» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Partial delivery).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tách lệnh giao nhiều đợt».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tách lệnh giao nhiều đợt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 9. Đặc tả Use Case "Pick list / soạn hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_009 |
| **Tên Use Case** | Pick list / soạn hàng |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Pick list / soạn hàng" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Picking list |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Pick list / soạn hàng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Pick list / soạn hàng» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Pick list / soạn hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Pick list / soạn hàng» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Picking list).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Pick list / soạn hàng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Pick list / soạn hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 10. Đặc tả Use Case "Xác nhận xuất hàng giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_010 |
| **Tên Use Case** | Xác nhận xuất hàng giao |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Xác nhận xuất hàng giao" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Ship confirmation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận xuất hàng giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận xuất hàng giao» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận xuất hàng giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Xác nhận xuất hàng giao» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Ship confirmation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận xuất hàng giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận xuất hàng giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 11. Đặc tả Use Case "In vận đơn / phiếu giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_011 |
| **Tên Use Case** | In vận đơn / phiếu giao |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "In vận đơn / phiếu giao" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Shipping documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In vận đơn / phiếu giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In vận đơn / phiếu giao» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In vận đơn / phiếu giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «In vận đơn / phiếu giao» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Shipping documents).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «In vận đơn / phiếu giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In vận đơn / phiếu giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Hủy / hoàn lệnh giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_012 |
| **Tên Use Case** | Hủy / hoàn lệnh giao |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Hủy / hoàn lệnh giao" thuộc nhóm Lệnh giao hàng trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Cancel/return delivery |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy / hoàn lệnh giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy / hoàn lệnh giao» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy / hoàn lệnh giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Hủy / hoàn lệnh giao» trong nhóm Lệnh giao hàng.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Cancel/return delivery).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hủy / hoàn lệnh giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy / hoàn lệnh giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Điều phối & theo dõi (`LOG-03`)

Nhóm **Điều phối & theo dõi** gồm **7** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 3 |

**Bảng 13. Đặc tả Use Case "Phân công tài xế / đơn vị vận chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_013 |
| **Tên Use Case** | Phân công tài xế / đơn vị vận chuyển |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Phân công tài xế / đơn vị vận chuyển" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Assign carrier |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân công tài xế / đơn vị vận chuyển» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân công tài xế / đơn vị vận chuyển» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân công tài xế / đơn vị vận chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator chọn đối tượng nguồn trong «Phân công tài xế / đơn vị vận chuyển».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân công tài xế / đơn vị vận chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 14. Đặc tả Use Case "Cập nhật trạng thái vận đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_014 |
| **Tên Use Case** | Cập nhật trạng thái vận đơn |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Cập nhật trạng thái vận đơn" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Shipment status update |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cập nhật trạng thái vận đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cập nhật trạng thái vận đơn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cập nhật trạng thái vận đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator tìm và mở bản ghi liên quan tới «Cập nhật trạng thái vận đơn» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Cập nhật trạng thái vận đơn» (Shipment status update).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cập nhật trạng thái vận đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 15. Đặc tả Use Case "Tracking mã vận đơn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_015 |
| **Tên Use Case** | Tracking mã vận đơn |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Tracking mã vận đơn" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Tracking integration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tracking mã vận đơn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tracking mã vận đơn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tracking mã vận đơn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Tracking mã vận đơn» trong nhóm Điều phối & theo dõi.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Tracking integration).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tracking mã vận đơn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tracking mã vận đơn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 16. Đặc tả Use Case "Chứng từ ký nhận (POD)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_016 |
| **Tên Use Case** | Chứng từ ký nhận (POD) |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Chứng từ ký nhận (POD)" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Proof of delivery |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chứng từ ký nhận (POD)» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chứng từ ký nhận (POD)» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chứng từ ký nhận (POD)» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Chứng từ ký nhận (POD)» trong nhóm Điều phối & theo dõi.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Proof of delivery).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chứng từ ký nhận (POD)».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chứng từ ký nhận (POD)» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Ghi nhận giao thất bại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_017 |
| **Tên Use Case** | Ghi nhận giao thất bại |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Ghi nhận giao thất bại" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Failed delivery log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận giao thất bại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận giao thất bại» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận giao thất bại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Ghi nhận giao thất bại» trong nhóm Điều phối & theo dõi.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Failed delivery log).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận giao thất bại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận giao thất bại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "Hẹn giao lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_018 |
| **Tên Use Case** | Hẹn giao lại |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Hẹn giao lại" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Redelivery scheduling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hẹn giao lại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hẹn giao lại» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hẹn giao lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Hẹn giao lại» trong nhóm Điều phối & theo dõi.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Redelivery scheduling).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hẹn giao lại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hẹn giao lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 19. Đặc tả Use Case "Theo dõi realtime trên bản đồ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_019 |
| **Tên Use Case** | Theo dõi realtime trên bản đồ |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Theo dõi realtime trên bản đồ" thuộc nhóm Điều phối & theo dõi trong module LOG — Giao vận (Logistics). Mô tả chi tiết: GPS tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi realtime trên bản đồ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi realtime trên bản đồ» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi realtime trên bản đồ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Theo dõi realtime trên bản đồ» trong nhóm Điều phối & theo dõi.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (GPS tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi realtime trên bản đồ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi realtime trên bản đồ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. COD (`LOG-04`)

Nhóm **COD** gồm **7** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 6 |

**Bảng 20. Đặc tả Use Case "Đánh dấu đơn thu COD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_020 |
| **Tên Use Case** | Đánh dấu đơn thu COD |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Đánh dấu đơn thu COD" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD flag |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh dấu đơn thu COD» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh dấu đơn thu COD» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh dấu đơn thu COD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Driver khởi tạo thao tác «Đánh dấu đơn thu COD» trong nhóm COD.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (COD flag).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh dấu đơn thu COD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh dấu đơn thu COD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Ghi nhận số tiền COD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_021 |
| **Tên Use Case** | Ghi nhận số tiền COD |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Ghi nhận số tiền COD" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD amount |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận số tiền COD» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận số tiền COD» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận số tiền COD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Driver khởi tạo thao tác «Ghi nhận số tiền COD» trong nhóm COD.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (COD amount).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận số tiền COD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận số tiền COD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Xác nhận đã thu COD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_022 |
| **Tên Use Case** | Xác nhận đã thu COD |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Xác nhận đã thu COD" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD collection confirm |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận đã thu COD» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận đã thu COD» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận đã thu COD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Driver khởi tạo thao tác «Xác nhận đã thu COD» trong nhóm COD.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (COD collection confirm).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận đã thu COD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận đã thu COD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Bàn giao tiền COD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_023 |
| **Tên Use Case** | Bàn giao tiền COD |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Bàn giao tiền COD" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD handover |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bàn giao tiền COD» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bàn giao tiền COD» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bàn giao tiền COD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Driver khởi tạo thao tác «Bàn giao tiền COD» trong nhóm COD.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (COD handover).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bàn giao tiền COD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bàn giao tiền COD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Đối soát 3 chiều COD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_024 |
| **Tên Use Case** | Đối soát 3 chiều COD |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Đối soát 3 chiều COD" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát 3 chiều COD» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát 3 chiều COD» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát 3 chiều COD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Driver khởi tạo thao tác «Đối soát 3 chiều COD» trong nhóm COD.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (COD reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát 3 chiều COD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát 3 chiều COD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Cảnh báo COD quá hạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_025 |
| **Tên Use Case** | Cảnh báo COD quá hạn |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Cảnh báo COD quá hạn" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD aging alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo COD quá hạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo COD quá hạn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo COD quá hạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Driver kích hoạt kiểm tra điều kiện «Cảnh báo COD quá hạn».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (COD aging alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo COD quá hạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Xử lý lệch COD"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_026 |
| **Tên Use Case** | Xử lý lệch COD |
| **Tác nhân** | Driver |
| **Mô tả chức năng** | Cho phép Driver thực hiện chức năng "Xử lý lệch COD" thuộc nhóm COD trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD variance handling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Driver] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xử lý lệch COD» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xử lý lệch COD» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xử lý lệch COD» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Driver khởi tạo thao tác «Xử lý lệch COD» trong nhóm COD.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (COD variance handling).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xử lý lệch COD».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xử lý lệch COD» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Hoàn hàng & giao lại (`LOG-05`)

Nhóm **Hoàn hàng & giao lại** gồm **4** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 3 |

**Bảng 27. Đặc tả Use Case "Tạo phiếu hoàn về kho"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_027 |
| **Tên Use Case** | Tạo phiếu hoàn về kho |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Tạo phiếu hoàn về kho" thuộc nhóm Hoàn hàng & giao lại trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Return to warehouse |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phiếu hoàn về kho» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phiếu hoàn về kho» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phiếu hoàn về kho» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper mở chức năng «Tạo phiếu hoàn về kho» trong nhóm Hoàn hàng & giao lại.<br>2. Hệ thống kiểm tra license `LOG`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phiếu hoàn về kho» (Return to warehouse).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phiếu hoàn về kho» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phiếu hoàn về kho» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 28. Đặc tả Use Case "Kiểm đếm hàng hoàn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_028 |
| **Tên Use Case** | Kiểm đếm hàng hoàn |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Kiểm đếm hàng hoàn" thuộc nhóm Hoàn hàng & giao lại trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Return count |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kiểm đếm hàng hoàn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kiểm đếm hàng hoàn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kiểm đếm hàng hoàn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Kiểm đếm hàng hoàn» trong nhóm Hoàn hàng & giao lại.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Return count).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kiểm đếm hàng hoàn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kiểm đếm hàng hoàn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Nhập kho hàng hoàn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_029 |
| **Tên Use Case** | Nhập kho hàng hoàn |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Nhập kho hàng hoàn" thuộc nhóm Hoàn hàng & giao lại trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Return receipt |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập kho hàng hoàn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập kho hàng hoàn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập kho hàng hoàn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Nhập kho hàng hoàn» trong nhóm Hoàn hàng & giao lại.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Return receipt).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập kho hàng hoàn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập kho hàng hoàn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Chi phí phát sinh hoàn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_030 |
| **Tên Use Case** | Chi phí phát sinh hoàn |
| **Tác nhân** | Storekeeper |
| **Mô tả chức năng** | Cho phép Storekeeper thực hiện chức năng "Chi phí phát sinh hoàn" thuộc nhóm Hoàn hàng & giao lại trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Return cost |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Storekeeper] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chi phí phát sinh hoàn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chi phí phát sinh hoàn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chi phí phát sinh hoàn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Storekeeper khởi tạo thao tác «Chi phí phát sinh hoàn» trong nhóm Hoàn hàng & giao lại.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Return cost).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chi phí phát sinh hoàn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chi phí phát sinh hoàn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Giao nội bộ (`LOG-06`)

Nhóm **Giao nội bộ** gồm **3** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 3 |
| Must | 0 |

**Bảng 31. Đặc tả Use Case "Lệnh giao nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_031 |
| **Tên Use Case** | Lệnh giao nội bộ |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Lệnh giao nội bộ" thuộc nhóm Giao nội bộ trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Internal delivery |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lệnh giao nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lệnh giao nội bộ» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lệnh giao nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Lệnh giao nội bộ» trong nhóm Giao nội bộ.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Internal delivery).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lệnh giao nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lệnh giao nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 32. Đặc tả Use Case "Xác nhận nhận hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_032 |
| **Tên Use Case** | Xác nhận nhận hàng |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Xác nhận nhận hàng" thuộc nhóm Giao nội bộ trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Receipt acknowledgment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận nhận hàng» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`, `BR-LOG-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận nhận hàng» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận nhận hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator mở chứng từ nhận liên quan «Xác nhận nhận hàng».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận nhận hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý. |

**Bảng 33. Đặc tả Use Case "Đối soát giao nội bộ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_033 |
| **Tên Use Case** | Đối soát giao nội bộ |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Đối soát giao nội bộ" thuộc nhóm Giao nội bộ trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Internal delivery reconciliation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đối soát giao nội bộ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đối soát giao nội bộ» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đối soát giao nội bộ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Đối soát giao nội bộ» trong nhóm Giao nội bộ.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Internal delivery reconciliation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đối soát giao nội bộ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đối soát giao nội bộ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Báo cáo giao vận (`LOG-07`)

Nhóm **Báo cáo giao vận** gồm **6** use case của module `LOG`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 34. Đặc tả Use Case "Tỷ lệ giao đúng hạn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_034 |
| **Tên Use Case** | Tỷ lệ giao đúng hạn |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Tỷ lệ giao đúng hạn" thuộc nhóm Báo cáo giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: On-time delivery |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tỷ lệ giao đúng hạn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tỷ lệ giao đúng hạn» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tỷ lệ giao đúng hạn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Tỷ lệ giao đúng hạn» trong nhóm Báo cáo giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (On-time delivery).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tỷ lệ giao đúng hạn».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tỷ lệ giao đúng hạn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 35. Đặc tả Use Case "Tỷ lệ hoàn / thất bại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_035 |
| **Tên Use Case** | Tỷ lệ hoàn / thất bại |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Tỷ lệ hoàn / thất bại" thuộc nhóm Báo cáo giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Delivery failure rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tỷ lệ hoàn / thất bại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tỷ lệ hoàn / thất bại» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tỷ lệ hoàn / thất bại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Tỷ lệ hoàn / thất bại» trong nhóm Báo cáo giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Delivery failure rate).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tỷ lệ hoàn / thất bại».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tỷ lệ hoàn / thất bại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Năng suất tài xế / chuyến"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_036 |
| **Tên Use Case** | Năng suất tài xế / chuyến |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Năng suất tài xế / chuyến" thuộc nhóm Báo cáo giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Driver productivity |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Năng suất tài xế / chuyến» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Năng suất tài xế / chuyến» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Năng suất tài xế / chuyến» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Năng suất tài xế / chuyến» trong nhóm Báo cáo giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Driver productivity).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Năng suất tài xế / chuyến».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Năng suất tài xế / chuyến» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 37. Đặc tả Use Case "Chi phí vận chuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_037 |
| **Tên Use Case** | Chi phí vận chuyển |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Chi phí vận chuyển" thuộc nhóm Báo cáo giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Freight cost analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chi phí vận chuyển» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chi phí vận chuyển» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chi phí vận chuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Logistics Coordinator khởi tạo thao tác «Chi phí vận chuyển» trong nhóm Báo cáo giao vận.<br>2. Hệ thống kiểm tra license `LOG`, quyền RBAC và tiền điều kiện nghiệp vụ (Freight cost analysis).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chi phí vận chuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chi phí vận chuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 38. Đặc tả Use Case "Báo cáo COD tồn / đã nộp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_038 |
| **Tên Use Case** | Báo cáo COD tồn / đã nộp |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Báo cáo COD tồn / đã nộp" thuộc nhóm Báo cáo giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: COD outstanding report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo COD tồn / đã nộp» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo COD tồn / đã nộp» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo COD tồn / đã nộp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator mở «Báo cáo COD tồn / đã nộp» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (COD outstanding report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo COD tồn / đã nộp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 39. Đặc tả Use Case "Dashboard giao vận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LOG_039 |
| **Tên Use Case** | Dashboard giao vận |
| **Tác nhân** | Logistics Coordinator |
| **Mô tả chức năng** | Cho phép Logistics Coordinator thực hiện chức năng "Dashboard giao vận" thuộc nhóm Báo cáo giao vận trong module LOG — Giao vận (Logistics). Mô tả chi tiết: Logistics dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Logistics Coordinator] và được cấp quyền RBAC tương ứng.<br>• License module `LOG` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard giao vận» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LOG-SCOPE-01`, `BR-LOG-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard giao vận» được lưu nhất quán trong module `LOG`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard giao vận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Logistics Coordinator mở «Dashboard giao vận» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Logistics dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard giao vận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-LOG-01 — Giao hàng có COD

**Mục tiêu:** Giao thành công và nộp đủ tiền COD

| Bước | Mô tả |
|---:|---|
| 1 | Tạo DO từ đơn; pick/xuất kho |
| 2 | Phân công tài xế/3PL |
| 3 | Cập nhật trạng thái; POD |
| 4 | Thu COD; bàn giao tiền |
| 5 | Đối soát 3 chiều Sales–Ship–Kế toán |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Carrier / Driver / Vehicle` | Năng lực giao |
| `DeliveryOrder / DeliveryTrip` | Lệnh & chuyến |
| `ShipmentEvent / POD` | Tracking |
| `CodCollection` | COD |
| `DeliveryReturn` | Hoàn |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `LOG`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-LOG-01: Chỉ tạo DO khi đơn đủ điều kiện fulfillment và có tồn/reserve.
- BR-LOG-02: Giao thất bại phải có lý do mã hóa.
- BR-LOG-03: COD chưa nộp quá hạn phải cảnh báo.
- BR-LOG-04: Hoàn hàng phải tạo chứng từ nhập lại INV.
- BR-LOG-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-LOG-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-LOG-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Mobile driver | Cập nhật trạng thái trên mobile |
| SLA tracking | Trạng thái gần realtime |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `LOG` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `log.do.manage` | Quyền chức năng module |
| `log.dispatch.assign` | Quyền chức năng module |
| `log.status.update` | Quyền chức năng module |
| `log.cod.reconcile` | Quyền chức năng module |
| `log.report.view` | Quyền chức năng module |
| `log.*.view` | Xem trong data scope |
| `log.*.manage` | Tạo/sửa trong data scope |
| `log.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| On-time delivery | Theo dõi vận hành module |
| Fail/return rate | Theo dõi vận hành module |
| COD aging | Theo dõi vận hành module |
| Cost per delivery | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Có thể giao nội bộ hoặc 3PL song song.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Phase 1 có map tracking GPS tài xế không?

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
| Bản SRS này | `SRS_LOG_v1.1.md` / `.docx` |
| UC IDs | `UC_LOG_001` … |

---

*Hết tài liệu SRS-LOG-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
