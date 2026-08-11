# Kế hoạch Chi tiết Triển khai & Polish 1.092 UC (Phân dòng chi tiết từng UC)

| Thuộc tính        | Giá trị                                                                            |
| ----------------- | ---------------------------------------------------------------------------------- |
| Mã tài liệu       | `KE_HOACH_TRIEN_KHAI_UC_V2_CHI_TIET`                                               |
| Cập nhật          | 11/08/2026                                                                         |
| Quy tắc trình bày | **Mỗi UC 1 dòng riêng trong bảng**, đường gạch ngang `---` phân chia giữa các bước |
| Tổng số UC        | **1.092 UC** (206 đã xong 100%, 540 UC dở dang cần polish, 346 UC chưa làm 0%)     |

---

## 📌 QUY ĐỊNH & TIÊU CHUẨN HOÀN THÀNH 100% PRODUCTION-READY

1. **Khối lượng thực hiện:** Mỗi bước chỉ làm từ **1 đến 4 UC** để đảm bảo chất lượng tuyệt đối, xử lý chỉn chu đến từng chi tiết nhỏ nhất.
2. **Chi tiết Edge Case & Luồng ngoại lệ:** Xử lý 100% các luồng ngoại lệ (exception flows), validation logic, chống trùng lặp, khóa trạng thái, outbox event, phân quyền data scope.
3. **Tiêu chuẩn Backend 100% (BE):**
   - **Data Model:** Entity định nghĩa chuẩn trong `Erp.Domain`, `DbSet` trong `AppDbContext.cs`, migration DB khi thay đổi schema.
   - **Service Layer:** Interface trong `Erp.Application` & implementation trong `Erp.Infrastructure`, giải quyết 100% nghiệp vụ.
   - **API Controller:** RESTful endpoints (`GET`, `POST`, `PUT`, `DELETE`) chuẩn hóa DTOs & phân quyền `@AuthorizePermission(...)`.
   - **Unit Tests BE:** Đạt **5 – 20 test cases** chạy thực tế trên `InMemoryDatabase` (`*PolishTests.cs`), bao phủ luồng thành công và luồng lỗi.
4. **Tiêu chuẩn Frontend 100% (FE):**
   - **UI Real-world (`page.tsx`):** Giao diện Next.js App Router hoàn chỉnh (Form, Table, SideSheet, Modal, Filter/Search), **loại bỏ 100% data giả/hard-code stub/`alert('stub')`**.
   - **API Wiring:** Kết nối API thực tế 100% qua Axios clients (`sys-api.ts`, `crm-api.ts`, ...).
   - **UX Complete:** Xử lý Loading state, Toast alert notification, Form error validation, Pagination & Lọc đa tiêu chí.
   - **Unit Tests FE:** Đạt **5 – 20 test cases** chạy trên Node test runner (`*.node-test.mts`) cho helper / validator.
5. **Quy tắc Cập nhật Tiến độ & Format:**
   - **Không tự dùng script Python sinh đè file `.md`.** Sửa ký tự trực tiếp.
   - **Bảo toàn 100% Format bảng:** Giữ nguyên khung bảng, độ rộng cột và alignment của `KE_HOACH_TRIEN_KHAI_UC.md` do Người dùng thiết lập. Cập nhật tỷ lệ `%` (`90% ➔ 100%`) và thêm ký hiệu `[XONG]`.
   - **Cập nhật đồng bộ:** Cập nhật trực tiếp `CHECKLIST_UC.md`, `uc_progress.json` và `KE_HOACH_TRIEN_KHAI_UC.md` sau khi hoàn thành từng bước.
6. **Đồng bộ Thực tế & Tài liệu (Bắt buộc tuyệt đối):**
   - **Cập nhật tức thì:** Ngay sau khi hoàn thành bất kỳ bước nào, UC nào hay nhiệm vụ/chức năng nào, BẮT BUỘC phải cập nhật lại ngay lập tức các file `.md` trong thư mục `Timeline`.
   - **Không lệch thực tế:** KHÔNG BAO GIỜ để xảy ra trường hợp file tài liệu `.md` bị sai lệch hay chậm trễ so với thực tế triển khai trong codebase.
7. **Phân tích Trường hợp Đặc biệt trước khi Code (Bắt buộc):**
   - **Phân tích tiền triển khai:** Trước khi bắt tay vào làm bất kỳ UC nào, BẮT BUỘC phải liệt kê và phân tích tất cả các trường hợp đặc biệt (edge cases), luồng lỗi (exceptions), chống trùng lặp, khóa trạng thái, ràng buộc dữ liệu & phân quyền.
   - **Bao phủ toàn diện:** Sau khi phân tích mới tiến hành viết code BE/FE và bổ sung unit tests để đảm bảo code bao phủ 100% mọi kịch bản góc hẹp.
8. **Tiêu chuẩn Code Chỉnh chu & Chất lượng Cao:**
   - **Cấm code qua loa/sơ sài:** Tuyệt đối KHÔNG được viết code sơ sài, đối phó hay hời hợt. Code phải sạch sẽ, chặt chẽ, tối ưu và chỉn chu đến từng dòng.
   - **Không để nợ kỹ thuật:** Mọi xử lý DTO, Controller, Service, Validator và Test cases đều phải đạt chất lượng Production-Ready chuẩn mực.
9. **Quy tắc Báo cáo Sau khi Hoàn thành Mỗi Bước (Bắt buộc tuyệt đối):**
   - **Phân chia 3 phần rõ ràng:** Mỗi khi hoàn thành một bước/công việc, BẮT BUỘC phải báo cáo kết quả tổng kết phân chia làm 3 mục chi tiết:
     - **BE (Backend) làm gì:** Liệt kê chi tiết các Services, Entities, Controllers, API Endpoints, Validation Rules và Exception handling đã triển khai/cập nhật.
     - **FE (Frontend) làm gì:** Liệt kê chi tiết Giao diện UI (pages/components), Helper Modules, Form validation và API Integrations.
     - **Test (Kiểm thử) làm gì:** Báo cáo số lượng và danh sách Test Cases Backend (.NET xUnit) & Frontend (Node.js test runner), cùng kết quả thực thi PASSED/FAILED.

---

## 🗺️ BẢNG CHI TIẾT CÁC BƯỚC TRIỂN KHAI & POLISH SÂU (CÓ ĐƯỜNG GẠCH PHÂN CHIA BƯỚC)

|     Bước     | Mã UC        | Tên UC & Nội dung công việc                                                 |  Tiến độ   |
| :----------: | :----------- | :-------------------------------------------------------------------------- | :--------: |
|  **Bước 1**  | `UC_SYS_004` | Quên MK: IntegrationCallLog SMS/Email OTP & Outbox                          |    100%    |
|   `[XONG]`   | `UC_SYS_008` | 2FA: TOTP QR code & Backup recovery codes                                   |    100%    |
|              | `UC_SYS_010` | Quản lý phiên: Websocket/SignalR real-time session kill                     |    100%    |
|              | `UC_SYS_011` | Giới hạn phiên: Max 5 sessions & auto-revoke oldest                         |    100%    |
|              | `UC_SYS_012` | Ghi nhớ thiết bị: Trusted Device fingerprinting 30d                         |    100%    |
|              | `UC_HRM_118` | Chấm công SDK: In/Out AttendanceRecord deduplication                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 2**  | `UC_SYS_005` | Reset pass OTP: Validate token, password policy & clear lock                |    100%    |
|   `[XONG]`   | `UC_SYS_013` | Tạo user: OrgUnit primary, License limit & multi-dept                       |    100%    |
|              | `UC_SYS_014` | Cập nhật user: Sync profile, email/phone & departments                      |    100%    |
|              | `UC_SYS_015` | Khóa/Mở khóa: Switch status Locked <-> Active                               |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 3**  | `UC_SYS_017` | Gán người dùng vào chi nhánh — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_SYS_019` | Mời người dùng qua email — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_SYS_021` | Tìm kiếm / lọc người dùng — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_SYS_023` | Tạo / sửa / ngưng vai trò (Role) — Polish BE/FE & Unit Test                 |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 4**  | `UC_SYS_028` | Phân quyền dữ liệu theo chi nhánh — Polish BE/FE & Unit Test                |    100%    |
|   `[XONG]`   | `UC_SYS_029` | Phân quyền dữ liệu theo kho / điểm — Polish BE/FE & Unit Test               |    100%    |
|              | `UC_SYS_030` | Phân quyền theo phòng ban — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_SYS_036` | Quản lý chi nhánh — Polish BE/FE & Unit Test                                |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 5**  | `UC_SYS_038` | Quản lý phòng ban — Polish BE/FE & Unit Test                                |    100%    |
|   `[XONG]`   | `UC_SYS_039` | Quản lý chức danh — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_SYS_040` | Sơ đồ tổ chức trực quan — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_SYS_042` | Cấu hình định dạng ngày số — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 6**  | `UC_SYS_052` | Cấu hình theo chi nhánh — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_SYS_059` | Thông báo in-app — Polish BE/FE & Unit Test                                 |    100%    |
|              | `UC_SYS_060` | Gửi email hệ thống — Polish BE/FE & Unit Test                               |    100%    |
|              | `UC_SYS_061` | Gửi SMS / messaging — Polish BE/FE & Unit Test                              |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 7**  | `UC_SYS_063` | Cấu hình sự kiện kích hoạt — Polish BE/FE & Unit Test                       |    100%    |
|   `[XONG]`   | `UC_SYS_065` | Nhật ký gửi thông báo — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_SYS_066` | Upload file — Polish BE/FE & Unit Test                                      |    100%    |
|              | `UC_SYS_067` | Tải xuống / xem trước file — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 8**  | `UC_SYS_069` | Phân quyền file theo đối tượng — Polish BE/FE & Unit Test                   |    100%    |
|   `[XONG]`   | `UC_SYS_074` | Export Excel — Polish BE/FE & Unit Test                                     |    100%    |
|              | `UC_SYS_075` | Export PDF — Polish BE/FE & Unit Test                                       |    100%    |
|              | `UC_SYS_076` | Lịch sử job import/export — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
|  **Bước 9**  | `UC_SYS_078` | Nhật ký thao tác người dùng — Polish BE/FE & Unit Test                      |    100%    |
|   `[XONG]`   | `UC_SYS_080` | Xem chi tiết thay đổi field — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_SYS_081` | Xuất audit log — Polish BE/FE & Unit Test                                   |    100%    |
|              | `UC_SYS_083` | Chính sách hết hạn phiên — Polish BE/FE & Unit Test                         |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 10**  | `UC_SYS_087` | Hàng đợi sự kiện liên module — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_SYS_088` | Kết nối email gateway — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_SYS_089` | Kết nối SMS gateway — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_SYS_101` | Đính kèm file trong tin nhắn — Polish BE/FE & Unit Test                     |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 11**  | `UC_HRM_001` | Tạo sơ đồ tổ chức công ty — Polish BE/FE & Unit Test                        |    100%    |
|   `[XONG]`   | `UC_HRM_002` | Quản lý khối vận hành — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_HRM_003` | Quản lý khối sản xuất — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_HRM_004` | Quản lý danh mục điểm bán — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 12**  | `UC_HRM_006` | Khai báo giờ làm việc theo đơn vị — Polish BE/FE & Unit Test                |    100%    |
|   `[XONG]`   | `UC_HRM_010` | Quản lý cấp bậc / level — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_012` | Sinh mã nhân sự tự động — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_017` | Upload giấy tờ tùy thân — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 13**  | `UC_HRM_026` | Xuất danh sách nhân sự Excel — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_HRM_027` | Khóa hồ sơ đã nghỉ — Polish BE/FE & Unit Test                               |    100%    |
|              | `UC_HRM_028` | Xem hồ sơ theo quyền — Polish BE/FE & Unit Test                             |    100%    |
|              | `UC_HRM_029` | Chuyển trạng thái Thử việc — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 14**  | `UC_HRM_032` | Chuyển trạng thái Nghỉ việc — Polish BE/FE & Unit Test                      |    100%    |
|   `[XONG]`   | `UC_HRM_033` | Lịch sử thay đổi trạng thái — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_HRM_034` | Điều chuyển đơn vị / bộ phận — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_036` | Cảnh báo sắp hết hạn thử việc — Polish BE/FE & Unit Test                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 15**  | `UC_HRM_038` | Tạo hợp đồng lao động — Polish BE/FE & Unit Test                            |    100%    |
|   `[XONG]`   | `UC_HRM_039` | Tạo phụ lục hợp đồng — Polish BE/FE & Unit Test                             |    100%    |
|              | `UC_HRM_043` | Cảnh báo hết hạn hợp đồng — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_HRM_046` | Lịch sử hợp đồng theo nhân sự — Polish BE/FE & Unit Test                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 16**  | `UC_HRM_047` | Tạo phiếu đề xuất tuyển dụng — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_HRM_048` | Chọn vị trí & số lượng cần tuyển — Polish BE/FE & Unit Test                 |    100%    |
|              | `UC_HRM_049` | Nhập lý do tuyển dụng — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_HRM_050` | Gửi phiếu đề xuất đi duyệt — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 17**  | `UC_HRM_051` | Duyệt / từ chối đề xuất — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_HRM_052` | Xem lịch sử duyệt đề xuất — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_HRM_053` | Đóng / hủy phiếu đề xuất — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_HRM_054` | Tạo tin tuyển từ phiếu đã duyệt — Polish BE/FE & Unit Test                  |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 18**  | `UC_HRM_055` | Ghi nhận kênh đăng tuyển — Polish BE/FE & Unit Test                         |    100%    |
|   `[XONG]`   | `UC_HRM_056` | Nhập hồ sơ ứng viên — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_057` | Upload file CV — Polish BE/FE & Unit Test                                   |    100%    |
|              | `UC_HRM_059` | Sơ loại ứng viên — Polish BE/FE & Unit Test                                 |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 19**  | `UC_HRM_060` | Chuyển ứng viên cho đơn vị đánh giá — Polish BE/FE & Unit Test              |    100%    |
|   `[XONG]`   | `UC_HRM_061` | Form đánh giá ứng viên — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_HRM_062` | Từ chối / chấp nhận ứng viên — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_063` | Pipeline trạng thái ứng viên — Polish BE/FE & Unit Test                     |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 20**  | `UC_HRM_064` | Lịch sử chăm sóc ứng viên — Polish BE/FE & Unit Test                        |    100%    |
|   `[XONG]`   | `UC_HRM_065` | Báo cáo hiệu quả kênh tuyển — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_HRM_066` | Cấu hình thời hạn onboarding — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_067` | Cấu hình thời hạn thử việc — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 21**  | `UC_HRM_068` | Tạo hồ sơ nhân viên mới từ ứng viên — Polish BE/FE & Unit Test              |    100%    |
|   `[XONG]`   | `UC_HRM_069` | Gán người hướng dẫn — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_070` | Checklist onboarding — Polish BE/FE & Unit Test                             |    100%    |
|              | `UC_HRM_071` | Upload chứng chỉ / giấy tờ — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 22**  | `UC_HRM_072` | Đánh giá kết thúc thử việc — Polish BE/FE & Unit Test                       |    100%    |
|   `[XONG]`   | `UC_HRM_073` | Chuyển thử việc thành chính thức — Polish BE/FE & Unit Test                 |    100%    |
|              | `UC_HRM_074` | Cảnh báo hết hạn thử việc — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_HRM_075` | Khai báo định biên theo đơn vị — Polish BE/FE & Unit Test                   |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 23**  | `UC_HRM_076` | Khai báo định biên theo ca — Polish BE/FE & Unit Test                       |    100%    |
|   `[XONG]`   | `UC_HRM_077` | Khai báo định biên theo bộ phận — Polish BE/FE & Unit Test                  |    100%    |
|              | `UC_HRM_078` | So sánh thực tế vs định biên — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_079` | Cảnh báo thiếu người — Polish BE/FE & Unit Test                             |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 24**  | `UC_HRM_080` | Duyệt thay đổi định biên — Polish BE/FE & Unit Test                         |    100%    |
|   `[XONG]`   | `UC_HRM_081` | Tạo mẫu ca làm việc — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_082` | Xếp lịch ca nhân viên — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_HRM_083` | Xếp lịch ca theo tuần / tháng — Polish BE/FE & Unit Test                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 25**  | `UC_HRM_084` | Đổi ca giữa nhân viên — Polish BE/FE & Unit Test                            |    100%    |
|   `[XONG]`   | `UC_HRM_085` | Hủy lịch ca — Polish BE/FE & Unit Test                                      |    100%    |
|              | `UC_HRM_086` | Xem lịch ca theo đơn vị — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_087` | Xem lịch ca cá nhân trên APP — Polish BE/FE & Unit Test                     |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 26**  | `UC_HRM_089` | Sao chép lịch ca — Polish BE/FE & Unit Test                                 |    100%    |
|   `[XONG]`   | `UC_HRM_090` | Khóa sổ lịch ca theo kỳ — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_091` | In / xuất lịch ca — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_HRM_092` | Tạo lệnh điều động — Polish BE/FE & Unit Test                               |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 27**  | `UC_HRM_093` | Đề xuất nhu cầu điều động — Polish BE/FE & Unit Test                        |    100%    |
|   `[XONG]`   | `UC_HRM_094` | Nhận lệnh điều động trên APP — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_095` | Theo dõi nhân sự điều động — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_HRM_096` | Gắn nhãn công điều động khi chấm — Polish BE/FE & Unit Test                 |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 28**  | `UC_HRM_097` | Báo cáo giờ / chi phí điều động — Polish BE/FE & Unit Test                  |    100%    |
|   `[XONG]`   | `UC_HRM_098` | Cấu hình chấm vân tay / sinh trắc — Polish BE/FE & Unit Test                |    100%    |
|              | `UC_HRM_099` | Cấu hình chấm APP điện thoại — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_100` | Cấu hình chấm QR / mã nhân sự — Polish BE/FE & Unit Test                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 29**  | `UC_HRM_101` | Đăng ký thiết bị chấm — Polish BE/FE & Unit Test                            |    100%    |
|   `[XONG]`   | `UC_HRM_102` | Cấu hình geo-fence điểm chấm — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_103` | Cấu hình quy tắc đi trễ — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_104` | Cấu hình mức trừ công khi trễ — Polish BE/FE & Unit Test                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 30**  | `UC_HRM_105` | Cấu hình quên check-out — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_HRM_106` | Cấu hình thời hạn xin điều chỉnh — Polish BE/FE & Unit Test                 |    100%    |
|              | `UC_HRM_107` | Cấu hình làm thêm giờ (OT) — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_HRM_108` | Cấu hình ca đêm / ngày lễ — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 31**  | `UC_HRM_109` | Check-in đầu ca — Polish BE/FE & Unit Test                                  |    100%    |
|   `[XONG]`   | `UC_HRM_110` | Check-out cuối ca — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_HRM_111` | Xem lịch sử chấm cá nhân — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_HRM_112` | Bảng chấm công theo đơn vị — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 32**  | `UC_HRM_113` | Bảng chấm công toàn công ty — Polish BE/FE & Unit Test                      |    100%    |
|   `[XONG]`   | `UC_HRM_114` | Cảnh báo thiếu chấm realtime — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_115` | Tự tính phút đi trễ — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_116` | Tự trừ công do đi trễ — Polish BE/FE & Unit Test                            |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 33**  | `UC_HRM_117` | Đánh dấu quên chấm — Polish BE/FE & Unit Test                               |    100%    |
|   `[XONG]`   | `UC_HRM_119` | Xử lý công OT tự động — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_HRM_120` | Tạo phiếu xin điều chỉnh công — Polish BE/FE & Unit Test                    |    100%    |
|              | `UC_HRM_121` | Đính kèm lý do / bằng chứng — Polish BE/FE & Unit Test                      |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 34**  | `UC_HRM_122` | Duyệt / từ chối điều chỉnh — Polish BE/FE & Unit Test                       |    100%    |
|   `[XONG]`   | `UC_HRM_123` | Ghi nhận vi phạm đi trễ — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_126` | Khóa bảng công theo kỳ — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_HRM_127` | Mở khóa bảng công có kiểm soát — Polish BE/FE & Unit Test                   |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 35**  | `UC_HRM_128` | Xác nhận bảng công — Polish BE/FE & Unit Test                               |    100%    |
|   `[XONG]`   | `UC_HRM_130` | Cấu hình quỹ phép theo loại NS — Polish BE/FE & Unit Test                   |    100%    |
|              | `UC_HRM_131` | Cấp phát / điều chỉnh quỹ phép — Polish BE/FE & Unit Test                   |    100%    |
|              | `UC_HRM_133` | Duyệt đơn nghỉ đa cấp — Polish BE/FE & Unit Test                            |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 36**  | `UC_HRM_134` | Hủy đơn nghỉ — Polish BE/FE & Unit Test                                     |    100%    |
|   `[XONG]`   | `UC_HRM_136` | Lịch nghỉ theo đơn vị — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_HRM_137` | Import nghỉ lễ / ngày nghỉ — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_HRM_138` | Báo cáo nghỉ / quỹ phép — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 37**  | `UC_HRM_139` | Ghi nhận quyết định khen thưởng — Polish BE/FE & Unit Test                  |    100%    |
|   `[XONG]`   | `UC_HRM_140` | Ghi nhận quyết định kỷ luật — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_HRM_141` | Đính kèm quyết định — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_142` | Ảnh hưởng lương / phụ cấp — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 38**  | `UC_HRM_143` | Báo cáo khen thưởng – kỷ luật — Polish BE/FE & Unit Test                    |    100%    |
|   `[XONG]`   | `UC_HRM_144` | Tạo đơn nghỉ việc — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_HRM_145` | Cấu hình / kiểm tra báo trước — Polish BE/FE & Unit Test                    |    100%    |
|              | `UC_HRM_146` | Duyệt đơn nghỉ việc — Polish BE/FE & Unit Test                              |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 39**  | `UC_HRM_147` | Checklist bàn giao — Polish BE/FE & Unit Test                               |    100%    |
|   `[XONG]`   | `UC_HRM_148` | Thu hồi quyền hệ thống — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_HRM_149` | Quyết toán phép / lương nghỉ việc — Polish BE/FE & Unit Test                |    100%    |
|              | `UC_HRM_150` | Phỏng vấn nghỉ việc — Polish BE/FE & Unit Test                              |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 40**  | `UC_HRM_151` | Báo cáo nghỉ việc / lý do — Polish BE/FE & Unit Test                        |    100%    |
|   `[XONG]`   | `UC_HRM_152` | Tạo thang bậc lương — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_153` | Gán bậc lương theo nhân sự — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_HRM_154` | Gán bậc theo trạng thái — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 41**  | `UC_HRM_155` | Đơn giá giờ / ngày nhân viên — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_HRM_156` | Quản lý lương thực tế chi trả — Polish BE/FE & Unit Test                    |    100%    |
|              | `UC_HRM_157` | Danh mục phụ cấp — Polish BE/FE & Unit Test                                 |    100%    |
|              | `UC_HRM_158` | Rule phụ cấp theo ca — Polish BE/FE & Unit Test                             |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 42**  | `UC_HRM_159` | Rule phụ cấp đặc thù — Polish BE/FE & Unit Test                             |    100%    |
|   `[XONG]`   | `UC_HRM_160` | Cấu hình bảo hiểm — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_HRM_161` | Cấu hình thuế TNCN — Polish BE/FE & Unit Test                               |    100%    |
|              | `UC_HRM_162` | Cấu hình tạm ứng / khấu trừ — Polish BE/FE & Unit Test                      |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 43**  | `UC_HRM_163` | Tạo kỳ lương — Polish BE/FE & Unit Test                                     |    100%    |
|   `[XONG]`   | `UC_HRM_164` | Tổng hợp công vào kỳ lương — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_HRM_165` | Tính lương tự động theo rule — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_HRM_166` | Nhập thưởng / phụ cấp phát sinh — Polish BE/FE & Unit Test                  |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 44**  | `UC_HRM_167` | Nhập khấu trừ / tạm ứng — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_HRM_168` | Xem / chỉnh bảng lương chi tiết — Polish BE/FE & Unit Test                  |    100%    |
|              | `UC_HRM_169` | Xác nhận bảng lương — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_HRM_170` | Khóa kỳ lương — Polish BE/FE & Unit Test                                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 45**  | `UC_HRM_171` | Phiếu lương cá nhân (APP) — Polish BE/FE & Unit Test                        |    100%    |
|   `[XONG]`   | `UC_HRM_172` | Xuất bảng lương tổng hợp — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_HRM_173` | Xuất file chi lương ngân hàng — Polish BE/FE & Unit Test                    |    100%    |
|              | `UC_HRM_175` | Báo cáo chi phí lương theo đơn vị — Polish BE/FE & Unit Test                |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 46**  | `UC_HRM_176` | So sánh lương kỳ này / kỳ trước — Polish BE/FE & Unit Test                  |    100%    |
|   `[XONG]`   | `UC_HRM_182` | Dashboard headcount & biến động — Polish BE/FE & Unit Test                  |    100%    |
|              | `UC_HRM_183` | Báo cáo công / OT / đi trễ — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_HRM_184` | Báo cáo tuyển dụng funnel — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 47**  | `UC_HRM_185` | Báo cáo quỹ phép — Polish BE/FE & Unit Test                                 |    100%    |
|   `[XONG]`   | `UC_HRM_186` | Báo cáo chi phí nhân sự — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_HRM_187` | Báo cáo định biên vs thực tế — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_LMS_001` | Danh mục chương trình đào tạo — Polish BE/FE & Unit Test                    |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 48**  | `UC_LMS_002` | Danh mục khóa học — Polish BE/FE & Unit Test                                |    100%    |
|   `[XONG]`   | `UC_LMS_003` | Phân loại khóa (online/offline/blended) — Polish BE/FE & Unit Test          |    100%    |
|              | `UC_LMS_004` | Quản lý chương / bài học — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_LMS_005` | Upload video bài giảng — Polish BE/FE & Unit Test                           |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 49**  | `UC_LMS_006` | Upload tài liệu PDF / slide — Polish BE/FE & Unit Test                      |    100%    |
|   `[XONG]`   | `UC_LMS_009` | Ẩn / xuất bản khóa học — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_LMS_014` | Cấu hình điểm đạt / số lần thi — Polish BE/FE & Unit Test                   |    100%    |
|              | `UC_LMS_016` | Mở lớp đào tạo offline — Polish BE/FE & Unit Test                           |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 50**  | `UC_LMS_017` | Gán giảng viên / địa điểm / lịch — Polish BE/FE & Unit Test                 |    100%    |
|   `[XONG]`   | `UC_LMS_018` | Tuyển sinh / ghi danh học viên — Polish BE/FE & Unit Test                   |    100%    |
|              | `UC_LMS_019` | Điểm danh buổi học — Polish BE/FE & Unit Test                               |    100%    |
|              | `UC_LMS_022` | Đóng lớp & tổng kết — Polish BE/FE & Unit Test                              |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 51**  | `UC_LMS_023` | Gán mentor cho học viên — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_LMS_028` | Đăng ký tài khoản học viên — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_LMS_029` | Đăng nhập / quên mật khẩu — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_LMS_030` | Danh sách & chi tiết khóa — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 52**  | `UC_LMS_031` | Mua khóa / thanh toán online — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_LMS_032` | Kích hoạt bằng mã voucher — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_LMS_033` | Tự mở khóa sau thanh toán — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_LMS_034` | Xem video / tài liệu — Polish BE/FE & Unit Test                             |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 53**  | `UC_LMS_035` | Đánh dấu hoàn thành bài học — Polish BE/FE & Unit Test                      |    100%    |
|   `[XONG]`   | `UC_LMS_036` | Tiếp tục học dở — Polish BE/FE & Unit Test                                  |    100%    |
|              | `UC_LMS_037` | Theo dõi % tiến độ khóa — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_LMS_040` | Làm quiz cuối chương — Polish BE/FE & Unit Test                             |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 54**  | `UC_LMS_041` | Thi cuối khóa — Polish BE/FE & Unit Test                                    |    100%    |
|   `[XONG]`   | `UC_LMS_042` | Chấm điểm tự động — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_LMS_043` | Xem kết quả & đáp án — Polish BE/FE & Unit Test                             |    100%    |
|              | `UC_LMS_044` | Điều kiện cấp chứng chỉ — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 55**  | `UC_LMS_045` | Cấp chứng chỉ điện tử — Polish BE/FE & Unit Test                            |    100%    |
|   `[XONG]`   | `UC_LMS_049` | Hồ sơ giảng viên — Polish BE/FE & Unit Test                                 |    100%    |
|              | `UC_LMS_050` | Phân quyền giảng viên — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_LMS_051` | Theo dõi danh sách học viên — Polish BE/FE & Unit Test                      |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 56**  | `UC_LMS_058` | Xác nhận đã đọc nội quy — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_LMS_065` | Dashboard tiến độ đào tạo — Polish BE/FE & Unit Test                        |    100%    |
|              | `UC_LMS_066` | Báo cáo hoàn thành theo đơn vị — Polish BE/FE & Unit Test                   |    100%    |
|              | `UC_LMS_070` | Xuất báo cáo đào tạo — Polish BE/FE & Unit Test                             |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 57**  | `UC_CRM_001` | Tạo khách hàng cá nhân — Polish BE/FE & Unit Test                           |    100%    |
|   `[XONG]`   | `UC_CRM_002` | Tạo khách hàng doanh nghiệp — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_CRM_003` | Cập nhật thông tin khách hàng — Polish BE/FE & Unit Test                    |    100%    |
|              | `UC_CRM_004` | Kiểm tra trùng SĐT / MST — Polish BE/FE & Unit Test                         |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 58**  | `UC_CRM_005` | Gộp khách hàng trùng — Polish BE/FE & Unit Test                             |    100%    |
|   `[XONG]`   | `UC_CRM_006` | Phân loại tệp khách hàng — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_008` | Gán người phụ trách — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_CRM_009` | Bàn giao khách hàng — Polish BE/FE & Unit Test                              |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 59**  | `UC_CRM_010` | Hồ sơ khách 360° — Polish BE/FE & Unit Test                                 |    100%    |
|   `[XONG]`   | `UC_CRM_011` | Danh sách người liên hệ — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_CRM_012` | Lịch sử thay đổi dữ liệu — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_013` | Ngưng sử dụng / blacklist — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 60**  | `UC_CRM_014` | Import / export khách hàng — Polish BE/FE & Unit Test                       |    100%    |
|   `[XONG]`   | `UC_CRM_015` | Tìm kiếm khách đa tiêu chí — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_CRM_016` | Tạo campaign marketing — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_CRM_017` | Quản lý nhóm quảng cáo — Polish BE/FE & Unit Test                           |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 61**  | `UC_CRM_018` | Gắn sản phẩm / đối tượng mục tiêu — Polish BE/FE & Unit Test                |    100%    |
|   `[XONG]`   | `UC_CRM_019` | Ghi nhận chi phí quảng cáo — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_CRM_020` | Gắn ngân sách & theo dõi — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_021` | Đánh giá hậu chiến dịch — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 62**  | `UC_CRM_023` | Đóng campaign — Polish BE/FE & Unit Test                                    |    100%    |
|   `[XONG]`   | `UC_CRM_024` | Danh mục nguồn lead — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_CRM_025` | Đồng bộ lead mạng xã hội — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_026` | Đồng bộ lead website / landing — Polish BE/FE & Unit Test                   |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 63**  | `UC_CRM_027` | Đồng bộ kênh khác — Polish BE/FE & Unit Test                                |    100%    |
|   `[XONG]`   | `UC_CRM_028` | Attribution nguồn khách — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_CRM_029` | Tính CPL / CAC / ROAS / ROI — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_CRM_030` | Funnel marketing đến doanh thu — Polish BE/FE & Unit Test                   |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 64**  | `UC_CRM_031` | Dashboard marketing — Polish BE/FE & Unit Test                              |    100%    |
|   `[XONG]`   | `UC_CRM_032` | Tạo chương trình khuyến mại — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_CRM_033` | Cấu hình điều kiện khuyến mại — Polish BE/FE & Unit Test                    |    100%    |
|              | `UC_CRM_034` | Sinh mã voucher — Polish BE/FE & Unit Test                                  |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 65**  | `UC_CRM_035` | Giới hạn lượt dùng voucher — Polish BE/FE & Unit Test                       |    100%    |
|   `[XONG]`   | `UC_CRM_036` | Đồng bộ khuyến mại sang POS — Polish BE/FE & Unit Test                      |    100%    |
|              | `UC_CRM_037` | Áp dụng khuyến mại trên báo giá — Polish BE/FE & Unit Test                  |    100%    |
|              | `UC_CRM_038` | Báo cáo sử dụng voucher — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 66**  | `UC_CRM_047` | Lưu lịch sử chat — Polish BE/FE & Unit Test                                 |    100%    |
|   `[XONG]`   | `UC_CRM_049` | Tạo lead thủ công — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_CRM_050` | Tiếp nhận lead tự động — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_CRM_051` | Phân bổ lead cho sales — Polish BE/FE & Unit Test                           |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 67**  | `UC_CRM_052` | Lead scoring — Polish BE/FE & Unit Test                                     |    100%    |
|   `[XONG]`   | `UC_CRM_053` | Cập nhật trạng thái pipeline — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_CRM_054` | Task follow-up lead — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_CRM_055` | Nhắc việc follow-up — Polish BE/FE & Unit Test                              |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 68**  | `UC_CRM_056` | Nhật ký chăm sóc lead — Polish BE/FE & Unit Test                            |    100%    |
|   `[XONG]`   | `UC_CRM_057` | Chuyển lead thành cơ hội — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_058` | Đánh dấu lead mất — Polish BE/FE & Unit Test                                |    100%    |
|              | `UC_CRM_059` | Gộp lead trùng — Polish BE/FE & Unit Test                                   |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 69**  | `UC_CRM_060` | Import lead Excel — Polish BE/FE & Unit Test                                |    100%    |
|   `[XONG]`   | `UC_CRM_061` | Báo cáo chuyển đổi lead — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_CRM_062` | Tạo cơ hội từ lead/khách — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_063` | Pipeline cơ hội theo giai đoạn — Polish BE/FE & Unit Test                   |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 70**  | `UC_CRM_064` | Dự báo doanh thu — Polish BE/FE & Unit Test                                 |    100%    |
|   `[XONG]`   | `UC_CRM_065` | Gắn sản phẩm / giá trị ước tính — Polish BE/FE & Unit Test                  |    100%    |
|              | `UC_CRM_066` | Đối thủ / ghi chú đàm phán — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_CRM_067` | Chuyển cơ hội sang báo giá — Polish BE/FE & Unit Test                       |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 71**  | `UC_CRM_068` | Đóng thắng / thua — Polish BE/FE & Unit Test                                |    100%    |
|   `[XONG]`   | `UC_CRM_069` | Báo cáo win-rate — Polish BE/FE & Unit Test                                 |    100%    |
|              | `UC_CRM_070` | Tạo báo giá từ cơ hội — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_CRM_071` | Thêm dòng sản phẩm / dịch vụ — Polish BE/FE & Unit Test                     |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 72**  | `UC_CRM_072` | Áp chính sách giá / bảng giá — Polish BE/FE & Unit Test                     |    100%    |
|   `[XONG]`   | `UC_CRM_073` | Xin duyệt chiết khấu — Polish BE/FE & Unit Test                             |    100%    |
|              | `UC_CRM_074` | Gửi báo giá PDF/email — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_CRM_075` | Phiên bản báo giá — Polish BE/FE & Unit Test                                |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 73**  | `UC_CRM_076` | Hết hạn báo giá tự động — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_CRM_077` | Chuyển báo giá thành đơn — Polish BE/FE & Unit Test                         |    100%    |
|              | `UC_CRM_078` | In mẫu báo giá — Polish BE/FE & Unit Test                                   |    100%    |
|              | `UC_CRM_079` | Tạo đơn hàng từ báo giá — Polish BE/FE & Unit Test                          |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 74**  | `UC_CRM_081` | Cập nhật trạng thái đơn — Polish BE/FE & Unit Test                          |    100%    |
|   `[XONG]`   | `UC_CRM_082` | Giữ tồn khi duyệt đơn — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_CRM_083` | Tách / gộp đơn — Polish BE/FE & Unit Test                                   |    100%    |
|              | `UC_CRM_084` | Hủy đơn có kiểm soát — Polish BE/FE & Unit Test                             |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 75**  | `UC_CRM_085` | Trả hàng / điều chỉnh đơn — Polish BE/FE & Unit Test                        |    100%    |
|   `[XONG]`   | `UC_CRM_086` | Gắn hợp đồng — Polish BE/FE & Unit Test                                     |    100%    |
|              | `UC_CRM_087` | Theo dõi thanh toán — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_CRM_088` | Đẩy đơn sang kho / giao vận — Polish BE/FE & Unit Test                      |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 76**  | `UC_POS_001` | Khai báo điểm bán POS — Polish BE/FE & Unit Test                            |    100%    |
|   `[XONG]`   | `UC_POS_002` | Khai báo quầy / máy POS — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_POS_003` | Cấu hình máy in hóa đơn — Polish BE/FE & Unit Test                          |    100%    |
|              | `UC_POS_007` | Phân quyền thu ngân trên POS — Polish BE/FE & Unit Test                     |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 77**  | `UC_POS_009` | Danh mục nhóm sản phẩm — Polish BE/FE & Unit Test                           |    100%    |
|   `[XONG]`   | `UC_POS_010` | Danh mục sản phẩm bán — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_POS_012` | BOM / định mức nguyên liệu — Polish BE/FE & Unit Test                       |    100%    |
|              | `UC_POS_014` | Ngưng bán sản phẩm tạm thời — Polish BE/FE & Unit Test                      |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 78**  | `UC_POS_015` | Đồng bộ catalog từ back-office — Polish BE/FE & Unit Test                   |    100%    |
|   `[XONG]`   | `UC_POS_016` | Bảng giá theo điểm bán — Polish BE/FE & Unit Test                           |    100%    |
|              | `UC_POS_019` | Cấu hình thuế GTGT — Polish BE/FE & Unit Test                               |    100%    |
|              | `UC_POS_021` | Áp dụng chương trình khuyến mại — Polish BE/FE & Unit Test                  |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 79**  | `UC_POS_022` | Nhập mã voucher — Polish BE/FE & Unit Test                                  |    100%    |
|   `[XONG]`   | `UC_POS_024` | Giảm giá tay có quyền — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_POS_026` | Mở đơn / chọn khu vực — Polish BE/FE & Unit Test                            |    100%    |
|              | `UC_POS_027` | Thêm / sửa / xóa sản phẩm — Polish BE/FE & Unit Test                        |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 80**  | `UC_POS_032` | Tạm tính / giữ đơn — Polish BE/FE & Unit Test                               |    100%    |
|   `[XONG]`   | `UC_POS_033` | Thanh toán tiền mặt — Polish BE/FE & Unit Test                              |    100%    |
|              | `UC_POS_034` | Thanh toán chuyển khoản / QR — Polish BE/FE & Unit Test                     |    100%    |
|              | `UC_POS_035` | Thanh toán thẻ / ví điện tử — Polish BE/FE & Unit Test                      |    100%    |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 81**  | `UC_POS_037` | In hóa đơn — Polish BE/FE & Unit Test                                       | 90% ➔ 100% |
|              | `UC_POS_038` | Hủy sản phẩm — Polish BE/FE & Unit Test                                     | 85% ➔ 100% |
|              | `UC_POS_039` | Hủy cả bill — Polish BE/FE & Unit Test                                      | 85% ➔ 100% |
|              | `UC_POS_040` | Trả hàng / hoàn tiền — Polish BE/FE & Unit Test                             | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 82**  | `UC_POS_042` | Mở ca thu ngân — Polish BE/FE & Unit Test                                   | 90% ➔ 100% |
|              | `UC_POS_043` | Nhập tiền đầu ca — Polish BE/FE & Unit Test                                 | 90% ➔ 100% |
|              | `UC_POS_045` | Xem doanh thu trong ca — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_POS_046` | Đóng ca & đếm quỹ — Polish BE/FE & Unit Test                                | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 83**  | `UC_POS_047` | Đối soát lệch quỹ — Polish BE/FE & Unit Test                                | 85% ➔ 100% |
|              | `UC_POS_048` | In báo cáo ca — Polish BE/FE & Unit Test                                    | 90% ➔ 100% |
|              | `UC_POS_054` | Trừ tồn theo BOM khi bán — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_POS_055` | Cảnh báo hết / sắp hết — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 84**  | `UC_POS_059` | Đồng bộ doanh thu ca sang FIN — Polish BE/FE & Unit Test                    | 90% ➔ 100% |
|              | `UC_POS_061` | Doanh thu theo giờ / ngày / ca — Polish BE/FE & Unit Test                   | 90% ➔ 100% |
|              | `UC_POS_062` | Doanh thu theo sản phẩm — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_POS_063` | Doanh thu theo thu ngân — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 85**  | `UC_POS_064` | Tỷ lệ hủy / giảm giá — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_POS_065` | Cost lý thuyết vs thực tế — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|              | `UC_POS_066` | Top sản phẩm bán chạy — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_POS_067` | So sánh điểm bán — Polish BE/FE & Unit Test                                 | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 86**  | `UC_POS_068` | Xuất báo cáo POS — Polish BE/FE & Unit Test                                 | 90% ➔ 100% |
|              | `UC_POS_069` | Giám sát doanh thu chuỗi realtime — Polish BE/FE & Unit Test                | 90% ➔ 100% |
|              | `UC_POS_072` | Cấu hình target doanh thu — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|              | `UC_PUR_001` | Tạo / cập nhật nhà cung cấp — Polish BE/FE & Unit Test                      | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 87**  | `UC_PUR_003` | Người liên hệ & điều khoản — Polish BE/FE & Unit Test                       | 85% ➔ 100% |
|              | `UC_PUR_009` | Gắn sản phẩm – nhà cung cấp — Polish BE/FE & Unit Test                      | 85% ➔ 100% |
|              | `UC_PUR_014` | Tạo PR từ đơn vị — Polish BE/FE & Unit Test                                 | 95% ➔ 100% |
|              | `UC_PUR_017` | Luồng duyệt PR — Polish BE/FE & Unit Test                                   | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 88**  | `UC_PUR_018` | Từ chối / trả lại PR — Polish BE/FE & Unit Test                             | 85% ➔ 100% |
|              | `UC_PUR_019` | Theo dõi trạng thái PR — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_PUR_026` | Tạo PO từ PR/RFQ — Polish BE/FE & Unit Test                                 | 85% ➔ 100% |
|              | `UC_PUR_027` | Duyệt PO theo hạn mức — Polish BE/FE & Unit Test                            | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 89**  | `UC_PUR_028` | Gửi PO cho nhà cung cấp — Polish BE/FE & Unit Test                          | 85% ➔ 100% |
|              | `UC_PUR_030` | Sửa PO phiên bản — Polish BE/FE & Unit Test                                 | 95% ➔ 100% |
|              | `UC_PUR_031` | Theo dõi nhận hàng từng phần — Polish BE/FE & Unit Test                     | 90% ➔ 100% |
|              | `UC_PUR_032` | Đóng / hủy PO — Polish BE/FE & Unit Test                                    | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 90**  | `UC_PUR_033` | In / xuất PO — Polish BE/FE & Unit Test                                     | 90% ➔ 100% |
|              | `UC_PUR_034` | Tạo phiếu nhận hàng theo PO — Polish BE/FE & Unit Test                      | 90% ➔ 100% |
|              | `UC_PUR_035` | Nhận hàng lệch số lượng / chất lượng — Polish BE/FE & Unit Test             | 85% ➔ 100% |
|              | `UC_PUR_037` | Đẩy nhập kho sang INV — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 91**  | `UC_PUR_040` | Nhập hóa đơn nhà cung cấp — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|              | `UC_PUR_041` | Đối soát 3 chiều PO–GRN–Invoice — Polish BE/FE & Unit Test                  | 85% ➔ 100% |
|              | `UC_PUR_043` | Đẩy công nợ sang FIN AP — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_PUR_048` | Báo cáo mua theo nhà cung cấp / SP — Polish BE/FE & Unit Test               | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 92**  | `UC_PUR_051` | Open PR / Open PO aging — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_PUR_052` | Xuất báo cáo mua hàng — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_INV_001` | Tạo / sửa SKU sản phẩm — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_INV_002` | Phân nhóm hàng / ngành hàng — Polish BE/FE & Unit Test                      | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 93**  | `UC_INV_003` | Đơn vị tính & quy đổi — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|              | `UC_INV_004` | Thuộc tính hàng (lô, serial, HSD) — Polish BE/FE & Unit Test                | 80% ➔ 100% |
|              | `UC_INV_005` | Giá vốn / phương pháp tính giá — Polish BE/FE & Unit Test                   | 80% ➔ 100% |
|              | `UC_INV_007` | Ngưng sử dụng SKU — Polish BE/FE & Unit Test                                | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 94**  | `UC_INV_008` | Import / export danh mục SP — Polish BE/FE & Unit Test                      | 85% ➔ 100% |
|              | `UC_INV_011` | Tạo kho — Polish BE/FE & Unit Test                                          | 85% ➔ 100% |
|              | `UC_INV_014` | Gán thủ kho / quyền — Polish BE/FE & Unit Test                              | 95% ➔ 100% |
|              | `UC_INV_015` | Cấu hình FEFO / FIFO — Polish BE/FE & Unit Test                             | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 95**  | `UC_INV_016` | Cho phép tồn âm hay không — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_INV_017` | Nhập từ mua hàng — Polish BE/FE & Unit Test                                 | 95% ➔ 100% |
|              | `UC_INV_018` | Nhập từ sản xuất — Polish BE/FE & Unit Test                                 | 80% ➔ 100% |
|              | `UC_INV_019` | Nhập điều chỉnh / kiểm kê — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 96**  | `UC_INV_020` | Nhập chuyển đến — Polish BE/FE & Unit Test                                  | 80% ➔ 100% |
|              | `UC_INV_022` | Nhập theo lô / HSD / serial — Polish BE/FE & Unit Test                      | 80% ➔ 100% |
|              | `UC_INV_024` | Xuất bán / giao hàng — Polish BE/FE & Unit Test                             | 80% ➔ 100% |
|              | `UC_INV_025` | Xuất sản xuất — Polish BE/FE & Unit Test                                    | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 97**  | `UC_INV_026` | Xuất nội bộ / tiêu hao — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_INV_029` | Xuất theo FEFO tự động — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|              | `UC_INV_030` | Xuất điều chỉnh — Polish BE/FE & Unit Test                                  | 85% ➔ 100% |
|              | `UC_INV_031` | Tạo phiếu chuyển kho — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 98**  | `UC_INV_033` | Xuất bên gửi / nhập bên nhận — Polish BE/FE & Unit Test                     | 90% ➔ 100% |
|              | `UC_INV_035` | Theo dõi hàng đang chuyển — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_INV_036` | Chuyển từ kho trung tâm — Polish BE/FE & Unit Test                          | 80% ➔ 100% |
|              | `UC_INV_037` | Giữ hàng theo đơn đã duyệt — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 99**  | `UC_INV_038` | Giải phóng giữ hàng — Polish BE/FE & Unit Test                              | 90% ➔ 100% |
|              | `UC_INV_039` | Xem tồn thực tế — Polish BE/FE & Unit Test                                  | 90% ➔ 100% |
|              | `UC_INV_041` | Xem tồn đang giữ / đang chuyển — Polish BE/FE & Unit Test                   | 80% ➔ 100% |
|              | `UC_INV_042` | Cảnh báo không đủ tồn — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 100** | `UC_INV_043` | Theo dõi tồn theo lô — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_INV_044` | Cảnh báo cận date / quá date — Polish BE/FE & Unit Test                     | 90% ➔ 100% |
|              | `UC_INV_045` | Chặn xuất hàng quá HSD — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|              | `UC_INV_048` | Báo cáo hàng sắp hết hạn — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 101** | `UC_INV_049` | Tạo phiếu kiểm kê — Polish BE/FE & Unit Test                                | 90% ➔ 100% |
|              | `UC_INV_050` | Nhập số đếm thực tế — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|              | `UC_INV_052` | Đối chiếu lệch kiểm kê — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_INV_053` | Duyệt điều chỉnh sau kiểm kê — Polish BE/FE & Unit Test                     | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 102** | `UC_INV_055` | Báo cáo kết quả kiểm kê — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_INV_060` | Xem giá trị tồn — Polish BE/FE & Unit Test                                  | 90% ➔ 100% |
|              | `UC_INV_062` | Đẩy bút toán kho sang FIN — Polish BE/FE & Unit Test                        | 80% ➔ 100% |
|              | `UC_INV_063` | Báo cáo giá trị tồn — Polish BE/FE & Unit Test                              | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 103** | `UC_INV_064` | Xuất nhập tồn theo kỳ — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_INV_065` | Thẻ kho / lịch sử sản phẩm — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|              | `UC_INV_067` | Hàng dưới min / trên max — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_INV_069` | Dashboard tồn & cảnh báo — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 104** | `UC_INV_070` | Xuất báo cáo kho Excel — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|              | `UC_LOG_001` | Danh mục đơn vị vận chuyển — Polish BE/FE & Unit Test                       | 85% ➔ 100% |
|              | `UC_LOG_006` | Tạo lệnh giao từ đơn hàng — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_LOG_008` | Tách lệnh giao nhiều đợt — Polish BE/FE & Unit Test                         | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 105** | `UC_LOG_009` | Pick list / soạn hàng — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|              | `UC_LOG_011` | In vận đơn / phiếu giao — Polish BE/FE & Unit Test                          | 95% ➔ 100% |
|              | `UC_LOG_012` | Hủy / hoàn lệnh giao — Polish BE/FE & Unit Test                             | 85% ➔ 100% |
|              | `UC_LOG_013` | Phân công tài xế / đơn vị vận chuyển — Polish BE/FE & Unit Test             | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 106** | `UC_LOG_014` | Cập nhật trạng thái vận đơn — Polish BE/FE & Unit Test                      | 95% ➔ 100% |
|              | `UC_LOG_017` | Ghi nhận giao thất bại — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_LOG_021` | Ghi nhận số tiền COD — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_LOG_022` | Xác nhận đã thu COD — Polish BE/FE & Unit Test                              | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 107** | `UC_LOG_023` | Bàn giao tiền COD — Polish BE/FE & Unit Test                                | 90% ➔ 100% |
|              | `UC_LOG_024` | Đối soát 3 chiều COD — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_LOG_026` | Xử lý lệch COD — Polish BE/FE & Unit Test                                   | 85% ➔ 100% |
|              | `UC_LOG_027` | Tạo phiếu hoàn về kho — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 108** | `UC_LOG_028` | Kiểm đếm hàng hoàn — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_LOG_029` | Nhập kho hàng hoàn — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_LOG_034` | Tỷ lệ giao đúng hạn — Polish BE/FE & Unit Test                              | 90% ➔ 100% |
|              | `UC_LOG_035` | Tỷ lệ hoàn / thất bại — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 109** | `UC_LOG_038` | Báo cáo COD tồn / đã nộp — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_LOG_039` | Dashboard giao vận — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_MFG_001` | Danh mục thành phẩm / bán thành phẩm — Polish BE/FE & Unit Test             | 85% ➔ 100% |
|              | `UC_MFG_002` | Danh mục nguyên vật liệu — Polish BE/FE & Unit Test                         | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 110** | `UC_MFG_003` | Danh mục xưởng / dây chuyền — Polish BE/FE & Unit Test                      | 85% ➔ 100% |
|              | `UC_MFG_006` | Tạo BOM nhiều cấp — Polish BE/FE & Unit Test                                | 85% ➔ 100% |
|              | `UC_MFG_007` | Phiên bản BOM — Polish BE/FE & Unit Test                                    | 85% ➔ 100% |
|              | `UC_MFG_008` | Định mức nguyên vật liệu — Polish BE/FE & Unit Test                         | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 111** | `UC_MFG_013` | Kế hoạch SX theo đơn hàng — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_MFG_017` | Tạo lệnh sản xuất — Polish BE/FE & Unit Test                                | 85% ➔ 100% |
|              | `UC_MFG_018` | Duyệt lệnh sản xuất — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|              | `UC_MFG_019` | Phát hành lệnh / in phiếu — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 112** | `UC_MFG_022` | Ghi nhận thành phẩm nhập kho — Polish BE/FE & Unit Test                     | 95% ➔ 100% |
|              | `UC_MFG_023` | Ghi nhận phế phẩm / hao hụt — Polish BE/FE & Unit Test                      | 90% ➔ 100% |
|              | `UC_MFG_024` | Tạm dừng / hủy lệnh — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|              | `UC_MFG_025` | Đóng lệnh sản xuất — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 113** | `UC_MFG_027` | Tập hợp chi phí nguyên vật liệu — Polish BE/FE & Unit Test                  | 90% ➔ 100% |
|              | `UC_MFG_029` | Giá thành đơn vị thành phẩm — Polish BE/FE & Unit Test                      | 90% ➔ 100% |
|              | `UC_MFG_031` | Đẩy giá thành sang INV/FIN — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|              | `UC_MFG_041` | Tiến độ lệnh sản xuất — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 114** | `UC_MFG_042` | Sản lượng theo ngày/ca/xưởng — Polish BE/FE & Unit Test                     | 95% ➔ 100% |
|              | `UC_MFG_043` | Tiêu hao nguyên vật liệu variance — Polish BE/FE & Unit Test                | 90% ➔ 100% |
|              | `UC_MFG_045` | Dashboard sản xuất — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_MFG_046` | Xuất báo cáo sản xuất — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 115** | `UC_FSM_001` | Danh mục loại dịch vụ — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|              | `UC_FSM_002` | Danh mục mã lỗi — Polish BE/FE & Unit Test                                  | 85% ➔ 100% |
|              | `UC_FSM_003` | Danh mục linh kiện — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_FSM_005` | Cấu hình SLA — Polish BE/FE & Unit Test                                     | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 116** | `UC_FSM_008` | Hồ sơ thiết bị đã bán — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|              | `UC_FSM_009` | Serial / model / ngày kích hoạt BH — Polish BE/FE & Unit Test               | 85% ➔ 100% |
|              | `UC_FSM_013` | Tạo ticket từ kênh — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_FSM_014` | Phân loại mức ưu tiên — Polish BE/FE & Unit Test                            | 95% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 117** | `UC_FSM_017` | Đổi kỹ thuật viên / escalate — Polish BE/FE & Unit Test                     | 95% ➔ 100% |
|              | `UC_FSM_018` | Lịch hẹn với khách — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_FSM_019` | Xác nhận lịch trên APP — Polish BE/FE & Unit Test                           | 80% ➔ 100% |
|              | `UC_FSM_022` | Ghi nhận nguyên nhân & xử lý — Polish BE/FE & Unit Test                     | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 118** | `UC_FSM_024` | Xuất linh kiện theo ticket — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|              | `UC_FSM_027` | Check-out / hoàn thành — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|              | `UC_FSM_028` | Khách ký nghiệm thu — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|              | `UC_FSM_030` | Đóng ticket đạt/trễ SLA — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 119** | `UC_FSM_037` | Tồn linh kiện tại kho KT — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_FSM_038` | Cấp linh kiện cho KTV — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_FSM_039` | Đối soát linh kiện — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_FSM_041` | Danh sách việc hôm nay — Polish BE/FE & Unit Test                           | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 120** | `UC_FSM_042` | Điều hướng / thông tin khách — Polish BE/FE & Unit Test                     | 80% ➔ 100% |
|              | `UC_FSM_045` | SLA compliance realtime — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_FSM_046` | Năng suất kỹ thuật viên — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_FSM_047` | Chi phí linh kiện — Polish BE/FE & Unit Test                                | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 121** | `UC_FSM_050` | Xuất báo cáo kỹ thuật — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_PJM_001` | Loại dự án — Polish BE/FE & Unit Test                                       | 85% ➔ 100% |
|              | `UC_PJM_002` | Mẫu hạng mục / WBS — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_PJM_004` | Trạng thái dự án chuẩn — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 122** | `UC_PJM_005` | Tạo dự án từ cơ hội CRM — Polish BE/FE & Unit Test                          | 85% ➔ 100% |
|              | `UC_PJM_006` | Tạo dự án thủ công — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_PJM_007` | Gắn khách hàng / hợp đồng — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_PJM_008` | Gán quản lý dự án / thành viên — Polish BE/FE & Unit Test                   | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 123** | `UC_PJM_009` | Ngân sách dự kiến & timeline — Polish BE/FE & Unit Test                     | 85% ➔ 100% |
|              | `UC_PJM_011` | Tạo hạng mục WBS — Polish BE/FE & Unit Test                                 | 85% ➔ 100% |
|              | `UC_PJM_012` | Gán người thực hiện — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|              | `UC_PJM_013` | Cập nhật % hoàn thành — Polish BE/FE & Unit Test                            | 95% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 124** | `UC_PJM_014` | Milestone & deadline — Polish BE/FE & Unit Test                             | 95% ➔ 100% |
|              | `UC_PJM_017` | Cảnh báo trễ tiến độ — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_PJM_019` | Phân công nhân sự — Polish BE/FE & Unit Test                                | 90% ➔ 100% |
|              | `UC_PJM_021` | Xuất nguyên vật liệu cho dự án — Polish BE/FE & Unit Test                   | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 125** | `UC_PJM_022` | Ghi nhận chi phí phát sinh — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|              | `UC_PJM_023` | Theo dõi ngân sách vs thực tế — Polish BE/FE & Unit Test                    | 90% ➔ 100% |
|              | `UC_PJM_031` | Biên bản nghiệm thu giai đoạn — Polish BE/FE & Unit Test                    | 90% ➔ 100% |
|              | `UC_PJM_032` | Nghiệm thu cuối & bàn giao — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 126** | `UC_PJM_033` | Khách ký xác nhận — Polish BE/FE & Unit Test                                | 90% ➔ 100% |
|              | `UC_PJM_034` | Ghi nhận doanh thu dự án — Polish BE/FE & Unit Test                         | 85% ➔ 100% |
|              | `UC_PJM_035` | Quyết toán chi phí & P&L — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_PJM_036` | Đóng dự án / lưu trữ — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 127** | `UC_PJM_038` | Portfolio dự án đang chạy — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|              | `UC_PJM_039` | Tiến độ & sức khỏe dự án — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_PJM_040` | Lợi nhuận theo dự án — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_PJM_042` | Xuất báo cáo dự án — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 128** | `UC_FIN_001` | Hệ thống tài khoản (COA) — Polish BE/FE & Unit Test                         | 85% ➔ 100% |
|              | `UC_FIN_002` | Nhóm tài khoản / chỉ tiêu — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_FIN_003` | Kỳ kế toán / năm tài chính — Polish BE/FE & Unit Test                       | 85% ➔ 100% |
|              | `UC_FIN_004` | Khóa sổ kỳ / mở lại — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 129** | `UC_FIN_006` | Trung tâm chi phí — Polish BE/FE & Unit Test                                | 85% ➔ 100% |
|              | `UC_FIN_008` | Hình thức thanh toán — Polish BE/FE & Unit Test                             | 85% ➔ 100% |
|              | `UC_FIN_009` | Danh mục thuế — Polish BE/FE & Unit Test                                    | 85% ➔ 100% |
|              | `UC_FIN_010` | Tạo bút toán thủ công — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 130** | `UC_FIN_012` | Đảo bút toán — Polish BE/FE & Unit Test                                     | 85% ➔ 100% |
|              | `UC_FIN_013` | Xem sổ cái theo tài khoản — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_FIN_014` | Sổ chi tiết theo đối tượng — Polish BE/FE & Unit Test                       | 85% ➔ 100% |
|              | `UC_FIN_015` | Nhận bút toán tự động — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 131** | `UC_FIN_018` | Danh mục quỹ / thủ quỹ — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|              | `UC_FIN_019` | Phiếu thu tiền mặt — Polish BE/FE & Unit Test                               | 95% ➔ 100% |
|              | `UC_FIN_020` | Phiếu chi tiền mặt — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_FIN_023` | Báo cáo sổ quỹ — Polish BE/FE & Unit Test                                   | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 132** | `UC_FIN_024` | Danh mục tài khoản ngân hàng — Polish BE/FE & Unit Test                     | 90% ➔ 100% |
|              | `UC_FIN_025` | Giấy báo Nợ / Có — Polish BE/FE & Unit Test                                 | 95% ➔ 100% |
|              | `UC_FIN_026` | Đối soát sao kê ngân hàng — Polish BE/FE & Unit Test                        | 85% ➔ 100% |
|              | `UC_FIN_027` | Đề nghị chuyển khoản — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 133** | `UC_FIN_029` | Theo dõi số dư ngân hàng — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_FIN_030` | Tạo hóa đơn phải thu — Polish BE/FE & Unit Test                             | 95% ➔ 100% |
|              | `UC_FIN_031` | Công nợ theo khách / hóa đơn — Polish BE/FE & Unit Test                     | 90% ➔ 100% |
|              | `UC_FIN_032` | Thu tiền & phân bổ — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 134** | `UC_FIN_035` | Cảnh báo vượt hạn mức — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_FIN_036` | Bảng tuổi nợ phải thu — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_FIN_039` | Tạo hóa đơn phải trả — Polish BE/FE & Unit Test                             | 95% ➔ 100% |
|              | `UC_FIN_040` | Công nợ theo nhà cung cấp — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 135** | `UC_FIN_041` | Đề nghị thanh toán — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_FIN_042` | Duyệt chi trả — Polish BE/FE & Unit Test                                    | 90% ➔ 100% |
|              | `UC_FIN_043` | Thanh toán & phân bổ AP — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_FIN_044` | Bảng tuổi nợ phải trả — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 136** | `UC_FIN_052` | Tính thuế GTGT đầu ra / đầu vào — Polish BE/FE & Unit Test                  | 90% ➔ 100% |
|              | `UC_FIN_053` | Bảng kê hóa đơn GTGT — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|              | `UC_FIN_056` | Cấu hình thuế suất — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_FIN_057` | Ghi nhận doanh thu từ POS — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 137** | `UC_FIN_058` | Ghi nhận doanh thu từ đơn — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|              | `UC_FIN_060` | Ghi nhận giá vốn hàng bán — Polish BE/FE & Unit Test                        | 90% ➔ 100% |
|              | `UC_AST_001` | Danh mục nhóm TSCĐ — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_AST_002` | Tạo thẻ tài sản — Polish BE/FE & Unit Test                                  | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 138** | `UC_AST_003` | Thông tin nguyên giá / ngày ghi tăng — Polish BE/FE & Unit Test             | 85% ➔ 100% |
|              | `UC_AST_004` | Gắn vị trí / chi nhánh — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_AST_008` | Cấu hình phương pháp khấu hao — Polish BE/FE & Unit Test                    | 85% ➔ 100% |
|              | `UC_AST_009` | Cấu hình thời gian / tỷ lệ — Polish BE/FE & Unit Test                       | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 139** | `UC_AST_011` | Xem sổ khấu hao — Polish BE/FE & Unit Test                                  | 85% ➔ 100% |
|              | `UC_AST_012` | Đẩy bút toán khấu hao sang FIN — Polish BE/FE & Unit Test                   | 90% ➔ 100% |
|              | `UC_AST_014` | Ghi tăng từ mua sắm — Polish BE/FE & Unit Test                              | 85% ➔ 100% |
|              | `UC_AST_016` | Điều chuyển tài sản nội bộ — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 140** | `UC_AST_017` | Bàn giao tài sản cho nhân viên — Polish BE/FE & Unit Test                   | 90% ➔ 100% |
|              | `UC_AST_018` | Thanh lý / nhượng bán — Polish BE/FE & Unit Test                            | 90% ➔ 100% |
|              | `UC_AST_021` | Tạo đợt kiểm kê tài sản — Polish BE/FE & Unit Test                          | 90% ➔ 100% |
|              | `UC_AST_022` | Đối chiếu thiếu / thừa — Polish BE/FE & Unit Test                           | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 141** | `UC_AST_030` | Sổ tài sản cố định — Polish BE/FE & Unit Test                               | 90% ➔ 100% |
|              | `UC_AST_031` | Báo cáo khấu hao theo kỳ — Polish BE/FE & Unit Test                         | 90% ➔ 100% |
|              | `UC_AST_032` | Báo cáo tài sản theo vị trí — Polish BE/FE & Unit Test                      | 90% ➔ 100% |
|              | `UC_AST_034` | Xuất báo cáo tài sản — Polish BE/FE & Unit Test                             | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 142** | `UC_WF_001`  | Loại công việc / ticket — Polish BE/FE & Unit Test                          | 80% ➔ 100% |
|              | `UC_WF_004`  | Nhóm / dự án nội bộ — Polish BE/FE & Unit Test                              | 80% ➔ 100% |
|              | `UC_WF_005`  | Tạo task / giao việc — Polish BE/FE & Unit Test                             | 80% ➔ 100% |
|              | `UC_WF_006`  | Gán người thực hiện / theo dõi — Polish BE/FE & Unit Test                   | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 143** | `UC_WF_007`  | Deadline / nhắc việc — Polish BE/FE & Unit Test                             | 80% ➔ 100% |
|              | `UC_WF_009`  | Bình luận / đính kèm file — Polish BE/FE & Unit Test                        | 80% ➔ 100% |
|              | `UC_WF_010`  | Chuyển trạng thái task — Polish BE/FE & Unit Test                           | 80% ➔ 100% |
|              | `UC_WF_012`  | Task liên kết chứng từ ERP — Polish BE/FE & Unit Test                       | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 144** | `UC_WF_014`  | Lọc task theo tiêu chí — Polish BE/FE & Unit Test                           | 80% ➔ 100% |
|              | `UC_WF_017`  | Tạo ticket nội bộ — Polish BE/FE & Unit Test                                | 80% ➔ 100% |
|              | `UC_WF_022`  | Tạo mẫu workflow duyệt — Polish BE/FE & Unit Test                           | 70% ➔ 100% |
|              | `UC_WF_023`  | Điều kiện duyệt theo quy tắc — Polish BE/FE & Unit Test                     | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 145** | `UC_WF_024`  | Nhiều cấp duyệt tuần tự / song song — Polish BE/FE & Unit Test              | 80% ➔ 100% |
|              | `UC_WF_025`  | Gắn workflow vào loại chứng từ — Polish BE/FE & Unit Test                   | 80% ➔ 100% |
|              | `UC_WF_029`  | Duyệt / từ chối / trả bổ sung — Polish BE/FE & Unit Test                    | 90% ➔ 100% |
|              | `UC_WF_031`  | Duyệt trên mobile APP — Polish BE/FE & Unit Test                            | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 146** | `UC_WF_032`  | Ủy quyền duyệt tạm thời — Polish BE/FE & Unit Test                          | 80% ➔ 100% |
|              | `UC_WF_033`  | Nhắc duyệt / escalate — Polish BE/FE & Unit Test                            | 80% ➔ 100% |
|              | `UC_WF_034`  | Lịch sử duyệt & comment — Polish BE/FE & Unit Test                          | 70% ➔ 100% |
|              | `UC_WF_038`  | Khối lượng task mở / quá hạn — Polish BE/FE & Unit Test                     | 80% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 147** | `UC_WF_040`  | Dashboard workflow — Polish BE/FE & Unit Test                               | 80% ➔ 100% |
|              | `UC_BI_001`  | Catalog dataset theo module — Polish BE/FE & Unit Test                      | 85% ➔ 100% |
|              | `UC_BI_002`  | Làm mới dữ liệu định kỳ — Polish BE/FE & Unit Test                          | 95% ➔ 100% |
|              | `UC_BI_003`  | Phân quyền xem báo cáo — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 148** | `UC_BI_006`  | Dashboard Ban lãnh đạo — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_BI_007`  | Dashboard theo module — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|              | `UC_BI_008`  | Widget doanh thu – lợi nhuận — Polish BE/FE & Unit Test                     | 95% ➔ 100% |
|              | `UC_BI_013`  | Danh mục báo cáo theo module — Polish BE/FE & Unit Test                     | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 149** | `UC_BI_014`  | Chạy báo cáo với bộ lọc — Polish BE/FE & Unit Test                          | 95% ➔ 100% |
|              | `UC_BI_016`  | Xuất Excel / PDF — Polish BE/FE & Unit Test                                 | 95% ➔ 100% |
|              | `UC_BI_018`  | So sánh kỳ / mục tiêu — Polish BE/FE & Unit Test                            | 95% ➔ 100% |
|              | `UC_BI_019`  | Cấu hình ngưỡng cảnh báo — Polish BE/FE & Unit Test                         | 95% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 150** | `UC_BI_021`  | Bảng theo dõi Target vs Actual — Polish BE/FE & Unit Test                   | 95% ➔ 100% |
|              | `UC_PRT_001` | Đăng ký tài khoản khách hàng — Polish BE/FE & Unit Test                     | 85% ➔ 100% |
|              | `UC_PRT_002` | Đăng nhập / quên mật khẩu — Polish BE/FE & Unit Test                        | 95% ➔ 100% |
|              | `UC_PRT_003` | Liên kết tài khoản với mã khách — Polish BE/FE & Unit Test                  | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 151** | `UC_PRT_007` | Xem danh sách đơn hàng — Polish BE/FE & Unit Test                           | 85% ➔ 100% |
|              | `UC_PRT_008` | Xem chi tiết & trạng thái đơn — Polish BE/FE & Unit Test                    | 85% ➔ 100% |
|              | `UC_PRT_014` | Xem công nợ hiện tại — Polish BE/FE & Unit Test                             | 95% ➔ 100% |
|              | `UC_PRT_015` | Xem bảng kê hóa đơn chưa thanh toán — Polish BE/FE & Unit Test              | 85% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 152** | `UC_PRT_016` | Lịch sử thanh toán — Polish BE/FE & Unit Test                               | 85% ➔ 100% |
|              | `UC_PRT_019` | Tạo ticket hỗ trợ — Polish BE/FE & Unit Test                                | 85% ➔ 100% |
|              | `UC_PRT_020` | Xem trạng thái ticket — Polish BE/FE & Unit Test                            | 85% ➔ 100% |
|              | `UC_PRT_037` | Cấu hình module portal theo gói — Polish BE/FE & Unit Test                  | 90% ➔ 100% |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 153** | `UC_SYS_009` | Đăng nhập SSO / OAuth — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_SYS_031` | Quyền theo trường nhạy cảm — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_SYS_058` | Quản lý phiên bản cấu hình — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_SYS_062` | Push notification mobile — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 154** | `UC_SYS_064` | Tùy chọn thông báo cá nhân — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_SYS_071` | Quét virus / bảo mật file — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_SYS_077` | Xuất dữ liệu hàng loạt — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_SYS_082` | Quản lý IP allow/deny — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 155** | `UC_SYS_093` | Tùy chỉnh theme / logo — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_SYS_094` | Trang chủ theo vai trò — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_SYS_103` | Tìm kiếm tin nhắn — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_SYS_104` | Tắt thông báo hội thoại — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 156** | `UC_HRM_005` | Quản lý bộ phận trong đơn vị — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_HRM_008` | Quản lý vị trí công việc — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_HRM_011` | Định nghĩa trung tâm chi phí NS — Khởi tạo Entity, Migration, API & UI      |  0% ➔ 90%  |
|              | `UC_HRM_023` | Quản lý người thân / liên hệ khẩn — Khởi tạo Entity, Migration, API & UI    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 157** | `UC_HRM_024` | Quản lý trình độ / kỹ năng — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_HRM_037` | Báo cáo biến động nhân sự — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_HRM_044` | In / xuất mẫu hợp đồng — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_HRM_058` | Import ứng viên hàng loạt — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 158** | `UC_HRM_088` | Import lịch ca Excel — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_HRM_124` | Lập bảng phạt — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_HRM_125` | Áp dụng phạt vào kỳ lương — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_HRM_174` | Đồng bộ bút toán lương sang FIN — Khởi tạo Entity, Migration, API & UI      |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 159** | `UC_HRM_177` | Mẫu đánh giá KPI / năng lực — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_HRM_178` | Tạo kỳ đánh giá — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_HRM_179` | Quản lý đánh giá nhân viên — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_HRM_180` | Nhân viên tự đánh giá — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 160** | `UC_HRM_181` | Tổng hợp kết quả đánh giá — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_LMS_007` | Gắn tag kỹ năng / vị trí — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_LMS_008` | Phiên bản nội dung khóa học — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_LMS_013` | Tạo đề thi random — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 161** | `UC_LMS_015` | Thời gian làm bài & chống gian lận — Khởi tạo Entity, Migration, API & UI   |  0% ➔ 90%  |
|              | `UC_LMS_024` | Checklist kèm cặp — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_LMS_026` | Đánh giá mentor / học viên — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_LMS_027` | Báo cáo hiệu quả mentoring — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 162** | `UC_LMS_038` | Nhắc học tiếp — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_LMS_039` | Diễn đàn / bình luận — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_LMS_046` | Mã xác thực chứng chỉ — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_LMS_047` | Thu hồi chứng chỉ — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 163** | `UC_LMS_048` | Đồng bộ chứng chỉ sang HRM — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_LMS_052` | Phản hồi bài tập — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_LMS_053` | Thống kê doanh thu theo khóa — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_LMS_054` | Chống chia sẻ tài khoản — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 164** | `UC_LMS_055` | Chặn tải video — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|              | `UC_LMS_056` | Tạo khảo sát hiểu bài — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_LMS_057` | Khảo sát tuân thủ — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_LMS_059` | Bắt buộc hoàn thành trước ca — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 165** | `UC_LMS_060` | Báo cáo tỷ lệ xác nhận — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_LMS_061` | Gán lộ trình theo chức danh — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_LMS_062` | Tự gán khóa bắt buộc khi nhận việc — Khởi tạo Entity, Migration, API & UI   |  0% ➔ 90%  |
|              | `UC_LMS_063` | Theo dõi hoàn thành lộ trình — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 166** | `UC_LMS_064` | Cảnh báo quá hạn đào tạo — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_LMS_067` | Báo cáo điểm thi / tỷ lệ đạt — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_LMS_068` | Báo cáo học viên bỏ dở — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_LMS_069` | Báo cáo hiệu quả khóa — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 167** | `UC_LMS_071` | Gợi ý khóa học tiếp theo — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_LMS_072` | Tóm tắt bài học bằng AI — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_LMS_073` | AI tạo quiz từ nội dung — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_LMS_074` | Trợ lý hỏi đáp — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 168** | `UC_CRM_007` | Đánh giá tiềm năng — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_CRM_022` | Nhân bản campaign — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_CRM_039` | Hộp thư tập trung đa kênh — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_CRM_040` | Tiếp nhận hội thoại mới — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 169** | `UC_CRM_041` | Phân phối hội thoại theo rule — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_CRM_042` | Chuyển hội thoại giữa agent — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_CRM_043` | SLA phản hồi & cảnh báo — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_CRM_044` | Chatbot kịch bản — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 170** | `UC_CRM_045` | Chatbot thu thập lead — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_CRM_046` | Chuyển bot sang agent — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_CRM_048` | Đánh giá CSAT — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_CRM_080` | Tiếp nhận đơn từ kênh online — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 171** | `UC_CRM_089` | Phân vùng / tuyến bán hàng — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_CRM_090` | Phân loại tần suất visit — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_CRM_091` | Lập kế hoạch visit — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_CRM_092` | Check-in / check-out GPS — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 172** | `UC_CRM_093` | Ghi nhận mục đích – kết quả visit — Khởi tạo Entity, Migration, API & UI    |  0% ➔ 90%  |
|              | `UC_CRM_094` | Ghi nhận nhu cầu khách hàng — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_CRM_095` | Đặt hàng tại điểm thăm — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_CRM_096` | Xem lịch sử visit — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 173** | `UC_CRM_097` | AI gợi ý việc ưu tiên — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_CRM_098` | Dashboard doanh số field — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_CRM_102` | Đối soát chứng từ đơn — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_CRM_103` | Xử lý khiếu nại đơn hàng — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 174** | `UC_CRM_105` | Báo cáo năng suất Sales Admin — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_CRM_106` | Quản lý hợp đồng bán — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_CRM_107` | Đính kèm file hợp đồng — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_CRM_108` | Theo dõi hiệu lực / tái tục — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 175** | `UC_CRM_111` | Chặn bán khi vượt công nợ — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_CRM_114` | Chuyển ticket sang FSM — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_CRM_115` | Lịch chăm sóc / nhắc tái mua — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_CRM_116` | Chương trình loyalty — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 176** | `UC_CRM_117` | Tích điểm / đổi quà — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_CRM_118` | Khảo sát hài lòng — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_CRM_119` | Báo cáo retention / tái mua — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_CRM_120` | Cấu hình rule hoa hồng — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 177** | `UC_CRM_121` | Tính hoa hồng theo kỳ — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_CRM_122` | Duyệt bảng hoa hồng — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_CRM_123` | Đồng bộ hoa hồng sang HRM/FIN — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_CRM_125` | Bảng xếp hạng sales — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 178** | `UC_CRM_130` | Báo cáo công nợ bán — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_CRM_131` | Xuất báo cáo định kỳ — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_POS_004` | Cấu hình máy in bếp/khu vực — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_POS_005` | Cấu hình ngăn kéo tiền — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 179** | `UC_POS_006` | Cấu hình thiết bị quét mã — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_POS_008` | Chế độ offline tạm — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_POS_011` | Thuộc tính sản phẩm — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_POS_013` | Ảnh sản phẩm / thứ tự hiển thị — Khởi tạo Entity, Migration, API & UI       |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 180** | `UC_POS_017` | Giá theo khung giờ — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_POS_018` | Giá theo ngày trong tuần — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_POS_020` | Làm tròn tiền — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_POS_023` | Khuyến mại theo combo — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 181** | `UC_POS_025` | Báo cáo khuyến mại — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_POS_028` | Tách bill / gộp bill — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_POS_029` | Chuyển đơn giữa quầy — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_POS_030` | Ghi chú đơn hàng — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 182** | `UC_POS_031` | Gửi lệnh khu vực chế biến — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_POS_036` | Thanh toán hỗn hợp — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_POS_041` | Gợi ý bán kèm — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_POS_044` | Nộp tiền / rút tiền ca — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 183** | `UC_POS_049` | Duyệt xác nhận ca — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_POS_050` | Gắn khách hàng vào đơn — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_POS_051` | Tích điểm loyalty — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_POS_052` | Đổi điểm / ưu đãi — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 184** | `UC_POS_053` | Tra cứu lịch sử mua — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_POS_056` | Tạo đề nghị nhập hàng — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_POS_057` | Nhận hàng từ kho trung tâm — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_POS_058` | Kiểm kê nhanh — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 185** | `UC_POS_060` | Đồng bộ đơn sang CRM — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_PUR_002` | Phân loại nhóm nhà cung cấp — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_PUR_004` | Lead time & MOQ — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_PUR_005` | Đánh giá chất lượng nhà cung cấp — Khởi tạo Entity, Migration, API & UI     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 186** | `UC_PUR_006` | Blacklist / ngưng dùng — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_PUR_007` | Import danh sách nhà cung cấp — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_PUR_008` | Hồ sơ pháp lý — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_PUR_011` | Hiệu lực bảng giá mua — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 187** | `UC_PUR_012` | Lịch sử giá mua — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_PUR_013` | Cảnh báo tăng giá bất thường — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_PUR_016` | Gộp nhiều nhu cầu thành PR — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_PUR_021` | Tạo RFQ gửi nhiều nhà cung cấp — Khởi tạo Entity, Migration, API & UI       |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 188** | `UC_PUR_022` | Nhập báo giá từ nhà cung cấp — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_PUR_023` | So sánh giá / điều kiện — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_PUR_024` | Chọn nhà cung cấp thắng — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_PUR_029` | Xác nhận PO từ nhà cung cấp — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 189** | `UC_PUR_036` | Từ chối lô hàng không đạt — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_PUR_038` | Trả hàng nhà cung cấp — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_PUR_039` | Biên bản giao nhận — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PUR_042` | Xử lý chênh lệch — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 190** | `UC_PUR_044` | Tạm ứng nhà cung cấp — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_PUR_045` | Hợp đồng mua khung — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PUR_046` | Theo dõi sản lượng / giá trị còn lại — Khởi tạo Entity, Migration, API & UI |  0% ➔ 90%  |
|              | `UC_PUR_047` | Cảnh báo hết hạn hợp đồng — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 191** | `UC_PUR_049` | Báo cáo đúng hạn giao hàng — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_PUR_050` | Báo cáo tiết kiệm từ RFQ — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_INV_006` | Ảnh & mô tả sản phẩm — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_INV_009` | Barcode / QR theo sản phẩm — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 192** | `UC_INV_013` | Vị trí / kệ / bin — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_INV_021` | Nhập trả từ khách — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_INV_023` | In tem lô / serial — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_INV_027` | Xuất cho dịch vụ kỹ thuật — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 193** | `UC_INV_028` | Xuất cho dự án — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|              | `UC_INV_032` | Duyệt chuyển kho — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_INV_034` | Chuyển kho một bước — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_INV_046` | Theo dõi serial — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 194** | `UC_INV_047` | Truy vết lô xuôi/ngược — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_INV_051` | Kiểm kê theo vị trí / nhóm — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_INV_054` | Khóa giao dịch khi đang kiểm kê — Khởi tạo Entity, Migration, API & UI      |  0% ➔ 90%  |
|              | `UC_INV_056` | Đề nghị xuất nội bộ — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 195** | `UC_INV_057` | Đề nghị cấp hàng — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_INV_058` | Duyệt đề nghị — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_INV_059` | Chuyển đề nghị thành phiếu xuất — Khởi tạo Entity, Migration, API & UI      |  0% ➔ 90%  |
|              | `UC_INV_066` | Hàng chậm luân chuyển — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 196** | `UC_INV_068` | Báo cáo xuất theo mục đích — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_LOG_002` | Danh mục tài xế / xe — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_LOG_003` | Bảng giá cước vận chuyển — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_LOG_004` | Cấu hình khu vực giao — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 197** | `UC_LOG_005` | Cấu hình ca giao hàng — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_LOG_007` | Gộp nhiều đơn thành chuyến — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_LOG_016` | Chứng từ ký nhận (POD) — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_LOG_018` | Hẹn giao lại — Khởi tạo Entity, Migration, API & UI                         |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 198** | `UC_LOG_019` | Theo dõi realtime trên bản đồ — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_LOG_031` | Lệnh giao nội bộ — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_LOG_032` | Xác nhận nhận hàng — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_LOG_033` | Đối soát giao nội bộ — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 199** | `UC_LOG_036` | Năng suất tài xế / chuyến — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_LOG_037` | Chi phí vận chuyển — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_MFG_004` | Danh mục công đoạn — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_MFG_005` | Ca sản xuất / năng lực — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 200** | `UC_MFG_009` | Định mức hao hụt — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_MFG_011` | Sao chép BOM — Khởi tạo Entity, Migration, API & UI                         |  0% ➔ 90%  |
|              | `UC_MFG_012` | Kế hoạch SX theo nhu cầu — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_MFG_014` | Tính nhu cầu nguyên vật liệu — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 201** | `UC_MFG_016` | Lịch SX theo xưởng/ca — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_MFG_021` | Ghi nhận tiến độ công đoạn — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_MFG_026` | Lệnh sản xuất lại — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_MFG_028` | Phân bổ nhân công / chi phí chung — Khởi tạo Entity, Migration, API & UI    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 202** | `UC_MFG_030` | Đối chiếu lý thuyết vs thực tế — Khởi tạo Entity, Migration, API & UI       |  0% ➔ 90%  |
|              | `UC_MFG_032` | Tiêu chí QC đầu vào — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_MFG_033` | QC thành phẩm — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_MFG_034` | Ghi nhận lô đạt / không đạt — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 203** | `UC_MFG_035` | Cách ly hàng lỗi — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_MFG_036` | Báo cáo tỷ lệ đạt QC — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_MFG_037` | Lô/mẻ sản xuất — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|              | `UC_MFG_038` | Ghi nhận thông số mẻ — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 204** | `UC_MFG_039` | Đóng gói & gắn tem — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_MFG_040` | Định mức phối trộn — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_MFG_044` | Hiệu suất / OEE — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_FSM_004` | Bảng giá dịch vụ — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 205** | `UC_FSM_006` | Kỹ năng / chứng chỉ kỹ thuật viên — Khởi tạo Entity, Migration, API & UI    |  0% ➔ 90%  |
|              | `UC_FSM_007` | Vùng phụ trách — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|              | `UC_FSM_011` | Cảnh báo hết hạn bảo hành — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_FSM_012` | Hợp đồng bảo trì định kỳ — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 206** | `UC_FSM_016` | Phân công theo rule — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_FSM_021` | Checklist công việc — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_FSM_023` | Chụp ảnh trước/sau — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_FSM_025` | Hoàn linh kiện thừa — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 207** | `UC_FSM_026` | Ghi nhận phí sửa chữa — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_FSM_029` | Đánh giá dịch vụ — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_FSM_031` | Tái mở ticket — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_FSM_032` | Chuyển chi phí sang FIN — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 208** | `UC_FSM_033` | Lịch bảo trì theo thiết bị — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_FSM_034` | Tự tạo ticket bảo trì đến hạn — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_FSM_035` | Checklist bảo trì chuẩn — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_FSM_036` | Báo cáo thực hiện bảo trì — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 209** | `UC_FSM_040` | Cảnh báo thất thoát — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_FSM_043` | Làm việc offline — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_FSM_044` | Nộp quyết toán ngày — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_FSM_048` | Tỷ lệ sửa lần đầu — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 210** | `UC_FSM_049` | Báo cáo bảo hành — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_PJM_003` | Mẫu checklist nghiệm thu — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_PJM_016` | Gantt / timeline dự án — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_PJM_018` | Nhật ký thay đổi kế hoạch — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 211** | `UC_PJM_020` | Timesheet theo dự án — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_PJM_024` | Cảnh báo vượt ngân sách — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_PJM_025` | Checklist khảo sát — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PJM_026` | Checklist lắp đặt — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 212** | `UC_PJM_027` | Checklist bàn giao — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PJM_028` | Ghi nhận ảnh / biên bản — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_PJM_029` | Phát sinh change request — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_PJM_030` | Duyệt change request — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 213** | `UC_PJM_037` | Bảo hành sau dự án — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PJM_041` | Năng suất nguồn lực — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_FIN_005` | Đồng tiền hạch toán & tỷ giá — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_FIN_007` | Khoản mục thu/chi — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 214** | `UC_FIN_011` | Bút toán định kỳ / mẫu — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_FIN_017` | Đính kèm chứng từ gốc — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_FIN_021` | Đề nghị tạm ứng / hoàn ứng — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_FIN_022` | Kiểm kê quỹ — Khởi tạo Entity, Migration, API & UI                          |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 215** | `UC_FIN_028` | Import sao kê — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_FIN_033` | Bù trừ công nợ — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|              | `UC_FIN_034` | Nhắc nợ tự động — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_FIN_037` | Xử lý nợ khó đòi — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 216** | `UC_FIN_038` | Đối soát COD về AR — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_FIN_045` | Tạm ứng nhà cung cấp — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_FIN_046` | Đối soát 3 chiều — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_FIN_047` | Cấu hình nhà cung cấp HĐĐT — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 217** | `UC_FIN_048` | Phát hành hóa đơn điện tử — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_FIN_049` | Điều chỉnh / thay thế / hủy — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_FIN_050` | Tra cứu trạng thái phát hành — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_FIN_051` | Lưu trữ bảng kê HĐĐT — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 218** | `UC_FIN_054` | Tờ khai thuế GTGT — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_FIN_055` | Thuế TNCN từ lương — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_FIN_059` | Ghi nhận doanh thu dự án — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_FIN_061` | Doanh thu nhận trước — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 219** | `UC_FIN_064` | Phân bổ chi phí — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_FIN_066` | Chi phí marketing từ CRM — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_FIN_067` | Tạm ứng chi phí / quyết toán — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_FIN_070` | Checklist khóa sổ tháng — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 220** | `UC_FIN_072` | Lập ngân sách theo kỳ — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_FIN_073` | So sánh thực tế vs ngân sách — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_FIN_074` | Cảnh báo vượt ngân sách — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_FIN_075` | Phiên bản ngân sách — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 221** | `UC_AST_005` | Ảnh & tài liệu kèm — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_AST_006` | Import danh mục tài sản — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_AST_007` | In tem mã tài sản — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_AST_013` | Tạm dừng / điều chỉnh khấu hao — Khởi tạo Entity, Migration, API & UI       |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 222** | `UC_AST_019` | Ghi giảm do mất mát — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_AST_023` | Lịch bảo trì TSCĐ — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|              | `UC_AST_024` | Lịch sử sửa chữa — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_AST_025` | Cảnh báo tài sản sắp hết khấu hao — Khởi tạo Entity, Migration, API & UI    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 223** | `UC_AST_026` | Quản lý công cụ dụng cụ — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_AST_027` | Cấp phát công cụ cho nhân viên — Khởi tạo Entity, Migration, API & UI       |  0% ➔ 90%  |
|              | `UC_AST_028` | Thu hồi công cụ — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_AST_029` | Phân bổ chi phí công cụ — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 224** | `UC_AST_033` | Giá trị còn lại theo nhóm — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_WF_002`  | Độ ưu tiên & SLA nội bộ — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_WF_003`  | Mẫu công việc lặp lại — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_WF_008`  | Checklist trong task — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 225** | `UC_WF_011`  | Ủy thác / chuyển người làm — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_WF_013`  | Kanban theo nhóm/dự án — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_WF_015`  | Calendar công việc — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_WF_016`  | Workload theo người — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 226** | `UC_WF_018`  | Phân loại & định tuyến — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|              | `UC_WF_019`  | Escalate ticket quá hạn — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_WF_020`  | CSAT nội bộ — Khởi tạo Entity, Migration, API & UI                          |  0% ➔ 90%  |
|              | `UC_WF_021`  | Kiến thức / FAQ nội bộ — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 227** | `UC_WF_026`  | Phiên bản quy trình — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_WF_027`  | Mô phỏng / kiểm thử — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_WF_030`  | Duyệt hàng loạt — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_WF_035`  | Thu hồi chứng từ đang chờ — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 228** | `UC_WF_036`  | Thời gian duyệt trung bình — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_WF_037`  | Bottleneck cấp duyệt — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_WF_039`  | Năng suất hoàn thành — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_BI_004`  | Từ điển chỉ tiêu KPI — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 229** | `UC_BI_005`  | Nhật ký truy cập báo cáo — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_BI_009`  | Widget tồn – mua – giao — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_BI_011`  | Widget sales pipeline — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_BI_012`  | Tùy chỉnh bố cục theo role — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 230** | `UC_BI_017`  | Gửi báo cáo email định kỳ — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_BI_020`  | Cảnh báo realtime / digest — Khởi tạo Entity, Migration, API & UI           |  0% ➔ 90%  |
|              | `UC_BI_022`  | Đăng ký nhận cảnh báo — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_BI_023`  | Tạo báo cáo tùy chỉnh — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 231** | `UC_BI_024`  | Pivot / biểu đồ tương tác — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|              | `UC_BI_025`  | Chia sẻ báo cáo — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_BI_026`  | Xuất dataset đã lọc — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_BI_027`  | Dự báo doanh thu — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 232** | `UC_BI_028`  | Dự báo nhu cầu — Khởi tạo Entity, Migration, API & UI                       |  0% ➔ 90%  |
|              | `UC_BI_029`  | Phát hiện bất thường — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_BI_030`  | Tóm tắt insight bằng AI — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|              | `UC_PRT_004` | Quản lý nhiều liên hệ — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 233** | `UC_PRT_005` | Phân quyền liên hệ — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PRT_006` | Xác thực email/SĐT — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PRT_009` | Theo dõi vận đơn — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_PRT_010` | Tải hóa đơn / biên bản — Khởi tạo Entity, Migration, API & UI               |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 234** | `UC_PRT_011` | Yêu cầu trả hàng / khiếu nại — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|              | `UC_PRT_012` | Đặt hàng lại — Khởi tạo Entity, Migration, API & UI                         |  0% ➔ 90%  |
|              | `UC_PRT_013` | Tạo yêu cầu báo giá — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_PRT_017` | Thanh toán online — Khởi tạo Entity, Migration, API & UI                    |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 235** | `UC_PRT_018` | Đối chiếu sao kê — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_PRT_021` | Trao đổi / gửi ảnh — Khởi tạo Entity, Migration, API & UI                   |  0% ➔ 90%  |
|              | `UC_PRT_022` | Xem thiết bị đã mua — Khởi tạo Entity, Migration, API & UI                  |  0% ➔ 90%  |
|              | `UC_PRT_023` | Đặt lịch bảo trì — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 236** | `UC_PRT_024` | Đánh giá dịch vụ — Khởi tạo Entity, Migration, API & UI                     |  0% ➔ 90%  |
|              | `UC_PRT_025` | Xem catalogue / bảng giá — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_PRT_026` | Tải tài liệu kỹ thuật — Khởi tạo Entity, Migration, API & UI                |  0% ➔ 90%  |
|              | `UC_PRT_027` | Thông báo từ nhà cung cấp — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 237** | `UC_PRT_028` | Đăng ký nhận bản tin — Khởi tạo Entity, Migration, API & UI                 |  0% ➔ 90%  |
|              | `UC_PRT_029` | Đăng nhập portal nhà cung cấp — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_PRT_030` | Xem PO được gửi — Khởi tạo Entity, Migration, API & UI                      |  0% ➔ 90%  |
|              | `UC_PRT_031` | Xác nhận PO / ngày giao — Khởi tạo Entity, Migration, API & UI              |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 238** | `UC_PRT_032` | Gửi thông báo sẵn sàng giao — Khởi tạo Entity, Migration, API & UI          |  0% ➔ 90%  |
|              | `UC_PRT_033` | Xem công nợ phía nhà cung cấp — Khởi tạo Entity, Migration, API & UI        |  0% ➔ 90%  |
|              | `UC_PRT_034` | Portal đại lý — Khởi tạo Entity, Migration, API & UI                        |  0% ➔ 90%  |
|              | `UC_PRT_035` | Thống kê lượt dùng portal — Khởi tạo Entity, Migration, API & UI            |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
| **Bước 239** | `UC_PRT_036` | Quản trị nội dung portal — Khởi tạo Entity, Migration, API & UI             |  0% ➔ 90%  |
|              | `UC_PRT_038` | Nhật ký thao tác phía portal — Khởi tạo Entity, Migration, API & UI         |  0% ➔ 90%  |
|     ---      | ---          | ---                                                                         |    ---     |
