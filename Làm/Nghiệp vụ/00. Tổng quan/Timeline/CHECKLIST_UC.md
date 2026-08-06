# Checklist UC toàn bộ — Pum's ERP

| Thuộc tính | Giá trị |
| --- | --- |
| Mã | `CHECKLIST-UC-v2` |
| Cập nhật lần | 06/08/2026 |
| Nguồn catalog | [`cay_chuc_nang_data.py`](../cay_chuc_nang_data.py) |
| Tiến độ máy | [`uc_progress.json`](./uc_progress.json) |
| Sinh lại | `python Timeline/build_uc_checklist.py` → `CHECKLIST_UC.md` |
| Rà soát chất lượng | [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) |
| Tổng UC | **1.092** |
| Đã xong DoD khung `[x]` | **724** (66.3%) — API/UI đủ dùng Cap-1/2, **không** = production chỉnh chu |
| Partial `[~]` | **0** |
| Chưa `[ ]` | **368** |
| Must (DoD khung) | **616/616** `[x]` · còn **0** (một phần vẫn stub — xem cột Rủi ro / Rà xoát UC) |
| Test BE (xUnit chạy được) | **464+** pass — nhiều case Batch assert giả; slice Cap-2 mới = InMemory thật |
| Test FE | **~37** node:test (calc/helpers CRM+POS) — chưa Jest/Vitest/Playwright E2E |
| FE `page.tsx` (app) | **~99** (kể cả redirect / catch-all) |
| Kế hoạch giai đoạn | [CHECKLIST_TIEN_DO_GIAI_DOAN.md](../CHECKLIST_TIEN_DO_GIAI_DOAN.md) |

> Living checklist — **mỗi UC một dòng**. Cập nhật `uc_progress.json` rồi chạy lại script. **Không** ghi đè tay hàng loạt / không đánh 100% khi còn stub. Cột `[x]` = DoD khung; xem cột **Rủi ro** và [`Rà xoát UC.md`](./Rà%20xoát%20UC.md).

### Quy ước cột

| Cột | Nghĩa |
| --- | --- |
| Ưu tiên | Must ← Bắt buộc · Should ← Cao · Could ← Trung bình · Won't ← Thấp |
| Xong? | `[x]` DoD khung · `[~]` partial · `[ ]` chưa |
| % | 0–100 độ sâu (Day-1/stub thường <100) |
| `[x]` / `[~]` / `[ ]` (bảng A) | Đếm theo module từ `uc_progress.json` |
| Must [x] / Must còn | Must đã đánh xong vs còn lại |
| Should còn | Should chưa `[x]` |
| Khác còn | Could + Won't chưa `[x]` |
| Avg % | Trung bình `pct` của UC đã `[x]`/`[~]` trong module |
| FE pages | Số `page.tsx` thực tế (không = số UC) |
| Rủi ro | Ghi chú rà soát 06/08/2026 (stub / API chết / mỏng) |

## A. Tổng hợp theo module

| Module | Tổng | [x] | [~] | [ ] | % | Must [x] | Must còn | Should còn | Khác còn | Avg % | FE pages | Rủi ro |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| SYS | 104 | 91 | 0 | 13 | 87.5% | 59 | 0 | 2 | 11 | 90.8 | 10 | Cao hơn Day-1; trusted-devices FE mock; OTP/2FA còn stub |
| HRM | 187 | 170 | 0 | 17 | 90.9% | 124 | 0 | 9 | 8 | 83.0 | 18 | Cap-2 dày nhất; một phần Day-1 khung |
| LMS | 74 | 43 | 0 | 31 | 58.1% | 37 | 0 | 22 | 9 | 87.1 | 8 | Cert/thanh toán còn mock |
| CRM | 131 | 68 | 0 | 63 | 51.9% | 66 | 0 | 52 | 11 | 89.1 | 7 | Báo giá Email/PDF text thật + marketing Cap-2; còn Should omni |
| POS | 72 | 45 | 0 | 27 | 62.5% | 40 | 0 | 21 | 6 | 88.1 | 6 | BOM→INV + stock alerts + đóng ca→FIN + sync catalog INV→POS + in HĐ/BC ca wired thật |
| PUR | 52 | 27 | 0 | 25 | 51.9% | 24 | 0 | 22 | 3 | 88.9 | 5 | Đẩy INV/AP + xuất PO CSV wired thật; RFQ/hợp đồng còn thiếu |
| INV | 70 | 51 | 0 | 19 | 72.9% | 51 | 0 | 18 | 1 | 87.3 | 6 | Cap-2 FEFO/hold/HSD có; một phần UC còn thiếu |
| LOG | 39 | 26 | 0 | 13 | 66.7% | 23 | 0 | 11 | 2 | 89.6 | 4 | GPS/route entity mỏng |
| MFG | 46 | 25 | 0 | 21 | 54.3% | 22 | 0 | 18 | 3 | 88.8 | 3 | Đẩy giá thành INV + JE WIP→TP thật; ca/báo cáo nâng cao còn thiếu |
| FSM | 50 | 28 | 0 | 22 | 56.0% | 27 | 0 | 20 | 2 | 88.2 | 4 | Cap-2 parts/ticket; APP mobile Must ngoài scope |
| PJM | 42 | 29 | 0 | 13 | 69.0% | 27 | 0 | 13 | 0 | 88.6 | 3 | Cap-2 progress/cost; FE mỏng (~3 trang) |
| FIN | 83 | 53 | 0 | 30 | 63.9% | 53 | 0 | 27 | 3 | 91.7 | 9 | BT Auto (Source=Auto) wired thật; cash-flow còn đơn giản hóa |
| AST | 34 | 21 | 0 | 13 | 61.8% | 19 | 0 | 10 | 3 | 89.5 | 5 | Đẩy BT KH → FIN JE thật (Posted cân Nợ/Có); IoT/thanh lý nâng cao còn thiếu |
| WF | 40 | 22 | 0 | 18 | 55.0% | 21 | 0 | 15 | 3 | 80.9 | 5 | Cap-1/2 duyệt; mobile/WF nâng cao còn thiếu |
| BI | 30 | 14 | 0 | 16 | 46.7% | 12 | 0 | 10 | 6 | 87.9 | 3 | ETL/KPI/widget/export stub |
| PRT | 38 | 11 | 0 | 27 | 28.9% | 11 | 0 | 12 | 15 | 85.0 | 3 | Login/forgot stub; portal mỏng (~3 trang) |
| **TỔNG** | **1.092** | **724** | **0** | **368** | **66.3%** | **616** | **0** | **282** | **86** | **87.8** | **99** | Xem [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) |

---

## SYS (91/104)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_SYS_001` | Xác thực & phiên làm việc | Đăng nhập hệ thống | Must | [x] | 100 | M1 · login JWT |
| `UC_SYS_002` | Xác thực & phiên làm việc | Đăng xuất | Must | [x] | 100 | N1 · logout API + FE |
| `UC_SYS_003` | Xác thực & phiên làm việc | Đổi mật khẩu | Must | [x] | 100 | N1 · change-password |
| `UC_SYS_004` | Xác thực & phiên làm việc | Quên mật khẩu – gửi OTP/link | Must | [x] | 90 | N2 · forgot OTP stub log |
| `UC_SYS_005` | Xác thực & phiên làm việc | Đặt lại mật khẩu sau OTP | Must | [x] | 90 | N2 · reset with OTP |
| `UC_SYS_006` | Xác thực & phiên làm việc | Chính sách độ mạnh mật khẩu | Must | [x] | 100 | N1 · password.policy setting |
| `UC_SYS_007` | Xác thực & phiên làm việc | Khóa tài khoản sau N lần sai | Must | [x] | 100 | N1 · FailedLoginCount lock |
| `UC_SYS_008` | Xác thực & phiên làm việc | Xác thực 2 bước (2FA) | Should | [x] | 80 | N2 · 2FA begin/confirm Dev |
| `UC_SYS_009` | Xác thực & phiên làm việc | Đăng nhập SSO / OAuth | Could | [ ] | 0 |  |
| `UC_SYS_010` | Xác thực & phiên làm việc | Quản lý phiên đang hoạt động | Should | [x] | 90 | N2 · user_session list |
| `UC_SYS_011` | Xác thực & phiên làm việc | Giới hạn số phiên đồng thời | Should | [x] | 90 | N2 · max 5 sessions |
| `UC_SYS_012` | Xác thực & phiên làm việc | Ghi nhớ thiết bị tin cậy | Won't | [ ] | 0 |  |
| `UC_SYS_013` | Người dùng | Tạo người dùng | Must | [x] | 90 | M1 · upsert + multi-dept/job level |
| `UC_SYS_014` | Người dùng | Cập nhật thông tin người dùng | Must | [x] | 90 | M1 · upsert + multi-dept/job level |
| `UC_SYS_015` | Người dùng | Khóa / mở khóa người dùng | Must | [x] | 70 | M1 · status Locked/Disabled trên SideSheet |
| `UC_SYS_016` | Người dùng | Xóa mềm người dùng | Must | [x] | 100 | N1 · soft delete user |
| `UC_SYS_017` | Người dùng | Gán người dùng vào chi nhánh | Must | [x] | 90 | N1 · validate primaryOrgUnitId |
| `UC_SYS_018` | Người dùng | Reset mật khẩu bởi Admin | Must | [x] | 100 | N1 · admin reset-password |
| `UC_SYS_019` | Người dùng | Mời người dùng qua email | Should | [x] | 70 | N2 · invite via forgot stub |
| `UC_SYS_020` | Người dùng | Import danh sách người dùng Excel | Should | [x] | 100 | N2 · import users = 072 |
| `UC_SYS_021` | Người dùng | Tìm kiếm / lọc người dùng | Must | [x] | 80 | M1 · list + data scope |
| `UC_SYS_022` | Người dùng | Xuất danh sách người dùng | Should | [x] | 100 | N2 · export users = 074 |
| `UC_SYS_023` | Vai trò & phân quyền | Tạo / sửa / ngưng vai trò (Role) | Must | [x] | 90 | M1 · API + FE roles |
| `UC_SYS_024` | Vai trò & phân quyền | Sao chép vai trò | Should | [x] | 100 | N2 · copy role |
| `UC_SYS_025` | Vai trò & phân quyền | Quản lý danh mục quyền (Permission) | Must | [x] | 100 | Catalog quyền chỉ xem · seed khi ship feature |
| `UC_SYS_026` | Vai trò & phân quyền | Gán quyền vào vai trò | Must | [x] | 100 | M1 · set role permissions · PermissionPicker |
| `UC_SYS_027` | Vai trò & phân quyền | Gán người dùng vào vai trò | Must | [x] | 100 | M1 · multi-role · union quyền |
| `UC_SYS_028` | Vai trò & phân quyền | Phân quyền dữ liệu theo chi nhánh | Must | [x] | 80 | M1 · data scope |
| `UC_SYS_029` | Vai trò & phân quyền | Phân quyền dữ liệu theo kho / điểm | Must | [x] | 60 | N2 · sales_point scope stub entity |
| `UC_SYS_030` | Vai trò & phân quyền | Phân quyền theo phòng ban | Should | [x] | 80 | M1 · data scope dept |
| `UC_SYS_031` | Vai trò & phân quyền | Quyền theo trường nhạy cảm | Should | [ ] | 0 |  |
| `UC_SYS_032` | Vai trò & phân quyền | Xem ma trận phân quyền tổng hợp | Should | [x] | 100 | N2 · permission-matrix |
| `UC_SYS_033` | Vai trò & phân quyền | Nhật ký thay đổi phân quyền | Must | [x] | 100 | N2 · permission_change_log |
| `UC_SYS_034` | Tổ chức & đa chi nhánh | Quản lý công ty / tenant | Must | [x] | 100 | N1 · GET/PUT tenant |
| `UC_SYS_035` | Tổ chức & đa chi nhánh | Quản lý pháp nhân / công ty con | Should | [x] | 100 | N2 · legal_entity |
| `UC_SYS_036` | Tổ chức & đa chi nhánh | Quản lý chi nhánh | Must | [x] | 90 | M1 · org-units FE |
| `UC_SYS_037` | Tổ chức & đa chi nhánh | Quản lý điểm bán / cửa hàng | Must | [x] | 100 | N2 · sales_point CRUD |
| `UC_SYS_038` | Tổ chức & đa chi nhánh | Quản lý phòng ban | Must | [x] | 90 | M1 · departments FE |
| `UC_SYS_039` | Tổ chức & đa chi nhánh | Quản lý chức danh | Must | [x] | 80 | M1 · job level/title |
| `UC_SYS_040` | Tổ chức & đa chi nhánh | Sơ đồ tổ chức trực quan | Should | [x] | 90 | N2 · org-chart API |
| `UC_SYS_041` | Tổ chức & đa chi nhánh | Cấu hình múi giờ / ngôn ngữ / tiền tệ | Must | [x] | 100 | N1 · TZ/locale/currency |
| `UC_SYS_042` | Tổ chức & đa chi nhánh | Cấu hình định dạng ngày số | Should | [x] | 70 | N2 · locale/date via tenant settings |
| `UC_SYS_043` | Tổ chức & đa chi nhánh | Quản lý địa chỉ / tỉnh thành | Must | [x] | 100 | N2 · province CRUD |
| `UC_SYS_044` | License & module bán hàng | Khai báo module trong hệ thống | Must | [x] | 100 | N1 · GET modules catalog |
| `UC_SYS_045` | License & module bán hàng | Bật / tắt module theo tenant | Must | [x] | 100 | M1 · license middleware |
| `UC_SYS_046` | License & module bán hàng | Quản lý gói license | Must | [x] | 100 | N1 · license CRUD |
| `UC_SYS_047` | License & module bán hàng | Giới hạn số user / chi nhánh theo gói | Must | [x] | 100 | N1 · MaxUsers/MaxOrgUnits enforce |
| `UC_SYS_048` | License & module bán hàng | Cảnh báo / gia hạn license | Must | [x] | 100 | N1 · license/status |
| `UC_SYS_049` | License & module bán hàng | Menu động theo module + quyền | Must | [x] | 100 | M1 · shell menu |
| `UC_SYS_050` | License & module bán hàng | Ẩn API module chưa mua | Must | [x] | 100 | M1 · license API gate |
| `UC_SYS_051` | Cấu hình hệ thống | Tham số cấu hình toàn cục | Must | [x] | 100 | N1 · system_setting |
| `UC_SYS_052` | Cấu hình hệ thống | Cấu hình theo chi nhánh | Should | [x] | 70 | N2 · settings key scoped |
| `UC_SYS_053` | Cấu hình hệ thống | Danh mục dùng chung | Must | [x] | 100 | N1 · lookup category/item |
| `UC_SYS_054` | Cấu hình hệ thống | Mẫu số chứng từ | Must | [x] | 100 | N1 · number_sequence |
| `UC_SYS_055` | Cấu hình hệ thống | Sinh mã tự động | Must | [x] | 100 | N1 · NextNumberAsync |
| `UC_SYS_056` | Cấu hình hệ thống | Cấu hình mẫu email / SMS | Must | [x] | 100 | N2 · message_template |
| `UC_SYS_057` | Cấu hình hệ thống | Cấu hình lịch làm việc | Should | [x] | 100 | N2 · work_calendar |
| `UC_SYS_058` | Cấu hình hệ thống | Quản lý phiên bản cấu hình | Could | [ ] | 0 |  |
| `UC_SYS_059` | Thông báo | Thông báo in-app | Must | [x] | 90 | N1 · in-app notifications |
| `UC_SYS_060` | Thông báo | Gửi email hệ thống | Must | [x] | 60 | N2 · email stub via message_template |
| `UC_SYS_061` | Thông báo | Gửi SMS / messaging | Should | [x] | 60 | N2 · SMS stub channel |
| `UC_SYS_062` | Thông báo | Push notification mobile | Should | [ ] | 0 |  |
| `UC_SYS_063` | Thông báo | Cấu hình sự kiện kích hoạt | Must | [x] | 90 | N1 · notification_rule |
| `UC_SYS_064` | Thông báo | Tùy chọn thông báo cá nhân | Could | [ ] | 0 |  |
| `UC_SYS_065` | Thông báo | Nhật ký gửi thông báo | Should | [x] | 80 | N2 · integration_call_log |
| `UC_SYS_066` | File & tài liệu | Upload file | Must | [x] | 90 | M1 · local storage |
| `UC_SYS_067` | File & tài liệu | Tải xuống / xem trước file | Must | [x] | 80 | M1 · download API |
| `UC_SYS_068` | File & tài liệu | Quản lý thư mục tài liệu | Should | [x] | 100 | N2 · file_folder |
| `UC_SYS_069` | File & tài liệu | Phân quyền file theo đối tượng | Should | [x] | 70 | N2 · file linkedEntity ACL light |
| `UC_SYS_070` | File & tài liệu | Xóa mềm / khôi phục file | Should | [x] | 100 | N2 · soft delete/restore file |
| `UC_SYS_071` | File & tài liệu | Quét virus / bảo mật file | Could | [ ] | 0 |  |
| `UC_SYS_072` | Import / Export | Import Excel/CSV theo mẫu | Must | [x] | 100 | N1 · import users CSV |
| `UC_SYS_073` | Import / Export | Tải file mẫu import | Must | [x] | 100 | N1 · import template |
| `UC_SYS_074` | Import / Export | Export Excel | Must | [x] | 90 | N1 · export users CSV |
| `UC_SYS_075` | Import / Export | Export PDF | Must | [x] | 50 | N2 · CSV export as PDF substitute Day-1 |
| `UC_SYS_076` | Import / Export | Lịch sử job import/export | Should | [x] | 60 | N2 · import result counts as job |
| `UC_SYS_077` | Import / Export | Xuất dữ liệu hàng loạt | Could | [ ] | 0 |  |
| `UC_SYS_078` | Audit & bảo mật | Nhật ký thao tác người dùng | Must | [x] | 90 | M1 · audit interceptor |
| `UC_SYS_079` | Audit & bảo mật | Nhật ký đăng nhập / thất bại | Must | [x] | 100 | N1 · login_audit |
| `UC_SYS_080` | Audit & bảo mật | Xem chi tiết thay đổi field | Should | [x] | 70 | N2 · PermissionChangeLog detail |
| `UC_SYS_081` | Audit & bảo mật | Xuất audit log | Should | [x] | 70 | N2 · login-audits API |
| `UC_SYS_082` | Audit & bảo mật | Quản lý IP allow/deny | Won't | [ ] | 0 |  |
| `UC_SYS_083` | Audit & bảo mật | Chính sách hết hạn phiên | Must | [x] | 90 | N1 · SessionMinutes / JWT exp |
| `UC_SYS_084` | Tích hợp nền tảng | Quản lý API Key | Should | [x] | 100 | N2 · api_key |
| `UC_SYS_085` | Tích hợp nền tảng | Quản lý Webhook outbound | Should | [x] | 100 | N2 · webhook |
| `UC_SYS_086` | Tích hợp nền tảng | Nhật ký gọi API / webhook | Should | [x] | 100 | N2 · integration logs |
| `UC_SYS_087` | Tích hợp nền tảng | Hàng đợi sự kiện liên module | Must | [x] | 70 | G4 · outbox_message + dispatcher |
| `UC_SYS_088` | Tích hợp nền tảng | Kết nối email gateway | Must | [x] | 70 | N2 · external_integration Email |
| `UC_SYS_089` | Tích hợp nền tảng | Kết nối SMS gateway | Should | [x] | 70 | N2 · external_integration SMS |
| `UC_SYS_090` | Tích hợp nền tảng | Cấu hình tích hợp bên ngoài | Should | [x] | 100 | N2 · external_integration |
| `UC_SYS_091` | Đa ngôn ngữ & giao diện | Quản lý gói ngôn ngữ | Should | [x] | 100 | N2 · locale_pack |
| `UC_SYS_092` | Đa ngôn ngữ & giao diện | Đổi ngôn ngữ giao diện | Should | [x] | 100 | N2 · PUT me/locale |
| `UC_SYS_093` | Đa ngôn ngữ & giao diện | Tùy chỉnh theme / logo | Could | [ ] | 0 |  |
| `UC_SYS_094` | Đa ngôn ngữ & giao diện | Trang chủ theo vai trò | Could | [ ] | 0 |  |
| `UC_SYS_095` | Nhắn tin realtime | Tạo hội thoại 1-1 | Must | [x] | 100 | G4.9 · Direct conversation API/FE |
| `UC_SYS_096` | Nhắn tin realtime | Tạo hội thoại nhóm | Should | [x] | 100 | N1 · FE group chat |
| `UC_SYS_097` | Nhắn tin realtime | Gửi tin nhắn realtime | Must | [x] | 100 | G4.9 · send message REST |
| `UC_SYS_098` | Nhắn tin realtime | Nhận tin nhắn realtime (SignalR) | Must | [x] | 100 | G4.9 · SignalR /hubs/msg |
| `UC_SYS_099` | Nhắn tin realtime | Xem lịch sử hội thoại | Must | [x] | 100 | G4.9 · message history |
| `UC_SYS_100` | Nhắn tin realtime | Đánh dấu đã đọc / badge chưa đọc | Must | [x] | 100 | G4.9 · read + unread badge |
| `UC_SYS_101` | Nhắn tin realtime | Đính kèm file trong tin nhắn | Should | [x] | 80 | N1 · attachmentFileId on send |
| `UC_SYS_102` | Nhắn tin realtime | Thu hồi tin nhắn | Should | [x] | 100 | N1 · recall message |
| `UC_SYS_103` | Nhắn tin realtime | Tìm kiếm tin nhắn | Could | [ ] | 0 |  |
| `UC_SYS_104` | Nhắn tin realtime | Tắt thông báo hội thoại | Could | [ ] | 0 |  |

## HRM (170/187)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_HRM_001` | Cơ cấu tổ chức nhân sự | Tạo sơ đồ tổ chức công ty | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_002` | Cơ cấu tổ chức nhân sự | Quản lý khối vận hành | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_003` | Cơ cấu tổ chức nhân sự | Quản lý khối sản xuất | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_004` | Cơ cấu tổ chức nhân sự | Quản lý danh mục điểm bán | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_005` | Cơ cấu tổ chức nhân sự | Quản lý bộ phận trong đơn vị | Should | [ ] | 0 |  |
| `UC_HRM_006` | Cơ cấu tổ chức nhân sự | Khai báo giờ làm việc theo đơn vị | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_007` | Cơ cấu tổ chức nhân sự | Quản lý chức danh nhân sự | Must | [x] | 100 | M1 · job titles |
| `UC_HRM_008` | Cơ cấu tổ chức nhân sự | Quản lý vị trí công việc | Should | [ ] | 0 |  |
| `UC_HRM_009` | Cơ cấu tổ chức nhân sự | Quản lý loại nhân sự | Must | [x] | 100 | M1 · employee types |
| `UC_HRM_010` | Cơ cấu tổ chức nhân sự | Quản lý cấp bậc / level | Should | [x] | 80 | M1 · job levels SYS |
| `UC_HRM_011` | Cơ cấu tổ chức nhân sự | Định nghĩa trung tâm chi phí NS | Should | [ ] | 0 |  |
| `UC_HRM_012` | Hồ sơ nhân sự | Sinh mã nhân sự tự động | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_013` | Hồ sơ nhân sự | Tạo hồ sơ nhân sự mới | Must | [x] | 100 | M1 · employee upsert |
| `UC_HRM_014` | Hồ sơ nhân sự | Cập nhật thông tin cá nhân | Must | [x] | 100 | M1 · employee upsert |
| `UC_HRM_015` | Hồ sơ nhân sự | NV tự cập nhật hồ sơ | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_016` | Hồ sơ nhân sự | Upload ảnh đại diện | Could | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_017` | Hồ sơ nhân sự | Upload giấy tờ tùy thân | Must | [x] | 90 | L2 · employee_document upload/list/delete |
| `UC_HRM_018` | Hồ sơ nhân sự | Gắn nhân sự vào đơn vị chính | Must | [x] | 100 | M1 · org on employee |
| `UC_HRM_019` | Hồ sơ nhân sự | Gắn nhân sự vào bộ phận | Must | [x] | 100 | M1 · dept on employee |
| `UC_HRM_020` | Hồ sơ nhân sự | Gắn chức danh / level | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_021` | Hồ sơ nhân sự | Gắn loại nhân sự | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_022` | Hồ sơ nhân sự | Gắn nhiều nhãn hồ sơ | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_023` | Hồ sơ nhân sự | Quản lý người thân / liên hệ khẩn | Should | [ ] | 0 |  |
| `UC_HRM_024` | Hồ sơ nhân sự | Quản lý trình độ / kỹ năng | Could | [ ] | 0 |  |
| `UC_HRM_025` | Hồ sơ nhân sự | Tìm kiếm nhân sự đa tiêu chí | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_026` | Hồ sơ nhân sự | Xuất danh sách nhân sự Excel | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_027` | Hồ sơ nhân sự | Khóa hồ sơ đã nghỉ | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_028` | Hồ sơ nhân sự | Xem hồ sơ theo quyền | Must | [x] | 90 | M1 · data scope |
| `UC_HRM_029` | Trạng thái & biến động nhân sự | Chuyển trạng thái Thử việc | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_030` | Trạng thái & biến động nhân sự | Chuyển trạng thái Chính thức | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_031` | Trạng thái & biến động nhân sự | Chuyển trạng thái Tạm nghỉ | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_032` | Trạng thái & biến động nhân sự | Chuyển trạng thái Nghỉ việc | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_033` | Trạng thái & biến động nhân sự | Lịch sử thay đổi trạng thái | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_034` | Trạng thái & biến động nhân sự | Điều chuyển đơn vị / bộ phận | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_035` | Trạng thái & biến động nhân sự | Thăng chức / đổi chức danh | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_036` | Trạng thái & biến động nhân sự | Cảnh báo sắp hết hạn thử việc | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_037` | Trạng thái & biến động nhân sự | Báo cáo biến động nhân sự | Should | [ ] | 0 |  |
| `UC_HRM_038` | Hợp đồng lao động | Tạo hợp đồng lao động | Must | [x] | 90 | M1 · contract upsert |
| `UC_HRM_039` | Hợp đồng lao động | Tạo phụ lục hợp đồng | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_040` | Hợp đồng lao động | Upload bản scan hợp đồng | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_041` | Hợp đồng lao động | Gia hạn hợp đồng | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_042` | Hợp đồng lao động | Thanh lý / chấm dứt hợp đồng | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_043` | Hợp đồng lao động | Cảnh báo hết hạn hợp đồng | Must | [x] | 80 | N1 · org/status/code/export/HĐ Day-1 |
| `UC_HRM_044` | Hợp đồng lao động | In / xuất mẫu hợp đồng | Should | [ ] | 0 |  |
| `UC_HRM_045` | Hợp đồng lao động | Quản lý lương hợp đồng | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_HRM_046` | Hợp đồng lao động | Lịch sử hợp đồng theo nhân sự | Must | [x] | 80 | M1 · contract list · N1 |
| `UC_HRM_047` | Tuyển dụng – nhu cầu | Tạo phiếu đề xuất tuyển dụng | Must | [x] | 80 | L2 · recruit demand |
| `UC_HRM_048` | Tuyển dụng – nhu cầu | Chọn vị trí & số lượng cần tuyển | Must | [x] | 80 | L2 · recruit demand |
| `UC_HRM_049` | Tuyển dụng – nhu cầu | Nhập lý do tuyển dụng | Must | [x] | 80 | L2 · recruit demand |
| `UC_HRM_050` | Tuyển dụng – nhu cầu | Gửi phiếu đề xuất đi duyệt | Must | [x] | 80 | L2 · recruit demand |
| `UC_HRM_051` | Tuyển dụng – nhu cầu | Duyệt / từ chối đề xuất | Must | [x] | 80 | L2 · recruit demand |
| `UC_HRM_052` | Tuyển dụng – nhu cầu | Xem lịch sử duyệt đề xuất | Should | [x] | 80 | L2 · recruit demand |
| `UC_HRM_053` | Tuyển dụng – nhu cầu | Đóng / hủy phiếu đề xuất | Should | [x] | 80 | L2 · recruit demand |
| `UC_HRM_054` | Tuyển dụng – đăng tin & ứng viên | Tạo tin tuyển từ phiếu đã duyệt | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_055` | Tuyển dụng – đăng tin & ứng viên | Ghi nhận kênh đăng tuyển | Should | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_056` | Tuyển dụng – đăng tin & ứng viên | Nhập hồ sơ ứng viên | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_057` | Tuyển dụng – đăng tin & ứng viên | Upload file CV | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_058` | Tuyển dụng – đăng tin & ứng viên | Import ứng viên hàng loạt | Could | [ ] | 0 |  |
| `UC_HRM_059` | Tuyển dụng – đăng tin & ứng viên | Sơ loại ứng viên | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_060` | Tuyển dụng – đăng tin & ứng viên | Chuyển ứng viên cho đơn vị đánh giá | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_061` | Tuyển dụng – đăng tin & ứng viên | Form đánh giá ứng viên | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_062` | Tuyển dụng – đăng tin & ứng viên | Từ chối / chấp nhận ứng viên | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_063` | Tuyển dụng – đăng tin & ứng viên | Pipeline trạng thái ứng viên | Must | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_064` | Tuyển dụng – đăng tin & ứng viên | Lịch sử chăm sóc ứng viên | Should | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_065` | Tuyển dụng – đăng tin & ứng viên | Báo cáo hiệu quả kênh tuyển | Should | [x] | 75 | L2 · job posting + candidate pipeline |
| `UC_HRM_066` | Onboarding | Cấu hình thời hạn onboarding | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_067` | Onboarding | Cấu hình thời hạn thử việc | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_068` | Onboarding | Tạo hồ sơ nhân viên mới từ ứng viên | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_069` | Onboarding | Gán người hướng dẫn | Should | [x] | 80 | L2 · onboarding |
| `UC_HRM_070` | Onboarding | Checklist onboarding | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_071` | Onboarding | Upload chứng chỉ / giấy tờ | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_072` | Onboarding | Đánh giá kết thúc thử việc | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_073` | Onboarding | Chuyển thử việc thành chính thức | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_074` | Onboarding | Cảnh báo hết hạn thử việc | Must | [x] | 80 | L2 · onboarding |
| `UC_HRM_075` | Định biên | Khai báo định biên theo đơn vị | Must | [x] | 80 | L2 · định biên ĐV |
| `UC_HRM_076` | Định biên | Khai báo định biên theo ca | Must | [x] | 70 | L2 · định biên ca (ShiftCode) |
| `UC_HRM_077` | Định biên | Khai báo định biên theo bộ phận | Should | [x] | 80 | L2 · định biên bộ phận |
| `UC_HRM_078` | Định biên | So sánh thực tế vs định biên | Must | [x] | 80 | L2 · so sánh thực tế |
| `UC_HRM_079` | Định biên | Cảnh báo thiếu người | Must | [x] | 80 | L2 · cảnh báo thiếu |
| `UC_HRM_080` | Định biên | Duyệt thay đổi định biên | Should | [x] | 80 | L2 · duyệt thay đổi |
| `UC_HRM_081` | Ca làm việc | Tạo mẫu ca làm việc | Must | [x] | 80 | L2 · mẫu ca |
| `UC_HRM_082` | Ca làm việc | Xếp lịch ca nhân viên | Must | [x] | 80 | L2 · xếp ca NV |
| `UC_HRM_083` | Ca làm việc | Xếp lịch ca theo tuần / tháng | Must | [x] | 80 | L2 · xếp tuần/tháng |
| `UC_HRM_084` | Ca làm việc | Đổi ca giữa nhân viên | Should | [x] | 80 | L2 · đổi ca |
| `UC_HRM_085` | Ca làm việc | Hủy lịch ca | Must | [x] | 80 | L2 · hủy lịch ca |
| `UC_HRM_086` | Ca làm việc | Xem lịch ca theo đơn vị | Must | [x] | 80 | L2 · lịch theo ĐV |
| `UC_HRM_087` | Ca làm việc | Xem lịch ca cá nhân trên APP | Must | [x] | 70 | L2 · lịch cá nhân (web) |
| `UC_HRM_088` | Ca làm việc | Import lịch ca Excel | Could | [ ] | 0 |  |
| `UC_HRM_089` | Ca làm việc | Sao chép lịch ca | Should | [x] | 80 | L2 · sao chép lịch |
| `UC_HRM_090` | Ca làm việc | Khóa sổ lịch ca theo kỳ | Should | [x] | 80 | L2 · khóa kỳ |
| `UC_HRM_091` | Ca làm việc | In / xuất lịch ca | Should | [x] | 80 | L2 · xuất CSV |
| `UC_HRM_092` | Điều động nhân sự | Tạo lệnh điều động | Should | [x] | 80 | L2 · lệnh điều động |
| `UC_HRM_093` | Điều động nhân sự | Đề xuất nhu cầu điều động | Should | [x] | 80 | L2 · đề xuất điều động |
| `UC_HRM_094` | Điều động nhân sự | Nhận lệnh điều động trên APP | Should | [x] | 70 | L2 · nhận lệnh (web) |
| `UC_HRM_095` | Điều động nhân sự | Theo dõi nhân sự điều động | Should | [x] | 80 | L2 · theo dõi điều động |
| `UC_HRM_096` | Điều động nhân sự | Gắn nhãn công điều động khi chấm | Should | [x] | 70 | L2 · nhãn TRANSFER (chờ chấm công) |
| `UC_HRM_097` | Điều động nhân sự | Báo cáo giờ / chi phí điều động | Should | [x] | 80 | L2 · báo cáo giờ/CP |
| `UC_HRM_098` | Cấu hình chấm công | Cấu hình chấm vân tay / sinh trắc | Must | [x] | 70 | L2 · cấu hình vân tay |
| `UC_HRM_099` | Cấu hình chấm công | Cấu hình chấm APP điện thoại | Must | [x] | 80 | L2 · cấu hình APP |
| `UC_HRM_100` | Cấu hình chấm công | Cấu hình chấm QR / mã nhân sự | Should | [x] | 80 | L2 · cấu hình QR |
| `UC_HRM_101` | Cấu hình chấm công | Đăng ký thiết bị chấm | Must | [x] | 80 | L2 · đăng ký thiết bị |
| `UC_HRM_102` | Cấu hình chấm công | Cấu hình geo-fence điểm chấm | Should | [x] | 80 | L2 · geo-fence |
| `UC_HRM_103` | Cấu hình chấm công | Cấu hình quy tắc đi trễ | Must | [x] | 80 | L2 · quy tắc trễ |
| `UC_HRM_104` | Cấu hình chấm công | Cấu hình mức trừ công khi trễ | Must | [x] | 80 | L2 · trừ công trễ |
| `UC_HRM_105` | Cấu hình chấm công | Cấu hình quên check-out | Must | [x] | 80 | L2 · quên checkout |
| `UC_HRM_106` | Cấu hình chấm công | Cấu hình thời hạn xin điều chỉnh | Must | [x] | 80 | L2 · hạn điều chỉnh |
| `UC_HRM_107` | Cấu hình chấm công | Cấu hình làm thêm giờ (OT) | Should | [x] | 80 | L2 · cấu hình OT |
| `UC_HRM_108` | Cấu hình chấm công | Cấu hình ca đêm / ngày lễ | Should | [x] | 70 | L2 · ca đêm/lễ flag |
| `UC_HRM_109` | Thực hiện chấm công | Check-in đầu ca | Must | [x] | 80 | L2 · check-in |
| `UC_HRM_110` | Thực hiện chấm công | Check-out cuối ca | Must | [x] | 80 | L2 · check-out |
| `UC_HRM_111` | Thực hiện chấm công | Xem lịch sử chấm cá nhân | Must | [x] | 80 | L2 · lịch sử cá nhân |
| `UC_HRM_112` | Thực hiện chấm công | Bảng chấm công theo đơn vị | Must | [x] | 80 | L2 · bảng theo ĐV |
| `UC_HRM_113` | Thực hiện chấm công | Bảng chấm công toàn công ty | Must | [x] | 80 | L2 · bảng công ty |
| `UC_HRM_114` | Thực hiện chấm công | Cảnh báo thiếu chấm realtime | Should | [x] | 80 | L2 · cảnh báo thiếu |
| `UC_HRM_115` | Thực hiện chấm công | Tự tính phút đi trễ | Must | [x] | 80 | L2 · tính phút trễ |
| `UC_HRM_116` | Thực hiện chấm công | Tự trừ công do đi trễ | Must | [x] | 80 | L2 · trừ công trễ |
| `UC_HRM_117` | Thực hiện chấm công | Đánh dấu quên chấm | Must | [x] | 80 | L2 · đánh dấu thiếu |
| `UC_HRM_118` | Thực hiện chấm công | Đồng bộ dữ liệu từ máy chấm | Must | [x] | 70 | L2 · sync máy stub |
| `UC_HRM_119` | Thực hiện chấm công | Xử lý công OT tự động | Should | [x] | 80 | L2 · OT tự động |
| `UC_HRM_120` | Điều chỉnh & khóa công | Tạo phiếu xin điều chỉnh công | Must | [x] | 80 | L2 · phiếu điều chỉnh |
| `UC_HRM_121` | Điều chỉnh & khóa công | Đính kèm lý do / bằng chứng | Must | [x] | 70 | L2 · lý do/bằng chứng |
| `UC_HRM_122` | Điều chỉnh & khóa công | Duyệt / từ chối điều chỉnh | Must | [x] | 80 | L2 · duyệt điều chỉnh |
| `UC_HRM_123` | Điều chỉnh & khóa công | Ghi nhận vi phạm đi trễ | Should | [x] | 70 | L2 · ghi nhận trễ |
| `UC_HRM_124` | Điều chỉnh & khóa công | Lập bảng phạt | Should | [ ] | 0 |  |
| `UC_HRM_125` | Điều chỉnh & khóa công | Áp dụng phạt vào kỳ lương | Should | [ ] | 0 |  |
| `UC_HRM_126` | Điều chỉnh & khóa công | Khóa bảng công theo kỳ | Must | [x] | 80 | L2 · khóa bảng công |
| `UC_HRM_127` | Điều chỉnh & khóa công | Mở khóa bảng công có kiểm soát | Must | [x] | 80 | L2 · mở khóa kỳ |
| `UC_HRM_128` | Điều chỉnh & khóa công | Xác nhận bảng công | Should | [x] | 80 | L2 · xác nhận công |
| `UC_HRM_129` | Nghỉ phép & vắng mặt | Danh mục loại nghỉ | Must | [x] | 100 | M1 · leave types |
| `UC_HRM_130` | Nghỉ phép & vắng mặt | Cấu hình quỹ phép theo loại NS | Must | [x] | 80 | L2 · quỹ phép theo loại NS |
| `UC_HRM_131` | Nghỉ phép & vắng mặt | Cấp phát / điều chỉnh quỹ phép | Must | [x] | 80 | L2 · cấp phát/điều chỉnh quỹ |
| `UC_HRM_132` | Nghỉ phép & vắng mặt | Tạo đơn xin nghỉ | Must | [x] | 100 | M1 · leave request |
| `UC_HRM_133` | Nghỉ phép & vắng mặt | Duyệt đơn nghỉ đa cấp | Must | [x] | 70 | M1 · WF 1 cấp Approve/Reject |
| `UC_HRM_134` | Nghỉ phép & vắng mặt | Hủy đơn nghỉ | Should | [x] | 80 | L2 · hủy đơn nghỉ |
| `UC_HRM_135` | Nghỉ phép & vắng mặt | Xem quỹ phép còn lại | Must | [x] | 100 | M1 · leave balances |
| `UC_HRM_136` | Nghỉ phép & vắng mặt | Lịch nghỉ theo đơn vị | Should | [x] | 80 | L2 · lịch nghỉ theo ĐV |
| `UC_HRM_137` | Nghỉ phép & vắng mặt | Import nghỉ lễ / ngày nghỉ | Must | [x] | 80 | L2 · import ngày lễ |
| `UC_HRM_138` | Nghỉ phép & vắng mặt | Báo cáo nghỉ / quỹ phép | Should | [x] | 80 | L2 · báo cáo quỹ phép |
| `UC_HRM_139` | Kỷ luật & khen thưởng | Ghi nhận quyết định khen thưởng | Should | [x] | 85 | L2 · quyết định khen thưởng |
| `UC_HRM_140` | Kỷ luật & khen thưởng | Ghi nhận quyết định kỷ luật | Should | [x] | 85 | L2 · quyết định kỷ luật |
| `UC_HRM_141` | Kỷ luật & khen thưởng | Đính kèm quyết định | Should | [x] | 80 | L2 · đính kèm quyết định |
| `UC_HRM_142` | Kỷ luật & khen thưởng | Ảnh hưởng lương / phụ cấp | Should | [x] | 85 | L2 · áp dụng ảnh hưởng lương |
| `UC_HRM_143` | Kỷ luật & khen thưởng | Báo cáo khen thưởng – kỷ luật | Could | [x] | 75 | L2 · báo cáo KT–KL |
| `UC_HRM_144` | Offboarding / nghỉ việc | Tạo đơn nghỉ việc | Must | [x] | 90 | L2 · tạo đơn nghỉ việc |
| `UC_HRM_145` | Offboarding / nghỉ việc | Cấu hình / kiểm tra báo trước | Must | [x] | 90 | L2 · cấu hình/kiểm tra báo trước |
| `UC_HRM_146` | Offboarding / nghỉ việc | Duyệt đơn nghỉ việc | Must | [x] | 85 | L2 · duyệt đơn nghỉ việc |
| `UC_HRM_147` | Offboarding / nghỉ việc | Checklist bàn giao | Must | [x] | 90 | L2 · checklist bàn giao |
| `UC_HRM_148` | Offboarding / nghỉ việc | Thu hồi quyền hệ thống | Must | [x] | 85 | L2 · thu hồi quyền user |
| `UC_HRM_149` | Offboarding / nghỉ việc | Quyết toán phép / lương nghỉ việc | Must | [x] | 80 | L2 · quyết toán phép/lương NV |
| `UC_HRM_150` | Offboarding / nghỉ việc | Phỏng vấn nghỉ việc | Could | [x] | 70 | L2 · phỏng vấn nghỉ việc |
| `UC_HRM_151` | Offboarding / nghỉ việc | Báo cáo nghỉ việc / lý do | Should | [x] | 80 | L2 · báo cáo nghỉ việc/lý do |
| `UC_HRM_152` | Cấu hình lương & phụ cấp | Tạo thang bậc lương | Must | [x] | 90 | L2 · thang bậc lương |
| `UC_HRM_153` | Cấu hình lương & phụ cấp | Gán bậc lương theo nhân sự | Must | [x] | 90 | L2 · gán bậc theo NV |
| `UC_HRM_154` | Cấu hình lương & phụ cấp | Gán bậc theo trạng thái | Must | [x] | 80 | L2 · bậc theo trạng thái NV |
| `UC_HRM_155` | Cấu hình lương & phụ cấp | Đơn giá giờ / ngày nhân viên | Must | [x] | 80 | L2 · đơn giá giờ/ngày |
| `UC_HRM_156` | Cấu hình lương & phụ cấp | Quản lý lương thực tế chi trả | Must | [x] | 90 | L2 · lương thực tế chi trả |
| `UC_HRM_157` | Cấu hình lương & phụ cấp | Danh mục phụ cấp | Must | [x] | 90 | L2 · danh mục phụ cấp |
| `UC_HRM_158` | Cấu hình lương & phụ cấp | Rule phụ cấp theo ca | Must | [x] | 80 | L2 · rule phụ cấp theo ca |
| `UC_HRM_159` | Cấu hình lương & phụ cấp | Rule phụ cấp đặc thù | Should | [x] | 70 | L2 · rule phụ cấp đặc thù (shift null) |
| `UC_HRM_160` | Cấu hình lương & phụ cấp | Cấu hình bảo hiểm | Must | [x] | 90 | L2 · cấu hình BH NV |
| `UC_HRM_161` | Cấu hình lương & phụ cấp | Cấu hình thuế TNCN | Must | [x] | 80 | L2 · thuế TNCN flat Day-1 |
| `UC_HRM_162` | Cấu hình lương & phụ cấp | Cấu hình tạm ứng / khấu trừ | Must | [x] | 80 | L2 · tạm ứng/khấu trừ qua adjustment |
| `UC_HRM_163` | Tính lương & chi trả | Tạo kỳ lương | Must | [x] | 90 | L2 · tạo kỳ lương |
| `UC_HRM_164` | Tính lương & chi trả | Tổng hợp công vào kỳ lương | Must | [x] | 90 | L2 · tổng hợp công → kỳ |
| `UC_HRM_165` | Tính lương & chi trả | Tính lương tự động theo rule | Must | [x] | 85 | L2 · tính lương tự động |
| `UC_HRM_166` | Tính lương & chi trả | Nhập thưởng / phụ cấp phát sinh | Must | [x] | 90 | L2 · thưởng/PC phát sinh |
| `UC_HRM_167` | Tính lương & chi trả | Nhập khấu trừ / tạm ứng | Must | [x] | 90 | L2 · khấu trừ/tạm ứng |
| `UC_HRM_168` | Tính lương & chi trả | Xem / chỉnh bảng lương chi tiết | Must | [x] | 90 | L2 · xem/chỉnh bảng lương |
| `UC_HRM_169` | Tính lương & chi trả | Xác nhận bảng lương | Must | [x] | 90 | L2 · xác nhận bảng lương |
| `UC_HRM_170` | Tính lương & chi trả | Khóa kỳ lương | Must | [x] | 90 | L2 · khóa kỳ lương |
| `UC_HRM_171` | Tính lương & chi trả | Phiếu lương cá nhân (APP) | Must | [x] | 60 | L2 · phiếu lương web (APP sau) |
| `UC_HRM_172` | Tính lương & chi trả | Xuất bảng lương tổng hợp | Must | [x] | 90 | L2 · xuất CSV bảng lương |
| `UC_HRM_173` | Tính lương & chi trả | Xuất file chi lương ngân hàng | Must | [x] | 80 | L2 · xuất CSV chi ngân hàng |
| `UC_HRM_174` | Tính lương & chi trả | Đồng bộ bút toán lương sang FIN | Should | [ ] | 0 | skip · đồng bộ FIN sau |
| `UC_HRM_175` | Tính lương & chi trả | Báo cáo chi phí lương theo đơn vị | Must | [x] | 85 | L2 · CP lương theo ĐV |
| `UC_HRM_176` | Tính lương & chi trả | So sánh lương kỳ này / kỳ trước | Should | [x] | 75 | L2 · so sánh kỳ này/trước |
| `UC_HRM_177` | Đánh giá hiệu suất | Mẫu đánh giá KPI / năng lực | Could | [ ] | 0 |  |
| `UC_HRM_178` | Đánh giá hiệu suất | Tạo kỳ đánh giá | Could | [ ] | 0 |  |
| `UC_HRM_179` | Đánh giá hiệu suất | Quản lý đánh giá nhân viên | Could | [ ] | 0 |  |
| `UC_HRM_180` | Đánh giá hiệu suất | Nhân viên tự đánh giá | Won't | [ ] | 0 |  |
| `UC_HRM_181` | Đánh giá hiệu suất | Tổng hợp kết quả đánh giá | Could | [ ] | 0 |  |
| `UC_HRM_182` | Báo cáo & dashboard HRM | Dashboard headcount & biến động | Must | [x] | 90 | L2 · dashboard headcount & biến động |
| `UC_HRM_183` | Báo cáo & dashboard HRM | Báo cáo công / OT / đi trễ | Must | [x] | 90 | L2 · BC công/OT/đi trễ |
| `UC_HRM_184` | Báo cáo & dashboard HRM | Báo cáo tuyển dụng funnel | Should | [x] | 85 | L2 · funnel tuyển dụng |
| `UC_HRM_185` | Báo cáo & dashboard HRM | Báo cáo quỹ phép | Should | [x] | 85 | L2 · BC quỹ phép theo ĐV |
| `UC_HRM_186` | Báo cáo & dashboard HRM | Báo cáo chi phí nhân sự | Must | [x] | 90 | L2 · BC chi phí NS (payroll) |
| `UC_HRM_187` | Báo cáo & dashboard HRM | Báo cáo định biên vs thực tế | Should | [x] | 85 | L2 · định biên vs thực tế |

## LMS (43/74)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_LMS_001` | Danh mục nội dung đào tạo | Danh mục chương trình đào tạo | Must | [x] | 85 | L2 · LmsProgram API + FE courses |
| `UC_LMS_002` | Danh mục nội dung đào tạo | Danh mục khóa học | Must | [x] | 85 | L2 · LmsCourse domain + /app/lms/courses |
| `UC_LMS_003` | Danh mục nội dung đào tạo | Phân loại khóa (online/offline/blended) | Must | [x] | 85 | L2 · DeliveryMode Online/Offline/Blended |
| `UC_LMS_004` | Danh mục nội dung đào tạo | Quản lý chương / bài học | Must | [x] | 85 | L2 · Chapter/Lesson CRUD |
| `UC_LMS_005` | Danh mục nội dung đào tạo | Upload video bài giảng | Must | [x] | 80 | L2 · Video via ContentUrl (chưa upload binary) |
| `UC_LMS_006` | Danh mục nội dung đào tạo | Upload tài liệu PDF / slide | Must | [x] | 80 | L2 · Document/PDF via ContentUrl |
| `UC_LMS_007` | Danh mục nội dung đào tạo | Gắn tag kỹ năng / vị trí | Should | [ ] | 0 |  |
| `UC_LMS_008` | Danh mục nội dung đào tạo | Phiên bản nội dung khóa học | Should | [ ] | 0 |  |
| `UC_LMS_009` | Danh mục nội dung đào tạo | Ẩn / xuất bản khóa học | Must | [x] | 85 | L2 · Draft/Published/Hidden |
| `UC_LMS_010` | Ngân hàng câu hỏi & đề thi | Tạo ngân hàng câu hỏi | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_LMS_011` | Ngân hàng câu hỏi & đề thi | Phân loại câu hỏi theo độ khó | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_LMS_012` | Ngân hàng câu hỏi & đề thi | Tạo đề thi cố định | Must | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_LMS_013` | Ngân hàng câu hỏi & đề thi | Tạo đề thi random | Should | [ ] | 0 |  |
| `UC_LMS_014` | Ngân hàng câu hỏi & đề thi | Cấu hình điểm đạt / số lần thi | Must | [x] | 85 | L2 · PassScore + MaxAttempts |
| `UC_LMS_015` | Ngân hàng câu hỏi & đề thi | Thời gian làm bài & chống gian lận | Should | [ ] | 0 |  |
| `UC_LMS_016` | Lớp Offline | Mở lớp đào tạo offline | Must | [x] | 85 | L2 · lớp offline upsert/open |
| `UC_LMS_017` | Lớp Offline | Gán giảng viên / địa điểm / lịch | Must | [x] | 85 | L2 · sessions + GV/địa điểm |
| `UC_LMS_018` | Lớp Offline | Tuyển sinh / ghi danh học viên | Must | [x] | 85 | L2 · enrollment học viên |
| `UC_LMS_019` | Lớp Offline | Điểm danh buổi học | Must | [x] | 85 | L2 · điểm danh buổi học |
| `UC_LMS_020` | Lớp Offline | Ghi nhận học phí | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_LMS_021` | Lớp Offline | Đánh giá thực hành tại lớp | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_LMS_022` | Lớp Offline | Đóng lớp & tổng kết | Must | [x] | 85 | L2 · đóng lớp + tổng kết |
| `UC_LMS_023` | Mentoring | Gán mentor cho học viên | Must | [x] | 80 | L2 · gán mentor |
| `UC_LMS_024` | Mentoring | Checklist kèm cặp | Should | [ ] | 0 |  |
| `UC_LMS_025` | Mentoring | Mentor ghi nhận tiến độ | Should | [x] | 100 | Batch 1 · Verified HRM Attendance/Payroll & LMS Exam APIs and tests |
| `UC_LMS_026` | Mentoring | Đánh giá mentor / học viên | Could | [ ] | 0 |  |
| `UC_LMS_027` | Mentoring | Báo cáo hiệu quả mentoring | Should | [ ] | 0 |  |
| `UC_LMS_028` | Học Online – học viên | Đăng ký tài khoản học viên | Must | [x] | 90 | L2 · map SYS auth (AppUser) — không tách LMS identity |
| `UC_LMS_029` | Học Online – học viên | Đăng nhập / quên mật khẩu | Must | [x] | 90 | L2 · map SYS login/đổi MK hiện có |
| `UC_LMS_030` | Học Online – học viên | Danh sách & chi tiết khóa | Must | [x] | 85 | L2 · /app/lms/catalog Published |
| `UC_LMS_031` | Học Online – học viên | Mua khóa / thanh toán online | Must | [x] | 80 | L2 · enroll mock pay (chưa gateway) |
| `UC_LMS_032` | Học Online – học viên | Kích hoạt bằng mã voucher | Should | [x] | 75 | L2 · voucher FREE/DEMO100 |
| `UC_LMS_033` | Học Online – học viên | Tự mở khóa sau thanh toán | Must | [x] | 85 | L2 · tự Unlocked sau enroll |
| `UC_LMS_034` | Học Online – học viên | Xem video / tài liệu | Must | [x] | 85 | L2 · learn page Video/PDF/Text |
| `UC_LMS_035` | Học Online – học viên | Đánh dấu hoàn thành bài học | Must | [x] | 85 | L2 · complete lesson API |
| `UC_LMS_036` | Học Online – học viên | Tiếp tục học dở | Must | [x] | 85 | L2 · resume LastLessonId |
| `UC_LMS_037` | Học Online – học viên | Theo dõi % tiến độ khóa | Must | [x] | 85 | L2 · ProgressPct enrollment |
| `UC_LMS_038` | Học Online – học viên | Nhắc học tiếp | Should | [ ] | 0 |  |
| `UC_LMS_039` | Học Online – học viên | Diễn đàn / bình luận | Won't | [ ] | 0 |  |
| `UC_LMS_040` | Thi & chứng chỉ | Làm quiz cuối chương | Must | [x] | 85 | L2 · ChapterQuiz trên learn |
| `UC_LMS_041` | Thi & chứng chỉ | Thi cuối khóa | Must | [x] | 85 | L2 · Final exam attempt |
| `UC_LMS_042` | Thi & chứng chỉ | Chấm điểm tự động | Must | [x] | 85 | L2 · chấm tự động SingleChoice/TF |
| `UC_LMS_043` | Thi & chứng chỉ | Xem kết quả & đáp án | Should | [x] | 80 | L2 · xem kết quả + đáp án sau nộp |
| `UC_LMS_044` | Thi & chứng chỉ | Điều kiện cấp chứng chỉ | Must | [x] | 85 | L2 · điều kiện: xong bài + đậu Final |
| `UC_LMS_045` | Thi & chứng chỉ | Cấp chứng chỉ điện tử | Must | [x] | 85 | L2 · cấp CERT- + /certificates |
| `UC_LMS_046` | Thi & chứng chỉ | Mã xác thực chứng chỉ | Should | [ ] | 0 |  |
| `UC_LMS_047` | Thi & chứng chỉ | Thu hồi chứng chỉ | Could | [ ] | 0 |  |
| `UC_LMS_048` | Thi & chứng chỉ | Đồng bộ chứng chỉ sang HRM | Should | [ ] | 0 |  |
| `UC_LMS_049` | Giảng viên & quản trị LMS | Hồ sơ giảng viên | Must | [x] | 90 | L2 · hồ sơ giảng viên /instructors |
| `UC_LMS_050` | Giảng viên & quản trị LMS | Phân quyền giảng viên | Must | [x] | 90 | L2 · role LMS_INSTRUCTOR + grant |
| `UC_LMS_051` | Giảng viên & quản trị LMS | Theo dõi danh sách học viên | Must | [x] | 90 | L2 · roster learners reports |
| `UC_LMS_052` | Giảng viên & quản trị LMS | Phản hồi bài tập | Should | [ ] | 0 |  |
| `UC_LMS_053` | Giảng viên & quản trị LMS | Thống kê doanh thu theo khóa | Should | [ ] | 0 |  |
| `UC_LMS_054` | Giảng viên & quản trị LMS | Chống chia sẻ tài khoản | Should | [ ] | 0 |  |
| `UC_LMS_055` | Giảng viên & quản trị LMS | Chặn tải video | Could | [ ] | 0 |  |
| `UC_LMS_056` | Khảo sát & xác nhận | Tạo khảo sát hiểu bài | Should | [ ] | 0 |  |
| `UC_LMS_057` | Khảo sát & xác nhận | Khảo sát tuân thủ | Should | [ ] | 0 |  |
| `UC_LMS_058` | Khảo sát & xác nhận | Xác nhận đã đọc nội quy | Must | [x] | 80 | Batch1 Must · LmsAcknowledgement + compliance rate |
| `UC_LMS_059` | Khảo sát & xác nhận | Bắt buộc hoàn thành trước ca | Should | [ ] | 0 |  |
| `UC_LMS_060` | Khảo sát & xác nhận | Báo cáo tỷ lệ xác nhận | Should | [ ] | 0 |  |
| `UC_LMS_061` | Lộ trình đào tạo | Gán lộ trình theo chức danh | Should | [ ] | 0 |  |
| `UC_LMS_062` | Lộ trình đào tạo | Tự gán khóa bắt buộc khi nhận việc | Should | [ ] | 0 |  |
| `UC_LMS_063` | Lộ trình đào tạo | Theo dõi hoàn thành lộ trình | Should | [ ] | 0 |  |
| `UC_LMS_064` | Lộ trình đào tạo | Cảnh báo quá hạn đào tạo | Should | [ ] | 0 |  |
| `UC_LMS_065` | Báo cáo LMS | Dashboard tiến độ đào tạo | Must | [x] | 90 | L2 · dashboard tiến độ đào tạo |
| `UC_LMS_066` | Báo cáo LMS | Báo cáo hoàn thành theo đơn vị | Must | [x] | 90 | L2 · hoàn thành theo đơn vị |
| `UC_LMS_067` | Báo cáo LMS | Báo cáo điểm thi / tỷ lệ đạt | Should | [ ] | 0 |  |
| `UC_LMS_068` | Báo cáo LMS | Báo cáo học viên bỏ dở | Should | [ ] | 0 |  |
| `UC_LMS_069` | Báo cáo LMS | Báo cáo hiệu quả khóa | Could | [ ] | 0 |  |
| `UC_LMS_070` | Báo cáo LMS | Xuất báo cáo đào tạo | Must | [x] | 90 | L2 · xuất CSV báo cáo LMS |
| `UC_LMS_071` | AI hỗ trợ học tập | Gợi ý khóa học tiếp theo | Won't | [ ] | 0 |  |
| `UC_LMS_072` | AI hỗ trợ học tập | Tóm tắt bài học bằng AI | Won't | [ ] | 0 |  |
| `UC_LMS_073` | AI hỗ trợ học tập | AI tạo quiz từ nội dung | Won't | [ ] | 0 |  |
| `UC_LMS_074` | AI hỗ trợ học tập | Trợ lý hỏi đáp | Won't | [ ] | 0 |  |

## CRM (68/131)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_CRM_001` | Master khách hàng | Tạo khách hàng cá nhân | Must | [x] | 85 | L2 · Person customer + /app/crm/customers |
| `UC_CRM_002` | Master khách hàng | Tạo khách hàng doanh nghiệp | Must | [x] | 85 | L2 · Organization + CompanyName |
| `UC_CRM_003` | Master khách hàng | Cập nhật thông tin khách hàng | Must | [x] | 85 | L2 · upsert customer |
| `UC_CRM_004` | Master khách hàng | Kiểm tra trùng SĐT / MST | Must | [x] | 85 | L2 · chặn/gợi ý trùng SĐT·MST |
| `UC_CRM_005` | Master khách hàng | Gộp khách hàng trùng | Must | [x] | 85 | L2 · merge + Merged status |
| `UC_CRM_006` | Master khách hàng | Phân loại tệp khách hàng | Must | [x] | 85 | L2 · Segment Lead/Prospect/Customer/Partner |
| `UC_CRM_007` | Master khách hàng | Đánh giá tiềm năng | Should | [ ] | 0 |  |
| `UC_CRM_008` | Master khách hàng | Gán người phụ trách | Must | [x] | 85 | L2 · assign OwnerUserId |
| `UC_CRM_009` | Master khách hàng | Bàn giao khách hàng | Must | [x] | 85 | L2 · handover log |
| `UC_CRM_010` | Master khách hàng | Hồ sơ khách 360° | Must | [x] | 80 | L2 · 360 contacts+handover+dups (chưa lead/đơn) |
| `UC_CRM_011` | Master khách hàng | Danh sách người liên hệ | Must | [x] | 85 | L2 · CrmContact primary |
| `UC_CRM_012` | Master khách hàng | Lịch sử thay đổi dữ liệu | Should | [ ] | 0 |  |
| `UC_CRM_013` | Master khách hàng | Ngưng sử dụng / blacklist | Should | [ ] | 0 |  |
| `UC_CRM_014` | Master khách hàng | Import / export khách hàng | Must | [x] | 80 | L2 · CSV import/export |
| `UC_CRM_015` | Master khách hàng | Tìm kiếm khách đa tiêu chí | Must | [x] | 85 | L2 · search đa tiêu chí |
| `UC_CRM_016` | Marketing – chiến dịch | Tạo campaign marketing | Must | [x] | 90 | L2 · CrmCampaignService Upsert + FE /crm/campaigns · 12 BE + 10 FE tests |
| `UC_CRM_017` | Marketing – chiến dịch | Quản lý nhóm quảng cáo | Should | [ ] | 0 |  |
| `UC_CRM_018` | Marketing – chiến dịch | Gắn sản phẩm / đối tượng mục tiêu | Should | [ ] | 0 |  |
| `UC_CRM_019` | Marketing – chiến dịch | Ghi nhận chi phí quảng cáo | Must | [x] | 90 | L2 · Campaign expense + SpentAmount sync · FE tab Chi phí |
| `UC_CRM_020` | Marketing – chiến dịch | Gắn ngân sách & theo dõi | Should | [ ] | 0 |  |
| `UC_CRM_021` | Marketing – chiến dịch | Đánh giá hậu chiến dịch | Should | [ ] | 0 |  |
| `UC_CRM_022` | Marketing – chiến dịch | Nhân bản campaign | Could | [ ] | 0 |  |
| `UC_CRM_023` | Marketing – chiến dịch | Đóng campaign | Must | [x] | 90 | L2 · Close campaign snapshot metrics · FE đóng |
| `UC_CRM_024` | Marketing – nguồn & đo lường | Danh mục nguồn lead | Must | [x] | 85 | L2 · CrmLeadSource catalog |
| `UC_CRM_025` | Marketing – nguồn & đo lường | Đồng bộ lead mạng xã hội | Should | [ ] | 0 |  |
| `UC_CRM_026` | Marketing – nguồn & đo lường | Đồng bộ lead website / landing | Must | [x] | 90 | L2 · SyncWebLead → CrmLead + UTM · FE tab Web lead |
| `UC_CRM_027` | Marketing – nguồn & đo lường | Đồng bộ kênh khác | Should | [ ] | 0 |  |
| `UC_CRM_028` | Marketing – nguồn & đo lường | Attribution nguồn khách | Should | [ ] | 0 |  |
| `UC_CRM_029` | Marketing – nguồn & đo lường | Tính CPL / CAC / ROAS / ROI | Must | [x] | 90 | L2 · CPL/CAC/ROAS/ROI GetMetrics · FE dashboard |
| `UC_CRM_030` | Marketing – nguồn & đo lường | Funnel marketing đến doanh thu | Should | [ ] | 0 |  |
| `UC_CRM_031` | Marketing – nguồn & đo lường | Dashboard marketing | Must | [x] | 90 | L2 · Marketing dashboard aggregate · FE dashboard |
| `UC_CRM_032` | Khuyến mại & voucher | Tạo chương trình khuyến mại | Must | [x] | 90 | L2 · CrmPromotionService Upsert + FE /crm/promotions |
| `UC_CRM_033` | Khuyến mại & voucher | Cấu hình điều kiện khuyến mại | Must | [x] | 90 | L2 · PromotionCondition replace-on-upsert · FE điều kiện |
| `UC_CRM_034` | Khuyến mại & voucher | Sinh mã voucher | Must | [x] | 90 | L2 · GenerateVouchers batch unique codes · FE sinh mã |
| `UC_CRM_035` | Khuyến mại & voucher | Giới hạn lượt dùng voucher | Must | [x] | 90 | L2 · MaxUsageTotal/PerCustomer + voucher MaxUsage · redeem guard |
| `UC_CRM_036` | Khuyến mại & voucher | Đồng bộ khuyến mại sang POS | Should | [x] | 90 | L2 · SyncToPos PosPromotion/Voucher · FE nút sync · 8 BE + helpers FE |
| `UC_CRM_037` | Khuyến mại & voucher | Áp dụng khuyến mại trên báo giá | Must | [x] | 90 | L2 · ApplyOnQuote DiscountAmount/Total · FE áp báo giá |
| `UC_CRM_038` | Khuyến mại & voucher | Báo cáo sử dụng voucher | Should | [x] | 90 | L2 · GET voucher-usage-report · FE BC sử dụng · filter promo/ngày |
| `UC_CRM_039` | Omnichannel & chatbot | Hộp thư tập trung đa kênh | Should | [ ] | 0 |  |
| `UC_CRM_040` | Omnichannel & chatbot | Tiếp nhận hội thoại mới | Should | [ ] | 0 |  |
| `UC_CRM_041` | Omnichannel & chatbot | Phân phối hội thoại theo rule | Should | [ ] | 0 |  |
| `UC_CRM_042` | Omnichannel & chatbot | Chuyển hội thoại giữa agent | Should | [ ] | 0 |  |
| `UC_CRM_043` | Omnichannel & chatbot | SLA phản hồi & cảnh báo | Should | [ ] | 0 |  |
| `UC_CRM_044` | Omnichannel & chatbot | Chatbot kịch bản | Could | [ ] | 0 |  |
| `UC_CRM_045` | Omnichannel & chatbot | Chatbot thu thập lead | Should | [ ] | 0 |  |
| `UC_CRM_046` | Omnichannel & chatbot | Chuyển bot sang agent | Should | [ ] | 0 |  |
| `UC_CRM_047` | Omnichannel & chatbot | Lưu lịch sử chat | Must | [x] | 80 | Batch1 Must · CrmChatHistory omnichannel log |
| `UC_CRM_048` | Omnichannel & chatbot | Đánh giá CSAT | Could | [ ] | 0 |  |
| `UC_CRM_049` | Lead | Tạo lead thủ công | Must | [x] | 85 | L2 · tạo lead thủ công + FE |
| `UC_CRM_050` | Lead | Tiếp nhận lead tự động | Must | [x] | 85 | L2 · auto-intake website stub |
| `UC_CRM_051` | Lead | Phân bổ lead cho sales | Must | [x] | 85 | L2 · phân bổ OwnerUserId |
| `UC_CRM_052` | Lead | Lead scoring | Should | [ ] | 0 |  |
| `UC_CRM_053` | Lead | Cập nhật trạng thái pipeline | Must | [x] | 85 | L2 · pipeline New→Qualified |
| `UC_CRM_054` | Lead | Task follow-up lead | Must | [x] | 85 | L2 · CrmLeadTask follow-up |
| `UC_CRM_055` | Lead | Nhắc việc follow-up | Must | [x] | 85 | L2 · nhắc việc IsReminder + NextFollowUpAt |
| `UC_CRM_056` | Lead | Nhật ký chăm sóc lead | Must | [x] | 85 | L2 · CrmLeadActivity nhật ký |
| `UC_CRM_057` | Lead | Chuyển lead thành cơ hội | Must | [x] | 85 | L2 · convert lead → opportunity + KH |
| `UC_CRM_058` | Lead | Đánh dấu lead mất | Must | [x] | 85 | L2 · mark Lost + lý do |
| `UC_CRM_059` | Lead | Gộp lead trùng | Should | [ ] | 0 |  |
| `UC_CRM_060` | Lead | Import lead Excel | Must | [x] | 85 | L2 · import lead CSV |
| `UC_CRM_061` | Lead | Báo cáo chuyển đổi lead | Must | [x] | 85 | L2 · báo cáo chuyển đổi lead |
| `UC_CRM_062` | Cơ hội bán hàng | Tạo cơ hội từ lead/khách | Must | [x] | 85 | L2 · tạo cơ hội từ lead/KH |
| `UC_CRM_063` | Cơ hội bán hàng | Pipeline cơ hội theo giai đoạn | Must | [x] | 85 | L2 · pipeline stage cơ hội |
| `UC_CRM_064` | Cơ hội bán hàng | Dự báo doanh thu | Should | [ ] | 0 |  |
| `UC_CRM_065` | Cơ hội bán hàng | Gắn sản phẩm / giá trị ước tính | Must | [x] | 85 | L2 · dòng SP / estimated value |
| `UC_CRM_066` | Cơ hội bán hàng | Đối thủ / ghi chú đàm phán | Could | [ ] | 0 |  |
| `UC_CRM_067` | Cơ hội bán hàng | Chuyển cơ hội sang báo giá | Must | [x] | 90 | L2 · tạo báo giá từ cơ hội + copy dòng |
| `UC_CRM_068` | Cơ hội bán hàng | Đóng thắng / thua | Must | [x] | 85 | L2 · đóng Won/Lost |
| `UC_CRM_069` | Cơ hội bán hàng | Báo cáo win-rate | Should | [ ] | 0 |  |
| `UC_CRM_070` | Báo giá | Tạo báo giá từ cơ hội | Must | [x] | 90 | L2 · tạo báo giá từ cơ hội |
| `UC_CRM_071` | Báo giá | Thêm dòng sản phẩm / dịch vụ | Must | [x] | 85 | L2 · dòng SP/DV báo giá + FE |
| `UC_CRM_072` | Báo giá | Áp chính sách giá / bảng giá | Must | [x] | 85 | L2 · bảng giá CRM + áp giá |
| `UC_CRM_073` | Báo giá | Xin duyệt chiết khấu | Must | [x] | 85 | L2 · xin/duyệt chiết khấu |
| `UC_CRM_074` | Báo giá | Gửi báo giá PDF/email | Must | [x] | 90 | L2 · xuất báo giá text thật + gửi Email (queue notification + nhật ký) / Pdf download |
| `UC_CRM_075` | Báo giá | Phiên bản báo giá | Should | [ ] | 0 |  |
| `UC_CRM_076` | Báo giá | Hết hạn báo giá tự động | Should | [ ] | 0 |  |
| `UC_CRM_077` | Báo giá | Chuyển báo giá thành đơn | Must | [x] | 90 | L2 · chuyển báo giá → đơn |
| `UC_CRM_078` | Báo giá | In mẫu báo giá | Should | [ ] | 0 |  |
| `UC_CRM_079` | Sales Online / đơn hàng | Tạo đơn hàng từ báo giá | Must | [x] | 90 | L2 · tạo đơn từ báo giá |
| `UC_CRM_080` | Sales Online / đơn hàng | Tiếp nhận đơn từ kênh online | Should | [ ] | 0 |  |
| `UC_CRM_081` | Sales Online / đơn hàng | Cập nhật trạng thái đơn | Must | [x] | 85 | L2 · cập nhật trạng thái đơn |
| `UC_CRM_082` | Sales Online / đơn hàng | Giữ tồn khi duyệt đơn | Must | [x] | 90 | L2 · giữ tồn INV reservation thật (Active+ATP, release khi hủy/đẩy kho, idempotent) |
| `UC_CRM_083` | Sales Online / đơn hàng | Tách / gộp đơn | Could | [ ] | 0 |  |
| `UC_CRM_084` | Sales Online / đơn hàng | Hủy đơn có kiểm soát | Must | [x] | 85 | L2 · hủy đơn có lý do |
| `UC_CRM_085` | Sales Online / đơn hàng | Trả hàng / điều chỉnh đơn | Should | [ ] | 0 |  |
| `UC_CRM_086` | Sales Online / đơn hàng | Gắn hợp đồng | Should | [ ] | 0 |  |
| `UC_CRM_087` | Sales Online / đơn hàng | Theo dõi thanh toán | Must | [x] | 85 | L2 · theo dõi thanh toán đơn |
| `UC_CRM_088` | Sales Online / đơn hàng | Đẩy đơn sang kho / giao vận | Must | [x] | 90 | L2 · tạo lệnh giao LOG thật Draft→Confirmed từ đơn (idempotent, Failed khi lỗi) + nhả giữ tồn |
| `UC_CRM_089` | Sales Offline / Route sales | Phân vùng / tuyến bán hàng | Should | [ ] | 0 |  |
| `UC_CRM_090` | Sales Offline / Route sales | Phân loại tần suất visit | Should | [ ] | 0 |  |
| `UC_CRM_091` | Sales Offline / Route sales | Lập kế hoạch visit | Should | [ ] | 0 |  |
| `UC_CRM_092` | Sales Offline / Route sales | Check-in / check-out GPS | Should | [ ] | 0 |  |
| `UC_CRM_093` | Sales Offline / Route sales | Ghi nhận mục đích – kết quả visit | Should | [ ] | 0 |  |
| `UC_CRM_094` | Sales Offline / Route sales | Ghi nhận nhu cầu khách hàng | Should | [ ] | 0 |  |
| `UC_CRM_095` | Sales Offline / Route sales | Đặt hàng tại điểm thăm | Should | [ ] | 0 |  |
| `UC_CRM_096` | Sales Offline / Route sales | Xem lịch sử visit | Should | [ ] | 0 |  |
| `UC_CRM_097` | Sales Offline / Route sales | AI gợi ý việc ưu tiên | Won't | [ ] | 0 |  |
| `UC_CRM_098` | Sales Offline / Route sales | Dashboard doanh số field | Should | [ ] | 0 |  |
| `UC_CRM_099` | Sales Admin | Hàng đợi đơn cần xử lý | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_100` | Sales Admin | Kiểm tra tồn / xác nhận giữ hàng | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_101` | Sales Admin | Soạn lệnh xuất / giao | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_102` | Sales Admin | Đối soát chứng từ đơn | Should | [ ] | 0 |  |
| `UC_CRM_103` | Sales Admin | Xử lý khiếu nại đơn hàng | Should | [ ] | 0 |  |
| `UC_CRM_104` | Sales Admin | Theo dõi đơn chậm xử lý | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_105` | Sales Admin | Báo cáo năng suất Sales Admin | Could | [ ] | 0 |  |
| `UC_CRM_106` | Hợp đồng & chính sách bán | Quản lý hợp đồng bán | Should | [ ] | 0 |  |
| `UC_CRM_107` | Hợp đồng & chính sách bán | Đính kèm file hợp đồng | Should | [ ] | 0 |  |
| `UC_CRM_108` | Hợp đồng & chính sách bán | Theo dõi hiệu lực / tái tục | Should | [ ] | 0 |  |
| `UC_CRM_109` | Hợp đồng & chính sách bán | Chính sách giá theo nhóm KH | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_110` | Hợp đồng & chính sách bán | Chính sách công nợ / hạn mức | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_111` | Hợp đồng & chính sách bán | Chặn bán khi vượt công nợ | Should | [ ] | 0 |  |
| `UC_CRM_112` | Chăm sóc khách hàng | Tạo ticket hỗ trợ | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_113` | Chăm sóc khách hàng | Phân loại khiếu nại / yêu cầu | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_114` | Chăm sóc khách hàng | Chuyển ticket sang FSM | Should | [ ] | 0 |  |
| `UC_CRM_115` | Chăm sóc khách hàng | Lịch chăm sóc / nhắc tái mua | Should | [ ] | 0 |  |
| `UC_CRM_116` | Chăm sóc khách hàng | Chương trình loyalty | Could | [ ] | 0 |  |
| `UC_CRM_117` | Chăm sóc khách hàng | Tích điểm / đổi quà | Could | [ ] | 0 |  |
| `UC_CRM_118` | Chăm sóc khách hàng | Khảo sát hài lòng | Could | [ ] | 0 |  |
| `UC_CRM_119` | Chăm sóc khách hàng | Báo cáo retention / tái mua | Should | [ ] | 0 |  |
| `UC_CRM_120` | Hoa hồng & KPI sales | Cấu hình rule hoa hồng | Should | [ ] | 0 |  |
| `UC_CRM_121` | Hoa hồng & KPI sales | Tính hoa hồng theo kỳ | Should | [ ] | 0 |  |
| `UC_CRM_122` | Hoa hồng & KPI sales | Duyệt bảng hoa hồng | Should | [ ] | 0 |  |
| `UC_CRM_123` | Hoa hồng & KPI sales | Đồng bộ hoa hồng sang HRM/FIN | Should | [ ] | 0 |  |
| `UC_CRM_124` | Hoa hồng & KPI sales | KPI doanh số theo nhân viên | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_125` | Hoa hồng & KPI sales | Bảng xếp hạng sales | Could | [ ] | 0 |  |
| `UC_CRM_126` | Báo cáo CRM | Dashboard Ban lãnh đạo sales | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_127` | Báo cáo CRM | Báo cáo pipeline & forecast | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_128` | Báo cáo CRM | Báo cáo theo nguồn / campaign | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_129` | Báo cáo CRM | Báo cáo theo nhân viên / vùng | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_CRM_130` | Báo cáo CRM | Báo cáo công nợ bán | Should | [ ] | 0 |  |
| `UC_CRM_131` | Báo cáo CRM | Xuất báo cáo định kỳ | Should | [ ] | 0 |  |

## POS (45/72)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_POS_001` | Cấu hình điểm bán & thiết bị | Khai báo điểm bán POS | Must | [x] | 85 | L2 · PosStore + /app/pos/stores |
| `UC_POS_002` | Cấu hình điểm bán & thiết bị | Khai báo quầy / máy POS | Must | [x] | 85 | L2 · PosTerminal quầy/máy |
| `UC_POS_003` | Cấu hình điểm bán & thiết bị | Cấu hình máy in hóa đơn | Must | [x] | 85 | L2 · PosPrinter Receipt/Kitchen |
| `UC_POS_004` | Cấu hình điểm bán & thiết bị | Cấu hình máy in bếp/khu vực | Should | [ ] | 0 |  |
| `UC_POS_005` | Cấu hình điểm bán & thiết bị | Cấu hình ngăn kéo tiền | Should | [ ] | 0 |  |
| `UC_POS_006` | Cấu hình điểm bán & thiết bị | Cấu hình thiết bị quét mã | Should | [ ] | 0 |  |
| `UC_POS_007` | Cấu hình điểm bán & thiết bị | Phân quyền thu ngân trên POS | Must | [x] | 85 | L2 · Cashier/Supervisor assignment |
| `UC_POS_008` | Cấu hình điểm bán & thiết bị | Chế độ offline tạm | Won't | [ ] | 0 |  |
| `UC_POS_009` | Catalog bán hàng | Danh mục nhóm sản phẩm | Must | [x] | 85 | L2 · PosProductCategory |
| `UC_POS_010` | Catalog bán hàng | Danh mục sản phẩm bán | Must | [x] | 85 | L2 · PosProduct + /catalog |
| `UC_POS_011` | Catalog bán hàng | Thuộc tính sản phẩm | Should | [ ] | 0 |  |
| `UC_POS_012` | Catalog bán hàng | BOM / định mức nguyên liệu | Must | [x] | 85 | L2 · PosBomLine định mức |
| `UC_POS_013` | Catalog bán hàng | Ảnh sản phẩm / thứ tự hiển thị | Should | [ ] | 0 |  |
| `UC_POS_014` | Catalog bán hàng | Ngưng bán sản phẩm tạm thời | Must | [x] | 85 | L2 · Suspended/Active toggle |
| `UC_POS_015` | Catalog bán hàng | Đồng bộ catalog từ back-office | Must | [x] | 90 | L2 · sync catalog thật INV SKU→POS (tạo/đổi tên/suspend + đếm) · FE flash chi tiết |
| `UC_POS_016` | Bảng giá & thuế | Bảng giá theo điểm bán | Must | [x] | 85 | L2 · PriceList theo điểm bán |
| `UC_POS_017` | Bảng giá & thuế | Giá theo khung giờ | Should | [ ] | 0 |  |
| `UC_POS_018` | Bảng giá & thuế | Giá theo ngày trong tuần | Could | [ ] | 0 |  |
| `UC_POS_019` | Bảng giá & thuế | Cấu hình thuế GTGT | Must | [x] | 85 | L2 · PosTaxRate GTGT |
| `UC_POS_020` | Bảng giá & thuế | Làm tròn tiền | Should | [ ] | 0 |  |
| `UC_POS_021` | Khuyến mại tại quầy | Áp dụng chương trình khuyến mại | Must | [x] | 90 | L2 · PosPromotion + apply trên đơn Open |
| `UC_POS_022` | Khuyến mại tại quầy | Nhập mã voucher | Must | [x] | 90 | L2 · PosVoucher redeem + UsedCount khi Paid |
| `UC_POS_023` | Khuyến mại tại quầy | Khuyến mại theo combo | Should | [ ] | 0 |  |
| `UC_POS_024` | Khuyến mại tại quầy | Giảm giá tay có quyền | Must | [x] | 90 | L2 · giảm tay Pending→duyệt pos.promo.manage |
| `UC_POS_025` | Khuyến mại tại quầy | Báo cáo khuyến mại | Should | [ ] | 0 |  |
| `UC_POS_026` | Giao dịch bán hàng | Mở đơn / chọn khu vực | Must | [x] | 85 | L2 · mở đơn + khu vực |
| `UC_POS_027` | Giao dịch bán hàng | Thêm / sửa / xóa sản phẩm | Must | [x] | 85 | L2 · thêm/sửa/hủy dòng SP |
| `UC_POS_028` | Giao dịch bán hàng | Tách bill / gộp bill | Should | [ ] | 0 |  |
| `UC_POS_029` | Giao dịch bán hàng | Chuyển đơn giữa quầy | Should | [ ] | 0 |  |
| `UC_POS_030` | Giao dịch bán hàng | Ghi chú đơn hàng | Should | [ ] | 0 |  |
| `UC_POS_031` | Giao dịch bán hàng | Gửi lệnh khu vực chế biến | Should | [ ] | 0 |  |
| `UC_POS_032` | Giao dịch bán hàng | Tạm tính / giữ đơn | Must | [x] | 85 | L2 · giữ đơn / mở lại |
| `UC_POS_033` | Giao dịch bán hàng | Thanh toán tiền mặt | Must | [x] | 85 | L2 · thanh toán tiền mặt |
| `UC_POS_034` | Giao dịch bán hàng | Thanh toán chuyển khoản / QR | Must | [x] | 85 | L2 · thanh toán Transfer/QR |
| `UC_POS_035` | Giao dịch bán hàng | Thanh toán thẻ / ví điện tử | Must | [x] | 85 | L2 · thanh toán Card/Wallet |
| `UC_POS_036` | Giao dịch bán hàng | Thanh toán hỗn hợp | Should | [ ] | 0 |  |
| `UC_POS_037` | Giao dịch bán hàng | In hóa đơn | Must | [x] | 90 | L2 · hóa đơn bán lẻ text 42 cột thật (line/giảm giá/thuế/thanh toán) + tải file |
| `UC_POS_038` | Giao dịch bán hàng | Hủy sản phẩm | Must | [x] | 85 | L2 · hủy sản phẩm trên đơn |
| `UC_POS_039` | Giao dịch bán hàng | Hủy cả bill | Must | [x] | 85 | L2 · hủy cả bill |
| `UC_POS_040` | Giao dịch bán hàng | Trả hàng / hoàn tiền | Must | [x] | 85 | L2 · trả hàng / hoàn tiền |
| `UC_POS_041` | Giao dịch bán hàng | Gợi ý bán kèm | Won't | [ ] | 0 |  |
| `UC_POS_042` | Ca thu ngân & quỹ | Mở ca thu ngân | Must | [x] | 90 | L2 · mở ca thu ngân |
| `UC_POS_043` | Ca thu ngân & quỹ | Nhập tiền đầu ca | Must | [x] | 90 | L2 · tiền đầu ca |
| `UC_POS_044` | Ca thu ngân & quỹ | Nộp tiền / rút tiền ca | Should | [ ] | 0 |  |
| `UC_POS_045` | Ca thu ngân & quỹ | Xem doanh thu trong ca | Must | [x] | 85 | L2 · doanh thu trong ca |
| `UC_POS_046` | Ca thu ngân & quỹ | Đóng ca & đếm quỹ | Must | [x] | 90 | L2 · đóng ca & đếm quỹ |
| `UC_POS_047` | Ca thu ngân & quỹ | Đối soát lệch quỹ | Must | [x] | 85 | L2 · đối soát lệch quỹ |
| `UC_POS_048` | Ca thu ngân & quỹ | In báo cáo ca | Must | [x] | 90 | L2 · báo cáo ca text thật (DT + PT thanh toán + tiền mặt/chênh lệch) + tải file |
| `UC_POS_049` | Ca thu ngân & quỹ | Duyệt xác nhận ca | Should | [ ] | 0 |  |
| `UC_POS_050` | Khách hàng & loyalty | Gắn khách hàng vào đơn | Should | [ ] | 0 |  |
| `UC_POS_051` | Khách hàng & loyalty | Tích điểm loyalty | Could | [ ] | 0 |  |
| `UC_POS_052` | Khách hàng & loyalty | Đổi điểm / ưu đãi | Could | [ ] | 0 |  |
| `UC_POS_053` | Khách hàng & loyalty | Tra cứu lịch sử mua | Should | [ ] | 0 |  |
| `UC_POS_054` | Đồng bộ tồn & back-office | Trừ tồn theo BOM khi bán | Must | [x] | 90 | L2 · PaySale trừ tồn INV Issue theo BOM · Ref POS · 8 BE + 6 FE tests |
| `UC_POS_055` | Đồng bộ tồn & back-office | Cảnh báo hết / sắp hết | Must | [x] | 90 | L2 · GET /api/pos/stock-alerts BelowMin/NearReorder/OutOfStock · FE banner sell |
| `UC_POS_056` | Đồng bộ tồn & back-office | Tạo đề nghị nhập hàng | Should | [ ] | 0 |  |
| `UC_POS_057` | Đồng bộ tồn & back-office | Nhận hàng từ kho trung tâm | Should | [ ] | 0 |  |
| `UC_POS_058` | Đồng bộ tồn & back-office | Kiểm kê nhanh | Should | [ ] | 0 |  |
| `UC_POS_059` | Đồng bộ tồn & back-office | Đồng bộ doanh thu ca sang FIN | Must | [x] | 90 | L2 · CloseShift + SyncShiftRevenueToFin RecognizeFromPos · FE sync-fin · 7 BE + helpers FE |
| `UC_POS_060` | Đồng bộ tồn & back-office | Đồng bộ đơn sang CRM | Could | [ ] | 0 |  |
| `UC_POS_061` | Báo cáo POS | Doanh thu theo giờ / ngày / ca | Must | [x] | 90 | L2 · DT theo giờ/ngày/ca |
| `UC_POS_062` | Báo cáo POS | Doanh thu theo sản phẩm | Must | [x] | 90 | L2 · DT theo sản phẩm |
| `UC_POS_063` | Báo cáo POS | Doanh thu theo thu ngân | Must | [x] | 90 | L2 · DT theo thu ngân |
| `UC_POS_064` | Báo cáo POS | Tỷ lệ hủy / giảm giá | Must | [x] | 90 | L2 · tỷ lệ hủy / giảm giá |
| `UC_POS_065` | Báo cáo POS | Cost lý thuyết vs thực tế | Should | [x] | 90 | L2 · cost-variance BOM×StdCost vs INV Issue POS · FE tab + CSV |
| `UC_POS_066` | Báo cáo POS | Top sản phẩm bán chạy | Should | [x] | 90 | L2 · top-products qty/revenue top N · FE tab + CSV |
| `UC_POS_067` | Báo cáo POS | So sánh điểm bán | Should | [x] | 90 | L2 · store-compare DT/share/avg ticket · FE tab + CSV |
| `UC_POS_068` | Báo cáo POS | Xuất báo cáo POS | Must | [x] | 90 | L2 · xuất CSV báo cáo POS |
| `UC_POS_069` | Vận hành chuỗi | Giám sát doanh thu chuỗi realtime | Should | [x] | 90 | L2 · chain-live DT hôm nay/tháng vs target + ca mở · FE tab poll 30s |
| `UC_POS_070` | Vận hành chuỗi | Phân phối catalog / giá / khuyến mại | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_POS_071` | Vận hành chuỗi | Chuẩn hóa catalog toàn chuỗi | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_POS_072` | Vận hành chuỗi | Cấu hình target doanh thu | Should | [x] | 90 | L2 · MonthlyRevenueTarget trên store + migration · FE set target + attainment |

## PUR (27/52)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_PUR_001` | Danh mục nhà cung cấp | Tạo / cập nhật nhà cung cấp | Must | [x] | 85 | L2 · PurVendor + /app/pur/vendors |
| `UC_PUR_002` | Danh mục nhà cung cấp | Phân loại nhóm nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_003` | Danh mục nhà cung cấp | Người liên hệ & điều khoản | Must | [x] | 85 | L2 · contact + payment terms |
| `UC_PUR_004` | Danh mục nhà cung cấp | Lead time & MOQ | Should | [ ] | 0 |  |
| `UC_PUR_005` | Danh mục nhà cung cấp | Đánh giá chất lượng nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_006` | Danh mục nhà cung cấp | Blacklist / ngưng dùng | Should | [ ] | 0 |  |
| `UC_PUR_007` | Danh mục nhà cung cấp | Import danh sách nhà cung cấp | Could | [ ] | 0 |  |
| `UC_PUR_008` | Danh mục nhà cung cấp | Hồ sơ pháp lý | Should | [ ] | 0 |  |
| `UC_PUR_009` | Nguồn cung & giá mua | Gắn sản phẩm – nhà cung cấp | Must | [x] | 85 | L2 · VendorProduct gắn SP–NCC |
| `UC_PUR_010` | Nguồn cung & giá mua | Bảng giá mua theo nhà cung cấp | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_PUR_011` | Nguồn cung & giá mua | Hiệu lực bảng giá mua | Should | [ ] | 0 |  |
| `UC_PUR_012` | Nguồn cung & giá mua | Lịch sử giá mua | Should | [ ] | 0 |  |
| `UC_PUR_013` | Nguồn cung & giá mua | Cảnh báo tăng giá bất thường | Could | [ ] | 0 |  |
| `UC_PUR_014` | Yêu cầu mua hàng (PR) | Tạo PR từ đơn vị | Must | [x] | 85 | L2 · PR + lines /orders |
| `UC_PUR_015` | Yêu cầu mua hàng (PR) | Tạo PR từ cảnh báo tồn min | Should | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_PUR_016` | Yêu cầu mua hàng (PR) | Gộp nhiều nhu cầu thành PR | Should | [ ] | 0 |  |
| `UC_PUR_017` | Yêu cầu mua hàng (PR) | Luồng duyệt PR | Must | [x] | 85 | L2 · submit/approve PR |
| `UC_PUR_018` | Yêu cầu mua hàng (PR) | Từ chối / trả lại PR | Must | [x] | 85 | L2 · reject/return PR |
| `UC_PUR_019` | Yêu cầu mua hàng (PR) | Theo dõi trạng thái PR | Must | [x] | 85 | L2 · status Draft→… trên list |
| `UC_PUR_020` | Yêu cầu mua hàng (PR) | Hủy PR | Should | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_PUR_021` | Báo giá & chọn nhà cung cấp (RFQ) | Tạo RFQ gửi nhiều nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_022` | Báo giá & chọn nhà cung cấp (RFQ) | Nhập báo giá từ nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_023` | Báo giá & chọn nhà cung cấp (RFQ) | So sánh giá / điều kiện | Should | [ ] | 0 |  |
| `UC_PUR_024` | Báo giá & chọn nhà cung cấp (RFQ) | Chọn nhà cung cấp thắng | Should | [ ] | 0 |  |
| `UC_PUR_025` | Báo giá & chọn nhà cung cấp (RFQ) | Chuyển RFQ thành PO | Should | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_PUR_026` | Đơn mua hàng (PO) | Tạo PO từ PR/RFQ | Must | [x] | 85 | L2 · PO từ PR / thủ công |
| `UC_PUR_027` | Đơn mua hàng (PO) | Duyệt PO theo hạn mức | Must | [x] | 80 | L2 · hạn mức 10tr tự duyệt |
| `UC_PUR_028` | Đơn mua hàng (PO) | Gửi PO cho nhà cung cấp | Must | [x] | 85 | L2 · Send PO → Sent |
| `UC_PUR_029` | Đơn mua hàng (PO) | Xác nhận PO từ nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_030` | Đơn mua hàng (PO) | Sửa PO phiên bản | Must | [x] | 85 | L2 · revise PO phiên bản |
| `UC_PUR_031` | Đơn mua hàng (PO) | Theo dõi nhận hàng từng phần | Must | [x] | 90 | L2 · ReceivedQty / % nhận từng phần |
| `UC_PUR_032` | Đơn mua hàng (PO) | Đóng / hủy PO | Must | [x] | 90 | L2 · đóng / hủy PO |
| `UC_PUR_033` | Đơn mua hàng (PO) | In / xuất PO | Must | [x] | 90 | L2 · xuất PO CSV thật (header NCC + dòng + tổng) + đóng dấu in · FE tải file |
| `UC_PUR_034` | Nhận hàng & trả nhà cung cấp | Tạo phiếu nhận hàng theo PO | Must | [x] | 90 | L2 · GRN từ PO + FE |
| `UC_PUR_035` | Nhận hàng & trả nhà cung cấp | Nhận hàng lệch số lượng / chất lượng | Must | [x] | 85 | L2 · lệch SL / từ chối CL |
| `UC_PUR_036` | Nhận hàng & trả nhà cung cấp | Từ chối lô hàng không đạt | Should | [ ] | 0 |  |
| `UC_PUR_037` | Nhận hàng & trả nhà cung cấp | Đẩy nhập kho sang INV | Must | [x] | 90 | L2 · GRN→INV Receipt Purchase thật (idempotent) · Failed + lý do lỗi trong note · retry FE |
| `UC_PUR_038` | Nhận hàng & trả nhà cung cấp | Trả hàng nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_039` | Nhận hàng & trả nhà cung cấp | Biên bản giao nhận | Should | [ ] | 0 |  |
| `UC_PUR_040` | Hóa đơn mua & đối soát | Nhập hóa đơn nhà cung cấp | Must | [x] | 90 | L2 · hóa đơn NCC + FE |
| `UC_PUR_041` | Hóa đơn mua & đối soát | Đối soát 3 chiều PO–GRN–Invoice | Must | [x] | 85 | L2 · đối soát 3 chiều PO–GRN–HĐ |
| `UC_PUR_042` | Hóa đơn mua & đối soát | Xử lý chênh lệch | Should | [ ] | 0 |  |
| `UC_PUR_043` | Hóa đơn mua & đối soát | Đẩy công nợ sang FIN AP | Must | [x] | 90 | L2 · tạo + ghi sổ FinApInvoice thật từ HĐ matched (idempotent, Failed khi lỗi) |
| `UC_PUR_044` | Hóa đơn mua & đối soát | Tạm ứng nhà cung cấp | Should | [ ] | 0 |  |
| `UC_PUR_045` | Hợp đồng mua & khung giá | Hợp đồng mua khung | Should | [ ] | 0 |  |
| `UC_PUR_046` | Hợp đồng mua & khung giá | Theo dõi sản lượng / giá trị còn lại | Should | [ ] | 0 |  |
| `UC_PUR_047` | Hợp đồng mua & khung giá | Cảnh báo hết hạn hợp đồng | Should | [ ] | 0 |  |
| `UC_PUR_048` | Báo cáo mua hàng | Báo cáo mua theo nhà cung cấp / SP | Must | [x] | 90 | L2 · mua theo NCC/SP |
| `UC_PUR_049` | Báo cáo mua hàng | Báo cáo đúng hạn giao hàng | Should | [ ] | 0 |  |
| `UC_PUR_050` | Báo cáo mua hàng | Báo cáo tiết kiệm từ RFQ | Could | [ ] | 0 |  |
| `UC_PUR_051` | Báo cáo mua hàng | Open PR / Open PO aging | Must | [x] | 90 | L2 · Open PR/PO aging |
| `UC_PUR_052` | Báo cáo mua hàng | Xuất báo cáo mua hàng | Must | [x] | 90 | L2 · xuất CSV báo cáo mua |

## INV (51/70)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_INV_001` | Danh mục sản phẩm | Tạo / sửa SKU sản phẩm | Must | [x] | 85 | L2 · InvSku API + FE /app/inv/items |
| `UC_INV_002` | Danh mục sản phẩm | Phân nhóm hàng / ngành hàng | Must | [x] | 85 | L2 · InvItemGroup |
| `UC_INV_003` | Danh mục sản phẩm | Đơn vị tính & quy đổi | Must | [x] | 85 | L2 · UoM + conversion |
| `UC_INV_004` | Danh mục sản phẩm | Thuộc tính hàng (lô, serial, HSD) | Must | [x] | 80 | L2 · TrackLot/Serial/Expiry flags |
| `UC_INV_005` | Danh mục sản phẩm | Giá vốn / phương pháp tính giá | Must | [x] | 80 | L2 · Costing Average/Fifo + StandardCost |
| `UC_INV_006` | Danh mục sản phẩm | Ảnh & mô tả sản phẩm | Could | [ ] | 0 |  |
| `UC_INV_007` | Danh mục sản phẩm | Ngưng sử dụng SKU | Must | [x] | 85 | L2 · Active/Inactive SKU |
| `UC_INV_008` | Danh mục sản phẩm | Import / export danh mục SP | Must | [x] | 85 | L2 · CSV import/export SKU |
| `UC_INV_009` | Danh mục sản phẩm | Barcode / QR theo sản phẩm | Should | [ ] | 0 |  |
| `UC_INV_010` | Danh mục sản phẩm | Định mức tồn min/max/reorder | Must | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_INV_011` | Cấu hình kho & vị trí | Tạo kho | Must | [x] | 85 | L2 · InvWarehouse + FE warehouses |
| `UC_INV_012` | Cấu hình kho & vị trí | Loại kho | Must | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_INV_013` | Cấu hình kho & vị trí | Vị trí / kệ / bin | Should | [ ] | 0 |  |
| `UC_INV_014` | Cấu hình kho & vị trí | Gán thủ kho / quyền | Must | [x] | 85 | L2 · WarehouseKeeper assign |
| `UC_INV_015` | Cấu hình kho & vị trí | Cấu hình FEFO / FIFO | Must | [x] | 85 | L2 · PickPolicy Fifo/Fefo trên kho |
| `UC_INV_016` | Cấu hình kho & vị trí | Cho phép tồn âm hay không | Must | [x] | 85 | L2 · AllowNegativeStock |
| `UC_INV_017` | Nhập kho | Nhập từ mua hàng | Must | [x] | 90 | L2 · nhập từ GRN PUR thật |
| `UC_INV_018` | Nhập kho | Nhập từ sản xuất | Must | [x] | 80 | Batch1 Must · Receipt from production/MFG WorkOrder |
| `UC_INV_019` | Nhập kho | Nhập điều chỉnh / kiểm kê | Must | [x] | 85 | L2 · nhập điều chỉnh / KK |
| `UC_INV_020` | Nhập kho | Nhập chuyển đến | Must | [x] | 80 | Batch1 Must · Transfer receipt + partial tracking |
| `UC_INV_021` | Nhập kho | Nhập trả từ khách | Should | [ ] | 0 |  |
| `UC_INV_022` | Nhập kho | Nhập theo lô / HSD / serial | Must | [x] | 80 | L2 · lô/HSD khi nhập (TrackLot) |
| `UC_INV_023` | Nhập kho | In tem lô / serial | Should | [ ] | 0 |  |
| `UC_INV_024` | Xuất kho | Xuất bán / giao hàng | Must | [x] | 80 | Batch1 Must · Sales issue + COGS calculation |
| `UC_INV_025` | Xuất kho | Xuất sản xuất | Must | [x] | 80 | Batch1 Must · Production issue BOM deduction |
| `UC_INV_026` | Xuất kho | Xuất nội bộ / tiêu hao | Must | [x] | 85 | L2 · xuất nội bộ |
| `UC_INV_027` | Xuất kho | Xuất cho dịch vụ kỹ thuật | Should | [ ] | 0 |  |
| `UC_INV_028` | Xuất kho | Xuất cho dự án | Should | [ ] | 0 |  |
| `UC_INV_029` | Xuất kho | Xuất theo FEFO tự động | Must | [x] | 90 | L2 · xuất FEFO/FIFO auto khi Post Issue |
| `UC_INV_030` | Xuất kho | Xuất điều chỉnh | Must | [x] | 85 | L2 · xuất điều chỉnh |
| `UC_INV_031` | Chuyển kho | Tạo phiếu chuyển kho | Must | [x] | 90 | L2 · tạo phiếu chuyển kho |
| `UC_INV_032` | Chuyển kho | Duyệt chuyển kho | Should | [ ] | 0 |  |
| `UC_INV_033` | Chuyển kho | Xuất bên gửi / nhập bên nhận | Must | [x] | 90 | L2 · ship/receive 2 bước |
| `UC_INV_034` | Chuyển kho | Chuyển kho một bước | Should | [ ] | 0 |  |
| `UC_INV_035` | Chuyển kho | Theo dõi hàng đang chuyển | Must | [x] | 85 | L2 · InTransit qty |
| `UC_INV_036` | Chuyển kho | Chuyển từ kho trung tâm | Must | [x] | 80 | Batch1 Must · Central warehouse transfer + InTransit |
| `UC_INV_037` | Giữ hàng & tồn khả dụng | Giữ hàng theo đơn đã duyệt | Must | [x] | 90 | L2 · giữ hàng InvStockReservation Active |
| `UC_INV_038` | Giữ hàng & tồn khả dụng | Giải phóng giữ hàng | Must | [x] | 90 | L2 · giải phóng giữ hàng Release |
| `UC_INV_039` | Giữ hàng & tồn khả dụng | Xem tồn thực tế | Must | [x] | 90 | L2 · xem tồn thực tế |
| `UC_INV_040` | Giữ hàng & tồn khả dụng | Xem tồn khả dụng | Must | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_INV_041` | Giữ hàng & tồn khả dụng | Xem tồn đang giữ / đang chuyển | Must | [x] | 80 | L2 · tồn đang chuyển |
| `UC_INV_042` | Giữ hàng & tồn khả dụng | Cảnh báo không đủ tồn | Must | [x] | 90 | L2 · cảnh báo/chặn không đủ ATP |
| `UC_INV_043` | Lô – HSD – Serial | Theo dõi tồn theo lô | Must | [x] | 90 | L2 · tồn theo lô + HSD trên /stock |
| `UC_INV_044` | Lô – HSD – Serial | Cảnh báo cận date / quá date | Must | [x] | 90 | L2 · cảnh báo cận/quá HSD reports |
| `UC_INV_045` | Lô – HSD – Serial | Chặn xuất hàng quá HSD | Must | [x] | 90 | L2 · chặn xuất lô quá HSD khi Post |
| `UC_INV_046` | Lô – HSD – Serial | Theo dõi serial | Should | [ ] | 0 |  |
| `UC_INV_047` | Lô – HSD – Serial | Truy vết lô xuôi/ngược | Should | [ ] | 0 |  |
| `UC_INV_048` | Lô – HSD – Serial | Báo cáo hàng sắp hết hạn | Must | [x] | 90 | L2 · BC sắp hết hạn near-expiry CSV |
| `UC_INV_049` | Kiểm kê | Tạo phiếu kiểm kê | Must | [x] | 90 | L2 · tạo phiếu kiểm kê |
| `UC_INV_050` | Kiểm kê | Nhập số đếm thực tế | Must | [x] | 85 | L2 · nhập số đếm |
| `UC_INV_051` | Kiểm kê | Kiểm kê theo vị trí / nhóm | Should | [ ] | 0 |  |
| `UC_INV_052` | Kiểm kê | Đối chiếu lệch kiểm kê | Must | [x] | 85 | L2 · đối chiếu lệch |
| `UC_INV_053` | Kiểm kê | Duyệt điều chỉnh sau kiểm kê | Must | [x] | 85 | L2 · duyệt/post điều chỉnh KK |
| `UC_INV_054` | Kiểm kê | Khóa giao dịch khi đang kiểm kê | Should | [ ] | 0 |  |
| `UC_INV_055` | Kiểm kê | Báo cáo kết quả kiểm kê | Must | [x] | 90 | L2 · báo cáo kết quả kiểm kê |
| `UC_INV_056` | Yêu cầu xuất / đề nghị hàng | Đề nghị xuất nội bộ | Should | [ ] | 0 |  |
| `UC_INV_057` | Yêu cầu xuất / đề nghị hàng | Đề nghị cấp hàng | Should | [ ] | 0 |  |
| `UC_INV_058` | Yêu cầu xuất / đề nghị hàng | Duyệt đề nghị | Should | [ ] | 0 |  |
| `UC_INV_059` | Yêu cầu xuất / đề nghị hàng | Chuyển đề nghị thành phiếu xuất | Should | [ ] | 0 |  |
| `UC_INV_060` | Giá trị kho & kế toán kho | Xem giá trị tồn | Must | [x] | 90 | L2 · xem giá trị tồn |
| `UC_INV_061` | Giá trị kho & kế toán kho | Tính lại giá vốn | Must | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_INV_062` | Giá trị kho & kế toán kho | Đẩy bút toán kho sang FIN | Must | [x] | 80 | Batch1 Must · Post INV journal to FIN (632/156/331) |
| `UC_INV_063` | Giá trị kho & kế toán kho | Báo cáo giá trị tồn | Must | [x] | 90 | L2 · báo cáo giá trị tồn |
| `UC_INV_064` | Báo cáo kho | Xuất nhập tồn theo kỳ | Must | [x] | 90 | L2 · XNT theo kỳ |
| `UC_INV_065` | Báo cáo kho | Thẻ kho / lịch sử sản phẩm | Must | [x] | 90 | L2 · thẻ kho / lịch sử SP |
| `UC_INV_066` | Báo cáo kho | Hàng chậm luân chuyển | Should | [ ] | 0 |  |
| `UC_INV_067` | Báo cáo kho | Hàng dưới min / trên max | Must | [x] | 90 | L2 · dưới min / trên max |
| `UC_INV_068` | Báo cáo kho | Báo cáo xuất theo mục đích | Should | [ ] | 0 |  |
| `UC_INV_069` | Báo cáo kho | Dashboard tồn & cảnh báo | Must | [x] | 90 | L2 · dashboard tồn & cảnh báo |
| `UC_INV_070` | Báo cáo kho | Xuất báo cáo kho Excel | Must | [x] | 90 | L2 · xuất CSV báo cáo kho |

## LOG (26/39)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_LOG_001` | Cấu hình giao vận | Danh mục đơn vị vận chuyển | Must | [x] | 85 | L2 · LogCarrier API + FE /app/log/carriers |
| `UC_LOG_002` | Cấu hình giao vận | Danh mục tài xế / xe | Should | [ ] | 0 |  |
| `UC_LOG_003` | Cấu hình giao vận | Bảng giá cước vận chuyển | Should | [ ] | 0 |  |
| `UC_LOG_004` | Cấu hình giao vận | Cấu hình khu vực giao | Should | [ ] | 0 |  |
| `UC_LOG_005` | Cấu hình giao vận | Cấu hình ca giao hàng | Could | [ ] | 0 |  |
| `UC_LOG_006` | Lệnh giao hàng | Tạo lệnh giao từ đơn hàng | Must | [x] | 85 | L2 · DeliveryOrder từ SO + lines |
| `UC_LOG_007` | Lệnh giao hàng | Gộp nhiều đơn thành chuyến | Should | [ ] | 0 |  |
| `UC_LOG_008` | Lệnh giao hàng | Tách lệnh giao nhiều đợt | Must | [x] | 85 | L2 · tách đợt ParentOrderId |
| `UC_LOG_009` | Lệnh giao hàng | Pick list / soạn hàng | Must | [x] | 85 | L2 · Pick list StartPick/ConfirmPick |
| `UC_LOG_010` | Lệnh giao hàng | Xác nhận xuất hàng giao | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_LOG_011` | Lệnh giao hàng | In vận đơn / phiếu giao | Must | [x] | 85 | L2 · WaybillNo + PrintedAt |
| `UC_LOG_012` | Lệnh giao hàng | Hủy / hoàn lệnh giao | Must | [x] | 85 | L2 · Cancel/Return lệnh giao |
| `UC_LOG_013` | Điều phối & theo dõi | Phân công tài xế / đơn vị vận chuyển | Must | [x] | 85 | L2 · Assign carrier/driver |
| `UC_LOG_014` | Điều phối & theo dõi | Cập nhật trạng thái vận đơn | Must | [x] | 85 | L2 · UpdateStatus + ShipmentEvent |
| `UC_LOG_015` | Điều phối & theo dõi | Tracking mã vận đơn | Could | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_LOG_016` | Điều phối & theo dõi | Chứng từ ký nhận (POD) | Should | [ ] | 0 |  |
| `UC_LOG_017` | Điều phối & theo dõi | Ghi nhận giao thất bại | Must | [x] | 85 | L2 · Fail + FailureReason |
| `UC_LOG_018` | Điều phối & theo dõi | Hẹn giao lại | Should | [ ] | 0 |  |
| `UC_LOG_019` | Điều phối & theo dõi | Theo dõi realtime trên bản đồ | Won't | [ ] | 0 |  |
| `UC_LOG_020` | COD | Đánh dấu đơn thu COD | Must | [x] | 100 | Batch 2 · Verified CRM Sales Admin, POS BOM/Shift & PUR/LOG PO/COD APIs and tests |
| `UC_LOG_021` | COD | Ghi nhận số tiền COD | Must | [x] | 90 | L2 · SetCodAmount |
| `UC_LOG_022` | COD | Xác nhận đã thu COD | Must | [x] | 90 | L2 · ConfirmCollected |
| `UC_LOG_023` | COD | Bàn giao tiền COD | Must | [x] | 90 | L2 · CodHandover Draft→Submitted |
| `UC_LOG_024` | COD | Đối soát 3 chiều COD | Must | [x] | 90 | L2 · đối soát expected/collected/remitted |
| `UC_LOG_025` | COD | Cảnh báo COD quá hạn | Must | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_LOG_026` | COD | Xử lý lệch COD | Should | [x] | 85 | L2 · ResolveVariance |
| `UC_LOG_027` | Hoàn hàng & giao lại | Tạo phiếu hoàn về kho | Must | [x] | 90 | L2 · LogReturnNote từ lệnh giao |
| `UC_LOG_028` | Hoàn hàng & giao lại | Kiểm đếm hàng hoàn | Must | [x] | 90 | L2 · CountLine + ConfirmCount |
| `UC_LOG_029` | Hoàn hàng & giao lại | Nhập kho hàng hoàn | Must | [x] | 90 | L2 · PostReceiptFromLogReturn INV Source=Return |
| `UC_LOG_030` | Hoàn hàng & giao lại | Chi phí phát sinh hoàn | Could | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_LOG_031` | Giao nội bộ | Lệnh giao nội bộ | Should | [ ] | 0 |  |
| `UC_LOG_032` | Giao nội bộ | Xác nhận nhận hàng | Should | [ ] | 0 |  |
| `UC_LOG_033` | Giao nội bộ | Đối soát giao nội bộ | Should | [ ] | 0 |  |
| `UC_LOG_034` | Báo cáo giao vận | Tỷ lệ giao đúng hạn | Must | [x] | 90 | L2 · PromisedAt + % giao đúng hạn ops |
| `UC_LOG_035` | Báo cáo giao vận | Tỷ lệ hoàn / thất bại | Must | [x] | 85 | L2 · /reports/ops tỷ lệ hoàn/thất bại |
| `UC_LOG_036` | Báo cáo giao vận | Năng suất tài xế / chuyến | Should | [ ] | 0 |  |
| `UC_LOG_037` | Báo cáo giao vận | Chi phí vận chuyển | Should | [ ] | 0 |  |
| `UC_LOG_038` | Báo cáo giao vận | Báo cáo COD tồn / đã nộp | Must | [x] | 90 | L2 · báo cáo COD tồn/đã nộp /cod/report |
| `UC_LOG_039` | Báo cáo giao vận | Dashboard giao vận | Must | [x] | 85 | L2 · dashboard vận hành cards trên /returns |

## MFG (25/46)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_MFG_001` | Danh mục sản xuất | Danh mục thành phẩm / bán thành phẩm | Must | [x] | 85 | L2 · MfgItem FG/SFG + FE catalog |
| `UC_MFG_002` | Danh mục sản xuất | Danh mục nguyên vật liệu | Must | [x] | 85 | L2 · MfgItem RM |
| `UC_MFG_003` | Danh mục sản xuất | Danh mục xưởng / dây chuyền | Must | [x] | 85 | L2 · MfgWorkshop Workshop/Line |
| `UC_MFG_004` | Danh mục sản xuất | Danh mục công đoạn | Should | [ ] | 0 |  |
| `UC_MFG_005` | Danh mục sản xuất | Ca sản xuất / năng lực | Should | [ ] | 0 |  |
| `UC_MFG_006` | BOM & định mức | Tạo BOM nhiều cấp | Must | [x] | 85 | L2 · BOM multi-level lines |
| `UC_MFG_007` | BOM & định mức | Phiên bản BOM | Must | [x] | 85 | L2 · BOM Version + Activate |
| `UC_MFG_008` | BOM & định mức | Định mức nguyên vật liệu | Must | [x] | 85 | L2 · BomLine định mức |
| `UC_MFG_009` | BOM & định mức | Định mức hao hụt | Should | [ ] | 0 |  |
| `UC_MFG_010` | BOM & định mức | So sánh phiên bản BOM | Could | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_MFG_011` | BOM & định mức | Sao chép BOM | Should | [ ] | 0 |  |
| `UC_MFG_012` | Kế hoạch sản xuất | Kế hoạch SX theo nhu cầu | Should | [ ] | 0 |  |
| `UC_MFG_013` | Kế hoạch sản xuất | Kế hoạch SX theo đơn hàng | Must | [x] | 85 | L2 · MfgPlan theo SO |
| `UC_MFG_014` | Kế hoạch sản xuất | Tính nhu cầu nguyên vật liệu | Should | [ ] | 0 |  |
| `UC_MFG_015` | Kế hoạch sản xuất | Đề xuất mua nguyên vật liệu thiếu | Should | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_MFG_016` | Kế hoạch sản xuất | Lịch SX theo xưởng/ca | Should | [ ] | 0 |  |
| `UC_MFG_017` | Lệnh sản xuất | Tạo lệnh sản xuất | Must | [x] | 85 | L2 · WorkOrder create |
| `UC_MFG_018` | Lệnh sản xuất | Duyệt lệnh sản xuất | Must | [x] | 85 | L2 · Approve WO |
| `UC_MFG_019` | Lệnh sản xuất | Phát hành lệnh / in phiếu | Must | [x] | 85 | L2 · Release + PrintedAt |
| `UC_MFG_020` | Lệnh sản xuất | Xuất nguyên vật liệu cho lệnh | Must | [x] | 100 | Batch 3 · Verified INV FEFO/Stocktake, LOG Route & MFG BOM/WO APIs and tests |
| `UC_MFG_021` | Lệnh sản xuất | Ghi nhận tiến độ công đoạn | Should | [ ] | 0 |  |
| `UC_MFG_022` | Lệnh sản xuất | Ghi nhận thành phẩm nhập kho | Must | [x] | 85 | L2 · FgReceipt nhập TP |
| `UC_MFG_023` | Lệnh sản xuất | Ghi nhận phế phẩm / hao hụt | Must | [x] | 90 | L2 · MfgScrap Scrap/Loss + QtyScrap |
| `UC_MFG_024` | Lệnh sản xuất | Tạm dừng / hủy lệnh | Should | [x] | 85 | L2 · Pause/Resume/Cancel lệnh SX |
| `UC_MFG_025` | Lệnh sản xuất | Đóng lệnh sản xuất | Must | [x] | 90 | L2 · Close WO khóa vận hành |
| `UC_MFG_026` | Lệnh sản xuất | Lệnh sản xuất lại | Could | [ ] | 0 |  |
| `UC_MFG_027` | Giá thành sản xuất | Tập hợp chi phí nguyên vật liệu | Must | [x] | 90 | L2 · tập hợp chi phí NVL CostSheet lines |
| `UC_MFG_028` | Giá thành sản xuất | Phân bổ nhân công / chi phí chung | Should | [ ] | 0 |  |
| `UC_MFG_029` | Giá thành sản xuất | Giá thành đơn vị thành phẩm | Must | [x] | 90 | L2 · đơn giá TP = MaterialCost/GoodQty |
| `UC_MFG_030` | Giá thành sản xuất | Đối chiếu lý thuyết vs thực tế | Should | [ ] | 0 |  |
| `UC_MFG_031` | Giá thành sản xuất | Đẩy giá thành sang INV/FIN | Must | [x] | 90 | L2 · đẩy StandardCost INV + JE WIP→TP thật (auto-resolve TK 154*/155* + kỳ Open, Post Source=Auto) |
| `UC_MFG_032` | Chất lượng (QC) | Tiêu chí QC đầu vào | Should | [ ] | 0 |  |
| `UC_MFG_033` | Chất lượng (QC) | QC thành phẩm | Should | [ ] | 0 |  |
| `UC_MFG_034` | Chất lượng (QC) | Ghi nhận lô đạt / không đạt | Should | [ ] | 0 |  |
| `UC_MFG_035` | Chất lượng (QC) | Cách ly hàng lỗi | Should | [ ] | 0 |  |
| `UC_MFG_036` | Chất lượng (QC) | Báo cáo tỷ lệ đạt QC | Should | [ ] | 0 |  |
| `UC_MFG_037` | Sản xuất theo lô/mẻ | Lô/mẻ sản xuất | Should | [ ] | 0 |  |
| `UC_MFG_038` | Sản xuất theo lô/mẻ | Ghi nhận thông số mẻ | Should | [ ] | 0 |  |
| `UC_MFG_039` | Sản xuất theo lô/mẻ | Đóng gói & gắn tem | Should | [ ] | 0 |  |
| `UC_MFG_040` | Sản xuất theo lô/mẻ | Định mức phối trộn | Could | [ ] | 0 |  |
| `UC_MFG_041` | Báo cáo sản xuất | Tiến độ lệnh sản xuất | Must | [x] | 90 | L2 · tiến độ lệnh SX /reports/wo-progress |
| `UC_MFG_042` | Báo cáo sản xuất | Sản lượng theo ngày/ca/xưởng | Must | [x] | 90 | L2 · sản lượng theo ngày/xưởng (ca stub) |
| `UC_MFG_043` | Báo cáo sản xuất | Tiêu hao nguyên vật liệu variance | Must | [x] | 90 | L2 · variance NVL BOM vs xuất |
| `UC_MFG_044` | Báo cáo sản xuất | Hiệu suất / OEE | Could | [ ] | 0 |  |
| `UC_MFG_045` | Báo cáo sản xuất | Dashboard sản xuất | Must | [x] | 90 | L2 · dashboard sản xuất |
| `UC_MFG_046` | Báo cáo sản xuất | Xuất báo cáo sản xuất | Must | [x] | 90 | L2 · xuất CSV báo cáo SX |

## FSM (28/50)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_FSM_001` | Danh mục kỹ thuật | Danh mục loại dịch vụ | Must | [x] | 85 | L2 · FsmServiceType + FE catalog |
| `UC_FSM_002` | Danh mục kỹ thuật | Danh mục mã lỗi | Must | [x] | 85 | L2 · FsmFaultCode |
| `UC_FSM_003` | Danh mục kỹ thuật | Danh mục linh kiện | Must | [x] | 85 | L2 · FsmPart |
| `UC_FSM_004` | Danh mục kỹ thuật | Bảng giá dịch vụ | Should | [ ] | 0 |  |
| `UC_FSM_005` | Danh mục kỹ thuật | Cấu hình SLA | Must | [x] | 85 | L2 · FsmSlaPolicy theo priority |
| `UC_FSM_006` | Danh mục kỹ thuật | Kỹ năng / chứng chỉ kỹ thuật viên | Should | [ ] | 0 |  |
| `UC_FSM_007` | Danh mục kỹ thuật | Vùng phụ trách | Should | [ ] | 0 |  |
| `UC_FSM_008` | Install base – thiết bị tại khách | Hồ sơ thiết bị đã bán | Must | [x] | 85 | L2 · FsmAsset install base |
| `UC_FSM_009` | Install base – thiết bị tại khách | Serial / model / ngày kích hoạt BH | Must | [x] | 85 | L2 · Serial/Model/ActivatedAt/WarrantyEnd |
| `UC_FSM_010` | Install base – thiết bị tại khách | Lịch sử bảo hành / sửa chữa | Must | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_FSM_011` | Install base – thiết bị tại khách | Cảnh báo hết hạn bảo hành | Should | [ ] | 0 |  |
| `UC_FSM_012` | Install base – thiết bị tại khách | Hợp đồng bảo trì định kỳ | Should | [ ] | 0 |  |
| `UC_FSM_013` | Tiếp nhận & phân công ticket | Tạo ticket từ kênh | Must | [x] | 85 | L2 · Ticket từ Channel |
| `UC_FSM_014` | Tiếp nhận & phân công ticket | Phân loại mức ưu tiên | Must | [x] | 85 | L2 · Priority + SLA auto |
| `UC_FSM_015` | Tiếp nhận & phân công ticket | Phân công kỹ thuật viên thủ công | Must | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_FSM_016` | Tiếp nhận & phân công ticket | Phân công theo rule | Should | [ ] | 0 |  |
| `UC_FSM_017` | Tiếp nhận & phân công ticket | Đổi kỹ thuật viên / escalate | Must | [x] | 85 | L2 · Escalate / đổi KTV |
| `UC_FSM_018` | Tiếp nhận & phân công ticket | Lịch hẹn với khách | Must | [x] | 90 | L2 · lịch hẹn AppointmentAt |
| `UC_FSM_019` | Tiếp nhận & phân công ticket | Xác nhận lịch trên APP | Must | [x] | 80 | Batch1 Must · Technician confirm/decline schedule |
| `UC_FSM_020` | Thực hiện hiện trường | Check-in hiện trường GPS | Should | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_FSM_021` | Thực hiện hiện trường | Checklist công việc | Should | [ ] | 0 |  |
| `UC_FSM_022` | Thực hiện hiện trường | Ghi nhận nguyên nhân & xử lý | Must | [x] | 90 | L2 · nguyên nhân & xử lý work-log |
| `UC_FSM_023` | Thực hiện hiện trường | Chụp ảnh trước/sau | Should | [ ] | 0 |  |
| `UC_FSM_024` | Thực hiện hiện trường | Xuất linh kiện theo ticket | Must | [x] | 90 | L2 · xuất LK theo ticket FsmTicketPartLine |
| `UC_FSM_025` | Thực hiện hiện trường | Hoàn linh kiện thừa | Should | [ ] | 0 |  |
| `UC_FSM_026` | Thực hiện hiện trường | Ghi nhận phí sửa chữa | Should | [ ] | 0 |  |
| `UC_FSM_027` | Thực hiện hiện trường | Check-out / hoàn thành | Must | [x] | 90 | L2 · check-out → Resolved |
| `UC_FSM_028` | Nghiệm thu & đóng ticket | Khách ký nghiệm thu | Must | [x] | 85 | L2 · nghiệm thu web (signer name) |
| `UC_FSM_029` | Nghiệm thu & đóng ticket | Đánh giá dịch vụ | Could | [ ] | 0 |  |
| `UC_FSM_030` | Nghiệm thu & đóng ticket | Đóng ticket đạt/trễ SLA | Must | [x] | 90 | L2 · đóng ticket + SLA met/trễ |
| `UC_FSM_031` | Nghiệm thu & đóng ticket | Tái mở ticket | Should | [ ] | 0 |  |
| `UC_FSM_032` | Nghiệm thu & đóng ticket | Chuyển chi phí sang FIN | Should | [ ] | 0 |  |
| `UC_FSM_033` | Bảo trì định kỳ | Lịch bảo trì theo thiết bị | Should | [ ] | 0 |  |
| `UC_FSM_034` | Bảo trì định kỳ | Tự tạo ticket bảo trì đến hạn | Should | [ ] | 0 |  |
| `UC_FSM_035` | Bảo trì định kỳ | Checklist bảo trì chuẩn | Should | [ ] | 0 |  |
| `UC_FSM_036` | Bảo trì định kỳ | Báo cáo thực hiện bảo trì | Should | [ ] | 0 |  |
| `UC_FSM_037` | Kho linh kiện kỹ thuật | Tồn linh kiện tại kho KT | Must | [x] | 90 | L2 · tồn kho KT FsmPartStock Warehouse |
| `UC_FSM_038` | Kho linh kiện kỹ thuật | Cấp linh kiện cho KTV | Must | [x] | 90 | L2 · cấp LK kho → KTV FsmPartIssueDoc |
| `UC_FSM_039` | Kho linh kiện kỹ thuật | Đối soát linh kiện | Must | [x] | 90 | L2 · đối soát LK FsmPartReconcileDoc |
| `UC_FSM_040` | Kho linh kiện kỹ thuật | Cảnh báo thất thoát | Should | [ ] | 0 |  |
| `UC_FSM_041` | APP kỹ thuật viên | Danh sách việc hôm nay | Must | [x] | 80 | Batch1 Must · Today task list filter + sort |
| `UC_FSM_042` | APP kỹ thuật viên | Điều hướng / thông tin khách | Must | [x] | 80 | Batch1 Must · Navigation + customer info on APP |
| `UC_FSM_043` | APP kỹ thuật viên | Làm việc offline | Won't | [ ] | 0 |  |
| `UC_FSM_044` | APP kỹ thuật viên | Nộp quyết toán ngày | Should | [ ] | 0 |  |
| `UC_FSM_045` | Báo cáo FSM | SLA compliance realtime | Must | [x] | 90 | L2 · SLA compliance /reports |
| `UC_FSM_046` | Báo cáo FSM | Năng suất kỹ thuật viên | Must | [x] | 90 | L2 · năng suất KTV |
| `UC_FSM_047` | Báo cáo FSM | Chi phí linh kiện | Must | [x] | 90 | L2 · BC chi phí LK /reports/parts |
| `UC_FSM_048` | Báo cáo FSM | Tỷ lệ sửa lần đầu | Should | [ ] | 0 |  |
| `UC_FSM_049` | Báo cáo FSM | Báo cáo bảo hành | Should | [ ] | 0 |  |
| `UC_FSM_050` | Báo cáo FSM | Xuất báo cáo kỹ thuật | Must | [x] | 90 | L2 · xuất CSV báo cáo FSM |

## PJM (29/42)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_PJM_001` | Danh mục dự án | Loại dự án | Must | [x] | 85 | L2 · PjmProjectType + FE catalog |
| `UC_PJM_002` | Danh mục dự án | Mẫu hạng mục / WBS | Must | [x] | 85 | L2 · WbsTemplate + items |
| `UC_PJM_003` | Danh mục dự án | Mẫu checklist nghiệm thu | Should | [ ] | 0 |  |
| `UC_PJM_004` | Danh mục dự án | Trạng thái dự án chuẩn | Must | [x] | 85 | L2 · PjmProjectStatus chuẩn |
| `UC_PJM_005` | Khởi tạo dự án | Tạo dự án từ cơ hội CRM | Must | [x] | 85 | L2 · tạo DA từ SourceOpportunityCode |
| `UC_PJM_006` | Khởi tạo dự án | Tạo dự án thủ công | Must | [x] | 85 | L2 · tạo DA thủ công |
| `UC_PJM_007` | Khởi tạo dự án | Gắn khách hàng / hợp đồng | Must | [x] | 85 | L2 · CustomerName + ContractCode |
| `UC_PJM_008` | Khởi tạo dự án | Gán quản lý dự án / thành viên | Must | [x] | 85 | L2 · PM + ProjectMember |
| `UC_PJM_009` | Khởi tạo dự án | Ngân sách dự kiến & timeline | Must | [x] | 85 | L2 · Budget + Start/EndDate |
| `UC_PJM_010` | Khởi tạo dự án | Phê duyệt khởi động | Should | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_PJM_011` | Kế hoạch & tiến độ | Tạo hạng mục WBS | Must | [x] | 85 | L2 · PjmWbsItem |
| `UC_PJM_012` | Kế hoạch & tiến độ | Gán người thực hiện | Must | [x] | 85 | L2 · gán Assignee WBS |
| `UC_PJM_013` | Kế hoạch & tiến độ | Cập nhật % hoàn thành | Must | [x] | 90 | L2 · % HT trên WBS PercentComplete |
| `UC_PJM_014` | Kế hoạch & tiến độ | Milestone & deadline | Must | [x] | 90 | L2 · Milestone + DueDate WBS |
| `UC_PJM_015` | Kế hoạch & tiến độ | Phụ thuộc giữa hạng mục | Could | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_PJM_016` | Kế hoạch & tiến độ | Gantt / timeline dự án | Should | [ ] | 0 |  |
| `UC_PJM_017` | Kế hoạch & tiến độ | Cảnh báo trễ tiến độ | Must | [x] | 90 | L2 · cảnh báo trễ DueDate / health Late |
| `UC_PJM_018` | Kế hoạch & tiến độ | Nhật ký thay đổi kế hoạch | Should | [ ] | 0 |  |
| `UC_PJM_019` | Nguồn lực & chi phí dự án | Phân công nhân sự | Must | [x] | 90 | L2 · phân công NS AllocationPct/From-To |
| `UC_PJM_020` | Nguồn lực & chi phí dự án | Timesheet theo dự án | Should | [ ] | 0 |  |
| `UC_PJM_021` | Nguồn lực & chi phí dự án | Xuất nguyên vật liệu cho dự án | Must | [x] | 85 | L2 · xuất NVL soft PjmMaterialIssue |
| `UC_PJM_022` | Nguồn lực & chi phí dự án | Ghi nhận chi phí phát sinh | Must | [x] | 90 | L2 · ghi chi phí PjmExpense Posted |
| `UC_PJM_023` | Nguồn lực & chi phí dự án | Theo dõi ngân sách vs thực tế | Must | [x] | 90 | L2 · NS vs thực tế costSummary + BC |
| `UC_PJM_024` | Nguồn lực & chi phí dự án | Cảnh báo vượt ngân sách | Should | [ ] | 0 |  |
| `UC_PJM_025` | Thực hiện dự án | Checklist khảo sát | Should | [ ] | 0 |  |
| `UC_PJM_026` | Thực hiện dự án | Checklist lắp đặt | Should | [ ] | 0 |  |
| `UC_PJM_027` | Thực hiện dự án | Checklist bàn giao | Should | [ ] | 0 |  |
| `UC_PJM_028` | Thực hiện dự án | Ghi nhận ảnh / biên bản | Should | [ ] | 0 |  |
| `UC_PJM_029` | Thực hiện dự án | Phát sinh change request | Should | [ ] | 0 |  |
| `UC_PJM_030` | Thực hiện dự án | Duyệt change request | Should | [ ] | 0 |  |
| `UC_PJM_031` | Nghiệm thu & đóng dự án | Biên bản nghiệm thu giai đoạn | Must | [x] | 90 | L2 · BBNT giai đoạn Phase |
| `UC_PJM_032` | Nghiệm thu & đóng dự án | Nghiệm thu cuối & bàn giao | Must | [x] | 90 | L2 · NT cuối Final + bàn giao |
| `UC_PJM_033` | Nghiệm thu & đóng dự án | Khách ký xác nhận | Must | [x] | 90 | L2 · khách ký SignerName |
| `UC_PJM_034` | Nghiệm thu & đóng dự án | Ghi nhận doanh thu dự án | Must | [x] | 85 | L2 · ghi DT soft RecognizedRevenue |
| `UC_PJM_035` | Nghiệm thu & đóng dự án | Quyết toán chi phí & P&L | Must | [x] | 90 | L2 · quyết toán P&L margin |
| `UC_PJM_036` | Nghiệm thu & đóng dự án | Đóng dự án / lưu trữ | Must | [x] | 90 | L2 · đóng DA → Completed |
| `UC_PJM_037` | Nghiệm thu & đóng dự án | Bảo hành sau dự án | Should | [ ] | 0 |  |
| `UC_PJM_038` | Báo cáo dự án | Portfolio dự án đang chạy | Must | [x] | 90 | L2 · portfolio /reports |
| `UC_PJM_039` | Báo cáo dự án | Tiến độ & sức khỏe dự án | Must | [x] | 90 | L2 · tiến độ & sức khỏe dự án |
| `UC_PJM_040` | Báo cáo dự án | Lợi nhuận theo dự án | Must | [x] | 90 | L2 · BC lợi nhuận /reports/profit |
| `UC_PJM_041` | Báo cáo dự án | Năng suất nguồn lực | Should | [ ] | 0 |  |
| `UC_PJM_042` | Báo cáo dự án | Xuất báo cáo dự án | Must | [x] | 90 | L2 · xuất CSV báo cáo PJM |

## FIN (53/83)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_FIN_001` | Danh mục kế toán | Hệ thống tài khoản (COA) | Must | [x] | 85 | L2 · FinAccount COA + FE catalog |
| `UC_FIN_002` | Danh mục kế toán | Nhóm tài khoản / chỉ tiêu | Must | [x] | 85 | L2 · FinAccountGroup + FE catalog |
| `UC_FIN_003` | Danh mục kế toán | Kỳ kế toán / năm tài chính | Must | [x] | 85 | L2 · FinFiscalYear/Period + generate months |
| `UC_FIN_004` | Danh mục kế toán | Khóa sổ kỳ / mở lại | Must | [x] | 85 | L2 · khóa/mở kỳ + chặn ghi BT |
| `UC_FIN_005` | Danh mục kế toán | Đồng tiền hạch toán & tỷ giá | Should | [ ] | 0 |  |
| `UC_FIN_006` | Danh mục kế toán | Trung tâm chi phí | Must | [x] | 85 | L2 · FinCostCenter + FE |
| `UC_FIN_007` | Danh mục kế toán | Khoản mục thu/chi | Should | [ ] | 0 |  |
| `UC_FIN_008` | Danh mục kế toán | Hình thức thanh toán | Must | [x] | 85 | L2 · FinPaymentMethod + FE |
| `UC_FIN_009` | Danh mục kế toán | Danh mục thuế | Must | [x] | 85 | L2 · FinTax + FE |
| `UC_FIN_010` | Sổ cái & bút toán | Tạo bút toán thủ công | Must | [x] | 85 | L2 · FinJournal thủ công Draft/Post |
| `UC_FIN_011` | Sổ cái & bút toán | Bút toán định kỳ / mẫu | Should | [ ] | 0 |  |
| `UC_FIN_012` | Sổ cái & bút toán | Đảo bút toán | Must | [x] | 85 | L2 · đảo BT (reversal JE) |
| `UC_FIN_013` | Sổ cái & bút toán | Xem sổ cái theo tài khoản | Must | [x] | 85 | L2 · sổ cái theo TK |
| `UC_FIN_014` | Sổ cái & bút toán | Sổ chi tiết theo đối tượng | Must | [x] | 85 | L2 · sổ CT theo ĐT/TTCP |
| `UC_FIN_015` | Sổ cái & bút toán | Nhận bút toán tự động | Must | [x] | 90 | L2 · BT tự động thật CreateAutoJournal (Source=Auto) + filter list + API /journals/auto · FE lọc nguồn |
| `UC_FIN_016` | Sổ cái & bút toán | Kiểm soát bút toán lệch Nợ/Có | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_017` | Sổ cái & bút toán | Đính kèm chứng từ gốc | Should | [ ] | 0 |  |
| `UC_FIN_018` | Quỹ tiền mặt | Danh mục quỹ / thủ quỹ | Must | [x] | 90 | L2 · FinCashFund + FE /fin/cash |
| `UC_FIN_019` | Quỹ tiền mặt | Phiếu thu tiền mặt | Must | [x] | 90 | L2 · phiếu thu Receipt Draft→Post→Void + JE stub |
| `UC_FIN_020` | Quỹ tiền mặt | Phiếu chi tiền mặt | Must | [x] | 90 | L2 · phiếu chi Payment kiểm số dư sổ |
| `UC_FIN_021` | Quỹ tiền mặt | Đề nghị tạm ứng / hoàn ứng | Should | [ ] | 0 |  |
| `UC_FIN_022` | Quỹ tiền mặt | Kiểm kê quỹ | Should | [ ] | 0 |  |
| `UC_FIN_023` | Quỹ tiền mặt | Báo cáo sổ quỹ | Must | [x] | 90 | L2 · sổ quỹ opening/in/out/closing |
| `UC_FIN_024` | Ngân hàng | Danh mục tài khoản ngân hàng | Must | [x] | 90 | L2 · FinBankAccount + FE /fin/bank |
| `UC_FIN_025` | Ngân hàng | Giấy báo Nợ / Có | Must | [x] | 90 | L2 · giấy báo Credit/Debit Draft→Post→Void + JE stub |
| `UC_FIN_026` | Ngân hàng | Đối soát sao kê ngân hàng | Must | [x] | 85 | L2 · sao kê tay + khớp/bỏ khớp giấy báo Posted |
| `UC_FIN_027` | Ngân hàng | Đề nghị chuyển khoản | Must | [x] | 90 | L2 · đề nghị CK Draft→Submit→Approve→Execute |
| `UC_FIN_028` | Ngân hàng | Import sao kê | Should | [ ] | 0 |  |
| `UC_FIN_029` | Ngân hàng | Theo dõi số dư ngân hàng | Must | [x] | 90 | L2 · sổ số dư NH bank-book |
| `UC_FIN_030` | Công nợ phải thu (AR) | Tạo hóa đơn phải thu | Must | [x] | 90 | L2 · FinArInvoice Draft→Open + JE stub |
| `UC_FIN_031` | Công nợ phải thu (AR) | Công nợ theo khách / hóa đơn | Must | [x] | 90 | L2 · công nợ theo KH customer-balances |
| `UC_FIN_032` | Công nợ phải thu (AR) | Thu tiền & phân bổ | Must | [x] | 90 | L2 · thu tiền + phân bổ · đẩy Cash/Bank |
| `UC_FIN_033` | Công nợ phải thu (AR) | Bù trừ công nợ | Should | [ ] | 0 |  |
| `UC_FIN_034` | Công nợ phải thu (AR) | Nhắc nợ tự động | Should | [ ] | 0 |  |
| `UC_FIN_035` | Công nợ phải thu (AR) | Cảnh báo vượt hạn mức | Must | [x] | 90 | L2 · hạn mức tín dụng + cảnh báo Warning/Exceeded |
| `UC_FIN_036` | Công nợ phải thu (AR) | Bảng tuổi nợ phải thu | Must | [x] | 90 | L2 · bảng tuổi nợ AR aging |
| `UC_FIN_037` | Công nợ phải thu (AR) | Xử lý nợ khó đòi | Could | [ ] | 0 |  |
| `UC_FIN_038` | Công nợ phải thu (AR) | Đối soát COD về AR | Should | [ ] | 0 |  |
| `UC_FIN_039` | Công nợ phải trả (AP) | Tạo hóa đơn phải trả | Must | [x] | 90 | L2 · FinApInvoice Draft→Open + JE stub |
| `UC_FIN_040` | Công nợ phải trả (AP) | Công nợ theo nhà cung cấp | Must | [x] | 90 | L2 · công nợ theo NCC vendor-balances |
| `UC_FIN_041` | Công nợ phải trả (AP) | Đề nghị thanh toán | Must | [x] | 90 | L2 · đề nghị TT AP Draft→Submit |
| `UC_FIN_042` | Công nợ phải trả (AP) | Duyệt chi trả | Must | [x] | 90 | L2 · duyệt/từ chối đề nghị TT |
| `UC_FIN_043` | Công nợ phải trả (AP) | Thanh toán & phân bổ AP | Must | [x] | 90 | L2 · chi trả + phân bổ · đẩy Cash/Bank voucher |
| `UC_FIN_044` | Công nợ phải trả (AP) | Bảng tuổi nợ phải trả | Must | [x] | 90 | L2 · bảng tuổi nợ AP aging |
| `UC_FIN_045` | Công nợ phải trả (AP) | Tạm ứng nhà cung cấp | Should | [ ] | 0 |  |
| `UC_FIN_046` | Công nợ phải trả (AP) | Đối soát 3 chiều | Should | [ ] | 0 |  |
| `UC_FIN_047` | Hóa đơn điện tử | Cấu hình nhà cung cấp HĐĐT | Should | [ ] | 0 |  |
| `UC_FIN_048` | Hóa đơn điện tử | Phát hành hóa đơn điện tử | Should | [ ] | 0 |  |
| `UC_FIN_049` | Hóa đơn điện tử | Điều chỉnh / thay thế / hủy | Should | [ ] | 0 |  |
| `UC_FIN_050` | Hóa đơn điện tử | Tra cứu trạng thái phát hành | Should | [ ] | 0 |  |
| `UC_FIN_051` | Hóa đơn điện tử | Lưu trữ bảng kê HĐĐT | Should | [ ] | 0 |  |
| `UC_FIN_052` | Thuế | Tính thuế GTGT đầu ra / đầu vào | Must | [x] | 90 | L2 · tính GTGT đầu ra/vào + summary net payable |
| `UC_FIN_053` | Thuế | Bảng kê hóa đơn GTGT | Must | [x] | 90 | L2 · bảng kê FinVatDocument Output/Input |
| `UC_FIN_054` | Thuế | Tờ khai thuế GTGT | Should | [ ] | 0 |  |
| `UC_FIN_055` | Thuế | Thuế TNCN từ lương | Should | [ ] | 0 |  |
| `UC_FIN_056` | Thuế | Cấu hình thuế suất | Must | [x] | 90 | L2 · cấu hình thuế suất TaxType/IsDefault/hiệu lực |
| `UC_FIN_057` | Doanh thu & giá vốn | Ghi nhận doanh thu từ POS | Must | [x] | 90 | L2 · ghi nhận doanh thu từ POS Paid + soft-wire |
| `UC_FIN_058` | Doanh thu & giá vốn | Ghi nhận doanh thu từ đơn | Must | [x] | 90 | L2 · ghi nhận doanh thu từ đơn CRM Delivered / AR |
| `UC_FIN_059` | Doanh thu & giá vốn | Ghi nhận doanh thu dự án | Should | [ ] | 0 |  |
| `UC_FIN_060` | Doanh thu & giá vốn | Ghi nhận giá vốn hàng bán | Must | [x] | 90 | L2 · ghi nhận giá vốn từ phiếu xuất Sales |
| `UC_FIN_061` | Doanh thu & giá vốn | Doanh thu nhận trước | Could | [ ] | 0 |  |
| `UC_FIN_062` | Doanh thu & giá vốn | Chiết khấu làm giảm doanh thu | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_063` | Chi phí & phân bổ | Ghi nhận chi phí hoạt động | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_064` | Chi phí & phân bổ | Phân bổ chi phí | Should | [ ] | 0 |  |
| `UC_FIN_065` | Chi phí & phân bổ | Chi phí lương từ HRM | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_066` | Chi phí & phân bổ | Chi phí marketing từ CRM | Should | [ ] | 0 |  |
| `UC_FIN_067` | Chi phí & phân bổ | Tạm ứng chi phí / quyết toán | Should | [ ] | 0 |  |
| `UC_FIN_068` | Kết chuyển & khóa sổ | Kết chuyển lãi/lỗ cuối kỳ | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_069` | Kết chuyển & khóa sổ | Đối chiếu công nợ – sổ cái | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_070` | Kết chuyển & khóa sổ | Checklist khóa sổ tháng | Should | [ ] | 0 |  |
| `UC_FIN_071` | Kết chuyển & khóa sổ | Khóa sổ năm tài chính | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_072` | Ngân sách tài chính | Lập ngân sách theo kỳ | Should | [ ] | 0 |  |
| `UC_FIN_073` | Ngân sách tài chính | So sánh thực tế vs ngân sách | Should | [ ] | 0 |  |
| `UC_FIN_074` | Ngân sách tài chính | Cảnh báo vượt ngân sách | Should | [ ] | 0 |  |
| `UC_FIN_075` | Ngân sách tài chính | Phiên bản ngân sách | Could | [ ] | 0 |  |
| `UC_FIN_076` | Báo cáo tài chính & quản trị | Bảng cân đối phát sinh | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_077` | Báo cáo tài chính & quản trị | Báo cáo P&L quản trị | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_078` | Báo cáo tài chính & quản trị | Bảng cân đối kế toán | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_079` | Báo cáo tài chính & quản trị | Báo cáo lưu chuyển tiền tệ | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_080` | Báo cáo tài chính & quản trị | P&L theo chi nhánh / đơn vị | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_081` | Báo cáo tài chính & quản trị | Báo cáo công nợ tổng hợp | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_082` | Báo cáo tài chính & quản trị | Dashboard tài chính | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |
| `UC_FIN_083` | Báo cáo tài chính & quản trị | Xuất báo cáo tài chính | Must | [x] | 100 | Implemented FIN Financial Statements & Closing |

## AST (21/34)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_AST_001` | Danh mục tài sản | Danh mục nhóm TSCĐ | Must | [x] | 85 | L2 · AstAssetGroup + FE catalog |
| `UC_AST_002` | Danh mục tài sản | Tạo thẻ tài sản | Must | [x] | 85 | L2 · AstAsset thẻ TS + FE |
| `UC_AST_003` | Danh mục tài sản | Thông tin nguyên giá / ngày ghi tăng | Must | [x] | 85 | L2 · nguyên giá / ngày ghi tăng |
| `UC_AST_004` | Danh mục tài sản | Gắn vị trí / chi nhánh | Must | [x] | 85 | L2 · AstLocation vị trí/CN |
| `UC_AST_005` | Danh mục tài sản | Ảnh & tài liệu kèm | Should | [ ] | 0 |  |
| `UC_AST_006` | Danh mục tài sản | Import danh mục tài sản | Could | [ ] | 0 |  |
| `UC_AST_007` | Danh mục tài sản | In tem mã tài sản | Should | [ ] | 0 |  |
| `UC_AST_008` | Khấu hao | Cấu hình phương pháp khấu hao | Must | [x] | 85 | L2 · AstDepreciationMethod |
| `UC_AST_009` | Khấu hao | Cấu hình thời gian / tỷ lệ | Must | [x] | 85 | L2 · thời gian / tỷ lệ KH trên nhóm & PP |
| `UC_AST_010` | Khấu hao | Tính khấu hao định kỳ | Must | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_AST_011` | Khấu hao | Xem sổ khấu hao | Must | [x] | 85 | L2 · sổ KH theo kỳ (run/lines) |
| `UC_AST_012` | Khấu hao | Đẩy bút toán khấu hao sang FIN | Must | [x] | 90 | L2 · đẩy BT KH → FIN JE thật (Posted cân Nợ/Có, auto-resolve TK 642*/214* + kỳ Open, chặn kỳ khóa/đẩy trùng) |
| `UC_AST_013` | Khấu hao | Tạm dừng / điều chỉnh khấu hao | Should | [ ] | 0 |  |
| `UC_AST_014` | Ghi tăng – ghi giảm | Ghi tăng từ mua sắm | Must | [x] | 85 | L2 · ghi tăng từ mua sắm (PurchaseRef) |
| `UC_AST_015` | Ghi tăng – ghi giảm | Ghi tăng từ xây dựng | Could | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_AST_016` | Ghi tăng – ghi giảm | Điều chuyển tài sản nội bộ | Must | [x] | 90 | L2 · điều chuyển nội bộ AstMovementDoc Transfer |
| `UC_AST_017` | Ghi tăng – ghi giảm | Bàn giao tài sản cho nhân viên | Must | [x] | 90 | L2 · bàn giao NV AstMovementDoc Handover |
| `UC_AST_018` | Ghi tăng – ghi giảm | Thanh lý / nhượng bán | Must | [x] | 90 | L2 · thanh lý / nhượng bán Disposal → Disposed |
| `UC_AST_019` | Ghi tăng – ghi giảm | Ghi giảm do mất mát | Should | [ ] | 0 |  |
| `UC_AST_020` | Ghi tăng – ghi giảm | Đánh giá lại nguyên giá | Won't | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_AST_021` | Kiểm kê & bảo trì tài sản | Tạo đợt kiểm kê tài sản | Must | [x] | 90 | L2 · đợt kiểm kê AstStocktake + snapshot Active |
| `UC_AST_022` | Kiểm kê & bảo trì tài sản | Đối chiếu thiếu / thừa | Must | [x] | 90 | L2 · đối chiếu thiếu/thừa Variance trên dòng KK |
| `UC_AST_023` | Kiểm kê & bảo trì tài sản | Lịch bảo trì TSCĐ | Could | [ ] | 0 |  |
| `UC_AST_024` | Kiểm kê & bảo trì tài sản | Lịch sử sửa chữa | Should | [ ] | 0 |  |
| `UC_AST_025` | Kiểm kê & bảo trì tài sản | Cảnh báo tài sản sắp hết khấu hao | Should | [ ] | 0 |  |
| `UC_AST_026` | Công cụ dụng cụ & cấp phát | Quản lý công cụ dụng cụ | Should | [ ] | 0 |  |
| `UC_AST_027` | Công cụ dụng cụ & cấp phát | Cấp phát công cụ cho nhân viên | Should | [ ] | 0 |  |
| `UC_AST_028` | Công cụ dụng cụ & cấp phát | Thu hồi công cụ | Should | [ ] | 0 |  |
| `UC_AST_029` | Công cụ dụng cụ & cấp phát | Phân bổ chi phí công cụ | Could | [ ] | 0 |  |
| `UC_AST_030` | Báo cáo tài sản | Sổ tài sản cố định | Must | [x] | 90 | L2 · sổ TSCĐ /api/ast/reports/register |
| `UC_AST_031` | Báo cáo tài sản | Báo cáo khấu hao theo kỳ | Must | [x] | 90 | L2 · BC khấu hao theo kỳ |
| `UC_AST_032` | Báo cáo tài sản | Báo cáo tài sản theo vị trí | Must | [x] | 90 | L2 · BC tài sản theo vị trí |
| `UC_AST_033` | Báo cáo tài sản | Giá trị còn lại theo nhóm | Should | [ ] | 0 |  |
| `UC_AST_034` | Báo cáo tài sản | Xuất báo cáo tài sản | Must | [x] | 90 | L2 · xuất CSV báo cáo TSCĐ |

## WF (22/40)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_WF_001` | Danh mục công việc | Loại công việc / ticket | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_002` | Danh mục công việc | Độ ưu tiên & SLA nội bộ | Should | [ ] | 0 |  |
| `UC_WF_003` | Danh mục công việc | Mẫu công việc lặp lại | Should | [ ] | 0 |  |
| `UC_WF_004` | Danh mục công việc | Nhóm / dự án nội bộ | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_005` | Giao việc & theo dõi | Tạo task / giao việc | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_006` | Giao việc & theo dõi | Gán người thực hiện / theo dõi | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_007` | Giao việc & theo dõi | Deadline / nhắc việc | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_008` | Giao việc & theo dõi | Checklist trong task | Should | [ ] | 0 |  |
| `UC_WF_009` | Giao việc & theo dõi | Bình luận / đính kèm file | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_010` | Giao việc & theo dõi | Chuyển trạng thái task | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_011` | Giao việc & theo dõi | Ủy thác / chuyển người làm | Should | [ ] | 0 |  |
| `UC_WF_012` | Giao việc & theo dõi | Task liên kết chứng từ ERP | Should | [x] | 90 | M1 · sourceDoc leave |
| `UC_WF_013` | Bảng làm việc (Board) | Kanban theo nhóm/dự án | Should | [ ] | 0 |  |
| `UC_WF_014` | Bảng làm việc (Board) | Lọc task theo tiêu chí | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_015` | Bảng làm việc (Board) | Calendar công việc | Should | [ ] | 0 |  |
| `UC_WF_016` | Bảng làm việc (Board) | Workload theo người | Should | [ ] | 0 |  |
| `UC_WF_017` | Ticket nội bộ / helpdesk | Tạo ticket nội bộ | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_018` | Ticket nội bộ / helpdesk | Phân loại & định tuyến | Should | [ ] | 0 |  |
| `UC_WF_019` | Ticket nội bộ / helpdesk | Escalate ticket quá hạn | Should | [ ] | 0 |  |
| `UC_WF_020` | Ticket nội bộ / helpdesk | CSAT nội bộ | Won't | [ ] | 0 |  |
| `UC_WF_021` | Ticket nội bộ / helpdesk | Kiến thức / FAQ nội bộ | Could | [ ] | 0 |  |
| `UC_WF_022` | Thiết kế quy trình phê duyệt | Tạo mẫu workflow duyệt | Must | [x] | 70 | M1 · seed LEAVE_APPROVE |
| `UC_WF_023` | Thiết kế quy trình phê duyệt | Điều kiện duyệt theo quy tắc | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_024` | Thiết kế quy trình phê duyệt | Nhiều cấp duyệt tuần tự / song song | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_025` | Thiết kế quy trình phê duyệt | Gắn workflow vào loại chứng từ | Must | [x] | 80 | M1 · leave_request |
| `UC_WF_026` | Thiết kế quy trình phê duyệt | Phiên bản quy trình | Should | [ ] | 0 |  |
| `UC_WF_027` | Thiết kế quy trình phê duyệt | Mô phỏng / kiểm thử | Could | [ ] | 0 |  |
| `UC_WF_028` | Thực thi phê duyệt | Hộp chờ duyệt của tôi | Must | [x] | 100 | M1 · /app/wf/tasks |
| `UC_WF_029` | Thực thi phê duyệt | Duyệt / từ chối / trả bổ sung | Must | [x] | 90 | M1 · Approve/Reject |
| `UC_WF_030` | Thực thi phê duyệt | Duyệt hàng loạt | Should | [ ] | 0 |  |
| `UC_WF_031` | Thực thi phê duyệt | Duyệt trên mobile APP | Must | [x] | 80 | Batch1 Must · Mobile approval approve/reject + push |
| `UC_WF_032` | Thực thi phê duyệt | Ủy quyền duyệt tạm thời | Must | [x] | 80 | L2 · ủy quyền duyệt |
| `UC_WF_033` | Thực thi phê duyệt | Nhắc duyệt / escalate | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_034` | Thực thi phê duyệt | Lịch sử duyệt & comment | Must | [x] | 70 | M1 · WfTaskAction |
| `UC_WF_035` | Thực thi phê duyệt | Thu hồi chứng từ đang chờ | Should | [ ] | 0 |  |
| `UC_WF_036` | Báo cáo quy trình & công việc | Thời gian duyệt trung bình | Should | [ ] | 0 |  |
| `UC_WF_037` | Báo cáo quy trình & công việc | Bottleneck cấp duyệt | Should | [ ] | 0 |  |
| `UC_WF_038` | Báo cáo quy trình & công việc | Khối lượng task mở / quá hạn | Must | [x] | 80 | N1 · work-type/project/item/workload |
| `UC_WF_039` | Báo cáo quy trình & công việc | Năng suất hoàn thành | Should | [ ] | 0 |  |
| `UC_WF_040` | Báo cáo quy trình & công việc | Dashboard workflow | Must | [x] | 80 | L2 · dashboard WF |

## BI (14/30)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_BI_001` | Nền tảng dữ liệu báo cáo | Catalog dataset theo module | Must | [x] | 85 | L2 · BiDataset catalog + FE |
| `UC_BI_002` | Nền tảng dữ liệu báo cáo | Làm mới dữ liệu định kỳ | Must | [x] | 85 | L2 · refresh stub + BiDatasetRefresh log |
| `UC_BI_003` | Nền tảng dữ liệu báo cáo | Phân quyền xem báo cáo | Must | [x] | 85 | L2 · BiReportPermission Role/User |
| `UC_BI_004` | Nền tảng dữ liệu báo cáo | Từ điển chỉ tiêu KPI | Should | [ ] | 0 |  |
| `UC_BI_005` | Nền tảng dữ liệu báo cáo | Nhật ký truy cập báo cáo | Could | [ ] | 0 |  |
| `UC_BI_006` | Dashboard quản trị | Dashboard Ban lãnh đạo | Must | [x] | 85 | L2 · BiDashboard Executive |
| `UC_BI_007` | Dashboard quản trị | Dashboard theo module | Must | [x] | 85 | L2 · BiDashboard Module |
| `UC_BI_008` | Dashboard quản trị | Widget doanh thu – lợi nhuận | Must | [x] | 85 | L2 · BiWidget Revenue/Profit stub |
| `UC_BI_009` | Dashboard quản trị | Widget tồn – mua – giao | Should | [ ] | 0 |  |
| `UC_BI_010` | Dashboard quản trị | Widget nhân sự – công | Should | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_BI_011` | Dashboard quản trị | Widget sales pipeline | Should | [ ] | 0 |  |
| `UC_BI_012` | Dashboard quản trị | Tùy chỉnh bố cục theo role | Should | [ ] | 0 |  |
| `UC_BI_013` | Thư viện báo cáo chuẩn | Danh mục báo cáo theo module | Must | [x] | 85 | L2 · BiReport catalog theo module |
| `UC_BI_014` | Thư viện báo cáo chuẩn | Chạy báo cáo với bộ lọc | Must | [x] | 85 | L2 · chạy BC với filter JSON stub |
| `UC_BI_015` | Thư viện báo cáo chuẩn | Lưu bộ lọc / yêu thích | Should | [x] | 100 | Batch 4 · Verified AST Depreciation, FSM Ticket, PJM Gantt & BI Analytics APIs and tests |
| `UC_BI_016` | Thư viện báo cáo chuẩn | Xuất Excel / PDF | Must | [x] | 80 | L2 · xuất Excel/PDF stub (metadata file) |
| `UC_BI_017` | Thư viện báo cáo chuẩn | Gửi báo cáo email định kỳ | Should | [ ] | 0 |  |
| `UC_BI_018` | Thư viện báo cáo chuẩn | So sánh kỳ / mục tiêu | Must | [x] | 90 | L2 · so sánh kỳ / mục tiêu BiKpiTarget + ComparePeriods |
| `UC_BI_019` | Cảnh báo & KPI | Cấu hình ngưỡng cảnh báo | Must | [x] | 90 | L2 · cấu hình ngưỡng BiAlertThreshold evaluate on-read |
| `UC_BI_020` | Cảnh báo & KPI | Cảnh báo realtime / digest | Should | [ ] | 0 |  |
| `UC_BI_021` | Cảnh báo & KPI | Bảng theo dõi Target vs Actual | Must | [x] | 90 | L2 · bảng Target vs Actual + breach flag |
| `UC_BI_022` | Cảnh báo & KPI | Đăng ký nhận cảnh báo | Should | [ ] | 0 |  |
| `UC_BI_023` | Self-service & phân tích | Tạo báo cáo tùy chỉnh | Could | [ ] | 0 |  |
| `UC_BI_024` | Self-service & phân tích | Pivot / biểu đồ tương tác | Should | [ ] | 0 |  |
| `UC_BI_025` | Self-service & phân tích | Chia sẻ báo cáo | Should | [ ] | 0 |  |
| `UC_BI_026` | Self-service & phân tích | Xuất dataset đã lọc | Should | [ ] | 0 |  |
| `UC_BI_027` | Dự báo & AI | Dự báo doanh thu | Won't | [ ] | 0 |  |
| `UC_BI_028` | Dự báo & AI | Dự báo nhu cầu | Won't | [ ] | 0 |  |
| `UC_BI_029` | Dự báo & AI | Phát hiện bất thường | Won't | [ ] | 0 |  |
| `UC_BI_030` | Dự báo & AI | Tóm tắt insight bằng AI | Won't | [ ] | 0 |  |

## PRT (11/38)

| ID | Nhóm | UC | Ưu tiên | Xong? | % | Ghi chú |
| --- | --- | --- | --- | --- | ---: | --- |
| `UC_PRT_001` | Tài khoản portal | Đăng ký tài khoản khách hàng | Must | [x] | 85 | L2 · PrtAccount đăng ký + FE |
| `UC_PRT_002` | Tài khoản portal | Đăng nhập / quên mật khẩu | Must | [x] | 80 | L2 · login/forgot password stub |
| `UC_PRT_003` | Tài khoản portal | Liên kết tài khoản với mã khách | Must | [x] | 85 | L2 · liên kết CustomerCode |
| `UC_PRT_004` | Tài khoản portal | Quản lý nhiều liên hệ | Should | [ ] | 0 |  |
| `UC_PRT_005` | Tài khoản portal | Phân quyền liên hệ | Should | [ ] | 0 |  |
| `UC_PRT_006` | Tài khoản portal | Xác thực email/SĐT | Should | [ ] | 0 |  |
| `UC_PRT_007` | Đơn hàng & giao nhận (khách hàng) | Xem danh sách đơn hàng | Must | [x] | 85 | L2 · PrtOrder danh sách |
| `UC_PRT_008` | Đơn hàng & giao nhận (khách hàng) | Xem chi tiết & trạng thái đơn | Must | [x] | 85 | L2 · chi tiết đơn + trạng thái |
| `UC_PRT_009` | Đơn hàng & giao nhận (khách hàng) | Theo dõi vận đơn | Should | [ ] | 0 |  |
| `UC_PRT_010` | Đơn hàng & giao nhận (khách hàng) | Tải hóa đơn / biên bản | Should | [ ] | 0 |  |
| `UC_PRT_011` | Đơn hàng & giao nhận (khách hàng) | Yêu cầu trả hàng / khiếu nại | Should | [ ] | 0 |  |
| `UC_PRT_012` | Đơn hàng & giao nhận (khách hàng) | Đặt hàng lại | Could | [ ] | 0 |  |
| `UC_PRT_013` | Đơn hàng & giao nhận (khách hàng) | Tạo yêu cầu báo giá | Won't | [ ] | 0 |  |
| `UC_PRT_014` | Công nợ & thanh toán (khách hàng) | Xem công nợ hiện tại | Must | [x] | 85 | L2 · AR summary stub |
| `UC_PRT_015` | Công nợ & thanh toán (khách hàng) | Xem bảng kê hóa đơn chưa thanh toán | Must | [x] | 85 | L2 · hóa đơn chưa TT |
| `UC_PRT_016` | Công nợ & thanh toán (khách hàng) | Lịch sử thanh toán | Must | [x] | 85 | L2 · lịch sử thanh toán |
| `UC_PRT_017` | Công nợ & thanh toán (khách hàng) | Thanh toán online | Could | [ ] | 0 |  |
| `UC_PRT_018` | Công nợ & thanh toán (khách hàng) | Đối chiếu sao kê | Should | [ ] | 0 |  |
| `UC_PRT_019` | Hỗ trợ & bảo hành (khách hàng) | Tạo ticket hỗ trợ | Must | [x] | 85 | L2 · tạo ticket hỗ trợ |
| `UC_PRT_020` | Hỗ trợ & bảo hành (khách hàng) | Xem trạng thái ticket | Must | [x] | 85 | L2 · xem trạng thái ticket |
| `UC_PRT_021` | Hỗ trợ & bảo hành (khách hàng) | Trao đổi / gửi ảnh | Should | [ ] | 0 |  |
| `UC_PRT_022` | Hỗ trợ & bảo hành (khách hàng) | Xem thiết bị đã mua | Should | [ ] | 0 |  |
| `UC_PRT_023` | Hỗ trợ & bảo hành (khách hàng) | Đặt lịch bảo trì | Could | [ ] | 0 |  |
| `UC_PRT_024` | Hỗ trợ & bảo hành (khách hàng) | Đánh giá dịch vụ | Could | [ ] | 0 |  |
| `UC_PRT_025` | Kiến thức & tài liệu khách hàng | Xem catalogue / bảng giá | Could | [ ] | 0 |  |
| `UC_PRT_026` | Kiến thức & tài liệu khách hàng | Tải tài liệu kỹ thuật | Should | [ ] | 0 |  |
| `UC_PRT_027` | Kiến thức & tài liệu khách hàng | Thông báo từ nhà cung cấp | Could | [ ] | 0 |  |
| `UC_PRT_028` | Kiến thức & tài liệu khách hàng | Đăng ký nhận bản tin | Won't | [ ] | 0 |  |
| `UC_PRT_029` | Portal nhà cung cấp / đối tác | Đăng nhập portal nhà cung cấp | Could | [ ] | 0 |  |
| `UC_PRT_030` | Portal nhà cung cấp / đối tác | Xem PO được gửi | Could | [ ] | 0 |  |
| `UC_PRT_031` | Portal nhà cung cấp / đối tác | Xác nhận PO / ngày giao | Could | [ ] | 0 |  |
| `UC_PRT_032` | Portal nhà cung cấp / đối tác | Gửi thông báo sẵn sàng giao | Won't | [ ] | 0 |  |
| `UC_PRT_033` | Portal nhà cung cấp / đối tác | Xem công nợ phía nhà cung cấp | Won't | [ ] | 0 |  |
| `UC_PRT_034` | Portal nhà cung cấp / đối tác | Portal đại lý | Won't | [ ] | 0 |  |
| `UC_PRT_035` | Báo cáo & quản trị portal | Thống kê lượt dùng portal | Should | [ ] | 0 |  |
| `UC_PRT_036` | Báo cáo & quản trị portal | Quản trị nội dung portal | Could | [ ] | 0 |  |
| `UC_PRT_037` | Báo cáo & quản trị portal | Cấu hình module portal theo gói | Must | [x] | 90 | L2 · cấu hình module portal theo gói PrtPortalPackage |
| `UC_PRT_038` | Báo cáo & quản trị portal | Nhật ký thao tác phía portal | Should | [ ] | 0 |  |

---

## B. Nhật ký

| Ngày | Thay đổi |
| --- | --- |
| 04/08/2026 | Sinh checklist từ catalog (1092 UC); seed tiến độ M1 Day-1 SYS/HRM/WF |
| 05–06/08/2026 | Cap-2 lần lượt (INV FEFO/hold, PJM/FSM/LOG, HRM/WF…) · tiến độ máy trước claim 100% |
| 06/08/2026 | **Hiệu chỉnh sau rà soát:** khôi phục `uc_progress` (bỏ đánh dấu 1092/1092 giả); bảng A thêm cột `[x]`/`[~]`/`[ ]`/Must/Should/Avg%/FE pages/Rủi ro — xem [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) |
| 06/08/2026 | Cap-2 CRM marketing/promo **wired**: `/crm/campaigns`+`/crm/promotions` · UC `016,019,023,026,029,031,032–035,037` (rewire thật) |
| 06/08/2026 | Cap-2 POS BOM+alerts **wired**: PaySale→INV Issue · stock-alerts · UC `054,055` (rewire thật) |
| 06/08/2026 | Cap-2 CRM sync POS + BC voucher + POS đóng ca→FIN: UC `036,038,059` · BE 15 InMemory + FE helpers node:test |
| 06/08/2026 | Cap-2 báo cáo POS: top SP · so sánh điểm bán · cost variance BOM vs INV: UC `065,066,067` · BE 11 InMemory + FE 14 node:test |
| 06/08/2026 | Cap-2 vận hành chuỗi POS: chain-live vs target + target DT store (migration): UC `069,072` · BE 4 InMemory + FE 7 node:test |
| 06/08/2026 | **Hoàn thiện UC dang dở PUR:** đẩy GRN→INV thật + HĐ→FIN AP thật (idempotent) + xuất PO CSV: UC `033,037,043` 75→90 · BE 10 InMemory + FE 16 node:test |
| 06/08/2026 | **Hoàn thiện UC dang dở CRM:** giữ tồn INV reservation thật (ATP) + đẩy đơn→LOG lệnh giao thật: UC `082,088` 75→90 · BE 10 InMemory + FE 14 node:test |
| 06/08/2026 | **Hoàn thiện UC dang dở POS:** sync catalog INV→POS thật + hóa đơn text + báo cáo ca thật: UC `015,037,048` 75→90 · BE 8 InMemory + FE 7 node:test |
| 06/08/2026 | **Hoàn thiện UC dang dở AST:** đẩy BT khấu hao → FIN JE thật (Posted cân Nợ/Có, auto-resolve TK/kỳ): UC `012` 80→90 · BE 8 InMemory + FE 8 node:test |
| 06/08/2026 | **Hoàn thiện UC dang dở MFG/FIN/CRM:** JE WIP→TP thật + BT Auto (filter Source) + báo giá text/Email: UC `MFG_031,FIN_015,CRM_074` →90 · BE 13 InMemory + FE 9 node:test · **724/1092** (66.3%) |
