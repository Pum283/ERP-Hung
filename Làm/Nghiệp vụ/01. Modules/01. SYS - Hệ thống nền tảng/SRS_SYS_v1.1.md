# SRS-SYS-v1.1 — Hệ thống nền tảng (System Platform)

> **Software Requirements Specification — Module SYS**
> Phiên bản chỉnh chu sau rà soát; thay thế bản sinh tự động v1.0 cho module này.
> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.

---

## 0. Thông tin tài liệu & lịch sử

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `SRS-SYS-v1.1` |
| Module | `SYS` — Hệ thống nền tảng |
| Phiên bản | 1.1 |
| Ngày | 03/08/2026 |
| Phân loại | SRS nghiệp vụ (BA) |
| Đóng gói | Không bán riêng — luôn kèm mọi gói sản phẩm |
| Số nhóm / UC | 13 nhóm / 104 UC |
| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3 + SYS-13 MSG) |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Sinh hàng loạt từ generator | Thay thế |
| 1.1 | 03/08/2026 | Viết lại đặc tả UC + BR + workflow chuyên sâu | Chờ duyệt |
| 1.1.1 | 04/08/2026 | Thêm nhóm **SYS-13 Nhắn tin realtime** (10 UC) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Đặc tả đầy đủ yêu cầu module **SYS** — lớp nền bắt buộc của sản phẩm ERP — để thống nhất nghiệp vụ trước khi thiết kế kiến trúc source và lập trình.

### 1.2. Vai trò sản phẩm
SYS không phải module nghiệp vụ đầu cuối. SYS trả lời các câu hỏi:

1. **Ai** được vào hệ thống? (xác thực, phiên, 2FA/SSO)
2. **Được làm gì**? (RBAC permission)
3. **Được thấy dữ liệu nào**? (data scope chi nhánh/kho/phòng ban)
4. **Được dùng module nào**? (license / gói bán)
5. **Hệ thống vận hành chung ra sao**? (cấu hình, file, thông báo, **nhắn tin realtime**, audit, tích hợp)

Không có SYS ổn định thì không thể bán tách HRM/CRM/FIN… một cách an toàn.

### 1.3. Mục tiêu đo được
| Mục tiêu | Chỉ dẫn đo |
|---|---|
| Onboard tenant mới trong ngày | Workflow WF-SYS-01 hoàn tất |
| Không lộ module chưa mua | UC_SYS_049/050 pass kiểm thử xâm nhập cơ bản |
| Truy vết thay đổi quyền | 100% thay đổi role/permission có audit |
| Chống brute-force | Khóa sau N lần sai hoạt động đúng cấu hình |

### 1.4. Đối tượng đọc
Product Owner, BA, Architect, Tech Lead, QA, Presales/Implementation.

---

## 2. Phạm vi

### 2.1. In Scope
- Xác thực & phiên (password, OTP reset, 2FA, SSO khung)
- User lifecycle (CRUD mềm, invite, import, khóa)
- RBAC + data scope + field-level security khung
- Tổ chức: tenant, pháp nhân, chi nhánh, điểm bán, phòng ban, chức danh
- License module, quota, menu động, chặn API
- Settings, danh mục dùng chung, sequence chứng từ
- Notification multi-channel, file, import/export framework
- Audit/security log, event bus, API key, webhook, gateway email/SMS
- Đa ngôn ngữ & branding cơ bản

### 2.2. Out of Scope
- Nghiệp vụ chuyên môn HRM/CRM/FIN/POS/… (chỉ cung cấp nền tảng)
- Portal khách hàng (PRT) và nội dung marketing CMS
- BI self-service / kho dữ liệu phân tích (BI)
- IAM doanh nghiệp thay thế Okta/Azure AD toàn phần (SYS chỉ tích hợp OIDC)
- Sao lưu CSDL hạ tầng (thuộc vận hành DevOps)

### 2.3. Đóng gói bán
| Tiêu chí | Quy định |
|---|---|
| Bán riêng | Không |
| Đi kèm | Mọi gói Starter → Full |
| Tắt được? | Không tắt SYS |
| Upsell liên quan | Quota user/chi nhánh; SSO; SMS; scanner file |

---

## 3. Tác nhân

| Tác nhân | Mô tả |
|---|---|
| System Admin | Quản trị tenant: user, org, license, cấu hình, tích hợp |
| Security Admin | Password policy, 2FA, RBAC, audit, IP policy |
| Org Admin | Chi nhánh / điểm bán / phòng ban trong phạm vi được ủy quyền |
| End User | Đăng nhập, hồ sơ cá nhân, thông báo, đổi mật khẩu |
| Integration Account | Máy-máy qua API Key |
| Hệ thống | Job, worker gửi mail/SMS, event bus, enforce license |

### 3.1. Phân tách trách nhiệm gợi ý
- Tenant nhỏ: một Super Admin giữ mọi quyền SYS.
- Tenant vừa/lớn: tách Security Admin khỏi admin vận hành thường ngày.
- Không để một user vừa là admin duy nhất vừa không bật 2FA trên môi trường production.

---

## 4. Thuật ngữ

| Thuật ngữ | Định nghĩa |
|---|---|
| Tenant | Không gian dữ liệu cách ly logic của một khách hàng thuê bao |
| Session / Token | Phiên đăng nhập hợp lệ để gọi API/UI |
| RBAC | Phân quyền dựa trên vai trò |
| Permission | Quyền nguyên tử dạng `domain.resource.action` |
| Data scope | Bộ lọc dữ liệu bắt buộc theo org/kho/điểm/phòng ban |
| License | Cam kết thương mại về module + hạn + quota |
| Sequence | Bộ sinh số chứng từ atomic |
| Event bus | Hàng đợi sự kiện nội bộ giữa các module |
| Soft-delete | Ngưng dùng nhưng giữ dữ liệu để truy vết |
| JIT provisioning | Tự tạo user khi đăng nhập SSO lần đầu (nếu bật) |

---

## 5. Ngữ cảnh kiến trúc nghiệp vụ

```text
Clients (Web/App)
       |  AuthN
       v
+---------------------------+
|            SYS            |
| Auth · RBAC · Data Scope  |
| License · Menu · Audit    |
| File · Notify · Event Bus |
+-------------+-------------+
              |
   +----------+----------+---------+
   v          v          v         v
 HRM/LMS   CRM/POS   PUR/INV/LOG  FIN/AST/...
```

### 5.1. Nguyên tắc phụ thuộc
1. Mọi module nghiệp vụ **bắt buộc** gọi SYS cho identity & authorization.
2. Module nghiệp vụ **đăng ký** permission + sequence + menu khi được bật.
3. Event xuyên module đi qua bus SYS (tránh gọi chéo chặt cứng không kiểm soát).
4. Enforce license ở **hai lớp**: UI menu và API middleware.

### 5.2. Tích hợp bên ngoài (khung)
| Loại | Ví dụ | UC liên quan |
|---|---|---|
| Email | SMTP / ESP | UC_SYS_088, 060 |
| SMS | SMS provider | UC_SYS_089, 061 |
| SSO | Google/Microsoft OIDC | UC_SYS_009 |
| Webhook | URL khách hàng | UC_SYS_085 |
| Push | FCM/APNs | UC_SYS_062 |

---

## 6. Catalog chức năng

**Tổng:** 13 nhóm · 104 use case.

| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |
|---:|---|---|---:|---:|---:|---:|
| 1 | `SYS-01` | Xác thực & phiên làm việc | 12 | 7 | 3 | 2 |
| 2 | `SYS-02` | Người dùng | 10 | 7 | 3 | 0 |
| 3 | `SYS-03` | Vai trò & phân quyền | 11 | 7 | 4 | 0 |
| 4 | `SYS-04` | Tổ chức & đa chi nhánh | 10 | 7 | 3 | 0 |
| 5 | `SYS-05` | License & module bán hàng | 7 | 7 | 0 | 0 |
| 6 | `SYS-06` | Cấu hình hệ thống | 8 | 5 | 2 | 1 |
| 7 | `SYS-07` | Thông báo | 7 | 3 | 3 | 1 |
| 8 | `SYS-08` | File & tài liệu | 6 | 2 | 3 | 1 |
| 9 | `SYS-09` | Import / Export | 6 | 4 | 1 | 1 |
| 10 | `SYS-10` | Audit & bảo mật | 6 | 3 | 2 | 1 |
| 11 | `SYS-11` | Tích hợp nền tảng | 7 | 2 | 5 | 0 |
| 12 | `SYS-12` | Đa ngôn ngữ & giao diện | 4 | 0 | 2 | 2 |
| 13 | `SYS-13` | Nhắn tin realtime | 10 | 5 | 3 | 2 |

<details>
<summary>Bảng mã UC đầy đủ</summary>

| Mã UC | Nhóm | Tên | Ưu tiên |
|---|---|---|---|
| `UC_SYS_001` | Xác thực & phiên làm việc | Đăng nhập hệ thống | Must |
| `UC_SYS_002` | Xác thực & phiên làm việc | Đăng xuất | Must |
| `UC_SYS_003` | Xác thực & phiên làm việc | Đổi mật khẩu | Must |
| `UC_SYS_004` | Xác thực & phiên làm việc | Quên mật khẩu – gửi OTP/link | Must |
| `UC_SYS_005` | Xác thực & phiên làm việc | Đặt lại mật khẩu sau OTP | Must |
| `UC_SYS_006` | Xác thực & phiên làm việc | Chính sách độ mạnh mật khẩu | Must |
| `UC_SYS_007` | Xác thực & phiên làm việc | Khóa tài khoản sau N lần sai | Must |
| `UC_SYS_008` | Xác thực & phiên làm việc | Xác thực 2 bước (2FA) | Should |
| `UC_SYS_009` | Xác thực & phiên làm việc | Đăng nhập SSO / OAuth | Could |
| `UC_SYS_010` | Xác thực & phiên làm việc | Quản lý phiên đang hoạt động | Should |
| `UC_SYS_011` | Xác thực & phiên làm việc | Giới hạn số phiên đồng thời | Should |
| `UC_SYS_012` | Xác thực & phiên làm việc | Ghi nhớ thiết bị tin cậy | Later |
| `UC_SYS_013` | Người dùng | Tạo người dùng | Must |
| `UC_SYS_014` | Người dùng | Cập nhật thông tin người dùng | Must |
| `UC_SYS_015` | Người dùng | Khóa / mở khóa người dùng | Must |
| `UC_SYS_016` | Người dùng | Xóa mềm người dùng | Must |
| `UC_SYS_017` | Người dùng | Gán người dùng vào chi nhánh | Must |
| `UC_SYS_018` | Người dùng | Reset mật khẩu bởi Admin | Must |
| `UC_SYS_019` | Người dùng | Mời người dùng qua email | Should |
| `UC_SYS_020` | Người dùng | Import danh sách người dùng Excel | Should |
| `UC_SYS_021` | Người dùng | Tìm kiếm / lọc người dùng | Must |
| `UC_SYS_022` | Người dùng | Xuất danh sách người dùng | Should |
| `UC_SYS_023` | Vai trò & phân quyền | Tạo / sửa / ngưng vai trò (Role) | Must |
| `UC_SYS_024` | Vai trò & phân quyền | Sao chép vai trò | Should |
| `UC_SYS_025` | Vai trò & phân quyền | Quản lý danh mục quyền (Permission) | Must |
| `UC_SYS_026` | Vai trò & phân quyền | Gán quyền vào vai trò | Must |
| `UC_SYS_027` | Vai trò & phân quyền | Gán người dùng vào vai trò | Must |
| `UC_SYS_028` | Vai trò & phân quyền | Phân quyền dữ liệu theo chi nhánh | Must |
| `UC_SYS_029` | Vai trò & phân quyền | Phân quyền dữ liệu theo kho / điểm | Must |
| `UC_SYS_030` | Vai trò & phân quyền | Phân quyền theo phòng ban | Should |
| `UC_SYS_031` | Vai trò & phân quyền | Quyền theo trường nhạy cảm | Should |
| `UC_SYS_032` | Vai trò & phân quyền | Xem ma trận phân quyền tổng hợp | Should |
| `UC_SYS_033` | Vai trò & phân quyền | Nhật ký thay đổi phân quyền | Must |
| `UC_SYS_034` | Tổ chức & đa chi nhánh | Quản lý công ty / tenant | Must |
| `UC_SYS_035` | Tổ chức & đa chi nhánh | Quản lý pháp nhân / công ty con | Should |
| `UC_SYS_036` | Tổ chức & đa chi nhánh | Quản lý chi nhánh | Must |
| `UC_SYS_037` | Tổ chức & đa chi nhánh | Quản lý điểm bán / cửa hàng | Must |
| `UC_SYS_038` | Tổ chức & đa chi nhánh | Quản lý phòng ban | Must |
| `UC_SYS_039` | Tổ chức & đa chi nhánh | Quản lý chức danh | Must |
| `UC_SYS_040` | Tổ chức & đa chi nhánh | Sơ đồ tổ chức trực quan | Should |
| `UC_SYS_041` | Tổ chức & đa chi nhánh | Cấu hình múi giờ / ngôn ngữ / tiền tệ | Must |
| `UC_SYS_042` | Tổ chức & đa chi nhánh | Cấu hình định dạng ngày số | Should |
| `UC_SYS_043` | Tổ chức & đa chi nhánh | Quản lý địa chỉ / tỉnh thành | Must |
| `UC_SYS_044` | License & module bán hàng | Khai báo module trong hệ thống | Must |
| `UC_SYS_045` | License & module bán hàng | Bật / tắt module theo tenant | Must |
| `UC_SYS_046` | License & module bán hàng | Quản lý gói license | Must |
| `UC_SYS_047` | License & module bán hàng | Giới hạn số user / chi nhánh theo gói | Must |
| `UC_SYS_048` | License & module bán hàng | Cảnh báo / gia hạn license | Must |
| `UC_SYS_049` | License & module bán hàng | Menu động theo module + quyền | Must |
| `UC_SYS_050` | License & module bán hàng | Ẩn API module chưa mua | Must |
| `UC_SYS_051` | Cấu hình hệ thống | Tham số cấu hình toàn cục | Must |
| `UC_SYS_052` | Cấu hình hệ thống | Cấu hình theo chi nhánh | Should |
| `UC_SYS_053` | Cấu hình hệ thống | Danh mục dùng chung | Must |
| `UC_SYS_054` | Cấu hình hệ thống | Mẫu số chứng từ | Must |
| `UC_SYS_055` | Cấu hình hệ thống | Sinh mã tự động | Must |
| `UC_SYS_056` | Cấu hình hệ thống | Cấu hình mẫu email / SMS | Must |
| `UC_SYS_057` | Cấu hình hệ thống | Cấu hình lịch làm việc | Should |
| `UC_SYS_058` | Cấu hình hệ thống | Quản lý phiên bản cấu hình | Could |
| `UC_SYS_059` | Thông báo | Thông báo in-app | Must |
| `UC_SYS_060` | Thông báo | Gửi email hệ thống | Must |
| `UC_SYS_061` | Thông báo | Gửi SMS / messaging | Should |
| `UC_SYS_062` | Thông báo | Push notification mobile | Should |
| `UC_SYS_063` | Thông báo | Cấu hình sự kiện kích hoạt thông báo | Must |
| `UC_SYS_064` | Thông báo | Tùy chọn thông báo cá nhân | Could |
| `UC_SYS_065` | Thông báo | Nhật ký gửi thông báo | Should |
| `UC_SYS_066` | File & tài liệu | Upload file | Must |
| `UC_SYS_067` | File & tài liệu | Tải xuống / xem trước file | Must |
| `UC_SYS_068` | File & tài liệu | Quản lý thư mục tài liệu | Should |
| `UC_SYS_069` | File & tài liệu | Phân quyền file theo đối tượng | Should |
| `UC_SYS_070` | File & tài liệu | Xóa mềm / khôi phục file | Should |
| `UC_SYS_071` | File & tài liệu | Quét virus / bảo mật file | Could |
| `UC_SYS_072` | Import / Export | Import Excel/CSV theo mẫu | Must |
| `UC_SYS_073` | Import / Export | Tải file mẫu import | Must |
| `UC_SYS_074` | Import / Export | Export Excel | Must |
| `UC_SYS_075` | Import / Export | Export PDF | Must |
| `UC_SYS_076` | Import / Export | Lịch sử job import/export | Should |
| `UC_SYS_077` | Import / Export | Xuất dữ liệu hàng loạt | Could |
| `UC_SYS_078` | Audit & bảo mật | Nhật ký thao tác người dùng | Must |
| `UC_SYS_079` | Audit & bảo mật | Nhật ký đăng nhập / thất bại | Must |
| `UC_SYS_080` | Audit & bảo mật | Xem chi tiết thay đổi field | Should |
| `UC_SYS_081` | Audit & bảo mật | Xuất audit log | Should |
| `UC_SYS_082` | Audit & bảo mật | Quản lý IP allow/deny | Later |
| `UC_SYS_083` | Audit & bảo mật | Chính sách hết hạn phiên | Must |
| `UC_SYS_084` | Tích hợp nền tảng | Quản lý API Key | Should |
| `UC_SYS_085` | Tích hợp nền tảng | Quản lý Webhook outbound | Should |
| `UC_SYS_086` | Tích hợp nền tảng | Nhật ký gọi API / webhook | Should |
| `UC_SYS_087` | Tích hợp nền tảng | Hàng đợi sự kiện liên module | Must |
| `UC_SYS_088` | Tích hợp nền tảng | Kết nối email gateway | Must |
| `UC_SYS_089` | Tích hợp nền tảng | Kết nối SMS gateway | Should |
| `UC_SYS_090` | Tích hợp nền tảng | Cấu hình tích hợp bên ngoài | Should |
| `UC_SYS_091` | Đa ngôn ngữ & giao diện | Quản lý gói ngôn ngữ | Should |
| `UC_SYS_092` | Đa ngôn ngữ & giao diện | Đổi ngôn ngữ giao diện | Should |
| `UC_SYS_093` | Đa ngôn ngữ & giao diện | Tùy chỉnh theme / logo | Could |
| `UC_SYS_094` | Đa ngôn ngữ & giao diện | Trang chủ theo vai trò | Could |
| `UC_SYS_095` | Nhắn tin realtime | Tạo hội thoại 1-1 | Must |
| `UC_SYS_096` | Nhắn tin realtime | Tạo hội thoại nhóm | Should |
| `UC_SYS_097` | Nhắn tin realtime | Gửi tin nhắn realtime | Must |
| `UC_SYS_098` | Nhắn tin realtime | Nhận tin nhắn realtime (SignalR) | Must |
| `UC_SYS_099` | Nhắn tin realtime | Xem lịch sử hội thoại | Must |
| `UC_SYS_100` | Nhắn tin realtime | Đánh dấu đã đọc / badge chưa đọc | Must |
| `UC_SYS_101` | Nhắn tin realtime | Đính kèm file trong tin nhắn | Should |
| `UC_SYS_102` | Nhắn tin realtime | Thu hồi tin nhắn | Should |
| `UC_SYS_103` | Nhắn tin realtime | Tìm kiếm tin nhắn | Could |
| `UC_SYS_104` | Nhắn tin realtime | Tắt thông báo hội thoại | Could |

</details>

### 6.1. Đề xuất Phase
| Phase | Phạm vi gợi ý |
|---|---|
| Phase 1 — Go-live nền | Toàn bộ **Must** (gồm SYS-13 nhắn tin realtime Must) |
| Phase 2 — An toàn & vận hành | Các **Should** (2FA, session manager, API key, webhook, SMS, field security…) |
| Phase 3 — Nâng cao | **Could/Later** (trusted device, IP allowlist, virus scan, bulk export, versioning cấu hình…) |

---

## 7. Đặc tả Use Case theo nhóm

Mỗi use case được đặc tả bằng **một bảng thống nhất** gồm 8 trường: Use Case ID, Tên Use Case, Tác nhân, Mô tả chức năng, Điều kiện tiên quyết, Yêu cầu, Kịch bản chính, Kịch bản phụ.

### 7.1. Xác thực & phiên làm việc (`SYS-01`)

Nhóm nền tảng an toàn truy cập: đăng nhập, phiên, mật khẩu, 2FA/SSO.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 12 |
| Must | 7 |

**Bảng 1. Đặc tả Use Case "Đăng nhập hệ thống"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_001 |
| **Tên Use Case** | Đăng nhập hệ thống |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đăng nhập hệ thống" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cho phép người dùng xác thực bằng username/email/SĐT + mật khẩu để nhận phiên làm việc. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [End User] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• Tenant đang hoạt động (chưa bị đình chỉ).<br>• User biết định danh đăng nhập (username/email/SĐT) đã được cấp.<br>• Tài khoản ở trạng thái Active (không bị khóa/xóa mềm). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-01`, `BR-SYS-AUTH-02`, `BR-SYS-SEC-01`.<br>• Hậu điều kiện: Phiên hợp lệ được tạo; người dùng vào được hệ thống đúng quyền. LoginLog ghi nhận IP, user-agent, thời điểm, kết quả.<br>• Tiêu chí chấp nhận AC1: Đăng nhập đúng credential → vào hệ thống < 3 giây trong điều kiện chuẩn.<br>• Tiêu chí chấp nhận AC2: Sai credential không cấp phiên.<br>• Tiêu chí chấp nhận AC3: Menu chỉ gồm module đang license và permission được gán. |
| **Kịch bản chính** | 1. Người dùng mở màn hình đăng nhập của tenant.<br>2. Nhập định danh + mật khẩu (có tùy chọn hiện/ẩn mật khẩu).<br>3. Hệ thống kiểm tra: tồn tại user, trạng thái, mật khẩu (hash), số lần sai, chính sách hết hạn mật khẩu.<br>4. Nếu bật 2FA và thiết bị chưa tin cậy → chuyển bước xác thực 2FA.<br>5. Cấp access token / refresh token (hoặc session tương đương); ghi LoginLog thành công.<br>6. Điều hướng trang chủ / landing theo role; tải menu theo license + permission. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Sai mật khẩu → tăng bộ đếm sai; thông báo chung (không tiết lộ user có tồn tại hay không — theo policy).<br>7.1. Vượt ngưỡng sai → khóa tạm tài khoản (UC_SYS_007).<br>8.1. User Locked/Disabled → từ chối kèm lý do phù hợp.<br>9.1. Tenant hết hạn license nền → chặn đăng nhập admin vận hành theo policy (trừ kênh gia hạn). |

**Bảng 2. Đặc tả Use Case "Đăng xuất"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_002 |
| **Tên Use Case** | Đăng xuất |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đăng xuất" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Thu hồi phiên hiện tại (và tùy chọn tất cả phiên) khi người dùng đăng xuất. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Người dùng đang có phiên hợp lệ. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-03`.<br>• Hậu điều kiện: Phiên hiện tại không còn dùng được để gọi API.<br>• Tiêu chí chấp nhận AC1: Sau đăng xuất, gọi API với token cũ trả 401.<br>• Tiêu chí chấp nhận AC2: Không còn truy cập được màn hình nội bộ. |
| **Kịch bản chính** | 1. Người dùng chọn Đăng xuất.<br>2. Hệ thống thu hồi access/refresh token (hoặc hủy server session) của phiên hiện tại.<br>3. Xóa cookie/local session phía client.<br>4. Ghi LoginLog/Audit sự kiện logout.<br>5. Chuyển về màn hình đăng nhập. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Token đã hết hạn vẫn đưa về màn hình đăng nhập an toàn (idempotent). |

**Bảng 3. Đặc tả Use Case "Đổi mật khẩu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_003 |
| **Tên Use Case** | Đổi mật khẩu |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đổi mật khẩu" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Người dùng đã đăng nhập đổi mật khẩu bằng cách xác nhận mật khẩu cũ và đặt mật khẩu mới theo policy. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập.<br>• Tài khoản cho phép login bằng mật khẩu (không purely SSO-only — trừ khi policy cho phép set local password). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-04`, `BR-SYS-AUTH-05`.<br>• Hậu điều kiện: Mật khẩu mới có hiệu lực; mật khẩu cũ không còn dùng được.<br>• Tiêu chí chấp nhận AC1: Đổi thành công với mật khẩu hợp lệ.<br>• Tiêu chí chấp nhận AC2: Mật khẩu yếu bị từ chối theo cấu hình tenant. |
| **Kịch bản chính** | 1. Mở form Đổi mật khẩu.<br>2. Nhập mật khẩu cũ, mật khẩu mới, xác nhận mật khẩu mới.<br>3. Hệ thống kiểm tra mật khẩu cũ đúng; mật khẩu mới đạt policy và không trùng N mật khẩu gần nhất.<br>4. Lưu hash mật khẩu mới; vô hiệu hóa các phiên khác (khuyến nghị); ghi audit.<br>5. Thông báo thành công; yêu cầu đăng nhập lại nếu policy bắt buộc. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Mật khẩu cũ sai → từ chối.<br>7.1. Mật khẩu mới không đạt policy → liệt kê rule vi phạm. |

**Bảng 4. Đặc tả Use Case "Quên mật khẩu – gửi OTP/link"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_004 |
| **Tên Use Case** | Quên mật khẩu – gửi OTP/link |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Quên mật khẩu – gửi OTP/link" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Khởi tạo quy trình đặt lại mật khẩu khi người dùng quên mật khẩu. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [End User] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• User nhớ định danh đăng nhập hoặc email/SĐT đã gắn.<br>• Kênh email/SMS đã cấu hình (ít nhất 1 kênh). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-06`.<br>• Hậu điều kiện: Có mã/OTP hiệu lực trong thời hạn cấu hình (ví dụ 15 phút).<br>• Tiêu chí chấp nhận AC1: User hợp lệ nhận được OTP/link.<br>• Tiêu chí chấp nhận AC2: OTP hết hạn không dùng được. |
| **Kịch bản chính** | 1. Người dùng chọn Quên mật khẩu, nhập định danh/email/SĐT.<br>2. Hệ thống tra cứu user (phản hồi trung tính nếu không tìm thấy — chống user enumeration theo policy).<br>3. Tạo token/OTP có thời hạn; lưu trạng thái Pending reset.<br>4. Gửi email link hoặc SMS OTP qua gateway.<br>5. Ghi NotificationLog + security log. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Gateway lỗi → báo thử lại; không để token dùng được nếu gửi thất bại (theo thiết kế). |

**Bảng 5. Đặc tả Use Case "Đặt lại mật khẩu sau OTP"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_005 |
| **Tên Use Case** | Đặt lại mật khẩu sau OTP |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đặt lại mật khẩu sau OTP" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Hoàn tất đặt lại mật khẩu sau khi xác thực OTP/link hợp lệ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [End User] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• Có token/OTP còn hiệu lực từ UC_SYS_004. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-04`, `BR-SYS-AUTH-06`.<br>• Hậu điều kiện: Đăng nhập được bằng mật khẩu mới; phiên cũ hết hiệu lực.<br>• Tiêu chí chấp nhận AC1: Reset thành công với OTP đúng.<br>• Tiêu chí chấp nhận AC2: Tái sử dụng OTP cũ thất bại. |
| **Kịch bản chính** | 1. Người dùng mở link hoặc nhập OTP.<br>2. Hệ thống xác thực token/OTP chưa dùng, chưa hết hạn.<br>3. Nhập mật khẩu mới + xác nhận theo policy.<br>4. Cập nhật mật khẩu; đánh dấu token đã dùng; thu hồi mọi phiên cũ.<br>5. Ghi audit; chuyển đăng nhập. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. OTP sai/hết hạn/đã dùng → từ chối.<br>7.1. Mật khẩu mới không đạt policy → từ chối. |

**Bảng 6. Đặc tả Use Case "Chính sách độ mạnh mật khẩu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_006 |
| **Tên Use Case** | Chính sách độ mạnh mật khẩu |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Chính sách độ mạnh mật khẩu" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấu hình và áp dụng rule độ mạnh mật khẩu cho tenant (độ dài, phức tạp, lịch sử, tuổi thọ). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Người dùng có quyền sys.setting.manage hoặc quyền bảo mật tương đương. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-04`.<br>• Hậu điều kiện: Policy được áp dụng nhất quán cho UC đổi/reset/tạo mật khẩu.<br>• Tiêu chí chấp nhận AC1: Đổi mật khẩu vi phạm policy bị chặn.<br>• Tiêu chí chấp nhận AC2: Admin xem được policy hiện hành. |
| **Kịch bản chính** | 1. Mở cấu hình Password Policy.<br>2. Thiết lập: min length, chữ hoa/thường/số/ký tự đặc biệt, số mật khẩu không được tái sử dụng, số ngày hết hạn (optional), thông báo trước hết hạn.<br>3. Lưu cấu hình; có hiệu lực với lần đổi/reset mật khẩu tiếp theo (và lần đăng nhập nếu buộc đổi).<br>4. Ghi audit thay đổi cấu hình. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Giá trị ngoài khoảng cho phép → validate lỗi. |

**Bảng 7. Đặc tả Use Case "Khóa tài khoản sau N lần sai"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_007 |
| **Tên Use Case** | Khóa tài khoản sau N lần sai |
| **Tác nhân** | Hệ thống / Security Admin |
| **Mô tả chức năng** | Cho phép Hệ thống / Security Admin thực hiện chức năng "Khóa tài khoản sau N lần sai" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tự động khóa tạm tài khoản khi vượt số lần đăng nhập sai; cho phép admin mở khóa. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống / Security Admin] và được cấp quyền RBAC tương ứng.<br>• Password policy có cấu hình ngưỡng N và thời gian khóa. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-02`.<br>• Hậu điều kiện: Tài khoản không đăng nhập được trong thời gian khóa.<br>• Tiêu chí chấp nhận AC1: Sai N lần liên tiếp → bị khóa.<br>• Tiêu chí chấp nhận AC2: Admin unlock thành công. |
| **Kịch bản chính** | 1. Mỗi lần đăng nhập sai tăng fail counter.<br>2. Khi đạt N: đặt trạng thái LockedTemporarily + thời điểm hết khóa (hoặc chờ admin).<br>3. Thông báo phù hợp cho user; ghi security log.<br>4. Admin có thể mở khóa thủ công; hoặc hệ thống tự mở sau thời gian cấu hình. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Đăng nhập đúng trong lúc bị khóa → vẫn từ chối đến khi hết khóa/admin mở. |

**Bảng 8. Đặc tả Use Case "Xác thực 2 bước (2FA)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_008 |
| **Tên Use Case** | Xác thực 2 bước (2FA) |
| **Tác nhân** | End User / Security Admin |
| **Mô tả chức năng** | Cho phép End User / Security Admin thực hiện chức năng "Xác thực 2 bước (2FA)" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Bật và xác thực lớp thứ hai bằng TOTP authenticator và/hoặc OTP SMS. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User / Security Admin] và được cấp quyền RBAC tương ứng.<br>• User đã vượt lớp mật khẩu.<br>• Admin đã cho phép 2FA (optional/bắt buộc theo role). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-07`.<br>• Hậu điều kiện: Chỉ cấp phiên đầy đủ khi 2FA thành công (nếu bật/bắt buộc).<br>• Tiêu chí chấp nhận AC1: Bật TOTP và đăng nhập thành công với mã đúng.<br>• Tiêu chí chấp nhận AC2: Mã sai không vào được hệ thống. |
| **Kịch bản chính** | 1. User bật 2FA: quét QR TOTP hoặc đăng ký SĐT nhận OTP.<br>2. Xác nhận mã lần đầu để kích hoạt.<br>3. Các lần đăng nhập sau: sau mật khẩu đúng → nhập mã 2FA.<br>4. Admin có thể reset 2FA khi mất thiết bị (có audit). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Mã 2FA sai quá số lần → tạm khóa bước 2FA/phiên đăng nhập.<br>7.1. Role bắt buộc 2FA mà chưa bật → buộc setup trước khi vào hệ thống. |

**Bảng 9. Đặc tả Use Case "Đăng nhập SSO / OAuth"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_009 |
| **Tên Use Case** | Đăng nhập SSO / OAuth |
| **Tác nhân** | End User / System Admin |
| **Mô tả chức năng** | Cho phép End User / System Admin thực hiện chức năng "Đăng nhập SSO / OAuth" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cho phép đăng nhập qua nhà cung cấp OIDC/OAuth (Google, Microsoft…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [End User / System Admin] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng.<br>• Admin đã cấu hình IdP (client id/secret/redirect/issuer).<br>• User có email map được với tài khoản nội bộ hoặc policy JIT provisioning được bật. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-08`.<br>• Hậu điều kiện: User vào hệ thống với quyền của tài khoản đã map.<br>• Tiêu chí chấp nhận AC1: SSO thành công với IdP cấu hình đúng.<br>• Tiêu chí chấp nhận AC2: User lạ bị từ chối khi JIT tắt. |
| **Kịch bản chính** | 1. User chọn Đăng nhập bằng IdP.<br>2. Redirect đến IdP; xác thực bên ngoài.<br>3. Callback về hệ thống với authorization code/token.<br>4. Map hoặc tạo user theo policy; cấp phiên nội bộ; ghi audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Email không map và JIT tắt → từ chối.<br>7.1. IdP lỗi/timeout → thông báo rõ. |

**Bảng 10. Đặc tả Use Case "Quản lý phiên đang hoạt động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_010 |
| **Tên Use Case** | Quản lý phiên đang hoạt động |
| **Tác nhân** | End User / Security Admin |
| **Mô tả chức năng** | Cho phép End User / Security Admin thực hiện chức năng "Quản lý phiên đang hoạt động" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xem danh sách phiên/thiết bị đang hoạt động và thu hồi từng phiên. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User / Security Admin] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-03`.<br>• Hậu điều kiện: Phiên bị thu hồi không gọi API được nữa.<br>• Tiêu chí chấp nhận AC1: User thấy phiên hiện tại.<br>• Tiêu chí chấp nhận AC2: Thu hồi phiên khác làm token đó hết hiệu lực. |
| **Kịch bản chính** | 1. Mở mục Phiên đăng nhập / Thiết bị.<br>2. Hiển thị danh sách: thiết bị, IP, thời điểm tạo/hoạt động gần nhất, phiên hiện tại.<br>3. User thu hồi 1 phiên hoặc tất cả phiên khác.<br>4. Admin (có quyền) có thể thu hồi phiên của user khác.<br>5. Ghi audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Thu hồi phiên hiện tại = đăng xuất. |

**Bảng 11. Đặc tả Use Case "Giới hạn số phiên đồng thời"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_011 |
| **Tên Use Case** | Giới hạn số phiên đồng thời |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Giới hạn số phiên đồng thời" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Giới hạn số phiên đồng thời trên mỗi user để giảm chia sẻ tài khoản. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Admin có quyền cấu hình bảo mật. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-09`.<br>• Hậu điều kiện: Số phiên active không vượt ngưỡng cấu hình.<br>• Tiêu chí chấp nhận AC1: Đăng nhập vượt max → áp dụng đúng policy (reject hoặc revoke oldest). |
| **Kịch bản chính** | 1. Cấu hình max concurrent sessions (theo tenant hoặc theo role).<br>2. Khi đăng nhập mới vượt ngưỡng: từ chối hoặc đá phiên cũ nhất (theo policy chọn).<br>3. Thông báo cho user về phiên bị thay thế (nếu có). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 12. Đặc tả Use Case "Ghi nhớ thiết bị tin cậy"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_012 |
| **Tên Use Case** | Ghi nhớ thiết bị tin cậy |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Ghi nhớ thiết bị tin cậy" thuộc nhóm Xác thực & phiên làm việc trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cho phép bỏ qua 2FA trong thời hạn trên thiết bị đã đánh dấu tin cậy. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• 2FA đã bật.<br>• Policy cho phép trusted device. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-07`.<br>• Hậu điều kiện: Thiết bị tin cậy còn hạn không bị hỏi 2FA.<br>• Tiêu chí chấp nhận AC1: Trusted device bỏ qua 2FA trong hạn.<br>• Tiêu chí chấp nhận AC2: Hết hạn phải 2FA lại. |
| **Kịch bản chính** | 1. Sau 2FA thành công, user chọn Tin cậy thiết bị này.<br>2. Hệ thống lưu device token gắn user + hạn dùng.<br>3. Lần sau: mật khẩu đúng + device token hợp lệ → bỏ qua 2FA. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Đổi mật khẩu / reset 2FA / admin revoke → xóa trusted devices. |

### 7.2. Người dùng (`SYS-02`)

Quản trị vòng đời tài khoản người dùng trong tenant.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 7 |

**Bảng 13. Đặc tả Use Case "Tạo người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_013 |
| **Tên Use Case** | Tạo người dùng |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Tạo người dùng" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tạo tài khoản người dùng mới trong tenant, gắn thông tin cơ bản và trạng thái ban đầu. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.user.manage.<br>• Chưa vượt quota user của license (nếu áp dụng). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-USER-01`, `BR-SYS-LIC-02`.<br>• Hậu điều kiện: User tồn tại và có thể được gán role.<br>• Tiêu chí chấp nhận AC1: Tạo user thành công với email duy nhất.<br>• Tiêu chí chấp nhận AC2: User trùng bị từ chối. |
| **Kịch bản chính** | 1. Mở form tạo user.<br>2. Nhập username/email/SĐT, họ tên, chi nhánh mặc định, trạng thái.<br>3. Validate trùng định danh.<br>4. Lưu user (Active/InvitePending).<br>5. Tùy chọn gửi invite/reset password.<br>6. Ghi audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Trùng email/username → lỗi.<br>7.1. Vượt quota → chặn kèm thông báo nâng gói. |

**Bảng 14. Đặc tả Use Case "Cập nhật thông tin người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_014 |
| **Tên Use Case** | Cập nhật thông tin người dùng |
| **Tác nhân** | System Admin / End User |
| **Mô tả chức năng** | Cho phép System Admin / End User thực hiện chức năng "Cập nhật thông tin người dùng" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cập nhật hồ sơ hiển thị của user (họ tên, SĐT, avatar…). End User chỉ sửa trường self-service cho phép. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin / End User] và được cấp quyền RBAC tương ứng.<br>• User tồn tại.<br>• Có quyền quản trị hoặc đang sửa chính mình với phạm vi field cho phép. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-USER-02`.<br>• Hậu điều kiện: Thông tin mới hiển thị đúng ở UI/API.<br>• Tiêu chí chấp nhận AC1: Admin cập nhật SĐT thành công.<br>• Tiêu chí chấp nhận AC2: User thường không sửa được username hệ thống nếu policy cấm. |
| **Kịch bản chính** | 1. Mở hồ sơ user.<br>2. Sửa các trường được phép.<br>3. Validate định dạng.<br>4. Lưu + audit field change. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Sửa field nhạy cảm không đủ quyền → 403. |

**Bảng 15. Đặc tả Use Case "Khóa / mở khóa người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_015 |
| **Tên Use Case** | Khóa / mở khóa người dùng |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Khóa / mở khóa người dùng" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Vô hiệu hóa hoặc kích hoạt lại khả năng đăng nhập của user mà không xóa dữ liệu. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.user.manage.<br>• Không phải tự khóa tài khoản admin cuối cùng theo rule an toàn. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-USER-03`.<br>• Hậu điều kiện: User bị khóa không đăng nhập được; mở khóa thì đăng nhập lại được.<br>• Tiêu chí chấp nhận AC1: Khóa user → login 403/denied.<br>• Tiêu chí chấp nhận AC2: Mở khóa → login được. |
| **Kịch bản chính** | 1. Chọn user → Khóa hoặc Mở khóa.<br>2. Xác nhận.<br>3. Cập nhật trạng thái; thu hồi phiên nếu khóa.<br>4. Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Không cho khóa hết toàn bộ System Admin của tenant. |

**Bảng 16. Đặc tả Use Case "Xóa mềm người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_016 |
| **Tên Use Case** | Xóa mềm người dùng |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Xóa mềm người dùng" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Ngưng sử dụng user bằng soft-delete; giữ lịch sử giao dịch và audit. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• User không còn cần truy cập.<br>• Có quyền sys.user.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-USER-01`.<br>• Hậu điều kiện: User không còn trong danh sách active; dữ liệu lịch sử vẫn truy vết được.<br>• Tiêu chí chấp nhận AC1: Soft-delete thành công.<br>• Tiêu chí chấp nhận AC2: Không còn đăng nhập được. |
| **Kịch bản chính** | 1. Chọn Xóa/Ngưng dùng.<br>2. Hệ thống chuyển Deleted/Inactive; giải phóng username theo policy (hoặc giữ).<br>3. Thu hồi phiên + API key của user.<br>4. Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Không xóa cứng nếu đã phát sinh chứng từ — chỉ soft-delete. |

**Bảng 17. Đặc tả Use Case "Gán người dùng vào chi nhánh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_017 |
| **Tên Use Case** | Gán người dùng vào chi nhánh |
| **Tác nhân** | System Admin / Org Admin |
| **Mô tả chức năng** | Cho phép System Admin / Org Admin thực hiện chức năng "Gán người dùng vào chi nhánh" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gán chi nhánh mặc định và danh sách chi nhánh được truy cập (nền tảng cho data scope). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin / Org Admin] và được cấp quyền RBAC tương ứng.<br>• Đã có master chi nhánh.<br>• Có quyền gán org cho user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-SCOPE-01`.<br>• Hậu điều kiện: Các module nghiệp vụ lọc dữ liệu theo scope chi nhánh của user.<br>• Tiêu chí chấp nhận AC1: User chỉ thấy dữ liệu chi nhánh được gán trong kịch bản kiểm thử mẫu. |
| **Kịch bản chính** | 1. Chọn user → tab Tổ chức.<br>2. Chọn chi nhánh mặc định + các chi nhánh bổ sung.<br>3. Lưu; tính lại data scope hiệu lực.<br>4. Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Chi nhánh ngưng dùng không gán mới được. |

**Bảng 18. Đặc tả Use Case "Reset mật khẩu bởi Admin"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_018 |
| **Tên Use Case** | Reset mật khẩu bởi Admin |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Reset mật khẩu bởi Admin" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Admin đặt lại mật khẩu hoặc gửi link reset cho user khi hỗ trợ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.user.manage.<br>• User tồn tại. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-05`, `BR-SYS-AUD-01`.<br>• Hậu điều kiện: User đăng nhập được bằng mật khẩu/link mới.<br>• Tiêu chí chấp nhận AC1: Có bản ghi audit reset.<br>• Tiêu chí chấp nhận AC2: User phải đổi mật khẩu tạm trước khi dùng bình thường. |
| **Kịch bản chính** | 1. Chọn Reset mật khẩu.<br>2. Chọn: đặt mật khẩu tạm hoặc gửi link.<br>3. Nếu mật khẩu tạm: bắt buộc đổi ở lần đăng nhập tiếp theo.<br>4. Audit bắt buộc (ai reset ai). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 19. Đặc tả Use Case "Mời người dùng qua email"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_019 |
| **Tên Use Case** | Mời người dùng qua email |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Mời người dùng qua email" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gửi lời mời kích hoạt tài khoản qua email có link hết hạn. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Email gateway đã cấu hình.<br>• User ở trạng thái InvitePending hoặc mới tạo. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-USER-04`.<br>• Hậu điều kiện: User kích hoạt thành công và đăng nhập được.<br>• Tiêu chí chấp nhận AC1: Link invite hết hạn không kích hoạt được.<br>• Tiêu chí chấp nhận AC2: Invite hợp lệ kích hoạt được. |
| **Kịch bản chính** | 1. Tạo/chọn user → Gửi lời mời.<br>2. Sinh invite token có hạn.<br>3. Gửi email.<br>4. User bấm link → đặt mật khẩu → Active. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Email bounce → trạng thái gửi thất bại trên log. |

**Bảng 20. Đặc tả Use Case "Import danh sách người dùng Excel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_020 |
| **Tên Use Case** | Import danh sách người dùng Excel |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Import danh sách người dùng Excel" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tạo hàng loạt user từ file Excel/CSV theo mẫu chuẩn. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền import.<br>• Trong hạn quota. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-USER-01`, `BR-SYS-IE-01`.<br>• Hậu điều kiện: Các user hợp lệ được tạo.<br>• Tiêu chí chấp nhận AC1: Import 10 user hợp lệ thành công.<br>• Tiêu chí chấp nhận AC2: Dòng trùng email bị báo lỗi rõ số dòng. |
| **Kịch bản chính** | 1. Tải mẫu → điền → upload.<br>2. Validate từng dòng; hiện preview lỗi.<br>3. Xác nhận import các dòng hợp lệ (hoặc all-or-nothing theo cấu hình).<br>4. Sinh báo cáo kết quả; audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Dòng lỗi không được ghi đè dữ liệu sai. |

**Bảng 21. Đặc tả Use Case "Tìm kiếm / lọc người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_021 |
| **Tên Use Case** | Tìm kiếm / lọc người dùng |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Tìm kiếm / lọc người dùng" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tìm user theo tên, email, SĐT, role, chi nhánh, trạng thái. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền xem danh sách user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-SCOPE-01`.<br>• Hậu điều kiện: Kết quả đúng filter và trong data scope quản trị.<br>• Tiêu chí chấp nhận AC1: Lọc theo chi nhánh A không trả user chỉ thuộc chi nhánh B. |
| **Kịch bản chính** | 1. Nhập từ khóa/bộ lọc → kết quả phân trang.<br>2. Click mở chi tiết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 22. Đặc tả Use Case "Xuất danh sách người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_022 |
| **Tên Use Case** | Xuất danh sách người dùng |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Xuất danh sách người dùng" thuộc nhóm Người dùng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xuất danh sách user theo bộ lọc hiện tại ra Excel. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền export user. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: File tải về đủ cột cấu hình, đúng dữ liệu lọc.<br>• Tiêu chí chấp nhận AC1: Export thành công file mở được bằng Excel. |
| **Kịch bản chính** | 1. Áp dụng filter → Export Excel.<br>2. Ghi audit export (ai xuất, số dòng). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.3. Vai trò & phân quyền (`SYS-03`)

RBAC + data scope + field-level security — trái tim kiểm soát truy cập ERP.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 11 |
| Must | 7 |

**Bảng 23. Đặc tả Use Case "Tạo / sửa / ngưng vai trò (Role)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_023 |
| **Tên Use Case** | Tạo / sửa / ngưng vai trò (Role) |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Tạo / sửa / ngưng vai trò (Role)" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý danh mục role nghiệp vụ (Admin, Kế toán, Sales…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.role.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-01`.<br>• Hậu điều kiện: Role sẵn sàng để gán permission và user.<br>• Tiêu chí chấp nhận AC1: Tạo role mới thành công.<br>• Tiêu chí chấp nhận AC2: Không tạo trùng mã. |
| **Kịch bản chính** | 1. Tạo role với mã, tên, mô tả, trạng thái.<br>2. Sửa thông tin.<br>3. Ngưng role (không xóa nếu đang được gán — hoặc chặn ngưng khi đang gán). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Trùng mã role → lỗi.<br>7.1. Ngưng role đang gán → cảnh báo/chặn theo policy. |

**Bảng 24. Đặc tả Use Case "Sao chép vai trò"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_024 |
| **Tên Use Case** | Sao chép vai trò |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Sao chép vai trò" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Nhân bản role kèm ma trận permission để tạo biến thể nhanh. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Role nguồn tồn tại. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-01`.<br>• Hậu điều kiện: Role mới có cùng permission như nguồn tại thời điểm copy.<br>• Tiêu chí chấp nhận AC1: Copy role tạo bản ghi mới; sửa bản mới không ảnh hưởng bản cũ. |
| **Kịch bản chính** | 1. Chọn role → Sao chép → nhập mã/tên mới.<br>2. Copy permission (và tùy chọn data scope mặc định).<br>3. Lưu role mới. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 25. Đặc tả Use Case "Quản lý danh mục quyền (Permission)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_025 |
| **Tên Use Case** | Quản lý danh mục quyền (Permission) |
| **Tác nhân** | Security Admin / Hệ thống |
| **Mô tả chức năng** | Cho phép Security Admin / Hệ thống thực hiện chức năng "Quản lý danh mục quyền (Permission)" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Danh mục permission kỹ thuật do module đăng ký (sys.user.manage, crm.order.view…). Cho phép xem/nhóm hóa; hạn chế sửa mã hệ thống. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin / Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Module đã đăng ký permission catalog khi cài/bật. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-02`.<br>• Hậu điều kiện: Catalog phản ánh đúng module đang license.<br>• Tiêu chí chấp nhận AC1: Permission của module tắt license không hiện để gán mới (hoặc hiện nhưng đánh dấu inactive). |
| **Kịch bản chính** | 1. Xem danh sách permission theo module.<br>2. Bật/ẩn nhóm hiển thị trên UI gán quyền.<br>3. Không cho xóa permission hệ thống đang được tham chiếu. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 26. Đặc tả Use Case "Gán quyền vào vai trò"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_026 |
| **Tên Use Case** | Gán quyền vào vai trò |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Gán quyền vào vai trò" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Thiết lập ma trận Role–Permission. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Role tồn tại.<br>• Có quyền sys.permission.assign. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-03`, `BR-SYS-AUD-01`.<br>• Hậu điều kiện: User mang role nhận đúng permission hiệu lực ngay phiên mới hoặc sau refresh quyền.<br>• Tiêu chí chấp nhận AC1: Gán crm.order.view → user có role đó gọi API xem đơn được.<br>• Tiêu chí chấp nhận AC2: Bỏ quyền → 403. |
| **Kịch bản chính** | 1. Mở role → tick/untick permission theo nhóm module.<br>2. Lưu; invalidate cache quyền.<br>3. Audit before/after. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 27. Đặc tả Use Case "Gán người dùng vào vai trò"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_027 |
| **Tên Use Case** | Gán người dùng vào vai trò |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Gán người dùng vào vai trò" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gán một user vào một hoặc nhiều role. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• User và role Active. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-04`.<br>• Hậu điều kiện: Quyền hiệu lực của user được cập nhật.<br>• Tiêu chí chấp nhận AC1: User 2 role nhận đủ permission của cả hai. |
| **Kịch bản chính** | 1. Chọn user → thêm/gỡ role.<br>2. Tính quyền hiệu lực = hợp các role (union) theo policy tenant.<br>3. Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 28. Đặc tả Use Case "Phân quyền dữ liệu theo chi nhánh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_028 |
| **Tên Use Case** | Phân quyền dữ liệu theo chi nhánh |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Phân quyền dữ liệu theo chi nhánh" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Giới hạn dữ liệu nghiệp vụ theo danh sách chi nhánh user được phép. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Đã có cây chi nhánh.<br>• User đã gán org. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-SCOPE-01`.<br>• Hậu điều kiện: Truy vấn dữ liệu ngoài chi nhánh bị loại/403.<br>• Tiêu chí chấp nhận AC1: User chi nhánh A không đọc được chứng từ chi nhánh B. |
| **Kịch bản chính** | 1. Cấu hình scope All / Assigned branches / Single branch.<br>2. Lưu vào hồ sơ phân quyền dữ liệu của user/role.<br>3. Module nghiệp vụ bắt buộc enforce scope này. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 29. Đặc tả Use Case "Phân quyền dữ liệu theo kho / điểm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_029 |
| **Tên Use Case** | Phân quyền dữ liệu theo kho / điểm |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Phân quyền dữ liệu theo kho / điểm" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Thu hẹp data scope theo kho hoặc điểm bán khi module INV/POS yêu cầu. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Master kho/điểm đã có (từ module tương ứng hoặc SYS location). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-SCOPE-02`.<br>• Hậu điều kiện: User chỉ thao tác kho được gán.<br>• Tiêu chí chấp nhận AC1: Xuất kho khác scope bị chặn. |
| **Kịch bản chính** | 1. Gán danh sách kho/điểm cho user/role.<br>2. Enforce ở API nghiệp vụ liên quan tồn/quầy. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 30. Đặc tả Use Case "Phân quyền theo phòng ban"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_030 |
| **Tên Use Case** | Phân quyền theo phòng ban |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Phân quyền theo phòng ban" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Data scope theo phòng ban cho các module dùng chiều tổ chức này (HRM, WF…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Có master phòng ban. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-SCOPE-03`.<br>• Hậu điều kiện: Dữ liệu ngoài phòng ban không hiển thị.<br>• Tiêu chí chấp nhận AC1: Manager phòng X không thấy hồ sơ phòng Y (kịch bản HRM). |
| **Kịch bản chính** | 1. Gán phòng ban được truy cập.<br>2. Module liên quan lọc theo dept scope. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 31. Đặc tả Use Case "Quyền theo trường nhạy cảm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_031 |
| **Tên Use Case** | Quyền theo trường nhạy cảm |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Quyền theo trường nhạy cảm" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Ẩn/mask hoặc cấm sửa các field nhạy cảm (ví dụ lương, CCCD, giá vốn) theo permission field-level. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Module đăng ký sensitive fields. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-05`.<br>• Hậu điều kiện: User thiếu quyền không đọc được plain sensitive data.<br>• Tiêu chí chấp nhận AC1: Role không có quyền lương không thấy số lương trên API/UI. |
| **Kịch bản chính** | 1. Cấu hình field permission theo role.<br>2. UI mask/ẩn; API không trả plain value nếu không có quyền. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 32. Đặc tả Use Case "Xem ma trận phân quyền tổng hợp"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_032 |
| **Tên Use Case** | Xem ma trận phân quyền tổng hợp |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Xem ma trận phân quyền tổng hợp" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Báo cáo ma trận Role×Permission hoặc User×Permission hiệu lực để kiểm toán. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền xem báo cáo phân quyền. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-RBAC-03`.<br>• Hậu điều kiện: Báo cáo khớp cấu hình thực tế.<br>• Tiêu chí chấp nhận AC1: Ma trận phản ánh đúng tick permission. |
| **Kịch bản chính** | 1. Chọn role hoặc user → xem permission hiệu lực + nguồn role.<br>2. Export Excel. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 33. Đặc tả Use Case "Nhật ký thay đổi phân quyền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_033 |
| **Tên Use Case** | Nhật ký thay đổi phân quyền |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Nhật ký thay đổi phân quyền" thuộc nhóm Vai trò & phân quyền trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Lưu và xem lịch sử thay đổi role/permission/scope. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.audit.view. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Truy vết được ai đổi quyền lúc nào.<br>• Tiêu chí chấp nhận AC1: Sau khi gán quyền có dòng audit tương ứng. |
| **Kịch bản chính** | 1. Mọi thay đổi UC_SYS_023–031 ghi audit.<br>2. Màn hình lọc theo thời gian/user/đối tượng.<br>3. Xem before/after. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.4. Tổ chức & đa chi nhánh (`SYS-04`)

Master tổ chức dùng chung toàn ERP và nền cho data scope.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 7 |

**Bảng 34. Đặc tả Use Case "Quản lý công ty / tenant"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_034 |
| **Tên Use Case** | Quản lý công ty / tenant |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý công ty / tenant" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý thông tin tenant/công ty: tên, MST, địa chỉ, logo, trạng thái thuê bao. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.org.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-ORG-01`.<br>• Hậu điều kiện: Thông tin công ty nhất quán trên chứng từ/header.<br>• Tiêu chí chấp nhận AC1: Cập nhật tên công ty phản ánh trên UI. |
| **Kịch bản chính** | 1. Xem/sửa hồ sơ công ty.<br>2. Upload logo.<br>3. Lưu; các chỗ branding đọc lại thông tin này. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 35. Đặc tả Use Case "Quản lý pháp nhân / công ty con"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_035 |
| **Tên Use Case** | Quản lý pháp nhân / công ty con |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý pháp nhân / công ty con" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Hỗ trợ multi-company trong một tenant: nhiều pháp nhân hạch toán/vận hành. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Gói license cho phép multi-company. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-ORG-02`.<br>• Hậu điều kiện: Chứng từ có thể gắn legal entity (khi module FIN bật).<br>• Tiêu chí chấp nhận AC1: Tạo được ≥2 pháp nhân và gán chi nhánh. |
| **Kịch bản chính** | 1. Tạo pháp nhân con với mã, MST, trạng thái.<br>2. Gán chi nhánh thuộc pháp nhân.<br>3. User được chỉ định legal entity scope nếu cần. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 36. Đặc tả Use Case "Quản lý chi nhánh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_036 |
| **Tên Use Case** | Quản lý chi nhánh |
| **Tác nhân** | Org Admin |
| **Mô tả chức năng** | Cho phép Org Admin thực hiện chức năng "Quản lý chi nhánh" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: CRUD cây chi nhánh: mã, tên, địa chỉ, quản lý, trạng thái. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Org Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.org.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-ORG-01`.<br>• Hậu điều kiện: Chi nhánh dùng được cho data scope và master module khác.<br>• Tiêu chí chấp nhận AC1: Tạo chi nhánh mới thành công.<br>• Tiêu chí chấp nhận AC2: Ngưng chi nhánh ẩn khỏi chọn mặc định mới. |
| **Kịch bản chính** | 1. Thêm/sửa/ngưng chi nhánh.<br>2. Thiết lập quan hệ cha–con nếu có.<br>3. Không xóa cứng khi đã phát sinh dữ liệu. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Ngưng chi nhánh đang là mặc định của user → cảnh báo. |

**Bảng 37. Đặc tả Use Case "Quản lý điểm bán / cửa hàng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_037 |
| **Tên Use Case** | Quản lý điểm bán / cửa hàng |
| **Tác nhân** | Org Admin |
| **Mô tả chức năng** | Cho phép Org Admin thực hiện chức năng "Quản lý điểm bán / cửa hàng" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Danh mục điểm bán thuộc chi nhánh (phục vụ POS/CRM/HRM). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Org Admin] và được cấp quyền RBAC tương ứng.<br>• Chi nhánh tồn tại. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-ORG-01`.<br>• Hậu điều kiện: Điểm bán xuất hiện cho các module được license.<br>• Tiêu chí chấp nhận AC1: Điểm bán thuộc đúng chi nhánh. |
| **Kịch bản chính** | 1. CRUD điểm bán: mã, tên, chi nhánh, địa chỉ, trạng thái.<br>2. Gắn timezone riêng nếu cần. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 38. Đặc tả Use Case "Quản lý phòng ban"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_038 |
| **Tên Use Case** | Quản lý phòng ban |
| **Tác nhân** | Org Admin |
| **Mô tả chức năng** | Cho phép Org Admin thực hiện chức năng "Quản lý phòng ban" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Danh mục phòng ban dùng chung (HRM/WF…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Org Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền quản lý org. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-ORG-01`.<br>• Hậu điều kiện: Master phòng ban sẵn sàng cho module nghiệp vụ.<br>• Tiêu chí chấp nhận AC1: Tạo phòng ban và chọn được khi gán user/NV. |
| **Kịch bản chính** | 1. CRUD phòng ban; gắn chi nhánh hoặc cấp công ty.<br>2. Ngưng dùng khi không còn hiệu lực. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 39. Đặc tả Use Case "Quản lý chức danh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_039 |
| **Tên Use Case** | Quản lý chức danh |
| **Tác nhân** | Org Admin |
| **Mô tả chức năng** | Cho phép Org Admin thực hiện chức năng "Quản lý chức danh" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Danh mục chức danh/job title dùng chung. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Org Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền quản lý org. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-ORG-01`.<br>• Hậu điều kiện: Chức danh dùng cho HRM và hiển thị user.<br>• Tiêu chí chấp nhận AC1: CRUD chức danh thành công. |
| **Kịch bản chính** | 1. CRUD chức danh; mã + tên + trạng thái. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 40. Đặc tả Use Case "Sơ đồ tổ chức trực quan"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_040 |
| **Tên Use Case** | Sơ đồ tổ chức trực quan |
| **Tác nhân** | Org Admin |
| **Mô tả chức năng** | Cho phép Org Admin thực hiện chức năng "Sơ đồ tổ chức trực quan" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Hiển thị cây tổ chức (công ty–chi nhánh–phòng ban) dạng sơ đồ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Org Admin] và được cấp quyền RBAC tương ứng.<br>• Đã có dữ liệu org. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Hậu điều kiện: Sơ đồ phản ánh đúng master.<br>• Tiêu chí chấp nhận AC1: Cây hiển thị đúng quan hệ cha–con đã cấu hình. |
| **Kịch bản chính** | 1. Mở Org Chart.<br>2. Xem/zoom/expand.<br>3. Click node mở chi tiết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 41. Đặc tả Use Case "Cấu hình múi giờ / ngôn ngữ / tiền tệ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_041 |
| **Tên Use Case** | Cấu hình múi giờ / ngôn ngữ / tiền tệ |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Cấu hình múi giờ / ngôn ngữ / tiền tệ" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Thiết lập locale mặc định của tenant: timezone, ngôn ngữ, tiền tệ gốc. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.setting.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-01`.<br>• Hậu điều kiện: Hệ thống format thời gian/tiền theo cấu hình.<br>• Tiêu chí chấp nhận AC1: Đổi timezone phản ánh trên timestamp UI. |
| **Kịch bản chính** | 1. Chọn timezone, default language, base currency.<br>2. Lưu; áp dụng cho hiển thị ngày giờ và chứng từ mặc định. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 42. Đặc tả Use Case "Cấu hình định dạng ngày số"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_042 |
| **Tên Use Case** | Cấu hình định dạng ngày số |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Cấu hình định dạng ngày số" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấu hình format ngày (dd/MM/yyyy…) và dấu phân tách số. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-01`.<br>• Hậu điều kiện: Ngày/số hiển thị nhất quán.<br>• Tiêu chí chấp nhận AC1: Export Excel dùng đúng format cấu hình. |
| **Kịch bản chính** | 1. Chọn format → lưu → áp dụng UI/export. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 43. Đặc tả Use Case "Quản lý địa chỉ / tỉnh thành"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_043 |
| **Tên Use Case** | Quản lý địa chỉ / tỉnh thành |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý địa chỉ / tỉnh thành" thuộc nhóm Tổ chức & đa chi nhánh trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Danh mục quốc gia/tỉnh-thành/quận-huyện/phường-xã (hoặc mức tương đương) dùng chung form địa chỉ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền quản lý danh mục dùng chung. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-02`.<br>• Hậu điều kiện: Các module dùng chung master địa chỉ.<br>• Tiêu chí chấp nhận AC1: Chọn tỉnh lọc đúng danh sách quận. |
| **Kịch bản chính** | 1. Import hoặc CRUD địa giới.<br>2. Form địa chỉ dùng cascade select. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.5. License & module bán hàng (`SYS-05`)

Cơ chế đóng gói–bán module và enforce runtime.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 7 |

**Bảng 44. Đặc tả Use Case "Khai báo module trong hệ thống"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_044 |
| **Tên Use Case** | Khai báo module trong hệ thống |
| **Tác nhân** | System Admin / Hệ thống |
| **Mô tả chức năng** | Cho phép System Admin / Hệ thống thực hiện chức năng "Khai báo module trong hệ thống" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Đăng ký catalog module kỹ thuật (SYS, HRM, CRM…) với mã, tên, phiên bản, dependencies. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin / Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Pack module được cài vào môi trường. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-01`.<br>• Hậu điều kiện: Catalog module làm nguồn cho license và menu.<br>• Tiêu chí chấp nhận AC1: Danh sách 16 module sản phẩm hiện đủ trong catalog. |
| **Kịch bản chính** | 1. Hệ thống đăng ký module khi deploy hoặc admin đồng bộ catalog.<br>2. Hiển thị dependencies (ví dụ LOG cần INV). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 45. Đặc tả Use Case "Bật / tắt module theo tenant"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_045 |
| **Tên Use Case** | Bật / tắt module theo tenant |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Bật / tắt module theo tenant" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Bật hoặc tắt module nghiệp vụ theo hợp đồng thuê bao. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Module có trong catalog.<br>• Không tắt SYS. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-01`, `BR-SYS-LIC-03`.<br>• Hậu điều kiện: User không truy cập UI/API module tắt.<br>• Tiêu chí chấp nhận AC1: Tắt CRM → menu CRM biến mất; API CRM 403. |
| **Kịch bản chính** | 1. Chọn module → Active/Inactive.<br>2. Khi Inactive: ẩn menu, chặn API, giữ dữ liệu.<br>3. Kiểm tra dependency (không bật LOG nếu INV off — cảnh báo/chặn).<br>4. Audit + event LicenseChanged. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Tắt module đang là dependency của module khác đang bật → cảnh báo. |

**Bảng 46. Đặc tả Use Case "Quản lý gói license"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_046 |
| **Tên Use Case** | Quản lý gói license |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý gói license" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý gói (Starter/Retail/…) gồm tập module, hạn dùng, quota. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.license.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-01`.<br>• Hậu điều kiện: Tenant vận hành đúng phạm vi gói.<br>• Tiêu chí chấp nhận AC1: Gán gói có CRM+FIN → chỉ bật đúng 2 module (+SYS). |
| **Kịch bản chính** | 1. Tạo/sửa gói: tên, danh sách module, effective/expiry, số user tối đa, số chi nhánh tối đa.<br>2. Gán gói cho tenant.<br>3. Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 47. Đặc tả Use Case "Giới hạn số user / chi nhánh theo gói"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_047 |
| **Tên Use Case** | Giới hạn số user / chi nhánh theo gói |
| **Tác nhân** | Hệ thống / System Admin |
| **Mô tả chức năng** | Cho phép Hệ thống / System Admin thực hiện chức năng "Giới hạn số user / chi nhánh theo gói" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Enforce quota user active và chi nhánh active theo gói. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống / System Admin] và được cấp quyền RBAC tương ứng.<br>• Gói có cấu hình quota. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-02`.<br>• Hậu điều kiện: Không vượt quota trừ khi admin nâng gói.<br>• Tiêu chí chấp nhận AC1: Quota 10 user: user thứ 11 bị chặn. |
| **Kịch bản chính** | 1. Khi tạo user/chi nhánh vượt quota → chặn.<br>2. Dashboard license hiển thị usage/quota.<br>3. Cảnh báo khi đạt ngưỡng (ví dụ 90%). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 48. Đặc tả Use Case "Cảnh báo / gia hạn license"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_048 |
| **Tên Use Case** | Cảnh báo / gia hạn license |
| **Tác nhân** | System Admin / Hệ thống |
| **Mô tả chức năng** | Cho phép System Admin / Hệ thống thực hiện chức năng "Cảnh báo / gia hạn license" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cảnh báo sắp hết hạn và xử lý trạng thái hết hạn (grace period). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin / Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Gói có ngày hết hạn. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-04`.<br>• Hậu điều kiện: Admin biết trước khi hết hạn; hết hạn áp dụng đúng policy.<br>• Tiêu chí chấp nhận AC1: Trước hạn 7 ngày có thông báo.<br>• Tiêu chí chấp nhận AC2: Hết hạn không tạo chứng từ mới nếu policy ReadOnly. |
| **Kịch bản chính** | 1. Job kiểm tra hạn hàng ngày.<br>2. Gửi thông báo in-app/email cho admin trước N ngày.<br>3. Khi hết hạn: chuyển ReadOnly hoặc Block theo policy; cho phép vào trang gia hạn. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 49. Đặc tả Use Case "Menu động theo module + quyền"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_049 |
| **Tên Use Case** | Menu động theo module + quyền |
| **Tác nhân** | Hệ thống / End User |
| **Mô tả chức năng** | Cho phép Hệ thống / End User thực hiện chức năng "Menu động theo module + quyền" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Render menu chỉ gồm mục thuộc module đang bật và permission user có. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống / End User] và được cấp quyền RBAC tương ứng.<br>• User đã đăng nhập. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-03`, `BR-SYS-RBAC-03`.<br>• Hậu điều kiện: Không lộ entry module chưa mua.<br>• Tiêu chí chấp nhận AC1: User thiếu quyền không thấy menu tương ứng.<br>• Tiêu chí chấp nhận AC2: Module off không có entry. |
| **Kịch bản chính** | 1. Client gọi API menu.<br>2. Server tính: licensed modules ∩ permissions ∩ feature flags.<br>3. Trả cây menu; client hiển thị. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 50. Đặc tả Use Case "Ẩn API module chưa mua"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_050 |
| **Tên Use Case** | Ẩn API module chưa mua |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Ẩn API module chưa mua" thuộc nhóm License & module bán hàng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Chặn gọi API của module không nằm trong license dù user đoán URL. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Request vào API gateway/backend. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-LIC-03`.<br>• Hậu điều kiện: API module off không thực thi nghiệp vụ.<br>• Tiêu chí chấp nhận AC1: Gọi API CRM khi CRM off → 403. |
| **Kịch bản chính** | 1. Middleware kiểm tra license module của route.<br>2. Nếu inactive → 403 FEATURE_NOT_LICENSED.<br>3. Ghi security log khi bị gọi lặp bất thường (optional). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.6. Cấu hình hệ thống (`SYS-06`)

Tham số, danh mục dùng chung, đánh số chứng từ, template thông báo.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 8 |
| Must | 5 |

**Bảng 51. Đặc tả Use Case "Tham số cấu hình toàn cục"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_051 |
| **Tên Use Case** | Tham số cấu hình toàn cục |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Tham số cấu hình toàn cục" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý key-value settings toàn tenant (timeout, ngưỡng, feature flags nội bộ…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.setting.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-01`.<br>• Hậu điều kiện: Giá trị mới có hiệu lực theo cơ chế cache invalidate.<br>• Tiêu chí chấp nhận AC1: Đổi session timeout áp dụng cho phiên mới. |
| **Kịch bản chính** | 1. Danh sách settings có nhóm/mô tả/kiểu dữ liệu.<br>2. Sửa giá trị có validate.<br>3. Audit thay đổi. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Sửa key hệ thống nguy hiểm cần xác nhận 2 bước (optional). |

**Bảng 52. Đặc tả Use Case "Cấu hình theo chi nhánh"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_052 |
| **Tên Use Case** | Cấu hình theo chi nhánh |
| **Tác nhân** | Org Admin |
| **Mô tả chức năng** | Cho phép Org Admin thực hiện chức năng "Cấu hình theo chi nhánh" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cho phép override một số setting theo chi nhánh. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Org Admin] và được cấp quyền RBAC tương ứng.<br>• Setting được đánh dấu overridable. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-01`.<br>• Hậu điều kiện: Chi nhánh dùng giá trị riêng khi có override.<br>• Tiêu chí chấp nhận AC1: Chi nhánh A override timezone khác global. |
| **Kịch bản chính** | 1. Chọn chi nhánh → override giá trị.<br>2. Clear override để kế thừa global. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 53. Đặc tả Use Case "Danh mục dùng chung"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_053 |
| **Tên Use Case** | Danh mục dùng chung |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Danh mục dùng chung" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý các danh mục dùng chung: đơn vị tính, loại tiền, trạng thái chung, lý do hủy… |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền danh mục. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-02`.<br>• Hậu điều kiện: Danh mục nhất quán xuyên module.<br>• Tiêu chí chấp nhận AC1: Thêm ĐVT mới và chọn được ở module INV (khi có). |
| **Kịch bản chính** | 1. CRUD item theo từng loại danh mục.<br>2. Mỗi item: mã, tên, thứ tự, trạng thái.<br>3. Module khác chỉ đọc/tham chiếu. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Không xóa item đã tham chiếu — chỉ ngưng. |

**Bảng 54. Đặc tả Use Case "Mẫu số chứng từ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_054 |
| **Tên Use Case** | Mẫu số chứng từ |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Mẫu số chứng từ" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Định nghĩa rule đánh số chứng từ: tiền tố, năm/tháng, độ dài, reset kỳ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình sequence. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-03`.<br>• Hậu điều kiện: Chứng từ mới nhận số đúng rule.<br>• Tiêu chí chấp nhận AC1: Pattern SO-{YYYY}-{00001} sinh đúng số tiếp theo. |
| **Kịch bản chính** | 1. Tạo rule theo loại chứng từ (SO, PO, INV… đăng ký bởi module).<br>2. Cấu hình pattern.<br>3. Xem số hiện tại; không cho sửa lùi tùy tiện. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 55. Đặc tả Use Case "Sinh mã tự động"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_055 |
| **Tên Use Case** | Sinh mã tự động |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Sinh mã tự động" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cung cấp service sinh mã atomic, không trùng dưới tải đồng thời. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Rule sequence đã cấu hình. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-03`.<br>• Hậu điều kiện: Không trùng mã trong cùng scope rule.<br>• Tiêu chí chấp nhận AC1: 100 request song song không sinh mã trùng. |
| **Kịch bản chính** | 1. Module gọi Sequence.next(docType, context).<br>2. SYS cấp số trong transaction/lock.<br>3. Trả mã hiển thị. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Hết dải số → lỗi rõ ràng. |

**Bảng 56. Đặc tả Use Case "Cấu hình mẫu email / SMS"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_056 |
| **Tên Use Case** | Cấu hình mẫu email / SMS |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Cấu hình mẫu email / SMS" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý template thông báo có biến động ({{user_name}}, {{reset_link}}…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình thông báo. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-01`.<br>• Hậu điều kiện: Sự kiện gửi dùng đúng template đang Active.<br>• Tiêu chí chấp nhận AC1: Sửa template invite → email mời dùng nội dung mới. |
| **Kịch bản chính** | 1. CRUD template theo sự kiện.<br>2. Preview với dữ liệu mẫu.<br>3. Chọn kênh mặc định. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 57. Đặc tả Use Case "Cấu hình lịch làm việc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_057 |
| **Tên Use Case** | Cấu hình lịch làm việc |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Cấu hình lịch làm việc" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Thiết lập ngày làm việc trong tuần và lịch nghỉ lễ dùng cho SLA/WF/chấm công (khi module dùng). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-CFG-01`.<br>• Hậu điều kiện: Các module đọc calendar chung từ SYS.<br>• Tiêu chí chấp nhận AC1: Đánh dấu 01/01 là holiday thành công. |
| **Kịch bản chính** | 1. Chọn ngày làm việc.<br>2. Thêm ngày nghỉ lễ theo năm.<br>3. Export/import lịch lễ. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 58. Đặc tả Use Case "Quản lý phiên bản cấu hình"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_058 |
| **Tên Use Case** | Quản lý phiên bản cấu hình |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý phiên bản cấu hình" thuộc nhóm Cấu hình hệ thống trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Lưu snapshot thay đổi cấu hình quan trọng để xem lại/rollback có kiểm soát. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình nâng cao. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Có lịch sử cấu hình truy vết được.<br>• Tiêu chí chấp nhận AC1: Xem được phiên bản cấu hình trước đó. |
| **Kịch bản chính** | 1. Mỗi lần đổi setting quan trọng tạo version.<br>2. Xem diff.<br>3. Rollback (optional) có xác nhận và audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Rollback có thể bị chặn nếu không an toàn. |

### 7.7. Thông báo (`SYS-07`)

Kênh in-app/email/SMS/push và cấu hình sự kiện kích hoạt.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 3 |

**Bảng 59. Đặc tả Use Case "Thông báo in-app"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_059 |
| **Tên Use Case** | Thông báo in-app |
| **Tác nhân** | End User / Hệ thống |
| **Mô tả chức năng** | Cho phép End User / Hệ thống thực hiện chức năng "Thông báo in-app" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Chuông thông báo trong ứng dụng: danh sách, đánh dấu đã đọc, deep-link tới chứng từ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User / Hệ thống] và được cấp quyền RBAC tương ứng.<br>• User đăng nhập. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-02`.<br>• Hậu điều kiện: User nhận được thông báo gần realtime (websocket/poll).<br>• Tiêu chí chấp nhận AC1: Có thông báo mới hiển thị badge.<br>• Tiêu chí chấp nhận AC2: Đánh dấu đã đọc cập nhật trạng thái. |
| **Kịch bản chính** | 1. Hệ thống tạo notification khi có sự kiện.<br>2. User mở trung tâm thông báo.<br>3. Click → điều hướng đối tượng liên quan nếu còn quyền.<br>4. Đánh dấu đã đọc / đọc tất cả. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Mất quyền xem đối tượng → thông báo vẫn xem được tiêu đề, deep-link bị chặn. |

**Bảng 60. Đặc tả Use Case "Gửi email hệ thống"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_060 |
| **Tên Use Case** | Gửi email hệ thống |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Gửi email hệ thống" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gửi email giao dịch qua SMTP/ESP đã cấu hình. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Email gateway Active.<br>• Có template hoặc nội dung. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-01`.<br>• Hậu điều kiện: Email được gửi hoặc ghi nhận thất bại rõ ràng.<br>• Tiêu chí chấp nhận AC1: Invite email gửi thành công trong môi trường có gateway test. |
| **Kịch bản chính** | 1. Module/SYS tạo Outbox message.<br>2. Worker gửi qua provider.<br>3. Cập nhật trạng thái Sent/Failed; retry có giới hạn. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Fail → log lý do; không crash nghiệp vụ nguồn. |

**Bảng 61. Đặc tả Use Case "Gửi SMS / messaging"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_061 |
| **Tên Use Case** | Gửi SMS / messaging |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Gửi SMS / messaging" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gửi SMS/Zalo OA (khung) cho OTP và cảnh báo. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• SMS/messaging gateway đã cấu hình. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-01`.<br>• Hậu điều kiện: OTP SMS nhận được trong môi trường tích hợp thật.<br>• Tiêu chí chấp nhận AC1: Gửi SMS test thành công hoặc mock provider ghi Sent. |
| **Kịch bản chính** | 1. Tạo message → worker gửi → log delivery. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Provider lỗi → retry/fail log. |

**Bảng 62. Đặc tả Use Case "Push notification mobile"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_062 |
| **Tên Use Case** | Push notification mobile |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Push notification mobile" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Đẩy thông báo về app mobile qua FCM/APNs. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Thiết bị user đã đăng ký push token.<br>• Cấu hình provider. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-02`.<br>• Hậu điều kiện: Thiết bị nhận push (hoặc log success từ provider).<br>• Tiêu chí chấp nhận AC1: Đăng ký token và gửi push test thành công. |
| **Kịch bản chính** | 1. Sự kiện → push payload → provider → log. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Token hết hạn → đánh dấu invalid. |

**Bảng 63. Đặc tả Use Case "Cấu hình sự kiện kích hoạt thông báo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_063 |
| **Tên Use Case** | Cấu hình sự kiện kích hoạt thông báo |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Cấu hình sự kiện kích hoạt thông báo" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Map sự kiện hệ thống → kênh + template + đối tượng nhận (role/user/owner). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình thông báo. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-01`.<br>• Hậu điều kiện: Khi event phát sinh, rule Active được thực thi.<br>• Tiêu chí chấp nhận AC1: Tắt rule → event không gửi kênh đó nữa. |
| **Kịch bản chính** | 1. Chọn event (UserInvited, ApprovalPending…).<br>2. Chọn kênh in-app/email/SMS/push.<br>3. Chọn template + recipients rule.<br>4. Bật/tắt rule. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 64. Đặc tả Use Case "Tùy chọn thông báo cá nhân"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_064 |
| **Tên Use Case** | Tùy chọn thông báo cá nhân |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tùy chọn thông báo cá nhân" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cho phép user tắt/bật một số loại thông báo không bắt buộc. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-NOTI-02`.<br>• Hậu điều kiện: Preference được tôn trọng khi gửi.<br>• Tiêu chí chấp nhận AC1: Tắt email nhắc việc → không nhận email loại đó. |
| **Kịch bản chính** | 1. Mở Preference.<br>2. Tắt email marketing/nhắc không critical.<br>3. Không cho tắt cảnh báo bảo mật bắt buộc. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 65. Đặc tả Use Case "Nhật ký gửi thông báo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_065 |
| **Tên Use Case** | Nhật ký gửi thông báo |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Nhật ký gửi thông báo" thuộc nhóm Thông báo trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tra cứu lịch sử gửi: kênh, trạng thái, thời điểm, lỗi. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền xem log thông báo. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Truy vết được thông báo đã gửi.<br>• Tiêu chí chấp nhận AC1: Có bản ghi Sent cho email invite vừa gửi. |
| **Kịch bản chính** | 1. Lọc theo thời gian/user/kênh/trạng thái.<br>2. Xem chi tiết payload đã che thông tin nhạy cảm nếu cần.<br>3. Resend thủ công (optional). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.8. File & tài liệu (`SYS-08`)

Lưu trữ đính kèm an toàn, có phân quyền và vòng đời file.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 2 |

**Bảng 66. Đặc tả Use Case "Upload file"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_066 |
| **Tên Use Case** | Upload file |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Upload file" thuộc nhóm File & tài liệu trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Upload file đính kèm với kiểm soát loại/dung lượng. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập.<br>• Có quyền upload trên đối tượng đích. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-FILE-01`.<br>• Hậu điều kiện: File sẵn sàng để tải/xem theo quyền.<br>• Tiêu chí chấp nhận AC1: Upload PDF hợp lệ thành công.<br>• Tiêu chí chấp nhận AC2: Upload .exe bị chặn nếu không nằm whitelist. |
| **Kịch bản chính** | 1. Chọn file → validate extension/MIME/size.<br>2. Lưu storage (local/S3 tương đương).<br>3. Tạo FileObject gắn entity.<br>4. Quét virus nếu bật.<br>5. Trả id file. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. File không hợp lệ → từ chối.<br>7.1. Vượt quota storage tenant → từ chối. |

**Bảng 67. Đặc tả Use Case "Tải xuống / xem trước file"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_067 |
| **Tên Use Case** | Tải xuống / xem trước file |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tải xuống / xem trước file" thuộc nhóm File & tài liệu trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tải hoặc preview file nếu user có quyền trên đối tượng/file. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• File tồn tại và chưa bị xóa cứng.<br>• User có quyền đọc. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-FILE-02`.<br>• Hậu điều kiện: User nhận đúng nội dung file.<br>• Tiêu chí chấp nhận AC1: User có quyền tải được; user khác 403. |
| **Kịch bản chính** | 1. Yêu cầu download/preview.<br>2. Authorize.<br>3. Stream file; ghi access log với file nhạy cảm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Hết quyền → 403. |

**Bảng 68. Đặc tả Use Case "Quản lý thư mục tài liệu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_068 |
| **Tên Use Case** | Quản lý thư mục tài liệu |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý thư mục tài liệu" thuộc nhóm File & tài liệu trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tổ chức cây thư mục tài liệu dùng chung (không thay DMS doanh nghiệp lớn). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền quản lý tài liệu. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-FILE-02`.<br>• Hậu điều kiện: File sắp xếp theo thư mục.<br>• Tiêu chí chấp nhận AC1: Tạo thư mục con và upload file vào đó. |
| **Kịch bản chính** | 1. Tạo/đổi tên/di chuyển thư mục.<br>2. Phân quyền thư mục cơ bản. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 69. Đặc tả Use Case "Phân quyền file theo đối tượng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_069 |
| **Tên Use Case** | Phân quyền file theo đối tượng |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Phân quyền file theo đối tượng" thuộc nhóm File & tài liệu trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quyền file kế thừa từ chứng từ/entity hoặc set riêng. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• File gắn entity. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-FILE-02`.<br>• Hậu điều kiện: Không lộ file ngoài quyền.<br>• Tiêu chí chấp nhận AC1: User ngoài scope entity không tải được file. |
| **Kịch bản chính** | 1. Mặc định: ai đọc được entity thì đọc được file.<br>2. Cho phép override share cụ thể (optional). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 70. Đặc tả Use Case "Xóa mềm / khôi phục file"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_070 |
| **Tên Use Case** | Xóa mềm / khôi phục file |
| **Tác nhân** | End User / System Admin |
| **Mô tả chức năng** | Cho phép End User / System Admin thực hiện chức năng "Xóa mềm / khôi phục file" thuộc nhóm File & tài liệu trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Đưa file vào thùng rác và khôi phục trong thời hạn giữ. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User / System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền xóa file trên đối tượng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-FILE-01`.<br>• Hậu điều kiện: File xóa mềm không còn gắn hiển thị bình thường.<br>• Tiêu chí chấp nhận AC1: Xóa mềm rồi khôi phục thành công trong hạn. |
| **Kịch bản chính** | 1. Xóa mềm → ẩn khỏi UI chính.<br>2. Khôi phục trong retention window.<br>3. Job purge sau hạn. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 71. Đặc tả Use Case "Quét virus / bảo mật file"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_071 |
| **Tên Use Case** | Quét virus / bảo mật file |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Quét virus / bảo mật file" thuộc nhóm File & tài liệu trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quét malware trước khi file ở trạng thái Available. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Engine quét được cấu hình. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-FILE-01`.<br>• Hậu điều kiện: File nhiễm không cho tải.<br>• Tiêu chí chấp nhận AC1: File EICAR test bị Blocked trong môi trường có scanner. |
| **Kịch bản chính** | 1. Sau upload → trạng thái Scanning.<br>2. Clean → Available; Infected → Blocked + thông báo. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Engine lỗi → giữ Pending/Blocked theo policy an toàn. |

### 7.9. Import / Export (`SYS-09`)

Khung nhập xuất dữ liệu dùng chung, có job và audit.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 4 |

**Bảng 72. Đặc tả Use Case "Import Excel/CSV theo mẫu"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_072 |
| **Tên Use Case** | Import Excel/CSV theo mẫu |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Import Excel/CSV theo mẫu" thuộc nhóm Import / Export trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Khung import dùng chung: upload, validate, preview, commit. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có mẫu import của đúng entity.<br>• Có quyền import entity đó. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-IE-01`.<br>• Hậu điều kiện: Dữ liệu hợp lệ được ghi; báo cáo lỗi tải được.<br>• Tiêu chí chấp nhận AC1: Import file đúng mẫu thành công; file sai cột bị từ chối. |
| **Kịch bản chính** | 1. Upload file đúng mẫu.<br>2. Validate schema + business rules từng dòng.<br>3. Preview: OK/Error.<br>4. Commit dòng hợp lệ; sinh job result. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. File sai schema → reject toàn bộ. |

**Bảng 73. Đặc tả Use Case "Tải file mẫu import"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_073 |
| **Tên Use Case** | Tải file mẫu import |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tải file mẫu import" thuộc nhóm Import / Export trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tải template Excel/CSV chuẩn cho từng loại import. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Loại import tồn tại. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-IE-01`.<br>• Hậu điều kiện: User có file mẫu đúng cấu trúc.<br>• Tiêu chí chấp nhận AC1: Template mở được và có header đúng. |
| **Kịch bản chính** | 1. Chọn loại → Download template (kèm sheet hướng dẫn nếu có). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 74. Đặc tả Use Case "Export Excel"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_074 |
| **Tên Use Case** | Export Excel |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Export Excel" thuộc nhóm Import / Export trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xuất dữ liệu danh sách theo filter/quyền hiện tại ra Excel. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Có quyền export trên màn hình. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-IE-02`, `BR-SYS-SCOPE-01`.<br>• Hậu điều kiện: File phản ánh đúng data scope.<br>• Tiêu chí chấp nhận AC1: Export đúng số bản ghi đang lọc. |
| **Kịch bản chính** | 1. Apply filter → Export.<br>2. Giới hạn số dòng/job async nếu lớn.<br>3. Audit export. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Vượt ngưỡng → chuyển job nền + thông báo khi xong. |

**Bảng 75. Đặc tả Use Case "Export PDF"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_075 |
| **Tên Use Case** | Export PDF |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Export PDF" thuộc nhóm Import / Export trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xuất chứng từ/báo cáo dạng PDF theo mẫu in. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Có mẫu in Active.<br>• Có quyền in/export. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-IE-02`.<br>• Hậu điều kiện: PDF chứa đúng dữ liệu chứng từ.<br>• Tiêu chí chấp nhận AC1: PDF tạo thành công cho chứng từ mẫu. |
| **Kịch bản chính** | 1. Chọn bản ghi → In/PDF.<br>2. Render template + dữ liệu.<br>3. Tải file hoặc mở preview. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 76. Đặc tả Use Case "Lịch sử job import/export"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_076 |
| **Tên Use Case** | Lịch sử job import/export |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Lịch sử job import/export" thuộc nhóm Import / Export trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Theo dõi các job import/export: trạng thái, tiến độ, file kết quả. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền xem job. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Truy vết được các lần import/export.<br>• Tiêu chí chấp nhận AC1: Job vừa chạy xuất hiện trong lịch sử. |
| **Kịch bản chính** | 1. Danh sách job lọc theo loại/ngày/user.<br>2. Xem log lỗi dòng.<br>3. Tải file result. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 77. Đặc tả Use Case "Xuất dữ liệu hàng loạt"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_077 |
| **Tên Use Case** | Xuất dữ liệu hàng loạt |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Xuất dữ liệu hàng loạt" thuộc nhóm Import / Export trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xuất lớn phục vụ migration/backup cấu hình (không thay thế DB backup). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền vận hành đặc biệt. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-IE-02`.<br>• Hậu điều kiện: Gói dữ liệu được tạo trong giới hạn kỹ thuật.<br>• Tiêu chí chấp nhận AC1: Tạo được job bulk export và tải file khi hoàn tất. |
| **Kịch bản chính** | 1. Chọn tập entity → tạo export job.<br>2. Chạy nền; thông báo khi xong; file có hạn tải. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.10. Audit & bảo mật (`SYS-10`)

Nhật ký, truy vết và chính sách phiên/IP.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 6 |
| Must | 3 |

**Bảng 78. Đặc tả Use Case "Nhật ký thao tác người dùng"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_078 |
| **Tên Use Case** | Nhật ký thao tác người dùng |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Nhật ký thao tác người dùng" thuộc nhóm Audit & bảo mật trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Ghi nhận thao tác CRUD/quan trọng: ai, khi nào, trên entity nào, hành động gì. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Hệ thống đang chạy. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Truy vết được thao tác critical.<br>• Tiêu chí chấp nhận AC1: Tạo user sinh audit Create. |
| **Kịch bản chính** | 1. Middleware/application tự ghi AuditLog.<br>2. Màn hình tra cứu có lọc.<br>3. Xem chi tiết. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Không ghi password/PII plaintext không cần thiết. |

**Bảng 79. Đặc tả Use Case "Nhật ký đăng nhập / thất bại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_079 |
| **Tên Use Case** | Nhật ký đăng nhập / thất bại |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Nhật ký đăng nhập / thất bại" thuộc nhóm Audit & bảo mật trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Lưu mọi attempt đăng nhập thành công/thất bại phục vụ an ninh. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng có định danh hợp lệ thuộc nhóm đối tượng [Security Admin] (hoặc được cấp tài khoản tương ứng) để thực hiện chức năng. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`, `BR-SYS-AUTH-02`.<br>• Hậu điều kiện: Đủ dữ liệu điều tra brute-force.<br>• Tiêu chí chấp nhận AC1: Login sai sinh bản ghi Failed. |
| **Kịch bản chính** | 1. Mỗi attempt ghi LoginLog.<br>2. Dashboard cảnh báo spiking thất bại (optional).<br>3. Export được. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 80. Đặc tả Use Case "Xem chi tiết thay đổi field"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_080 |
| **Tên Use Case** | Xem chi tiết thay đổi field |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Xem chi tiết thay đổi field" thuộc nhóm Audit & bảo mật trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Hiển thị before/after ở cấp field với các entity bật field audit. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Entity hỗ trợ field audit.<br>• Có quyền xem audit. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Biết giá trị cũ/mới.<br>• Tiêu chí chấp nhận AC1: Đổi email user → thấy before/after email. |
| **Kịch bản chính** | 1. Mở lịch sử bản ghi → xem từng field đổi.<br>2. Lọc theo field. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 81. Đặc tả Use Case "Xuất audit log"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_081 |
| **Tên Use Case** | Xuất audit log |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Xuất audit log" thuộc nhóm Audit & bảo mật trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xuất audit/login log ra Excel/CSV cho kiểm toán. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền export audit. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: File phục vụ đối soát ngoài hệ thống.<br>• Tiêu chí chấp nhận AC1: Export được log 7 ngày gần nhất. |
| **Kịch bản chính** | 1. Chọn khoảng thời gian/filter → export job.<br>2. Audit chính việc export. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 82. Đặc tả Use Case "Quản lý IP allow/deny"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_082 |
| **Tên Use Case** | Quản lý IP allow/deny |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Quản lý IP allow/deny" thuộc nhóm Audit & bảo mật trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Giới hạn đăng nhập theo allowlist/denylist IP (tùy gói). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Feature được bật. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Later**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-SEC-01`.<br>• Hậu điều kiện: Chỉ IP hợp lệ truy cập được.<br>• Tiêu chí chấp nhận AC1: IP ngoài list bị chặn khi policy allowlist bật. |
| **Kịch bản chính** | 1. Cấu hình danh sách IP/CIDR.<br>2. Enforce ở bước đăng nhập/API. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. IP không thuộc allowlist → từ chối. |

**Bảng 83. Đặc tả Use Case "Chính sách hết hạn phiên"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_083 |
| **Tên Use Case** | Chính sách hết hạn phiên |
| **Tác nhân** | Security Admin |
| **Mô tả chức năng** | Cho phép Security Admin thực hiện chức năng "Chính sách hết hạn phiên" thuộc nhóm Audit & bảo mật trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấu hình idle timeout và absolute session lifetime. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Security Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình bảo mật. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUTH-03`.<br>• Hậu điều kiện: Phiên idle quá hạn không dùng tiếp được.<br>• Tiêu chí chấp nhận AC1: Idle timeout 15 phút: sau 15 phút không hoạt động bị đăng xuất. |
| **Kịch bản chính** | 1. Đặt idle timeout / max lifetime.<br>2. Client/server enforce; hết hạn → 401 + yêu cầu đăng nhập lại. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.11. Tích hợp nền tảng (`SYS-11`)

API Key, webhook, event bus và gateway kết nối ngoài.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 7 |
| Must | 2 |

**Bảng 84. Đặc tả Use Case "Quản lý API Key"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_084 |
| **Tên Use Case** | Quản lý API Key |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý API Key" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấp/thu hồi API Key cho tích hợp máy-máy với quyền tối thiểu. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền sys.integration.manage. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-INT-01`.<br>• Hậu điều kiện: Client dùng key còn hạn gọi được API trong scope.<br>• Tiêu chí chấp nhận AC1: Key bị thu hồi → 401.<br>• Tiêu chí chấp nhận AC2: Key thiếu scope → 403. |
| **Kịch bản chính** | 1. Tạo key: tên, scopes/permissions, hạn dùng.<br>2. Hiển thị secret một lần.<br>3. Thu hồi/rotate.<br>4. Audit. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 85. Đặc tả Use Case "Quản lý Webhook outbound"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_085 |
| **Tên Use Case** | Quản lý Webhook outbound |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý Webhook outbound" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Đăng ký URL nhận sự kiện; ký payload; bật/tắt subscription. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền integration. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-INT-02`.<br>• Hậu điều kiện: Sự kiện được POST tới URL khi phát sinh.<br>• Tiêu chí chấp nhận AC1: Test ping trả 2xx được đánh dấu Healthy. |
| **Kịch bản chính** | 1. Tạo subscription: event types, URL, secret.<br>2. Test ping.<br>3. Xem trạng thái giao hàng. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. URL không HTTPS (production) → cảnh báo/chặn theo policy. |

**Bảng 86. Đặc tả Use Case "Nhật ký gọi API / webhook"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_086 |
| **Tên Use Case** | Nhật ký gọi API / webhook |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Nhật ký gọi API / webhook" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Log request/response tóm tắt của API key và webhook delivery (ẩn secret). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền xem log tích hợp. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-AUD-01`.<br>• Hậu điều kiện: Debug tích hợp được.<br>• Tiêu chí chấp nhận AC1: Delivery thất bại có log status/code. |
| **Kịch bản chính** | 1. Lọc theo thời gian/status/key.<br>2. Xem retry history.<br>3. Replay delivery (optional). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 87. Đặc tả Use Case "Hàng đợi sự kiện liên module"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_087 |
| **Tên Use Case** | Hàng đợi sự kiện liên module |
| **Tác nhân** | Hệ thống |
| **Mô tả chức năng** | Cho phép Hệ thống thực hiện chức năng "Hàng đợi sự kiện liên module" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Event bus nội bộ để các module giao tiếp bất đồng bộ (UserDisabled, LicenseChanged…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [Hệ thống] và được cấp quyền RBAC tương ứng.<br>• Hệ thống chạy worker/queue. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-INT-03`.<br>• Hậu điều kiện: Module đích nhận và xử lý event eventually consistent.<br>• Tiêu chí chấp nhận AC1: Publish UserDisabled → subscriber thu hồi phiên/chạy cleanup. |
| **Kịch bản chính** | 1. Module publish domain event.<br>2. Bus lưu và phân phối tới subscriber.<br>3. Retry/poison queue khi lỗi.<br>4. Idempotent consumer. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Consumer lỗi không làm mất event nguồn quá giới hạn retention. |

**Bảng 88. Đặc tả Use Case "Kết nối email gateway"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_088 |
| **Tên Use Case** | Kết nối email gateway |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Kết nối email gateway" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấu hình SMTP/ESP (host, port, credential, from-name) và gửi email thử. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền integration/setting. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-INT-01`.<br>• Hậu điều kiện: UC gửi email dùng được gateway.<br>• Tiêu chí chấp nhận AC1: Test email thành công với SMTP giả lập/dev. |
| **Kịch bản chính** | 1. Nhập cấu hình → Test connection/send.<br>2. Lưu trạng thái Active.<br>3. Che credential khi xem lại. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.<br>6.1. Test fail → không Active. |

**Bảng 89. Đặc tả Use Case "Kết nối SMS gateway"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_089 |
| **Tên Use Case** | Kết nối SMS gateway |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Kết nối SMS gateway" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấu hình nhà cung cấp SMS và gửi tin thử. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền integration. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-INT-01`.<br>• Hậu điều kiện: Gửi SMS OTP dùng được khi Active.<br>• Tiêu chí chấp nhận AC1: Cấu hình lưu và test status hiển thị đúng. |
| **Kịch bản chính** | 1. Nhập API key/provider → test → Active. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 90. Đặc tả Use Case "Cấu hình tích hợp bên ngoài"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_090 |
| **Tên Use Case** | Cấu hình tích hợp bên ngoài |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Cấu hình tích hợp bên ngoài" thuộc nhóm Tích hợp nền tảng trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Registry connector (HĐĐT, ngân hàng, chat…) ở mức khung: bật connector, lưu config, health. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Connector package có sẵn. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-INT-01`.<br>• Hậu điều kiện: Module nghiệp vụ gọi connector qua interface chuẩn.<br>• Tiêu chí chấp nhận AC1: Enable connector hiển thị Healthy/Unhealthy. |
| **Kịch bản chính** | 1. Danh sách connector → cấu hình → enable.<br>2. Health check định kỳ.<br>3. Phân quyền sử dụng connector. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

### 7.12. Đa ngôn ngữ & giao diện (`SYS-12`)

Ngôn ngữ, branding và trải nghiệm landing theo role.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 4 |
| Must | 0 |

**Bảng 91. Đặc tả Use Case "Quản lý gói ngôn ngữ"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_091 |
| **Tên Use Case** | Quản lý gói ngôn ngữ |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Quản lý gói ngôn ngữ" thuộc nhóm Đa ngôn ngữ & giao diện trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Quản lý resource chuỗi UI theo ngôn ngữ (VI mặc định, EN…). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình giao diện. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-UX-01`.<br>• Hậu điều kiện: UI lấy chuỗi theo ngôn ngữ đang chọn.<br>• Tiêu chí chấp nhận AC1: Chuyển pack EN → một số nhãn key có bản dịch hiển thị EN. |
| **Kịch bản chính** | 1. Import/export language pack.<br>2. Sửa chuỗi.<br>3. Đặt ngôn ngữ mặc định tenant. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 92. Đặc tả Use Case "Đổi ngôn ngữ giao diện"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_092 |
| **Tên Use Case** | Đổi ngôn ngữ giao diện |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đổi ngôn ngữ giao diện" thuộc nhóm Đa ngôn ngữ & giao diện trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: User tự chọn ngôn ngữ UI riêng. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập.<br>• Language pack Active. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-UX-01`.<br>• Hậu điều kiện: UI theo preference user; nếu thiếu key → fallback VI.<br>• Tiêu chí chấp nhận AC1: Đổi sang EN cập nhật giao diện ngay. |
| **Kịch bản chính** | 1. Chọn ngôn ngữ ở profile/header.<br>2. Lưu preference.<br>3. Reload UI. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 93. Đặc tả Use Case "Tùy chỉnh theme / logo"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_093 |
| **Tên Use Case** | Tùy chỉnh theme / logo |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Tùy chỉnh theme / logo" thuộc nhóm Đa ngôn ngữ & giao diện trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Branding cơ bản: logo, màu chủ đạo, favicon theo tenant. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền branding. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-UX-01`.<br>• Hậu điều kiện: Màn hình đăng nhập và header dùng branding mới.<br>• Tiêu chí chấp nhận AC1: Đổi logo phản ánh trên trang đăng nhập. |
| **Kịch bản chính** | 1. Upload logo/favicon; chọn màu.<br>2. Preview.<br>3. Publish. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

**Bảng 94. Đặc tả Use Case "Trang chủ theo vai trò"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_094 |
| **Tên Use Case** | Trang chủ theo vai trò |
| **Tác nhân** | System Admin |
| **Mô tả chức năng** | Cho phép System Admin thực hiện chức năng "Trang chủ theo vai trò" thuộc nhóm Đa ngôn ngữ & giao diện trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cấu hình landing/widget mặc định theo role sau đăng nhập. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [System Admin] và được cấp quyền RBAC tương ứng.<br>• Có quyền cấu hình UI. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-UX-01`.<br>• Hậu điều kiện: Đăng nhập vào đúng landing.<br>• Tiêu chí chấp nhận AC1: Role Sales vào landing CRM; role Accountant vào FIN (khi module bật). |
| **Kịch bản chính** | 1. Gán landing page/dashboard mặc định theo role.<br>2. User thuộc nhiều role → chọn theo độ ưu tiên cấu hình. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


### 7.13. Nhắn tin realtime (`SYS-13`)

> Chat nội bộ user↔user (SignalR). Khác **SYS-07 Thông báo** (hệ thống→user). Spec kỹ thuật: `Source/docs/04_MSG_REALTIME.md`.

| Chỉ số | Giá trị |
|---|---|
| Số UC | 10 |
| Must | 5 |
| Should | 3 |
| Could | 2 |

**BR-SYS-MSG-01:** Mọi tin/hội thoại thuộc tenant; chỉ member được đọc/gửi; realtime qua `/hubs/msg` — cấm poll.


**Bảng 95. Đặc tả Use Case "Tạo hội thoại 1-1"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_095 |
| **Tên Use Case** | Tạo hội thoại 1-1 |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tạo hội thoại 1-1" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tạo hoặc mở hội thoại Direct với một user khác cùng tenant. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập; có quyền sys.msg.send; đối phương thuộc cùng tenant và Active. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Không tạo trùng Direct giữa cùng 2 user; ghi audit tạo hội thoại.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hội thoại 1-1» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Chọn user đích.<br>2. Hệ thống tìm Direct hiện có hoặc tạo mới + 2 members.<br>3. Mở khung chat. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 96. Đặc tả Use Case "Tạo hội thoại nhóm"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_096 |
| **Tên Use Case** | Tạo hội thoại nhóm |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tạo hội thoại nhóm" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tạo hội thoại Group với tiêu đề và nhiều thành viên. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã đăng nhập; sys.msg.send; ≥2 thành viên hợp lệ. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Lưu title; members ≥2; creator là admin nhóm mặc định.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tạo hội thoại nhóm» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Nhập tên nhóm + chọn members.<br>2. Tạo conversation kind=Group.<br>3. Thông báo thành viên qua SignalR conversationUpdated. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 97. Đặc tả Use Case "Gửi tin nhắn realtime"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_097 |
| **Tên Use Case** | Gửi tin nhắn realtime |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Gửi tin nhắn realtime" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gửi tin text (và tuỳ chọn đính kèm) vào hội thoại đang tham gia. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Là member của hội thoại; sys.msg.send; body không rỗng (trừ khi có file). |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Persist chat_message; đẩy messageReceived tới members online.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Gửi tin nhắn realtime» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Nhập nội dung → Gửi.<br>2. API lưu DB.<br>3. Hub đẩy realtime; UI người gửi hiện tin ngay. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 98. Đặc tả Use Case "Nhận tin nhắn realtime (SignalR)"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_098 |
| **Tên Use Case** | Nhận tin nhắn realtime (SignalR) |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Nhận tin nhắn realtime (SignalR)" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Nhận tin mới qua hub /hubs/msg mà không poll API. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Đã kết nối SignalR với JWT; thuộc group user:{id} hoặc conv:{id}. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Cấm setInterval gọi lịch sử; mất kết nối thì reconnect + sync phần thiếu.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Nhận tin nhắn realtime (SignalR)» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. FE subscribe hub.<br>2. Khi có messageReceived → append UI / tăng badge.<br>3. Offline: khi online lại gọi REST lịch sử. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 99. Đặc tả Use Case "Xem lịch sử hội thoại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_099 |
| **Tên Use Case** | Xem lịch sử hội thoại |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Xem lịch sử hội thoại" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Xem tin đã lưu với phân trang (before/take). |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Là member; sys.msg.read. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Không lộ tin ngoài hội thoại; tin đã thu hồi hiển thị trạng thái Recalled.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Xem lịch sử hội thoại» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Mở hội thoại.<br>2. GET messages phân trang.<br>3. Cuộn lên tải thêm. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 100. Đặc tả Use Case "Đánh dấu đã đọc / badge chưa đọc"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_100 |
| **Tên Use Case** | Đánh dấu đã đọc / badge chưa đọc |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đánh dấu đã đọc / badge chưa đọc" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Cập nhật last_read_at và badge unread trên shell. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Là member; sys.msg.read. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Must**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Badge tổng = tổng tin sau last_read_at của mọi hội thoại.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đánh dấu đã đọc / badge chưa đọc» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Mở hội thoại → POST read.<br>2. Cập nhật unread-count.<br>3. SignalR conversationUpdated cho peers (tuỳ chọn). |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 101. Đặc tả Use Case "Đính kèm file trong tin nhắn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_101 |
| **Tên Use Case** | Đính kèm file trong tin nhắn |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Đính kèm file trong tin nhắn" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Gửi tin kèm file đã upload qua SYS file. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• sys.msg.send + quyền file; file thuộc tenant. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Lưu attachment_file_id; người nhận tải theo quyền file.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Đính kèm file trong tin nhắn» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Upload file SYS.<br>2. Gửi message kèm fileId.<br>3. UI hiện preview/tên file. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 102. Đặc tả Use Case "Thu hồi tin nhắn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_102 |
| **Tên Use Case** | Thu hồi tin nhắn |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Thu hồi tin nhắn" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Người gửi thu hồi tin trong cửa sổ thời gian cấu hình. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Là sender; trong TTL thu hồi; sys.msg.send. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Should**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Set recalled_at; đẩy messageRecalled; body ẩn với mọi member.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Thu hồi tin nhắn» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Chọn Thu hồi.<br>2. Validate TTL.<br>3. Cập nhật DB + broadcast. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 103. Đặc tả Use Case "Tìm kiếm tin nhắn"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_103 |
| **Tên Use Case** | Tìm kiếm tin nhắn |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tìm kiếm tin nhắn" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Tìm theo từ khóa trong các hội thoại user tham gia. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• sys.msg.read. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: Chỉ trả tin thuộc hội thoại của user; phân trang.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tìm kiếm tin nhắn» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Nhập từ khóa.<br>2. Search full-text/like.<br>3. Jump tới tin trong hội thoại. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |


**Bảng 104. Đặc tả Use Case "Tắt thông báo hội thoại"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_104 |
| **Tên Use Case** | Tắt thông báo hội thoại |
| **Tác nhân** | End User |
| **Mô tả chức năng** | Cho phép End User thực hiện chức năng "Tắt thông báo hội thoại" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: Mute hội thoại: không tăng badge / không toast. |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [End User] và được cấp quyền RBAC tương ứng.<br>• Là member. |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **Could**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: muted=true trên conversation_member; vẫn đọc được lịch sử.<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «Tắt thông báo hội thoại» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | 1. Bật Mute.<br>2. Tin mới không đẩy toast cho user.<br>3. Unmute khôi phục. |
| **Kịch bản phụ** | 3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log. |

---

## 8. Workflow end-to-end

### WF-SYS-01 — Onboard tenant mới

**Mục tiêu:** Tenant sẵn sàng để admin đầu tiên làm việc và bật module đã mua.

| Bước | Thực hiện bởi | Hành động | UC |
|---:|---|---|---|
| 1 | Ops / Hệ thống | Tạo tenant + hồ sơ công ty | UC_SYS_034 |
| 2 | Ops | Gán gói license (module, hạn, quota) | UC_SYS_046, 045 |
| 3 | Ops | Tạo System Admin đầu tiên + gửi invite | UC_SYS_013, 019 |
| 4 | Admin | Kích hoạt, đặt mật khẩu, (khuyến nghị) bật 2FA | UC_SYS_005, 008 |
| 5 | Admin | Tạo chi nhánh/điểm bán gốc | UC_SYS_036, 037 |
| 6 | Admin | Cấu hình timezone/tiền tệ + email gateway | UC_SYS_041, 088 |
| 7 | Admin | Tạo role chuẩn + gán permission | UC_SYS_023, 026 |
| 8 | Hệ thống | Menu động phản ánh đúng module | UC_SYS_049 |

**Hoàn tất khi:** Admin đăng nhập được, thấy đúng menu module đã mua, gửi email hệ thống test OK.

### WF-SYS-02 — Cấp quyền nhân sự mới vào hệ thống

| Bước | Hành động | UC |
|---:|---|---|
| 1 | Tạo user / gửi invite | UC_SYS_013, 019 |
| 2 | Gán chi nhánh + scope kho (nếu cần) | UC_SYS_017, 028, 029 |
| 3 | Gán role | UC_SYS_027 |
| 4 | User kích hoạt & đăng nhập | UC_SYS_001 |
| 5 | Kiểm tra menu/data scope đúng thiết kế | UC_SYS_049, 028 |

**Hoàn tất khi:** User vào đúng màn hình được phép; không thấy dữ liệu ngoài scope.

### WF-SYS-03 — Upsell / đổi gói module

| Bước | Hành động | UC |
|---:|---|---|
| 1 | Cập nhật gói license (thêm/bớt module, hạn, quota) | UC_SYS_046 |
| 2 | Bật/tắt module runtime | UC_SYS_045 |
| 3 | Menu + API enforce tức thì | UC_SYS_049, 050 |
| 4 | Thông báo admin + audit | UC_SYS_059, 078 |

**Lưu ý:** Tắt module **không xóa dữ liệu**; chỉ ẩn/chặn truy cập theo chính sách lưu trữ.

### WF-SYS-04 — Quên mật khẩu

| Bước | UC |
|---:|---|
| 1. Request OTP/link | UC_SYS_004 |
| 2. Đặt mật khẩu mới | UC_SYS_005 |
| 3. Đăng nhập | UC_SYS_001 |
| 4. Thu hồi phiên cũ | UC_SYS_005 / 010 |

---

## 9. Mô hình dữ liệu domain (conceptual)

| Thực thể | Mô tả | Quan hệ chính |
|---|---|---|
| `Tenant` | Khách hàng thuê bao | 1–n Company/Branch |
| `Company` / `LegalEntity` | Pháp nhân | thuộc Tenant |
| `Branch` / `Outlet` | Chi nhánh / điểm bán | cây tổ chức |
| `Department` / `JobTitle` | Phòng ban / chức danh | master dùng chung |
| `User` | Tài khoản đăng nhập | n–n Role; n–n Branch scope |
| `Role` / `Permission` | RBAC | Role n–n Permission |
| `UserRole` / `DataScope` | Gán quyền & phạm vi | theo User/Role |
| `License` / `LicenseModule` | Gói & module | theo Tenant |
| `Setting` / `Sequence` | Cấu hình & sinh mã | theo Tenant/(Branch) |
| `Notification` / `Template` / `Outbox` | Thông báo | theo User/Event |
| `FileObject` | File đính kèm | gắn entity nghiệp vụ |
| `AuditLog` / `LoginLog` | Nhật ký | theo User/Entity |
| `ApiKey` / `WebhookSubscription` | Tích hợp | theo Tenant |
| `DomainEvent` | Sự kiện bus | publish/subscribe |

### 9.1. Trạng thái User (gợi ý)
`InvitePending` → `Active` → `LockedTemporarily` / `Disabled` → `SoftDeleted`

---

## 10. Quy tắc nghiệp vụ tổng hợp

### 10.1. Xác thực & phiên
- `BR-SYS-AUTH-01`: Không lưu mật khẩu plaintext; chỉ lưu hash mạnh.
- `BR-SYS-AUTH-02`: Vượt N lần đăng nhập sai → khóa tạm theo cấu hình.
- `BR-SYS-AUTH-03`: Đăng xuất / thu hồi phiên làm token hết hiệu lực ngay.
- `BR-SYS-AUTH-04`: Mật khẩu mới phải đạt Password Policy tenant.
- `BR-SYS-AUTH-05`: Reset/đổi mật khẩu phải ghi audit; khuyến nghị thu hồi phiên khác.
- `BR-SYS-AUTH-06`: OTP/link reset có hạn và chỉ dùng một lần.
- `BR-SYS-AUTH-07`: Role bắt buộc 2FA thì chưa setup 2FA không vào được hệ thống.
- `BR-SYS-AUTH-08`: SSO chỉ tạo user mới khi JIT provisioning được bật.
- `BR-SYS-AUTH-09`: Số phiên đồng thời không vượt ngưỡng cấu hình.

### 10.2. User / RBAC / Scope
- `BR-SYS-USER-01`: Không xóa cứng user đã phát sinh dữ liệu; chỉ soft-delete/disable.
- `BR-SYS-USER-02`: Self-service chỉ sửa được field được phép.
- `BR-SYS-USER-03`: Không khóa/xóa hết System Admin cuối cùng của tenant.
- `BR-SYS-USER-04`: Invite token có thời hạn.
- `BR-SYS-RBAC-01`: Mã role duy nhất trong tenant.
- `BR-SYS-RBAC-02`: Permission hệ thống do module đăng ký; không xóa tùy tiện.
- `BR-SYS-RBAC-03`: Quyền hiệu lực enforce ở API, không chỉ ẩn UI.
- `BR-SYS-RBAC-04`: Multi-role → hợp permission (union), trừ khi tenant cấu hình khác.
- `BR-SYS-RBAC-05`: Field nhạy cảm không trả plain value nếu thiếu quyền.
- `BR-SYS-SCOPE-01/02/03`: Mọi truy vấn nghiệp vụ phải áp data scope chi nhánh/kho/phòng ban.

### 10.3. License & cấu hình
- `BR-SYS-LIC-01`: Catalog module là nguồn sự thật cho bán/bật-tắt.
- `BR-SYS-LIC-02`: Không vượt quota user/chi nhánh của gói.
- `BR-SYS-LIC-03`: Module off → ẩn menu + chặn API; không xóa dữ liệu.
- `BR-SYS-LIC-04`: Hết hạn license áp dụng ReadOnly/Block theo policy + grace period.
- `BR-SYS-CFG-01`: Đổi setting quan trọng phải audit.
- `BR-SYS-CFG-02`: Danh mục dùng chung đã tham chiếu chỉ được ngưng, không xóa cứng.
- `BR-SYS-CFG-03`: Sequence sinh mã phải atomic, không trùng.

### 10.4. Thông báo / File / Tích hợp / Audit
- `BR-SYS-NOTI-01`: Gửi theo template Active và rule sự kiện.
- `BR-SYS-NOTI-02`: Không cho user tắt cảnh báo bảo mật bắt buộc.
- `BR-SYS-FILE-01`: Validate loại/dung lượng; soft-delete mặc định.
- `BR-SYS-FILE-02`: Authorize theo quyền entity/file trước khi download.
- `BR-SYS-IE-01/02`: Import có preview lỗi; export tôn trọng data scope + audit.
- `BR-SYS-INT-01`: API Key least privilege; che secret.
- `BR-SYS-INT-02`: Webhook có chữ ký/secret; retry có giới hạn.
- `BR-SYS-INT-03`: Consumer event phải idempotent.
- `BR-SYS-AUD-01`: Thay đổi critical (user/role/license/password/integration) bắt buộc có audit.
- `BR-SYS-SEC-01`: Chính sách phiên/IP áp dụng nhất quán.
- `BR-SYS-UX-01`: Thiếu bản dịch → fallback ngôn ngữ mặc định (VI).

---

## 11. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Bảo mật | TLS; hash mật khẩu; hỗ trợ 2FA; chống brute-force; secret không log plaintext |
| Hiệu năng | Đăng nhập p95 < 2s; kiểm tra permission cache được; menu build < 500ms sau cache ấm |
| Độ tin cậy | Event bus có retry/poison; gửi mail không làm fail giao dịch nguồn |
| Audit | Giữ audit/login log tối thiểu 12 tháng (cấu hình được) |
| Đa thuê bao | Cách ly dữ liệu theo TenantId trên mọi bảng SYS |
| Khả dụng | Dịch vụ auth là critical path — cần HA khi production |
| Usability | Form lỗi rõ field; tiếng Việt mặc định |
| Quan sát | Metric đăng nhập thất bại, quota license, webhook fail rate |

---

## 12. Tích hợp & sự kiện

### 12.1. Sự kiện domain (logical)
| Event | Khi nào | Subscriber ví dụ |
|---|---|---|
| `UserCreated` / `UserInvited` | Tạo/mời user | Notification |
| `UserDisabled` / `UserDeleted` | Khóa/xóa mềm | Thu hồi session, API key |
| `RolePermissionsChanged` | Đổi ma trận quyền | Invalidate cache quyền |
| `LicenseChanged` | Đổi gói/module | Menu, API enforce, BI dataset |
| `NotificationRequested` | Module yêu cầu gửi thông báo | Email/SMS/Push workers |
| `FileScanned` | Xong quét virus | Đổi trạng thái FileObject |

### 12.2. Hợp đồng với module nghiệp vụ
Mỗi module khi bật phải đăng ký tối thiểu:
1. Danh mục `Permission`
2. Menu entries
3. Sequence/doc types (nếu có chứng từ)
4. (Optional) sensitive fields + event handlers

---

## 13. Phân quyền & bảo mật

### 13.1. Permission catalog đề xuất
```
sys.user.manage | sys.user.view
sys.role.manage | sys.permission.assign
sys.org.manage
sys.license.manage
sys.setting.manage
sys.file.manage
sys.audit.view
sys.integration.manage
sys.notify.manage
sys.security.manage
```

### 13.2. Nguyên tắc
- Deny by default.
- Enforce tại API gateway/middleware + kiểm tra trong use-case.
- Tách quyền xem audit khỏi quyền sửa phân quyền.
- Môi trường production: bắt buộc HTTPS; khuyến nghị bắt buộc 2FA cho admin.

---

## 14. Báo cáo & KPI vận hành SYS

| KPI | Mục đích |
|---|---|
| User active / quota | Kiểm soát gói bán |
| Chi nhánh active / quota | Kiểm soát gói bán |
| Tỷ lệ đăng nhập thất bại | An ninh |
| Số user bị khóa tạm | Brute-force / hỗ trợ |
| License days-to-expire | Gia hạn |
| Webhook failure rate | Sức khỏe tích hợp |
| Email/SMS fail rate | Sức khỏe thông báo |
| Số thay đổi phân quyền / tuần | Kiểm toán |

---

## 15. Giả định, rủi ro, câu hỏi mở

### 15.1. Giả định
- Một khách hàng thương mại = một Tenant (multi-company nằm trong tenant nếu cần).
- Module nghiệp vụ tuân thủ hợp đồng đăng ký permission/menu/sequence.
- Email gateway có sẵn trước khi dùng invite/reset password trên production.

### 15.2. Rủi ro
| Rủi ro | Mức | Hướng xử lý |
|---|---|---|
| Admin quên bật 2FA | Cao | Checklist go-live + cảnh báo |
| Cấu hình scope sai → lộ dữ liệu chi nhánh | Cao | Bộ role template + test case scope |
| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |
| Spam OTP quên mật khẩu | Trung bình | Rate limit theo IP/định danh |

### 15.3. Câu hỏi cần chốt
1. Phase 1 có bắt buộc tách DB theo tenant hay chỉ lọc `TenantId`?
2. Khi hết hạn license: **ReadOnly** hay **Block login** (trừ admin gia hạn)?
3. SSO/JIT có nằm Phase 1 không, hay để Phase 2?
4. Giữ dữ liệu module đã tắt trong bao lâu trước khi cho phép purge?
5. Có cho phép một user thuộc nhiều tenant (chuyển context) không?

---

## 16. Tiêu chí nghiệm thu & truy vết

### 16.1. Điều kiện chấp nhận module SYS
1. 100% UC **Must** pass UAT.
2. WF-SYS-01..04 chạy thành công trên môi trường demo.
3. Kiểm thử phủ: login/logout/reset password/lock after N fails.
4. Kiểm thử license: tắt module → menu mất + API 403; dữ liệu vẫn còn.
5. Kiểm thử RBAC + data scope với ≥ 3 role và ≥ 2 chi nhánh.
6. Audit có before/after cho đổi quyền và reset mật khẩu.
7. Email gateway test gửi thành công.
8. Không còn đặc tả UC dùng luồng khuôn mẫu sai (đăng xuất ≠ dashboard…).
9. Nhắn tin realtime A→B nhận không F5; unread badge đúng; user ngoài hội thoại 403.

### 16.2. Truy vết
| Artifact | Vị trí |
|---|---|
| Catalog chức năng | `../../00. Tổng quan/cay_chuc_nang_data.py` |
| Excel tổng hợp | `../../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx` |
| Chuẩn SRS | `../00_CHUAN_VIET_SRS.md` |
| Bản SRS này | `SRS_SYS_v1.1.md` |
| UC IDs | `UC_SYS_001` … `UC_SYS_104` |

---

## Phụ lục A — Role template khởi tạo (gợi ý)

| Role | Mục đích | Nhóm quyền gợi ý |
|---|---|---|
| Super Admin | Toàn quyền tenant | tất cả `sys.*` |
| Security Admin | Bảo mật & phân quyền | `sys.security.*`, `sys.role.*`, `sys.audit.view` |
| Org Admin | Tổ chức | `sys.org.manage`, `sys.user.view` |
| Support Agent | Hỗ trợ user | `sys.user.manage` (hạn chế), không `sys.license.manage` |
| End User mặc định | Người dùng nghiệp vụ | self-service password/notify/file/`sys.msg.*` |

---

## Phụ lục B — SYS-13 Nhắn tin realtime (xem mục 7.13)

> Phân biệt: **SYS-07 Thông báo** = hệ thống → user (email/SMS/in-app). **SYS-13** = user ↔ user (chat nội bộ, SignalR).  
> Đặc tả kỹ thuật living: `Làm/Source/docs/04_MSG_REALTIME.md`.

| Mã UC | Tên | Ưu tiên | Ghi chú |
|---|---|---|---|
| `UC_SYS_095` | Tạo hội thoại 1-1 | Must | Cùng tenant |
| `UC_SYS_096` | Tạo hội thoại nhóm | Should | Nhiều thành viên |
| `UC_SYS_097` | Gửi tin nhắn realtime | Must | Persist + đẩy SignalR |
| `UC_SYS_098` | Nhận tin nhắn realtime (SignalR) | Must | Hub `/hubs/msg` · cấm poll |
| `UC_SYS_099` | Xem lịch sử hội thoại | Must | Phân trang |
| `UC_SYS_100` | Đánh dấu đã đọc / badge chưa đọc | Must | Shell badge |
| `UC_SYS_101` | Đính kèm file trong tin nhắn | Should | Dùng SYS file |
| `UC_SYS_102` | Thu hồi tin nhắn | Should | Trong cửa sổ thời gian |
| `UC_SYS_103` | Tìm kiếm tin nhắn | Could | |
| `UC_SYS_104` | Tắt thông báo hội thoại | Could | Mute |

**Permission:** `sys.msg.read` · `sys.msg.send` · (tuỳ chọn) `sys.msg.manage`.  
**License:** thuộc hard path SYS — không module license riêng.  
**Không gồm:** CRM omnichannel / chatbot khách hàng (`UC_CRM_039`…).

---

*Hết tài liệu SRS-SYS-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang module tiếp theo.*
