# SRS-HRM-v1.0 — Quản trị nhân sự (Human Resource Management)

> Tài liệu đặc tả yêu cầu phần mềm (Software Requirements Specification) cho module ERP bán độc lập.
> Trạng thái: **Đề xuất / chờ duyệt nghiệp vụ**. Không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu & lịch sử thay đổi

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-HRM-v1.0` |
| Module | `HRM` — Quản trị nhân sự (Human Resource Management) |
| Phiên bản | 1.0 |
| Ngày lập | 03/08/2026 |
| Ngôn ngữ | Tiếng Việt |
| Phân loại | Nghiệp vụ / BA |
| Lớp sản phẩm | Nhân sự & Đào tạo |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | SYS |
| Khuyến nghị kèm | WF, FIN, LMS |
| Số nhóm chức năng | 20 |
| Số use case / chức năng | 187 |

| Phiên bản | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Solution | Sinh SRS từ danh mục chức năng generic v3 + meta nghiệp vụ | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích tài liệu
Tài liệu này mô tả đầy đủ yêu cầu nghiệp vụ và yêu cầu hệ thống của module **Quản trị nhân sự (Human Resource Management)**, làm cơ sở để thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai cấu trúc source code.

### 1.2. Tóm tắt module
Module HRM quản lý vòng đời nhân sự: tổ chức, hồ sơ, hợp đồng, tuyển dụng, onboarding, xếp ca, chấm công, nghỉ phép, điều động, lương/phụ cấp, offboarding và báo cáo nhân sự. Thiết kế generic cho doanh nghiệp đa chi nhánh.

### 1.3. Mục tiêu nghiệp vụ
1. Số hóa hồ sơ và biến động nhân sự end-to-end.
2. Chấm công – xếp ca – nghỉ phép chính xác, có kiểm soát duyệt.
3. Tính lương theo kỳ từ dữ liệu công và rule cấu hình.
4. Cung cấp số liệu headcount, chi phí nhân sự, tuân thủ hợp đồng.

### 1.4. Đối tượng đọc
- Chủ sản phẩm / Ban giám đốc dự án
- Business Analyst, Solution Architect
- Trưởng nhóm Dev/QA
- Đội triển khai & Presales (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Org HR, employee master, contract, recruitment, onboarding/probation, staffing & shift, attendance, leave, transfer/secondment, payroll, discipline/reward, offboarding, HR reports.

### 2.2. Out of Scope
- Nội dung đào tạo chi tiết (LMS).
- Hạch toán kế toán đầy đủ (FIN nhận bút toán lương).
- Quản lý tài sản chi tiết khi bàn giao (AST; HRM chỉ checklist).

### 2.3. Nguyên tắc đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`.
- **Khuyến nghị kèm** để có giá trị E2E: WF, FIN, LMS.
- Tính năng ngành (F&B, sản xuất rời rạc, phân phối…) cấu hình bằng template khi triển khai, không hard-code vào SRS gốc.

---

## 3. Tác nhân & stakeholder

| Tác nhân | Trách nhiệm chính |
|---|---|
| HR Admin | Cấu hình master HR, rule lương/phép |
| HR Officer | Vận hành hồ sơ, tuyển dụng, công, lương |
| Line Manager | Duyệt nghỉ/đổi ca, xem đội nhóm, đánh giá |
| Employee | Self-service: công, phép, phiếu lương, cập nhật thông tin hạn chế |
| Payroll Accountant | Chốt kỳ lương, xuất chi lương |
| Recruiter | Quản lý tin tuyển và ứng viên |
| Hệ thống | Nhắc hạn HĐ/thử việc, tính công/lương |

---

## 4. Thuật ngữ & viết tắt

| Thuật ngữ | Định nghĩa |
|---|---|
| Headcount | Định biên / số lượng nhân sự |
| Timesheet | Bảng công theo kỳ |
| Payroll | Kỳ và bảng tính lương |
| Probation | Thời gian thử việc |
| Secondment | Điều động hỗ trợ đơn vị khác |
| UC | Use Case / chức năng nguyên tử trong catalog |
| MoSCoW | Must / Should / Could / Won't (ưu tiên) |
| Data scope | Phạm vi dữ liệu theo tổ chức/kho/… do SYS kiểm soát |

---

## 5. Ngữ cảnh module & phụ thuộc

### 5.1. Vị trí trong kiến trúc sản phẩm
Module `HRM` thuộc lớp **Nhân sự & Đào tạo**. Mọi truy cập đi qua lớp nền `SYS` (xác thực, RBAC, license, audit, file, thông báo).

### 5.2. Phụ thuộc & tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | SYS | User lifecycle, org master, workflow duyệt, file, thông báo |
| Tích hợp | WF | Duyệt nghỉ / điều chỉnh công / đề xuất tuyển / kỳ lương |
| Tích hợp | LMS | Đồng bộ chứng chỉ đào tạo bắt buộc vào hồ sơ |
| Tích hợp | FIN | Post chi phí lương, tạm ứng, khấu trừ |
| Tích hợp | AST | Checklist thu hồi tài sản khi nghỉ việc |
| Tích hợp | Device | Máy chấm công / geo-fence app |

### 5.3. Ràng buộc license
- API/UI của `HRM` chỉ mở khi license module active.
- Dataset BI liên quan module chỉ mở khi vừa có license `BI` vừa có license module nguồn.

---

## 6. Catalog chức năng (Module → Nhóm → UC)

**Tổng hợp:** 20 nhóm | 187 chức năng/use case.

| STT | Mã nhóm | Nhóm chức năng | Số UC |
|---:|---|---|---:|
| 1 | `HRM-01` | Cơ cấu tổ chức nhân sự | 11 |
| 2 | `HRM-02` | Hồ sơ nhân sự | 17 |
| 3 | `HRM-03` | Trạng thái & biến động nhân sự | 9 |
| 4 | `HRM-04` | Hợp đồng lao động | 9 |
| 5 | `HRM-05` | Tuyển dụng – nhu cầu | 7 |
| 6 | `HRM-06` | Tuyển dụng – đăng tin & ứng viên | 12 |
| 7 | `HRM-07` | Onboarding | 9 |
| 8 | `HRM-08` | Định biên | 6 |
| 9 | `HRM-09` | Ca làm việc | 11 |
| 10 | `HRM-10` | Điều động nhân sự | 6 |
| 11 | `HRM-11` | Cấu hình chấm công | 11 |
| 12 | `HRM-12` | Thực hiện chấm công | 11 |
| 13 | `HRM-13` | Điều chỉnh & khóa công | 9 |
| 14 | `HRM-14` | Nghỉ phép & vắng mặt | 10 |
| 15 | `HRM-15` | Kỷ luật & khen thưởng | 5 |
| 16 | `HRM-16` | Offboarding / nghỉ việc | 8 |
| 17 | `HRM-17` | Cấu hình lương & phụ cấp | 11 |
| 18 | `HRM-18` | Tính lương & chi trả | 14 |
| 19 | `HRM-19` | Đánh giá hiệu suất | 5 |
| 20 | `HRM-20` | Báo cáo & dashboard HRM | 6 |

<details>
<summary>Bảng đầy đủ mã UC (bấm để mở)</summary>

| Mã UC | Nhóm | Tên chức năng | Ưu tiên | MoSCoW |
|---|---|---|---|---|
| `UC_HRM_001` | Cơ cấu tổ chức nhân sự | Tạo sơ đồ tổ chức công ty | Bắt buộc | Must |
| `UC_HRM_002` | Cơ cấu tổ chức nhân sự | Quản lý khối vận hành | Bắt buộc | Must |
| `UC_HRM_003` | Cơ cấu tổ chức nhân sự | Quản lý khối sản xuất | Bắt buộc | Must |
| `UC_HRM_004` | Cơ cấu tổ chức nhân sự | Quản lý danh mục điểm bán | Bắt buộc | Must |
| `UC_HRM_005` | Cơ cấu tổ chức nhân sự | Quản lý bộ phận trong đơn vị | Cao | Should |
| `UC_HRM_006` | Cơ cấu tổ chức nhân sự | Khai báo giờ làm việc theo đơn vị | Bắt buộc | Must |
| `UC_HRM_007` | Cơ cấu tổ chức nhân sự | Quản lý chức danh nhân sự | Bắt buộc | Must |
| `UC_HRM_008` | Cơ cấu tổ chức nhân sự | Quản lý vị trí công việc | Cao | Should |
| `UC_HRM_009` | Cơ cấu tổ chức nhân sự | Quản lý loại nhân sự | Bắt buộc | Must |
| `UC_HRM_010` | Cơ cấu tổ chức nhân sự | Quản lý cấp bậc / level | Cao | Should |
| `UC_HRM_011` | Cơ cấu tổ chức nhân sự | Định nghĩa trung tâm chi phí NS | Cao | Should |
| `UC_HRM_012` | Hồ sơ nhân sự | Sinh mã nhân sự tự động | Bắt buộc | Must |
| `UC_HRM_013` | Hồ sơ nhân sự | Tạo hồ sơ nhân sự mới | Bắt buộc | Must |
| `UC_HRM_014` | Hồ sơ nhân sự | Cập nhật thông tin cá nhân | Bắt buộc | Must |
| `UC_HRM_015` | Hồ sơ nhân sự | NV tự cập nhật hồ sơ | Cao | Should |
| `UC_HRM_016` | Hồ sơ nhân sự | Upload ảnh đại diện | Trung bình | Could |
| `UC_HRM_017` | Hồ sơ nhân sự | Upload giấy tờ tùy thân | Bắt buộc | Must |
| `UC_HRM_018` | Hồ sơ nhân sự | Gắn nhân sự vào đơn vị chính | Bắt buộc | Must |
| `UC_HRM_019` | Hồ sơ nhân sự | Gắn nhân sự vào bộ phận | Bắt buộc | Must |
| `UC_HRM_020` | Hồ sơ nhân sự | Gắn chức danh / level | Bắt buộc | Must |
| `UC_HRM_021` | Hồ sơ nhân sự | Gắn loại nhân sự | Bắt buộc | Must |
| `UC_HRM_022` | Hồ sơ nhân sự | Gắn nhiều nhãn hồ sơ | Cao | Should |
| `UC_HRM_023` | Hồ sơ nhân sự | Quản lý người thân / liên hệ khẩn | Cao | Should |
| `UC_HRM_024` | Hồ sơ nhân sự | Quản lý trình độ / kỹ năng | Trung bình | Could |
| `UC_HRM_025` | Hồ sơ nhân sự | Tìm kiếm nhân sự đa tiêu chí | Bắt buộc | Must |
| `UC_HRM_026` | Hồ sơ nhân sự | Xuất danh sách nhân sự Excel | Bắt buộc | Must |
| `UC_HRM_027` | Hồ sơ nhân sự | Khóa hồ sơ đã nghỉ | Bắt buộc | Must |
| `UC_HRM_028` | Hồ sơ nhân sự | Xem hồ sơ theo quyền | Bắt buộc | Must |
| `UC_HRM_029` | Trạng thái & biến động nhân sự | Chuyển trạng thái Thử việc | Bắt buộc | Must |
| `UC_HRM_030` | Trạng thái & biến động nhân sự | Chuyển trạng thái Chính thức | Bắt buộc | Must |
| `UC_HRM_031` | Trạng thái & biến động nhân sự | Chuyển trạng thái Tạm nghỉ | Cao | Should |
| `UC_HRM_032` | Trạng thái & biến động nhân sự | Chuyển trạng thái Nghỉ việc | Bắt buộc | Must |
| `UC_HRM_033` | Trạng thái & biến động nhân sự | Lịch sử thay đổi trạng thái | Bắt buộc | Must |
| `UC_HRM_034` | Trạng thái & biến động nhân sự | Điều chuyển đơn vị / bộ phận | Bắt buộc | Must |
| `UC_HRM_035` | Trạng thái & biến động nhân sự | Thăng chức / đổi chức danh | Bắt buộc | Must |
| `UC_HRM_036` | Trạng thái & biến động nhân sự | Cảnh báo sắp hết hạn thử việc | Bắt buộc | Must |
| `UC_HRM_037` | Trạng thái & biến động nhân sự | Báo cáo biến động nhân sự | Cao | Should |
| `UC_HRM_038` | Hợp đồng lao động | Tạo hợp đồng lao động | Bắt buộc | Must |
| `UC_HRM_039` | Hợp đồng lao động | Tạo phụ lục hợp đồng | Bắt buộc | Must |
| `UC_HRM_040` | Hợp đồng lao động | Upload bản scan hợp đồng | Bắt buộc | Must |
| `UC_HRM_041` | Hợp đồng lao động | Gia hạn hợp đồng | Bắt buộc | Must |
| `UC_HRM_042` | Hợp đồng lao động | Thanh lý / chấm dứt hợp đồng | Bắt buộc | Must |
| `UC_HRM_043` | Hợp đồng lao động | Cảnh báo hết hạn hợp đồng | Bắt buộc | Must |
| `UC_HRM_044` | Hợp đồng lao động | In / xuất mẫu hợp đồng | Cao | Should |
| `UC_HRM_045` | Hợp đồng lao động | Quản lý lương hợp đồng | Bắt buộc | Must |
| `UC_HRM_046` | Hợp đồng lao động | Lịch sử hợp đồng theo nhân sự | Bắt buộc | Must |
| `UC_HRM_047` | Tuyển dụng – nhu cầu | Tạo phiếu đề xuất tuyển dụng | Bắt buộc | Must |
| `UC_HRM_048` | Tuyển dụng – nhu cầu | Chọn vị trí & số lượng cần tuyển | Bắt buộc | Must |
| `UC_HRM_049` | Tuyển dụng – nhu cầu | Nhập lý do tuyển dụng | Bắt buộc | Must |
| `UC_HRM_050` | Tuyển dụng – nhu cầu | Gửi phiếu đề xuất đi duyệt | Bắt buộc | Must |
| `UC_HRM_051` | Tuyển dụng – nhu cầu | Duyệt / từ chối đề xuất | Bắt buộc | Must |
| `UC_HRM_052` | Tuyển dụng – nhu cầu | Xem lịch sử duyệt đề xuất | Cao | Should |
| `UC_HRM_053` | Tuyển dụng – nhu cầu | Đóng / hủy phiếu đề xuất | Cao | Should |
| `UC_HRM_054` | Tuyển dụng – đăng tin & ứng viên | Tạo tin tuyển từ phiếu đã duyệt | Bắt buộc | Must |
| `UC_HRM_055` | Tuyển dụng – đăng tin & ứng viên | Ghi nhận kênh đăng tuyển | Cao | Should |
| `UC_HRM_056` | Tuyển dụng – đăng tin & ứng viên | Nhập hồ sơ ứng viên | Bắt buộc | Must |
| `UC_HRM_057` | Tuyển dụng – đăng tin & ứng viên | Upload file CV | Bắt buộc | Must |
| `UC_HRM_058` | Tuyển dụng – đăng tin & ứng viên | Import ứng viên hàng loạt | Trung bình | Could |
| `UC_HRM_059` | Tuyển dụng – đăng tin & ứng viên | Sơ loại ứng viên | Bắt buộc | Must |
| `UC_HRM_060` | Tuyển dụng – đăng tin & ứng viên | Chuyển ứng viên cho đơn vị đánh giá | Bắt buộc | Must |
| `UC_HRM_061` | Tuyển dụng – đăng tin & ứng viên | Form đánh giá ứng viên | Bắt buộc | Must |
| `UC_HRM_062` | Tuyển dụng – đăng tin & ứng viên | Từ chối / chấp nhận ứng viên | Bắt buộc | Must |
| `UC_HRM_063` | Tuyển dụng – đăng tin & ứng viên | Pipeline trạng thái ứng viên | Bắt buộc | Must |
| `UC_HRM_064` | Tuyển dụng – đăng tin & ứng viên | Lịch sử chăm sóc ứng viên | Cao | Should |
| `UC_HRM_065` | Tuyển dụng – đăng tin & ứng viên | Báo cáo hiệu quả kênh tuyển | Cao | Should |
| `UC_HRM_066` | Onboarding | Cấu hình thời hạn onboarding | Bắt buộc | Must |
| `UC_HRM_067` | Onboarding | Cấu hình thời hạn thử việc | Bắt buộc | Must |
| `UC_HRM_068` | Onboarding | Tạo hồ sơ nhân viên mới từ ứng viên | Bắt buộc | Must |
| `UC_HRM_069` | Onboarding | Gán người hướng dẫn | Cao | Should |
| `UC_HRM_070` | Onboarding | Checklist onboarding | Bắt buộc | Must |
| `UC_HRM_071` | Onboarding | Upload chứng chỉ / giấy tờ | Bắt buộc | Must |
| `UC_HRM_072` | Onboarding | Đánh giá kết thúc thử việc | Bắt buộc | Must |
| `UC_HRM_073` | Onboarding | Chuyển thử việc thành chính thức | Bắt buộc | Must |
| `UC_HRM_074` | Onboarding | Cảnh báo hết hạn thử việc | Bắt buộc | Must |
| `UC_HRM_075` | Định biên | Khai báo định biên theo đơn vị | Bắt buộc | Must |
| `UC_HRM_076` | Định biên | Khai báo định biên theo ca | Bắt buộc | Must |
| `UC_HRM_077` | Định biên | Khai báo định biên theo bộ phận | Cao | Should |
| `UC_HRM_078` | Định biên | So sánh thực tế vs định biên | Bắt buộc | Must |
| `UC_HRM_079` | Định biên | Cảnh báo thiếu người | Bắt buộc | Must |
| `UC_HRM_080` | Định biên | Duyệt thay đổi định biên | Cao | Should |
| `UC_HRM_081` | Ca làm việc | Tạo mẫu ca làm việc | Bắt buộc | Must |
| `UC_HRM_082` | Ca làm việc | Xếp lịch ca nhân viên | Bắt buộc | Must |
| `UC_HRM_083` | Ca làm việc | Xếp lịch ca theo tuần / tháng | Bắt buộc | Must |
| `UC_HRM_084` | Ca làm việc | Đổi ca giữa nhân viên | Cao | Should |
| `UC_HRM_085` | Ca làm việc | Hủy lịch ca | Bắt buộc | Must |
| `UC_HRM_086` | Ca làm việc | Xem lịch ca theo đơn vị | Bắt buộc | Must |
| `UC_HRM_087` | Ca làm việc | Xem lịch ca cá nhân trên APP | Bắt buộc | Must |
| `UC_HRM_088` | Ca làm việc | Import lịch ca Excel | Trung bình | Could |
| `UC_HRM_089` | Ca làm việc | Sao chép lịch ca | Cao | Should |
| `UC_HRM_090` | Ca làm việc | Khóa sổ lịch ca theo kỳ | Cao | Should |
| `UC_HRM_091` | Ca làm việc | In / xuất lịch ca | Cao | Should |
| `UC_HRM_092` | Điều động nhân sự | Tạo lệnh điều động | Cao | Should |
| `UC_HRM_093` | Điều động nhân sự | Đề xuất nhu cầu điều động | Cao | Should |
| `UC_HRM_094` | Điều động nhân sự | Nhận lệnh điều động trên APP | Cao | Should |
| `UC_HRM_095` | Điều động nhân sự | Theo dõi nhân sự điều động | Cao | Should |
| `UC_HRM_096` | Điều động nhân sự | Gắn nhãn công điều động khi chấm | Cao | Should |
| `UC_HRM_097` | Điều động nhân sự | Báo cáo giờ / chi phí điều động | Cao | Should |
| `UC_HRM_098` | Cấu hình chấm công | Cấu hình chấm vân tay / sinh trắc | Bắt buộc | Must |
| `UC_HRM_099` | Cấu hình chấm công | Cấu hình chấm APP điện thoại | Bắt buộc | Must |
| `UC_HRM_100` | Cấu hình chấm công | Cấu hình chấm QR / mã nhân sự | Cao | Should |
| `UC_HRM_101` | Cấu hình chấm công | Đăng ký thiết bị chấm | Bắt buộc | Must |
| `UC_HRM_102` | Cấu hình chấm công | Cấu hình geo-fence điểm chấm | Cao | Should |
| `UC_HRM_103` | Cấu hình chấm công | Cấu hình quy tắc đi trễ | Bắt buộc | Must |
| `UC_HRM_104` | Cấu hình chấm công | Cấu hình mức trừ công khi trễ | Bắt buộc | Must |
| `UC_HRM_105` | Cấu hình chấm công | Cấu hình quên check-out | Bắt buộc | Must |
| `UC_HRM_106` | Cấu hình chấm công | Cấu hình thời hạn xin điều chỉnh | Bắt buộc | Must |
| `UC_HRM_107` | Cấu hình chấm công | Cấu hình làm thêm giờ (OT) | Cao | Should |
| `UC_HRM_108` | Cấu hình chấm công | Cấu hình ca đêm / ngày lễ | Cao | Should |
| `UC_HRM_109` | Thực hiện chấm công | Check-in đầu ca | Bắt buộc | Must |
| `UC_HRM_110` | Thực hiện chấm công | Check-out cuối ca | Bắt buộc | Must |
| `UC_HRM_111` | Thực hiện chấm công | Xem lịch sử chấm cá nhân | Bắt buộc | Must |
| `UC_HRM_112` | Thực hiện chấm công | Bảng chấm công theo đơn vị | Bắt buộc | Must |
| `UC_HRM_113` | Thực hiện chấm công | Bảng chấm công toàn công ty | Bắt buộc | Must |
| `UC_HRM_114` | Thực hiện chấm công | Cảnh báo thiếu chấm realtime | Cao | Should |
| `UC_HRM_115` | Thực hiện chấm công | Tự tính phút đi trễ | Bắt buộc | Must |
| `UC_HRM_116` | Thực hiện chấm công | Tự trừ công do đi trễ | Bắt buộc | Must |
| `UC_HRM_117` | Thực hiện chấm công | Đánh dấu quên chấm | Bắt buộc | Must |
| `UC_HRM_118` | Thực hiện chấm công | Đồng bộ dữ liệu từ máy chấm | Bắt buộc | Must |
| `UC_HRM_119` | Thực hiện chấm công | Xử lý công OT tự động | Cao | Should |
| `UC_HRM_120` | Điều chỉnh & khóa công | Tạo phiếu xin điều chỉnh công | Bắt buộc | Must |
| `UC_HRM_121` | Điều chỉnh & khóa công | Đính kèm lý do / bằng chứng | Bắt buộc | Must |
| `UC_HRM_122` | Điều chỉnh & khóa công | Duyệt / từ chối điều chỉnh | Bắt buộc | Must |
| `UC_HRM_123` | Điều chỉnh & khóa công | Ghi nhận vi phạm đi trễ | Cao | Should |
| `UC_HRM_124` | Điều chỉnh & khóa công | Lập bảng phạt | Cao | Should |
| `UC_HRM_125` | Điều chỉnh & khóa công | Áp dụng phạt vào kỳ lương | Cao | Should |
| `UC_HRM_126` | Điều chỉnh & khóa công | Khóa bảng công theo kỳ | Bắt buộc | Must |
| `UC_HRM_127` | Điều chỉnh & khóa công | Mở khóa bảng công có kiểm soát | Bắt buộc | Must |
| `UC_HRM_128` | Điều chỉnh & khóa công | Xác nhận bảng công | Cao | Should |
| `UC_HRM_129` | Nghỉ phép & vắng mặt | Danh mục loại nghỉ | Bắt buộc | Must |
| `UC_HRM_130` | Nghỉ phép & vắng mặt | Cấu hình quỹ phép theo loại NS | Bắt buộc | Must |
| `UC_HRM_131` | Nghỉ phép & vắng mặt | Cấp phát / điều chỉnh quỹ phép | Bắt buộc | Must |
| `UC_HRM_132` | Nghỉ phép & vắng mặt | Tạo đơn xin nghỉ | Bắt buộc | Must |
| `UC_HRM_133` | Nghỉ phép & vắng mặt | Duyệt đơn nghỉ đa cấp | Bắt buộc | Must |
| `UC_HRM_134` | Nghỉ phép & vắng mặt | Hủy đơn nghỉ | Cao | Should |
| `UC_HRM_135` | Nghỉ phép & vắng mặt | Xem quỹ phép còn lại | Bắt buộc | Must |
| `UC_HRM_136` | Nghỉ phép & vắng mặt | Lịch nghỉ theo đơn vị | Cao | Should |
| `UC_HRM_137` | Nghỉ phép & vắng mặt | Import nghỉ lễ / ngày nghỉ | Bắt buộc | Must |
| `UC_HRM_138` | Nghỉ phép & vắng mặt | Báo cáo nghỉ / quỹ phép | Cao | Should |
| `UC_HRM_139` | Kỷ luật & khen thưởng | Ghi nhận quyết định khen thưởng | Cao | Should |
| `UC_HRM_140` | Kỷ luật & khen thưởng | Ghi nhận quyết định kỷ luật | Cao | Should |
| `UC_HRM_141` | Kỷ luật & khen thưởng | Đính kèm quyết định | Cao | Should |
| `UC_HRM_142` | Kỷ luật & khen thưởng | Ảnh hưởng lương / phụ cấp | Cao | Should |
| `UC_HRM_143` | Kỷ luật & khen thưởng | Báo cáo khen thưởng – kỷ luật | Trung bình | Could |
| `UC_HRM_144` | Offboarding / nghỉ việc | Tạo đơn nghỉ việc | Bắt buộc | Must |
| `UC_HRM_145` | Offboarding / nghỉ việc | Cấu hình / kiểm tra báo trước | Bắt buộc | Must |
| `UC_HRM_146` | Offboarding / nghỉ việc | Duyệt đơn nghỉ việc | Bắt buộc | Must |
| `UC_HRM_147` | Offboarding / nghỉ việc | Checklist bàn giao | Bắt buộc | Must |
| `UC_HRM_148` | Offboarding / nghỉ việc | Thu hồi quyền hệ thống | Bắt buộc | Must |
| `UC_HRM_149` | Offboarding / nghỉ việc | Quyết toán phép / lương nghỉ việc | Bắt buộc | Must |
| `UC_HRM_150` | Offboarding / nghỉ việc | Phỏng vấn nghỉ việc | Trung bình | Could |
| `UC_HRM_151` | Offboarding / nghỉ việc | Báo cáo nghỉ việc / lý do | Cao | Should |
| `UC_HRM_152` | Cấu hình lương & phụ cấp | Tạo thang bậc lương | Bắt buộc | Must |
| `UC_HRM_153` | Cấu hình lương & phụ cấp | Gán bậc lương theo nhân sự | Bắt buộc | Must |
| `UC_HRM_154` | Cấu hình lương & phụ cấp | Gán bậc theo trạng thái | Bắt buộc | Must |
| `UC_HRM_155` | Cấu hình lương & phụ cấp | Đơn giá giờ / ngày nhân viên | Bắt buộc | Must |
| `UC_HRM_156` | Cấu hình lương & phụ cấp | Quản lý lương thực tế chi trả | Bắt buộc | Must |
| `UC_HRM_157` | Cấu hình lương & phụ cấp | Danh mục phụ cấp | Bắt buộc | Must |
| `UC_HRM_158` | Cấu hình lương & phụ cấp | Rule phụ cấp theo ca | Bắt buộc | Must |
| `UC_HRM_159` | Cấu hình lương & phụ cấp | Rule phụ cấp đặc thù | Cao | Should |
| `UC_HRM_160` | Cấu hình lương & phụ cấp | Cấu hình bảo hiểm | Bắt buộc | Must |
| `UC_HRM_161` | Cấu hình lương & phụ cấp | Cấu hình thuế TNCN | Bắt buộc | Must |
| `UC_HRM_162` | Cấu hình lương & phụ cấp | Cấu hình tạm ứng / khấu trừ | Bắt buộc | Must |
| `UC_HRM_163` | Tính lương & chi trả | Tạo kỳ lương | Bắt buộc | Must |
| `UC_HRM_164` | Tính lương & chi trả | Tổng hợp công vào kỳ lương | Bắt buộc | Must |
| `UC_HRM_165` | Tính lương & chi trả | Tính lương tự động theo rule | Bắt buộc | Must |
| `UC_HRM_166` | Tính lương & chi trả | Nhập thưởng / phụ cấp phát sinh | Bắt buộc | Must |
| `UC_HRM_167` | Tính lương & chi trả | Nhập khấu trừ / tạm ứng | Bắt buộc | Must |
| `UC_HRM_168` | Tính lương & chi trả | Xem / chỉnh bảng lương chi tiết | Bắt buộc | Must |
| `UC_HRM_169` | Tính lương & chi trả | Xác nhận bảng lương | Bắt buộc | Must |
| `UC_HRM_170` | Tính lương & chi trả | Khóa kỳ lương | Bắt buộc | Must |
| `UC_HRM_171` | Tính lương & chi trả | Phiếu lương cá nhân (APP) | Bắt buộc | Must |
| `UC_HRM_172` | Tính lương & chi trả | Xuất bảng lương tổng hợp | Bắt buộc | Must |
| `UC_HRM_173` | Tính lương & chi trả | Xuất file chi lương ngân hàng | Bắt buộc | Must |
| `UC_HRM_174` | Tính lương & chi trả | Đồng bộ bút toán lương sang FIN | Cao | Should |
| `UC_HRM_175` | Tính lương & chi trả | Báo cáo chi phí lương theo đơn vị | Bắt buộc | Must |
| `UC_HRM_176` | Tính lương & chi trả | So sánh lương kỳ này / kỳ trước | Cao | Should |
| `UC_HRM_177` | Đánh giá hiệu suất | Mẫu đánh giá KPI / năng lực | Trung bình | Could |
| `UC_HRM_178` | Đánh giá hiệu suất | Tạo kỳ đánh giá | Trung bình | Could |
| `UC_HRM_179` | Đánh giá hiệu suất | Quản lý đánh giá nhân viên | Trung bình | Could |
| `UC_HRM_180` | Đánh giá hiệu suất | Nhân viên tự đánh giá | Thấp | Won't / Later |
| `UC_HRM_181` | Đánh giá hiệu suất | Tổng hợp kết quả đánh giá | Trung bình | Could |
| `UC_HRM_182` | Báo cáo & dashboard HRM | Dashboard headcount & biến động | Bắt buộc | Must |
| `UC_HRM_183` | Báo cáo & dashboard HRM | Báo cáo công / OT / đi trễ | Bắt buộc | Must |
| `UC_HRM_184` | Báo cáo & dashboard HRM | Báo cáo tuyển dụng funnel | Cao | Should |
| `UC_HRM_185` | Báo cáo & dashboard HRM | Báo cáo quỹ phép | Cao | Should |
| `UC_HRM_186` | Báo cáo & dashboard HRM | Báo cáo chi phí nhân sự | Bắt buộc | Must |
| `UC_HRM_187` | Báo cáo & dashboard HRM | Báo cáo định biên vs thực tế | Cao | Should |

</details>

---

## 7. Đặc tả chức năng theo nhóm

Mỗi UC bên dưới gồm: mô tả, tác nhân, tiền/hậu điều kiện, luồng chính, quy tắc, tiêu chí chấp nhận và ưu tiên. Đây là mức đặc tả BA để chốt phạm vi; chi tiết UI/API sẽ bổ sung ở giai đoạn thiết kế.

### 7.1. Cơ cấu tổ chức nhân sự (`HRM-01`)

Nhóm này gồm **11** chức năng. Tác nhân mặc định: **HR Admin**.

#### UC_HRM_001 — Tạo sơ đồ tổ chức công ty

- **Mô tả:** Company org structure
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo sơ đồ tổ chức công ty” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_002 — Quản lý khối vận hành

- **Mô tả:** Operating divisions
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý khối vận hành
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Operating divisions)
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
  - AC1: Thực hiện thành công thao tác “Quản lý khối vận hành” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_003 — Quản lý khối sản xuất

- **Mô tả:** Manufacturing divisions
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Quản lý khối sản xuất” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_004 — Quản lý danh mục điểm bán

- **Mô tả:** Sales location master
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Quản lý danh mục điểm bán” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_005 — Quản lý bộ phận trong đơn vị

- **Mô tả:** Department within unit
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý bộ phận trong đơn vị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Department within unit)
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
  - AC1: Thực hiện thành công thao tác “Quản lý bộ phận trong đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_006 — Khai báo giờ làm việc theo đơn vị

- **Mô tả:** Working hours by unit
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Khai báo giờ làm việc theo đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_007 — Quản lý chức danh nhân sự

- **Mô tả:** Position titles
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý chức danh nhân sự
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Position titles)
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
  - AC1: Thực hiện thành công thao tác “Quản lý chức danh nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_008 — Quản lý vị trí công việc

- **Mô tả:** Job positions
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý vị trí công việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Job positions)
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
  - AC1: Thực hiện thành công thao tác “Quản lý vị trí công việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_009 — Quản lý loại nhân sự

- **Mô tả:** Employee types
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý loại nhân sự
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Employee types)
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
  - AC1: Thực hiện thành công thao tác “Quản lý loại nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_010 — Quản lý cấp bậc / level

- **Mô tả:** Grade/level master
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý cấp bậc / level
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Grade/level master)
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
  - AC1: Thực hiện thành công thao tác “Quản lý cấp bậc / level” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_011 — Định nghĩa trung tâm chi phí NS

- **Mô tả:** HR cost centers
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Định nghĩa trung tâm chi phí NS
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (HR cost centers)
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
  - AC1: Thực hiện thành công thao tác “Định nghĩa trung tâm chi phí NS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.2. Hồ sơ nhân sự (`HRM-02`)

Nhóm này gồm **17** chức năng. Tác nhân mặc định: **HR Officer**.

#### UC_HRM_012 — Sinh mã nhân sự tự động

- **Mô tả:** Auto employee ID
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Sinh mã nhân sự tự động
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Auto employee ID)
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
  - AC1: Thực hiện thành công thao tác “Sinh mã nhân sự tự động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_013 — Tạo hồ sơ nhân sự mới

- **Mô tả:** Create employee record
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo hồ sơ nhân sự mới” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_014 — Cập nhật thông tin cá nhân

- **Mô tả:** Personal info update
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cập nhật thông tin cá nhân
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Personal info update)
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
  - AC1: Thực hiện thành công thao tác “Cập nhật thông tin cá nhân” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_015 — NV tự cập nhật hồ sơ

- **Mô tả:** Self-service profile
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: NV tự cập nhật hồ sơ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Self-service profile)
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
  - AC1: Thực hiện thành công thao tác “NV tự cập nhật hồ sơ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_016 — Upload ảnh đại diện

- **Mô tả:** Profile photo
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Upload ảnh đại diện
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Profile photo)
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
  - AC1: Thực hiện thành công thao tác “Upload ảnh đại diện” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_017 — Upload giấy tờ tùy thân

- **Mô tả:** ID documents
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Upload giấy tờ tùy thân
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (ID documents)
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
  - AC1: Thực hiện thành công thao tác “Upload giấy tờ tùy thân” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_018 — Gắn nhân sự vào đơn vị chính

- **Mô tả:** Primary org assignment
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn nhân sự vào đơn vị chính
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Primary org assignment)
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
  - AC1: Thực hiện thành công thao tác “Gắn nhân sự vào đơn vị chính” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_019 — Gắn nhân sự vào bộ phận

- **Mô tả:** Department assignment
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn nhân sự vào bộ phận
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Department assignment)
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
  - AC1: Thực hiện thành công thao tác “Gắn nhân sự vào bộ phận” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_020 — Gắn chức danh / level

- **Mô tả:** Position & grade
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn chức danh / level
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Position & grade)
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
  - AC1: Thực hiện thành công thao tác “Gắn chức danh / level” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_021 — Gắn loại nhân sự

- **Mô tả:** Employee classification
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn loại nhân sự
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Employee classification)
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
  - AC1: Thực hiện thành công thao tác “Gắn loại nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_022 — Gắn nhiều nhãn hồ sơ

- **Mô tả:** Employee tags
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn nhiều nhãn hồ sơ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Employee tags)
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
  - AC1: Thực hiện thành công thao tác “Gắn nhiều nhãn hồ sơ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_023 — Quản lý người thân / liên hệ khẩn

- **Mô tả:** Emergency contacts
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý người thân / liên hệ khẩn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Emergency contacts)
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
  - AC1: Thực hiện thành công thao tác “Quản lý người thân / liên hệ khẩn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_024 — Quản lý trình độ / kỹ năng

- **Mô tả:** Skills & qualifications
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý trình độ / kỹ năng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Skills & qualifications)
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
  - AC1: Thực hiện thành công thao tác “Quản lý trình độ / kỹ năng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_025 — Tìm kiếm nhân sự đa tiêu chí

- **Mô tả:** Advanced employee search
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tìm kiếm nhân sự đa tiêu chí
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Advanced employee search)
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
  - AC1: Thực hiện thành công thao tác “Tìm kiếm nhân sự đa tiêu chí” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_026 — Xuất danh sách nhân sự Excel

- **Mô tả:** Export employee list
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất danh sách nhân sự Excel” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_027 — Khóa hồ sơ đã nghỉ

- **Mô tả:** Lock terminated records
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khóa hồ sơ đã nghỉ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Lock terminated records)
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
  - AC1: Thực hiện thành công thao tác “Khóa hồ sơ đã nghỉ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_028 — Xem hồ sơ theo quyền

- **Mô tả:** Field-level security
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem hồ sơ theo quyền
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Field-level security)
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
  - AC1: Thực hiện thành công thao tác “Xem hồ sơ theo quyền” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.3. Trạng thái & biến động nhân sự (`HRM-03`)

Nhóm này gồm **9** chức năng. Tác nhân mặc định: **HR Officer**.

#### UC_HRM_029 — Chuyển trạng thái Thử việc

- **Mô tả:** Probation status
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển trạng thái Thử việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Probation status)
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
  - AC1: Thực hiện thành công thao tác “Chuyển trạng thái Thử việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_030 — Chuyển trạng thái Chính thức

- **Mô tả:** Regular status
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển trạng thái Chính thức
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Regular status)
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
  - AC1: Thực hiện thành công thao tác “Chuyển trạng thái Chính thức” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_031 — Chuyển trạng thái Tạm nghỉ

- **Mô tả:** Temporary leave
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển trạng thái Tạm nghỉ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Temporary leave)
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
  - AC1: Thực hiện thành công thao tác “Chuyển trạng thái Tạm nghỉ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_032 — Chuyển trạng thái Nghỉ việc

- **Mô tả:** Termination
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển trạng thái Nghỉ việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Termination)
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
  - AC1: Thực hiện thành công thao tác “Chuyển trạng thái Nghỉ việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_033 — Lịch sử thay đổi trạng thái

- **Mô tả:** Status timeline
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch sử thay đổi trạng thái
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Status timeline)
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
  - AC1: Thực hiện thành công thao tác “Lịch sử thay đổi trạng thái” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_034 — Điều chuyển đơn vị / bộ phận

- **Mô tả:** Internal transfer
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Điều chuyển đơn vị / bộ phận
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Internal transfer)
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
  - AC1: Thực hiện thành công thao tác “Điều chuyển đơn vị / bộ phận” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_035 — Thăng chức / đổi chức danh

- **Mô tả:** Promotion/demotion
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thăng chức / đổi chức danh
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Promotion/demotion)
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
  - AC1: Thực hiện thành công thao tác “Thăng chức / đổi chức danh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_036 — Cảnh báo sắp hết hạn thử việc

- **Mô tả:** Probation end reminder
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo sắp hết hạn thử việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Probation end reminder)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo sắp hết hạn thử việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_037 — Báo cáo biến động nhân sự

- **Mô tả:** Headcount movement report
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo biến động nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.4. Hợp đồng lao động (`HRM-04`)

Nhóm này gồm **9** chức năng. Tác nhân mặc định: **HR Officer**.

#### UC_HRM_038 — Tạo hợp đồng lao động

- **Mô tả:** Create employment contract
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo hợp đồng lao động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_039 — Tạo phụ lục hợp đồng

- **Mô tả:** Contract addendum
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo phụ lục hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_040 — Upload bản scan hợp đồng

- **Mô tả:** Contract file attachment
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Upload bản scan hợp đồng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Contract file attachment)
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
  - AC1: Thực hiện thành công thao tác “Upload bản scan hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_041 — Gia hạn hợp đồng

- **Mô tả:** Contract renewal
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gia hạn hợp đồng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Contract renewal)
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
  - AC1: Thực hiện thành công thao tác “Gia hạn hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_042 — Thanh lý / chấm dứt hợp đồng

- **Mô tả:** Contract termination
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thanh lý / chấm dứt hợp đồng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Contract termination)
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
  - AC1: Thực hiện thành công thao tác “Thanh lý / chấm dứt hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_043 — Cảnh báo hết hạn hợp đồng

- **Mô tả:** Contract expiry alert
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo hết hạn hợp đồng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Contract expiry alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo hết hạn hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_044 — In / xuất mẫu hợp đồng

- **Mô tả:** Contract template
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “In / xuất mẫu hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_045 — Quản lý lương hợp đồng

- **Mô tả:** Contract salary
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý lương hợp đồng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Contract salary)
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
  - AC1: Thực hiện thành công thao tác “Quản lý lương hợp đồng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_046 — Lịch sử hợp đồng theo nhân sự

- **Mô tả:** Contract history
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch sử hợp đồng theo nhân sự
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Contract history)
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
  - AC1: Thực hiện thành công thao tác “Lịch sử hợp đồng theo nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.5. Tuyển dụng – nhu cầu (`HRM-05`)

Nhóm này gồm **7** chức năng. Tác nhân mặc định: **Line Manager / HR Officer**.

#### UC_HRM_047 — Tạo phiếu đề xuất tuyển dụng

- **Mô tả:** Recruitment requisition
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo phiếu đề xuất tuyển dụng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_048 — Chọn vị trí & số lượng cần tuyển

- **Mô tả:** Headcount request
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chọn vị trí & số lượng cần tuyển
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Headcount request)
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
  - AC1: Thực hiện thành công thao tác “Chọn vị trí & số lượng cần tuyển” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_049 — Nhập lý do tuyển dụng

- **Mô tả:** Recruitment reason
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhập lý do tuyển dụng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Recruitment reason)
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
  - AC1: Thực hiện thành công thao tác “Nhập lý do tuyển dụng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_050 — Gửi phiếu đề xuất đi duyệt

- **Mô tả:** Submit for approval
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Gửi phiếu đề xuất đi duyệt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_051 — Duyệt / từ chối đề xuất

- **Mô tả:** Approval workflow
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt / từ chối đề xuất” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_052 — Xem lịch sử duyệt đề xuất

- **Mô tả:** Approval history
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xem lịch sử duyệt đề xuất” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_053 — Đóng / hủy phiếu đề xuất

- **Mô tả:** Close requisition
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Đóng / hủy phiếu đề xuất” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.6. Tuyển dụng – đăng tin & ứng viên (`HRM-06`)

Nhóm này gồm **12** chức năng. Tác nhân mặc định: **Recruiter / HR Officer**.

#### UC_HRM_054 — Tạo tin tuyển từ phiếu đã duyệt

- **Mô tả:** Job posting
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo tin tuyển từ phiếu đã duyệt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_055 — Ghi nhận kênh đăng tuyển

- **Mô tả:** Recruitment channel log
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận kênh đăng tuyển
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Recruitment channel log)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận kênh đăng tuyển” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_056 — Nhập hồ sơ ứng viên

- **Mô tả:** Candidate entry
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhập hồ sơ ứng viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Candidate entry)
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
  - AC1: Thực hiện thành công thao tác “Nhập hồ sơ ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_057 — Upload file CV

- **Mô tả:** CV attachment
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Upload file CV
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (CV attachment)
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
  - AC1: Thực hiện thành công thao tác “Upload file CV” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_058 — Import ứng viên hàng loạt

- **Mô tả:** Bulk candidate import
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Import ứng viên hàng loạt
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bulk candidate import)
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
  - AC1: Thực hiện thành công thao tác “Import ứng viên hàng loạt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_059 — Sơ loại ứng viên

- **Mô tả:** Candidate screening
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Sơ loại ứng viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Candidate screening)
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
  - AC1: Thực hiện thành công thao tác “Sơ loại ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_060 — Chuyển ứng viên cho đơn vị đánh giá

- **Mô tả:** Assign to hiring manager
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển ứng viên cho đơn vị đánh giá
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Assign to hiring manager)
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
  - AC1: Thực hiện thành công thao tác “Chuyển ứng viên cho đơn vị đánh giá” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_061 — Form đánh giá ứng viên

- **Mô tả:** Interview scorecard
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Form đánh giá ứng viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Interview scorecard)
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
  - AC1: Thực hiện thành công thao tác “Form đánh giá ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_062 — Từ chối / chấp nhận ứng viên

- **Mô tả:** Hiring decision
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Từ chối / chấp nhận ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_063 — Pipeline trạng thái ứng viên

- **Mô tả:** Recruitment pipeline
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Pipeline trạng thái ứng viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Recruitment pipeline)
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
  - AC1: Thực hiện thành công thao tác “Pipeline trạng thái ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_064 — Lịch sử chăm sóc ứng viên

- **Mô tả:** Candidate communication log
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch sử chăm sóc ứng viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Candidate communication log)
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
  - AC1: Thực hiện thành công thao tác “Lịch sử chăm sóc ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_065 — Báo cáo hiệu quả kênh tuyển

- **Mô tả:** Source effectiveness
- **Tác nhân chính:** Recruiter / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo hiệu quả kênh tuyển” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.7. Onboarding (`HRM-07`)

Nhóm này gồm **9** chức năng. Tác nhân mặc định: **HR Officer / Line Manager**.

#### UC_HRM_066 — Cấu hình thời hạn onboarding

- **Mô tả:** Onboarding period setup
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thời hạn onboarding” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_067 — Cấu hình thời hạn thử việc

- **Mô tả:** Probation period setup
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thời hạn thử việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_068 — Tạo hồ sơ nhân viên mới từ ứng viên

- **Mô tả:** New hire from candidate
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo hồ sơ nhân viên mới từ ứng viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_069 — Gán người hướng dẫn

- **Mô tả:** Assign buddy/mentor
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gán người hướng dẫn
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Assign buddy/mentor)
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
  - AC1: Thực hiện thành công thao tác “Gán người hướng dẫn” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_070 — Checklist onboarding

- **Mô tả:** Onboarding tasks
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist onboarding
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Onboarding tasks)
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
  - AC1: Thực hiện thành công thao tác “Checklist onboarding” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_071 — Upload chứng chỉ / giấy tờ

- **Mô tả:** Required documents
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Upload chứng chỉ / giấy tờ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Required documents)
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
  - AC1: Thực hiện thành công thao tác “Upload chứng chỉ / giấy tờ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_072 — Đánh giá kết thúc thử việc

- **Mô tả:** Probation evaluation
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đánh giá kết thúc thử việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Probation evaluation)
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
  - AC1: Thực hiện thành công thao tác “Đánh giá kết thúc thử việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_073 — Chuyển thử việc thành chính thức

- **Mô tả:** Confirmation
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Chuyển thử việc thành chính thức
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Confirmation)
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
  - AC1: Thực hiện thành công thao tác “Chuyển thử việc thành chính thức” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_074 — Cảnh báo hết hạn thử việc

- **Mô tả:** Probation end alert
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo hết hạn thử việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Probation end alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo hết hạn thử việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.8. Định biên (`HRM-08`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **HR Admin / Line Manager**.

#### UC_HRM_075 — Khai báo định biên theo đơn vị

- **Mô tả:** Headcount plan by unit
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Khai báo định biên theo đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_076 — Khai báo định biên theo ca

- **Mô tả:** Headcount by shift
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Khai báo định biên theo ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_077 — Khai báo định biên theo bộ phận

- **Mô tả:** Headcount by department
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Khai báo định biên theo bộ phận” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_078 — So sánh thực tế vs định biên

- **Mô tả:** Headcount gap analysis
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: So sánh thực tế vs định biên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Headcount gap analysis)
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
  - AC1: Thực hiện thành công thao tác “So sánh thực tế vs định biên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_079 — Cảnh báo thiếu người

- **Mô tả:** Understaffing alert
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo thiếu người
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Understaffing alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo thiếu người” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_080 — Duyệt thay đổi định biên

- **Mô tả:** Headcount change approval
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt thay đổi định biên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.9. Ca làm việc (`HRM-09`)

Nhóm này gồm **11** chức năng. Tác nhân mặc định: **Line Manager / HR Officer**.

#### UC_HRM_081 — Tạo mẫu ca làm việc

- **Mô tả:** Shift templates
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo mẫu ca làm việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_082 — Xếp lịch ca nhân viên

- **Mô tả:** Shift scheduling
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xếp lịch ca nhân viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Shift scheduling)
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
  - AC1: Thực hiện thành công thao tác “Xếp lịch ca nhân viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_083 — Xếp lịch ca theo tuần / tháng

- **Mô tả:** Calendar view
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xếp lịch ca theo tuần / tháng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Calendar view)
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
  - AC1: Thực hiện thành công thao tác “Xếp lịch ca theo tuần / tháng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_084 — Đổi ca giữa nhân viên

- **Mô tả:** Shift swap
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đổi ca giữa nhân viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Shift swap)
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
  - AC1: Thực hiện thành công thao tác “Đổi ca giữa nhân viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_085 — Hủy lịch ca

- **Mô tả:** Cancel shift assignment
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hủy lịch ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cancel shift assignment)
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
  - AC1: Thực hiện thành công thao tác “Hủy lịch ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_086 — Xem lịch ca theo đơn vị

- **Mô tả:** Manager shift view
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem lịch ca theo đơn vị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Manager shift view)
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
  - AC1: Thực hiện thành công thao tác “Xem lịch ca theo đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_087 — Xem lịch ca cá nhân trên APP

- **Mô tả:** Employee shift view
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem lịch ca cá nhân trên APP
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Employee shift view)
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
  - AC1: Thực hiện thành công thao tác “Xem lịch ca cá nhân trên APP” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_088 — Import lịch ca Excel

- **Mô tả:** Bulk shift import
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Import lịch ca Excel
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Bulk shift import)
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
  - AC1: Thực hiện thành công thao tác “Import lịch ca Excel” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_089 — Sao chép lịch ca

- **Mô tả:** Copy shift schedule
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Sao chép lịch ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Copy shift schedule)
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
  - AC1: Thực hiện thành công thao tác “Sao chép lịch ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_090 — Khóa sổ lịch ca theo kỳ

- **Mô tả:** Lock shift roster
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khóa sổ lịch ca theo kỳ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Lock shift roster)
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
  - AC1: Thực hiện thành công thao tác “Khóa sổ lịch ca theo kỳ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_091 — In / xuất lịch ca

- **Mô tả:** Export shift schedule
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “In / xuất lịch ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.10. Điều động nhân sự (`HRM-10`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **HR Officer / Line Manager**.

#### UC_HRM_092 — Tạo lệnh điều động

- **Mô tả:** Secondment order
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo lệnh điều động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_093 — Đề xuất nhu cầu điều động

- **Mô tả:** Secondment request
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Đề xuất nhu cầu điều động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_094 — Nhận lệnh điều động trên APP

- **Mô tả:** Accept assignment
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhận lệnh điều động trên APP
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Accept assignment)
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
  - AC1: Thực hiện thành công thao tác “Nhận lệnh điều động trên APP” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_095 — Theo dõi nhân sự điều động

- **Mô tả:** Secondment tracking
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Theo dõi nhân sự điều động
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Secondment tracking)
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
  - AC1: Thực hiện thành công thao tác “Theo dõi nhân sự điều động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_096 — Gắn nhãn công điều động khi chấm

- **Mô tả:** Secondment attendance tag
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gắn nhãn công điều động khi chấm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Secondment attendance tag)
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
  - AC1: Thực hiện thành công thao tác “Gắn nhãn công điều động khi chấm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_097 — Báo cáo giờ / chi phí điều động

- **Mô tả:** Secondment cost report
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo giờ / chi phí điều động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.11. Cấu hình chấm công (`HRM-11`)

Nhóm này gồm **11** chức năng. Tác nhân mặc định: **HR Admin**.

#### UC_HRM_098 — Cấu hình chấm vân tay / sinh trắc

- **Mô tả:** Biometric settings
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình chấm vân tay / sinh trắc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_099 — Cấu hình chấm APP điện thoại

- **Mô tả:** Mobile punch settings
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình chấm APP điện thoại” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_100 — Cấu hình chấm QR / mã nhân sự

- **Mô tả:** QR code punch
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình chấm QR / mã nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_101 — Đăng ký thiết bị chấm

- **Mô tả:** Time clock registration
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đăng ký thiết bị chấm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Time clock registration)
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
  - AC1: Thực hiện thành công thao tác “Đăng ký thiết bị chấm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_102 — Cấu hình geo-fence điểm chấm

- **Mô tả:** GPS-based attendance
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình geo-fence điểm chấm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_103 — Cấu hình quy tắc đi trễ

- **Mô tả:** Late arrival rules
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình quy tắc đi trễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_104 — Cấu hình mức trừ công khi trễ

- **Mô tả:** Late penalty rules
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình mức trừ công khi trễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_105 — Cấu hình quên check-out

- **Mô tả:** Missing checkout handling
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình quên check-out” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_106 — Cấu hình thời hạn xin điều chỉnh

- **Mô tả:** Adjustment request window
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thời hạn xin điều chỉnh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_107 — Cấu hình làm thêm giờ (OT)

- **Mô tả:** Overtime rules
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình làm thêm giờ (OT)” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_108 — Cấu hình ca đêm / ngày lễ

- **Mô tả:** Special shift rates
- **Tác nhân chính:** HR Admin
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình ca đêm / ngày lễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.12. Thực hiện chấm công (`HRM-12`)

Nhóm này gồm **11** chức năng. Tác nhân mặc định: **Employee / Line Manager**.

#### UC_HRM_109 — Check-in đầu ca

- **Mô tả:** Clock in
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Check-in đầu ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Clock in)
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
  - AC1: Thực hiện thành công thao tác “Check-in đầu ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_110 — Check-out cuối ca

- **Mô tả:** Clock out
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Check-out cuối ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Clock out)
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
  - AC1: Thực hiện thành công thao tác “Check-out cuối ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_111 — Xem lịch sử chấm cá nhân

- **Mô tả:** My attendance
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem lịch sử chấm cá nhân
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (My attendance)
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
  - AC1: Thực hiện thành công thao tác “Xem lịch sử chấm cá nhân” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_112 — Bảng chấm công theo đơn vị

- **Mô tả:** Unit timesheet
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng chấm công theo đơn vị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Unit timesheet)
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
  - AC1: Thực hiện thành công thao tác “Bảng chấm công theo đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_113 — Bảng chấm công toàn công ty

- **Mô tả:** Company timesheet
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Bảng chấm công toàn công ty
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Company timesheet)
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
  - AC1: Thực hiện thành công thao tác “Bảng chấm công toàn công ty” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_114 — Cảnh báo thiếu chấm realtime

- **Mô tả:** Missing punch alert
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cảnh báo thiếu chấm realtime
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Missing punch alert)
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
  - AC1: Thực hiện thành công thao tác “Cảnh báo thiếu chấm realtime” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_115 — Tự tính phút đi trễ

- **Mô tả:** Auto late calculation
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tự tính phút đi trễ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Auto late calculation)
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
  - AC1: Thực hiện thành công thao tác “Tự tính phút đi trễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_116 — Tự trừ công do đi trễ

- **Mô tả:** Auto attendance deduction
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tự trừ công do đi trễ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Auto attendance deduction)
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
  - AC1: Thực hiện thành công thao tác “Tự trừ công do đi trễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_117 — Đánh dấu quên chấm

- **Mô tả:** Missing punch flag
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đánh dấu quên chấm
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Missing punch flag)
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
  - AC1: Thực hiện thành công thao tác “Đánh dấu quên chấm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_118 — Đồng bộ dữ liệu từ máy chấm

- **Mô tả:** Time clock sync
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Kích hoạt đồng bộ thủ công hoặc theo sự kiện/job
  2. Hệ thống lấy dữ liệu nguồn và ánh xạ sang đích
  3. Ghi nhận kết quả/ thành công/ lỗi có thể retry
  4. Cập nhật trạng thái đồng bộ trên bản ghi liên quan
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Đồng bộ dữ liệu từ máy chấm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_119 — Xử lý công OT tự động

- **Mô tả:** OT auto recognition
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xử lý công OT tự động
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (OT auto recognition)
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
  - AC1: Thực hiện thành công thao tác “Xử lý công OT tự động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.13. Điều chỉnh & khóa công (`HRM-13`)

Nhóm này gồm **9** chức năng. Tác nhân mặc định: **HR Officer / Line Manager**.

#### UC_HRM_120 — Tạo phiếu xin điều chỉnh công

- **Mô tả:** Attendance adjustment request
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo phiếu xin điều chỉnh công” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_121 — Đính kèm lý do / bằng chứng

- **Mô tả:** Evidence attachment
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đính kèm lý do / bằng chứng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Evidence attachment)
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
  - AC1: Thực hiện thành công thao tác “Đính kèm lý do / bằng chứng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_122 — Duyệt / từ chối điều chỉnh

- **Mô tả:** Approve adjustment
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt / từ chối điều chỉnh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_123 — Ghi nhận vi phạm đi trễ

- **Mô tả:** Violation log
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận vi phạm đi trễ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Violation log)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận vi phạm đi trễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_124 — Lập bảng phạt

- **Mô tả:** Penalty calculation
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lập bảng phạt
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Penalty calculation)
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
  - AC1: Thực hiện thành công thao tác “Lập bảng phạt” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_125 — Áp dụng phạt vào kỳ lương

- **Mô tả:** Link penalty to payroll
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Áp dụng phạt vào kỳ lương
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Link penalty to payroll)
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
  - AC1: Thực hiện thành công thao tác “Áp dụng phạt vào kỳ lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_126 — Khóa bảng công theo kỳ

- **Mô tả:** Lock timesheet period
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khóa bảng công theo kỳ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Lock timesheet period)
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
  - AC1: Thực hiện thành công thao tác “Khóa bảng công theo kỳ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_127 — Mở khóa bảng công có kiểm soát

- **Mô tả:** Unlock with approval
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Mở khóa bảng công có kiểm soát
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Unlock with approval)
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
  - AC1: Thực hiện thành công thao tác “Mở khóa bảng công có kiểm soát” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_128 — Xác nhận bảng công

- **Mô tả:** Timesheet acknowledgment
- **Tác nhân chính:** HR Officer / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xác nhận bảng công
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Timesheet acknowledgment)
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
  - AC1: Thực hiện thành công thao tác “Xác nhận bảng công” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.14. Nghỉ phép & vắng mặt (`HRM-14`)

Nhóm này gồm **10** chức năng. Tác nhân mặc định: **Employee / Line Manager**.

#### UC_HRM_129 — Danh mục loại nghỉ

- **Mô tả:** Leave types
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục loại nghỉ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_130 — Cấu hình quỹ phép theo loại NS

- **Mô tả:** Leave balance rules
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình quỹ phép theo loại NS” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_131 — Cấp phát / điều chỉnh quỹ phép

- **Mô tả:** Grant leave days
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Cấp phát / điều chỉnh quỹ phép
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Grant leave days)
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
  - AC1: Thực hiện thành công thao tác “Cấp phát / điều chỉnh quỹ phép” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_132 — Tạo đơn xin nghỉ

- **Mô tả:** Leave request
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo đơn xin nghỉ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_133 — Duyệt đơn nghỉ đa cấp

- **Mô tả:** Leave approval workflow
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt đơn nghỉ đa cấp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_134 — Hủy đơn nghỉ

- **Mô tả:** Cancel leave
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Hủy đơn nghỉ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Cancel leave)
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
  - AC1: Thực hiện thành công thao tác “Hủy đơn nghỉ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_135 — Xem quỹ phép còn lại

- **Mô tả:** Leave balance view
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem quỹ phép còn lại
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Leave balance view)
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
  - AC1: Thực hiện thành công thao tác “Xem quỹ phép còn lại” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_136 — Lịch nghỉ theo đơn vị

- **Mô tả:** Team leave calendar
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Lịch nghỉ theo đơn vị
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Team leave calendar)
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
  - AC1: Thực hiện thành công thao tác “Lịch nghỉ theo đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_137 — Import nghỉ lễ / ngày nghỉ

- **Mô tả:** Holiday calendar
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Import nghỉ lễ / ngày nghỉ
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Holiday calendar)
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
  - AC1: Thực hiện thành công thao tác “Import nghỉ lễ / ngày nghỉ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_138 — Báo cáo nghỉ / quỹ phép

- **Mô tả:** Leave report
- **Tác nhân chính:** Employee / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo nghỉ / quỹ phép” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.15. Kỷ luật & khen thưởng (`HRM-15`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **HR Officer**.

#### UC_HRM_139 — Ghi nhận quyết định khen thưởng

- **Mô tả:** Reward record
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận quyết định khen thưởng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Reward record)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận quyết định khen thưởng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_140 — Ghi nhận quyết định kỷ luật

- **Mô tả:** Discipline record
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ghi nhận quyết định kỷ luật
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Discipline record)
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
  - AC1: Thực hiện thành công thao tác “Ghi nhận quyết định kỷ luật” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_141 — Đính kèm quyết định

- **Mô tả:** Decision document
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đính kèm quyết định
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Decision document)
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
  - AC1: Thực hiện thành công thao tác “Đính kèm quyết định” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_142 — Ảnh hưởng lương / phụ cấp

- **Mô tả:** Link to payroll
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Ảnh hưởng lương / phụ cấp
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Link to payroll)
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
  - AC1: Thực hiện thành công thao tác “Ảnh hưởng lương / phụ cấp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_143 — Báo cáo khen thưởng – kỷ luật

- **Mô tả:** Reward/discipline report
- **Tác nhân chính:** HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo khen thưởng – kỷ luật” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.16. Offboarding / nghỉ việc (`HRM-16`)

Nhóm này gồm **8** chức năng. Tác nhân mặc định: **HR Officer / Employee**.

#### UC_HRM_144 — Tạo đơn nghỉ việc

- **Mô tả:** Resignation form
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo đơn nghỉ việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_145 — Cấu hình / kiểm tra báo trước

- **Mô tả:** Notice period
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình / kiểm tra báo trước” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_146 — Duyệt đơn nghỉ việc

- **Mô tả:** Exit approval
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Duyệt đơn nghỉ việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_147 — Checklist bàn giao

- **Mô tả:** Handover checklist
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Checklist bàn giao
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Handover checklist)
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
  - AC1: Thực hiện thành công thao tác “Checklist bàn giao” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_148 — Thu hồi quyền hệ thống

- **Mô tả:** Disable system access
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Thu hồi quyền hệ thống
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Disable system access)
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
  - AC1: Thực hiện thành công thao tác “Thu hồi quyền hệ thống” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_149 — Quyết toán phép / lương nghỉ việc

- **Mô tả:** Final pay calculation
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quyết toán phép / lương nghỉ việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Final pay calculation)
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
  - AC1: Thực hiện thành công thao tác “Quyết toán phép / lương nghỉ việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_150 — Phỏng vấn nghỉ việc

- **Mô tả:** Exit interview
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phỏng vấn nghỉ việc
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Exit interview)
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
  - AC1: Thực hiện thành công thao tác “Phỏng vấn nghỉ việc” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_151 — Báo cáo nghỉ việc / lý do

- **Mô tả:** Turnover analysis
- **Tác nhân chính:** HR Officer / Employee
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo nghỉ việc / lý do” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.17. Cấu hình lương & phụ cấp (`HRM-17`)

Nhóm này gồm **11** chức năng. Tác nhân mặc định: **HR Admin / Payroll Accountant**.

#### UC_HRM_152 — Tạo thang bậc lương

- **Mô tả:** Salary grade structure
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo thang bậc lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_153 — Gán bậc lương theo nhân sự

- **Mô tả:** Salary assignment
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gán bậc lương theo nhân sự
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Salary assignment)
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
  - AC1: Thực hiện thành công thao tác “Gán bậc lương theo nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_154 — Gán bậc theo trạng thái

- **Mô tả:** Salary by status
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Gán bậc theo trạng thái
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Salary by status)
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
  - AC1: Thực hiện thành công thao tác “Gán bậc theo trạng thái” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_155 — Đơn giá giờ / ngày nhân viên

- **Mô tả:** Hourly/daily rate
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Đơn giá giờ / ngày nhân viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Hourly/daily rate)
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
  - AC1: Thực hiện thành công thao tác “Đơn giá giờ / ngày nhân viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_156 — Quản lý lương thực tế chi trả

- **Mô tả:** Actual pay rate
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý lương thực tế chi trả
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Actual pay rate)
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
  - AC1: Thực hiện thành công thao tác “Quản lý lương thực tế chi trả” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_157 — Danh mục phụ cấp

- **Mô tả:** Allowance master
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Danh mục phụ cấp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_158 — Rule phụ cấp theo ca

- **Mô tả:** Shift allowance rules
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Rule phụ cấp theo ca
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Shift allowance rules)
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
  - AC1: Thực hiện thành công thao tác “Rule phụ cấp theo ca” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_159 — Rule phụ cấp đặc thù

- **Mô tả:** Special allowance rules
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Rule phụ cấp đặc thù
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Special allowance rules)
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
  - AC1: Thực hiện thành công thao tác “Rule phụ cấp đặc thù” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_160 — Cấu hình bảo hiểm

- **Mô tả:** Insurance rates
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình bảo hiểm” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_161 — Cấu hình thuế TNCN

- **Mô tả:** Personal income tax
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình thuế TNCN” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_162 — Cấu hình tạm ứng / khấu trừ

- **Mô tả:** Deduction types
- **Tác nhân chính:** HR Admin / Payroll Accountant
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Cấu hình tạm ứng / khấu trừ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

### 7.18. Tính lương & chi trả (`HRM-18`)

Nhóm này gồm **14** chức năng. Tác nhân mặc định: **Payroll Accountant / HR Officer**.

#### UC_HRM_163 — Tạo kỳ lương

- **Mô tả:** Payroll period
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo kỳ lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_164 — Tổng hợp công vào kỳ lương

- **Mô tả:** Import attendance
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tổng hợp công vào kỳ lương
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Import attendance)
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
  - AC1: Thực hiện thành công thao tác “Tổng hợp công vào kỳ lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_165 — Tính lương tự động theo rule

- **Mô tả:** Payroll calculation engine
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tính lương tự động theo rule
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payroll calculation engine)
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
  - AC1: Thực hiện thành công thao tác “Tính lương tự động theo rule” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_166 — Nhập thưởng / phụ cấp phát sinh

- **Mô tả:** Manual additions
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhập thưởng / phụ cấp phát sinh
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Manual additions)
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
  - AC1: Thực hiện thành công thao tác “Nhập thưởng / phụ cấp phát sinh” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_167 — Nhập khấu trừ / tạm ứng

- **Mô tả:** Manual deductions
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhập khấu trừ / tạm ứng
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Manual deductions)
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
  - AC1: Thực hiện thành công thao tác “Nhập khấu trừ / tạm ứng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_168 — Xem / chỉnh bảng lương chi tiết

- **Mô tả:** Payslip review
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xem / chỉnh bảng lương chi tiết
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payslip review)
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
  - AC1: Thực hiện thành công thao tác “Xem / chỉnh bảng lương chi tiết” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_169 — Xác nhận bảng lương

- **Mô tả:** Payroll approval
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Xác nhận bảng lương
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Payroll approval)
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
  - AC1: Thực hiện thành công thao tác “Xác nhận bảng lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_170 — Khóa kỳ lương

- **Mô tả:** Lock payroll period
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Khóa kỳ lương
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Lock payroll period)
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
  - AC1: Thực hiện thành công thao tác “Khóa kỳ lương” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_171 — Phiếu lương cá nhân (APP)

- **Mô tả:** Employee payslip view
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Phiếu lương cá nhân (APP)
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Employee payslip view)
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
  - AC1: Thực hiện thành công thao tác “Phiếu lương cá nhân (APP)” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_172 — Xuất bảng lương tổng hợp

- **Mô tả:** Payroll summary export
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất bảng lương tổng hợp” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_173 — Xuất file chi lương ngân hàng

- **Mô tả:** Bank transfer file
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Xuất file chi lương ngân hàng” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_174 — Đồng bộ bút toán lương sang FIN

- **Mô tả:** GL posting
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Kích hoạt đồng bộ thủ công hoặc theo sự kiện/job
  2. Hệ thống lấy dữ liệu nguồn và ánh xạ sang đích
  3. Ghi nhận kết quả/ thành công/ lỗi có thể retry
  4. Cập nhật trạng thái đồng bộ trên bản ghi liên quan
- **Luồng thay thế / ngoại lệ:**
  - Thiếu quyền / ngoài data scope → từ chối + ghi audit.
  - Vi phạm validate / trùng khóa → báo lỗi field-level, không lưu.
  - Conflict trạng thái (đã khóa kỳ / đã hủy…) → chặn thao tác.
- **Hậu điều kiện:**
  - Dữ liệu được tạo/cập nhật nhất quán; có mã tham chiếu nếu là chứng từ.
  - Thông báo/sự kiện liên module được phát khi cấu hình có yêu cầu.
- **Quy tắc nghiệp vụ liên quan:** Áp dụng các BR tổng hợp ở Mục 10 và rule riêng của nhóm.
- **Tiêu chí chấp nhận (AC):**
  - AC1: Thực hiện thành công thao tác “Đồng bộ bút toán lương sang FIN” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_175 — Báo cáo chi phí lương theo đơn vị

- **Mô tả:** Labor cost by unit
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo chi phí lương theo đơn vị” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_176 — So sánh lương kỳ này / kỳ trước

- **Mô tả:** Period variance
- **Tác nhân chính:** Payroll Accountant / HR Officer
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: So sánh lương kỳ này / kỳ trước
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Period variance)
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
  - AC1: Thực hiện thành công thao tác “So sánh lương kỳ này / kỳ trước” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.19. Đánh giá hiệu suất (`HRM-19`)

Nhóm này gồm **5** chức năng. Tác nhân mặc định: **Line Manager / HR Officer**.

#### UC_HRM_177 — Mẫu đánh giá KPI / năng lực

- **Mô tả:** Performance review template
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Mẫu đánh giá KPI / năng lực
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Performance review template)
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
  - AC1: Thực hiện thành công thao tác “Mẫu đánh giá KPI / năng lực” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_178 — Tạo kỳ đánh giá

- **Mô tả:** Review cycle
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Tạo kỳ đánh giá” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_179 — Quản lý đánh giá nhân viên

- **Mô tả:** Manager review
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Quản lý đánh giá nhân viên
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Manager review)
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
  - AC1: Thực hiện thành công thao tác “Quản lý đánh giá nhân viên” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_180 — Nhân viên tự đánh giá

- **Mô tả:** Self assessment
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Thấp → **MoSCoW:** Won't / Later
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Nhân viên tự đánh giá
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Self assessment)
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
  - AC1: Thực hiện thành công thao tác “Nhân viên tự đánh giá” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_181 — Tổng hợp kết quả đánh giá

- **Mô tả:** Review results
- **Tác nhân chính:** Line Manager / HR Officer
- **Ưu tiên danh mục:** Trung bình → **MoSCoW:** Could
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
  - Dữ liệu tham chiếu liên quan tồn tại và thuộc data scope của user.
- **Luồng chính:**
  1. Người dùng khởi tạo thao tác: Tổng hợp kết quả đánh giá
  2. Hệ thống kiểm tra quyền, license module và tiền điều kiện (Review results)
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
  - AC1: Thực hiện thành công thao tác “Tổng hợp kết quả đánh giá” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

### 7.20. Báo cáo & dashboard HRM (`HRM-20`)

Nhóm này gồm **6** chức năng. Tác nhân mặc định: **HR Admin / Line Manager**.

#### UC_HRM_182 — Dashboard headcount & biến động

- **Mô tả:** HR overview dashboard
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Dashboard headcount & biến động” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_183 — Báo cáo công / OT / đi trễ

- **Mô tả:** Attendance KPIs
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo công / OT / đi trễ” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_184 — Báo cáo tuyển dụng funnel

- **Mô tả:** Recruitment funnel
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo tuyển dụng funnel” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_185 — Báo cáo quỹ phép

- **Mô tả:** Leave balance report
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo quỹ phép” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

#### UC_HRM_186 — Báo cáo chi phí nhân sự

- **Mô tả:** HR cost dashboard
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Bắt buộc → **MoSCoW:** Must
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo chi phí nhân sự” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).
  - AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module.

#### UC_HRM_187 — Báo cáo định biên vs thực tế

- **Mô tả:** Headcount gap report
- **Tác nhân chính:** HR Admin / Line Manager
- **Ưu tiên danh mục:** Cao → **MoSCoW:** Should
- **Tiền điều kiện:**
  - User đã đăng nhập và có permission tương ứng trong `HRM`.
  - License module `HRM` đang hiệu lực (trừ khi là chức năng nền SYS).
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
  - AC1: Thực hiện thành công thao tác “Báo cáo định biên vs thực tế” với dữ liệu hợp lệ.
  - AC2: User không đủ quyền không thể thực hiện được (UI ẩn/disabled hoặc API 403).
  - AC3: Có thể truy vết thao tác trên audit log (với nhóm thay đổi dữ liệu).

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

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

### WF-HRM-02 — Xếp ca → Chấm công → Khóa công

**Mục tiêu:** Có bảng công kỳ sạch để tính lương

| Bước | Mô tả |
|---:|---|
| 1 | Khai báo mẫu ca và định biên (nếu dùng) |
| 2 | Xếp lịch ca; nhân viên xem trên app/web |
| 3 | Chấm công theo phương thức cấu hình |
| 4 | Xin–duyệt điều chỉnh công khi sai/thiếu |
| 5 | Khóa bảng công kỳ; mở khóa có kiểm soát nếu cần |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

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

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

### WF-HRM-04 — Nghỉ việc / Offboarding

**Mục tiêu:** Chấm dứt quan hệ LĐ có bàn giao và quyết toán

| Bước | Mô tả |
|---:|---|
| 1 | Nhân viên/HR tạo đơn nghỉ việc |
| 2 | Duyệt theo workflow; kiểm tra báo trước |
| 3 | Checklist bàn giao quyền/hệ thống/tài sản |
| 4 | Quyết toán phép – lương cuối |
| 5 | Khóa hồ sơ; thu hồi truy cập SYS |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái nghiệp vụ cuối nhất quán; có audit/trace.

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

### 9.1. Xuất xứ & kiểm soát dữ liệu
- Master dùng chung (KH, SP, chi nhánh…) tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ nghiệp vụ có trạng thái vòng đời rõ ràng (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete hoặc trạng thái ngưng dùng là mặc định; hạn chế xóa cứng.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-HRM-01: Mã nhân sự duy nhất trong tenant, không tái sử dụng.
- BR-HRM-02: Chỉ tính lương từ bảng công đã khóa (trừ điều chỉnh được duyệt).
- BR-HRM-03: Quỹ phép không âm trừ khi policy cho phép ứng phép.
- BR-HRM-04: Nhân sự nghỉ việc phải hoàn tất checklist offboarding trước khi khóa hồ sơ.
- BR-HRM-05: Dữ liệu lương là dữ liệu nhạy cảm — mask theo permission field-level.
- BR-HRM-06: Mọi điều chỉnh công/lương sau khóa kỳ phải có lý do + người duyệt.
- BR-HRM-GEN-01: Mọi thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-HRM-GEN-02: Mọi chứng từ có mã duy nhất theo rule Sequence của SYS.
- BR-HRM-GEN-03: Thao tác sau khi khóa kỳ/chốt sổ (nếu có) phải đi đường điều chỉnh có kiểm soát.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Bảo mật | Mã hóa dữ liệu nhạy cảm; phân quyền field-level cho lương/CCCD |
| Hiệu năng | Bảng công 5.000 NV khóa kỳ < 5 phút (batch) |
| Mobile | Employee self-service dùng được trên mobile web/app |
| Tuân thủ | Lưu hồ sơ/HĐ theo thời hạn pháp lý cấu hình được |
| Usability | Form có validate rõ; bảng có lọc/phân trang; hỗ trợ tiếng Việt |
| Reliability | Không mất chứng từ đã post; giao dịch quan trọng atomic |
| Maintainability | Permission và cấu hình không hard-code trong source nghiệp vụ |
| Observability | Có log ứng dụng + audit nghiệp vụ tách bạch |

---

## 12. Tích hợp & sự kiện

### 12.1. Ma trận tích hợp

| Thành phần | Mô tả |
|---|---|
| SYS | User lifecycle, org master, workflow duyệt, file, thông báo |
| WF | Duyệt nghỉ / điều chỉnh công / đề xuất tuyển / kỳ lương |
| LMS | Đồng bộ chứng chỉ đào tạo bắt buộc vào hồ sơ |
| FIN | Post chi phí lương, tạm ứng, khấu trừ |
| AST | Checklist thu hồi tài sản khi nghỉ việc |
| Device | Máy chấm công / geo-fence app |

### 12.2. Sự kiện (logical)
- `HRM.EntityCreated` / `HRM.EntityUpdated` / `HRM.EntityStatusChanged`
- `HRM.DocumentSubmitted` / `HRM.DocumentApproved` / `HRM.DocumentPosted`
- Mapping cụ thể API/topic sẽ định nghĩa ở tài liệu Interface Spec sau khi chốt SRS.

---

## 13. Phân quyền & bảo mật

### 13.1. Permission catalog (đề xuất)

- `hrm.employee.manage`
- `hrm.employee.view`
- `hrm.contract.manage`
- `hrm.recruitment.manage`
- `hrm.attendance.manage`
- `hrm.attendance.approve`
- `hrm.leave.approve`
- `hrm.payroll.run`
- `hrm.payroll.view_all`
- `hrm.payslip.view_self`
- `hrm.report.view`

### 13.2. Nguyên tắc
- Deny by default; chỉ mở theo role.
- Data scope theo chi nhánh/kho/đơn vị do SYS quyết định.
- Field-level security cho dữ liệu nhạy cảm (lương, công nợ chi tiết, giá vốn…) khi áp dụng.
- Mọi thay đổi phân quyền và thao tác critical ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| Headcount theo đơn vị/trạng thái | Giám sát vận hành module `HRM` |
| Tỷ lệ đi trễ / quên chấm | Giám sát vận hành module `HRM` |
| Thời gian fill vị trí tuyển dụng | Giám sát vận hành module `HRM` |
| Chi phí lương / doanh thu (khi có FIN) | Giám sát vận hành module `HRM` |
| Tỷ lệ nghỉ việc (turnover) | Giám sát vận hành module `HRM` |

Báo cáo chi tiết vận hành nằm trong từng nhóm “Báo cáo…” của Mục 7; tổng hợp điều hành nằm trên module `BI` khi khách mua thêm.

---

## 15. Giả định, rủi ro & câu hỏi mở

### 15.1. Giả định
- Chính sách lương/phụ cấp cấu hình được theo tenant, không hard-code ngành.
- Có thể dùng HRM độc lập với LMS; chứng chỉ thủ công nếu chưa mua LMS.

### 15.2. Câu hỏi mở cần chốt
- Phase 1 có gồm đánh giá hiệu suất đầy đủ hay chỉ checklist cơ bản?
- Phạm vi tích hợp máy chấm công cụ thể theo hãng nào ở triển khai?

### 15.3. Rủi ro
- Phụ thuộc module khác chưa mua → một số workflow E2E chỉ chạy được một phần (cần nêu rõ khi bán gói).
- Cấu hình quá linh hoạt có thể làm tăng effort QA; cần bộ template mặc định.
- Chưa chốt chuẩn kế toán/thuế chi tiết có thể ảnh hưởng FIN và posting.

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện nghiệm thu module
1. 100% UC ưu tiên **Bắt buộc (Must)** của `HRM` pass UAT.
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
| Use case IDs | `UC_HRM_001` … `UC_HRM_187` |

---

*Hết tài liệu SRS-HRM-v1.0.*
