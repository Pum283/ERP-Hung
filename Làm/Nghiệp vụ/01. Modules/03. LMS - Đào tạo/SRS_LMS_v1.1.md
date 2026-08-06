# SRS-LMS-v1.1 — Đào tạo (Learning Management System)

> **Software Requirements Specification — Module LMS**
> Phiên bản chỉnh chu theo chuẩn SYS v1.1; đặc tả UC bảng 8 trường, phục vụ chốt nghiệp vụ trước khi code.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-LMS-v1.1` |
| Module | `LMS` — Đào tạo (Learning Management System) |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Lớp sản phẩm | Nhân sự & Đào tạo |
| Bán riêng | Có |
| Phụ thuộc bắt buộc | `SYS` |
| Khuyến nghị kèm | `HRM`, `CRM`, `FIN` |
| Số nhóm / UC | 11 nhóm / 74 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |

| Ver | Ngày | Người thực hiện | Mô tả | Trạng thái |
|---|---|---|---|---|
| 1.0 | 03/08/2026 | BA / Generator | Sinh từ catalog + meta | Thay thế |
| 1.1 | 03/08/2026 | BA / Solution | Viết lại đặc tả UC chuyên sâu + chuẩn Word (như SYS) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Tài liệu mô tả yêu cầu nghiệp vụ module **Đào tạo (Learning Management System)** (`LMS`), làm căn cứ thống nhất phạm vi, ước lượng, thiết kế và nghiệm thu trước khi triển khai source.

### 1.2. Vai trò sản phẩm
Module LMS quản lý chương trình/khóa học, lớp offline, học online, ngân hàng câu hỏi, thi–chứng chỉ, lộ trình theo vị trí, khảo sát tuân thủ và báo cáo đào tạo. Phục vụ đào tạo nội bộ và/hoặc đào tạo có thu phí.

### 1.3. Mục tiêu đo được
1. Chuẩn hóa nội dung và tiến độ đào tạo theo vị trí.
2. Hỗ trợ học offline + online trên cùng nền tảng.
3. Đo lường hoàn thành, điểm thi, chứng chỉ.
4. Đồng bộ chứng chỉ bắt buộc sang hồ sơ nhân sự (khi có HRM).

### 1.4. Đối tượng đọc
- Chủ sản phẩm / PO, Ban dự án
- Business Analyst, Solution Architect
- Tech Lead / QA Lead
- Presales & triển khai (đóng gói bán module)

---

## 2. Phạm vi

### 2.1. In Scope
- Catalog khóa học, nội dung, lớp, enrollment, online learning, exam, certificate, learning path, survey/acknowledge, LMS reports.

### 2.2. Out of Scope
- Tuyển dụng và tính lương (HRM).
- POS bán hàng vật lý tại quầy (POS).
- AI tutor nâng cao (phase sau).

### 2.3. Đóng gói bán
- **Bán riêng:** Có
- **Phụ thuộc bắt buộc:** `SYS`
- **Khuyến nghị kèm (E2E):** `HRM`, `CRM`, `FIN`
- Tính năng ngành hóa bằng cấu hình/template khi triển khai — không hard-code vào SRS gốc.

---

## 3. Tác nhân

| Tác nhân | Trách nhiệm chính |
|---|---|
| LMS Admin | Cấu hình catalog, quyền giảng viên, publish khóa |
| Instructor | Giảng dạy, điểm danh, phản hồi bài |
| Learner (NV nội bộ) | Học bắt buộc / đăng ký nội bộ |
| Learner (Khách) | Học viên bên ngoài mua khóa |
| HR Training | Gán lộ trình, theo dõi tuân thủ |
| Hệ thống | Mở khóa bài, chấm quiz, nhắc học, cấp chứng chỉ |

### 3.1. Phân tách trách nhiệm gợi ý
- Cấu hình master / rule: Admin module.
- Vận hành chứng từ hàng ngày: Officer / User nghiệp vụ.
- Duyệt: Manager / Approver qua WF (nếu bật).
- Hệ thống: job nhắc hạn, tính toán batch, đồng bộ sự kiện.

---

## 4. Thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| Enrollment | Ghi danh học viên vào khóa/lớp |
| Learning path | Lộ trình khóa bắt buộc/tùy chọn theo vị trí |
| Certificate | Chứng chỉ hoàn thành điện tử |
| Acknowledge | Xác nhận đã đọc quy định/SOP |
| Tenant | Không gian dữ liệu khách hàng trên SYS |
| RBAC | Phân quyền theo vai trò do SYS cấp |
| Data scope | Phạm vi dữ liệu (chi nhánh/đơn vị/kho…) được phép thao tác |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
SYS (Auth · RBAC · License · Org · Audit · File · Notify · Event)
        |
        +-- LMS (Đào tạo (Learning Management System))
                |-- Master / cấu hình
                |-- Chứng từ & quy trình
                +-- Báo cáo / sự kiện liên module
```

### 5.1. Nguyên tắc phụ thuộc
1. Module `LMS` **bắt buộc** chạy trên SYS (identity, permission, license, audit).
2. Menu/API `LMS` chỉ mở khi license module active.
3. Duyệt liên phòng ban ưu tiên qua WF nếu khách mua WF; không thì dùng duyệt nội module.
4. Sự kiện liên module đi qua bus SYS (tránh gọi chéo cứng không kiểm soát).

### 5.2. Tích hợp

| Hướng | Hệ thống / Module | Nội dung |
|---|---|---|
| Tích hợp | SYS | User, thông báo, file, license |
| Tích hợp | HRM | Gán lộ trình theo vị trí; đồng bộ chứng chỉ |
| Tích hợp | CRM/FIN | Doanh thu khóa học / upsell sau đào tạo |
| Tích hợp | Payment | Cổng thanh toán mua khóa online |

---

## 6. Catalog chức năng

**Tổng:** 11 nhóm · 74 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `LMS-01` | Danh mục nội dung đào tạo | 9 | 7 | 2 | 0 |
| 2 | `LMS-02` | Ngân hàng câu hỏi & đề thi | 6 | 3 | 3 | 0 |
| 3 | `LMS-03` | Lớp Offline | 7 | 5 | 2 | 0 |
| 4 | `LMS-04` | Mentoring | 5 | 1 | 3 | 1 |
| 5 | `LMS-05` | Học Online – học viên | 12 | 9 | 2 | 1 |
| 6 | `LMS-06` | Thi & chứng chỉ | 9 | 5 | 3 | 1 |
| 7 | `LMS-07` | Giảng viên & quản trị LMS | 7 | 3 | 3 | 1 |
| 8 | `LMS-08` | Khảo sát & xác nhận | 5 | 1 | 4 | 0 |
| 9 | `LMS-09` | Lộ trình đào tạo | 4 | 0 | 4 | 0 |
| 10 | `LMS-10` | Báo cáo LMS | 6 | 3 | 2 | 1 |
| 11 | `LMS-11` | AI hỗ trợ học tập | 4 | 0 | 0 | 4 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_LMS_001` | Danh mục nội dung đào tạo | Danh mục chương trình đào tạo | Must |
| `UC_LMS_002` | Danh mục nội dung đào tạo | Danh mục khóa học | Must |
| `UC_LMS_003` | Danh mục nội dung đào tạo | Phân loại khóa (online/offline/blended) | Must |
| `UC_LMS_004` | Danh mục nội dung đào tạo | Quản lý chương / bài học | Must |
| `UC_LMS_005` | Danh mục nội dung đào tạo | Upload video bài giảng | Must |
| `UC_LMS_006` | Danh mục nội dung đào tạo | Upload tài liệu PDF / slide | Must |
| `UC_LMS_007` | Danh mục nội dung đào tạo | Gắn tag kỹ năng / vị trí | Should |
| `UC_LMS_008` | Danh mục nội dung đào tạo | Phiên bản nội dung khóa học | Should |
| `UC_LMS_009` | Danh mục nội dung đào tạo | Ẩn / xuất bản khóa học | Must |
| `UC_LMS_010` | Ngân hàng câu hỏi & đề thi | Tạo ngân hàng câu hỏi | Must |
| `UC_LMS_011` | Ngân hàng câu hỏi & đề thi | Phân loại câu hỏi theo độ khó | Should |
| `UC_LMS_012` | Ngân hàng câu hỏi & đề thi | Tạo đề thi cố định | Must |
| `UC_LMS_013` | Ngân hàng câu hỏi & đề thi | Tạo đề thi random | Should |
| `UC_LMS_014` | Ngân hàng câu hỏi & đề thi | Cấu hình điểm đạt / số lần thi | Must |
| `UC_LMS_015` | Ngân hàng câu hỏi & đề thi | Thời gian làm bài & chống gian lận | Should |
| `UC_LMS_016` | Lớp Offline | Mở lớp đào tạo offline | Must |
| `UC_LMS_017` | Lớp Offline | Gán giảng viên / địa điểm / lịch | Must |
| `UC_LMS_018` | Lớp Offline | Tuyển sinh / ghi danh học viên | Must |
| `UC_LMS_019` | Lớp Offline | Điểm danh buổi học | Must |
| `UC_LMS_020` | Lớp Offline | Ghi nhận học phí | Should |
| `UC_LMS_021` | Lớp Offline | Đánh giá thực hành tại lớp | Should |
| `UC_LMS_022` | Lớp Offline | Đóng lớp & tổng kết | Must |
| `UC_LMS_023` | Mentoring | Gán mentor cho học viên | Must |
| `UC_LMS_024` | Mentoring | Checklist kèm cặp | Should |
| `UC_LMS_025` | Mentoring | Mentor ghi nhận tiến độ | Should |
| `UC_LMS_026` | Mentoring | Đánh giá mentor / học viên | Could |
| `UC_LMS_027` | Mentoring | Báo cáo hiệu quả mentoring | Should |
| `UC_LMS_028` | Học Online – học viên | Đăng ký tài khoản học viên | Must |
| `UC_LMS_029` | Học Online – học viên | Đăng nhập / quên mật khẩu | Must |
| `UC_LMS_030` | Học Online – học viên | Danh sách & chi tiết khóa | Must |
| `UC_LMS_031` | Học Online – học viên | Mua khóa / thanh toán online | Must |
| `UC_LMS_032` | Học Online – học viên | Kích hoạt bằng mã voucher | Should |
| `UC_LMS_033` | Học Online – học viên | Tự mở khóa sau thanh toán | Must |
| `UC_LMS_034` | Học Online – học viên | Xem video / tài liệu | Must |
| `UC_LMS_035` | Học Online – học viên | Đánh dấu hoàn thành bài học | Must |
| `UC_LMS_036` | Học Online – học viên | Tiếp tục học dở | Must |
| `UC_LMS_037` | Học Online – học viên | Theo dõi % tiến độ khóa | Must |
| `UC_LMS_038` | Học Online – học viên | Nhắc học tiếp | Should |
| `UC_LMS_039` | Học Online – học viên | Diễn đàn / bình luận | Later |
| `UC_LMS_040` | Thi & chứng chỉ | Làm quiz cuối chương | Must |
| `UC_LMS_041` | Thi & chứng chỉ | Thi cuối khóa | Must |
| `UC_LMS_042` | Thi & chứng chỉ | Chấm điểm tự động | Must |
| `UC_LMS_043` | Thi & chứng chỉ | Xem kết quả & đáp án | Should |
| `UC_LMS_044` | Thi & chứng chỉ | Điều kiện cấp chứng chỉ | Must |
| `UC_LMS_045` | Thi & chứng chỉ | Cấp chứng chỉ điện tử | Must |
| `UC_LMS_046` | Thi & chứng chỉ | Mã xác thực chứng chỉ | Should |
| `UC_LMS_047` | Thi & chứng chỉ | Thu hồi chứng chỉ | Could |
| `UC_LMS_048` | Thi & chứng chỉ | Đồng bộ chứng chỉ sang HRM | Should |
| `UC_LMS_049` | Giảng viên & quản trị LMS | Hồ sơ giảng viên | Must |
| `UC_LMS_050` | Giảng viên & quản trị LMS | Phân quyền giảng viên | Must |
| `UC_LMS_051` | Giảng viên & quản trị LMS | Theo dõi danh sách học viên | Must |
| `UC_LMS_052` | Giảng viên & quản trị LMS | Phản hồi bài tập | Should |
| `UC_LMS_053` | Giảng viên & quản trị LMS | Thống kê doanh thu theo khóa | Should |
| `UC_LMS_054` | Giảng viên & quản trị LMS | Chống chia sẻ tài khoản | Should |
| `UC_LMS_055` | Giảng viên & quản trị LMS | Chặn tải video | Could |
| `UC_LMS_056` | Khảo sát & xác nhận | Tạo khảo sát hiểu bài | Should |
| `UC_LMS_057` | Khảo sát & xác nhận | Khảo sát tuân thủ | Should |
| `UC_LMS_058` | Khảo sát & xác nhận | Xác nhận đã đọc nội quy | Must |
| `UC_LMS_059` | Khảo sát & xác nhận | Bắt buộc hoàn thành trước ca | Should |
| `UC_LMS_060` | Khảo sát & xác nhận | Báo cáo tỷ lệ xác nhận | Should |
| `UC_LMS_061` | Lộ trình đào tạo | Gán lộ trình theo chức danh | Should |
| `UC_LMS_062` | Lộ trình đào tạo | Tự gán khóa bắt buộc khi nhận việc | Should |
| `UC_LMS_063` | Lộ trình đào tạo | Theo dõi hoàn thành lộ trình | Should |
| `UC_LMS_064` | Lộ trình đào tạo | Cảnh báo quá hạn đào tạo | Should |
| `UC_LMS_065` | Báo cáo LMS | Dashboard tiến độ đào tạo | Must |
| `UC_LMS_066` | Báo cáo LMS | Báo cáo hoàn thành theo đơn vị | Must |
| `UC_LMS_067` | Báo cáo LMS | Báo cáo điểm thi / tỷ lệ đạt | Should |
| `UC_LMS_068` | Báo cáo LMS | Báo cáo học viên bỏ dở | Should |
| `UC_LMS_069` | Báo cáo LMS | Báo cáo hiệu quả khóa | Could |
| `UC_LMS_070` | Báo cáo LMS | Xuất báo cáo đào tạo | Must |
| `UC_LMS_071` | AI hỗ trợ học tập | Gợi ý khóa học tiếp theo | Later |
| `UC_LMS_072` | AI hỗ trợ học tập | Tóm tắt bài học bằng AI | Later |
| `UC_LMS_073` | AI hỗ trợ học tập | AI tạo quiz từ nội dung | Later |
| `UC_LMS_074` | AI hỗ trợ học tập | Trợ lý hỏi đáp | Later |

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

### 7.1. Danh mục nội dung đào tạo (`LMS-01`)

Nhóm **Danh mục nội dung đào tạo** gồm **9** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 7 |

**Bảng 1. Đặc tả Use Case "Danh mục chương trình đào tạo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_001 |
| **Tên Use Case** | Danh mục chương trình đào tạo |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Danh mục chương trình đào tạo" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Training program catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục chương trình đào tạo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục chương trình đào tạo» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục chương trình đào tạo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Danh mục chương trình đào tạo» trong nhóm Danh mục nội dung đào tạo.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Training program catalog).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Danh mục chương trình đào tạo».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục chương trình đào tạo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 2. Đặc tả Use Case "Danh mục khóa học"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_002 |
| **Tên Use Case** | Danh mục khóa học |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Danh mục khóa học" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course catalog |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh mục khóa học» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh mục khóa học» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh mục khóa học» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin chọn kỳ/ca/đối tượng cần khóa trong «Danh mục khóa học».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh mục khóa học» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 3. Đặc tả Use Case "Phân loại khóa (online/offline/blended)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_003 |
| **Tên Use Case** | Phân loại khóa (online/offline/blended) |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Phân loại khóa (online/offline/blended)" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Delivery mode |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại khóa (online/offline/blended)» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại khóa (online/offline/blended)» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại khóa (online/offline/blended)» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin chọn kỳ/ca/đối tượng cần khóa trong «Phân loại khóa (online/offline/blended)».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại khóa (online/offline/blended)» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 4. Đặc tả Use Case "Quản lý chương / bài học"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_004 |
| **Tên Use Case** | Quản lý chương / bài học |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Quản lý chương / bài học" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Curriculum structure |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Quản lý chương / bài học» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Quản lý chương / bài học» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Quản lý chương / bài học» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở danh mục quản lý «Quản lý chương / bài học» (đào tạo / khóa học / bài kiểm tra; nhóm «Danh mục nội dung đào tạo»).<br>2. Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.<br>3. Hệ thống validate mã duy nhất và tham chiếu đang dùng.<br>4. Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.<br>5. Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Quản lý chương / bài học» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 5. Đặc tả Use Case "Upload video bài giảng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_005 |
| **Tên Use Case** | Upload video bài giảng |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Upload video bài giảng" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Video content |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload video bài giảng» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload video bài giảng» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload video bài giảng» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở bản ghi liên quan và chọn «Upload video bài giảng».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload video bài giảng» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 6. Đặc tả Use Case "Upload tài liệu PDF / slide"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_006 |
| **Tên Use Case** | Upload tài liệu PDF / slide |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Upload tài liệu PDF / slide" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Learning materials |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Upload tài liệu PDF / slide» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Upload tài liệu PDF / slide» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Upload tài liệu PDF / slide» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở bản ghi liên quan và chọn «Upload tài liệu PDF / slide».<br>2. Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).<br>3. Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.<br>4. Ghi Audit; hiển thị file trên danh sách đính kèm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Upload tài liệu PDF / slide» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 7. Đặc tả Use Case "Gắn tag kỹ năng / vị trí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_007 |
| **Tên Use Case** | Gắn tag kỹ năng / vị trí |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Gắn tag kỹ năng / vị trí" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Skill tagging |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gắn tag kỹ năng / vị trí» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gắn tag kỹ năng / vị trí» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gắn tag kỹ năng / vị trí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Gắn tag kỹ năng / vị trí» trong nhóm Danh mục nội dung đào tạo.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Skill tagging).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Gắn tag kỹ năng / vị trí».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gắn tag kỹ năng / vị trí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 8. Đặc tả Use Case "Phiên bản nội dung khóa học"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_008 |
| **Tên Use Case** | Phiên bản nội dung khóa học |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Phiên bản nội dung khóa học" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Content versioning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phiên bản nội dung khóa học» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phiên bản nội dung khóa học» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phiên bản nội dung khóa học» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. LMS Admin chọn kỳ/ca/đối tượng cần khóa trong «Phiên bản nội dung khóa học».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phiên bản nội dung khóa học» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 9. Đặc tả Use Case "Ẩn / xuất bản khóa học"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_009 |
| **Tên Use Case** | Ẩn / xuất bản khóa học |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Ẩn / xuất bản khóa học" thuộc nhóm Danh mục nội dung đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course publish control |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ẩn / xuất bản khóa học» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ẩn / xuất bản khóa học» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ẩn / xuất bản khóa học» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin chọn kỳ/ca/đối tượng cần khóa trong «Ẩn / xuất bản khóa học».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ẩn / xuất bản khóa học» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

### 7.2. Ngân hàng câu hỏi & đề thi (`LMS-02`)

Nhóm **Ngân hàng câu hỏi & đề thi** gồm **6** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 10. Đặc tả Use Case "Tạo ngân hàng câu hỏi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_010 |
| **Tên Use Case** | Tạo ngân hàng câu hỏi |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Tạo ngân hàng câu hỏi" thuộc nhóm Ngân hàng câu hỏi & đề thi trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Question bank |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo ngân hàng câu hỏi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo ngân hàng câu hỏi» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo ngân hàng câu hỏi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở chức năng «Tạo ngân hàng câu hỏi» trong nhóm Ngân hàng câu hỏi & đề thi.<br>2. Hệ thống kiểm tra license `LMS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo ngân hàng câu hỏi» (Question bank).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo ngân hàng câu hỏi» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo ngân hàng câu hỏi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 11. Đặc tả Use Case "Phân loại câu hỏi theo độ khó"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_011 |
| **Tên Use Case** | Phân loại câu hỏi theo độ khó |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Phân loại câu hỏi theo độ khó" thuộc nhóm Ngân hàng câu hỏi & đề thi trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Question taxonomy |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân loại câu hỏi theo độ khó» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân loại câu hỏi theo độ khó» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân loại câu hỏi theo độ khó» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Phân loại câu hỏi theo độ khó» trong nhóm Ngân hàng câu hỏi & đề thi.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Question taxonomy).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân loại câu hỏi theo độ khó».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân loại câu hỏi theo độ khó» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 12. Đặc tả Use Case "Tạo đề thi cố định"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_012 |
| **Tên Use Case** | Tạo đề thi cố định |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Tạo đề thi cố định" thuộc nhóm Ngân hàng câu hỏi & đề thi trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Fixed exam |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đề thi cố định» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đề thi cố định» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đề thi cố định» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở chức năng «Tạo đề thi cố định» trong nhóm Ngân hàng câu hỏi & đề thi.<br>2. Hệ thống kiểm tra license `LMS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đề thi cố định» (Fixed exam).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đề thi cố định» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đề thi cố định» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 13. Đặc tả Use Case "Tạo đề thi random"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_013 |
| **Tên Use Case** | Tạo đề thi random |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Tạo đề thi random" thuộc nhóm Ngân hàng câu hỏi & đề thi trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Random exam generation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo đề thi random» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo đề thi random» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo đề thi random» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin mở chức năng «Tạo đề thi random» trong nhóm Ngân hàng câu hỏi & đề thi.<br>2. Hệ thống kiểm tra license `LMS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo đề thi random» (Random exam generation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo đề thi random» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo đề thi random» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 14. Đặc tả Use Case "Cấu hình điểm đạt / số lần thi"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_014 |
| **Tên Use Case** | Cấu hình điểm đạt / số lần thi |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Cấu hình điểm đạt / số lần thi" thuộc nhóm Ngân hàng câu hỏi & đề thi trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Passing criteria |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấu hình điểm đạt / số lần thi» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấu hình điểm đạt / số lần thi» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấu hình điểm đạt / số lần thi» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở màn hình cấu hình «Cấu hình điểm đạt / số lần thi» trong Ngân hàng câu hỏi & đề thi.<br>2. Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.<br>3. Người dùng thiết lập tham số (Passing criteria) và lưu nháp/áp dụng.<br>4. Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.<br>5. Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.<br>6. Thông báo hiệu lực cấu hình (ngay / từ kỳ sau). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấu hình điểm đạt / số lần thi» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 15. Đặc tả Use Case "Thời gian làm bài & chống gian lận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_015 |
| **Tên Use Case** | Thời gian làm bài & chống gian lận |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Thời gian làm bài & chống gian lận" thuộc nhóm Ngân hàng câu hỏi & đề thi trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Exam timer & proctoring |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thời gian làm bài & chống gian lận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thời gian làm bài & chống gian lận» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thời gian làm bài & chống gian lận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Thời gian làm bài & chống gian lận» trong nhóm Ngân hàng câu hỏi & đề thi.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Exam timer & proctoring).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thời gian làm bài & chống gian lận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thời gian làm bài & chống gian lận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.3. Lớp Offline (`LMS-03`)

Nhóm **Lớp Offline** gồm **7** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 5 |

**Bảng 16. Đặc tả Use Case "Mở lớp đào tạo offline"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_016 |
| **Tên Use Case** | Mở lớp đào tạo offline |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Mở lớp đào tạo offline" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Create training class |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mở lớp đào tạo offline» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mở lớp đào tạo offline» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mở lớp đào tạo offline» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở chức năng «Mở lớp đào tạo offline» trong nhóm Lớp Offline.<br>2. Hệ thống kiểm tra license `LMS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Mở lớp đào tạo offline» (Create training class).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Mở lớp đào tạo offline» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mở lớp đào tạo offline» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 17. Đặc tả Use Case "Gán giảng viên / địa điểm / lịch"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_017 |
| **Tên Use Case** | Gán giảng viên / địa điểm / lịch |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Gán giảng viên / địa điểm / lịch" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Schedule class |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán giảng viên / địa điểm / lịch» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán giảng viên / địa điểm / lịch» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán giảng viên / địa điểm / lịch» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin chọn đối tượng nguồn trong «Gán giảng viên / địa điểm / lịch».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán giảng viên / địa điểm / lịch» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 18. Đặc tả Use Case "Tuyển sinh / ghi danh học viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_018 |
| **Tên Use Case** | Tuyển sinh / ghi danh học viên |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Tuyển sinh / ghi danh học viên" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Class enrollment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tuyển sinh / ghi danh học viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tuyển sinh / ghi danh học viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tuyển sinh / ghi danh học viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Tuyển sinh / ghi danh học viên» trong nhóm Lớp Offline.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Class enrollment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tuyển sinh / ghi danh học viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tuyển sinh / ghi danh học viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 19. Đặc tả Use Case "Điểm danh buổi học"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_019 |
| **Tên Use Case** | Điểm danh buổi học |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Điểm danh buổi học" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Class attendance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điểm danh buổi học» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điểm danh buổi học» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điểm danh buổi học» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Điểm danh buổi học» trong nhóm Lớp Offline.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Class attendance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Điểm danh buổi học».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điểm danh buổi học» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 20. Đặc tả Use Case "Ghi nhận học phí"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_020 |
| **Tên Use Case** | Ghi nhận học phí |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Ghi nhận học phí" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Training fee tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Ghi nhận học phí» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Ghi nhận học phí» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Ghi nhận học phí» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Ghi nhận học phí» trong nhóm Lớp Offline.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Training fee tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Ghi nhận học phí».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Ghi nhận học phí» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 21. Đặc tả Use Case "Đánh giá thực hành tại lớp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_021 |
| **Tên Use Case** | Đánh giá thực hành tại lớp |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Đánh giá thực hành tại lớp" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Practical assessment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá thực hành tại lớp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá thực hành tại lớp» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá thực hành tại lớp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Đánh giá thực hành tại lớp» trong nhóm Lớp Offline.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Practical assessment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá thực hành tại lớp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá thực hành tại lớp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 22. Đặc tả Use Case "Đóng lớp & tổng kết"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_022 |
| **Tên Use Case** | Đóng lớp & tổng kết |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Đóng lớp & tổng kết" thuộc nhóm Lớp Offline trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Class completion |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đóng lớp & tổng kết» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đóng lớp & tổng kết» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đóng lớp & tổng kết» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Đóng lớp & tổng kết» trong nhóm Lớp Offline.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Class completion).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đóng lớp & tổng kết».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đóng lớp & tổng kết» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.4. Mentoring (`LMS-04`)

Nhóm **Mentoring** gồm **5** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 1 |

**Bảng 23. Đặc tả Use Case "Gán mentor cho học viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_023 |
| **Tên Use Case** | Gán mentor cho học viên |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Gán mentor cho học viên" thuộc nhóm Mentoring trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Mentor assignment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán mentor cho học viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán mentor cho học viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán mentor cho học viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner chọn đối tượng nguồn trong «Gán mentor cho học viên».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán mentor cho học viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 24. Đặc tả Use Case "Checklist kèm cặp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_024 |
| **Tên Use Case** | Checklist kèm cặp |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Checklist kèm cặp" thuộc nhóm Mentoring trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Mentoring checklist |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Checklist kèm cặp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Checklist kèm cặp» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Checklist kèm cặp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Checklist kèm cặp» trong nhóm Mentoring.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Mentoring checklist).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Checklist kèm cặp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Checklist kèm cặp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 25. Đặc tả Use Case "Mentor ghi nhận tiến độ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_025 |
| **Tên Use Case** | Mentor ghi nhận tiến độ |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Mentor ghi nhận tiến độ" thuộc nhóm Mentoring trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Progress tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mentor ghi nhận tiến độ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mentor ghi nhận tiến độ» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mentor ghi nhận tiến độ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Mentor ghi nhận tiến độ» trong nhóm Mentoring.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Progress tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mentor ghi nhận tiến độ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mentor ghi nhận tiến độ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 26. Đặc tả Use Case "Đánh giá mentor / học viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_026 |
| **Tên Use Case** | Đánh giá mentor / học viên |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Đánh giá mentor / học viên" thuộc nhóm Mentoring trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Two-way feedback |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh giá mentor / học viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh giá mentor / học viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh giá mentor / học viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Đánh giá mentor / học viên» trong nhóm Mentoring.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Two-way feedback).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh giá mentor / học viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh giá mentor / học viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 27. Đặc tả Use Case "Báo cáo hiệu quả mentoring"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_027 |
| **Tên Use Case** | Báo cáo hiệu quả mentoring |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Báo cáo hiệu quả mentoring" thuộc nhóm Mentoring trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Mentoring effectiveness |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo hiệu quả mentoring» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo hiệu quả mentoring» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo hiệu quả mentoring» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Instructor mở «Báo cáo hiệu quả mentoring» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Mentoring effectiveness); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo hiệu quả mentoring» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.5. Học Online – học viên (`LMS-05`)

Nhóm **Học Online – học viên** gồm **12** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 12 |
| Must | 9 |

**Bảng 28. Đặc tả Use Case "Đăng ký tài khoản học viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_028 |
| **Tên Use Case** | Đăng ký tài khoản học viên |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Đăng ký tài khoản học viên" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Learner registration |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng ký tài khoản học viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng ký tài khoản học viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng ký tài khoản học viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Đăng ký tài khoản học viên» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Learner registration).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng ký tài khoản học viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng ký tài khoản học viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 29. Đặc tả Use Case "Đăng nhập / quên mật khẩu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_029 |
| **Tên Use Case** | Đăng nhập / quên mật khẩu |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Đăng nhập / quên mật khẩu" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Learner authentication |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [Learner] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đăng nhập / quên mật khẩu» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đăng nhập / quên mật khẩu» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đăng nhập / quên mật khẩu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Đăng nhập / quên mật khẩu» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Learner authentication).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đăng nhập / quên mật khẩu».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đăng nhập / quên mật khẩu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 30. Đặc tả Use Case "Danh sách & chi tiết khóa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_030 |
| **Tên Use Case** | Danh sách & chi tiết khóa |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Danh sách & chi tiết khóa" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course catalog view |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Danh sách & chi tiết khóa» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Danh sách & chi tiết khóa» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Danh sách & chi tiết khóa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner chọn kỳ/ca/đối tượng cần khóa trong «Danh sách & chi tiết khóa».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Danh sách & chi tiết khóa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 31. Đặc tả Use Case "Mua khóa / thanh toán online"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_031 |
| **Tên Use Case** | Mua khóa / thanh toán online |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Mua khóa / thanh toán online" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course checkout |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mua khóa / thanh toán online» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mua khóa / thanh toán online» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mua khóa / thanh toán online» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner chọn kỳ/ca/đối tượng cần khóa trong «Mua khóa / thanh toán online».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mua khóa / thanh toán online» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 32. Đặc tả Use Case "Kích hoạt bằng mã voucher"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_032 |
| **Tên Use Case** | Kích hoạt bằng mã voucher |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Kích hoạt bằng mã voucher" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Activation code |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Kích hoạt bằng mã voucher» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Kích hoạt bằng mã voucher» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Kích hoạt bằng mã voucher» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Kích hoạt bằng mã voucher» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Activation code).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Kích hoạt bằng mã voucher».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Kích hoạt bằng mã voucher» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 33. Đặc tả Use Case "Tự mở khóa sau thanh toán"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_033 |
| **Tên Use Case** | Tự mở khóa sau thanh toán |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Tự mở khóa sau thanh toán" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Auto enrollment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tự mở khóa sau thanh toán» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tự mở khóa sau thanh toán» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tự mở khóa sau thanh toán» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner yêu cầu mở khóa đối tượng trong «Tự mở khóa sau thanh toán» kèm lý do.<br>2. Hệ thống kiểm tra quyền mở khóa đặc biệt và chính sách tenant.<br>3. Xác nhận mở khóa có giới hạn thời gian/phạm vi nếu cấu hình.<br>4. Ghi Audit bắt buộc (who/when/why); thông báo người liên quan.<br>5. Cho phép chỉnh sửa có kiểm soát rồi khóa lại. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tự mở khóa sau thanh toán» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 34. Đặc tả Use Case "Xem video / tài liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_034 |
| **Tên Use Case** | Xem video / tài liệu |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Xem video / tài liệu" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Content consumption |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem video / tài liệu» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem video / tài liệu» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem video / tài liệu» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner mở «Xem video / tài liệu» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Content consumption).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem video / tài liệu» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 35. Đặc tả Use Case "Đánh dấu hoàn thành bài học"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_035 |
| **Tên Use Case** | Đánh dấu hoàn thành bài học |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Đánh dấu hoàn thành bài học" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Mark lesson complete |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đánh dấu hoàn thành bài học» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đánh dấu hoàn thành bài học» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh dấu hoàn thành bài học» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Đánh dấu hoàn thành bài học» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Mark lesson complete).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đánh dấu hoàn thành bài học».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đánh dấu hoàn thành bài học» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 36. Đặc tả Use Case "Tiếp tục học dở"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_036 |
| **Tên Use Case** | Tiếp tục học dở |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Tiếp tục học dở" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Resume learning |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tiếp tục học dở» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tiếp tục học dở» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tiếp tục học dở» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Tiếp tục học dở» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Resume learning).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tiếp tục học dở».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tiếp tục học dở» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 37. Đặc tả Use Case "Theo dõi % tiến độ khóa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_037 |
| **Tên Use Case** | Theo dõi % tiến độ khóa |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Theo dõi % tiến độ khóa" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Progress tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi % tiến độ khóa» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi % tiến độ khóa» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi % tiến độ khóa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner chọn kỳ/ca/đối tượng cần khóa trong «Theo dõi % tiến độ khóa».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi % tiến độ khóa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 38. Đặc tả Use Case "Nhắc học tiếp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_038 |
| **Tên Use Case** | Nhắc học tiếp |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Nhắc học tiếp" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Learning reminders |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Nhắc học tiếp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Nhắc học tiếp» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhắc học tiếp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Nhắc học tiếp» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Learning reminders).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Nhắc học tiếp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Nhắc học tiếp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 39. Đặc tả Use Case "Diễn đàn / bình luận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_039 |
| **Tên Use Case** | Diễn đàn / bình luận |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Diễn đàn / bình luận" thuộc nhóm Học Online – học viên trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Discussion forum |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Diễn đàn / bình luận» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Diễn đàn / bình luận» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Diễn đàn / bình luận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Diễn đàn / bình luận» trong nhóm Học Online – học viên.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Discussion forum).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Diễn đàn / bình luận».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Diễn đàn / bình luận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.6. Thi & chứng chỉ (`LMS-06`)

Nhóm **Thi & chứng chỉ** gồm **9** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 9 |
| Must | 5 |

**Bảng 40. Đặc tả Use Case "Làm quiz cuối chương"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_040 |
| **Tên Use Case** | Làm quiz cuối chương |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Làm quiz cuối chương" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Chapter quiz |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Làm quiz cuối chương» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Làm quiz cuối chương» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Làm quiz cuối chương» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Làm quiz cuối chương» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Chapter quiz).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Làm quiz cuối chương».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Làm quiz cuối chương» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 41. Đặc tả Use Case "Thi cuối khóa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_041 |
| **Tên Use Case** | Thi cuối khóa |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Thi cuối khóa" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Final exam |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thi cuối khóa» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thi cuối khóa» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thi cuối khóa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner chọn kỳ/ca/đối tượng cần khóa trong «Thi cuối khóa».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thi cuối khóa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 42. Đặc tả Use Case "Chấm điểm tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_042 |
| **Tên Use Case** | Chấm điểm tự động |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Chấm điểm tự động" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Auto grading |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chấm điểm tự động» đã được cấu hình trong phạm vi data scope.<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chấm điểm tự động» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chấm điểm tự động» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Chấm điểm tự động» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Auto grading).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chấm điểm tự động».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chấm điểm tự động» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 43. Đặc tả Use Case "Xem kết quả & đáp án"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_043 |
| **Tên Use Case** | Xem kết quả & đáp án |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Xem kết quả & đáp án" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Result review |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xem kết quả & đáp án» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xem kết quả & đáp án» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem kết quả & đáp án» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner mở «Xem kết quả & đáp án» và nhập tiêu chí tìm kiếm/lọc.<br>2. Hệ thống áp permission + data scope, trả kết quả phân trang.<br>3. Người dùng xem chi tiết / timeline / pipeline theo nhu cầu (Result review).<br>4. Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xem kết quả & đáp án» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 44. Đặc tả Use Case "Điều kiện cấp chứng chỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_044 |
| **Tên Use Case** | Điều kiện cấp chứng chỉ |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Điều kiện cấp chứng chỉ" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Certificate criteria |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Điều kiện cấp chứng chỉ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Điều kiện cấp chứng chỉ» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Điều kiện cấp chứng chỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Điều kiện cấp chứng chỉ» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Certificate criteria).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Điều kiện cấp chứng chỉ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Điều kiện cấp chứng chỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 45. Đặc tả Use Case "Cấp chứng chỉ điện tử"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_045 |
| **Tên Use Case** | Cấp chứng chỉ điện tử |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Cấp chứng chỉ điện tử" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: E-certificate issuance |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cấp chứng chỉ điện tử» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cấp chứng chỉ điện tử» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cấp chứng chỉ điện tử» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Cấp chứng chỉ điện tử» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (E-certificate issuance).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Cấp chứng chỉ điện tử».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cấp chứng chỉ điện tử» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 46. Đặc tả Use Case "Mã xác thực chứng chỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_046 |
| **Tên Use Case** | Mã xác thực chứng chỉ |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Mã xác thực chứng chỉ" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Certificate verification |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Mã xác thực chứng chỉ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Mã xác thực chứng chỉ» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Mã xác thực chứng chỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Mã xác thực chứng chỉ» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Certificate verification).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Mã xác thực chứng chỉ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Mã xác thực chứng chỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 47. Đặc tả Use Case "Thu hồi chứng chỉ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_047 |
| **Tên Use Case** | Thu hồi chứng chỉ |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Thu hồi chứng chỉ" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Certificate revocation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thu hồi chứng chỉ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thu hồi chứng chỉ» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thu hồi chứng chỉ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Thu hồi chứng chỉ» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Certificate revocation).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Thu hồi chứng chỉ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thu hồi chứng chỉ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 48. Đặc tả Use Case "Đồng bộ chứng chỉ sang HRM"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_048 |
| **Tên Use Case** | Đồng bộ chứng chỉ sang HRM |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Đồng bộ chứng chỉ sang HRM" thuộc nhóm Thi & chứng chỉ trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Sync to employee profile |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Đồng bộ chứng chỉ sang HRM» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Đồng bộ chứng chỉ sang HRM» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đồng bộ chứng chỉ sang HRM» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Đồng bộ chứng chỉ sang HRM» trong nhóm Thi & chứng chỉ.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Sync to employee profile).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Đồng bộ chứng chỉ sang HRM».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Đồng bộ chứng chỉ sang HRM» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.7. Giảng viên & quản trị LMS (`LMS-07`)

Nhóm **Giảng viên & quản trị LMS** gồm **7** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 3 |

**Bảng 49. Đặc tả Use Case "Hồ sơ giảng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_049 |
| **Tên Use Case** | Hồ sơ giảng viên |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Hồ sơ giảng viên" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Instructor profile |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Hồ sơ giảng viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Hồ sơ giảng viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Hồ sơ giảng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Hồ sơ giảng viên» trong nhóm Giảng viên & quản trị LMS.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Instructor profile).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Hồ sơ giảng viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Hồ sơ giảng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 50. Đặc tả Use Case "Phân quyền giảng viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_050 |
| **Tên Use Case** | Phân quyền giảng viên |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Phân quyền giảng viên" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Instructor permissions |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phân quyền giảng viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phân quyền giảng viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phân quyền giảng viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Phân quyền giảng viên» trong nhóm Giảng viên & quản trị LMS.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Instructor permissions).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phân quyền giảng viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phân quyền giảng viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 51. Đặc tả Use Case "Theo dõi danh sách học viên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_051 |
| **Tên Use Case** | Theo dõi danh sách học viên |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Theo dõi danh sách học viên" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Student roster |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi danh sách học viên» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi danh sách học viên» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi danh sách học viên» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. Learner khởi tạo thao tác «Theo dõi danh sách học viên» trong nhóm Giảng viên & quản trị LMS.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Student roster).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi danh sách học viên».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi danh sách học viên» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 52. Đặc tả Use Case "Phản hồi bài tập"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_052 |
| **Tên Use Case** | Phản hồi bài tập |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Phản hồi bài tập" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Assignment feedback |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Phản hồi bài tập» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Phản hồi bài tập» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Phản hồi bài tập» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Phản hồi bài tập» trong nhóm Giảng viên & quản trị LMS.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Assignment feedback).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Phản hồi bài tập».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Phản hồi bài tập» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 53. Đặc tả Use Case "Thống kê doanh thu theo khóa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_053 |
| **Tên Use Case** | Thống kê doanh thu theo khóa |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Thống kê doanh thu theo khóa" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course revenue |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Thống kê doanh thu theo khóa» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Thống kê doanh thu theo khóa» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thống kê doanh thu theo khóa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. Instructor chọn kỳ/ca/đối tượng cần khóa trong «Thống kê doanh thu theo khóa».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Thống kê doanh thu theo khóa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 54. Đặc tả Use Case "Chống chia sẻ tài khoản"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_054 |
| **Tên Use Case** | Chống chia sẻ tài khoản |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Chống chia sẻ tài khoản" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Device limit enforcement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chống chia sẻ tài khoản» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chống chia sẻ tài khoản» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chống chia sẻ tài khoản» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Chống chia sẻ tài khoản» trong nhóm Giảng viên & quản trị LMS.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Device limit enforcement).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chống chia sẻ tài khoản».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chống chia sẻ tài khoản» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 55. Đặc tả Use Case "Chặn tải video"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_055 |
| **Tên Use Case** | Chặn tải video |
| **Tác nhân** | Instructor |
| **Mô tả chức năng** | Cho phép Instructor thực hiện chức năng "Chặn tải video" thuộc nhóm Giảng viên & quản trị LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Video download protection |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Instructor] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Chặn tải video» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Chặn tải video» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Chặn tải video» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Instructor khởi tạo thao tác «Chặn tải video» trong nhóm Giảng viên & quản trị LMS.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Video download protection).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Chặn tải video».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Chặn tải video» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.8. Khảo sát & xác nhận (`LMS-08`)

Nhóm **Khảo sát & xác nhận** gồm **5** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 5 |
| Must | 1 |

**Bảng 56. Đặc tả Use Case "Tạo khảo sát hiểu bài"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_056 |
| **Tên Use Case** | Tạo khảo sát hiểu bài |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Tạo khảo sát hiểu bài" thuộc nhóm Khảo sát & xác nhận trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Post-training survey |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tạo khảo sát hiểu bài» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tạo khảo sát hiểu bài» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo khảo sát hiểu bài» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Training mở chức năng «Tạo khảo sát hiểu bài» trong nhóm Khảo sát & xác nhận.<br>2. Hệ thống kiểm tra license `LMS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «Tạo khảo sát hiểu bài» (Post-training survey).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «Tạo khảo sát hiểu bài» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tạo khảo sát hiểu bài» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 57. Đặc tả Use Case "Khảo sát tuân thủ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_057 |
| **Tên Use Case** | Khảo sát tuân thủ |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Khảo sát tuân thủ" thuộc nhóm Khảo sát & xác nhận trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Compliance survey |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Khảo sát tuân thủ» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Khảo sát tuân thủ» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Khảo sát tuân thủ» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Training khởi tạo thao tác «Khảo sát tuân thủ» trong nhóm Khảo sát & xác nhận.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Compliance survey).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Khảo sát tuân thủ».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Khảo sát tuân thủ» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 58. Đặc tả Use Case "Xác nhận đã đọc nội quy"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_058 |
| **Tên Use Case** | Xác nhận đã đọc nội quy |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Xác nhận đã đọc nội quy" thuộc nhóm Khảo sát & xác nhận trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Policy acknowledgment |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xác nhận đã đọc nội quy» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xác nhận đã đọc nội quy» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xác nhận đã đọc nội quy» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. HR Training khởi tạo thao tác «Xác nhận đã đọc nội quy» trong nhóm Khảo sát & xác nhận.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Policy acknowledgment).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Xác nhận đã đọc nội quy».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xác nhận đã đọc nội quy» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 59. Đặc tả Use Case "Bắt buộc hoàn thành trước ca"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_059 |
| **Tên Use Case** | Bắt buộc hoàn thành trước ca |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Bắt buộc hoàn thành trước ca" thuộc nhóm Khảo sát & xác nhận trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Training gate before work |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Bắt buộc hoàn thành trước ca» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Bắt buộc hoàn thành trước ca» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Bắt buộc hoàn thành trước ca» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Training khởi tạo thao tác «Bắt buộc hoàn thành trước ca» trong nhóm Khảo sát & xác nhận.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Training gate before work).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Bắt buộc hoàn thành trước ca».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Bắt buộc hoàn thành trước ca» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 60. Đặc tả Use Case "Báo cáo tỷ lệ xác nhận"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_060 |
| **Tên Use Case** | Báo cáo tỷ lệ xác nhận |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Báo cáo tỷ lệ xác nhận" thuộc nhóm Khảo sát & xác nhận trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Compliance rate |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo tỷ lệ xác nhận» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo tỷ lệ xác nhận» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo tỷ lệ xác nhận» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Training mở «Báo cáo tỷ lệ xác nhận» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Compliance rate); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo tỷ lệ xác nhận» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.9. Lộ trình đào tạo (`LMS-09`)

Nhóm **Lộ trình đào tạo** gồm **4** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 61. Đặc tả Use Case "Gán lộ trình theo chức danh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_061 |
| **Tên Use Case** | Gán lộ trình theo chức danh |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Gán lộ trình theo chức danh" thuộc nhóm Lộ trình đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Learning path by role |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gán lộ trình theo chức danh» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gán lộ trình theo chức danh» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gán lộ trình theo chức danh» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Training chọn đối tượng nguồn trong «Gán lộ trình theo chức danh».<br>2. Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.<br>3. Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.<br>4. Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.<br>5. Cập nhật lịch/board và ghi Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gán lộ trình theo chức danh» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 62. Đặc tả Use Case "Tự gán khóa bắt buộc khi nhận việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_062 |
| **Tên Use Case** | Tự gán khóa bắt buộc khi nhận việc |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Tự gán khóa bắt buộc khi nhận việc" thuộc nhóm Lộ trình đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Auto-assign on hire |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tự gán khóa bắt buộc khi nhận việc» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).<br>• Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tự gán khóa bắt buộc khi nhận việc» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tự gán khóa bắt buộc khi nhận việc» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. HR Training chọn kỳ/ca/đối tượng cần khóa trong «Tự gán khóa bắt buộc khi nhận việc».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tự gán khóa bắt buộc khi nhận việc» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 63. Đặc tả Use Case "Theo dõi hoàn thành lộ trình"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_063 |
| **Tên Use Case** | Theo dõi hoàn thành lộ trình |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Theo dõi hoàn thành lộ trình" thuộc nhóm Lộ trình đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Path completion tracking |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Theo dõi hoàn thành lộ trình» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Theo dõi hoàn thành lộ trình» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Theo dõi hoàn thành lộ trình» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. HR Training khởi tạo thao tác «Theo dõi hoàn thành lộ trình» trong nhóm Lộ trình đào tạo.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (Path completion tracking).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Theo dõi hoàn thành lộ trình».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Theo dõi hoàn thành lộ trình» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 64. Đặc tả Use Case "Cảnh báo quá hạn đào tạo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_064 |
| **Tên Use Case** | Cảnh báo quá hạn đào tạo |
| **Tác nhân** | HR Training |
| **Mô tả chức năng** | Cho phép HR Training thực hiện chức năng "Cảnh báo quá hạn đào tạo" thuộc nhóm Lộ trình đào tạo trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Overdue training alert |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [HR Training] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Cảnh báo quá hạn đào tạo» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Cảnh báo quá hạn đào tạo» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Cảnh báo quá hạn đào tạo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Job hệ thống hoặc HR Training kích hoạt kiểm tra điều kiện «Cảnh báo quá hạn đào tạo».<br>2. Hệ thống quét dữ liệu theo rule cảnh báo (Overdue training alert).<br>3. Tập hợp đối tượng vi phạm/đến hạn trong data scope.<br>4. Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.<br>5. Ghi NotificationLog / lịch sử cảnh báo để truy vết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Cảnh báo quá hạn đào tạo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.10. Báo cáo LMS (`LMS-10`)

Nhóm **Báo cáo LMS** gồm **6** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 65. Đặc tả Use Case "Dashboard tiến độ đào tạo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_065 |
| **Tên Use Case** | Dashboard tiến độ đào tạo |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Dashboard tiến độ đào tạo" thuộc nhóm Báo cáo LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: LMS dashboard |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Dashboard tiến độ đào tạo» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Dashboard tiến độ đào tạo» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Dashboard tiến độ đào tạo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở «Dashboard tiến độ đào tạo» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (LMS dashboard); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Dashboard tiến độ đào tạo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 66. Đặc tả Use Case "Báo cáo hoàn thành theo đơn vị"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_066 |
| **Tên Use Case** | Báo cáo hoàn thành theo đơn vị |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Báo cáo hoàn thành theo đơn vị" thuộc nhóm Báo cáo LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Completion by org |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo hoàn thành theo đơn vị» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo hoàn thành theo đơn vị» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo hoàn thành theo đơn vị» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở «Báo cáo hoàn thành theo đơn vị» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Completion by org); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo hoàn thành theo đơn vị» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 67. Đặc tả Use Case "Báo cáo điểm thi / tỷ lệ đạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_067 |
| **Tên Use Case** | Báo cáo điểm thi / tỷ lệ đạt |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Báo cáo điểm thi / tỷ lệ đạt" thuộc nhóm Báo cáo LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Exam analytics |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo điểm thi / tỷ lệ đạt» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo điểm thi / tỷ lệ đạt» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo điểm thi / tỷ lệ đạt» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin mở «Báo cáo điểm thi / tỷ lệ đạt» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Exam analytics); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo điểm thi / tỷ lệ đạt» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 68. Đặc tả Use Case "Báo cáo học viên bỏ dở"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_068 |
| **Tên Use Case** | Báo cáo học viên bỏ dở |
| **Tác nhân** | Learner |
| **Mô tả chức năng** | Cho phép Learner thực hiện chức năng "Báo cáo học viên bỏ dở" thuộc nhóm Báo cáo LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Dropout analysis |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Learner] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo học viên bỏ dở» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo học viên bỏ dở» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo học viên bỏ dở» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. Learner mở «Báo cáo học viên bỏ dở» và chọn bộ lọc thời gian / đơn vị / tiêu chí.<br>2. Hệ thống kiểm tra quyền dataset và data scope.<br>3. Truy vấn và tổng hợp số liệu (Dropout analysis); hiển thị bảng/biểu đồ.<br>4. Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.<br>5. Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo học viên bỏ dở» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 69. Đặc tả Use Case "Báo cáo hiệu quả khóa"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_069 |
| **Tên Use Case** | Báo cáo hiệu quả khóa |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Báo cáo hiệu quả khóa" thuộc nhóm Báo cáo LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course engagement |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Báo cáo hiệu quả khóa» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Báo cáo hiệu quả khóa» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Báo cáo hiệu quả khóa» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. LMS Admin chọn kỳ/ca/đối tượng cần khóa trong «Báo cáo hiệu quả khóa».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Báo cáo hiệu quả khóa» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 70. Đặc tả Use Case "Xuất báo cáo đào tạo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_070 |
| **Tên Use Case** | Xuất báo cáo đào tạo |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Xuất báo cáo đào tạo" thuộc nhóm Báo cáo LMS trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Export training report |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Xuất báo cáo đào tạo» đã được cấu hình trong phạm vi data scope.<br>• User có quyền xem dữ liệu trong data scope tương ứng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Xuất báo cáo đào tạo» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. File/bản in đã được tạo; có nhật ký export.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xuất báo cáo đào tạo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.<br>• Tiêu chí chấp nhận AC5: Thuộc phạm vi Phase 1 / go-live tối thiểu của module. |
| **Kịch bản chính** | 1. LMS Admin mở «Xuất báo cáo đào tạo», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).<br>2. Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.<br>3. Sinh file/bản in theo mẫu «Xuất báo cáo đào tạo» (Export training report).<br>4. Ghi nhật ký export (ai/lúc nào/bộ lọc).<br>5. Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Xuất báo cáo đào tạo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

### 7.11. AI hỗ trợ học tập (`LMS-11`)

Nhóm **AI hỗ trợ học tập** gồm **4** use case của module `LMS`. Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, gắn RBAC/license SYS và data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 71. Đặc tả Use Case "Gợi ý khóa học tiếp theo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_071 |
| **Tên Use Case** | Gợi ý khóa học tiếp theo |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Gợi ý khóa học tiếp theo" thuộc nhóm AI hỗ trợ học tập trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: Course recommendation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Gợi ý khóa học tiếp theo» đã được cấu hình trong phạm vi data scope.<br>• Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`, `BR-LMS-LOCK-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Gợi ý khóa học tiếp theo» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gợi ý khóa học tiếp theo» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.<br>• Tiêu chí chấp nhận AC4: Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy. |
| **Kịch bản chính** | 1. LMS Admin chọn kỳ/ca/đối tượng cần khóa trong «Gợi ý khóa học tiếp theo».<br>2. Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).<br>3. Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].<br>4. Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.<br>5. Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Gợi ý khóa học tiếp theo» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại. |

**Bảng 72. Đặc tả Use Case "Tóm tắt bài học bằng AI"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_072 |
| **Tên Use Case** | Tóm tắt bài học bằng AI |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Tóm tắt bài học bằng AI" thuộc nhóm AI hỗ trợ học tập trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: AI content summary |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Tóm tắt bài học bằng AI» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Tóm tắt bài học bằng AI» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tóm tắt bài học bằng AI» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Tóm tắt bài học bằng AI» trong nhóm AI hỗ trợ học tập.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (AI content summary).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Tóm tắt bài học bằng AI».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Tóm tắt bài học bằng AI» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

**Bảng 73. Đặc tả Use Case "AI tạo quiz từ nội dung"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_073 |
| **Tên Use Case** | AI tạo quiz từ nội dung |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "AI tạo quiz từ nội dung" thuộc nhóm AI hỗ trợ học tập trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: AI quiz generation |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «AI tạo quiz từ nội dung» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «AI tạo quiz từ nội dung» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit. Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «AI tạo quiz từ nội dung» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin mở chức năng «AI tạo quiz từ nội dung» trong nhóm AI hỗ trợ học tập.<br>2. Hệ thống kiểm tra license `LMS`, permission và data scope; hiển thị form tạo mới.<br>3. Người dùng nhập/chọn các trường nghiệp vụ cho «AI tạo quiz từ nội dung» (AI quiz generation).<br>4. Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.<br>5. Lưu bản ghi/chứng từ «AI tạo quiz từ nội dung» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.<br>6. Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «AI tạo quiz từ nội dung» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.<br>7.1. Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu. |

**Bảng 74. Đặc tả Use Case "Trợ lý hỏi đáp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_LMS_074 |
| **Tên Use Case** | Trợ lý hỏi đáp |
| **Tác nhân** | LMS Admin |
| **Mô tả chức năng** | Cho phép LMS Admin thực hiện chức năng "Trợ lý hỏi đáp" thuộc nhóm AI hỗ trợ học tập trong module LMS — Đào tạo (Learning Management System). Mô tả chi tiết: AI learning assistant |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [LMS Admin] và được cấp quyền RBAC tương ứng.<br>• License module `LMS` đang hiệu lực trên tenant.<br>• Dữ liệu tham chiếu liên quan tới «Trợ lý hỏi đáp» đã được cấu hình trong phạm vi data scope. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-LMS-SCOPE-01`, `BR-LMS-AUD-01`.<br>• Hậu điều kiện: Kết quả nghiệp vụ của «Trợ lý hỏi đáp» được lưu nhất quán trong module `LMS`; có thể truy vết trên audit.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Trợ lý hỏi đáp» với dữ liệu hợp lệ trong data scope.<br>• Tiêu chí chấp nhận AC2: User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).<br>• Tiêu chí chấp nhận AC3: Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu. |
| **Kịch bản chính** | 1. LMS Admin khởi tạo thao tác «Trợ lý hỏi đáp» trong nhóm AI hỗ trợ học tập.<br>2. Hệ thống kiểm tra license `LMS`, quyền RBAC và tiền điều kiện nghiệp vụ (AI learning assistant).<br>3. Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «Trợ lý hỏi đáp».<br>4. Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.<br>5. Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.<br>6. Hiển thị kết quả thành công và trạng thái mới trên UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. User thiếu permission hoặc ngoài data scope khi gọi «Trợ lý hỏi đáp» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit. |

---

## 8. Workflow end-to-end

### WF-LMS-01 — Xây dựng và xuất bản khóa học

**Mục tiêu:** Có khóa học sẵn sàng để ghi danh

| Bước | Mô tả |
|---:|---|
| 1 | Tạo chương trình/khóa; cấu trúc chương–bài |
| 2 | Upload video/tài liệu; tạo ngân hàng câu hỏi |
| 3 | Cấu hình điều kiện hoàn thành & chứng chỉ |
| 4 | Review nội dung; xuất bản |
| 5 | Gán vào lộ trình vị trí (nếu có) |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-LMS-02 — Đào tạo bắt buộc cho nhân sự mới

**Mục tiêu:** NV hoàn thành khóa trước hạn

| Bước | Mô tả |
|---:|---|
| 1 | HRM/HR Training kích hoạt gán lộ trình khi nhận việc |
| 2 | Hệ thống tạo enrollment và gửi thông báo |
| 3 | Học viên học, làm quiz/thi |
| 4 | Đạt điều kiện → cấp chứng chỉ |
| 5 | Đồng bộ chứng chỉ sang hồ sơ HRM; cảnh báo quá hạn nếu chậm |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

### WF-LMS-03 — Lớp offline có thu phí / nội bộ

**Mục tiêu:** Mở lớp – ghi danh – điểm danh – tổng kết

| Bước | Mô tả |
|---:|---|
| 1 | Mở lớp, gán giảng viên/lịch/địa điểm |
| 2 | Ghi danh học viên; ghi nhận học phí nếu có |
| 3 | Điểm danh buổi; đánh giá thực hành |
| 4 | Đóng lớp; cấp chứng chỉ; ghi nhận doanh thu (FIN/CRM nếu có) |

**Điều kiện hoàn tất:** Các bước cốt lõi hoàn thành; trạng thái cuối nhất quán; có audit/trace.

---

## 9. Mô hình dữ liệu domain (logic)

> Mức conceptual — chưa phải thiết kế CSDL vật lý.

| Thực thể | Vai trò |
|---|---|
| `Program / Course / Lesson` | Cấu trúc nội dung |
| `QuestionBank / Exam` | Thi cử |
| `ClassSession / Enrollment` | Lớp & ghi danh |
| `LearningProgress` | Tiến độ học |
| `Certificate` | Chứng chỉ |
| `LearningPath` | Lộ trình |
| `Survey / Acknowledgement` | Khảo sát & xác nhận |

### 9.1. Kiểm soát dữ liệu
- Master tham chiếu từ module sở hữu / SYS, không nhân bản lệch.
- Chứng từ có vòng đời trạng thái rõ (Draft → Submitted → Approved → Posted/Closed…).
- Soft-delete / ngưng dùng là mặc định; hạn chế xóa cứng.
- Mọi bản ghi nghiệp vụ gắn `TenantId` và data scope phù hợp module `LMS`.

---

## 10. Quy tắc nghiệp vụ tổng hợp

- BR-LMS-01: Chỉ khóa đã publish mới cho ghi danh mới.
- BR-LMS-02: Chứng chỉ chỉ cấp khi đủ điều kiện hoàn thành cấu hình.
- BR-LMS-03: Khóa bắt buộc quá hạn phải cảnh báo quản lý/HR.
- BR-LMS-04: Học viên khách và nội bộ có thể tách chính sách giá/quyền.
- BR-LMS-GEN-01: Thao tác thay đổi dữ liệu phải thuộc data scope của user.
- BR-LMS-GEN-02: Chứng từ có mã duy nhất theo Sequence SYS (nếu áp dụng).
- BR-LMS-GEN-03: Sau khóa kỳ/chốt sổ (nếu có), chỉ điều chỉnh có kiểm soát + audit.

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Streaming | Xem video ổn định trên web/mobile; hỗ trợ resume |
| Bảo mật nội dung | Giới hạn thiết bị/phiên theo cấu hình gói |
| Hiệu năng | Dashboard tiến độ 10.000 enrollment truy vấn được phân trang |
| Usability | Form validate rõ; bảng lọc/phân trang; tiếng Việt |
| Reliability | Giao dịch quan trọng atomic; không mất chứng từ đã post |
| Audit | Truy vết who/when/before/after với thay đổi trọng yếu |

---

## 12. Tích hợp & sự kiện liên module

- Module `LMS` phát/nhận sự kiện qua SYS Event Bus theo catalog tích hợp mục 5.2.
- Lỗi đồng bộ phải retry được và có nhật ký; không im lặng nuốt lỗi.
- Khi tắt license module: chặn API/UI; **giữ dữ liệu** theo chính sách lưu trữ.

---

## 13. Phân quyền & bảo mật

| Nhóm quyền gợi ý | Mô tả |
|---|---|
| `lms.course.manage` | Quyền chức năng module |
| `lms.class.manage` | Quyền chức năng module |
| `lms.grade.manage` | Quyền chức năng module |
| `lms.learn.access` | Quyền chức năng module |
| `lms.certificate.issue` | Quyền chức năng module |
| `lms.report.view` | Quyền chức năng module |
| `lms.*.view` | Xem trong data scope |
| `lms.*.manage` | Tạo/sửa trong data scope |
| `lms.*.approve` | Duyệt chứng từ (nếu có) |

- Field-level security áp dụng cho dữ liệu nhạy cảm (lương, CCCD, giá vốn…).
- Mọi từ chối quyền ghi audit.

---

## 14. Báo cáo & KPI

| KPI / Báo cáo | Mục đích |
|---|---|
| % hoàn thành đào tạo bắt buộc đúng hạn | Theo dõi vận hành module |
| Tỷ lệ đạt thi lần 1 | Theo dõi vận hành module |
| Số giờ học / học viên | Theo dõi vận hành module |
| Doanh thu khóa (nếu bán ngoài) | Theo dõi vận hành module |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Nội dung video do khách tự cung cấp hoặc mua riêng; LMS là nền tảng quản trị học tập.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Sai cấu hình data scope → lộ dữ liệu | Cao | Role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Duyệt tắc nghẽn khi thiếu WF | Trung bình | Escalation / ủy quyền |

### 15.3. Câu hỏi cần chốt
1. Có cần SCORM/xAPI phase 1 không?
2. Portal học viên tách biệt PRT hay dùng chung UI LMS?

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
| Bản SRS này | `SRS_LMS_v1.1.md` / `.docx` |
| UC IDs | `UC_LMS_001` … |

---

*Hết tài liệu SRS-LMS-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang thiết kế source.*
