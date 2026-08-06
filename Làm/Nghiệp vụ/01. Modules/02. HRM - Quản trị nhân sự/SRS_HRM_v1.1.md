# SRS-HRM-v1.1 — Quản trị nhân sự (Human Resource Management)

> **Software Requirements Specification — Module HRM**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-HRM-v1.1` |
| Module | `HRM` — Quản trị nhân sự (Human Resource Management) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Nhân sự & Đào tạo |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `WF`, `FIN`, `LMS` |
| Số nhóm / UC | 20 nhóm / 187 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Quản trị nhân sự (Human Resource Management)** (`HRM`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module HRM quản lý vòng đời nhân sự: tổ chức, hồ sơ, hợp đồng, tuyển dụng, onboarding, xếp ca, chấm công, nghỉ phép, điều động, lương/phụ cấp, offboarding và báo cáo nhân sự. Thiết kế generic cho doanh nghiệp đa chi nhánh.

### 1.3. Mục tiêu đo được
1. Số hóa hồ sơ và biến động nhân sự end-to-end.
2. Chấm công – xếp ca – nghỉ phép chính xác, có kiểm soát duyệt.
3. Tính lương theo kỳ từ dữ liệu công và rule cấu hình.
4. Cung cấp số liệu headcount, chi phí nhân sự, tuân thủ hợp đồng.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Org HR, employee master, contract, recruitment, onboarding/probation, staffing & shift, attendance, leave, transfer/secondment, payroll, discipline/reward, offboarding, HR reports.

### 2.2. Out of Scope
- Nội dung đào tạo chi tiết (LMS).
- Hạch toán kế toán đầy đủ (FIN nhận bút toán lương).
- Quản lý tài sản chi tiết khi bàn giao (AST; HRM chỉ checklist).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `WF`, `FIN`, `LMS`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| HR Admin | Cấu hình master HR, rule lương/phép |
| HR Officer | Vận hành hồ sơ, tuyển dụng, công, lương |
| Line Manager | Duyệt nghỉ/đổi ca, xem đội nhóm, đánh giá |
| Employee | Self-service: công, phép, phiếu lương, cập nhật thông tin hạn chế |
| Payroll Accountant | Chốt kỳ lương, xuất chi lương |
| Recruiter | Quản lý tin tuyển và ứng viên |
| Hệ thống | Nhắc hạn HĐ/thử việc, tính công/lương |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Headcount | Định biên / số lượng nhân sự |
| Timesheet | Bảng công theo kỳ |
| Payroll | Kỳ và bảng tính lương |
| Probation | Thời gian thử việc |
| Secondment | Điều động hỗ trợ đơn vị khác |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- HRM (Quản trị nhân sự (Human Resource Management))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `HRM` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `HRM` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | SYS | User lifecycle, org master, workflow duyệt, file, thông báo |
| Tích hợp | WF | Duyệt nghỉ / điều chỉnh công / đề xuất tuyển / kỳ lương |
| Tích hợp | LMS | Đồng bộ chứng chỉ đào tạo bắt buộc vào hồ sơ |
| Tích hợp | FIN | Post chi phí lương, tạm ứng, khấu trừ |
| Tích hợp | AST | Checklist thu hồi tài sản khi nghỉ việc |
| Tích hợp | Device | Máy chấm công / geo-fence app |

---

## 6. Catalog chức năng

**Tổng:** 20 nhóm · 187 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `HRM-01` | Cơ cấu tổ chức nhân sự | 11 | 7 | 4 | 0 |
| 2 | `HRM-02` | Hồ sơ nhân sự | 17 | 12 | 3 | 2 |
| 3 | `HRM-03` | Trạng thái & biến động nhân sự | 9 | 7 | 2 | 0 |
| 4 | `HRM-04` | Hợp đồng lao động | 9 | 8 | 1 | 0 |
| 5 | `HRM-05` | Tuyển dụng – nhu cầu | 7 | 5 | 2 | 0 |
| 6 | `HRM-06` | Tuyển dụng – đăng tin & ứng viên | 12 | 8 | 3 | 1 |
| 7 | `HRM-07` | Onboarding | 9 | 8 | 1 | 0 |
| 8 | `HRM-08` | Định biên | 6 | 4 | 2 | 0 |
| 9 | `HRM-09` | Ca làm việc | 11 | 6 | 4 | 1 |
| 10 | `HRM-10` | Điều động nhân sự | 6 | 0 | 6 | 0 |
| 11 | `HRM-11` | Cấu hình chấm công | 11 | 7 | 4 | 0 |
| 12 | `HRM-12` | Thực hiện chấm công | 11 | 9 | 2 | 0 |
| 13 | `HRM-13` | Điều chỉnh & khóa công | 9 | 5 | 4 | 0 |
| 14 | `HRM-14` | Nghỉ phép & vắng mặt | 10 | 7 | 3 | 0 |
| 15 | `HRM-15` | Kỷ luật & khen thưởng | 5 | 0 | 4 | 1 |
| 16 | `HRM-16` | Offboarding / nghỉ việc | 8 | 6 | 1 | 1 |
| 17 | `HRM-17` | Cấu hình lương & phụ cấp | 11 | 10 | 1 | 0 |
| 18 | `HRM-18` | Tính lương & chi trả | 14 | 12 | 2 | 0 |
| 19 | `HRM-19` | Đánh giá hiệu suất | 5 | 0 | 0 | 5 |
| 20 | `HRM-20` | Báo cáo & dashboard HRM | 6 | 3 | 3 | 0 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_HRM_001` | Cơ cấu tổ chức nhân sự | Tạo sơ đồ tổ chức công ty | Must |
| `UC_HRM_002` | Cơ cấu tổ chức nhân sự | Quản lý khối vận hành | Must |
| `UC_HRM_003` | Cơ cấu tổ chức nhân sự | Quản lý khối sản xuất | Must |
| `UC_HRM_004` | Cơ cấu tổ chức nhân sự | Quản lý danh mục điểm bán | Must |
| `UC_HRM_005` | Cơ cấu tổ chức nhân sự | Quản lý bộ phận trong đơn vị | Should |
| `UC_HRM_006` | Cơ cấu tổ chức nhân sự | Khai báo giờ làm việc theo đơn vị | Must |
| `UC_HRM_007` | Cơ cấu tổ chức nhân sự | Quản lý chức danh nhân sự | Must |
| `UC_HRM_008` | Cơ cấu tổ chức nhân sự | Quản lý vị trí công việc | Should |
| `UC_HRM_009` | Cơ cấu tổ chức nhân sự | Quản lý loại nhân sự | Must |
| `UC_HRM_010` | Cơ cấu tổ chức nhân sự | Quản lý cấp bậc / level | Should |
| `UC_HRM_011` | Cơ cấu tổ chức nhân sự | Định nghĩa trung tâm chi phí NS | Should |
| `UC_HRM_012` | Hồ sơ nhân sự | Sinh mã nhân sự tự động | Must |
| `UC_HRM_013` | Hồ sơ nhân sự | Tạo hồ sơ nhân sự mới | Must |
| `UC_HRM_014` | Hồ sơ nhân sự | Cập nhật thông tin cá nhân | Must |
| `UC_HRM_015` | Hồ sơ nhân sự | NV tự cập nhật hồ sơ | Should |
| `UC_HRM_016` | Hồ sơ nhân sự | Upload ảnh đại diện | Could |
| `UC_HRM_017` | Hồ sơ nhân sự | Upload giấy tờ tùy thân | Must |
| `UC_HRM_018` | Hồ sơ nhân sự | Gắn nhân sự vào đơn vị chính | Must |
| `UC_HRM_019` | Hồ sơ nhân sự | Gắn nhân sự vào bộ phận | Must |
| `UC_HRM_020` | Hồ sơ nhân sự | Gắn chức danh / level | Must |
| `UC_HRM_021` | Hồ sơ nhân sự | Gắn loại nhân sự | Must |
| `UC_HRM_022` | Hồ sơ nhân sự | Gắn nhiều nhãn hồ sơ | Should |
| `UC_HRM_023` | Hồ sơ nhân sự | Quản lý người thân / liên hệ khẩn | Should |
| `UC_HRM_024` | Hồ sơ nhân sự | Quản lý trình độ / kỹ năng | Could |
| `UC_HRM_025` | Hồ sơ nhân sự | Tìm kiếm nhân sự đa tiêu chí | Must |
| `UC_HRM_026` | Hồ sơ nhân sự | Xuất danh sách nhân sự Excel | Must |
| `UC_HRM_027` | Hồ sơ nhân sự | Khóa hồ sơ đã nghỉ | Must |
| `UC_HRM_028` | Hồ sơ nhân sự | Xem hồ sơ theo quyền | Must |
| `UC_HRM_029` | Trạng thái & biến động nhân sự | Chuyển trạng thái Thử việc | Must |
| `UC_HRM_030` | Trạng thái & biến động nhân sự | Chuyển trạng thái Chính thức | Must |
| `UC_HRM_031` | Trạng thái & biến động nhân sự | Chuyển trạng thái Tạm nghỉ | Should |
| `UC_HRM_032` | Trạng thái & biến động nhân sự | Chuyển trạng thái Nghỉ việc | Must |
| `UC_HRM_033` | Trạng thái & biến động nhân sự | Lịch sử thay đổi trạng thái | Must |
| `UC_HRM_034` | Trạng thái & biến động nhân sự | Điều chuyển đơn vị / bộ phận | Must |
| `UC_HRM_035` | Trạng thái & biến động nhân sự | Thăng chức / đổi chức danh | Must |
| `UC_HRM_036` | Trạng thái & biến động nhân sự | Cảnh báo sắp hết hạn thử việc | Must |
| `UC_HRM_037` | Trạng thái & biến động nhân sự | Báo cáo biến động nhân sự | Should |
| `UC_HRM_038` | Hợp đồng lao động | Tạo hợp đồng lao động | Must |
| `UC_HRM_039` | Hợp đồng lao động | Tạo phụ lục hợp đồng | Must |
| `UC_HRM_040` | Hợp đồng lao động | Upload bản scan hợp đồng | Must |
| `UC_HRM_041` | Hợp đồng lao động | Gia hạn hợp đồng | Must |
| `UC_HRM_042` | Hợp đồng lao động | Thanh lý / chấm dứt hợp đồng | Must |
| `UC_HRM_043` | Hợp đồng lao động | Cảnh báo hết hạn hợp đồng | Must |
| `UC_HRM_044` | Hợp đồng lao động | In / xuất mẫu hợp đồng | Should |
| `UC_HRM_045` | Hợp đồng lao động | Quản lý lương hợp đồng | Must |
| `UC_HRM_046` | Hợp đồng lao động | Lịch sử hợp đồng theo nhân sự | Must |
| `UC_HRM_047` | Tuyển dụng – nhu cầu | Tạo phiếu đề xuất tuyển dụng | Must |
| `UC_HRM_048` | Tuyển dụng – nhu cầu | Chọn vị trí & số lượng cần tuyển | Must |
| `UC_HRM_049` | Tuyển dụng – nhu cầu | Nhập lý do tuyển dụng | Must |
| `UC_HRM_050` | Tuyển dụng – nhu cầu | Gửi phiếu đề xuất đi duyệt | Must |
| `UC_HRM_051` | Tuyển dụng – nhu cầu | Duyệt / từ chối đề xuất | Must |
| `UC_HRM_052` | Tuyển dụng – nhu cầu | Xem lịch sử duyệt đề xuất | Should |
| `UC_HRM_053` | Tuyển dụng – nhu cầu | Đóng / hủy phiếu đề xuất | Should |
| `UC_HRM_054` | Tuyển dụng – đăng tin & ứng viên | Tạo tin tuyển từ phiếu đã duyệt | Must |
| `UC_HRM_055` | Tuyển dụng – đăng tin & ứng viên | Ghi nhận kênh đăng tuyển | Should |
| `UC_HRM_056` | Tuyển dụng – đăng tin & ứng viên | Nhập hồ sơ ứng viên | Must |
| `UC_HRM_057` | Tuyển dụng – đăng tin & ứng viên | Upload file CV | Must |
| `UC_HRM_058` | Tuyển dụng – đăng tin & ứng viên | Import ứng viên hàng loạt | Could |
| `UC_HRM_059` | Tuyển dụng – đăng tin & ứng viên | Sơ loại ứng viên | Must |
| `UC_HRM_060` | Tuyển dụng – đăng tin & ứng viên | Chuyển ứng viên cho đơn vị đánh giá | Must |
| `UC_HRM_061` | Tuyển dụng – đăng tin & ứng viên | Form đánh giá ứng viên | Must |
| `UC_HRM_062` | Tuyển dụng – đăng tin & ứng viên | Từ chối / chấp nhận ứng viên | Must |
| `UC_HRM_063` | Tuyển dụng – đăng tin & ứng viên | Pipeline trạng thái ứng viên | Must |
| `UC_HRM_064` | Tuyển dụng – đăng tin & ứng viên | Lịch sử chăm sóc ứng viên | Should |
| `UC_HRM_065` | Tuyển dụng – đăng tin & ứng viên | Báo cáo hiệu quả kênh tuyển | Should |
| `UC_HRM_066` | Onboarding | Cấu hình thời hạn onboarding | Must |
| `UC_HRM_067` | Onboarding | Cấu hình thời hạn thử việc | Must |
| `UC_HRM_068` | Onboarding | Tạo hồ sơ nhân viên mới từ ứng viên | Must |
| `UC_HRM_069` | Onboarding | Gán người hướng dẫn | Should |
| `UC_HRM_070` | Onboarding | Checklist onboarding | Must |
| `UC_HRM_071` | Onboarding | Upload chứng chỉ / giấy tờ | Must |
| `UC_HRM_072` | Onboarding | Đánh giá kết thúc thử việc | Must |
| `UC_HRM_073` | Onboarding | Chuyển thử việc thành chính thức | Must |
| `UC_HRM_074` | Onboarding | Cảnh báo hết hạn thử việc | Must |
| `UC_HRM_075` | Định biên | Khai báo định biên theo đơn vị | Must |
| `UC_HRM_076` | Định biên | Khai báo định biên theo ca | Must |
| `UC_HRM_077` | Định biên | Khai báo định biên theo bộ phận | Should |
| `UC_HRM_078` | Định biên | So sánh thực tế vs định biên | Must |
| `UC_HRM_079` | Định biên | Cảnh báo thiếu người | Must |
| `UC_HRM_080` | Định biên | Duyệt thay đổi định biên | Should |
| `UC_HRM_081` | Ca làm việc | Tạo mẫu ca làm việc | Must |
| `UC_HRM_082` | Ca làm việc | Xếp lịch ca nhân viên | Must |
| `UC_HRM_083` | Ca làm việc | Xếp lịch ca theo tuần / tháng | Must |
| `UC_HRM_084` | Ca làm việc | Đổi ca giữa nhân viên | Should |
| `UC_HRM_085` | Ca làm việc | Hủy lịch ca | Must |
| `UC_HRM_086` | Ca làm việc | Xem lịch ca theo đơn vị | Must |
| `UC_HRM_087` | Ca làm việc | Xem lịch ca cá nhân trên APP | Must |
| `UC_HRM_088` | Ca làm việc | Import lịch ca Excel | Could |
| `UC_HRM_089` | Ca làm việc | Sao chép lịch ca | Should |
| `UC_HRM_090` | Ca làm việc | Khóa sổ lịch ca theo kỳ | Should |
| `UC_HRM_091` | Ca làm việc | In / xuất lịch ca | Should |
| `UC_HRM_092` | Điều động nhân sự | Tạo lệnh điều động | Should |
| `UC_HRM_093` | Điều động nhân sự | Đề xuất nhu cầu điều động | Should |
| `UC_HRM_094` | Điều động nhân sự | Nhận lệnh điều động trên APP | Should |
| `UC_HRM_095` | Điều động nhân sự | Theo dõi nhân sự điều động | Should |
| `UC_HRM_096` | Điều động nhân sự | Gắn nhãn công điều động khi chấm | Should |
| `UC_HRM_097` | Điều động nhân sự | Báo cáo giờ / chi phí điều động | Should |
| `UC_HRM_098` | Cấu hình chấm công | Cấu hình chấm vân tay / sinh trắc | Must |
| `UC_HRM_099` | Cấu hình chấm công | Cấu hình chấm APP điện thoại | Must |
| `UC_HRM_100` | Cấu hình chấm công | Cấu hình chấm QR / mã nhân sự | Should |
| `UC_HRM_101` | Cấu hình chấm công | Đăng ký thiết bị chấm | Must |
| `UC_HRM_102` | Cấu hình chấm công | Cấu hình geo-fence điểm chấm | Should |
| `UC_HRM_103` | Cấu hình chấm công | Cấu hình quy tắc đi trễ | Must |
| `UC_HRM_104` | Cấu hình chấm công | Cấu hình mức trừ công khi trễ | Must |
| `UC_HRM_105` | Cấu hình chấm công | Cấu hình quên check-out | Must |
| `UC_HRM_106` | Cấu hình chấm công | Cấu hình thời hạn xin điều chỉnh | Must |
| `UC_HRM_107` | Cấu hình chấm công | Cấu hình làm thêm giờ (OT) | Should |
| `UC_HRM_108` | Cấu hình chấm công | Cấu hình ca đêm / ngày lễ | Should |
| `UC_HRM_109` | Thực hiện chấm công | Check-in đầu ca | Must |
| `UC_HRM_110` | Thực hiện chấm công | Check-out cuối ca | Must |
| `UC_HRM_111` | Thực hiện chấm công | Xem lịch sử chấm cá nhân | Must |
| `UC_HRM_112` | Thực hiện chấm công | Bảng chấm công theo đơn vị | Must |
| `UC_HRM_113` | Thực hiện chấm công | Bảng chấm công toàn công ty | Must |
| `UC_HRM_114` | Thực hiện chấm công | Cảnh báo thiếu chấm realtime | Should |
| `UC_HRM_115` | Thực hiện chấm công | Tự tính phút đi trễ | Must |
| `UC_HRM_116` | Thực hiện chấm công | Tự trừ công do đi trễ | Must |
| `UC_HRM_117` | Thực hiện chấm công | Đánh dấu quên chấm | Must |
| `UC_HRM_118` | Thực hiện chấm công | Đồng bộ dữ liệu từ máy chấm | Must |
| `UC_HRM_119` | Thực hiện chấm công | Xử lý công OT tự động | Should |
| `UC_HRM_120` | Điều chỉnh & khóa công | Tạo phiếu xin điều chỉnh công | Must |
| `UC_HRM_121` | Điều chỉnh & khóa công | Đính kèm lý do / bằng chứng | Must |
| `UC_HRM_122` | Điều chỉnh & khóa công | Duyệt / từ chối điều chỉnh | Must |
| `UC_HRM_123` | Điều chỉnh & khóa công | Ghi nhận vi phạm đi trễ | Should |
| `UC_HRM_124` | Điều chỉnh & khóa công | Lập bảng phạt | Should |
| `UC_HRM_125` | Điều chỉnh & khóa công | Áp dụng phạt vào kỳ lương | Should |
| `UC_HRM_126` | Điều chỉnh & khóa công | Khóa bảng công theo kỳ | Must |
| `UC_HRM_127` | Điều chỉnh & khóa công | Mở khóa bảng công có kiểm soát | Must |
| `UC_HRM_128` | Điều chỉnh & khóa công | Xác nhận bảng công | Should |
| `UC_HRM_129` | Nghỉ phép & vắng mặt | Danh mục loại nghỉ | Must |
| `UC_HRM_130` | Nghỉ phép & vắng mặt | Cấu hình quỹ phép theo loại NS | Must |
| `UC_HRM_131` | Nghỉ phép & vắng mặt | Cấp phát / điều chỉnh quỹ phép | Must |
| `UC_HRM_132` | Nghỉ phép & vắng mặt | Tạo đơn xin nghỉ | Must |
| `UC_HRM_133` | Nghỉ phép & vắng mặt | Duyệt đơn nghỉ đa cấp | Must |
| `UC_HRM_134` | Nghỉ phép & vắng mặt | Hủy đơn nghỉ | Should |
| `UC_HRM_135` | Nghỉ phép & vắng mặt | Xem quỹ phép còn lại | Must |
| `UC_HRM_136` | Nghỉ phép & vắng mặt | Lịch nghỉ theo đơn vị | Should |
| `UC_HRM_137` | Nghỉ phép & vắng mặt | Import nghỉ lễ / ngày nghỉ | Must |
| `UC_HRM_138` | Nghỉ phép & vắng mặt | Báo cáo nghỉ / quỹ phép | Should |
| `UC_HRM_139` | Kỷ luật & khen thưởng | Ghi nhận quyết định khen thưởng | Should |
| `UC_HRM_140` | Kỷ luật & khen thưởng | Ghi nhận quyết định kỷ luật | Should |
| `UC_HRM_141` | Kỷ luật & khen thưởng | Đính kèm quyết định | Should |
| `UC_HRM_142` | Kỷ luật & khen thưởng | Ảnh hưởng lương / phụ cấp | Should |
| `UC_HRM_143` | Kỷ luật & khen thưởng | Báo cáo khen thưởng – kỷ luật | Could |
| `UC_HRM_144` | Offboarding / nghỉ việc | Tạo đơn nghỉ việc | Must |
| `UC_HRM_145` | Offboarding / nghỉ việc | Cấu hình / kiểm tra báo trước | Must |
| `UC_HRM_146` | Offboarding / nghỉ việc | Duyệt đơn nghỉ việc | Must |
| `UC_HRM_147` | Offboarding / nghỉ việc | Checklist bàn giao | Must |
| `UC_HRM_148` | Offboarding / nghỉ việc | Thu hồi quyền hệ thống | Must |
| `UC_HRM_149` | Offboarding / nghỉ việc | Quyết toán phép / lương nghỉ việc | Must |
| `UC_HRM_150` | Offboarding / nghỉ việc | Phỏng vấn nghỉ việc | Could |
| `UC_HRM_151` | Offboarding / nghỉ việc | Báo cáo nghỉ việc / lý do | Should |
| `UC_HRM_152` | Cấu hình lương & phụ cấp | Tạo thang bậc lương | Must |
| `UC_HRM_153` | Cấu hình lương & phụ cấp | Gán bậc lương theo nhân sự | Must |
| `UC_HRM_154` | Cấu hình lương & phụ cấp | Gán bậc theo trạng thái | Must |
| `UC_HRM_155` | Cấu hình lương & phụ cấp | Đơn giá giờ / ngày nhân viên | Must |
| `UC_HRM_156` | Cấu hình lương & phụ cấp | Quản lý lương thực tế chi trả | Must |
| `UC_HRM_157` | Cấu hình lương & phụ cấp | Danh mục phụ cấp | Must |
| `UC_HRM_158` | Cấu hình lương & phụ cấp | Rule phụ cấp theo ca | Must |
| `UC_HRM_159` | Cấu hình lương & phụ cấp | Rule phụ cấp đặc thù | Should |
| `UC_HRM_160` | Cấu hình lương & phụ cấp | Cấu hình bảo hiểm | Must |
| `UC_HRM_161` | Cấu hình lương & phụ cấp | Cấu hình thuế TNCN | Must |
| `UC_HRM_162` | Cấu hình lương & phụ cấp | Cấu hình tạm ứng / khấu trừ | Must |
| `UC_HRM_163` | Tính lương & chi trả | Tạo kỳ lương | Must |
| `UC_HRM_164` | Tính lương & chi trả | Tổng hợp công vào kỳ lương | Must |
| `UC_HRM_165` | Tính lương & chi trả | Tính lương tự động theo rule | Must |
| `UC_HRM_166` | Tính lương & chi trả | Nhập thưởng / phụ cấp phát sinh | Must |
| `UC_HRM_167` | Tính lương & chi trả | Nhập khấu trừ / tạm ứng | Must |
| `UC_HRM_168` | Tính lương & chi trả | Xem / chỉnh bảng lương chi tiết | Must |
| `UC_HRM_169` | Tính lương & chi trả | Xác nhận bảng lương | Must |
| `UC_HRM_170` | Tính lương & chi trả | Khóa kỳ lương | Must |
| `UC_HRM_171` | Tính lương & chi trả | Phiếu lương cá nhân (APP) | Must |
| `UC_HRM_172` | Tính lương & chi trả | Xuất bảng lương tổng hợp | Must |
| `UC_HRM_173` | Tính lương & chi trả | Xuất file chi lương ngân hàng | Must |
| `UC_HRM_174` | Tính lương & chi trả | Đồng bộ bút toán lương sang FIN | Should |
| `UC_HRM_175` | Tính lương & chi trả | Báo cáo chi phí lương theo đơn vị | Must |
| `UC_HRM_176` | Tính lương & chi trả | So sánh lương kỳ này / kỳ trước | Should |
| `UC_HRM_177` | Đánh giá hiệu suất | Mẫu đánh giá KPI / năng lực | Could |
| `UC_HRM_178` | Đánh giá hiệu suất | Tạo kỳ đánh giá | Could |
| `UC_HRM_179` | Đánh giá hiệu suất | Quản lý đánh giá nhân viên | Could |
| `UC_HRM_180` | Đánh giá hiệu suất | Nhân viên tự đánh giá | Later |
| `UC_HRM_181` | Đánh giá hiệu suất | Tổng hợp kết quả đánh giá | Could |
| `UC_HRM_182` | Báo cáo & dashboard HRM | Dashboard headcount & biến động | Must |
| `UC_HRM_183` | Báo cáo & dashboard HRM | Báo cáo công / OT / đi trễ | Must |
| `UC_HRM_184` | Báo cáo & dashboard HRM | Báo cáo tuyển dụng funnel | Should |
| `UC_HRM_185` | Báo cáo & dashboard HRM | Báo cáo quỹ phép | Should |
| `UC_HRM_186` | Báo cáo & dashboard HRM | Báo cáo chi phí nhân sự | Must |
| `UC_HRM_187` | Báo cáo & dashboard HRM | Báo cáo định biên vs thực tế | Should |

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

### 7.1. Cơ cấu tổ chức nhân sự (`HRM-01`)

Nhóm **Cơ cấu tổ chức nhân sự** gồm **11** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 11 |
| Must | 7 |

**Bảng 1. Đặc tả Use Case "Tạo sơ đồ tổ chức công ty"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_001 |
| **Tên Use Case** | Tạo sơ đồ tổ chức công ty |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Tạo sơ đồ tổ chức công ty" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Company org structure |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo sơ đồ tổ chức công ty» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo sơ đồ tổ chức công ty» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo sơ đồ tổ chức công ty» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở chức năng «Tạo sơ đồ tổ chức công ty» trong nhóm Cơ cấu tổ chức nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo sơ đồ tổ chức công ty» (Company org structure).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo sơ đồ tổ chức công ty» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo sơ đồ tổ chức công ty» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 2. Đặc tả Use Case "Quản lý khối vận hành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_002 |
| **Tên Use Case** | Quản lý khối vận hành |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý khối vận hành" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Operating divisions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý khối vận hành» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý khối vận hành» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý khối vận hành» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý khối vận hành» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý khối vận hành» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 3. Đặc tả Use Case "Quản lý khối sản xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_003 |
| **Tên Use Case** | Quản lý khối sản xuất |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý khối sản xuất" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Manufacturing divisions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý khối sản xuất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý khối sản xuất» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý khối sản xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý khối sản xuất» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý khối sản xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 4. Đặc tả Use Case "Quản lý danh mục điểm bán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_004 |
| **Tên Use Case** | Quản lý danh mục điểm bán |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý danh mục điểm bán" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Sales location master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý danh mục điểm bán» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý danh mục điểm bán» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý danh mục điểm bán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý danh mục điểm bán» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý danh mục điểm bán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 5. Đặc tả Use Case "Quản lý bộ phận trong đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_005 |
| **Tên Use Case** | Quản lý bộ phận trong đơn vị |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý bộ phận trong đơn vị" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Department within unit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý bộ phận trong đơn vị» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý bộ phận trong đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý bộ phận trong đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý bộ phận trong đơn vị» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý bộ phận trong đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 6. Đặc tả Use Case "Khai báo giờ làm việc theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_006 |
| **Tên Use Case** | Khai báo giờ làm việc theo đơn vị |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Khai báo giờ làm việc theo đơn vị" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Working hours by unit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khai báo giờ làm việc theo đơn vị» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khai báo giờ làm việc theo đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khai báo giờ làm việc theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Khai báo giờ làm việc theo đơn vị» trong Cơ cấu tổ chức nhân sự.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Working hours by unit) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khai báo giờ làm việc theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 7. Đặc tả Use Case "Quản lý chức danh nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_007 |
| **Tên Use Case** | Quản lý chức danh nhân sự |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý chức danh nhân sự" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Position titles |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý chức danh nhân sự» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý chức danh nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý chức danh nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý chức danh nhân sự» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý chức danh nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 8. Đặc tả Use Case "Quản lý vị trí công việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_008 |
| **Tên Use Case** | Quản lý vị trí công việc |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý vị trí công việc" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Job positions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý vị trí công việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý vị trí công việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý vị trí công việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý vị trí công việc» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý vị trí công việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 9. Đặc tả Use Case "Quản lý loại nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_009 |
| **Tên Use Case** | Quản lý loại nhân sự |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý loại nhân sự" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Employee types |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý loại nhân sự» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý loại nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý loại nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý loại nhân sự» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý loại nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 10. Đặc tả Use Case "Quản lý cấp bậc / level"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_010 |
| **Tên Use Case** | Quản lý cấp bậc / level |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý cấp bậc / level" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Grade/level master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý cấp bậc / level» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý cấp bậc / level» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý cấp bậc / level» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý cấp bậc / level» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cơ cấu tổ chức nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý cấp bậc / level» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 11. Đặc tả Use Case "Định nghĩa trung tâm chi phí NS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_011 |
| **Tên Use Case** | Định nghĩa trung tâm chi phí NS |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Định nghĩa trung tâm chi phí NS" thuộc nhóm Cơ cấu tổ chức nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: HR cost centers |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Định nghĩa trung tâm chi phí NS» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Định nghĩa trung tâm chi phí NS» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Định nghĩa trung tâm chi phí NS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Định nghĩa trung tâm chi phí NS» trong Cơ cấu tổ chức nhân sự.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (HR cost centers) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Định nghĩa trung tâm chi phí NS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

### 7.2. Hồ sơ nhân sự (`HRM-02`)

Nhóm **Hồ sơ nhân sự** gồm **17** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 17 |
| Must | 12 |

**Bảng 12. Đặc tả Use Case "Sinh mã nhân sự tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_012 |
| **Tên Use Case** | Sinh mã nhân sự tự động |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Sinh mã nhân sự tự động" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Auto employee ID |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sinh mã nhân sự tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sinh mã nhân sự tự động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sinh mã nhân sự tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Sinh mã nhân sự tự động» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Sinh mã nhân sự tự động» (Auto employee ID).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Sinh mã nhân sự tự động» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sinh mã nhân sự tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 13. Đặc tả Use Case "Tạo hồ sơ nhân sự mới"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_013 |
| **Tên Use Case** | Tạo hồ sơ nhân sự mới |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo hồ sơ nhân sự mới" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Create employee record |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo hồ sơ nhân sự mới» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo hồ sơ nhân sự mới» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hồ sơ nhân sự mới» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo hồ sơ nhân sự mới» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo hồ sơ nhân sự mới» (Create employee record).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo hồ sơ nhân sự mới» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo hồ sơ nhân sự mới» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 14. Đặc tả Use Case "Cập nhật thông tin cá nhân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_014 |
| **Tên Use Case** | Cập nhật thông tin cá nhân |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cập nhật thông tin cá nhân" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Personal info update |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cập nhật thông tin cá nhân» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cập nhật thông tin cá nhân» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cập nhật thông tin cá nhân» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Cập nhật thông tin cá nhân» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Cập nhật thông tin cá nhân» (Personal info update).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cập nhật thông tin cá nhân» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 15. Đặc tả Use Case "NV tự cập nhật hồ sơ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_015 |
| **Tên Use Case** | NV tự cập nhật hồ sơ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "NV tự cập nhật hồ sơ" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Self-service profile |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «NV tự cập nhật hồ sơ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «NV tự cập nhật hồ sơ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «NV tự cập nhật hồ sơ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Employee tìm và mở bản ghi liên quan tới «NV tự cập nhật hồ sơ» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «NV tự cập nhật hồ sơ» (Self-service profile).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «NV tự cập nhật hồ sơ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 16. Đặc tả Use Case "Upload ảnh đại diện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_016 |
| **Tên Use Case** | Upload ảnh đại diện |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Upload ảnh đại diện" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Profile photo |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload ảnh đại diện» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload ảnh đại diện» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload ảnh đại diện» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở bản ghi liên quan và chọn «Upload ảnh đại diện».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload ảnh đại diện» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 17. Đặc tả Use Case "Upload giấy tờ tùy thân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_017 |
| **Tên Use Case** | Upload giấy tờ tùy thân |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Upload giấy tờ tùy thân" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: ID documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload giấy tờ tùy thân» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload giấy tờ tùy thân» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload giấy tờ tùy thân» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở bản ghi liên quan và chọn «Upload giấy tờ tùy thân».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload giấy tờ tùy thân» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "Gắn nhân sự vào đơn vị chính"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_018 |
| **Tên Use Case** | Gắn nhân sự vào đơn vị chính |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gắn nhân sự vào đơn vị chính" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Primary org assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn nhân sự vào đơn vị chính» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn nhân sự vào đơn vị chính» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn nhân sự vào đơn vị chính» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Gắn nhân sự vào đơn vị chính» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Primary org assignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn nhân sự vào đơn vị chính».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn nhân sự vào đơn vị chính» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 19. Đặc tả Use Case "Gắn nhân sự vào bộ phận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_019 |
| **Tên Use Case** | Gắn nhân sự vào bộ phận |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gắn nhân sự vào bộ phận" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Department assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn nhân sự vào bộ phận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn nhân sự vào bộ phận» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn nhân sự vào bộ phận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Gắn nhân sự vào bộ phận» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Department assignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn nhân sự vào bộ phận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn nhân sự vào bộ phận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Gắn chức danh / level"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_020 |
| **Tên Use Case** | Gắn chức danh / level |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gắn chức danh / level" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Position & grade |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn chức danh / level» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn chức danh / level» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn chức danh / level» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Gắn chức danh / level» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Position & grade).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn chức danh / level».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn chức danh / level» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Gắn loại nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_021 |
| **Tên Use Case** | Gắn loại nhân sự |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gắn loại nhân sự" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Employee classification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn loại nhân sự» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn loại nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn loại nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Gắn loại nhân sự» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Employee classification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn loại nhân sự».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn loại nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Gắn nhiều nhãn hồ sơ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_022 |
| **Tên Use Case** | Gắn nhiều nhãn hồ sơ |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gắn nhiều nhãn hồ sơ" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Employee tags |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn nhiều nhãn hồ sơ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn nhiều nhãn hồ sơ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn nhiều nhãn hồ sơ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Gắn nhiều nhãn hồ sơ» trong nhóm Hồ sơ nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Employee tags).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn nhiều nhãn hồ sơ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn nhiều nhãn hồ sơ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 23. Đặc tả Use Case "Quản lý người thân / liên hệ khẩn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_023 |
| **Tên Use Case** | Quản lý người thân / liên hệ khẩn |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Quản lý người thân / liên hệ khẩn" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Emergency contacts |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý người thân / liên hệ khẩn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý người thân / liên hệ khẩn» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý người thân / liên hệ khẩn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở danh mục quản lý «Quản lý người thân / liên hệ khẩn» (nhân sự / hồ sơ / công – phép – lương; nhóm «Hồ sơ nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý người thân / liên hệ khẩn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 24. Đặc tả Use Case "Quản lý trình độ / kỹ năng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_024 |
| **Tên Use Case** | Quản lý trình độ / kỹ năng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Quản lý trình độ / kỹ năng" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Skills & qualifications |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý trình độ / kỹ năng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý trình độ / kỹ năng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý trình độ / kỹ năng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở danh mục quản lý «Quản lý trình độ / kỹ năng» (nhân sự / hồ sơ / công – phép – lương; nhóm «Hồ sơ nhân sự»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý trình độ / kỹ năng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 25. Đặc tả Use Case "Tìm kiếm nhân sự đa tiêu chí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_025 |
| **Tên Use Case** | Tìm kiếm nhân sự đa tiêu chí |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tìm kiếm nhân sự đa tiêu chí" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Advanced employee search |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tìm kiếm nhân sự đa tiêu chí» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tìm kiếm nhân sự đa tiêu chí» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tìm kiếm nhân sự đa tiêu chí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở «Tìm kiếm nhân sự đa tiêu chí» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Advanced employee search).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tìm kiếm nhân sự đa tiêu chí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Xuất danh sách nhân sự Excel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_026 |
| **Tên Use Case** | Xuất danh sách nhân sự Excel |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Xuất danh sách nhân sự Excel" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Export employee list |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất danh sách nhân sự Excel» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất danh sách nhân sự Excel» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất danh sách nhân sự Excel» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở «Xuất danh sách nhân sự Excel», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất danh sách nhân sự Excel» (Export employee list).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất danh sách nhân sự Excel» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 27. Đặc tả Use Case "Khóa hồ sơ đã nghỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_027 |
| **Tên Use Case** | Khóa hồ sơ đã nghỉ |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Khóa hồ sơ đã nghỉ" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Lock terminated records |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa hồ sơ đã nghỉ» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa hồ sơ đã nghỉ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa hồ sơ đã nghỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer chọn kỳ/ca/đối tượng cần khóa trong «Khóa hồ sơ đã nghỉ».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa hồ sơ đã nghỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 28. Đặc tả Use Case "Xem hồ sơ theo quyền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_028 |
| **Tên Use Case** | Xem hồ sơ theo quyền |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Xem hồ sơ theo quyền" thuộc nhóm Hồ sơ nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Field-level security |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem hồ sơ theo quyền» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem hồ sơ theo quyền» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem hồ sơ theo quyền» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở «Xem hồ sơ theo quyền» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Field-level security).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem hồ sơ theo quyền» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Trạng thái & biến động nhân sự (`HRM-03`)

Nhóm **Trạng thái & biến động nhân sự** gồm **9** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 7 |

**Bảng 29. Đặc tả Use Case "Chuyển trạng thái Thử việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_029 |
| **Tên Use Case** | Chuyển trạng thái Thử việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Chuyển trạng thái Thử việc" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Probation status |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển trạng thái Thử việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển trạng thái Thử việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển trạng thái Thử việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Chuyển trạng thái Thử việc» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Chuyển trạng thái Thử việc» (Probation status).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển trạng thái Thử việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 30. Đặc tả Use Case "Chuyển trạng thái Chính thức"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_030 |
| **Tên Use Case** | Chuyển trạng thái Chính thức |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Chuyển trạng thái Chính thức" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Regular status |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển trạng thái Chính thức» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển trạng thái Chính thức» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển trạng thái Chính thức» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Chuyển trạng thái Chính thức» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Chuyển trạng thái Chính thức» (Regular status).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển trạng thái Chính thức» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 31. Đặc tả Use Case "Chuyển trạng thái Tạm nghỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_031 |
| **Tên Use Case** | Chuyển trạng thái Tạm nghỉ |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Chuyển trạng thái Tạm nghỉ" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Temporary leave |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển trạng thái Tạm nghỉ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển trạng thái Tạm nghỉ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển trạng thái Tạm nghỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Chuyển trạng thái Tạm nghỉ» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Chuyển trạng thái Tạm nghỉ» (Temporary leave).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển trạng thái Tạm nghỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 32. Đặc tả Use Case "Chuyển trạng thái Nghỉ việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_032 |
| **Tên Use Case** | Chuyển trạng thái Nghỉ việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Chuyển trạng thái Nghỉ việc" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Termination |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển trạng thái Nghỉ việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển trạng thái Nghỉ việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển trạng thái Nghỉ việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Chuyển trạng thái Nghỉ việc» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Chuyển trạng thái Nghỉ việc» (Termination).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển trạng thái Nghỉ việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 33. Đặc tả Use Case "Lịch sử thay đổi trạng thái"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_033 |
| **Tên Use Case** | Lịch sử thay đổi trạng thái |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Lịch sử thay đổi trạng thái" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Status timeline |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử thay đổi trạng thái» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử thay đổi trạng thái» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử thay đổi trạng thái» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở «Lịch sử thay đổi trạng thái» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Status timeline).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử thay đổi trạng thái» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 34. Đặc tả Use Case "Điều chuyển đơn vị / bộ phận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_034 |
| **Tên Use Case** | Điều chuyển đơn vị / bộ phận |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Điều chuyển đơn vị / bộ phận" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Internal transfer |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điều chuyển đơn vị / bộ phận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điều chuyển đơn vị / bộ phận» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điều chuyển đơn vị / bộ phận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Điều chuyển đơn vị / bộ phận» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Điều chuyển đơn vị / bộ phận» (Internal transfer).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điều chuyển đơn vị / bộ phận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 35. Đặc tả Use Case "Thăng chức / đổi chức danh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_035 |
| **Tên Use Case** | Thăng chức / đổi chức danh |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Thăng chức / đổi chức danh" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Promotion/demotion |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thăng chức / đổi chức danh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thăng chức / đổi chức danh» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thăng chức / đổi chức danh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Thăng chức / đổi chức danh» trong nhóm Trạng thái & biến động nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Promotion/demotion).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thăng chức / đổi chức danh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thăng chức / đổi chức danh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Cảnh báo sắp hết hạn thử việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_036 |
| **Tên Use Case** | Cảnh báo sắp hết hạn thử việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cảnh báo sắp hết hạn thử việc" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Probation end reminder |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo sắp hết hạn thử việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo sắp hết hạn thử việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo sắp hết hạn thử việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc HR Officer kích hoạt kiểm tra điều kiện «Cảnh báo sắp hết hạn thử việc».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Probation end reminder).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo sắp hết hạn thử việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 37. Đặc tả Use Case "Báo cáo biến động nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_037 |
| **Tên Use Case** | Báo cáo biến động nhân sự |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Báo cáo biến động nhân sự" thuộc nhóm Trạng thái & biến động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount movement report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo biến động nhân sự» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo biến động nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo biến động nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở «Báo cáo biến động nhân sự» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Headcount movement report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo biến động nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Hợp đồng lao động (`HRM-04`)

Nhóm **Hợp đồng lao động** gồm **9** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 8 |

**Bảng 38. Đặc tả Use Case "Tạo hợp đồng lao động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_038 |
| **Tên Use Case** | Tạo hợp đồng lao động |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo hợp đồng lao động" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Create employment contract |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo hợp đồng lao động» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo hợp đồng lao động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hợp đồng lao động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo hợp đồng lao động» trong nhóm Hợp đồng lao động.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo hợp đồng lao động» (Create employment contract).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo hợp đồng lao động» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo hợp đồng lao động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 39. Đặc tả Use Case "Tạo phụ lục hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_039 |
| **Tên Use Case** | Tạo phụ lục hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo phụ lục hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract addendum |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phụ lục hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phụ lục hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phụ lục hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo phụ lục hợp đồng» trong nhóm Hợp đồng lao động.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phụ lục hợp đồng» (Contract addendum).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phụ lục hợp đồng» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phụ lục hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 40. Đặc tả Use Case "Upload bản scan hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_040 |
| **Tên Use Case** | Upload bản scan hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Upload bản scan hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract file attachment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload bản scan hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload bản scan hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload bản scan hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở bản ghi liên quan và chọn «Upload bản scan hợp đồng».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload bản scan hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 41. Đặc tả Use Case "Gia hạn hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_041 |
| **Tên Use Case** | Gia hạn hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gia hạn hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract renewal |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gia hạn hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gia hạn hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gia hạn hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer tìm và mở bản ghi liên quan tới «Gia hạn hợp đồng» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Gia hạn hợp đồng» (Contract renewal).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gia hạn hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 42. Đặc tả Use Case "Thanh lý / chấm dứt hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_042 |
| **Tên Use Case** | Thanh lý / chấm dứt hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Thanh lý / chấm dứt hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract termination |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thanh lý / chấm dứt hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-CAN-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thanh lý / chấm dứt hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thanh lý / chấm dứt hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer chọn đối tượng cần hủy/ngưng trong «Thanh lý / chấm dứt hợp đồng».<br>2. Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.<br>3. Yêu cầu lý do; xác nhận cảnh báo tác động.<br>4. Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.<br>5. Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thanh lý / chấm dứt hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 43. Đặc tả Use Case "Cảnh báo hết hạn hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_043 |
| **Tên Use Case** | Cảnh báo hết hạn hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cảnh báo hết hạn hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract expiry alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo hết hạn hợp đồng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo hết hạn hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo hết hạn hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc HR Officer kích hoạt kiểm tra điều kiện «Cảnh báo hết hạn hợp đồng».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Contract expiry alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo hết hạn hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 44. Đặc tả Use Case "In / xuất mẫu hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_044 |
| **Tên Use Case** | In / xuất mẫu hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "In / xuất mẫu hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract template |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In / xuất mẫu hợp đồng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In / xuất mẫu hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In / xuất mẫu hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. HR Officer mở «In / xuất mẫu hợp đồng», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «In / xuất mẫu hợp đồng» (Contract template).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In / xuất mẫu hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 45. Đặc tả Use Case "Quản lý lương hợp đồng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_045 |
| **Tên Use Case** | Quản lý lương hợp đồng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Quản lý lương hợp đồng" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract salary |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý lương hợp đồng» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý lương hợp đồng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý lương hợp đồng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở danh mục quản lý «Quản lý lương hợp đồng» (nhân sự / hồ sơ / công – phép – lương; nhóm «Hợp đồng lao động»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý lương hợp đồng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 46. Đặc tả Use Case "Lịch sử hợp đồng theo nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_046 |
| **Tên Use Case** | Lịch sử hợp đồng theo nhân sự |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Lịch sử hợp đồng theo nhân sự" thuộc nhóm Hợp đồng lao động trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Contract history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử hợp đồng theo nhân sự» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử hợp đồng theo nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử hợp đồng theo nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở «Lịch sử hợp đồng theo nhân sự» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Contract history).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử hợp đồng theo nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Tuyển dụng – nhu cầu (`HRM-05`)

Nhóm **Tuyển dụng – nhu cầu** gồm **7** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 47. Đặc tả Use Case "Tạo phiếu đề xuất tuyển dụng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_047 |
| **Tên Use Case** | Tạo phiếu đề xuất tuyển dụng |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Tạo phiếu đề xuất tuyển dụng" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Recruitment requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phiếu đề xuất tuyển dụng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phiếu đề xuất tuyển dụng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phiếu đề xuất tuyển dụng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager mở chức năng «Tạo phiếu đề xuất tuyển dụng» trong nhóm Tuyển dụng – nhu cầu.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phiếu đề xuất tuyển dụng» (Recruitment requisition).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phiếu đề xuất tuyển dụng» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phiếu đề xuất tuyển dụng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 48. Đặc tả Use Case "Chọn vị trí & số lượng cần tuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_048 |
| **Tên Use Case** | Chọn vị trí & số lượng cần tuyển |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Chọn vị trí & số lượng cần tuyển" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chọn vị trí & số lượng cần tuyển» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chọn vị trí & số lượng cần tuyển» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chọn vị trí & số lượng cần tuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Chọn vị trí & số lượng cần tuyển» trong nhóm Tuyển dụng – nhu cầu.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Headcount request).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chọn vị trí & số lượng cần tuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chọn vị trí & số lượng cần tuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 49. Đặc tả Use Case "Nhập lý do tuyển dụng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_049 |
| **Tên Use Case** | Nhập lý do tuyển dụng |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Nhập lý do tuyển dụng" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Recruitment reason |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập lý do tuyển dụng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập lý do tuyển dụng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập lý do tuyển dụng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Nhập lý do tuyển dụng» trong nhóm Tuyển dụng – nhu cầu.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Recruitment reason).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập lý do tuyển dụng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập lý do tuyển dụng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 50. Đặc tả Use Case "Gửi phiếu đề xuất đi duyệt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_050 |
| **Tên Use Case** | Gửi phiếu đề xuất đi duyệt |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Gửi phiếu đề xuất đi duyệt" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Submit for approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gửi phiếu đề xuất đi duyệt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gửi phiếu đề xuất đi duyệt» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi phiếu đề xuất đi duyệt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager mở hộp chờ / chứng từ cần xử lý cho «Gửi phiếu đề xuất đi duyệt».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Gửi phiếu đề xuất đi duyệt», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gửi phiếu đề xuất đi duyệt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 51. Đặc tả Use Case "Duyệt / từ chối đề xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_051 |
| **Tên Use Case** | Duyệt / từ chối đề xuất |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Duyệt / từ chối đề xuất" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Approval workflow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt / từ chối đề xuất» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt / từ chối đề xuất» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt / từ chối đề xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager mở hộp chờ / chứng từ cần xử lý cho «Duyệt / từ chối đề xuất».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt / từ chối đề xuất», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt / từ chối đề xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 52. Đặc tả Use Case "Xem lịch sử duyệt đề xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_052 |
| **Tên Use Case** | Xem lịch sử duyệt đề xuất |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Xem lịch sử duyệt đề xuất" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Approval history |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem lịch sử duyệt đề xuất» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem lịch sử duyệt đề xuất» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem lịch sử duyệt đề xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. Line Manager mở hộp chờ / chứng từ cần xử lý cho «Xem lịch sử duyệt đề xuất».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Xem lịch sử duyệt đề xuất», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem lịch sử duyệt đề xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 53. Đặc tả Use Case "Đóng / hủy phiếu đề xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_053 |
| **Tên Use Case** | Đóng / hủy phiếu đề xuất |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Đóng / hủy phiếu đề xuất" thuộc nhóm Tuyển dụng – nhu cầu trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Close requisition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng / hủy phiếu đề xuất» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-CAN-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng / hủy phiếu đề xuất» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng / hủy phiếu đề xuất» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager chọn đối tượng cần hủy/ngưng trong «Đóng / hủy phiếu đề xuất».<br>2. Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.<br>3. Yêu cầu lý do; xác nhận cảnh báo tác động.<br>4. Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.<br>5. Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng / hủy phiếu đề xuất» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

### 7.6. Tuyển dụng – đăng tin & ứng viên (`HRM-06`)

Nhóm **Tuyển dụng – đăng tin & ứng viên** gồm **12** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 12 |
| Must | 8 |

**Bảng 54. Đặc tả Use Case "Tạo tin tuyển từ phiếu đã duyệt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_054 |
| **Tên Use Case** | Tạo tin tuyển từ phiếu đã duyệt |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Tạo tin tuyển từ phiếu đã duyệt" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Job posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo tin tuyển từ phiếu đã duyệt» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo tin tuyển từ phiếu đã duyệt» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo tin tuyển từ phiếu đã duyệt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter mở hộp chờ / chứng từ cần xử lý cho «Tạo tin tuyển từ phiếu đã duyệt».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Tạo tin tuyển từ phiếu đã duyệt», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo tin tuyển từ phiếu đã duyệt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 55. Đặc tả Use Case "Ghi nhận kênh đăng tuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_055 |
| **Tên Use Case** | Ghi nhận kênh đăng tuyển |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Ghi nhận kênh đăng tuyển" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Recruitment channel log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận kênh đăng tuyển» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận kênh đăng tuyển» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận kênh đăng tuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Recruiter khởi tạo thao tác «Ghi nhận kênh đăng tuyển» trong nhóm Tuyển dụng – đăng tin & ứng viên.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Recruitment channel log).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận kênh đăng tuyển».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận kênh đăng tuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 56. Đặc tả Use Case "Nhập hồ sơ ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_056 |
| **Tên Use Case** | Nhập hồ sơ ứng viên |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Nhập hồ sơ ứng viên" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Candidate entry |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập hồ sơ ứng viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập hồ sơ ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập hồ sơ ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter khởi tạo thao tác «Nhập hồ sơ ứng viên» trong nhóm Tuyển dụng – đăng tin & ứng viên.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Candidate entry).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập hồ sơ ứng viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập hồ sơ ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 57. Đặc tả Use Case "Upload file CV"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_057 |
| **Tên Use Case** | Upload file CV |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Upload file CV" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: CV attachment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload file CV» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload file CV» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload file CV» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter mở bản ghi liên quan và chọn «Upload file CV».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload file CV» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 58. Đặc tả Use Case "Import ứng viên hàng loạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_058 |
| **Tên Use Case** | Import ứng viên hàng loạt |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Import ứng viên hàng loạt" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Bulk candidate import |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import ứng viên hàng loạt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-IMP-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import ứng viên hàng loạt» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import ứng viên hàng loạt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Recruiter tải file mẫu (nếu có) và chọn file import cho «Import ứng viên hàng loạt».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import ứng viên hàng loạt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn. |

**Bảng 59. Đặc tả Use Case "Sơ loại ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_059 |
| **Tên Use Case** | Sơ loại ứng viên |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Sơ loại ứng viên" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Candidate screening |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sơ loại ứng viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sơ loại ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sơ loại ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter khởi tạo thao tác «Sơ loại ứng viên» trong nhóm Tuyển dụng – đăng tin & ứng viên.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Candidate screening).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sơ loại ứng viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sơ loại ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 60. Đặc tả Use Case "Chuyển ứng viên cho đơn vị đánh giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_060 |
| **Tên Use Case** | Chuyển ứng viên cho đơn vị đánh giá |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Chuyển ứng viên cho đơn vị đánh giá" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Assign to hiring manager |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển ứng viên cho đơn vị đánh giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển ứng viên cho đơn vị đánh giá» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển ứng viên cho đơn vị đánh giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter khởi tạo thao tác «Chuyển ứng viên cho đơn vị đánh giá» trong nhóm Tuyển dụng – đăng tin & ứng viên.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Assign to hiring manager).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển ứng viên cho đơn vị đánh giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển ứng viên cho đơn vị đánh giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 61. Đặc tả Use Case "Form đánh giá ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_061 |
| **Tên Use Case** | Form đánh giá ứng viên |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Form đánh giá ứng viên" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Interview scorecard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Form đánh giá ứng viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Form đánh giá ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Form đánh giá ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter khởi tạo thao tác «Form đánh giá ứng viên» trong nhóm Tuyển dụng – đăng tin & ứng viên.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Interview scorecard).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Form đánh giá ứng viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Form đánh giá ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 62. Đặc tả Use Case "Từ chối / chấp nhận ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_062 |
| **Tên Use Case** | Từ chối / chấp nhận ứng viên |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Từ chối / chấp nhận ứng viên" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Hiring decision |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Từ chối / chấp nhận ứng viên» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Từ chối / chấp nhận ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Từ chối / chấp nhận ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter mở hộp chờ / chứng từ cần xử lý cho «Từ chối / chấp nhận ứng viên».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Từ chối / chấp nhận ứng viên», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Từ chối / chấp nhận ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 63. Đặc tả Use Case "Pipeline trạng thái ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_063 |
| **Tên Use Case** | Pipeline trạng thái ứng viên |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Pipeline trạng thái ứng viên" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Recruitment pipeline |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Pipeline trạng thái ứng viên» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Pipeline trạng thái ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Pipeline trạng thái ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Recruiter mở «Pipeline trạng thái ứng viên» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Recruitment pipeline).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Pipeline trạng thái ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 64. Đặc tả Use Case "Lịch sử chăm sóc ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_064 |
| **Tên Use Case** | Lịch sử chăm sóc ứng viên |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Lịch sử chăm sóc ứng viên" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Candidate communication log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch sử chăm sóc ứng viên» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch sử chăm sóc ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch sử chăm sóc ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Recruiter mở «Lịch sử chăm sóc ứng viên» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Candidate communication log).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch sử chăm sóc ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 65. Đặc tả Use Case "Báo cáo hiệu quả kênh tuyển"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_065 |
| **Tên Use Case** | Báo cáo hiệu quả kênh tuyển |
| **Tác nhân** | Recruiter |
| **Mô tả chức năng** | Cho phép Recruiter thực hiện chức năng "Báo cáo hiệu quả kênh tuyển" thuộc nhóm Tuyển dụng – đăng tin & ứng viên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Source effectiveness |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Recruiter] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo hiệu quả kênh tuyển» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo hiệu quả kênh tuyển» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo hiệu quả kênh tuyển» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Recruiter mở «Báo cáo hiệu quả kênh tuyển» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Source effectiveness); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo hiệu quả kênh tuyển» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Onboarding (`HRM-07`)

Nhóm **Onboarding** gồm **9** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 8 |

**Bảng 66. Đặc tả Use Case "Cấu hình thời hạn onboarding"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_066 |
| **Tên Use Case** | Cấu hình thời hạn onboarding |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cấu hình thời hạn onboarding" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Onboarding period setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thời hạn onboarding» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thời hạn onboarding» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thời hạn onboarding» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở màn hình cấu hình «Cấu hình thời hạn onboarding» trong Onboarding.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Onboarding period setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thời hạn onboarding» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 67. Đặc tả Use Case "Cấu hình thời hạn thử việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_067 |
| **Tên Use Case** | Cấu hình thời hạn thử việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cấu hình thời hạn thử việc" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Probation period setup |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thời hạn thử việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thời hạn thử việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thời hạn thử việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở màn hình cấu hình «Cấu hình thời hạn thử việc» trong Onboarding.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Probation period setup) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thời hạn thử việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 68. Đặc tả Use Case "Tạo hồ sơ nhân viên mới từ ứng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_068 |
| **Tên Use Case** | Tạo hồ sơ nhân viên mới từ ứng viên |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo hồ sơ nhân viên mới từ ứng viên" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: New hire from candidate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo hồ sơ nhân viên mới từ ứng viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo hồ sơ nhân viên mới từ ứng viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hồ sơ nhân viên mới từ ứng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo hồ sơ nhân viên mới từ ứng viên» trong nhóm Onboarding.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo hồ sơ nhân viên mới từ ứng viên» (New hire from candidate).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo hồ sơ nhân viên mới từ ứng viên» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo hồ sơ nhân viên mới từ ứng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 69. Đặc tả Use Case "Gán người hướng dẫn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_069 |
| **Tên Use Case** | Gán người hướng dẫn |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gán người hướng dẫn" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Assign buddy/mentor |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán người hướng dẫn» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán người hướng dẫn» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán người hướng dẫn» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer chọn đối tượng nguồn trong «Gán người hướng dẫn».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán người hướng dẫn» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 70. Đặc tả Use Case "Checklist onboarding"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_070 |
| **Tên Use Case** | Checklist onboarding |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Checklist onboarding" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Onboarding tasks |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist onboarding» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist onboarding» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist onboarding» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Checklist onboarding» trong nhóm Onboarding.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Onboarding tasks).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist onboarding».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist onboarding» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 71. Đặc tả Use Case "Upload chứng chỉ / giấy tờ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_071 |
| **Tên Use Case** | Upload chứng chỉ / giấy tờ |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Upload chứng chỉ / giấy tờ" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Required documents |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload chứng chỉ / giấy tờ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload chứng chỉ / giấy tờ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload chứng chỉ / giấy tờ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở bản ghi liên quan và chọn «Upload chứng chỉ / giấy tờ».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload chứng chỉ / giấy tờ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 72. Đặc tả Use Case "Đánh giá kết thúc thử việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_072 |
| **Tên Use Case** | Đánh giá kết thúc thử việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Đánh giá kết thúc thử việc" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Probation evaluation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá kết thúc thử việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá kết thúc thử việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá kết thúc thử việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Đánh giá kết thúc thử việc» trong nhóm Onboarding.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Probation evaluation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá kết thúc thử việc».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá kết thúc thử việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 73. Đặc tả Use Case "Chuyển thử việc thành chính thức"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_073 |
| **Tên Use Case** | Chuyển thử việc thành chính thức |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Chuyển thử việc thành chính thức" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Confirmation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chuyển thử việc thành chính thức» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chuyển thử việc thành chính thức» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chuyển thử việc thành chính thức» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Chuyển thử việc thành chính thức» trong nhóm Onboarding.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Confirmation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chuyển thử việc thành chính thức».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chuyển thử việc thành chính thức» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 74. Đặc tả Use Case "Cảnh báo hết hạn thử việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_074 |
| **Tên Use Case** | Cảnh báo hết hạn thử việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cảnh báo hết hạn thử việc" thuộc nhóm Onboarding trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Probation end alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo hết hạn thử việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo hết hạn thử việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo hết hạn thử việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc HR Officer kích hoạt kiểm tra điều kiện «Cảnh báo hết hạn thử việc».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Probation end alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo hết hạn thử việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.8. Định biên (`HRM-08`)

Nhóm **Định biên** gồm **6** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 75. Đặc tả Use Case "Khai báo định biên theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_075 |
| **Tên Use Case** | Khai báo định biên theo đơn vị |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Khai báo định biên theo đơn vị" thuộc nhóm Định biên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount plan by unit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khai báo định biên theo đơn vị» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khai báo định biên theo đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khai báo định biên theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Khai báo định biên theo đơn vị» trong Định biên.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Headcount plan by unit) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khai báo định biên theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 76. Đặc tả Use Case "Khai báo định biên theo ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_076 |
| **Tên Use Case** | Khai báo định biên theo ca |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Khai báo định biên theo ca" thuộc nhóm Định biên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount by shift |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khai báo định biên theo ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khai báo định biên theo ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khai báo định biên theo ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Khai báo định biên theo ca» trong Định biên.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Headcount by shift) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khai báo định biên theo ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 77. Đặc tả Use Case "Khai báo định biên theo bộ phận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_077 |
| **Tên Use Case** | Khai báo định biên theo bộ phận |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Khai báo định biên theo bộ phận" thuộc nhóm Định biên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount by department |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khai báo định biên theo bộ phận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khai báo định biên theo bộ phận» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khai báo định biên theo bộ phận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Khai báo định biên theo bộ phận» trong Định biên.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Headcount by department) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khai báo định biên theo bộ phận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 78. Đặc tả Use Case "So sánh thực tế vs định biên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_078 |
| **Tên Use Case** | So sánh thực tế vs định biên |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "So sánh thực tế vs định biên" thuộc nhóm Định biên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount gap analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh thực tế vs định biên» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh thực tế vs định biên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh thực tế vs định biên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở «So sánh thực tế vs định biên» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Headcount gap analysis); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh thực tế vs định biên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 79. Đặc tả Use Case "Cảnh báo thiếu người"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_079 |
| **Tên Use Case** | Cảnh báo thiếu người |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cảnh báo thiếu người" thuộc nhóm Định biên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Understaffing alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo thiếu người» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo thiếu người» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo thiếu người» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Job hệ thống hoặc HR Admin kích hoạt kiểm tra điều kiện «Cảnh báo thiếu người».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Understaffing alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo thiếu người» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 80. Đặc tả Use Case "Duyệt thay đổi định biên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_080 |
| **Tên Use Case** | Duyệt thay đổi định biên |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Duyệt thay đổi định biên" thuộc nhóm Định biên trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount change approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt thay đổi định biên» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt thay đổi định biên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt thay đổi định biên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình. |
| **Kịch bản chính** | 1. HR Admin mở hộp chờ / chứng từ cần xử lý cho «Duyệt thay đổi định biên».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt thay đổi định biên», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt thay đổi định biên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

### 7.9. Ca làm việc (`HRM-09`)

Nhóm **Ca làm việc** gồm **11** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 11 |
| Must | 6 |

**Bảng 81. Đặc tả Use Case "Tạo mẫu ca làm việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_081 |
| **Tên Use Case** | Tạo mẫu ca làm việc |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Tạo mẫu ca làm việc" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Shift templates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo mẫu ca làm việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo mẫu ca làm việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo mẫu ca làm việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager mở chức năng «Tạo mẫu ca làm việc» trong nhóm Ca làm việc.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo mẫu ca làm việc» (Shift templates).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo mẫu ca làm việc» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo mẫu ca làm việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 82. Đặc tả Use Case "Xếp lịch ca nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_082 |
| **Tên Use Case** | Xếp lịch ca nhân viên |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Xếp lịch ca nhân viên" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Shift scheduling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xếp lịch ca nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xếp lịch ca nhân viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xếp lịch ca nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager chọn đối tượng nguồn trong «Xếp lịch ca nhân viên».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xếp lịch ca nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 83. Đặc tả Use Case "Xếp lịch ca theo tuần / tháng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_083 |
| **Tên Use Case** | Xếp lịch ca theo tuần / tháng |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Xếp lịch ca theo tuần / tháng" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Calendar view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xếp lịch ca theo tuần / tháng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xếp lịch ca theo tuần / tháng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xếp lịch ca theo tuần / tháng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager chọn đối tượng nguồn trong «Xếp lịch ca theo tuần / tháng».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xếp lịch ca theo tuần / tháng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 84. Đặc tả Use Case "Đổi ca giữa nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_084 |
| **Tên Use Case** | Đổi ca giữa nhân viên |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Đổi ca giữa nhân viên" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Shift swap |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đổi ca giữa nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đổi ca giữa nhân viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đổi ca giữa nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Đổi ca giữa nhân viên» trong nhóm Ca làm việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Shift swap).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đổi ca giữa nhân viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đổi ca giữa nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 85. Đặc tả Use Case "Hủy lịch ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_085 |
| **Tên Use Case** | Hủy lịch ca |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Hủy lịch ca" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Cancel shift assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy lịch ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy lịch ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy lịch ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Hủy lịch ca» trong nhóm Ca làm việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Cancel shift assignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hủy lịch ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy lịch ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 86. Đặc tả Use Case "Xem lịch ca theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_086 |
| **Tên Use Case** | Xem lịch ca theo đơn vị |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Xem lịch ca theo đơn vị" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Manager shift view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem lịch ca theo đơn vị» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem lịch ca theo đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem lịch ca theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager mở «Xem lịch ca theo đơn vị» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Manager shift view).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem lịch ca theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 87. Đặc tả Use Case "Xem lịch ca cá nhân trên APP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_087 |
| **Tên Use Case** | Xem lịch ca cá nhân trên APP |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Xem lịch ca cá nhân trên APP" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Employee shift view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem lịch ca cá nhân trên APP» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem lịch ca cá nhân trên APP» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem lịch ca cá nhân trên APP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Line Manager mở «Xem lịch ca cá nhân trên APP» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Employee shift view).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem lịch ca cá nhân trên APP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 88. Đặc tả Use Case "Import lịch ca Excel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_088 |
| **Tên Use Case** | Import lịch ca Excel |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Import lịch ca Excel" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Bulk shift import |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import lịch ca Excel» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-IMP-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import lịch ca Excel» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import lịch ca Excel» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager tải file mẫu (nếu có) và chọn file import cho «Import lịch ca Excel».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import lịch ca Excel» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn. |

**Bảng 89. Đặc tả Use Case "Sao chép lịch ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_089 |
| **Tên Use Case** | Sao chép lịch ca |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Sao chép lịch ca" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Copy shift schedule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Sao chép lịch ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Sao chép lịch ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Sao chép lịch ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Sao chép lịch ca» trong nhóm Ca làm việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Copy shift schedule).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Sao chép lịch ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Sao chép lịch ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 90. Đặc tả Use Case "Khóa sổ lịch ca theo kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_090 |
| **Tên Use Case** | Khóa sổ lịch ca theo kỳ |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Khóa sổ lịch ca theo kỳ" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Lock shift roster |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa sổ lịch ca theo kỳ» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa sổ lịch ca theo kỳ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa sổ lịch ca theo kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. Line Manager chọn kỳ/ca/đối tượng cần khóa trong «Khóa sổ lịch ca theo kỳ».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa sổ lịch ca theo kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 91. Đặc tả Use Case "In / xuất lịch ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_091 |
| **Tên Use Case** | In / xuất lịch ca |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "In / xuất lịch ca" thuộc nhóm Ca làm việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Export shift schedule |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «In / xuất lịch ca» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «In / xuất lịch ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «In / xuất lịch ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope. |
| **Kịch bản chính** | 1. Line Manager mở «In / xuất lịch ca», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «In / xuất lịch ca» (Export shift schedule).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «In / xuất lịch ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.10. Điều động nhân sự (`HRM-10`)

Nhóm **Điều động nhân sự** gồm **6** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 0 |

**Bảng 92. Đặc tả Use Case "Tạo lệnh điều động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_092 |
| **Tên Use Case** | Tạo lệnh điều động |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo lệnh điều động" thuộc nhóm Điều động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Secondment order |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo lệnh điều động» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo lệnh điều động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo lệnh điều động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo lệnh điều động» trong nhóm Điều động nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo lệnh điều động» (Secondment order).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo lệnh điều động» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo lệnh điều động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 93. Đặc tả Use Case "Đề xuất nhu cầu điều động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_093 |
| **Tên Use Case** | Đề xuất nhu cầu điều động |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Đề xuất nhu cầu điều động" thuộc nhóm Điều động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Secondment request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đề xuất nhu cầu điều động» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đề xuất nhu cầu điều động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đề xuất nhu cầu điều động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Đề xuất nhu cầu điều động» trong nhóm Điều động nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Secondment request).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đề xuất nhu cầu điều động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đề xuất nhu cầu điều động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 94. Đặc tả Use Case "Nhận lệnh điều động trên APP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_094 |
| **Tên Use Case** | Nhận lệnh điều động trên APP |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Nhận lệnh điều động trên APP" thuộc nhóm Điều động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Accept assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhận lệnh điều động trên APP» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhận lệnh điều động trên APP» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhận lệnh điều động trên APP» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Nhận lệnh điều động trên APP» trong nhóm Điều động nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Accept assignment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhận lệnh điều động trên APP».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhận lệnh điều động trên APP» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 95. Đặc tả Use Case "Theo dõi nhân sự điều động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_095 |
| **Tên Use Case** | Theo dõi nhân sự điều động |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Theo dõi nhân sự điều động" thuộc nhóm Điều động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Secondment tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi nhân sự điều động» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi nhân sự điều động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi nhân sự điều động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Theo dõi nhân sự điều động» trong nhóm Điều động nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Secondment tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi nhân sự điều động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi nhân sự điều động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 96. Đặc tả Use Case "Gắn nhãn công điều động khi chấm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_096 |
| **Tên Use Case** | Gắn nhãn công điều động khi chấm |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Gắn nhãn công điều động khi chấm" thuộc nhóm Điều động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Secondment attendance tag |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn nhãn công điều động khi chấm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn nhãn công điều động khi chấm» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn nhãn công điều động khi chấm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Gắn nhãn công điều động khi chấm» trong nhóm Điều động nhân sự.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Secondment attendance tag).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn nhãn công điều động khi chấm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn nhãn công điều động khi chấm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 97. Đặc tả Use Case "Báo cáo giờ / chi phí điều động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_097 |
| **Tên Use Case** | Báo cáo giờ / chi phí điều động |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Báo cáo giờ / chi phí điều động" thuộc nhóm Điều động nhân sự trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Secondment cost report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo giờ / chi phí điều động» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo giờ / chi phí điều động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo giờ / chi phí điều động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở «Báo cáo giờ / chi phí điều động» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Secondment cost report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo giờ / chi phí điều động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.11. Cấu hình chấm công (`HRM-11`)

Nhóm **Cấu hình chấm công** gồm **11** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 11 |
| Must | 7 |

**Bảng 98. Đặc tả Use Case "Cấu hình chấm vân tay / sinh trắc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_098 |
| **Tên Use Case** | Cấu hình chấm vân tay / sinh trắc |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình chấm vân tay / sinh trắc" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Biometric settings |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình chấm vân tay / sinh trắc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình chấm vân tay / sinh trắc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình chấm vân tay / sinh trắc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình chấm vân tay / sinh trắc» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Biometric settings) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình chấm vân tay / sinh trắc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 99. Đặc tả Use Case "Cấu hình chấm APP điện thoại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_099 |
| **Tên Use Case** | Cấu hình chấm APP điện thoại |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình chấm APP điện thoại" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Mobile punch settings |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình chấm APP điện thoại» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình chấm APP điện thoại» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình chấm APP điện thoại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình chấm APP điện thoại» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Mobile punch settings) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình chấm APP điện thoại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 100. Đặc tả Use Case "Cấu hình chấm QR / mã nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_100 |
| **Tên Use Case** | Cấu hình chấm QR / mã nhân sự |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình chấm QR / mã nhân sự" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: QR code punch |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình chấm QR / mã nhân sự» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình chấm QR / mã nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình chấm QR / mã nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình chấm QR / mã nhân sự» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (QR code punch) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình chấm QR / mã nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 101. Đặc tả Use Case "Đăng ký thiết bị chấm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_101 |
| **Tên Use Case** | Đăng ký thiết bị chấm |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Đăng ký thiết bị chấm" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Time clock registration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng ký thiết bị chấm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng ký thiết bị chấm» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng ký thiết bị chấm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin khởi tạo thao tác «Đăng ký thiết bị chấm» trong nhóm Cấu hình chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Time clock registration).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng ký thiết bị chấm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng ký thiết bị chấm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 102. Đặc tả Use Case "Cấu hình geo-fence điểm chấm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_102 |
| **Tên Use Case** | Cấu hình geo-fence điểm chấm |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình geo-fence điểm chấm" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: GPS-based attendance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình geo-fence điểm chấm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình geo-fence điểm chấm» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình geo-fence điểm chấm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình geo-fence điểm chấm» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (GPS-based attendance) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình geo-fence điểm chấm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 103. Đặc tả Use Case "Cấu hình quy tắc đi trễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_103 |
| **Tên Use Case** | Cấu hình quy tắc đi trễ |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình quy tắc đi trễ" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Late arrival rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình quy tắc đi trễ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình quy tắc đi trễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình quy tắc đi trễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình quy tắc đi trễ» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Late arrival rules) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình quy tắc đi trễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 104. Đặc tả Use Case "Cấu hình mức trừ công khi trễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_104 |
| **Tên Use Case** | Cấu hình mức trừ công khi trễ |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình mức trừ công khi trễ" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Late penalty rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình mức trừ công khi trễ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình mức trừ công khi trễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình mức trừ công khi trễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình mức trừ công khi trễ» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Late penalty rules) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình mức trừ công khi trễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 105. Đặc tả Use Case "Cấu hình quên check-out"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_105 |
| **Tên Use Case** | Cấu hình quên check-out |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình quên check-out" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Missing checkout handling |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình quên check-out» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình quên check-out» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình quên check-out» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình quên check-out» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Missing checkout handling) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình quên check-out» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 106. Đặc tả Use Case "Cấu hình thời hạn xin điều chỉnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_106 |
| **Tên Use Case** | Cấu hình thời hạn xin điều chỉnh |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình thời hạn xin điều chỉnh" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Adjustment request window |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thời hạn xin điều chỉnh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thời hạn xin điều chỉnh» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thời hạn xin điều chỉnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình thời hạn xin điều chỉnh» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Adjustment request window) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thời hạn xin điều chỉnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 107. Đặc tả Use Case "Cấu hình làm thêm giờ (OT)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_107 |
| **Tên Use Case** | Cấu hình làm thêm giờ (OT) |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình làm thêm giờ (OT)" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Overtime rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình làm thêm giờ (OT)» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình làm thêm giờ (OT)» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình làm thêm giờ (OT)» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình làm thêm giờ (OT)» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Overtime rules) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình làm thêm giờ (OT)» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 108. Đặc tả Use Case "Cấu hình ca đêm / ngày lễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_108 |
| **Tên Use Case** | Cấu hình ca đêm / ngày lễ |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình ca đêm / ngày lễ" thuộc nhóm Cấu hình chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Special shift rates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình ca đêm / ngày lễ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình ca đêm / ngày lễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình ca đêm / ngày lễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình ca đêm / ngày lễ» trong Cấu hình chấm công.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Special shift rates) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình ca đêm / ngày lễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

### 7.12. Thực hiện chấm công (`HRM-12`)

Nhóm **Thực hiện chấm công** gồm **11** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 11 |
| Must | 9 |

**Bảng 109. Đặc tả Use Case "Check-in đầu ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_109 |
| **Tên Use Case** | Check-in đầu ca |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Check-in đầu ca" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Clock in |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Check-in đầu ca» đã được cấu hình trong phạm vi data scope.<br>• Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-RCV-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Check-in đầu ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Check-in đầu ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee mở chứng từ nhận liên quan «Check-in đầu ca».<br>2. Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.<br>3. Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.<br>4. Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.<br>5. Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Check-in đầu ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý. |

**Bảng 110. Đặc tả Use Case "Check-out cuối ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_110 |
| **Tên Use Case** | Check-out cuối ca |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Check-out cuối ca" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Clock out |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Check-out cuối ca» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Check-out cuối ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Check-out cuối ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Check-out cuối ca» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Clock out).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Check-out cuối ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Check-out cuối ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 111. Đặc tả Use Case "Xem lịch sử chấm cá nhân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_111 |
| **Tên Use Case** | Xem lịch sử chấm cá nhân |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Xem lịch sử chấm cá nhân" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: My attendance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem lịch sử chấm cá nhân» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem lịch sử chấm cá nhân» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem lịch sử chấm cá nhân» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee mở «Xem lịch sử chấm cá nhân» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (My attendance).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem lịch sử chấm cá nhân» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 112. Đặc tả Use Case "Bảng chấm công theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_112 |
| **Tên Use Case** | Bảng chấm công theo đơn vị |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Bảng chấm công theo đơn vị" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Unit timesheet |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng chấm công theo đơn vị» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng chấm công theo đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng chấm công theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Bảng chấm công theo đơn vị» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Unit timesheet).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng chấm công theo đơn vị».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng chấm công theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 113. Đặc tả Use Case "Bảng chấm công toàn công ty"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_113 |
| **Tên Use Case** | Bảng chấm công toàn công ty |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Bảng chấm công toàn công ty" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Company timesheet |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bảng chấm công toàn công ty» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bảng chấm công toàn công ty» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bảng chấm công toàn công ty» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Bảng chấm công toàn công ty» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Company timesheet).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bảng chấm công toàn công ty».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bảng chấm công toàn công ty» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 114. Đặc tả Use Case "Cảnh báo thiếu chấm realtime"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_114 |
| **Tên Use Case** | Cảnh báo thiếu chấm realtime |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Cảnh báo thiếu chấm realtime" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Missing punch alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo thiếu chấm realtime» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo thiếu chấm realtime» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo thiếu chấm realtime» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc Employee kích hoạt kiểm tra điều kiện «Cảnh báo thiếu chấm realtime».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Missing punch alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo thiếu chấm realtime» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 115. Đặc tả Use Case "Tự tính phút đi trễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_115 |
| **Tên Use Case** | Tự tính phút đi trễ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Tự tính phút đi trễ" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Auto late calculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tự tính phút đi trễ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tự tính phút đi trễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tự tính phút đi trễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Tự tính phút đi trễ» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Auto late calculation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tự tính phút đi trễ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tự tính phút đi trễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 116. Đặc tả Use Case "Tự trừ công do đi trễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_116 |
| **Tên Use Case** | Tự trừ công do đi trễ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Tự trừ công do đi trễ" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Auto attendance deduction |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tự trừ công do đi trễ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tự trừ công do đi trễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tự trừ công do đi trễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Tự trừ công do đi trễ» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Auto attendance deduction).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tự trừ công do đi trễ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tự trừ công do đi trễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 117. Đặc tả Use Case "Đánh dấu quên chấm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_117 |
| **Tên Use Case** | Đánh dấu quên chấm |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Đánh dấu quên chấm" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Missing punch flag |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh dấu quên chấm» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh dấu quên chấm» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh dấu quên chấm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Đánh dấu quên chấm» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Missing punch flag).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh dấu quên chấm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh dấu quên chấm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 118. Đặc tả Use Case "Đồng bộ dữ liệu từ máy chấm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_118 |
| **Tên Use Case** | Đồng bộ dữ liệu từ máy chấm |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Đồng bộ dữ liệu từ máy chấm" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Time clock sync |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ dữ liệu từ máy chấm» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ dữ liệu từ máy chấm» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ dữ liệu từ máy chấm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Đồng bộ dữ liệu từ máy chấm» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Time clock sync).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ dữ liệu từ máy chấm».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ dữ liệu từ máy chấm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 119. Đặc tả Use Case "Xử lý công OT tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_119 |
| **Tên Use Case** | Xử lý công OT tự động |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Xử lý công OT tự động" thuộc nhóm Thực hiện chấm công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: OT auto recognition |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xử lý công OT tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xử lý công OT tự động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xử lý công OT tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Xử lý công OT tự động» trong nhóm Thực hiện chấm công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (OT auto recognition).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xử lý công OT tự động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xử lý công OT tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

### 7.13. Điều chỉnh & khóa công (`HRM-13`)

Nhóm **Điều chỉnh & khóa công** gồm **9** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 5 |

**Bảng 120. Đặc tả Use Case "Tạo phiếu xin điều chỉnh công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_120 |
| **Tên Use Case** | Tạo phiếu xin điều chỉnh công |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo phiếu xin điều chỉnh công" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Attendance adjustment request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo phiếu xin điều chỉnh công» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo phiếu xin điều chỉnh công» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo phiếu xin điều chỉnh công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo phiếu xin điều chỉnh công» trong nhóm Điều chỉnh & khóa công.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo phiếu xin điều chỉnh công» (Attendance adjustment request).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo phiếu xin điều chỉnh công» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo phiếu xin điều chỉnh công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 121. Đặc tả Use Case "Đính kèm lý do / bằng chứng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_121 |
| **Tên Use Case** | Đính kèm lý do / bằng chứng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Đính kèm lý do / bằng chứng" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Evidence attachment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đính kèm lý do / bằng chứng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đính kèm lý do / bằng chứng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đính kèm lý do / bằng chứng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở bản ghi liên quan và chọn «Đính kèm lý do / bằng chứng».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đính kèm lý do / bằng chứng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 122. Đặc tả Use Case "Duyệt / từ chối điều chỉnh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_122 |
| **Tên Use Case** | Duyệt / từ chối điều chỉnh |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Duyệt / từ chối điều chỉnh" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Approve adjustment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt / từ chối điều chỉnh» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt / từ chối điều chỉnh» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt / từ chối điều chỉnh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở hộp chờ / chứng từ cần xử lý cho «Duyệt / từ chối điều chỉnh».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt / từ chối điều chỉnh», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt / từ chối điều chỉnh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 123. Đặc tả Use Case "Ghi nhận vi phạm đi trễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_123 |
| **Tên Use Case** | Ghi nhận vi phạm đi trễ |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Ghi nhận vi phạm đi trễ" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Violation log |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận vi phạm đi trễ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận vi phạm đi trễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận vi phạm đi trễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Ghi nhận vi phạm đi trễ» trong nhóm Điều chỉnh & khóa công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Violation log).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận vi phạm đi trễ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận vi phạm đi trễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 124. Đặc tả Use Case "Lập bảng phạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_124 |
| **Tên Use Case** | Lập bảng phạt |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Lập bảng phạt" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Penalty calculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lập bảng phạt» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lập bảng phạt» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lập bảng phạt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Lập bảng phạt» trong nhóm Điều chỉnh & khóa công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Penalty calculation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lập bảng phạt».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lập bảng phạt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 125. Đặc tả Use Case "Áp dụng phạt vào kỳ lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_125 |
| **Tên Use Case** | Áp dụng phạt vào kỳ lương |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Áp dụng phạt vào kỳ lương" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Link penalty to payroll |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Áp dụng phạt vào kỳ lương» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Áp dụng phạt vào kỳ lương» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Áp dụng phạt vào kỳ lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Áp dụng phạt vào kỳ lương» trong nhóm Điều chỉnh & khóa công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Link penalty to payroll).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Áp dụng phạt vào kỳ lương».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Áp dụng phạt vào kỳ lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 126. Đặc tả Use Case "Khóa bảng công theo kỳ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_126 |
| **Tên Use Case** | Khóa bảng công theo kỳ |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Khóa bảng công theo kỳ" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Lock timesheet period |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa bảng công theo kỳ» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa bảng công theo kỳ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa bảng công theo kỳ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer chọn kỳ/ca/đối tượng cần khóa trong «Khóa bảng công theo kỳ».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa bảng công theo kỳ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 127. Đặc tả Use Case "Mở khóa bảng công có kiểm soát"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_127 |
| **Tên Use Case** | Mở khóa bảng công có kiểm soát |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Mở khóa bảng công có kiểm soát" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Unlock with approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mở khóa bảng công có kiểm soát» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mở khóa bảng công có kiểm soát» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mở khóa bảng công có kiểm soát» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer yêu cầu mở khóa đối tượng trong «Mở khóa bảng công có kiểm soát» kèm lý do.<br>2. Hệ thống kiểm tra quyền mở khóa đặc biệt và chính sách tenant.<br>3. Xác nhận mở khóa có giới hạn thời gian/phạm vi nếu cấu hình.<br>4. Ghi Audit bắt buộc (who/when/why); thông báo người liên quan.<br>5. Cho phép chỉnh sửa có kiểm soát rồi khóa lại. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mở khóa bảng công có kiểm soát» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 128. Đặc tả Use Case "Xác nhận bảng công"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_128 |
| **Tên Use Case** | Xác nhận bảng công |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Xác nhận bảng công" thuộc nhóm Điều chỉnh & khóa công trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Timesheet acknowledgment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận bảng công» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận bảng công» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận bảng công» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Xác nhận bảng công» trong nhóm Điều chỉnh & khóa công.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Timesheet acknowledgment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận bảng công».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận bảng công» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

### 7.14. Nghỉ phép & vắng mặt (`HRM-14`)

Nhóm **Nghỉ phép & vắng mặt** gồm **10** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 7 |

**Bảng 129. Đặc tả Use Case "Danh mục loại nghỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_129 |
| **Tên Use Case** | Danh mục loại nghỉ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Danh mục loại nghỉ" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave types |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục loại nghỉ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục loại nghỉ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục loại nghỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Danh mục loại nghỉ» trong nhóm Nghỉ phép & vắng mặt.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Leave types).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục loại nghỉ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục loại nghỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 130. Đặc tả Use Case "Cấu hình quỹ phép theo loại NS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_130 |
| **Tên Use Case** | Cấu hình quỹ phép theo loại NS |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Cấu hình quỹ phép theo loại NS" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave balance rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình quỹ phép theo loại NS» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-03`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình quỹ phép theo loại NS» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình quỹ phép theo loại NS» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee mở màn hình cấu hình «Cấu hình quỹ phép theo loại NS» trong Nghỉ phép & vắng mặt.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Leave balance rules) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình quỹ phép theo loại NS» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 131. Đặc tả Use Case "Cấp phát / điều chỉnh quỹ phép"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_131 |
| **Tên Use Case** | Cấp phát / điều chỉnh quỹ phép |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Cấp phát / điều chỉnh quỹ phép" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Grant leave days |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấp phát / điều chỉnh quỹ phép» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-03`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấp phát / điều chỉnh quỹ phép» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấp phát / điều chỉnh quỹ phép» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee tìm và mở bản ghi liên quan tới «Cấp phát / điều chỉnh quỹ phép» trong phạm vi được phép.<br>2. Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).<br>3. Người dùng cập nhật thông tin theo yêu cầu «Cấp phát / điều chỉnh quỹ phép» (Grant leave days).<br>4. Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.<br>5. Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.<br>6. Làm mới UI và thông báo kết quả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấp phát / điều chỉnh quỹ phép» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 132. Đặc tả Use Case "Tạo đơn xin nghỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_132 |
| **Tên Use Case** | Tạo đơn xin nghỉ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Tạo đơn xin nghỉ" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave request |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đơn xin nghỉ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đơn xin nghỉ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đơn xin nghỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee mở chức năng «Tạo đơn xin nghỉ» trong nhóm Nghỉ phép & vắng mặt.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đơn xin nghỉ» (Leave request).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đơn xin nghỉ» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đơn xin nghỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 133. Đặc tả Use Case "Duyệt đơn nghỉ đa cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_133 |
| **Tên Use Case** | Duyệt đơn nghỉ đa cấp |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Duyệt đơn nghỉ đa cấp" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave approval workflow |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt đơn nghỉ đa cấp» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt đơn nghỉ đa cấp» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt đơn nghỉ đa cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee mở hộp chờ / chứng từ cần xử lý cho «Duyệt đơn nghỉ đa cấp».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt đơn nghỉ đa cấp», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt đơn nghỉ đa cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 134. Đặc tả Use Case "Hủy đơn nghỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_134 |
| **Tên Use Case** | Hủy đơn nghỉ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Hủy đơn nghỉ" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Cancel leave |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hủy đơn nghỉ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-CAN-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hủy đơn nghỉ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hủy đơn nghỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Employee chọn đối tượng cần hủy/ngưng trong «Hủy đơn nghỉ».<br>2. Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.<br>3. Yêu cầu lý do; xác nhận cảnh báo tác động.<br>4. Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.<br>5. Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hủy đơn nghỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 135. Đặc tả Use Case "Xem quỹ phép còn lại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_135 |
| **Tên Use Case** | Xem quỹ phép còn lại |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Xem quỹ phép còn lại" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave balance view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem quỹ phép còn lại» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-03`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem quỹ phép còn lại» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem quỹ phép còn lại» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee mở «Xem quỹ phép còn lại» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Leave balance view).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem quỹ phép còn lại» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 136. Đặc tả Use Case "Lịch nghỉ theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_136 |
| **Tên Use Case** | Lịch nghỉ theo đơn vị |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Lịch nghỉ theo đơn vị" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Team leave calendar |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Lịch nghỉ theo đơn vị» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Lịch nghỉ theo đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Lịch nghỉ theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Employee khởi tạo thao tác «Lịch nghỉ theo đơn vị» trong nhóm Nghỉ phép & vắng mặt.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Team leave calendar).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Lịch nghỉ theo đơn vị».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Lịch nghỉ theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 137. Đặc tả Use Case "Import nghỉ lễ / ngày nghỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_137 |
| **Tên Use Case** | Import nghỉ lễ / ngày nghỉ |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Import nghỉ lễ / ngày nghỉ" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Holiday calendar |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Import nghỉ lễ / ngày nghỉ» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-IMP-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Import nghỉ lễ / ngày nghỉ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Import nghỉ lễ / ngày nghỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Employee tải file mẫu (nếu có) và chọn file import cho «Import nghỉ lễ / ngày nghỉ».<br>2. Hệ thống parse file, map cột, validate từng dòng.<br>3. Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.<br>4. Xác nhận import; ghi nhận transaction + Audit; tạo job log.<br>5. Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Import nghỉ lễ / ngày nghỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn. |

**Bảng 138. Đặc tả Use Case "Báo cáo nghỉ / quỹ phép"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_138 |
| **Tên Use Case** | Báo cáo nghỉ / quỹ phép |
| **Tác nhân** | Employee |
| **Mô tả chức năng** | Cho phép Employee thực hiện chức năng "Báo cáo nghỉ / quỹ phép" thuộc nhóm Nghỉ phép & vắng mặt trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Employee] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo nghỉ / quỹ phép» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-03`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo nghỉ / quỹ phép» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo nghỉ / quỹ phép» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Employee mở «Báo cáo nghỉ / quỹ phép» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Leave report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo nghỉ / quỹ phép» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

### 7.15. Kỷ luật & khen thưởng (`HRM-15`)

Nhóm **Kỷ luật & khen thưởng** gồm **5** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 0 |

**Bảng 139. Đặc tả Use Case "Ghi nhận quyết định khen thưởng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_139 |
| **Tên Use Case** | Ghi nhận quyết định khen thưởng |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Ghi nhận quyết định khen thưởng" thuộc nhóm Kỷ luật & khen thưởng trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Reward record |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận quyết định khen thưởng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận quyết định khen thưởng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận quyết định khen thưởng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Ghi nhận quyết định khen thưởng» trong nhóm Kỷ luật & khen thưởng.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Reward record).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận quyết định khen thưởng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận quyết định khen thưởng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 140. Đặc tả Use Case "Ghi nhận quyết định kỷ luật"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_140 |
| **Tên Use Case** | Ghi nhận quyết định kỷ luật |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Ghi nhận quyết định kỷ luật" thuộc nhóm Kỷ luật & khen thưởng trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Discipline record |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận quyết định kỷ luật» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận quyết định kỷ luật» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận quyết định kỷ luật» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Ghi nhận quyết định kỷ luật» trong nhóm Kỷ luật & khen thưởng.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Discipline record).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận quyết định kỷ luật».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận quyết định kỷ luật» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 141. Đặc tả Use Case "Đính kèm quyết định"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_141 |
| **Tên Use Case** | Đính kèm quyết định |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Đính kèm quyết định" thuộc nhóm Kỷ luật & khen thưởng trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Decision document |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đính kèm quyết định» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đính kèm quyết định» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đính kèm quyết định» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở bản ghi liên quan và chọn «Đính kèm quyết định».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đính kèm quyết định» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 142. Đặc tả Use Case "Ảnh hưởng lương / phụ cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_142 |
| **Tên Use Case** | Ảnh hưởng lương / phụ cấp |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Ảnh hưởng lương / phụ cấp" thuộc nhóm Kỷ luật & khen thưởng trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Link to payroll |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ảnh hưởng lương / phụ cấp» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ảnh hưởng lương / phụ cấp» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ảnh hưởng lương / phụ cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Ảnh hưởng lương / phụ cấp» trong nhóm Kỷ luật & khen thưởng.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Link to payroll).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ảnh hưởng lương / phụ cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ảnh hưởng lương / phụ cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 143. Đặc tả Use Case "Báo cáo khen thưởng – kỷ luật"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_143 |
| **Tên Use Case** | Báo cáo khen thưởng – kỷ luật |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Báo cáo khen thưởng – kỷ luật" thuộc nhóm Kỷ luật & khen thưởng trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Reward/discipline report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo khen thưởng – kỷ luật» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo khen thưởng – kỷ luật» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo khen thưởng – kỷ luật» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở «Báo cáo khen thưởng – kỷ luật» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Reward/discipline report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo khen thưởng – kỷ luật» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.16. Offboarding / nghỉ việc (`HRM-16`)

Nhóm **Offboarding / nghỉ việc** gồm **8** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 6 |

**Bảng 144. Đặc tả Use Case "Tạo đơn nghỉ việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_144 |
| **Tên Use Case** | Tạo đơn nghỉ việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Tạo đơn nghỉ việc" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Resignation form |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đơn nghỉ việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đơn nghỉ việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đơn nghỉ việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở chức năng «Tạo đơn nghỉ việc» trong nhóm Offboarding / nghỉ việc.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đơn nghỉ việc» (Resignation form).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đơn nghỉ việc» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đơn nghỉ việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 145. Đặc tả Use Case "Cấu hình / kiểm tra báo trước"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_145 |
| **Tên Use Case** | Cấu hình / kiểm tra báo trước |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Cấu hình / kiểm tra báo trước" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Notice period |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình / kiểm tra báo trước» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình / kiểm tra báo trước» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình / kiểm tra báo trước» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở màn hình cấu hình «Cấu hình / kiểm tra báo trước» trong Offboarding / nghỉ việc.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Notice period) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình / kiểm tra báo trước» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 146. Đặc tả Use Case "Duyệt đơn nghỉ việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_146 |
| **Tên Use Case** | Duyệt đơn nghỉ việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Duyệt đơn nghỉ việc" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Exit approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Duyệt đơn nghỉ việc» đã được cấu hình trong phạm vi data scope.<br>• Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-APPR-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Duyệt đơn nghỉ việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Duyệt đơn nghỉ việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer mở hộp chờ / chứng từ cần xử lý cho «Duyệt đơn nghỉ việc».<br>2. Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.<br>3. Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.<br>4. Chọn [Duyệt] cho «Duyệt đơn nghỉ việc», nhập ghi chú nếu bắt buộc.<br>5. Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.<br>6. Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Duyệt đơn nghỉ việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 147. Đặc tả Use Case "Checklist bàn giao"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_147 |
| **Tên Use Case** | Checklist bàn giao |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Checklist bàn giao" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Handover checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist bàn giao» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist bàn giao» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist bàn giao» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Checklist bàn giao» trong nhóm Offboarding / nghỉ việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Handover checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist bàn giao».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist bàn giao» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 148. Đặc tả Use Case "Thu hồi quyền hệ thống"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_148 |
| **Tên Use Case** | Thu hồi quyền hệ thống |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Thu hồi quyền hệ thống" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Disable system access |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thu hồi quyền hệ thống» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thu hồi quyền hệ thống» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thu hồi quyền hệ thống» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Thu hồi quyền hệ thống» trong nhóm Offboarding / nghỉ việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Disable system access).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thu hồi quyền hệ thống».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thu hồi quyền hệ thống» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 149. Đặc tả Use Case "Quyết toán phép / lương nghỉ việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_149 |
| **Tên Use Case** | Quyết toán phép / lương nghỉ việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Quyết toán phép / lương nghỉ việc" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Final pay calculation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quyết toán phép / lương nghỉ việc» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`, `BR-HRM-03`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quyết toán phép / lương nghỉ việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quyết toán phép / lương nghỉ việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Quyết toán phép / lương nghỉ việc» trong nhóm Offboarding / nghỉ việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Final pay calculation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Quyết toán phép / lương nghỉ việc».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quyết toán phép / lương nghỉ việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 150. Đặc tả Use Case "Phỏng vấn nghỉ việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_150 |
| **Tên Use Case** | Phỏng vấn nghỉ việc |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Phỏng vấn nghỉ việc" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Exit interview |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phỏng vấn nghỉ việc» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phỏng vấn nghỉ việc» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phỏng vấn nghỉ việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer khởi tạo thao tác «Phỏng vấn nghỉ việc» trong nhóm Offboarding / nghỉ việc.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Exit interview).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phỏng vấn nghỉ việc».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phỏng vấn nghỉ việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 151. Đặc tả Use Case "Báo cáo nghỉ việc / lý do"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_151 |
| **Tên Use Case** | Báo cáo nghỉ việc / lý do |
| **Tác nhân** | HR Officer |
| **Mô tả chức năng** | Cho phép HR Officer thực hiện chức năng "Báo cáo nghỉ việc / lý do" thuộc nhóm Offboarding / nghỉ việc trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Turnover analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Officer] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo nghỉ việc / lý do» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo nghỉ việc / lý do» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo nghỉ việc / lý do» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Officer mở «Báo cáo nghỉ việc / lý do» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Turnover analysis); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo nghỉ việc / lý do» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.17. Cấu hình lương & phụ cấp (`HRM-17`)

Nhóm **Cấu hình lương & phụ cấp** gồm **11** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 11 |
| Must | 10 |

**Bảng 152. Đặc tả Use Case "Tạo thang bậc lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_152 |
| **Tên Use Case** | Tạo thang bậc lương |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Tạo thang bậc lương" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Salary grade structure |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo thang bậc lương» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo thang bậc lương» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo thang bậc lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở chức năng «Tạo thang bậc lương» trong nhóm Cấu hình lương & phụ cấp.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo thang bậc lương» (Salary grade structure).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo thang bậc lương» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo thang bậc lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 153. Đặc tả Use Case "Gán bậc lương theo nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_153 |
| **Tên Use Case** | Gán bậc lương theo nhân sự |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Gán bậc lương theo nhân sự" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Salary assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán bậc lương theo nhân sự» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán bậc lương theo nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán bậc lương theo nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin chọn đối tượng nguồn trong «Gán bậc lương theo nhân sự».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán bậc lương theo nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 154. Đặc tả Use Case "Gán bậc theo trạng thái"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_154 |
| **Tên Use Case** | Gán bậc theo trạng thái |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Gán bậc theo trạng thái" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Salary by status |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán bậc theo trạng thái» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán bậc theo trạng thái» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán bậc theo trạng thái» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin chọn đối tượng nguồn trong «Gán bậc theo trạng thái».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán bậc theo trạng thái» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 155. Đặc tả Use Case "Đơn giá giờ / ngày nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_155 |
| **Tên Use Case** | Đơn giá giờ / ngày nhân viên |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Đơn giá giờ / ngày nhân viên" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Hourly/daily rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đơn giá giờ / ngày nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đơn giá giờ / ngày nhân viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đơn giá giờ / ngày nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin khởi tạo thao tác «Đơn giá giờ / ngày nhân viên» trong nhóm Cấu hình lương & phụ cấp.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Hourly/daily rate).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đơn giá giờ / ngày nhân viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đơn giá giờ / ngày nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 156. Đặc tả Use Case "Quản lý lương thực tế chi trả"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_156 |
| **Tên Use Case** | Quản lý lương thực tế chi trả |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Quản lý lương thực tế chi trả" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Actual pay rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý lương thực tế chi trả» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý lương thực tế chi trả» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý lương thực tế chi trả» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở danh mục quản lý «Quản lý lương thực tế chi trả» (nhân sự / hồ sơ / công – phép – lương; nhóm «Cấu hình lương & phụ cấp»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý lương thực tế chi trả» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 157. Đặc tả Use Case "Danh mục phụ cấp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_157 |
| **Tên Use Case** | Danh mục phụ cấp |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Danh mục phụ cấp" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Allowance master |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục phụ cấp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục phụ cấp» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục phụ cấp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin khởi tạo thao tác «Danh mục phụ cấp» trong nhóm Cấu hình lương & phụ cấp.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Allowance master).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục phụ cấp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục phụ cấp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 158. Đặc tả Use Case "Rule phụ cấp theo ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_158 |
| **Tên Use Case** | Rule phụ cấp theo ca |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Rule phụ cấp theo ca" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Shift allowance rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Rule phụ cấp theo ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Rule phụ cấp theo ca» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Rule phụ cấp theo ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin khởi tạo thao tác «Rule phụ cấp theo ca» trong nhóm Cấu hình lương & phụ cấp.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Shift allowance rules).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Rule phụ cấp theo ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Rule phụ cấp theo ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 159. Đặc tả Use Case "Rule phụ cấp đặc thù"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_159 |
| **Tên Use Case** | Rule phụ cấp đặc thù |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Rule phụ cấp đặc thù" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Special allowance rules |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Rule phụ cấp đặc thù» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Rule phụ cấp đặc thù» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Rule phụ cấp đặc thù» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin khởi tạo thao tác «Rule phụ cấp đặc thù» trong nhóm Cấu hình lương & phụ cấp.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Special allowance rules).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Rule phụ cấp đặc thù».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Rule phụ cấp đặc thù» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 160. Đặc tả Use Case "Cấu hình bảo hiểm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_160 |
| **Tên Use Case** | Cấu hình bảo hiểm |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình bảo hiểm" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Insurance rates |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình bảo hiểm» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình bảo hiểm» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình bảo hiểm» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình bảo hiểm» trong Cấu hình lương & phụ cấp.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Insurance rates) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình bảo hiểm» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 161. Đặc tả Use Case "Cấu hình thuế TNCN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_161 |
| **Tên Use Case** | Cấu hình thuế TNCN |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình thuế TNCN" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Personal income tax |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình thuế TNCN» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình thuế TNCN» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình thuế TNCN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình thuế TNCN» trong Cấu hình lương & phụ cấp.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Personal income tax) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình thuế TNCN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 162. Đặc tả Use Case "Cấu hình tạm ứng / khấu trừ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_162 |
| **Tên Use Case** | Cấu hình tạm ứng / khấu trừ |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Cấu hình tạm ứng / khấu trừ" thuộc nhóm Cấu hình lương & phụ cấp trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Deduction types |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình tạm ứng / khấu trừ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình tạm ứng / khấu trừ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình tạm ứng / khấu trừ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở màn hình cấu hình «Cấu hình tạm ứng / khấu trừ» trong Cấu hình lương & phụ cấp.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Deduction types) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình tạm ứng / khấu trừ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

### 7.18. Tính lương & chi trả (`HRM-18`)

Nhóm **Tính lương & chi trả** gồm **14** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 14 |
| Must | 12 |

**Bảng 163. Đặc tả Use Case "Tạo kỳ lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_163 |
| **Tên Use Case** | Tạo kỳ lương |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Tạo kỳ lương" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Payroll period |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo kỳ lương» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo kỳ lương» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo kỳ lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant mở chức năng «Tạo kỳ lương» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo kỳ lương» (Payroll period).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo kỳ lương» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo kỳ lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 164. Đặc tả Use Case "Tổng hợp công vào kỳ lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_164 |
| **Tên Use Case** | Tổng hợp công vào kỳ lương |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Tổng hợp công vào kỳ lương" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Import attendance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tổng hợp công vào kỳ lương» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tổng hợp công vào kỳ lương» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tổng hợp công vào kỳ lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «Tổng hợp công vào kỳ lương» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Import attendance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tổng hợp công vào kỳ lương».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tổng hợp công vào kỳ lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 165. Đặc tả Use Case "Tính lương tự động theo rule"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_165 |
| **Tên Use Case** | Tính lương tự động theo rule |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Tính lương tự động theo rule" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Payroll calculation engine |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tính lương tự động theo rule» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu nguồn (công, tồn, tỷ giá…) đã sẵn sàng và đạt điều kiện chốt.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-CALC-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tính lương tự động theo rule» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tính lương tự động theo rule» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Kết quả tính toán tái lập được với cùng input/rule (deterministic trong cùng phiên bản rule).<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant chọn phạm vi tính toán cho «Tính lương tự động theo rule» (kỳ, đơn vị, bộ lọc).<br>2. Hệ thống nạp dữ liệu nguồn liên quan (Payroll calculation engine).<br>3. Chạy engine tính theo rule cấu hình; log chi tiết từng bước lỗi nếu có.<br>4. Hiển thị kết quả nháp để rà soát; cho phép điều chỉnh có audit trước khi chốt.<br>5. Xác nhận ghi nhận kết quả chính thức; phát sự kiện cho FIN/module liên quan nếu cần.<br>6. Thông báo hoàn tất và cập nhật trạng thái kỳ/tính toán. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tính lương tự động theo rule» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thiếu dữ liệu nguồn hoặc rule cấu hình không đầy đủ → dừng job, liệt kê lỗi chi tiết để sửa.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 166. Đặc tả Use Case "Nhập thưởng / phụ cấp phát sinh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_166 |
| **Tên Use Case** | Nhập thưởng / phụ cấp phát sinh |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Nhập thưởng / phụ cấp phát sinh" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Manual additions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập thưởng / phụ cấp phát sinh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập thưởng / phụ cấp phát sinh» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập thưởng / phụ cấp phát sinh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «Nhập thưởng / phụ cấp phát sinh» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Manual additions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập thưởng / phụ cấp phát sinh».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập thưởng / phụ cấp phát sinh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 167. Đặc tả Use Case "Nhập khấu trừ / tạm ứng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_167 |
| **Tên Use Case** | Nhập khấu trừ / tạm ứng |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Nhập khấu trừ / tạm ứng" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Manual deductions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhập khấu trừ / tạm ứng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhập khấu trừ / tạm ứng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhập khấu trừ / tạm ứng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «Nhập khấu trừ / tạm ứng» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Manual deductions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhập khấu trừ / tạm ứng».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhập khấu trừ / tạm ứng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 168. Đặc tả Use Case "Xem / chỉnh bảng lương chi tiết"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_168 |
| **Tên Use Case** | Xem / chỉnh bảng lương chi tiết |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Xem / chỉnh bảng lương chi tiết" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Payslip review |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem / chỉnh bảng lương chi tiết» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem / chỉnh bảng lương chi tiết» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem / chỉnh bảng lương chi tiết» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant mở «Xem / chỉnh bảng lương chi tiết» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Payslip review).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem / chỉnh bảng lương chi tiết» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 169. Đặc tả Use Case "Xác nhận bảng lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_169 |
| **Tên Use Case** | Xác nhận bảng lương |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Xác nhận bảng lương" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Payroll approval |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận bảng lương» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận bảng lương» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận bảng lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «Xác nhận bảng lương» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Payroll approval).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận bảng lương».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận bảng lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 170. Đặc tả Use Case "Khóa kỳ lương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_170 |
| **Tên Use Case** | Khóa kỳ lương |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Khóa kỳ lương" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Lock payroll period |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khóa kỳ lương» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-LOCK-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khóa kỳ lương» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khóa kỳ lương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant chọn kỳ/ca/đối tượng cần khóa trong «Khóa kỳ lương».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khóa kỳ lương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.<br>8.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 171. Đặc tả Use Case "Phiếu lương cá nhân (APP)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_171 |
| **Tên Use Case** | Phiếu lương cá nhân (APP) |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Phiếu lương cá nhân (APP)" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Employee payslip view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiếu lương cá nhân (APP)» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiếu lương cá nhân (APP)» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiếu lương cá nhân (APP)» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «Phiếu lương cá nhân (APP)» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Employee payslip view).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phiếu lương cá nhân (APP)».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiếu lương cá nhân (APP)» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 172. Đặc tả Use Case "Xuất bảng lương tổng hợp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_172 |
| **Tên Use Case** | Xuất bảng lương tổng hợp |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Xuất bảng lương tổng hợp" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Payroll summary export |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất bảng lương tổng hợp» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất bảng lương tổng hợp» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất bảng lương tổng hợp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant mở «Xuất bảng lương tổng hợp», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất bảng lương tổng hợp» (Payroll summary export).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất bảng lương tổng hợp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 173. Đặc tả Use Case "Xuất file chi lương ngân hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_173 |
| **Tên Use Case** | Xuất file chi lương ngân hàng |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Xuất file chi lương ngân hàng" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Bank transfer file |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất file chi lương ngân hàng» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất file chi lương ngân hàng» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất file chi lương ngân hàng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant mở «Xuất file chi lương ngân hàng», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất file chi lương ngân hàng» (Bank transfer file).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất file chi lương ngân hàng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 174. Đặc tả Use Case "Đồng bộ bút toán lương sang FIN"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_174 |
| **Tên Use Case** | Đồng bộ bút toán lương sang FIN |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Đồng bộ bút toán lương sang FIN" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: GL posting |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ bút toán lương sang FIN» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ bút toán lương sang FIN» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ bút toán lương sang FIN» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «Đồng bộ bút toán lương sang FIN» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (GL posting).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ bút toán lương sang FIN».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ bút toán lương sang FIN» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 175. Đặc tả Use Case "Báo cáo chi phí lương theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_175 |
| **Tên Use Case** | Báo cáo chi phí lương theo đơn vị |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "Báo cáo chi phí lương theo đơn vị" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Labor cost by unit |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo chi phí lương theo đơn vị» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo chi phí lương theo đơn vị» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo chi phí lương theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Payroll Accountant mở «Báo cáo chi phí lương theo đơn vị» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Labor cost by unit); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo chi phí lương theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 176. Đặc tả Use Case "So sánh lương kỳ này / kỳ trước"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_176 |
| **Tên Use Case** | So sánh lương kỳ này / kỳ trước |
| **Tác nhân** | Payroll Accountant |
| **Mô tả chức năng** | Cho phép Payroll Accountant thực hiện chức năng "So sánh lương kỳ này / kỳ trước" thuộc nhóm Tính lương & chi trả trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Period variance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Payroll Accountant] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «So sánh lương kỳ này / kỳ trước» đã được cấu hình trong phạm vi data scope.<br>• Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-02`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «So sánh lương kỳ này / kỳ trước» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «So sánh lương kỳ này / kỳ trước» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Payroll Accountant khởi tạo thao tác «So sánh lương kỳ này / kỳ trước» trong nhóm Tính lương & chi trả.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Period variance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «So sánh lương kỳ này / kỳ trước».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «So sánh lương kỳ này / kỳ trước» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

### 7.19. Đánh giá hiệu suất (`HRM-19`)

Nhóm **Đánh giá hiệu suất** gồm **5** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 0 |

**Bảng 177. Đặc tả Use Case "Mẫu đánh giá KPI / năng lực"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_177 |
| **Tên Use Case** | Mẫu đánh giá KPI / năng lực |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Mẫu đánh giá KPI / năng lực" thuộc nhóm Đánh giá hiệu suất trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Performance review template |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mẫu đánh giá KPI / năng lực» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mẫu đánh giá KPI / năng lực» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mẫu đánh giá KPI / năng lực» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Mẫu đánh giá KPI / năng lực» trong nhóm Đánh giá hiệu suất.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Performance review template).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mẫu đánh giá KPI / năng lực».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mẫu đánh giá KPI / năng lực» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 178. Đặc tả Use Case "Tạo kỳ đánh giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_178 |
| **Tên Use Case** | Tạo kỳ đánh giá |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Tạo kỳ đánh giá" thuộc nhóm Đánh giá hiệu suất trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Review cycle |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo kỳ đánh giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo kỳ đánh giá» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo kỳ đánh giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager mở chức năng «Tạo kỳ đánh giá» trong nhóm Đánh giá hiệu suất.<br>2. Hệ thống kiểm tra license `HRM`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo kỳ đánh giá» (Review cycle).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo kỳ đánh giá» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo kỳ đánh giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 179. Đặc tả Use Case "Quản lý đánh giá nhân viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_179 |
| **Tên Use Case** | Quản lý đánh giá nhân viên |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Quản lý đánh giá nhân viên" thuộc nhóm Đánh giá hiệu suất trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Manager review |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý đánh giá nhân viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý đánh giá nhân viên» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý đánh giá nhân viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager mở danh mục quản lý «Quản lý đánh giá nhân viên» (nhân sự / hồ sơ / công – phép – lương; nhóm «Đánh giá hiệu suất»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý đánh giá nhân viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 180. Đặc tả Use Case "Nhân viên tự đánh giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_180 |
| **Tên Use Case** | Nhân viên tự đánh giá |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Nhân viên tự đánh giá" thuộc nhóm Đánh giá hiệu suất trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Self assessment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhân viên tự đánh giá» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhân viên tự đánh giá» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhân viên tự đánh giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Nhân viên tự đánh giá» trong nhóm Đánh giá hiệu suất.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Self assessment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhân viên tự đánh giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhân viên tự đánh giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 181. Đặc tả Use Case "Tổng hợp kết quả đánh giá"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_181 |
| **Tên Use Case** | Tổng hợp kết quả đánh giá |
| **Tác nhân** | Line Manager |
| **Mô tả chức năng** | Cho phép Line Manager thực hiện chức năng "Tổng hợp kết quả đánh giá" thuộc nhóm Đánh giá hiệu suất trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Review results |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Line Manager] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tổng hợp kết quả đánh giá» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tổng hợp kết quả đánh giá» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tổng hợp kết quả đánh giá» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Line Manager khởi tạo thao tác «Tổng hợp kết quả đánh giá» trong nhóm Đánh giá hiệu suất.<br>2. Hệ thống kiểm tra license `HRM`, quyền RBAC và tiền điều kiện nghiệp vụ (Review results).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tổng hợp kết quả đánh giá».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tổng hợp kết quả đánh giá» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.20. Báo cáo & dashboard HRM (`HRM-20`)

Nhóm **Báo cáo & dashboard HRM** gồm **6** use case của module `HRM`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 182. Đặc tả Use Case "Dashboard headcount & biến động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_182 |
| **Tên Use Case** | Dashboard headcount & biến động |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Dashboard headcount & biến động" thuộc nhóm Báo cáo & dashboard HRM trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: HR overview dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard headcount & biến động» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard headcount & biến động» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard headcount & biến động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở «Dashboard headcount & biến động» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (HR overview dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard headcount & biến động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 183. Đặc tả Use Case "Báo cáo công / OT / đi trễ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_183 |
| **Tên Use Case** | Báo cáo công / OT / đi trễ |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Báo cáo công / OT / đi trễ" thuộc nhóm Báo cáo & dashboard HRM trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Attendance KPIs |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo công / OT / đi trễ» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo công / OT / đi trễ» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo công / OT / đi trễ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở «Báo cáo công / OT / đi trễ» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Attendance KPIs); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo công / OT / đi trễ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 184. Đặc tả Use Case "Báo cáo tuyển dụng funnel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_184 |
| **Tên Use Case** | Báo cáo tuyển dụng funnel |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Báo cáo tuyển dụng funnel" thuộc nhóm Báo cáo & dashboard HRM trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Recruitment funnel |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo tuyển dụng funnel» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo tuyển dụng funnel» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo tuyển dụng funnel» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở «Báo cáo tuyển dụng funnel» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Recruitment funnel); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo tuyển dụng funnel» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 185. Đặc tả Use Case "Báo cáo quỹ phép"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_185 |
| **Tên Use Case** | Báo cáo quỹ phép |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Báo cáo quỹ phép" thuộc nhóm Báo cáo & dashboard HRM trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Leave balance report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo quỹ phép» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`, `BR-HRM-03`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo quỹ phép» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo quỹ phép» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở «Báo cáo quỹ phép» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Leave balance report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo quỹ phép» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do. |

**Bảng 186. Đặc tả Use Case "Báo cáo chi phí nhân sự"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_186 |
| **Tên Use Case** | Báo cáo chi phí nhân sự |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Báo cáo chi phí nhân sự" thuộc nhóm Báo cáo & dashboard HRM trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: HR cost dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo chi phí nhân sự» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo chi phí nhân sự» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo chi phí nhân sự» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Admin mở «Báo cáo chi phí nhân sự» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (HR cost dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo chi phí nhân sự» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 187. Đặc tả Use Case "Báo cáo định biên vs thực tế"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_HRM_187 |
| **Tên Use Case** | Báo cáo định biên vs thực tế |
| **Tác nhân** | HR Admin |
| **Mô tả chức năng** | Cho phép HR Admin thực hiện chức năng "Báo cáo định biên vs thực tế" thuộc nhóm Báo cáo & dashboard HRM trong module HRM — Quản trị nhân sự (Human Resource Management). Mô tả chi tiết: Headcount gap report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Admin] và được cấp quyền RBAC tương ứng.<br>• License module `HRM` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo định biên vs thực tế» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-HRM-SCOPE-01`, `BR-HRM-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo định biên vs thực tế» được lưu nhất quán trong module `HRM`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo định biên vs thực tế» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Admin mở «Báo cáo định biên vs thực tế» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Headcount gap report); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo định biên vs thực tế» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-HRM-01 — Tuyển dụng → Onboarding → Chính thức

**Mục tiêu:** Từ nhu cầu tuyển đến nhân sự chính thức có hồ sơ & hợp đồng

| Bước | Mô tả |
|---:|---|
| 1 | Tạo và duyệt nhu cầu tuyển (PR tuyển) |
| 2 | Đăng tin / tiếp nhận ứng viên / sơ loại |
| 3 | Đánh giá và quyết định nhận việc |
| 4 | Tạo hồ sơ nhân sự + checklist onboarding |
| 5 | Ký hợp đồng / phụ lục; gán đơn vị – chức danh |
| 6 | Theo dõi thử việc; chuyển chính thức hoặc chấm dứt |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-HRM-02 — Xếp ca → Chấm công → Khóa công

**Mục tiêu:** Có bảng công kỳ sạch để tính lương

| Bước | Mô tả |
|---:|---|
| 1 | Khai báo mẫu ca và định biên (nếu dùng) |
| 2 | Xếp lịch ca; nhân viên xem trên app/web |
| 3 | Chấm công theo phương thức cấu hình |
| 4 | Xin–duyệt điều chỉnh công khi sai/thiếu |
| 5 | Khóa bảng công kỳ; mở khóa có kiểm soát nếu cần |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-HRM-03 — Tính lương theo kỳ

**Mục tiêu:** Sinh bảng lương và chi trả

| Bước | Mô tả |
|---:|---|
| 1 | Tạo kỳ lương; nạp công đã khóa + phụ cấp/khấu trừ |
| 2 | Chạy payroll engine theo rule |
| 3 | HR/Kế toán rà soát, điều chỉnh có audit |
| 4 | Duyệt và khóa kỳ lương |
| 5 | Xuất phiếu lương NV + file ngân hàng |
| 6 | Đẩy bút toán/chi phí sang FIN (nếu có) |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-HRM-04 — Nghỉ việc / Offboarding

**Mục tiêu:** Chấm dứt quan hệ LĐ có bàn giao và quyết toán

| Bước | Mô tả |
|---:|---|
| 1 | Nhân viên/HR tạo đơn nghỉ việc |
| 2 | Duyệt theo workflow; kiểm tra báo trước |
| 3 | Checklist bàn giao quyền/hệ thống/tài sản |
| 4 | Quyết toán phép – lương cuối |
| 5 | Khóa hồ sơ; thu hồi truy cập SYS |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `OrgUnit / JobTitle / Position` | Tổ chức & vị trí |
| `Employee` | Hồ sơ nhân sự |
| `EmploymentStatusHistory` | Lịch sử trạng thái |
| `Contract / ContractAppendix` | Hợp đồng |
| `RecruitmentRequest / Candidate / JobPosting` | Tuyển dụng |
| `OnboardingChecklist` | Checklist nhận việc |
| `ShiftTemplate / ShiftAssignment` | Ca làm việc |
| `AttendancePunch / Timesheet` | Chấm công |
| `LeaveType / LeaveBalance / LeaveRequest` | Nghỉ phép |
| `PayrollPeriod / Payslip / SalaryRule` | Lương |
| `TransferOrder` | Điều động |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `HRM`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-HRM-01: Mã nhân sự duy nhất trong tenant, không tái sử dụng.
- BR-HRM-02: Chỉ tính lương từ bảng công đã khóa (trừ điều chỉnh được duyệt).
- BR-HRM-03: Quỹ phép không âm trừ khi policy cho phép ứng phép.
- BR-HRM-04: Nhân sự nghỉ việc phải hoàn tất checklist offboarding trước khi khóa hồ sơ.
- BR-HRM-05: Dữ liệu lương là dữ liệu nhạy cảm — mask theo permission field-level.
- BR-HRM-06: Mọi điều chỉnh công/lương sau khóa kỳ phải có lý do + người duyệt.
- BR-HRM-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-HRM-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-HRM-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Bảo mật | Mã hóa dữ liệu nhạy cảm; phân quyền field-level cho lương/CCCD |
| Hiệu năng | Bảng công 5.000 NV khóa kỳ < 5 phút (batch) |
| Mobile | Employee self-service dùng được trên mobile web/app |
| Tuân thủ | Lưu hồ sơ/HĐ theo thời hạn pháp lý cấu hình được |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `HRM` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `hrm.employee.manage` | Quyền chức năng module |
| `hrm.employee.view` | Quyền chức năng module |
| `hrm.contract.manage` | Quyền chức năng module |
| `hrm.recruitment.manage` | Quyền chức năng module |
| `hrm.attendance.manage` | Quyền chức năng module |
| `hrm.attendance.approve` | Quyền chức năng module |
| `hrm.leave.approve` | Quyền chức năng module |
| `hrm.payroll.run` | Quyền chức năng module |
| `hrm.payroll.view_all` | Quyền chức năng module |
| `hrm.payslip.view_self` | Quyền chức năng module |
| `hrm.report.view` | Quyền chức năng module |
| `hrm.*.view` | Xem trong data scope |
| `hrm.*.manage` | Tạo/sửa trong data scope |
| `hrm.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Headcount theo đơn vị/trạng thái | Theo dõi vận hành module |
| Tỷ lệ đi trễ / quên chấm | Theo dõi vận hành module |
| Thời gian fill vị trí tuyển dụng | Theo dõi vận hành module |
| Chi phí lương / doanh thu (khi có FIN) | Theo dõi vận hành module |
| Tỷ lệ nghỉ việc (turnover) | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Chính sách lương/phụ cấp cấu hình được theo tenant, không hard-code ngành.
- Có thể dùng HRM độc lập với LMS; chứng chỉ thủ công nếu chưa mua LMS.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Phase 1 có gồm đánh giá hiệu suất đầy đủ hay chỉ checklist cơ bản?
2. Phạm vi tích hợp máy chấm công cụ thể theo hãng nào ở triển khai?

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
| Bản SRS này | `SRS_HRM_v1.1.md` / `.docx` |
| UC IDs | `UC_HRM_001` … |

---

*Hết tài liệu SRS-HRM-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
