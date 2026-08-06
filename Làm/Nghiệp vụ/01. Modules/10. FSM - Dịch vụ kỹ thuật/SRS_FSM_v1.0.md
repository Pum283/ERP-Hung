# SRS-FSM-v1.0 — Dịch vụ kỹ thuật hiện trường (Field Service)

> Tài liệu đặc tả yêu cầu phần mềm (Software Requirements Specification) cho module ERP bán độc lập.
> Trạng thái: **Đề xuất / chờ duyệt nghiệp vụ**. Không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu & lịch sử thay đổi

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-FSM-v1.0` |
| Module | `FSM` — Dịch vụ kỹ thuật hiện trường (Field Service) |
| Phiên bản | 1.0 |
| Ngày lập | 03/08/2026 |
| Ngôn ngữ | Tiếng Việt |
| Phân loại | Nghiệp vụ / BA |
| Lớp sản phẩm | Sản xuất & Dịch vụ |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | SYS |
| Khuyến nghị kèm | CRM, INV, FIN, PRT |
| Số nhóm chức năng | 9 |
| Số use case / chức năng | 50 |

| Phiên bản | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Solution | Sinh SRS từ danh mục chức năng generic v3 + meta nghiệp vụ | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích tài liệu
Tài liệu này mô tả đầy đủ yêu cầu nghiệp vụ và yêu cầu hệ thống của module **Dịch vụ kỹ thuật hiện trường (Field Service)**, làm cơ sở để thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai cấu trúc source code.

### 1.2. Tóm tắt module
Quản lý install base, ticket bảo hành/sửa chữa/bảo trì, phân công KTV, linh kiện, SLA, nghiệm thu và báo cáo dịch vụ.

### 1.3. Mục tiêu nghiệp vụ
1. Chuẩn hóa tiếp nhận và xử lý ticket theo SLA.
2. Kiểm soát linh kiện mang đi/mang về.
3. Lưu lịch sử dịch vụ theo thiết bị khách.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / Ban giám đốc dự án
- Business Analyst, Solution Architect
- Trưởng nhóm Dev/QA
- Đội triển khai & Presales (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Service catalog, install base, ticket lifecycle, scheduling, onsite work, parts, PM, technician app, FSM reports.

### 2.2. Out of Scope
- CRM bán hàng đầy đủ.
- Kho tổng WMS (INV).

### 2.3. Nguyên tắc đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`.
- **Khuyến nghị kèm** để có giá trị E2E: CRM, INV, FIN, PRT.
- Tính năng ngành (F&B, sản xuất rời rạc, phân phối…) cấu hình bằng template khi triển khai, không hard-code vào SRS gốc.

---

## 3. Tác nhân & stakeholder

| Tác nhân | Trách nhiệm chính |
|---|---|
| Dispatcher | Tiếp nhận & phân công |
| Technician | Xử lý hiện trường |
| Service Manager | SLA & năng suất |
| Parts Storekeeper | Cấp linh kiện |
| Customer (via PRT) | Tạo/theo dõi ticket |

---

## 4. Thuật ngữ & viết tắt

| Thuật ngữ | Định nghĩa |
|---|---|
| Install base | Thiết bị đang vận hành tại khách |
| SLA | Cam kết thời gian phản hồi/xử lý |
| FTF | First Time Fix |
| PM | Preventive Maintenance |
| UC | Use Case / chức năng nguyên tử trong catalog |
| MoSCoW | Must / Should / Could / Won't (ưu tiên) |
| Data scope | Phạm vi dữ liệu theo tổ chức/kho/… do SYS kiểm soát |

---

## 5. Ngữ cảnh module & phụ thuộc

### 5.1. Vị trí trong kiến trúc sản phẩm
Module `FSM` thuộc lớp **Sản xuất & Dịch vụ**. Mọi truy cập đi qua lớp nền `SYS` (xác thực, RBAC, license, audit, file, thông báo).

### 5.2. Phụ thuộc & tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | CRM | KH & escalate |
| Tích hợp | INV | Linh kiện |
| Tích hợp | FIN | Phí dịch vụ |
| Tích hợp | PRT | Self-service ticket |
| Tích hợp | PJM | Bảo hành sau bàn giao dự án |

### 5.3. Ràng buộc license
- API/UI của `FSM` chỉ mở khi license module active.
- Dataset BI liên quan module chỉ mở khi vừa có license `BI` vừa có license module nguồn.

---

## 6. Catalog chức năng (Module → Nhóm → UC)

**Tổng hợp:** 9 nhóm | 50 chức năng/use case.

| STT | Mã nhóm | Nhóm chức năng | Số UC |
|---:|---|---|---:|
| 1 | `FSM-01` | Danh mục kỹ thuật | 7 |
| 2 | `FSM-02` | Install base – thiết bị tại khách | 5 |
| 3 | `FSM-03` | Tiếp nhận & phân công ticket | 7 |
| 4 | `FSM-04` | Thực hiện hiện trường | 8 |
| 5 | `FSM-05` | Nghiệm thu & đóng ticket | 5 |
| 6 | `FSM-06` | Bảo trì định kỳ | 4 |
| 7 | `FSM-07` | Kho linh kiện kỹ thuật | 4 |
| 8 | `FSM-08` | APP kỹ thuật viên | 4 |
| 9 | `FSM-09` | Báo cáo FSM | 6 |

<details>
<summary>Bảng đầy đủ mã UC (bấm để mở)</summary>

| Mã UC | Nhóm | Tên chức năng | Ưu tiên | MoSCoW |
|---|---|---|---|---|
| `UC_FSM_001` | Danh mục kỹ thuật | Danh mục loại dịch vụ | Bắt buộc | Must |
| `UC_FSM_002` | Danh mục kỹ thuật | Danh mục mã lỗi | Bắt buộc | Must |
| `UC_FSM_003` | Danh mục kỹ thuật | Danh mục linh kiện | Bắt buộc | Must |
| `UC_FSM_004` | Danh mục kỹ thuật | Bảng giá dịch vụ | Cao | Should |
| `UC_FSM_005` | Danh mục kỹ thuật | Cấu hình SLA | Bắt buộc | Must |
| `UC_FSM_006` | Danh mục kỹ thuật | Kỹ năng / chứng chỉ kỹ thuật viên | Cao | Should |
| `UC_FSM_007` | Danh mục kỹ thuật | Vùng phụ trách | Cao | Should |
| `UC_FSM_008` | Install base – thiết bị tại khách | Hồ sơ thiết bị đã bán | Bắt buộc | Must |
| `UC_FSM_009` | Install base – thiết bị tại khách | Serial / model / ngày kích hoạt BH | Bắt buộc | Must |
| `UC_FSM_010` | Install base – thiết bị tại khách | Lịch sử bảo hành / sửa chữa | Bắt buộc | Must |
| `UC_FSM_011` | Install base – thiết bị tại khách | Cảnh báo hết hạn bảo hành | Cao | Should |
| `UC_FSM_012` | Install base – thiết bị tại khách | Hợp đồng bảo trì định kỳ | Cao | Should |
| `UC_FSM_013` | Tiếp nhận & phân công ticket | Tạo ticket từ kênh | Bắt buộc | Must |
| `UC_FSM_014` | Tiếp nhận & phân công ticket | Phân loại mức ưu tiên | Bắt buộc | Must |
| `UC_FSM_015` | Tiếp nhận & phân công ticket | Phân công kỹ thuật viên thủ công | Bắt buộc | Must |
| `UC_FSM_016` | Tiếp nhận & phân công ticket | Phân công theo rule | Cao | Should |
| `UC_FSM_017` | Tiếp nhận & phân công ticket | Đổi kỹ thuật viên / escalate | Bắt buộc | Must |
| `UC_FSM_018` | Tiếp nhận & phân công ticket | Lịch hẹn với khách | Bắt buộc | Must |
| `UC_FSM_019` | Tiếp nhận & phân công ticket | Xác nhận lịch trên APP | Bắt buộc | Must |
| `UC_FSM_020` | Thực hiện hiện trường | Check-in hiện trường GPS | Cao | Should |
| `UC_FSM_021` | Thực hiện hiện trường | Checklist công việc | Cao | Should |
| `UC_FSM_022` | Thực hiện hiện trường | Ghi nhận nguyên nhân & xử lý | Bắt buộc | Must |
| `UC_FSM_023` | Thực hiện hiện trường | Chụp ảnh trước/sau | Cao | Should |
| `UC_FSM_024` | Thực hiện hiện trường | Xuất linh kiện theo ticket | Bắt buộc | Must |
| `UC_FSM_025` | Thực hiện hiện trường | Hoàn linh kiện thừa | Cao | Should |
| `UC_FSM_026` | Thực hiện hiện trường | Ghi nhận phí sửa chữa | Cao | Should |
| `UC_FSM_027` | Thực hiện hiện trường | Check-out / hoàn thành | Bắt buộc | Must |
| `UC_FSM_028` | Nghiệm thu & đóng ticket | Khách ký nghiệm thu | Bắt buộc | Must |
| `UC_FSM_029` | Nghiệm thu & đóng ticket | Đánh giá dịch vụ | Trung bình | Could |
| `UC_FSM_030` | Nghiệm thu & đóng ticket | Đóng ticket đạt/trễ SLA | Bắt buộc | Must |
| `UC_FSM_031` | Nghiệm thu & đóng ticket | Tái mở ticket | Cao | Should |
| `UC_FSM_032` | Nghiệm thu & đóng ticket | Chuyển chi phí sang FIN | Cao | Should |
| `UC_FSM_033` | Bảo trì định kỳ | Lịch bảo trì theo thiết bị | Cao | Should |
| `UC_FSM_034` | Bảo trì định kỳ | Tự tạo ticket bảo trì đến hạn | Cao | Should |
| `UC_FSM_035` | Bảo trì định kỳ | Checklist bảo trì chuẩn | Cao | Should |
| `UC_FSM_036` | Bảo trì định kỳ | Báo cáo thực hiện bảo trì | Cao | Should |
| `UC_FSM_037` | Kho linh kiện kỹ thuật | Tồn linh kiện tại kho KT | Bắt buộc | Must |
| `UC_FSM_038` | Kho linh kiện kỹ thuật | Cấp linh kiện cho KTV | Bắt buộc | Must |
| `UC_FSM_039` | Kho linh kiện kỹ thuật | Đối soát linh kiện | Bắt buộc | Must |
| `UC_FSM_040` | Kho linh kiện kỹ thuật | Cảnh báo thất thoát | Cao | Should |
| `UC_FSM_041` | APP kỹ thuật viên | Danh sách việc hôm nay | Bắt buộc | Must |
| `UC_FSM_042` | APP kỹ thuật viên | Điều hướng / thông tin khách | Bắt buộc | Must |
| `UC_FSM_043` | APP kỹ thuật viên | Làm việc offline | Thấp | Won't / Later |
| `UC_FSM_044` | APP kỹ thuật viên | Nộp quyết toán ngày | Cao | Should |
| `UC_FSM_045` | Báo cáo FSM | SLA compliance realtime | Bắt buộc | Must |
| `UC_FSM_046` | Báo cáo FSM | Năng suất kỹ thuật viên | Bắt buộc | Must |
| `UC_FSM_047` | Báo cáo FSM | Chi phí linh kiện | Bắt buộc | Must |
| `UC_FSM_048` | Báo cáo FSM | Tỷ lệ sửa lần đầu | Cao | Should |
| `UC_FSM_049` | Báo cáo FSM | Báo cáo bảo hành | Cao | Should |
| `UC_FSM_050` | Báo cáo FSM | Xuất báo cáo kỹ thuật | Bắt buộc | Must |

</details>

---

## 7. Đặc tả chức năng theo nhóm

Mỗi UC bên dưới gồm: mô tả, tác nhân, tiền/hậu điều kiện, luồng chính, quy tắc, tiêu chí chấp nhận và ưu tiên. Đây là mức đặc tả BA để chốt phạm vi; chi tiết UI/API sẽ bổ sung ở giai đoạn thiết kế.

### 7.1. Danh mục kỹ thuật (`FSM-01`)

Nhóm này gồm **7** chức năng. Tác nhân mặc định: **Service Manager**.

#### UC_FSM_001 — Danh mục loại dịch vụ

- **Mô tả:** Service type master
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục loại dịch vụ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_002 — Danh mục mã lỗi

- **Mô tả:** Fault code catalog
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục mã lỗi” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_003 — Danh mục linh kiện

- **Mô tả:** Spare parts catalog
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục linh kiện” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_004 — Bảng giá dịch vụ

- **Mô tả:** Service pricing
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng giá dịch vụ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Service pricing)
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
  - AC1: Thực hiện thành công thao tác “Bảng giá dịch vụ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_005 — Cấu hình SLA

- **Mô tả:** SLA matrix
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình SLA” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_006 — Kỹ năng / chứng chỉ kỹ thuật viên

- **Mô tả:** Technician skill matrix
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Kỹ năng / chứng chỉ kỹ thuật viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Technician skill matrix)
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
  - AC1: Thực hiện thành công thao tác “Kỹ năng / chứng chỉ kỹ thuật viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_007 — Vùng phụ trách

- **Mô tả:** Service territory
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Vùng phụ trách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Service territory)
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
  - AC1: Thực hiện thành công thao tác “Vùng phụ trách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.2. Install base – thiết bị tại khách (`FSM-02`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **Service Manager / Dispatcher**.

#### UC_FSM_008 — Hồ sơ thiết bị đã bán

- **Mô tả:** Installed equipment base
- **Tác nhân chính:** Service Manager / Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hồ sơ thiết bị đã bán
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Installed equipment base)
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
  - AC1: Thực hiện thành công thao tác “Hồ sơ thiết bị đã bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_009 — Serial / model / ngày kích hoạt BH

- **Mô tả:** Warranty tracking
- **Tác nhân chính:** Service Manager / Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Serial / model / ngày kích hoạt BH
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Warranty tracking)
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
  - AC1: Thực hiện thành công thao tác “Serial / model / ngày kích hoạt BH” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_010 — Lịch sử bảo hành / sửa chữa

- **Mô tả:** Service history
- **Tác nhân chính:** Service Manager / Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch sử bảo hành / sửa chữa
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Service history)
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
  - AC1: Thực hiện thành công thao tác “Lịch sử bảo hành / sửa chữa” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_011 — Cảnh báo hết hạn bảo hành

- **Mô tả:** Warranty expiry alert
- **Tác nhân chính:** Service Manager / Dispatcher
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo hết hạn bảo hành
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Warranty expiry alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo hết hạn bảo hành” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_012 — Hợp đồng bảo trì định kỳ

- **Mô tả:** Maintenance contract
- **Tác nhân chính:** Service Manager / Dispatcher
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hợp đồng bảo trì định kỳ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Maintenance contract)
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
  - AC1: Thực hiện thành công thao tác “Hợp đồng bảo trì định kỳ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.3. Tiếp nhận & phân công ticket (`FSM-03`)

Nhóm này gồm **7** chức năng. Tác nhân mặc định: **Dispatcher**.

#### UC_FSM_013 — Tạo ticket từ kênh

- **Mô tả:** Service ticket creation
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo ticket từ kênh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_014 — Phân loại mức ưu tiên

- **Mô tả:** Priority classification
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân loại mức ưu tiên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Priority classification)
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
  - AC1: Thực hiện thành công thao tác “Phân loại mức ưu tiên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_015 — Phân công kỹ thuật viên thủ công

- **Mô tả:** Manual technician assignment
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân công kỹ thuật viên thủ công
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Manual technician assignment)
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
  - AC1: Thực hiện thành công thao tác “Phân công kỹ thuật viên thủ công” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_016 — Phân công theo rule

- **Mô tả:** Auto assignment by rule
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phân công theo rule
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Auto assignment by rule)
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
  - AC1: Thực hiện thành công thao tác “Phân công theo rule” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_017 — Đổi kỹ thuật viên / escalate

- **Mô tả:** Reassignment
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đổi kỹ thuật viên / escalate
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Reassignment)
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
  - AC1: Thực hiện thành công thao tác “Đổi kỹ thuật viên / escalate” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_018 — Lịch hẹn với khách

- **Mô tả:** Appointment scheduling
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch hẹn với khách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Appointment scheduling)
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
  - AC1: Thực hiện thành công thao tác “Lịch hẹn với khách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_019 — Xác nhận lịch trên APP

- **Mô tả:** Accept job on mobile
- **Tác nhân chính:** Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xác nhận lịch trên APP
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Accept job on mobile)
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
  - AC1: Thực hiện thành công thao tác “Xác nhận lịch trên APP” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.4. Thực hiện hiện trường (`FSM-04`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **Technician**.

#### UC_FSM_020 — Check-in hiện trường GPS

- **Mô tả:** Onsite check-in
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Check-in hiện trường GPS
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Onsite check-in)
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
  - AC1: Thực hiện thành công thao tác “Check-in hiện trường GPS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_021 — Checklist công việc

- **Mô tả:** Service checklist
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist công việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Service checklist)
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
  - AC1: Thực hiện thành công thao tác “Checklist công việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_022 — Ghi nhận nguyên nhân & xử lý

- **Mô tả:** Work log & resolution
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận nguyên nhân & xử lý
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Work log & resolution)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận nguyên nhân & xử lý” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_023 — Chụp ảnh trước/sau

- **Mô tả:** Photo documentation
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chụp ảnh trước/sau
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Photo documentation)
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
  - AC1: Thực hiện thành công thao tác “Chụp ảnh trước/sau” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_024 — Xuất linh kiện theo ticket

- **Mô tả:** Parts consumption
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất linh kiện theo ticket” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_025 — Hoàn linh kiện thừa

- **Mô tả:** Return unused parts
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hoàn linh kiện thừa
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Return unused parts)
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
  - AC1: Thực hiện thành công thao tác “Hoàn linh kiện thừa” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_026 — Ghi nhận phí sửa chữa

- **Mô tả:** Chargeable service
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận phí sửa chữa
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Chargeable service)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận phí sửa chữa” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_027 — Check-out / hoàn thành

- **Mô tả:** Complete job
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Check-out / hoàn thành
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Complete job)
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
  - AC1: Thực hiện thành công thao tác “Check-out / hoàn thành” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.5. Nghiệm thu & đóng ticket (`FSM-05`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **Technician / Dispatcher**.

#### UC_FSM_028 — Khách ký nghiệm thu

- **Mô tả:** Customer sign-off
- **Tác nhân chính:** Technician / Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khách ký nghiệm thu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Customer sign-off)
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
  - AC1: Thực hiện thành công thao tác “Khách ký nghiệm thu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_029 — Đánh giá dịch vụ

- **Mô tả:** Service rating
- **Tác nhân chính:** Technician / Dispatcher
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đánh giá dịch vụ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Service rating)
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
  - AC1: Thực hiện thành công thao tác “Đánh giá dịch vụ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_030 — Đóng ticket đạt/trễ SLA

- **Mô tả:** Close ticket with SLA flag
- **Tác nhân chính:** Technician / Dispatcher
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đóng ticket đạt/trễ SLA
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Close ticket with SLA flag)
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
  - AC1: Thực hiện thành công thao tác “Đóng ticket đạt/trễ SLA” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_031 — Tái mở ticket

- **Mô tả:** Reopen ticket
- **Tác nhân chính:** Technician / Dispatcher
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tái mở ticket
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Reopen ticket)
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
  - AC1: Thực hiện thành công thao tác “Tái mở ticket” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_032 — Chuyển chi phí sang FIN

- **Mô tả:** Post service costs
- **Tác nhân chính:** Technician / Dispatcher
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển chi phí sang FIN
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Post service costs)
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
  - AC1: Thực hiện thành công thao tác “Chuyển chi phí sang FIN” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.6. Bảo trì định kỳ (`FSM-06`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **Service Manager**.

#### UC_FSM_033 — Lịch bảo trì theo thiết bị

- **Mô tả:** PM schedule
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch bảo trì theo thiết bị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (PM schedule)
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
  - AC1: Thực hiện thành công thao tác “Lịch bảo trì theo thiết bị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_034 — Tự tạo ticket bảo trì đến hạn

- **Mô tả:** Auto PM ticket generation
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tự tạo ticket bảo trì đến hạn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_035 — Checklist bảo trì chuẩn

- **Mô tả:** PM checklist
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist bảo trì chuẩn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (PM checklist)
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
  - AC1: Thực hiện thành công thao tác “Checklist bảo trì chuẩn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_036 — Báo cáo thực hiện bảo trì

- **Mô tả:** PM compliance report
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo thực hiện bảo trì” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.7. Kho linh kiện kỹ thuật (`FSM-07`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **Parts Storekeeper**.

#### UC_FSM_037 — Tồn linh kiện tại kho KT

- **Mô tả:** Parts inventory view
- **Tác nhân chính:** Parts Storekeeper
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tồn linh kiện tại kho KT
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Parts inventory view)
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
  - AC1: Thực hiện thành công thao tác “Tồn linh kiện tại kho KT” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_038 — Cấp linh kiện cho KTV

- **Mô tả:** Issue parts to technician
- **Tác nhân chính:** Parts Storekeeper
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cấp linh kiện cho KTV
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Issue parts to technician)
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
  - AC1: Thực hiện thành công thao tác “Cấp linh kiện cho KTV” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_039 — Đối soát linh kiện

- **Mô tả:** Parts reconciliation
- **Tác nhân chính:** Parts Storekeeper
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đối soát linh kiện
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Parts reconciliation)
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
  - AC1: Thực hiện thành công thao tác “Đối soát linh kiện” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_040 — Cảnh báo thất thoát

- **Mô tả:** Parts loss alert
- **Tác nhân chính:** Parts Storekeeper
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo thất thoát
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Parts loss alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo thất thoát” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.8. APP kỹ thuật viên (`FSM-08`)

Nhóm này gồm **4** chức năng. Tác nhân mặc định: **Technician**.

#### UC_FSM_041 — Danh sách việc hôm nay

- **Mô tả:** My jobs list
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Danh sách việc hôm nay
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (My jobs list)
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
  - AC1: Thực hiện thành công thao tác “Danh sách việc hôm nay” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_042 — Điều hướng / thông tin khách

- **Mô tả:** Job detail & navigation
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Điều hướng / thông tin khách
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Job detail & navigation)
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
  - AC1: Thực hiện thành công thao tác “Điều hướng / thông tin khách” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_043 — Làm việc offline

- **Mô tả:** Offline mode
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Thấp → **MoSCoW:** Won't / Later
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Làm việc offline
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
  - AC1: Thực hiện thành công thao tác “Làm việc offline” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_044 — Nộp quyết toán ngày

- **Mô tả:** Daily closeout
- **Tác nhân chính:** Technician
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nộp quyết toán ngày
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Daily closeout)
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
  - AC1: Thực hiện thành công thao tác “Nộp quyết toán ngày” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.9. Báo cáo FSM (`FSM-09`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **Service Manager**.

#### UC_FSM_045 — SLA compliance realtime

- **Mô tả:** SLA dashboard
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: SLA compliance realtime
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (SLA dashboard)
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
  - AC1: Thực hiện thành công thao tác “SLA compliance realtime” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_046 — Năng suất kỹ thuật viên

- **Mô tả:** Technician productivity
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Năng suất kỹ thuật viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Technician productivity)
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
  - AC1: Thực hiện thành công thao tác “Năng suất kỹ thuật viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_047 — Chi phí linh kiện

- **Mô tả:** Parts cost by ticket
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chi phí linh kiện
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Parts cost by ticket)
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
  - AC1: Thực hiện thành công thao tác “Chi phí linh kiện” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_FSM_048 — Tỷ lệ sửa lần đầu

- **Mô tả:** First time fix rate
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tỷ lệ sửa lần đầu
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (First time fix rate)
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
  - AC1: Thực hiện thành công thao tác “Tỷ lệ sửa lần đầu” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_049 — Báo cáo bảo hành

- **Mô tả:** Warranty split report
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo bảo hành” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_FSM_050 — Xuất báo cáo kỹ thuật

- **Mô tả:** Export service reports
- **Tác nhân chính:** Service Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `FSM`.
  - License module `FSM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất báo cáo kỹ thuật” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

---

## 8. Workflow end-to-end

### WF-FSM-01 — Xử lý ticket hiện trường

**Mục tiêu:** Đóng ticket có nghiệm thu và đối soát linh kiện

| Bước | Mô tả |
|---:|---|
| 1 | Tạo ticket từ hotline/CRM/PRT |
| 2 | Phân công KTV + lịch hẹn |
| 3 | Check-in; xử lý; xuất linh kiện |
| 4 | KH nghiệm thu; đóng ticket |
| 5 | Đối soát linh kiện; ghi phí/BH; post FIN nếu có |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `ServiceType / FaultCode / SlaPolicy` | Danh mục |
| `InstalledAsset` | Thiết bị tại KH |
| `ServiceTicket / Appointment` | Ticket & lịch |
| `TicketPartIssue` | Linh kiện |
| `PmSchedule` | Bảo trì định kỳ |

### 9.1. Xuất xứ & kiểm soát dữ liệu
- Master dùng chung (KH, SP, chi nhánh…) tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ nghiệp vụ có trạng thái vòng đời rõ ràng (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete hoặc trạng thái ngưng dùng là mặc định; hạn chế xóa cứng.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-FSM-01: Ticket trong hạn BH không tính phí công/linh kiện theo policy.
- BR-FSM-02: Đóng ticket bắt buộc có nghiệm thu (trừ hủy hợp lệ).
- BR-FSM-03: Linh kiện xuất phải gắn ticket.
- BR-FSM-04: Vi phạm SLA phải được gắn cờ và báo cáo.
- BR-FSM-GEN-01: Mọi thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-FSM-GEN-02: Mọi chứng từ có mã duy nhất theo rule Sequence của SYS.
- BR-FSM-GEN-03: Thao tác sau khi khóa kỳ/chốt sổ (nếu có) phải đi đường điều chỉnh có kiểm soát.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Mobile tech | APP KTV offline nhẹ tùy gói |
| SLA engine | Tính SLA theo lịch làm việc |
| Usability | Form có validate rõ; bảng có lọc/phân trang; hỗ trợ tiếng Việt |
| Reliability | Không mất chứng từ đã post; giao dịch quan trọng atomic |
| Maintainability | Permission và cấu hình không hard-code trong source nghiệp vụ |
| Observability | Có log ứng dụng + audit nghiệp vụ tách bạch |

---

## 12. Tích hợp & sự kiện

### 12.1. Ma trận tích hợp

| Thành phần | Mô tả |
|---|---|
| CRM | KH & escalate |
| INV | Linh kiện |
| FIN | Phí dịch vụ |
| PRT | Self-service ticket |
| PJM | Bảo hành sau bàn giao dự án |

### 12.2. Sự kiện (logical)
- `FSM.EntityCreated` / `FSM.EntityUpdated` / `FSM.EntityStatusChanged`
- `FSM.DocumentSubmitted` / `FSM.DocumentApproved` / `FSM.DocumentPosted`
- Mapping cụ thể API/topic sẽ định nghĩa ở tài liệu Interface Spec sau khi chốt SRS.

---

## 13. Phân quyền & bảo mật

### 13.1. Permission catalog (đề xuất)

- `fsm.ticket.manage`
- `fsm.dispatch.assign`
- `fsm.parts.issue`
- `fsm.ticket.close`
- `fsm.pm.manage`
- `fsm.report.view`

### 13.2. Nguyên tắc
- Deny by default; chỉ mở theo role.
- Data scope theo chi nhánh/kho/đơn vị do SYS quyết định.
- Field-level security cho dữ liệu nhạy cảm (lương, công nợ chi tiết, giá vốn…) khi áp dụng.
- Mọi thay đổi phân quyền và thao tác critical ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| SLA compliance | Giám sát vận hành module `FSM` |
| FTF | Giám sát vận hành module `FSM` |
| Parts cost/ticket | Giám sát vận hành module `FSM` |
| Tech utilization | Giám sát vận hành module `FSM` |

Báo cáo chi tiết vận hành nằm trong từng nhóm “Báo cáo…” của Mục 7; tổng hợp điều hành nằm trên module `BI` khi khách mua thêm.

---

## 15. Giả định, rủi ro & câu hỏi mở

### 15.1. Giả định
- Thiết bị có thể gắn serial từ đơn bán CRM/INV.

### 15.2. Câu hỏi mở cần chốt
- Có tích hợp IoT/cảnh báo máy phase sau?

### 15.3. Rủi ro
- Phụ thuộc module khác chưa mua → một số workflow E2E chỉ chạy được một phần (cần nêu rõ khi bán gói).
- Cấu hình quá linh hoạt có thể làm tăng effort QA; cần bộ template mặc định.
- Chưa chốt chuẩn kế toán/thuế chi tiết có thể ảnh hưởng FIN và posting.

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện nghiệm thu module
1. 100% UC ưu tiên **Bắt buộc (Must)** của `FSM` pass UAT.
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
| Use case IDs | `UC_FSM_001` … `UC_FSM_050` |

---

*Hết tài liệu SRS-FSM-v1.0.*
